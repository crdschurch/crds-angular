using System;
using System.Data;
using crds_angular.DataAccess;
using crds_angular.Models.Crossroads.Stewardship;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.DataAccess
{
    public class EzScanCheckScannerDaoTest
    {
        private EzScanCheckScannerDao _fixture;
        private Mock<IDbConnection> _dbConnection;

        [SetUp]
        public void SetUp()
        {
            _dbConnection = new Mock<IDbConnection>(MockBehavior.Strict);

            _fixture = new EzScanCheckScannerDao(_dbConnection.Object);
        }

        [Test]
        public void TestGetOpenBatches()
        {
            var dateTime1 = DateTime.Now.AddDays(1);
            var dateTime2 = DateTime.Now.AddDays(2);

            var dataReader = new Mock<IDataReader>();
            dataReader.SetupSequence(mocked => mocked.Read()).Returns(true).Returns(true).Returns(false);
            dataReader.SetupSequence(mocked => mocked[0]).Returns(1).Returns(2);
            dataReader.SetupSequence(mocked => mocked[1]).Returns("name1").Returns("name2");
            dataReader.SetupSequence(mocked => mocked[2]).Returns(dateTime1).Returns(dateTime2);
            dataReader.Setup(mocked => mocked.IsDBNull(3)).Returns(false);
            dataReader.SetupSequence(mocked => mocked.GetInt16(3)).Returns(0).Returns(1);

            var dbCommand = new Mock<IDbCommand>();
            dbCommand.Setup(mocked => mocked.Dispose());
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SELECT ID, IDBatch, DateProcess, Exported FROM Batches b WHERE ((SELECT COUNT(*) FROM Items WHERE IDBatch = b.IDBatch) > 0) AND (COALESCE(Exported, 0) <> 1) ORDER BY DateProcess DESC").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(dataReader.Object);

            _dbConnection.Setup(mocked => mocked.Open());
            _dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            _dbConnection.Setup(mocked => mocked.Close());

            var result = _fixture.GetBatches();
            _dbConnection.VerifyAll();
            dataReader.VerifyAll();
            dbCommand.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("name1", result[0].Name);
            Assert.AreEqual(dateTime1, result[0].ScanDate);
            Assert.AreEqual(BatchStatus.NotExported, result[0].Status);

            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual("name2", result[1].Name);
            Assert.AreEqual(dateTime2, result[1].ScanDate);
            Assert.AreEqual(BatchStatus.Exported, result[1].Status);
        }

        [Test]
        public void TestGetAllBatches()
        {
            var dateTime1 = DateTime.Now.AddDays(1);
            var dateTime2 = DateTime.Now.AddDays(2);

            var dataReader = new Mock<IDataReader>();
            dataReader.SetupSequence(mocked => mocked.Read()).Returns(true).Returns(true).Returns(false);
            dataReader.SetupSequence(mocked => mocked[0]).Returns(1).Returns(2);
            dataReader.SetupSequence(mocked => mocked[1]).Returns("name1").Returns("name2");
            dataReader.SetupSequence(mocked => mocked[2]).Returns(dateTime1).Returns(dateTime2);
            dataReader.Setup(mocked => mocked.IsDBNull(3)).Returns(false);
            dataReader.SetupSequence(mocked => mocked.GetInt16(3)).Returns(0).Returns(1);

            var dbCommand = new Mock<IDbCommand>();
            dbCommand.Setup(mocked => mocked.Dispose());
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SELECT ID, IDBatch, DateProcess, Exported FROM Batches b WHERE ((SELECT COUNT(*) FROM Items WHERE IDBatch = b.IDBatch) > 0) ORDER BY DateProcess DESC").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(dataReader.Object);

            _dbConnection.Setup(mocked => mocked.Open());
            _dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            _dbConnection.Setup(mocked => mocked.Close());

            var result = _fixture.GetBatches(false);
            _dbConnection.VerifyAll();
            dataReader.VerifyAll();
            dbCommand.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("name1", result[0].Name);
            Assert.AreEqual(dateTime1, result[0].ScanDate);
            Assert.AreEqual(BatchStatus.NotExported, result[0].Status);

            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual("name2", result[1].Name);
            Assert.AreEqual(dateTime2, result[1].ScanDate);
            Assert.AreEqual(BatchStatus.Exported, result[1].Status);
        }

        [Test]
        public void TestGetChecksForBatch()
        {
            const int checkId = 1;
            const int donorId = 5;
            const int exported = 1;
            const string error = null;
            const string accountNumber = "123456789";
            const double amount = 5.67;
            const string checkNumber = "5150";
            var scanDate = DateTime.Now.AddDays(1);
            const string routingNumber = "OU812";
            const string name1 = "edward";
            var checkDate = DateTime.Now.AddDays(2);
            const string name2 = "alex";
            const string addressLine1 = "1234 main st";
            const string addressLine2 = "suite 123";
            const string city = "anytown";
            const string state = "OH";
            const string postalCode = "90210";

            var dataReader = new Mock<IDataReader>();
            dataReader.SetupSequence(mocked => mocked.Read()).Returns(true).Returns(false);
            dataReader.SetupGet(mocked => mocked[0]).Returns(checkId);
            dataReader.SetupGet(mocked => mocked[1]).Returns(donorId+"");
            dataReader.SetupGet(mocked => mocked[2]).Returns(exported);
            dataReader.SetupGet(mocked => mocked[3]).Returns(error);
            dataReader.SetupGet(mocked => mocked[4]).Returns(accountNumber);
            dataReader.SetupGet(mocked => mocked[5]).Returns(amount);
            dataReader.SetupGet(mocked => mocked[6]).Returns(checkNumber);
            dataReader.SetupGet(mocked => mocked[7]).Returns(scanDate);
            dataReader.SetupGet(mocked => mocked[8]).Returns(routingNumber);
            dataReader.SetupGet(mocked => mocked[9]).Returns(name1);
            dataReader.SetupGet(mocked => mocked[10]).Returns(checkDate);
            dataReader.SetupGet(mocked => mocked[11]).Returns(name2);
            dataReader.SetupGet(mocked => mocked[12]).Returns(addressLine1);
            dataReader.SetupGet(mocked => mocked[13]).Returns(addressLine2);
            dataReader.SetupGet(mocked => mocked[14]).Returns(city);
            dataReader.SetupGet(mocked => mocked[15]).Returns(state);
            dataReader.SetupGet(mocked => mocked[16]).Returns(postalCode);

            var dbCommand = new Mock<IDbCommand>();
            dbCommand.Setup(mocked => mocked.Dispose());
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SELECT ID, DonorID, COALESCE(Exported, 0), ErrorMessage, EncryptAccount, Amount, CheckNo, DateScan, EncryptRoute, Payor, DateProcess, Payor2, Address, Address2, City, State, Zip FROM Items WHERE IDBatch = @IDBatch").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(dataReader.Object);

            var idBatchParam = new Mock<IDbDataParameter>();
            idBatchParam.SetupSet(mocked => mocked.ParameterName = "IDBatch");
            idBatchParam.SetupSet(mocked => mocked.DbType = DbType.String);
            idBatchParam.SetupSet(mocked => mocked.Value = "batch123");
            idBatchParam.SetupSet(mocked => mocked.Size = "batch123".Length);

            var parameters = new Mock<IDataParameterCollection>(MockBehavior.Strict);
            parameters.Setup(mocked => mocked.Add(idBatchParam.Object)).Returns(1);

            dbCommand.Setup(mocked => mocked.CreateParameter()).Returns(idBatchParam.Object);
            dbCommand.SetupGet(mocked => mocked.Parameters).Returns(parameters.Object);
            dbCommand.Setup(mocked => mocked.Prepare());

            _dbConnection.Setup(mocked => mocked.Open());
            _dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            _dbConnection.Setup(mocked => mocked.Close());

            var result = _fixture.GetChecksForBatch("batch123");
            _dbConnection.VerifyAll();
            dataReader.VerifyAll();
            dbCommand.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Exported);
            Assert.AreEqual(error, result[0].Error);
            Assert.AreEqual(checkId, result[0].Id);
            Assert.AreEqual(donorId, result[0].DonorId);
            Assert.AreEqual(accountNumber, result[0].AccountNumber);
            Assert.AreEqual((decimal)amount, result[0].Amount);
            Assert.AreEqual(checkNumber, result[0].CheckNumber);
            Assert.AreEqual(scanDate, result[0].ScanDate);
            Assert.AreEqual(routingNumber, result[0].RoutingNumber);
            Assert.AreEqual(name1, result[0].Name1);
            Assert.AreEqual(checkDate, result[0].CheckDate);
            Assert.AreEqual(name2, result[0].Name2);
            Assert.AreEqual(addressLine1, result[0].Address.Line1);
            Assert.AreEqual(addressLine2, result[0].Address.Line2);
            Assert.AreEqual(city, result[0].Address.City);
            Assert.AreEqual(state, result[0].Address.State);
            Assert.AreEqual(postalCode, result[0].Address.PostalCode);
        }

        [Test]
        public void TestUpdateBatchStatus()
        {
            var dbCommand = new Mock<IDbCommand>();
            dbCommand.Setup(mocked => mocked.Dispose());
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "UPDATE Batches SET Exported = @BatchStatus WHERE IDBatch = @IDBatch").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteNonQuery());

            var idBatchParam = new Mock<IDbDataParameter>();
            idBatchParam.SetupSet(mocked => mocked.ParameterName = "IDBatch");
            idBatchParam.SetupSet(mocked => mocked.DbType = DbType.String);
            idBatchParam.SetupSet(mocked => mocked.Value = "batch123");

            var batchStatusParam = new Mock<IDbDataParameter>();
            batchStatusParam.SetupSet(mocked => mocked.ParameterName = "BatchStatus");
            batchStatusParam.SetupSet(mocked => mocked.DbType = DbType.Int16);
            batchStatusParam.SetupSet(mocked => mocked.Value = 1);

            var parameters = new Mock<IDataParameterCollection>(MockBehavior.Strict);
            parameters.Setup(mocked => mocked.Add(idBatchParam.Object)).Returns(1);
            parameters.Setup(mocked => mocked.Add(batchStatusParam.Object)).Returns(2);

            dbCommand.Setup(mocked => mocked.CreateParameter()).Returns(idBatchParam.Object);
            dbCommand.SetupGet(mocked => mocked.Parameters).Returns(parameters.Object);
            dbCommand.Setup(mocked => mocked.Prepare());

            _dbConnection.Setup(mocked => mocked.Open());
            _dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            _dbConnection.Setup(mocked => mocked.Close());

            var result = _fixture.UpdateBatchStatus("batch123", BatchStatus.Exported);
            _dbConnection.VerifyAll();
            dbCommand.VerifyAll();

            Assert.NotNull(result);
            Assert.AreEqual("batch123", result.Name);
            Assert.AreEqual(BatchStatus.Exported, result.Status);
        }

    }
}
