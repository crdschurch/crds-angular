using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crossroads.Utilities;
using Crossroads.Utilities.Enums;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Enum;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;
using MinistryPlatform.Translation.Models.DTO; 

namespace MinistryPlatform.Translation.Repositories
{
    public class DonationRepository : BaseRepository, IDonationRepository
    {
        private readonly int _donationsPageId;
        private readonly int _donationDistributionPageId;
        private readonly int _donorMessageTemplateId;
        private readonly int _distributionPageId;
        private readonly int _batchesPageId;
        private readonly int _depositsPageId;
        private readonly int _paymentProcessorErrorsPageId;
        private readonly int _tripDistributionsPageView;
        private readonly int _gpExportPageView;
        private readonly int _paymentGPExportPageView;
        private readonly int _scholarshipPaymentTypeId;
        private readonly int _tripDonationMessageTemplateId;
        private readonly int _donationCommunicationsPageId;
        private readonly int _messagesPageId;
        private readonly int _donationDistributionsSubPage;

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IDonorRepository _donorService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IPledgeRepository _pledgeService;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        private readonly int _donationStatusSucceeded;

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DonationRepository(IMinistryPlatformService ministryPlatformService, IDonorRepository donorService, ICommunicationRepository communicationService, IPledgeRepository pledgeService, IConfigurationWrapper configuration, IAuthenticationRepository authenticationService, IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRest)
            : base(authenticationService, configuration)
        {
            _ministryPlatformService = ministryPlatformService;
            _donorService = donorService;
            _communicationService = communicationService;
            _pledgeService = pledgeService;
            _donationsPageId = configuration.GetConfigIntValue("Donations");
            _donationDistributionPageId = configuration.GetConfigIntValue("Distributions");
            _donorMessageTemplateId = configuration.GetConfigIntValue("DonorMessageTemplateId");
            _distributionPageId = configuration.GetConfigIntValue("Distributions");
            _batchesPageId = configuration.GetConfigIntValue("Batches");
            _depositsPageId = configuration.GetConfigIntValue("Deposits");
            _paymentProcessorErrorsPageId = configuration.GetConfigIntValue("PaymentProcessorEventErrors");
            _tripDistributionsPageView = configuration.GetConfigIntValue("TripDistributionsView");
            _gpExportPageView = configuration.GetConfigIntValue("GPExportView");
            _paymentGPExportPageView = configuration.GetConfigIntValue("PaymentsGPExportView");
            _scholarshipPaymentTypeId = configuration.GetConfigIntValue("ScholarshipPaymentTypeId");
            _tripDonationMessageTemplateId = configuration.GetConfigIntValue("TripDonationMessageTemplateId");
            _donationStatusSucceeded = configuration.GetConfigIntValue("DonationStatusSucceeded");
            _donationCommunicationsPageId = configuration.GetConfigIntValue("DonationCommunications");
            _messagesPageId = configuration.GetConfigIntValue("Messages");
            _donationDistributionsSubPage = configuration.GetConfigIntValue("DonationDistributionsApiSubPageView");
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRest = ministryPlatformRest;
        }

        public int UpdateDonationStatus(int donationId, int statusId, DateTime statusDate,
            string statusNote = null)
        {
            UpdateDonationStatus(ApiLogin(), donationId, statusId, statusDate, statusNote);
            return (donationId);
        }

        public int UpdateDonationStatus(string processorPaymentId, int statusId,
            DateTime statusDate, string statusNote = null)
        {
            return(WithApiLogin(token =>
            {
                var result = GetDonationByProcessorPaymentId(processorPaymentId, token, false);

                UpdateDonationStatus(token, result.donationId, statusId, statusDate, statusNote);
                if (statusId == _donationStatusSucceeded)
                {
                    FinishSendMessageFromDonor(result.donationId, true);
                }
                return (result.donationId);
            }));
        }

        public MpDonationBatch GetDonationBatchByProcessorTransferId(string processorTransferId)
        {
            return(WithApiLogin(token =>
            {
                var search = string.Format(",,,,,,,,{0},", processorTransferId);
                var batches = _ministryPlatformService.GetRecordsDict(_batchesPageId, token, search);
                if (batches == null || batches.Count == 0)
                {
                    return (null);
                }

                return (Mapper.Map<Dictionary<string, object>, MpDonationBatch>(batches[0]));
            }));
            
        }

        public MpDeposit GetDepositByProcessorTransferId(string processorTransferId)
        {
            var apiToken = _apiUserRepository.GetToken();
            var searchString = $"Processor_Transfer_ID='{processorTransferId}'";

            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpDeposit>(searchString).ToList().FirstOrDefault();
        }

        public MpDonationBatch GetDonationBatch(int batchId)
        {
            return (WithApiLogin(token => (Mapper.Map<Dictionary<string,object>, MpDonationBatch>(_ministryPlatformService.GetRecordDict(_batchesPageId, batchId, token)))));
        }

        public MpDonationBatch GetDonationBatchByDepositId(int depositId)
        {
            return (WithApiLogin(token =>
            {
                var search = string.Format(",,,,,{0}", depositId);
                var batches = _ministryPlatformService.GetRecordsDict(_batchesPageId, token, search);
                if (batches == null || batches.Count == 0)
                {
                    return (null);
                }

                return (Mapper.Map<Dictionary<string, object>, MpDonationBatch>(batches[0]));
            }));
        }

        public List<MpDeposit> GetSelectedDonationBatches(int selectionId, string token)
        {
            var results = _ministryPlatformService.GetSelectionsForPageDict(_depositsPageId, selectionId, token);
            var deposits = new List<MpDeposit>();

            foreach (var result in results)
            {
                deposits.Add(Mapper.Map<Dictionary<string, object>, MpDeposit>(result));
            }

            return deposits;
        }

        public MpDeposit GetDepositById(int depositId)
        {
            var apiToken = ApiLogin();
            var searchString = $"Deposit_ID = {depositId}";
            var results = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpDeposit>(searchString).ToList();
            return results.FirstOrDefault();
        }

        public int CreateDonationBatch(string batchName, DateTime setupDateTime, decimal batchTotalAmount, int itemCount,
            int batchEntryType, int? depositId, DateTime finalizedDateTime, string processorTransferId)
        {
            
            var parms = new Dictionary<string, object>
            {
                {"Batch_Name", batchName},
                {"Setup_Date", setupDateTime},
                {"Batch_Total", batchTotalAmount},
                {"Item_Count", itemCount},
                {"Batch_Entry_Type_ID", batchEntryType},
                {"Deposit_ID", depositId},
                {"Finalize_Date", finalizedDateTime},
                {"Processor_Transfer_ID", processorTransferId}
            };
            try
            {
                var token = ApiLogin();
                var batchId = _ministryPlatformService.CreateRecord(_batchesPageId, parms, token);

                // Important! These two fields have to be set on an update, not on the initial
                // create.  They are nullable fields with default values, but setting a null
                // value on the CreateRecord call has no effect (the default values still get used).
                var updateParms = new Dictionary<string, object>
                {
                    {"Batch_ID", batchId},
                    {"Currency", null},
                    {"Default_Payment_Type", null}
                };
                _ministryPlatformService.UpdateRecord(_batchesPageId, updateParms, token);

                return (batchId);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format(
                        "CreateDonationBatch failed. batchName: {0}, setupDateTime: {1}, batchTotalAmount: {2}, itemCount: {3}, batchEntryType: {4}, depositId: {5}, finalizedDateTime: {6}",
                        batchName, setupDateTime, batchTotalAmount, itemCount, batchEntryType, depositId,
                        finalizedDateTime), e);
            }
        }

        public void AddDonationToBatch(int batchId, int donationId)
        {
            var parms = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Batch_ID", batchId}
            };

            try
            {
                WithApiLogin(token =>
                {
                    _ministryPlatformService.UpdateRecord(_donationsPageId, parms, token);
                    return (true);
                });
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format(
                        "AddDonationToBatch failed. batchId: {0}, donationId: {1}",
                        batchId, donationId), e);
            }
        }

        public int CreateDeposit(string depositName, decimal depositTotalAmount, decimal depositAmount, decimal depositProcessorFee, DateTime depositDateTime,
            string accountNumber, int batchCount, bool exported, string notes, string processorTransferId)
        {
            var parms = new Dictionary<string, object>
            {
                {"Deposit_Name", depositName},
                {"Deposit_Total", depositTotalAmount},
                {"Deposit_Amount", depositAmount},
                {"Processor_Fee_Total", depositProcessorFee},
                {"Deposit_Date", depositDateTime},
                {"Account_Number", accountNumber},
                {"Batch_Count", batchCount},
                {"Exported", exported},
                {"Notes", notes},
                {"Processor_Transfer_ID", processorTransferId}
            };

            try
            {
                return (WithApiLogin(token => (_ministryPlatformService.CreateRecord(_depositsPageId, parms, token))));
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format(
                        "CreateDeposit failed. depositName: {0}, depositTotalAmount: {1}, depositAmount: {2}, depositProcessorFee: {3}, depositDateTime: {4}, accountNumber: {5}, batchCount: {6}, exported: {7}, notes: {8}",
                        depositName, depositTotalAmount, depositAmount, depositProcessorFee, depositDateTime, accountNumber, batchCount, exported, notes), e);
            }
        }

        public void CreatePaymentProcessorEventError(DateTime? eventDateTime, string eventId, string eventType, string eventMessage,
            string responseMessage)
        {
            var parms = new Dictionary<string, object>
            {
                {"Event_Date_Time", eventDateTime ?? DateTime.Now},
                {"Event_ID", eventId},
                {"Event_Type", eventType},
                {"Event_Message", eventMessage},
                {"Response_Message", responseMessage}
            };
            try
            {
                WithApiLogin(token => _ministryPlatformService.CreateRecord(_paymentProcessorErrorsPageId, parms, token));
            }
            catch (Exception e)
            {
                throw (new ApplicationException(string.Format("Could not insert event error dateTime: {0}, eventId: {1}, eventType: {2}, eventMessage: {3}, responseMessage: {4}, Error: {5}", eventDateTime, eventId, eventType, eventMessage, responseMessage, e.Message)));
            }
        }

        private void UpdateDonationStatus(string apiToken, int donationId, int statusId, DateTime statusDate,
            string statusNote)
        {
            var parms = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Donation_Status_Date", statusDate},
                {"Donation_Status_Notes", statusNote},
                {"Donation_Status_ID", statusId}
            };

            try
            {
                _ministryPlatformService.UpdateRecord(_donationsPageId, parms, apiToken);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format(
                        "UpdateDonationStatus failed. donationId: {0}, statusId: {1}, statusNote: {2}, statusDate: {3}",
                        donationId, statusId, statusNote, statusDate), e);
            }
        }

        public void ProcessDeclineEmail(string processorPaymentId)
        {
            try
            {
                var apiToken = ApiLogin();
                var result = GetDonationByProcessorPaymentId(processorPaymentId, apiToken, false);

                var rec = _ministryPlatformService.GetRecordsDict(_distributionPageId, apiToken, string.Format(",,,,,,,,\"{0}\"", result.donationId));
                
                if (rec.Count == 0 || (rec.Last().ToNullableInt("dp_RecordID")) == null)
                {
                    throw (new DonationNotFoundException(processorPaymentId));
                }
                
                var program = rec.First().ToString("Statement_Title");
                var paymentType = PaymentType.GetPaymentType(result.paymentTypeId).name;
                var declineEmailTemplate = PaymentType.GetPaymentType(result.paymentTypeId).declineEmailTemplateId;

                _donorService.SendEmail(declineEmailTemplate, result.donorId, result.donationAmt / Constants.StripeDecimalConversionValue, paymentType, result.donationDate, DateTime.Now,
                    program, result.donationNotes);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format(
                        "ProcessDeclineEmail failed. processorPaymentId: {0},", processorPaymentId), ex);
            }
        }

        public MpDonation GetDonationByProcessorPaymentId(string processorPaymentId, bool retrieveDistributions = false)
        {
            return (WithApiLogin(token => GetDonationByProcessorPaymentId(processorPaymentId, token, retrieveDistributions)));
        }
        
        private MpDonation GetDonationByProcessorPaymentId(string processorPaymentId, string apiToken, bool retrieveDistributions)
        {
            var result = _ministryPlatformService.GetRecordsDict(_donationsPageId, apiToken,
                string.Format(",,,,,,,\"{0}\"", processorPaymentId));
          
            if (result.Count == 0 || (result.Last().ToNullableInt("dp_RecordID")) == null)
            {
                throw (new DonationNotFoundException(processorPaymentId));
            }

            var dictionary = result.First();

            var d = new MpDonation()
            {
                donationId = dictionary.ToInt("dp_RecordID"),
                donorId = dictionary.ToInt("Donor_ID"),
                donationDate = dictionary.ToDate("Donation_Date"),
                donationAmt = (int)((dictionary["Donation_Amount"] as decimal? ?? 0M) * Constants.StripeDecimalConversionValue),
                paymentTypeId = PaymentType.GetPaymentType(dictionary.ToString("Payment_Type")).id,
                donationNotes = dictionary.ToString("Donation_Status_Notes"),
                batchId = dictionary.ToNullableInt("Batch_ID"),
                donationStatus = dictionary.ToInt("Donation_Status_ID")
            };

            if (!retrieveDistributions)
            {
                return d;
            }

            var distributions = _ministryPlatformService.GetSubpageViewRecords(_donationDistributionsSubPage, d.donationId, apiToken);
            if (distributions == null || !distributions.Any())
            {
                return (d);
            }

            foreach (var dist in distributions)
            {
                d.Distributions.Add(new MpDonationDistribution
                {
                    donationId = d.donationId,
                    donationDistributionAmt = (int)((dist["Amount"] as decimal? ?? 0M) * Constants.StripeDecimalConversionValue),
                    donationDistributionId = dist.ToInt("Donation_Distribution_ID"),
                    donationDistributionProgram = dist.ToString("Program_ID"),
                    PledgeId = dist.ToNullableInt("Pledge_ID")
                });
            }
            return (d);
        }

        public List<MpTripDistribution> GetMyTripDistributions(int contactId)
        {
            string searchString = $"{contactId},";
            var results = _ministryPlatformService.GetPageViewRecords(_tripDistributionsPageView, ApiLogin(), searchString);
            var trips = new List<MpTripDistribution>();
            foreach (var result in results)
            {
                var trip = new MpTripDistribution
                {
                    EventId = result.ToInt("Event ID"),
                    EventTypeId = result.ToInt("Event Type ID"),
                    EventTitle = result.ToString("Event Title"),
                    EventStartDate = result.ToDate("Event Start Date"),
                    EventEndDate = result.ToDate("Event End Date"),
                    TotalPledge = Convert.ToInt32(result["Total Pledge"]),
                    CampaignStartDate = result.ToDate("Start Date"),
                    CampaignEndDate = result.ToDate("End Date"),
                    DonorId = result.ToInt("Donor ID"),
                    DonationDistributionId = result.ToInt("dp_RecordID"),
                    DonorNickname = result.ToString("Nickname"),
                    DonorFirstName = result.ToString("First Name"),
                    DonorLastName = result.ToString("Last Name"),
                    DonorEmail = result.ToString("Email Address"),
                    DonationDate = result.ToDate("Donation Date"),
                    DonationAmount = Convert.ToInt32(result["Amount"]),
                    PaymentTypeId = Convert.ToInt32(result["Payment Type ID"]),
                    AnonymousGift = result.ToBool("Anonymous"),
                    RegisteredDonor = result.ToBool("Registered Donor"),
                    MessageSent = result.ToBool("Message Sent")
                };

                trips.Add(trip);
            }
            return trips;
        }

        public List<MpGPExportDatum> GetGpExport(int depositId, string token)
        {                        
            var gpExportDonationLevel = GetGpExportData(depositId, token);
            gpExportDonationLevel.AddRange(GetGPExportDataForPayments(depositId, token));
            
            var gpExportGLLevel= new List<MpGPExportDatum>();

            var gpExportDonationGroup = gpExportDonationLevel.GroupBy(r => new
            {
                r.DepositId,
                r.ProccessorFeeMappingId,
                r.ProgramId,
                r.DocumentType,
                r.BatchName,
                r.DonationDate,
                r.DepositDate, 
                r.CustomerId,
                r.DepositAmount, 
                r.CheckbookId, 
                r.CashAccount,
                r.ReceivableAccount, 
                r.DistributionAccount,
                r.ScholarshipExpenseAccount, 
                r.ScholarshipPaymentTypeId, 
                r.PaymentTypeId,
                r.TransactionType,
                r.CongregationId
            });

            var gpExportDonationSum = gpExportDonationGroup
                .Select(x => new MpGPExportDatum()
                {
                    DepositId = x.Key.DepositId,
                    ProccessorFeeMappingId = x.Key.ProccessorFeeMappingId,
                    ProgramId = x.Key.ProgramId,
                    DocumentType = x.Key.DocumentType,
                    BatchName = x.Key.BatchName,
                    DonationDate = x.Key.DonationDate,
                    DepositDate = x.Key.DepositDate,
                    CustomerId = x.Key.CustomerId,     
                    CongregationId = x.Key.CongregationId,
                    DepositAmount = x.Key.DepositAmount,
                    DonationAmount = x.Sum(g => g.DonationAmount),
                    CheckbookId = x.Key.CheckbookId,
                    CashAccount = x.Key.CashAccount,
                    ReceivableAccount = x.Key.ReceivableAccount,
                    DistributionAccount = x.Key.DistributionAccount,
                    ScholarshipExpenseAccount = x.Key.ScholarshipExpenseAccount,
                    Amount = x.Sum(g => g.Amount),
                    ScholarshipPaymentTypeId = x.Key.ScholarshipPaymentTypeId,
                    PaymentTypeId = x.Key.PaymentTypeId,
                    ProcessorFeeAmount = x.Sum(g => g.ProcessorFeeAmount),
                    TransactionType = x.Key.TransactionType,                    
                }).ToList();

            //loop through each group of donations and add it to the the correct GL Mapping
            var indx = 1;
            foreach (var gpExportDistLevel in gpExportDonationSum)
            {
                var processingFeeGLMapping = GetProcessingFeeGLMapping(gpExportDistLevel.ProccessorFeeMappingId, token);
                GenerateGLLevelGpExport(gpExportGLLevel, gpExportDistLevel, processingFeeGLMapping, indx);
                indx++;
            }

            return gpExportGLLevel.ToList();
        }

        private void GenerateGLLevelGpExport(List<MpGPExportDatum> gpExportGLLevel, MpGPExportDatum gpExportDistLevel, MPGLAccountMapping processingFeeGLMapping,int indx)
        {
            gpExportGLLevel.Add(AdjustGPExportDatumAmount(gpExportDistLevel, indx));

            if (gpExportDistLevel.ProcessorFeeAmount != 0)
            {
                gpExportGLLevel.Add(CreateProcessorFee(gpExportDistLevel, processingFeeGLMapping));
            }
        }
        
        private static MpGPExportDatum AdjustGPExportDatumAmount(MpGPExportDatum datum, int indx)
        {
            if (datum.ProcessorFeeAmount < 0)
            {
                //always a refund that is initated by Crossroads
                datum.DocumentType = "RETURNS";
                datum.DonationAmount = (datum.DonationAmount * -1);
                datum.Amount = (datum.Amount  - datum.ProcessorFeeAmount) * -1 ;
            }
            else if (datum.Amount < 0)
            {
                //always a refund due to processing problems: nsf, etc
                datum.DocumentType = "RETURNS";
                datum.DonationAmount = (datum.DonationAmount * -1) + datum.ProcessorFeeAmount;
                datum.Amount = (datum.Amount * -1);
            }
            else
            {
                //not a refund
                datum.Amount = datum.Amount - datum.ProcessorFeeAmount;
            }
            datum.DistributionReference = datum.TransactionType == TransactionType.Payment ? $"Payment {datum.DonationDate}" : $"Contribution {datum.DonationDate}";
            datum.DocumentNumber = string.Format("{0}000{1}", datum.DepositId, indx);
            return datum;
        }

        private MpGPExportDatum CreateProcessorFee(MpGPExportDatum datum, MPGLAccountMapping processingFeeGLMapping)
        {
            var processorFeeAmount  = datum.ProcessorFeeAmount < 0 ? -1 * datum.ProcessorFeeAmount : datum.ProcessorFeeAmount;
            
            return new MpGPExportDatum 
            {
                DocumentNumber = datum.DocumentNumber,
                ProgramId = processingFeeGLMapping.ProgramId,
                DocumentType = datum.DocumentType,
                DepositId = datum.DepositId,
                DonationId = datum.DonationId,
                BatchName = datum.BatchName,
                DonationDate = datum.DonationDate,
                DepositDate = datum.DepositDate,
                CustomerId = processingFeeGLMapping.CustomerId,
                DepositAmount = datum.DepositAmount,
                DonationAmount = datum.DonationAmount,
                CheckbookId = processingFeeGLMapping.CheckbookId,
                CashAccount = (datum.DocumentType == "SALES" || datum.ProcessorFeeAmount < 0) ? processingFeeGLMapping.DistributionAccount : processingFeeGLMapping.CashAccount,
                ReceivableAccount = datum.ReceivableAccount,
                DistributionAccount = (datum.DocumentType == "SALES" || datum.ProcessorFeeAmount < 0) ? datum.DistributionAccount : processingFeeGLMapping.DistributionAccount,
                ScholarshipExpenseAccount = datum.ScholarshipExpenseAccount,
                Amount = processorFeeAmount,
                ScholarshipPaymentTypeId = datum.ScholarshipPaymentTypeId,
                PaymentTypeId = datum.PaymentTypeId,
                ProcessorFeeAmount = datum.ProcessorFeeAmount,
                TransactionType = datum.TransactionType,
                DistributionReference = $"Processor Fees {datum.DonationDate}"
            };
        }

        public List<MpGPExportDatum> GetGPExportDataForPayments(int depositId , string token)
        {
            var searchStr = $"{depositId},";
            var results = _ministryPlatformService.GetPageViewRecords(_paymentGPExportPageView, token, searchStr);
            return (from result in results
                    let amount = Convert.ToDecimal(result.ToString("Payment_Amount"))

                    select new MpGPExportDatum
                    {
                        ProccessorFeeMappingId = GetProcessingFeeMappingID(result.ToInt("Program_ID"), result.ToInt("Congregation_ID"), token),
                        DepositId = result.ToInt("Deposit_ID"),
                        ProgramId = result.ToInt("Program_ID"),
                        CongregationId = result.ToInt("Congregation_ID"),
                        DocumentType = result.ToString("Document_Type"),
                        DonationId = result.ToInt("Payment_ID"),
                        BatchName = result.ToString("Batch_Name"),
                        DonationDate = result.ToDate("Payment_Date"),
                        DepositDate = result.ToDate("Deposit_Date"),
                        CustomerId = result.ToString("Customer_ID"),
                        DonationAmount = amount,
                        DepositAmount = result.ToString("Deposit_Amount"),
                        CheckbookId = result.ToString("Checkbook_ID"),
                        CashAccount = result.ToString("Cash_Account"),
                        ReceivableAccount = result.ToString("Receivable_Account"),
                        DistributionAccount = result.ToString("Distribution_Account"),
                        ScholarshipExpenseAccount = result.ToString("Scholarship_Expense_Account"),
                        Amount = amount,
                        ScholarshipPaymentTypeId = _scholarshipPaymentTypeId,
                        PaymentTypeId = result.ToInt("Payment_Type_ID"),
                        ProcessorFeeAmount = Convert.ToDecimal(result.ToString("Processor_Fee_Amount")),
                        TransactionType = TransactionType.Payment
                    }).ToList();
        }

        /// <summary>
        /// Find the processing fee program id based on the current program id and congregation id.
        /// If the current program does not have a processing fee program attached to it, get the default 
        /// from the config (ProcessingProgramId) value.
        /// </summary>
        /// <param name="programId"> The id of the Program we are looking for the processing fee for. </param>
        /// <param name="congregationId"> The id of the congregation we are looking for the processing fee for.  </param>
        /// <param name="token"> The authentication token</param>
        /// <returns>The processing fee program id or the default.</returns>
        public int GetProcessingFeeMappingID(int programId, int congregationId, string token)
        {
            try
            {
                var pId = _ministryPlatformRest.UsingAuthenticationToken(token).Search<int>("GL_Account_Mapping",
                                                                                            $"GL_Account_Mapping.Program_ID={programId} AND GL_Account_Mapping.Congregation_ID={congregationId}",
                                                                                            "Processor_Fee_Mapping_ID", null, false);
                if (pId == 0)
                {
                    pId = _configurationWrapper.GetConfigIntValue("ProcessingMappingId");
                }
                return pId;
            }
            catch (Exception e)
            {
                log.Debug($"Got an exception trying to get the Processing Fee ProgramID based on the program with ID={programId} and congregation with ID={congregationId}", e);
                var pid =  _configurationWrapper.GetConfigIntValue("ProcessingMappingId");
                return pid;
            }
        }

        public List<MpGPExportDatum> GetGpExportData(int depositId, string token)
        {
            var searchStr = $"{depositId},";
            var results = _ministryPlatformService.GetPageViewRecords(_gpExportPageView, token, searchStr);


            return (from result in results let amount = Convert.ToDecimal(result.ToString("Amount"))                
                select new MpGPExportDatum
                {
                    ProccessorFeeMappingId = GetProcessingFeeMappingID(result.ToInt("Program_ID"), result.ToInt("Congregation_ID"), token),
                    DepositId = result.ToInt("Deposit_ID"),
                    ProgramId = result.ToInt("Program_ID"),
                    CongregationId = result.ToInt("Congregation_ID"),
                    DocumentType = result.ToString("Document_Type"),
                    DonationId = result.ToInt("Donation_ID"),
                    BatchName = result.ToString("Batch_Name"),
                    DonationDate = result.ToDate("Donation_Date"),
                    DepositDate = result.ToDate("Deposit_Date"),
                    CustomerId = result.ToString("Customer_ID"),
                    DonationAmount = amount,
                    DepositAmount = result.ToString("Deposit_Amount"),
                    CheckbookId = result.ToString("Checkbook_ID"),
                    CashAccount = result.ToString("Cash_Account"),
                    ReceivableAccount = result.ToString("Receivable_Account"),
                    DistributionAccount = result.ToString("Distribution_Account"),
                    ScholarshipExpenseAccount = result.ToString("Scholarship_Expense_Account"),
                    Amount = amount, ScholarshipPaymentTypeId = _scholarshipPaymentTypeId,
                    PaymentTypeId = result.ToInt("Payment_Type_ID"),
                    ProcessorFeeAmount = Convert.ToDecimal(result.ToString("Processor_Fee_Amount")),
                    TransactionType = TransactionType.Donation
                }).ToList();
        }

        public MPGLAccountMapping GetProcessingFeeGLMapping(int processingFeeMapping, string token)
        {
            return _ministryPlatformRest.UsingAuthenticationToken(token).Get<MPGLAccountMapping>(processingFeeMapping);
        }

        public void UpdateDepositToExported(int selectionId, int depositId, string token)
        {
            var paramaters = new Dictionary<string, object>
            {
                {"Deposit_ID", depositId},
                {"Exported", true},
            };

            _ministryPlatformService.UpdateRecord(_depositsPageId, paramaters, token);
            _ministryPlatformService.RemoveSelection(selectionId, new [] {depositId}, token);
        }

        public void SendMessageToDonor(int donorId, int donationDistributionId, int fromContactId, string body, string tripName )
        {
            var template = _communicationService.GetTemplate(_donorMessageTemplateId);
            var defaultContactId = AppSetting("DefaultGivingContactEmailId");
            var defaultContactEmail = _communicationService.GetEmailFromContactId(defaultContactId);

            var messageData = new Dictionary<string, object>
            {
                {"TripName", tripName},
                {"DonorMessage", body}
            };
            var toEmail = _donorService.GetEmailViaDonorId(donorId);

            var to = new List<MpContact>()
            {
                new MpContact()
                {
                     ContactId = toEmail.ContactId,
                    EmailAddress = toEmail.Email
                }
            };

            var authorId = _communicationService.GetUserIdFromContactId(fromContactId);
            var fromEmail = _communicationService.GetEmailFromContactId(fromContactId);

            var comm = new MpCommunication
            {
                AuthorUserId = authorId,
                DomainId = 1,
                ToContacts = to,
                FromContact = new MpContact(){ContactId = defaultContactId, EmailAddress = defaultContactEmail},
                ReplyToContact = new MpContact(){ContactId = fromContactId, EmailAddress = fromEmail},
                EmailSubject = _communicationService.ParseTemplateBody(template.Subject, messageData),
                EmailBody = _communicationService.ParseTemplateBody(template.Body, messageData),
                MergeData = messageData
            };
            _communicationService.SendMessage(comm);

            //mark donation distribution with message sent

            var distributionData = new Dictionary<string, object>
            {
                {"Donation_Distribution_ID", donationDistributionId},
                {"Message_Sent", true}
            };
           
            _ministryPlatformService.UpdateRecord(_donationDistributionPageId, distributionData, ApiLogin());
        }

        public void SendMessageFromDonor(int pledgeId, int donationId, string message, string fromDonor)
        {
            var toDonor = _pledgeService.GetDonorForPledge(pledgeId);
            var toContact = _donorService.GetEmailViaDonorId(toDonor);
            var template = _communicationService.GetTemplate(_tripDonationMessageTemplateId);

            var toContacts = new List<MpContact> {new MpContact {ContactId = toContact.ContactId, EmailAddress = toContact.Email}};

            var from = new MpContact()
            {
                ContactId = 5,
                EmailAddress = "updates@crossroads.net"
            };


            var mergeData = new Dictionary<string, object>
                {
                    {"Donor_Name", fromDonor }
                };

            var comm = new MpCommunication
            {
                AuthorUserId = 5,
                DomainId = 1,
                EmailBody = message,
                EmailSubject = template.Subject,
                FromContact = from,
                ReplyToContact = from,
                ToContacts = toContacts,
                MergeData = mergeData
            };
            var communicationId = _communicationService.SendMessage(comm, true);
            AddDonationCommunication(donationId, communicationId);
        }

        public void FinishSendMessageFromDonor(int donationId, bool succeeded)
        {
            // this code sets the status of a pending message to donor to ready to send, once there's a successful donation
            // stripe webhook returned - JPC 2/25/2016
            var donationCommunicationRecords = _ministryPlatformService.GetRecordsDict(_donationCommunicationsPageId, ApiLogin(), string.Format("\"{0}\"", donationId), "");
            var donationCommunicationRecord = donationCommunicationRecords.FirstOrDefault();

            // a message from the donor is optional, so there's nothing to do if no message is available
            if (donationCommunicationRecord == null)
                return;

            var communicationId = Int32.Parse(donationCommunicationRecord["Communication_ID"].ToString());
            var recordId = Int32.Parse(donationCommunicationRecord["dp_RecordID"].ToString());

            Dictionary<string, object> communicationUpdateValues = new Dictionary<string, object>();
            communicationUpdateValues.Add("Communication_ID", communicationId);

            if (succeeded == true)
            {
                communicationUpdateValues.Add("Communication_Status_ID", 3);
                _ministryPlatformService.UpdateRecord(_messagesPageId, communicationUpdateValues, ApiLogin());
            }

            DeleteOption[] deleteOptions = new DeleteOption[]
            {
                new DeleteOption
                {
                    Action = DeleteAction.Delete
                }
            };

            try
            {
                _ministryPlatformService.DeleteRecord(_donationCommunicationsPageId, recordId, deleteOptions, ApiLogin());
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("RemoveDonationCommunication failed.  DonationCommunicationId: {0}", recordId), e);
            }
        }

        public void AddDonationCommunication(int donationId, int communicationId)
        {
            var communication = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Communication_ID", communicationId},
                {"Domain_ID", 1 }
            };
            var pageId = AppSetting("DonationCommunication");
            _ministryPlatformService.CreateRecord(pageId, communication, ApiLogin(), true);
        }

        public List<int> GetPredefinedDonationAmounts()
        {
            var apiToken = _apiUserRepository.GetToken();
            string tableName = "cr_Predefined_Donation_Amounts";

            List<PredefinedDonationAmountDTO> predefinedAmounts = _ministryPlatformRest
                .UsingAuthenticationToken(apiToken)
                .Get<PredefinedDonationAmountDTO>(tableName, new Dictionary<string, object>());

            List<int> predefinedAmountValues;

            if (predefinedAmounts.Any())
            {
                predefinedAmountValues = predefinedAmounts.Select(qA => qA.Amount).ToList();
            }
            else
            {
                predefinedAmountValues = new List<int>();
            }

            return predefinedAmountValues;
        }

    }
}
