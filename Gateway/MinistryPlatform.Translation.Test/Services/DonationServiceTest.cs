using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.App_Start;
using Crossroads.Utilities;
using Crossroads.Utilities.Enums;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Enum;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using Constants = Crossroads.Utilities.Constants;

namespace MinistryPlatform.Translation.Test.Services
{
    public class DonationServiceTest
    {
        private DonationRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IDonorRepository> _donorService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IPledgeRepository> _pledgeService;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IConfigurationWrapper> _config;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _ministryPlatformService = new Mock<IMinistryPlatformService>(MockBehavior.Strict);
            _donorService = new Mock<IDonorRepository>(MockBehavior.Strict);
            _authService = new Mock<IAuthenticationRepository>();
            _pledgeService = new Mock<IPledgeRepository>();
            _communicationService = new Mock<ICommunicationRepository>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();

            _config = new Mock<IConfigurationWrapper>();
            _config.Setup(mocked => mocked.GetConfigIntValue("Donations")).Returns(9090);
            _config.Setup(mocked => mocked.GetConfigIntValue("Batches")).Returns(8080);
            _config.Setup(mocked => mocked.GetConfigIntValue("Distributions")).Returns(1234);
            _config.Setup(mocked => mocked.GetConfigIntValue("Deposits")).Returns(7070);
            _config.Setup(mocked => mocked.GetConfigIntValue("PaymentProcessorEventErrors")).Returns(6060);
            _config.Setup(mocked => mocked.GetConfigIntValue("GPExportView")).Returns(92198);
            _config.Setup(mocked => mocked.GetConfigIntValue("PaymentsGPExportView")).Returns(1112);            
            _config.Setup(mocked => mocked.GetConfigIntValue("DonationCommunications")).Returns(540);
            _config.Setup(mocked => mocked.GetConfigIntValue("Messages")).Returns(341);
            _config.Setup(mocked => mocked.GetConfigIntValue("GLAccountMappingByProgramPageView")).Returns(2213);
            _config.Setup(mocked => mocked.GetConfigIntValue("ScholarshipPaymentTypeId")).Returns(9);
            _config.Setup(mocked => mocked.GetConfigIntValue("DonationDistributionsApiSubPageView")).Returns(5050);

            _config.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _config.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _fixture = new DonationRepository(_ministryPlatformService.Object,
                                              _donorService.Object,
                                              _communicationService.Object,
                                              _pledgeService.Object,
                                              _config.Object,
                                              _authService.Object,
                                              _ministryPlatformRest.Object,
                                              _apiUserRepository.Object);
        }

        [Test]
        public void TestGetDonationBatchByProcessorTransferId()
        {
            const string processorTransferId = "123";
            const int depositId = 456;
            const int batchId = 789;
            const string batchName = "TestBachName";
            var searchResult = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {"dp_RecordID", batchId},
                        {"Processor_Transfer_ID", processorTransferId},
                        {"Deposit_ID", depositId},
                        {"Batch_Name", batchName},
                    }
                }
            };
            _ministryPlatformService.Setup(mocked => mocked.GetRecordsDict(8080, It.IsAny<string>(), string.Format(",,,,,,,,{0},", processorTransferId), "")).Returns(searchResult);

            var result = _fixture.GetDonationBatchByProcessorTransferId(processorTransferId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(processorTransferId, result.ProcessorTransferId);
            Assert.AreEqual(batchId, result.Id);
            Assert.AreEqual(depositId, result.DepositId);
            Assert.AreEqual(batchName, result.BatchName);
        }

        [Test]
        public void GetPredefinedDonationAmounts()
        {
            string apiToken = "abc123";
            string tableName = "cr_Predefined_Donation_Amounts";

            var mockedPredefinedDonationAmts = new List<PredefinedDonationAmountDTO>
            {
                new PredefinedDonationAmountDTO() {Id = 1, Amount = 5, DomainId = 1},
                new PredefinedDonationAmountDTO() {Id = 2, Amount = 10, DomainId = 1},
                new PredefinedDonationAmountDTO() {Id = 3, Amount = 25, DomainId = 1},
                new PredefinedDonationAmountDTO() {Id = 4, Amount = 50, DomainId = 1},
                new PredefinedDonationAmountDTO() {Id = 5, Amount = 100, DomainId = 1},
                new PredefinedDonationAmountDTO() {Id = 6, Amount = 500, DomainId = 1},
            };

            List<int> expectedResults = new List<int> {5, 10, 25, 50, 100, 500};

            _apiUserRepository.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns(apiToken);

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRest.Object);

            _ministryPlatformRest.Setup(m => m.Get<PredefinedDonationAmountDTO>(tableName, new Dictionary<string, object>()))
                .Returns(mockedPredefinedDonationAmts);

            var result = _fixture.GetPredefinedDonationAmounts();

            Assert.AreEqual(expectedResults, result);
        }

        [Test]
        public void TestGetDonationBatch()
        {
            const string processorTransferId = "123";
            const int depositId = 456;
            const int batchId = 789;
            const string batchName = "TestBatchName";
            var getResult = new Dictionary<string, object>
            {
                {"Batch_ID", batchId},
                {"Processor_Transfer_ID", processorTransferId},
                {"Deposit_ID", depositId},
                {"Batch_Name", batchName},
            };
            _ministryPlatformService.Setup(mocked => mocked.GetRecordDict(8080, batchId, It.IsAny<string>(), false)).Returns(getResult);

            var result = _fixture.GetDonationBatch(batchId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(processorTransferId, result.ProcessorTransferId);
            Assert.AreEqual(batchId, result.Id);
            Assert.AreEqual(depositId, result.DepositId);
            Assert.AreEqual(batchName, result.BatchName);
        }

        [Test]
        public void TestGetDonationBatchByDepositId()
        {
            const string processorTransferId = "123";
            const int depositId = 456;
            const int batchId = 789;
            const string batchName = "TestBachName";
            var searchResult = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {"dp_RecordID", batchId},
                        {"Processor_Transfer_ID", processorTransferId},
                        {"Deposit_ID", depositId},
                        {"Batch_Name", batchName},
                    }
                }
            };
            _ministryPlatformService.Setup(mocked => mocked.GetRecordsDict(8080, It.IsAny<string>(), string.Format(",,,,,{0}", depositId), "")).Returns(searchResult);

            var result = _fixture.GetDonationBatchByDepositId(depositId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(processorTransferId, result.ProcessorTransferId);
            Assert.AreEqual(batchId, result.Id);
            Assert.AreEqual(depositId, result.DepositId);
            Assert.AreEqual(batchName, result.BatchName);
        }

        [Test]
        public void TestGetSelectedDonationBatches()
        {
            const int selectionId = 1248579;
            const int depositPageId = 7070;
            const string token = "afasdfoweradfafewwefafdsajfdafoew";

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _ministryPlatformService.Setup(mocked => mocked.GetSelectionsForPageDict(depositPageId, selectionId, token)).Returns(MockDepositList);

            var result = _fixture.GetSelectedDonationBatches(selectionId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(DateTime.Parse("2/12/2015"), result[1].DepositDateTime);
            Assert.AreEqual(456, result[0].Id);
        }

        private List<Dictionary<string, object>> MockDepositList()
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Deposit_Date", DateTime.Parse("2/12/2010")},
                    {"Deposit_Name", "Test Deposit Name 1"},
                    {"Deposit_ID", 456},
                    {"Deposit_Total", 7829.00},
                    {"Batch_Count", 1},
                    {"Exported", false},
                    {"Processor_Transfer_ID", "1233"},
                },
                new Dictionary<string, object>
                {
                    {"Deposit_Date", DateTime.Parse("2/12/2015")},
                    {"Deposit_Name", "Test Deposit Name 2"},
                    {"Deposit_ID", 777},
                    {"Deposit_Total", 2.00},
                    {"Batch_Count", 11},
                    {"Exported", false},
                    {"Processor_Transfer_ID", "122233"},
                }
            };
        }

        [Test]
        public void TestUpdateDonationStatusById()
        {
            const int donationId = 987;
            var donationStatusDate = DateTime.Now.AddDays(-1);
            const string donationStatusNotes = "note";
            const int donationStatusId = 654;

            var expectedParms = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Donation_Status_Date", donationStatusDate},
                {"Donation_Status_Notes", donationStatusNotes},
                {"Donation_Status_ID", donationStatusId}
            };
            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(9090, expectedParms, It.IsAny<string>()));

            _fixture.UpdateDonationStatus(donationId, donationStatusId, donationStatusDate, donationStatusNotes);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestUpdateDonationStatusByProcessorPaymentId()
        {
            const int donationId = 987;
            var donationStatusDate = DateTime.Now.AddDays(-1);
            const string donationStatusNotes = "note";
            const int donationStatusId = 654;
            const int donorId = 9876;
            const int donationAmt = 4343;
            const string paymentType = "Bank";
            const int batchId = 9090;

            var expectedParms = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Donation_Status_Date", donationStatusDate},
                {"Donation_Status_Notes", donationStatusNotes},
                {"Donation_Status_ID", donationStatusId}
            };

            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(9090, expectedParms, It.IsAny<string>()));

            var searchResult = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {"dp_RecordID", donationId},
                        {"Donor_ID", donorId},
                        {"Donation_Amount", donationAmt},
                        {"Donation_Date", donationStatusDate},
                        {"Donation_Status_Notes", donationStatusNotes},
                        {"Payment_Type", paymentType},
                        {"Batch_ID", batchId},
                        {"Donation_Status_ID", donationStatusId + 1}
                    }
                }
            };
            _ministryPlatformService.Setup(
                mocked => mocked.GetRecordsDict(9090, It.IsAny<string>(), ",,,,,,,\"ch_123\"", It.IsAny<string>()))
                .Returns(searchResult);

            _fixture.UpdateDonationStatus("ch_123", donationStatusId, donationStatusDate, donationStatusNotes);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationBatch()
        {
            const string batchName = "MP12345";
            var setupDateTime = DateTime.Now;
            const decimal batchTotalAmount = 456.78M;
            const int itemCount = 55;
            const int batchEntryType = 44;
            const int depositId = 987;
            var finalizedDateTime = DateTime.Now;
            const string processorTransferId = "transfer 1";

            var expectedParms = new Dictionary<string, object>
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
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(8080, expectedParms, It.IsAny<string>(), false))
                .Returns(513);

            var expectedUpdateParms = new Dictionary<string, object>
            {
                {"Batch_ID", 513},
                {"Currency", null},
                {"Default_Payment_Type", null}
            };
            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(8080, expectedUpdateParms, It.IsAny<string>()));
            var batchId = _fixture.CreateDonationBatch(batchName,
                                                       setupDateTime,
                                                       batchTotalAmount,
                                                       itemCount,
                                                       batchEntryType,
                                                       depositId,
                                                       finalizedDateTime,
                                                       processorTransferId);
            Assert.AreEqual(513, batchId);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestAddDonationToBatch()
        {
            const int batchId = 123;
            const int donationId = 456;

            var expectedParms = new Dictionary<string, object>
            {
                {"Donation_ID", donationId},
                {"Batch_ID", batchId}
            };
            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(9090, expectedParms, It.IsAny<string>()));
            _fixture.AddDonationToBatch(batchId, donationId);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestCreateDeposit()
        {
            const string depositName = "MP12345";
            const decimal depositTotalAmount = 456.78M;
            const decimal depositAmount = 450.00M;
            const decimal depositProcessorFee = 6.78M;
            var depositDateTime = DateTime.Now;
            const string accountNumber = "8675309";
            const int batchCount = 55;
            const bool exported = true;
            const string notes = "C Sharp";
            const string processorTransferId = "transfer 1";

            var expectedParms = new Dictionary<string, object>
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

            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(7070, expectedParms, It.IsAny<string>(), false))
                .Returns(513);
            var depositId = _fixture.CreateDeposit(depositName,
                                                   depositTotalAmount,
                                                   depositAmount,
                                                   depositProcessorFee,
                                                   depositDateTime,
                                                   accountNumber,
                                                   batchCount,
                                                   exported,
                                                   notes,
                                                   processorTransferId);
            Assert.AreEqual(513, depositId);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestCreatePaymentProcessorEventError()
        {
            var dateTime = DateTime.Now;
            const string eventId = "123";
            const string eventType = "456";
            const string message = "message";
            const string response = "response";
            var expectedParms = new Dictionary<string, object>
            {
                {"Event_Date_Time", dateTime},
                {"Event_ID", eventId},
                {"Event_Type", eventType},
                {"Event_Message", message},
                {"Response_Message", response}
            };
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(6060, expectedParms, It.IsAny<string>(), false)).Returns(513);

            _fixture.CreatePaymentProcessorEventError(dateTime, eventId, eventType, message, response);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestGetGPExport()
        {
            const int viewId = 92198;
            const int paymentViewId = 1112;
            const int depositId = 789;
            const string token = "faketoken";

            var mockGPExportData = MockGPExportDataTest2();

            _ministryPlatformService.Setup(mock => mock.GetPageViewRecords(viewId, It.IsAny<string>(), $"{depositId},", "", 0)).Returns(MockGPExport());
            _ministryPlatformService.Setup(mock => mock.GetPageViewRecords(paymentViewId, It.IsAny<string>(), $"{depositId},", "", 0)).Returns(MockGPPaymentExport());

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("GL_Account_Mapping", $"GL_Account_Mapping.Program_ID={It.IsAny<int>()} AND GL_Account_Mapping.Congregation_ID={It.IsAny<int>()}", "Processor_Fee_Mapping_ID_Table.Program_ID", null, false))
                .Returns(0);
            _ministryPlatformRest.Setup(mock => mock.Get<MPGLAccountMapping>(It.IsAny<int>(), (string)null)).Returns(MockGLAccountMapping());
            _config.Setup(mocked => mocked.GetConfigIntValue("ProcessingMappingId")).Returns(127);
            
            var result = _fixture.GetGpExport(depositId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);

            var payments = result.Where(p => p.TransactionType == TransactionType.Payment);
            var donations = result.Where(p => p.TransactionType == TransactionType.Donation);

            Assert.AreEqual(6, payments.Count());
            Assert.AreEqual(6, donations.Count());

            Assert.AreEqual(mockGPExportData[0].DocumentType, result[0].DocumentType);
            Assert.AreEqual(mockGPExportData[0].BatchName, result[0].BatchName);
            Assert.AreEqual(mockGPExportData[0].DonationDate, result[0].DonationDate);
            Assert.AreEqual(mockGPExportData[0].DepositDate, result[0].DepositDate);
            Assert.AreEqual(mockGPExportData[0].CustomerId, result[0].CustomerId);
            Assert.AreEqual(mockGPExportData[0].DepositAmount, result[0].DepositAmount);
            Assert.AreEqual(mockGPExportData[0].CheckbookId, result[0].CheckbookId);
            Assert.AreEqual(mockGPExportData[0].CashAccount, result[0].CashAccount);
            Assert.AreEqual(mockGPExportData[0].ReceivableAccount, result[0].ReceivableAccount);
            Assert.AreEqual(mockGPExportData[0].DistributionAccount, result[0].DistributionAccount);
            Assert.AreEqual(mockGPExportData[0].Amount, result[0].Amount);
            Assert.AreEqual(mockGPExportData[0].ProcessorFeeAmount, result[0].ProcessorFeeAmount);
            Assert.AreEqual(mockGPExportData[0].ProgramId, result[0].ProgramId);
            Assert.AreEqual(mockGPExportData[0].ProccessorFeeMappingId, result[0].ProccessorFeeMappingId);
            Assert.AreEqual(mockGPExportData[0].PaymentTypeId, result[0].PaymentTypeId);
            Assert.AreEqual(mockGPExportData[0].ScholarshipExpenseAccount, result[0].ScholarshipExpenseAccount);
            Assert.AreEqual(mockGPExportData[0].ScholarshipPaymentTypeId, result[0].ScholarshipPaymentTypeId);
            Assert.AreEqual(mockGPExportData[0].DonationAmount, result[0].DonationAmount);

            Assert.AreEqual(mockGPExportData[1].DocumentType, result[1].DocumentType);
            Assert.AreEqual(mockGPExportData[1].Amount, result[1].Amount);
            Assert.AreEqual(mockGPExportData[1].CashAccount, result[1].CashAccount);
            Assert.AreEqual(mockGPExportData[1].DistributionAccount, result[1].DistributionAccount);
            Assert.AreEqual(mockGPExportData[1].DonationAmount, result[1].DonationAmount);

            Assert.AreEqual(mockGPExportData[4].DocumentType, result[4].DocumentType);
            Assert.AreEqual(mockGPExportData[4].DonationAmount, result[4].DonationAmount);
            Assert.AreEqual(mockGPExportData[4].Amount, result[4].Amount);

            Assert.AreEqual(mockGPExportData[5].DocumentType, result[5].DocumentType);
            Assert.AreEqual(mockGPExportData[5].DonationAmount, result[5].DonationAmount);
            Assert.AreEqual(mockGPExportData[5].Amount, result[5].Amount);

            Assert.AreEqual(mockGPExportData[0].DocumentNumber, result[0].DocumentNumber);
            Assert.AreEqual(mockGPExportData[1].DocumentNumber, result[1].DocumentNumber);
            Assert.AreEqual(mockGPExportData[2].DocumentNumber, result[2].DocumentNumber);
            Assert.AreEqual(mockGPExportData[3].DocumentNumber, result[3].DocumentNumber);
            Assert.AreEqual(mockGPExportData[4].DocumentNumber, result[4].DocumentNumber);
            Assert.AreEqual(mockGPExportData[5].DocumentNumber, result[5].DocumentNumber);

            Assert.AreEqual(mockGPExportData[11].DocumentType, result[11].DocumentType);
        }

        [Test]
        public void TestGettingGLMappingForFees()
        {
            const int feeMapping = 1;
            const string token = "mockToken";
            var mockMapping = MockGLAccountMapping();

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(mock => mock.Get<MPGLAccountMapping>(It.IsAny<int>(), (string)null)).Returns(mockMapping);

            var result = _fixture.GetProcessingFeeGLMapping(feeMapping, token);

            Assert.IsNotNull(result);
            Assert.AreEqual(mockMapping, result);
            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void TestNotGettingGLMappingForFees()
        {
            const int feeMapping = 1;
            const string token = "mockToken";
            
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(mock => mock.Get<MPGLAccountMapping>(It.IsAny<int>(), (string)null)).Returns( (MPGLAccountMapping) null);

            var result = _fixture.GetProcessingFeeGLMapping(feeMapping, token);

            Assert.IsNull(result);
            _ministryPlatformRest.VerifyAll();
        }

        private MPGLAccountMapping MockGLAccountMapping()
        {
            return
                new MPGLAccountMapping
                {
                    ProgramId = 1,
                    CongregationId = 1,
                    CashAccount = "77777-031-20",
                    CheckbookId = "PNC001",
                    CustomerId = "CONTRIBUTI001",
                    DistributionAccount = "77777-031-22",
                    DocumentType = "SALE",
                    GLAccount = "",
                    ProcessorFeeMappingId = 1,
                    ReceivableAccount = "77777-031-21",
                    ScholarshipExpenseAccount = "77777-900-11"
                };
        }

        [Test]
        public void TestGetGPExportData()
        {
            const int viewId = 92198;
            const int depositId = 789;
            const int programId = 345;
            const int congregationId = 1;
            var returnedData = MockGPExport(programId, congregationId);

            var mockGPExportData = MockGPExportDataTest1(programId);            
            
            var searchStr = $"{depositId},";
            _ministryPlatformService.Setup(mock => mock.GetPageViewRecords(viewId, It.IsAny<string>(), searchStr, "", 0)).Returns(returnedData);
            
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("GL_Account_Mapping", $"GL_Account_Mapping.Program_ID={programId} AND GL_Account_Mapping.Congregation_ID={congregationId}", "Processor_Fee_Mapping_ID_Table.Program_ID", null, false))
                .Returns(0);
            _config.Setup(mocked => mocked.GetConfigIntValue("ProcessingMappingId")).Returns(127);          

            var result = _fixture.GetGpExportData(depositId, It.IsAny<string>());
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.AreEqual(mockGPExportData[0].DocumentType, result[0].DocumentType);
            Assert.AreEqual(mockGPExportData[0].DonationId, result[0].DonationId);
            Assert.AreEqual(mockGPExportData[0].BatchName, result[0].BatchName);
            Assert.AreEqual(mockGPExportData[0].DonationDate, result[0].DonationDate);
            Assert.AreEqual(mockGPExportData[0].DepositDate, result[0].DepositDate);
            Assert.AreEqual(mockGPExportData[0].CustomerId, result[0].CustomerId);
            Assert.AreEqual(mockGPExportData[0].DepositAmount, result[0].DepositAmount);
            Assert.AreEqual(mockGPExportData[0].CheckbookId, result[0].CheckbookId);
            Assert.AreEqual(mockGPExportData[0].CashAccount, result[0].CashAccount);
            Assert.AreEqual(mockGPExportData[0].ReceivableAccount, result[0].ReceivableAccount);
            Assert.AreEqual(mockGPExportData[0].DistributionAccount, result[0].DistributionAccount);
            Assert.AreEqual(mockGPExportData[0].Amount, result[0].Amount);
            Assert.AreEqual(mockGPExportData[0].ProcessorFeeAmount, result[0].ProcessorFeeAmount);
            Assert.AreEqual(mockGPExportData[0].ProgramId, result[0].ProgramId);
            Assert.AreEqual(mockGPExportData[0].ProccessorFeeMappingId, result[0].ProccessorFeeMappingId);
            Assert.AreEqual(mockGPExportData[0].PaymentTypeId, result[0].PaymentTypeId);
            Assert.AreEqual(mockGPExportData[0].ScholarshipExpenseAccount, result[0].ScholarshipExpenseAccount);
            Assert.AreEqual(mockGPExportData[0].ScholarshipPaymentTypeId, result[0].ScholarshipPaymentTypeId);

            Assert.AreEqual(mockGPExportData[1].DocumentType, result[1].DocumentType);
            Assert.AreEqual(mockGPExportData[1].DonationId, result[1].DonationId);
            Assert.AreEqual(mockGPExportData[1].BatchName, result[1].BatchName);
            Assert.AreEqual(mockGPExportData[1].DonationDate, result[1].DonationDate);
            Assert.AreEqual(mockGPExportData[1].DepositDate, result[1].DepositDate);
            Assert.AreEqual(mockGPExportData[1].CustomerId, result[1].CustomerId);
            Assert.AreEqual(mockGPExportData[1].DonationAmount, result[1].DonationAmount);
            Assert.AreEqual(mockGPExportData[1].CheckbookId, result[1].CheckbookId);
            Assert.AreEqual(mockGPExportData[1].CashAccount, result[1].CashAccount);
            Assert.AreEqual(mockGPExportData[1].ReceivableAccount, result[1].ReceivableAccount);
            Assert.AreEqual(mockGPExportData[1].DistributionAccount, result[1].DistributionAccount);
            Assert.AreEqual(mockGPExportData[1].Amount, result[1].Amount);
            Assert.AreEqual(mockGPExportData[1].ProgramId, result[1].ProgramId);
            Assert.AreEqual(mockGPExportData[1].ProccessorFeeMappingId, result[1].ProccessorFeeMappingId);
            Assert.AreEqual(mockGPExportData[1].PaymentTypeId, result[1].PaymentTypeId);

            Assert.AreEqual(mockGPExportData[3].DocumentType, result[3].DocumentType);
            Assert.AreEqual(mockGPExportData[3].Amount, result[3].Amount);
        }

        [Test]
        public void ShouldGetProcessorFeeProgramIdForProgram()
        {
            const string token = "letmein";
            const int programId = 1234;
            const int congregationId = 9;
            const int processingMappingId = 56787;

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("GL_Account_Mapping", $"GL_Account_Mapping.Program_ID={programId} AND GL_Account_Mapping.Congregation_ID={congregationId}", "Processor_Fee_Mapping_ID", null, false))
                .Returns(processingMappingId);

            var result = _fixture.GetProcessingFeeMappingID(programId, congregationId, token);
            Assert.AreEqual(processingMappingId, result);

            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void ShouldHandleProcessingProgramException()
        {
            const string token = "letmein";
            const int programId = 1234;
            const int congregationId = 9;
            const int defaultProcessingFeeMappingId = 127;

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("GL_Account_Mapping", $"GL_Account_Mapping.Program_ID={programId} AND GL_Account_Mapping.Congregation_ID={congregationId}", "Processor_Fee_Mapping_ID", null, false))
                .Throws<Exception>();
            _config.Setup(mocked => mocked.GetConfigIntValue("ProcessingMappingId")).Returns(defaultProcessingFeeMappingId);

            var result = _fixture.GetProcessingFeeMappingID(programId, congregationId, token);
            Assert.AreEqual(defaultProcessingFeeMappingId, result);
            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void ShouldDefaultIfProcessingProgramNotFound()
        {
            const string token = "letmein";
            const int programId = 1234;
            const int congregationId = 9;
            const int processingProgramId = 0;
            const int defaultProcessingFeeProgramId = 127;

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("GL_Account_Mapping", $"GL_Account_Mapping.Program_ID={programId} AND GL_Account_Mapping.Congregation_ID={congregationId}", "Processor_Fee_Mapping_ID", null, false))
                .Returns(processingProgramId);
            _config.Setup(mocked => mocked.GetConfigIntValue("ProcessingMappingId")).Returns(defaultProcessingFeeProgramId);

            var result = _fixture.GetProcessingFeeMappingID(programId, congregationId, token);
            Assert.AreEqual(defaultProcessingFeeProgramId, result);
            _ministryPlatformRest.VerifyAll();

        }

        private List<Dictionary<string, object>> MockGPExport(int programId = 0,  int congregationId = 0)
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 100},
                    {"Document_Type", "SALE"},
                    {"Deposit_ID", "12341234"},
                    {"Donation_ID", "10002"},
                    {"Batch_Name", "Test Batch"},
                    {"Donation_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Customer_ID", "CONTRIBUTI001"},
                    {"Deposit_Amount", "400.00"},
                    {"Donation_Amount", "200.00"},
                    {"Checkbook_ID", "PNC001"},
                    {"Cash_Account", "91213-031-20"},
                    {"Congregation_ID", congregationId },
                    {"Receivable_Account", "90013-031-21"},
                    {"Distribution_Account", "90001-031-22"},
                    {"Amount", "185.00"},
                    {"Program_ID", programId > 0 ? programId : 15},
                    {"Payment_Type_ID", 8},
                    {"Scholarship_Expense_Account", "19948-900-11"},
                    {"Processor_Fee_Amount", "0.25"}                   
                },
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 200},
                    {"Document_Type", "SALE"},
                    {"Deposit_ID", "12341234"},
                    {"Donation_ID", "10002"},
                    {"Batch_Name", "Test Batch"},
                    {"Donation_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Customer_ID", "CONTRIBUTI001"},
                    {"Deposit_Amount", "400.00"},
                    {"Donation_Amount", "200.00"},
                    {"Checkbook_ID", "PNC001"},
                    {"Cash_Account", "91213-031-20"},
                    {"Congregation_ID", congregationId },
                    {"Receivable_Account", "90013-031-21"},
                    {"Distribution_Account", "90001-031-22"},
                    {"Amount", "15.00"},
                    {"Program_ID", programId > 0 ? programId : 15},
                    {"Payment_Type_ID", 7},
                    {"Scholarship_Expense_Account", "19948-900-11"},
                    {"Processor_Fee_Amount", "0.25"}
                },
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 300},
                    {"Document_Type", "SALE"},
                    {"Deposit_ID", "12341234"},
                    {"Donation_ID", "10003"},
                    {"Batch_Name", "Test Batch"},
                    {"Donation_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                    {"Customer_ID", "CONTRIBUTI001"},
                    {"Deposit_Amount", "400.00"},
                    {"Donation_Amount", "300.00"},
                    {"Checkbook_ID", "PNC001"},
                    {"Cash_Account", "91213-031-20"},
                    {"Congregation_ID", congregationId },
                    {"Receivable_Account", "90013-031-21"},
                    {"Distribution_Account", "90001-031-22"},
                    {"Amount", "300.00"},
                    {"Program_ID", programId > 0 ? programId : 15},
                    {"Payment_Type_ID", 8},
                    {"Scholarship_Expense_Account", "19948-900-11"},
                    {"Processor_Fee_Amount", "0.25"}
                },
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 300},
                    {"Deposit_ID", "12341234"},
                    {"Document_Type", "RETURNS"},
                    {"Donation_ID", "10004"},
                    {"Batch_Name", "Test Batch"},
                    {"Donation_Date", new DateTime(2015, 3, 10, 8, 30, 0)},
                    {"Deposit_Date", new DateTime(2015, 3, 10, 8, 30, 0)},
                    {"Customer_ID", "CONTRIBUTI001"},
                    {"Deposit_Amount", "400.00"},
                    {"Donation_Amount", "-300.00"},
                    {"Checkbook_ID", "PNC001"},
                    {"Cash_Account", "90287-031-20"},
                    {"Congregation_ID", congregationId },
                    {"Receivable_Account", "90287-031-21"},
                    {"Distribution_Account", "90287-031-22"},
                    {"Amount", "-300.00"},
                    {"Program_ID", programId > 0 ? programId : 150},
                    {"Payment_Type_ID", 2},
                    {"Scholarship_Expense_Account", "49998-900-11"},
                    {"Processor_Fee_Amount", "0.25"}
                },
            };
        }

        private List<Dictionary<string, object>> MockGPPaymentExport()
        {
            var one = new Dictionary<string, object>
            {
                {"dp_RecordID", 105},
                {"Document_Type", "SALE"},
                {"Deposit_ID", "123412345"},
                {"Payment_ID", "100025"},
                {"Batch_Name", "Test Batch"},
                {"Payment_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Customer_ID", "CONTRIBUTI001"},
                {"Deposit_Amount", "400.00"},
                {"Payment_Total", "185.00"},
                {"Congregation_ID", 2 },
                {"Checkbook_ID", "PNC001"},
                {"Cash_Account", "91213-031-20"},
                {"Receivable_Account", "90013-031-21"},
                {"Distribution_Account", "90001-031-22"},
                {"Payment_Amount", "185.00"},
                {"Program_ID", "155"},
                {"Payment_Type_ID", 8},
                {"Scholarship_Expense_Account", "19948-900-11"},
                {"Processor_Fee_Amount", "0.25"}
            };

            var two = new Dictionary<string, object>
            {
                {"dp_RecordID", 205},
                {"Document_Type", "SALE"},
                {"Deposit_ID", "123412345"},
                {"Payment_ID", "100025"},
                {"Batch_Name", "Test Batch"},
                {"Payment_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Customer_ID", "CONTRIBUTI001"},
                {"Deposit_Amount", "400.00"},
                {"Payment_Total", "15.00"},
                {"Congregation_ID", 2 },
                {"Checkbook_ID", "PNC001"},
                {"Cash_Account", "91213-031-20"},
                {"Receivable_Account", "90013-031-21"},
                {"Distribution_Account", "90001-031-22"},
                {"Payment_Amount", "15.00"},
                {"Program_ID", "155"},
                {"Payment_Type_ID", 7},
                {"Scholarship_Expense_Account", "19948-900-11"},
                {"Processor_Fee_Amount", "0.25"}
            };
            var three = new Dictionary<string, object>
            {
                {"dp_RecordID", 305},
                {"Document_Type", "SALE"},
                {"Deposit_ID", "123412345"},
                {"Payment_ID", "100035"},
                {"Batch_Name", "Test Batch"},
                {"Payment_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Deposit_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Customer_ID", "CONTRIBUTI001"},
                {"Deposit_Amount", "400.00"},
                {"Payment_Total", "300.00"},
                {"Congregation_ID", 2 },
                {"Checkbook_ID", "PNC001"},
                {"Cash_Account", "91213-031-20"},
                {"Receivable_Account", "90013-031-21"},
                {"Distribution_Account", "90001-031-22"},
                {"Payment_Amount", "300.00"},
                {"Program_ID", "155"},
                {"Payment_Type_ID", 8},
                {"Scholarship_Expense_Account", "19948-900-11"},
                {"Processor_Fee_Amount", "0.25"}
            };

            var four = new Dictionary<string, object>
            {
                {"dp_RecordID", 305},
                {"Deposit_ID", "123412345"},
                {"Document_Type", "RETURNS"},
                {"Payment_ID", "100045"},
                {"Batch_Name", "Test Batch"},
                {"Payment_Date", new DateTime(2015, 3, 10, 8, 30, 0)},
                {"Deposit_Date", new DateTime(2015, 3, 10, 8, 30, 0)},
                {"Customer_ID", "CONTRIBUTI001"},
                {"Deposit_Amount", "400.00"},
                {"Payment_Total", "-300.00"},
                {"Congregation_ID", 2 },
                {"Checkbook_ID", "PNC001"},
                {"Cash_Account", "90287-031-20"},
                {"Receivable_Account", "90287-031-21"},
                {"Distribution_Account", "90287-031-22"},
                {"Payment_Amount", "-300.00"},
                {"Program_ID", "150"},
                {"Payment_Type_ID", 2},
                {"Scholarship_Expense_Account", "49998-900-11"},
                {"Processor_Fee_Amount", "0.25"}
            };

            var l =  new List<Dictionary<string, object>>
            {
                one,
                two,
                three,
                four,
            };

            return l;
        }
    

    private List<MpGPExportDatum> MockGPExportDataTest1(int programId)
        {
            var dict = new List<MpGPExportDatum>
            {
                new MpGPExportDatum
                {
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("185.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("185.00"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = programId,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9
                },
                new MpGPExportDatum
                {
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("15"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("15"),
                    ProgramId = programId,
                    ProcessorFeeAmount = Convert.ToDecimal("0.25"),
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 7,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9,
                },
                new MpGPExportDatum
                {
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10003,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "90287-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("300.00"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = programId,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 2,
                    ScholarshipExpenseAccount = "49998-900-11",
                    ScholarshipPaymentTypeId = 9
                },
                new MpGPExportDatum
                {
                    DepositId = 12341234,
                    DocumentType = "RETURNS",
                    DonationId = 10004,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "90287-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90287-031-22",
                    Amount = Convert.ToDecimal("300.00") * -1,
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = programId,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 2,
                    ScholarshipExpenseAccount = "49998-900-11",
                    ScholarshipPaymentTypeId = 9
                },
            };

            return dict;
        }   

        private List<MpGPExportDatum> MockGPExportDataTest2()
        {
            return new List<MpGPExportDatum>
            {
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340001",
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00") - Convert.ToDecimal(".25") - Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".50"),
                    ProgramId = 15,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340001",
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "77777-031-22",
                    Amount = Convert.ToDecimal(".25") + Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 15,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340002",
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("15.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("15"),
                    ProgramId = 127,
                    ProcessorFeeAmount = Convert.ToDecimal("0.25"),
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 7,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340002",
                    DepositId = 12341234,
                    DocumentType = "SALE",
                    DonationId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("15.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal(".12"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 15,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340003",
                    DepositId = 12341234,
                    DocumentType = "RETURNS",
                    DonationId = 10004,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.25"),
                    CheckbookId = "PNC001",
                    CashAccount = "90287-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90287-031-22",
                    Amount = Convert.ToDecimal("300.00"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 150,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 2,
                    ScholarshipExpenseAccount = "49998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "123412340003",
                    DepositId = 12341234,
                    DocumentType = "RETURNS",
                    DonationId = 10004,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.25"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90287-031-22",
                    Amount = Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 15,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Donation
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450004",
                    DepositId = 123412345,
                    DocumentType = "SALE",
                    DonationId = 100025,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00") - Convert.ToDecimal(".25") - Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".50"),
                    ProgramId = 155,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450004",
                    DepositId = 123412345,
                    DocumentType = "SALE",
                    DonationId = 100025,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("185.00") + Convert.ToDecimal("300.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "77777-031-22",
                    Amount = Convert.ToDecimal(".25") + Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 155,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450005",
                    DepositId = 123412345,
                    DocumentType = "SALE",
                    DonationId = 100025,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("15.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal("15"),
                    ProgramId = 127,
                    ProcessorFeeAmount = Convert.ToDecimal("0.25"),
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 7,
                    ScholarshipExpenseAccount = "19948-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450005",
                    DepositId = 123412345,
                    DocumentType = "SALE",
                    DonationId = 100025,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("15.00"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    Amount = Convert.ToDecimal(".12"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 155,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450006",
                    DepositId = 123412345,
                    DocumentType = "RETURNS",
                    DonationId = 100045,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.25"),
                    CheckbookId = "PNC001",
                    CashAccount = "90287-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90287-031-22",
                    Amount = Convert.ToDecimal("300.00"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 150,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 2,
                    ScholarshipExpenseAccount = "49998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
                new MpGPExportDatum
                {
                    DocumentNumber = "1234123450006",
                    DepositId = 123412345,
                    DocumentType = "RETURNS",
                    DonationId = 100045,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    DonationAmount = Convert.ToDecimal("300.25"),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "90287-031-21",
                    DistributionAccount = "90287-031-22",
                    Amount = Convert.ToDecimal(".25"),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 155,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 8,
                    ScholarshipExpenseAccount = "19998-900-11",
                    ScholarshipPaymentTypeId = 9,
                    TransactionType = TransactionType.Payment
                },
            };
        }

        [Test]
        public void TestUpdateDepositToExported()
        {
            const int selectionId = 124112312;
            const int depositId = 1245;
            const bool exported = true;

            var expectedParms = new Dictionary<string, object>
            {
                {"Deposit_ID", depositId},
                {"Exported", exported},
            };
            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(7070, expectedParms, It.IsAny<string>()));
            _ministryPlatformService.Setup(mocked => mocked.RemoveSelection(selectionId, new [] {depositId}, It.IsAny<string>()));

            _fixture.UpdateDepositToExported(selectionId, depositId);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestCompleteSendMessageFromDonor()
        {
            var pageId = 341;

            var expectedParams = new Dictionary<string, object>
            {
                {"Communication_ID", 123},
                {"Communication_Status_ID", 3}
            };

            List<Dictionary<string, object>> resultsDict = new List<Dictionary<string, object>>();
            var getResult = new Dictionary<string, object>
                {
                    { "dp_RecordID", 123 },
                    { "Communication_ID", 123 }
            };
            resultsDict.Add(getResult);

            _ministryPlatformService.Setup(mocked => mocked.GetRecordsDict(540, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(resultsDict);
            _ministryPlatformService.Setup(mocked => mocked.UpdateRecord(341, expectedParams, It.IsAny<string>()));
            _ministryPlatformService.Setup(mocked => mocked.DeleteRecord(540, It.IsAny<int>(), It.IsAny<DeleteOption[]>(), It.IsAny<string>())).Returns(1);
            _fixture.FinishSendMessageFromDonor(123,true);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestGetDonationByProcessorPaymentIdNoDistributions()
        {
            const int donationId = 987;
            var donationDate = DateTime.Today.AddDays(-1);
            const string donationStatusNotes = "note";
            const int donationStatusId = 654;
            const int donorId = 9876;
            const decimal donationAmt = 4343;
            const string paymentType = "Bank";
            const int batchId = 9090;

            var searchResult = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {"dp_RecordID", donationId},
                        {"Donor_ID", donorId},
                        {"Donation_Amount", donationAmt},
                        {"Donation_Date", donationDate},
                        {"Donation_Status_Notes", donationStatusNotes},
                        {"Payment_Type", paymentType},
                        {"Batch_ID", batchId},
                        {"Donation_Status_ID", donationStatusId}
                    }
                }
            };

            _ministryPlatformService.Setup(
                mocked => mocked.GetRecordsDict(9090, It.IsAny<string>(), ",,,,,,,\"ch_123\"", It.IsAny<string>()))
                .Returns(searchResult);

            var result = _fixture.GetDonationByProcessorPaymentId("ch_123");
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(donationId, result.donationId);
            Assert.AreEqual(donorId, result.donorId);
            Assert.AreEqual((int)(donationAmt * Constants.StripeDecimalConversionValue), result.donationAmt);
            Assert.AreEqual(donationDate, result.donationDate);
            Assert.AreEqual(donationStatusNotes, result.donationNotes);
            Assert.AreEqual(PaymentType.GetPaymentType(paymentType).id, result.paymentTypeId);
            Assert.AreEqual(batchId, result.batchId);
            Assert.AreEqual(donationStatusId, result.donationStatus);
        }

        [Test]
        public void TestGetDonationByProcessorPaymentIdWithDistributions()
        {
            const int donationId = 987;
            var donationDate = DateTime.Today.AddDays(-1);
            const string donationStatusNotes = "note";
            const int donationStatusId = 654;
            const int donorId = 9876;
            const decimal donationAmt = 4343;
            const string paymentType = "Bank";
            const int batchId = 9090;

            var searchResult = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {"dp_RecordID", donationId},
                        {"Donor_ID", donorId},
                        {"Donation_Amount", donationAmt},
                        {"Donation_Date", donationDate},
                        {"Donation_Status_Notes", donationStatusNotes},
                        {"Payment_Type", paymentType},
                        {"Batch_ID", batchId},
                        {"Donation_Status_ID", donationStatusId}
                    }
                }
            };

            _ministryPlatformService.Setup(
                mocked => mocked.GetRecordsDict(9090, It.IsAny<string>(), ",,,,,,,\"ch_123\"", It.IsAny<string>()))
                .Returns(searchResult);
            var distributions = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Amount", 123M},
                    {"Donation_Distribution_ID", 999},
                    {"Program_ID", 99},
                    {"Pledge_ID", 9}
                },
                new Dictionary<string, object>
                {
                    {"Amount", 456M},
                    {"Donation_Distribution_ID", 888},
                    {"Program_ID", 88},
                    {"Pledge_ID", null}
                }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetSubpageViewRecords(5050, donationId, It.IsAny<string>(), string.Empty, string.Empty, 0)).Returns(distributions);

            var result = _fixture.GetDonationByProcessorPaymentId("ch_123", true);
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(donationId, result.donationId);
            Assert.AreEqual(donorId, result.donorId);
            Assert.AreEqual((int)(donationAmt * Constants.StripeDecimalConversionValue), result.donationAmt);
            Assert.AreEqual(donationDate, result.donationDate);
            Assert.AreEqual(donationStatusNotes, result.donationNotes);
            Assert.AreEqual(PaymentType.GetPaymentType(paymentType).id, result.paymentTypeId);
            Assert.AreEqual(batchId, result.batchId);
            Assert.AreEqual(donationStatusId, result.donationStatus);
            Assert.IsNotNull(result.Distributions);
            Assert.AreEqual(2, result.Distributions.Count);

            Assert.AreEqual(donationId, result.Distributions[0].donationId);
            Assert.AreEqual((int) ((distributions[0]["Amount"] as decimal? ?? 0M)*Constants.StripeDecimalConversionValue), result.Distributions[0].donationDistributionAmt);
            Assert.AreEqual(distributions[0].ToInt("Donation_Distribution_ID"), result.Distributions[0].donationDistributionId);
            Assert.AreEqual(distributions[0].ToString("Program_ID"), result.Distributions[0].donationDistributionProgram);
            Assert.AreEqual(distributions[0].ToNullableInt("Pledge_ID"), result.Distributions[0].PledgeId);

            Assert.AreEqual(donationId, result.Distributions[1].donationId);
            Assert.AreEqual((int)((distributions[1]["Amount"] as decimal? ?? 0M) * Constants.StripeDecimalConversionValue), result.Distributions[1].donationDistributionAmt);
            Assert.AreEqual(distributions[1].ToInt("Donation_Distribution_ID"), result.Distributions[1].donationDistributionId);
            Assert.AreEqual(distributions[1].ToString("Program_ID"), result.Distributions[1].donationDistributionProgram);
            Assert.IsNull(result.Distributions[1].PledgeId);
        }
    }
}
