using System;
using System.Collections.Generic;
using System.Data;
using crds_angular.DataAccess.Interfaces;
using crds_angular.Models.Crossroads.Stewardship;
using Crossroads.Utilities.Interfaces;

namespace crds_angular.DataAccess
{
    public class EzScanCheckScannerDao : ICheckScannerDao
    {
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// This must match the length of the check_scanner.itemsStatus.ErrorMessage column.
        /// </summary>
        private const int ErrorMessageMaxLength = 4000;

        public EzScanCheckScannerDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public List<CheckScannerBatch> GetBatches(bool onlyOpenBatches = true)
        {
            List<CheckScannerBatch> batches;
            IDataReader reader = null;
            try
            {
                var whereClause = onlyOpenBatches ? "WHERE (SELECT COUNT(*) FROM Items WHERE IDBatch = b.IDBatch) > 0" : string.Empty;

                batches = WithDbCommand(dbCommand =>
                {
                    dbCommand.CommandType = CommandType.Text;
                    dbCommand.CommandText = string.Format("SELECT ID, IDBatch, DateProcess, Exported FROM Batches {0} ORDER BY DateProcess DESC", whereClause);

                    var b = new List<CheckScannerBatch>();
                    reader = dbCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        var i = 0;
                        b.Add(new CheckScannerBatch
                        {
                            Id = reader[i++] as int? ?? 0,
                            Name = reader[i++] as string,
                            ScanDate = reader[i++] as DateTime? ?? DateTime.Now,
                            Status = reader.IsDBNull(i) || reader.GetInt16(i) == 0 ? BatchStatus.NotExported : BatchStatus.Exported
                        });
                    }
                    return (b);
                });
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return (batches);
        }

        public List<CheckScannerCheck> GetChecksForBatch(string batchName)
        {
            List<CheckScannerCheck> checks;
            IDataReader reader = null;
            try
            {
                checks = WithDbCommand(dbCommand =>
                {
                    dbCommand.CommandType = CommandType.Text;
                    dbCommand.CommandText = "SELECT ID, DonorID, COALESCE(Exported, 0), ErrorMessage, EncryptAccount, Amount, CheckNo, DateScan, EncryptRoute, Payor, DateProcess, Payor2, Address, Address2, City, State, Zip FROM Items WHERE IDBatch = @IDBatch";
                    var idBatchParam = dbCommand.CreateParameter();
                    idBatchParam.ParameterName = "IDBatch";
                    idBatchParam.DbType = DbType.String;
                    idBatchParam.Value = batchName;
                    idBatchParam.Size = batchName.Length;
                    dbCommand.Parameters.Add(idBatchParam);
                    dbCommand.Prepare();

                    var c = new List<CheckScannerCheck>();
                    reader = dbCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        var i = 0;
                        c.Add(new CheckScannerCheck
                        {
                            Id = reader[i++] as int? ?? 0,
                            DonorId = ToNullableInt32(reader[i++] as string),
                            Exported = (reader[i++] as int? ?? 0) > 0,
                            Error = reader[i++] as string,
                            AccountNumber = reader[i++] as string,
                            Amount = (decimal)(reader[i++] as double? ?? 0.0),
                            CheckNumber = reader[i++] as string,
                            ScanDate = reader[i++] as DateTime?,
                            RoutingNumber = reader[i++] as string,
                            Name1 = reader[i++] as string,
                            CheckDate = reader[i++] as DateTime?,
                            Name2 = reader[i++] as string,
                            Address = new Address
                            {
                                Line1 = reader[i++] as string,
                                Line2 = reader[i++] as string,
                                City = reader[i++] as string,
                                State = reader[i++] as string,
                                PostalCode = reader[i] as string
                            }
                        });
                    }

                    return (c);
                });
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return (checks);
        }

        private static int? ToNullableInt32(string s)
        {
            int i;
            if (int.TryParse(s, out i))
            {
                return i;
            }
            return null;
        }

        public CheckScannerBatch UpdateBatchStatus(string batchName, BatchStatus newStatus)
        {
            WithDbCommand(dbCommand =>
            {
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = "UPDATE Batches SET Exported = @BatchStatus WHERE IDBatch = @IDBatch";

                var batchStatusParam = dbCommand.CreateParameter();
                batchStatusParam.ParameterName = "BatchStatus";
                batchStatusParam.DbType = DbType.Int16;
                batchStatusParam.Value = newStatus == BatchStatus.Exported ? 1 : 0;
                dbCommand.Parameters.Add(batchStatusParam);

                var idBatchParam = dbCommand.CreateParameter();
                idBatchParam.ParameterName = "IDBatch";
                idBatchParam.DbType = DbType.String;
                idBatchParam.Value = batchName;
                idBatchParam.Size = batchName.Length;
                dbCommand.Parameters.Add(idBatchParam);

                dbCommand.Prepare();
                dbCommand.ExecuteNonQuery();

                return (true);
            });

            return (new CheckScannerBatch
            {
                Name = batchName,
                Status = newStatus
            });
        }

        public void UpdateCheckStatus(int checkId, bool exported, string errorMessage = null)
        {
            WithDbCommand(dbCommand =>
            {
                dbCommand.CommandText = "UPDATE Items SET Exported = @Exported, ErrorMessage = @ErrorMessage WHERE ID = @ItemID";
                dbCommand.CommandType = CommandType.Text;

                var itemIdParam = dbCommand.CreateParameter();
                itemIdParam.ParameterName = "ItemID";
                itemIdParam.DbType = DbType.Int32;
                itemIdParam.Value = checkId;
                dbCommand.Parameters.Add(itemIdParam);

                var exportedParam = dbCommand.CreateParameter();
                exportedParam.ParameterName = "Exported";
                exportedParam.DbType = DbType.Int16;
                exportedParam.Value = exported ? 1 : 0;
                dbCommand.Parameters.Add(exportedParam);

                var errorMessageParam = dbCommand.CreateParameter();
                errorMessageParam.ParameterName = "ErrorMessage";
                errorMessageParam.DbType = DbType.String;
                if (errorMessage == null)
                {
                    errorMessageParam.Value = DBNull.Value;
                }
                else
                {
                    errorMessageParam.Value = errorMessage.Length > ErrorMessageMaxLength ? errorMessage.Substring(ErrorMessageMaxLength) : errorMessage;
                }
                errorMessageParam.Size = ErrorMessageMaxLength;
                dbCommand.Parameters.Add(errorMessageParam);

                dbCommand.Prepare();
                dbCommand.ExecuteNonQuery();

                return (true);
            });
        }

        private T WithDbCommand<T>(Func<IDbCommand, T> func)
        {
            IDbCommand dbCommand = null;
            try
            {
                _dbConnection.Open();
                dbCommand = _dbConnection.CreateCommand();
                return (func(dbCommand));
            }
            finally
            {
                if (dbCommand != null)
                {
                    dbCommand.Dispose();
                }

                _dbConnection.Close();
            }
        }


    }
}