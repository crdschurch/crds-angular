using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using MPServices=MinistryPlatform.Translation.Repositories.Interfaces;
using Constants = Crossroads.Utilities.Constants;

namespace crds_angular.test.Services
{
    public class DonationServiceTest
    {
        private DonationService _fixture;
        private Mock<MPServices.IDonationRepository> _mpDonationService;
        private Mock<MPServices.IDonorRepository> _mpDonorService;
        private Mock<IPaymentProcessorService> _paymentService;
        private Mock<MPServices.IContactRepository> _contactService;
        private Mock<IConfigurationWrapper> _configurationWrapper;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _mpDonationService = new Mock<MPServices.IDonationRepository>(MockBehavior.Strict);
            _mpDonorService = new Mock<MPServices.IDonorRepository>(MockBehavior.Strict);
            _paymentService = new Mock<IPaymentProcessorService>();
            _contactService = new Mock<MPServices.IContactRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementTypeFamily")).Returns(456);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorIdForBankErrorRefund")).Returns(987);

            _fixture = new DonationService(_mpDonationService.Object, _mpDonorService.Object, _paymentService.Object, _contactService.Object, _configurationWrapper.Object);
        }

        [Test]
        public void TestGetDonationBatch()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationBatch(123)).Returns(new MpDonationBatch
            {
                Id = 123,
                DepositId = 456,
                ProcessorTransferId = "789"
            });
            var result = _fixture.GetDonationBatch(123);
            _mpDonationService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(123, result.Id);
            Assert.AreEqual(456, result.DepositId);
            Assert.AreEqual("789", result.ProcessorTransferId);
        }

        [Test]
        public void TestGetDonationBatchReturnsNull()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationBatch(123)).Returns((MpDonationBatch) null);
            var result = _fixture.GetDonationBatch(123);
            _mpDonationService.VerifyAll();
            Assert.IsNull(result);
        }

       [Test]
        public void TestGetDonationByProcessorPaymentIdDonationNotFound()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("123", false)).Returns((MpDonation) null);
            Assert.IsNull(_fixture.GetDonationByProcessorPaymentId("123"));
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestGetDonationByProcessorPaymentId()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("123", false)).Returns(new MpDonation
            {
                donationId = 123,
                donationAmt = 456,
                batchId = 789,
                donationStatus = (int)crds_angular.Models.Crossroads.Stewardship.DonationStatus.Declined
            });
            var result = _fixture.GetDonationByProcessorPaymentId("123");
            _mpDonationService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(123+"", result.Id);
            Assert.AreEqual(456, result.Amount);
            Assert.AreEqual(789, result.BatchId);
            Assert.AreEqual(crds_angular.Models.Crossroads.Stewardship.DonationStatus.Declined, result.Status);
        }

        [Test]
        public void TestGetDonationBatchByDepositDonationIdNotFound()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationBatchByDepositId(12424)).Returns((MpDonationBatch)null);
            Assert.IsNull(_fixture.GetDonationBatchByDepositId(12424));
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestGetDonationBatchByDepositId()
        {
            _mpDonationService.Setup(mocked => mocked.GetDonationBatchByDepositId(12424)).Returns(new MpDonationBatch
            {
                Id = 123,
                DepositId = 12424,
            });
            var result = _fixture.GetDonationBatchByDepositId(12424);
            _mpDonationService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(123, result.Id);
            Assert.AreEqual(12424, result.DepositId);
        }

        [Test]
        public void TestGetSelectedDonationBatches()
        {
            _mpDonationService.Setup(mocked => mocked.GetSelectedDonationBatches(12424, "afdasfsafd")).Returns(MockDepositList);

            var result = _fixture.GetSelectedDonationBatches(12424, "afdasfsafd");
            _mpDonationService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(DateTime.Parse("2/12/2015"), result[1].DepositDateTime);
            Assert.AreEqual(456, result[0].Id);
        }

        private List<MpDeposit> MockDepositList()
        {
            return new List<MpDeposit>
            {
                new MpDeposit
                {
                    DepositDateTime = DateTime.Parse("12/01/2010"),
                    DepositName = "Test Deposit Name 1",
                    Id = 456,
                    DepositTotalAmount = Convert.ToDecimal(7829.00),
                    BatchCount = 1,
                    Exported = false,
                    ProcessorTransferId = "1233"
                },
                new MpDeposit
                {
                    DepositDateTime = DateTime.Parse("2/12/2015"),
                    DepositName = "Test Deposit Name 2",
                    Id = 4557657,
                    DepositTotalAmount = Convert.ToDecimal(4.00),
                    BatchCount = 11,
                    Exported = false,
                    ProcessorTransferId = "12325523"
                }
            };
        }

        [Test]
        public void TestUpdateDonationByIdWithOptionalParameters()
        {
            var d = DateTime.Now.AddDays(-1);
            _mpDonationService.Setup(mocked => mocked.UpdateDonationStatus(123, 4, d, "note")).Returns(456);
            var response = _fixture.UpdateDonationStatus(123, 4, d, "note");
            Assert.AreEqual(456, response);
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestUpdateDonationByIdWithoutOptionalParameters()
        {
            _mpDonationService.Setup(mocked => mocked.UpdateDonationStatus(123, 4, It.IsNotNull<DateTime>(), null)).Returns(456);
            var response = _fixture.UpdateDonationStatus(123, 4, null);
            Assert.AreEqual(456, response);
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestUpdateDonationByProcessorIdWithOptionalParameters()
        {
            var d = DateTime.Now.AddDays(-1);
            _mpDonationService.Setup(mocked => mocked.UpdateDonationStatus("ch_123", 4, d, "note")).Returns(456);
            var response = _fixture.UpdateDonationStatus("ch_123", 4, d, "note");
            Assert.AreEqual(456, response);
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestUpdateDonationByProcessorIdWithoutOptionalParameters()
        {
            _mpDonationService.Setup(mocked => mocked.UpdateDonationStatus("ch_123", 4, It.IsNotNull<DateTime>(), null)).Returns(456);
            var response = _fixture.UpdateDonationStatus("ch_123", 4, null);
            Assert.AreEqual(456, response);
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationBatch()
        {
            var dto = new DonationBatchDTO
            {
                DepositId = 123,
                BatchEntryType = 2,
                BatchName = "batch name",
                BatchTotalAmount = 456.78M,
                FinalizedDateTime = DateTime.Now,
                ItemCount = 5,
                SetupDateTime = DateTime.Now,
                ProcessorTransferId = "transfer 1",
                Id = 999 // Should be overwritten in service
            };
            dto.Donations.Add(new DonationDTO { Id = "102030"});
            _mpDonationService.Setup(
                mocked =>
                    mocked.CreateDonationBatch(dto.BatchName, dto.SetupDateTime, dto.BatchTotalAmount, dto.ItemCount,
                        dto.BatchEntryType, dto.DepositId, dto.FinalizedDateTime, dto.ProcessorTransferId)).Returns(987);
            _mpDonationService.Setup(mocked => mocked.AddDonationToBatch(987, 102030));

            var response = _fixture.CreateDonationBatch(dto);
            _mpDonationService.VerifyAll();
            Assert.AreSame(dto, response);
            Assert.AreEqual(987, response.Id);
        }

        [Test]
        public void TestCreateDeposit()
        {
            var dto = new DepositDTO
            {
                AccountNumber = "8675309",
                BatchCount = 5,
                DepositDateTime = DateTime.Now,
                DepositName = "deposit name",
                DepositTotalAmount = 456.78M,
                Exported = true,
                Notes = "blah blah blah",
                ProcessorTransferId = "transfer 1",
                Id = 123 // should be overwritten in service
            };

            _mpDonationService.Setup(
                mocked =>
                    mocked.CreateDeposit(dto.DepositName, dto.DepositTotalAmount, dto.DepositAmount, dto.ProcessorFeeTotal, dto.DepositDateTime, dto.AccountNumber,
                        dto.BatchCount, dto.Exported, dto.Notes, dto.ProcessorTransferId)).Returns(987);

            var response = _fixture.CreateDeposit(dto);
            _mpDonationService.VerifyAll();
            Assert.AreSame(dto, response);
            Assert.AreEqual(987, response.Id);
        }

        [Test]
        public void TestCreatePaymentProcessorEventError()
        {
            var stripeEvent = new StripeEvent
            {
                Created = DateTime.Now,
                Data = new StripeEventData
                {
                    Object = new StripeTransfer
                    {
                        Amount = 1000
                    }
                },
                LiveMode = true,
                Id = "123",
                Type = "transfer.paid"
            };

            var stripeEventResponse = new StripeEventResponseDTO
            {
                Exception = new ApplicationException()
            };

            var eventString = JsonConvert.SerializeObject(stripeEvent, Formatting.Indented);
            var responseString = JsonConvert.SerializeObject(stripeEventResponse, Formatting.Indented);

            _mpDonationService.Setup(
                mocked =>
                    mocked.CreatePaymentProcessorEventError(stripeEvent.Created, stripeEvent.Id, stripeEvent.Type,
                        eventString, responseString));

            _fixture.CreatePaymentProcessorEventError(stripeEvent, stripeEventResponse);
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestGpExportFileName()
        {
            var date = DateTime.Today;
            var fileName = string.Format("XRDReceivables-Test_Deposit_Name_{0}{1}{2}.txt", date.ToString("yy"), date.ToString("MM"), date.ToString("dd"));

            var deposit = new DepositDTO()
            {
                DepositName = "Test Deposit Name"
            };

            var result = _fixture.GPExportFileName(deposit);
            Assert.AreEqual(fileName, result);
        }

        [Test]
        public void TestGenerateGpExportFileNames()
        {
            var date = DateTime.Today;
            var fileName = string.Format("XRDReceivables-Test_Deposit_Name_1_{0}{1}{2}.txt", date.ToString("yy"), date.ToString("MM"), date.ToString("dd"));

            _mpDonationService.Setup(mocked => mocked.GetSelectedDonationBatches(12424, "afdasfsafd")).Returns(MockDepositList);

            var results = _fixture.GenerateGPExportFileNames(12424, "afdasfsafd");

            _mpDonationService.VerifyAll();
            Assert.AreEqual(fileName, results[0].ExportFileName);
        }

        [Test]
        public void TestGetGpExport()
        {
            const int depositId = 789;
            var mockedExport = MockGPExport();
            var mockedPaymentExport = MockPaymentExport();

            var expectedReturn = MockExpectedGpExportDto();

            _mpDonationService.Setup(mocked => mocked.GetGpExport(depositId, It.IsAny<string>())).Returns(mockedExport);
            
            var result = _fixture.GetGpExport(depositId, "asdfafasdfas");

            _mpDonationService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(expectedReturn[0].DocumentType, mockedExport[0].DocumentType);
            Assert.AreEqual(expectedReturn[0].DocumentNumber, mockedExport[0].DocumentNumber);
            Assert.AreEqual(expectedReturn[0].DocumentDescription, mockedExport[0].BatchName);
            Assert.AreEqual(expectedReturn[0].BatchId, mockedExport[0].BatchName);
            Assert.AreEqual(expectedReturn[0].ContributionDate, mockedExport[0].DonationDate.ToString("MM/dd/yyyy"));
            Assert.AreEqual(expectedReturn[0].SettlementDate, mockedExport[0].DepositDate.ToString("MM/dd/yyyy"));
            Assert.AreEqual(expectedReturn[0].CustomerId, mockedExport[0].CustomerId);
            Assert.AreEqual(expectedReturn[0].ContributionAmount, mockedExport[0].DonationAmount.ToString());
            Assert.AreEqual(expectedReturn[0].CheckbookId, mockedExport[0].CheckbookId);
            Assert.AreEqual(expectedReturn[0].CashAccount, mockedExport[0].ScholarshipExpenseAccount);
            Assert.AreEqual(expectedReturn[0].ReceivablesAccount, mockedExport[0].ReceivableAccount);
            Assert.AreEqual(expectedReturn[0].DistributionAccount, mockedExport[0].DistributionAccount);
            Assert.AreEqual(expectedReturn[0].DistributionAmount, mockedExport[0].Amount.ToString());
            Assert.AreEqual(expectedReturn[0].DistributionReference, "Contribution " + mockedExport[0].DonationDate);
            Assert.AreEqual(expectedReturn[0].CashAccount, mockedExport[0].ScholarshipExpenseAccount);
            Assert.AreEqual(expectedReturn[1].DistributionReference, "Processor Fees " + mockedExport[1].DonationDate);


            Assert.AreEqual(expectedReturn[0].DocumentType, result[0].DocumentType);
            Assert.AreEqual(expectedReturn[0].DocumentNumber, result[0].DocumentNumber);
            Assert.AreEqual(expectedReturn[0].DocumentDescription, result[0].DocumentDescription);
            Assert.AreEqual(expectedReturn[0].BatchId, result[0].BatchId);
            Assert.AreEqual(expectedReturn[0].ContributionDate, result[0].ContributionDate);
            Assert.AreEqual(expectedReturn[0].SettlementDate, result[0].SettlementDate);
            Assert.AreEqual(expectedReturn[0].CustomerId, result[0].CustomerId);
            Assert.AreEqual(expectedReturn[0].ContributionAmount, result[0].ContributionAmount);
            Assert.AreEqual(expectedReturn[0].CheckbookId, result[0].CheckbookId);
            Assert.AreEqual(expectedReturn[0].CashAccount, result[0].CashAccount);
            Assert.AreEqual(expectedReturn[0].ReceivablesAccount, result[0].ReceivablesAccount);
            Assert.AreEqual(expectedReturn[0].DistributionAccount, result[0].DistributionAccount);
            Assert.AreEqual(expectedReturn[0].DistributionAmount, result[0].DistributionAmount);
            Assert.AreEqual(expectedReturn[0].DistributionReference, result[0].DistributionReference);
            Assert.AreEqual(expectedReturn[0].CashAccount, result[0].CashAccount);
            Assert.AreEqual(expectedReturn[1].DistributionReference, result[1].DistributionReference);

            Assert.AreEqual(expectedReturn[2].DocumentType, result[2].DocumentType);
            Assert.AreEqual(expectedReturn[2].DocumentNumber, result[2].DocumentNumber);
            Assert.AreEqual(expectedReturn[2].DocumentDescription, result[2].DocumentDescription);
            Assert.AreEqual(expectedReturn[2].BatchId, result[2].BatchId);
            Assert.AreEqual(expectedReturn[2].ContributionDate, result[2].ContributionDate);
            Assert.AreEqual(expectedReturn[2].SettlementDate, result[2].SettlementDate);
            Assert.AreEqual(expectedReturn[2].CustomerId, result[2].CustomerId);
            Assert.AreEqual(expectedReturn[2].ContributionAmount, result[2].ContributionAmount);
            Assert.AreEqual(expectedReturn[2].CheckbookId, result[2].CheckbookId);
            Assert.AreEqual(expectedReturn[2].CashAccount, result[2].CashAccount);
            Assert.AreEqual(expectedReturn[2].ReceivablesAccount, result[2].ReceivablesAccount);
            Assert.AreEqual(expectedReturn[2].DistributionAccount, result[2].DistributionAccount);
            Assert.AreEqual(expectedReturn[2].DistributionAmount, result[2].DistributionAmount);
            Assert.AreEqual(expectedReturn[2].DistributionReference, result[2].DistributionReference);
            Assert.AreEqual(expectedReturn[2].CashAccount, result[2].CashAccount);
            Assert.AreEqual(expectedReturn[3].DistributionReference, result[3].DistributionReference);
        }

        private static List<GPExportDatumDTO> MockExpectedGpExportDto()
        {
            return new List<GPExportDatumDTO>
            {
                new GPExportDatumDTO
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DocumentDescription = "Test Batch",
                    BatchId = "Test Batch",
                    ContributionDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    SettlementDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    CustomerId = "CONTRIBUTI001",
                    ContributionAmount = Convert.ToDecimal("380.00").ToString(),
                    CheckbookId = "PNC001",
                    CashAccount = "90551-031-02",
                    ReceivablesAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DistributionAmount = "379.87",
                    DistributionReference = "Contribution " + new DateTime(2015, 3, 28, 8, 30, 0)
                },
                new GPExportDatumDTO
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DocumentDescription = "Test Batch",
                    BatchId = "Test Batch",
                    ContributionDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    SettlementDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    CustomerId = "CONTRIBUTI001",
                    ContributionAmount = Convert.ToDecimal("380.00").ToString(),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-02",
                    ReceivablesAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DistributionAmount = "0.13",
                    DistributionReference = "Processor Fees " + new DateTime(2015, 3, 28, 8, 30, 0)
                },
                new GPExportDatumDTO
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DocumentDescription = "Test 2 Batch",
                    BatchId = "Test 2 Batch",
                    ContributionDate = new DateTime(2014, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    SettlementDate = new DateTime(2014, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    CustomerId = "CONTRIBUTI001",
                    ContributionAmount = Convert.ToDecimal("20.00").ToString(),
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivablesAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DistributionAmount = "19.88",
                    DistributionReference = "Contribution " + new DateTime(2014, 3, 28, 8, 30, 0)
                },
                new GPExportDatumDTO
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DocumentDescription = "Test 2 Batch",
                    BatchId = "Test Batch",
                    ContributionDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    SettlementDate = new DateTime(2015, 3, 28, 8, 30, 0).ToString("MM/dd/yyyy"),
                    CustomerId = "CONTRIBUTI001",
                    ContributionAmount = Convert.ToDecimal("20.00").ToString(),
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-02",
                    ReceivablesAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DistributionAmount = "0.12",
                    DistributionReference = "Processor Fees " + new DateTime(2015, 3, 28, 8, 30, 0)
                },
            };
        }

        private List<MpGPExportDatum> MockPaymentExport()
        {
            return new List<MpGPExportDatum>
            {
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DepositId = 10002,
                    BatchName = "Test Payment Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount ="400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DonationAmount = Convert.ToDecimal("380.00"),
                    Amount = Convert.ToDecimal("380.00")-Convert.ToDecimal(0.13),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 12,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 9,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DepositId = 10002,
                    BatchName = "Test Payment Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DonationAmount = Convert.ToDecimal("380.00"),
                    Amount = Convert.ToDecimal(0.13),
                    ProgramId = 127,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 9,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DepositId = 10002,
                    BatchName = "Test Payment Batch 2",
                    DonationDate = new DateTime(2014, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2014, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DonationAmount = Convert.ToDecimal("20.00"),
                    Amount = Convert.ToDecimal(20.0)-Convert.ToDecimal(0.12),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 112,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 15,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DepositId = 10002,
                    BatchName = "Test Payment Batch 2",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DonationAmount = Convert.ToDecimal("20.00"),
                    Amount = Convert.ToDecimal(0.12),
                    ProgramId = 127,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 15,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                },
            };
        }

        private List<MpGPExportDatum> MockGPExport()
        {
            return new List<MpGPExportDatum>
            {
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DepositId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount ="400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DonationAmount = Convert.ToDecimal("380.00"),
                    Amount = Convert.ToDecimal("380.00")-Convert.ToDecimal(0.13),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 12,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 9,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9, 
                    DistributionReference = $"Contribution {new DateTime(2015, 3, 28, 8, 30, 0)}"
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002001",
                    DepositId = 10002,
                    BatchName = "Test Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DonationAmount = Convert.ToDecimal("380.00"),
                    Amount = Convert.ToDecimal(0.13),
                    ProgramId = 127,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 9,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                    DistributionReference = $"Processor Fees {new DateTime(2015, 3, 28, 8, 30, 0)}"
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DepositId = 10002,
                    BatchName = "Test 2 Batch",
                    DonationDate = new DateTime(2014, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2014, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "91213-031-20",
                    ReceivableAccount = "90013-031-21",
                    DistributionAccount = "90001-031-22",
                    DonationAmount = Convert.ToDecimal("20.00"),
                    Amount = Convert.ToDecimal(20.0)-Convert.ToDecimal(0.12),
                    ProcessorFeeAmount = Convert.ToDecimal(".25"),
                    ProgramId = 112,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 15,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                    DistributionReference = $"Contribution {new DateTime(2014, 3, 28, 8, 30, 0)}",
                },
                new MpGPExportDatum
                {
                    DocumentType = "SALE",
                    DocumentNumber = "10002002",
                    DepositId = 10002,
                    BatchName = "Test 2 Batch",
                    DonationDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    DepositDate = new DateTime(2015, 3, 28, 8, 30, 0),
                    CustomerId = "CONTRIBUTI001",
                    DepositAmount = "400.00",
                    CheckbookId = "PNC001",
                    CashAccount = "77777-031-20",
                    ReceivableAccount = "77777-031-21",
                    DistributionAccount = "77777-031-22",
                    DonationAmount = Convert.ToDecimal("20.00"),
                    Amount = Convert.ToDecimal(0.12),
                    ProgramId = 127,
                    ProccessorFeeMappingId = 127,
                    PaymentTypeId = 15,
                    ScholarshipExpenseAccount = "90551-031-02",
                    ScholarshipPaymentTypeId = 9,
                    DistributionReference = $"Processor Fees {new DateTime(2015, 3, 28, 8, 30, 0)}",
                },
            };
        }
    
        [Test]
        public void TestGetDonationsForDonor()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67"
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, //bank
                    transactionCode = "tx_78"
                }
            };

            var donor = new MpContactDonor
            {
                ContactId = 987,
                DonorId = 123,
                StatementTypeId = 456,
                Details = new MpContactDetails
                {
                    HouseholdId = 901
                }
            };

            var household = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    DonorId = 678,
                    StatementTypeId = 456
                },
                new MpHouseholdMember
                {
                    DonorId = 123,
                    StatementTypeId = 456
                },
                new MpHouseholdMember
                {
                    DonorId = 444,
                    StatementTypeId = 455
                },
                new MpHouseholdMember
                {
                    DonorId = 345,
                    StatementTypeId = 456
                }
            };
            _contactService.Setup(mocked => mocked.GetHouseholdFamilyMembers(901)).Returns(household);
            _mpDonorService.Setup(mocked => mocked.GetEmailViaDonorId(123)).Returns(donor);
            _mpDonorService.Setup(mocked => mocked.GetDonations(new [] {123, 678, 345}, "1999")).Returns(donations);
            _paymentService.Setup(mocked => mocked.GetCharge("tx_67")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "9876"
                }
            });
            _paymentService.Setup(mocked => mocked.GetCharge("tx_78")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "8765",
                    Brand = CardBrand.AmericanExpress
                }
            });
            var response = _fixture.GetDonationsForDonor(123, "1999");
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(3, response.Donations.Count);
            Assert.AreEqual(donations[0].donationAmt + donations[1].donationAmt + donations[2].donationAmt, response.DonationTotalAmount);

            Assert.AreEqual(donations[2].donationDate, response.Donations[0].DonationDate);
            Assert.AreEqual("8765", response.Donations[0].Source.AccountNumberLast4);
            Assert.AreEqual(CreditCardType.AmericanExpress, response.Donations[0].Source.CardType);

            Assert.AreEqual(donations[1].donationDate, response.Donations[1].DonationDate);
            Assert.AreEqual("9876", response.Donations[1].Source.AccountNumberLast4);

            Assert.AreEqual(donations[0].donationDate, response.Donations[2].DonationDate);
            Assert.AreEqual("cash", response.Donations[2].Source.AccountHolderName);
        }

        [Test]
        public void TestGetDonationsForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash,
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 0,
                }
            };
            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", false, "1999", true)).Returns(donations);
            _paymentService.Setup(mocked => mocked.GetCharge("tx_67")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "9876"
                }
            });
            _paymentService.Setup(mocked => mocked.GetCharge("tx_78")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "8765",
                    Brand = CardBrand.AmericanExpress
                }
            });
            var response = _fixture.GetDonationsForAuthenticatedUser("auth token", "1999", null, false);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(3, response.Donations.Count);
            Assert.AreEqual(donations[0].donationAmt + donations[1].donationAmt + donations[2].donationAmt, response.DonationTotalAmount);

            Assert.AreEqual(donations[2].donationDate, response.Donations[0].DonationDate);
            Assert.AreEqual("8765", response.Donations[0].Source.AccountNumberLast4);
            Assert.AreEqual(CreditCardType.AmericanExpress, response.Donations[0].Source.CardType);

            Assert.AreEqual(donations[1].donationDate, response.Donations[1].DonationDate);
            Assert.AreEqual("9876", response.Donations[1].Source.AccountNumberLast4);

            Assert.AreEqual(donations[0].donationDate, response.Donations[2].DonationDate);
            Assert.AreEqual("cash", response.Donations[2].Source.AccountHolderName);
        }

        [Test]
        public void TestGetDonationYearsForDonor()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("2000-01-01 00:00:01"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1998-10-30 23:59:59"),
                }
            };

            var softCreditDonations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationDate = DateTime.Parse("1997-12-31 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("2001-01-01 00:00:01"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1997-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1996-10-30 23:59:59"),
                }
            };

            var expectedYears = new List<string>
            {
                "2001",
                "2000",
                "1999",
                "1998",
                "1997",
                "1996"
            };

            var donor = new MpContactDonor
            {
                ContactId = 987,
                DonorId = 123,
                StatementTypeId = 456
            };
            _mpDonorService.Setup(mocked => mocked.GetEmailViaDonorId(123)).Returns(donor);
            _mpDonorService.Setup(mocked => mocked.GetDonations(new [] {123}, null)).Returns(donations);
            _mpDonorService.Setup(mocked => mocked.GetSoftCreditDonations(new [] {123}, null)).Returns(softCreditDonations);

            var response = _fixture.GetDonationYearsForDonor(123);
            _mpDonorService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.AvailableDonationYears);
            Assert.AreEqual(expectedYears.Count, response.AvailableDonationYears.Count);
        }

        [Test]
        public void TestGetDonationYearsForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("2000-01-01 00:00:01"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1998-10-30 23:59:59"),
                }
            };

            var softCreditDonations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationDate = DateTime.Parse("1997-12-31 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("2001-01-01 00:00:01"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1997-11-30 23:59:59"),
                },
                new MpDonation
                {
                    donationDate = DateTime.Parse("1996-10-30 23:59:59"),
                }
            };

            var expectedYears = new List<string>
            {
                "2001",
                "2000",
                "1999",
                "1998",
                "1997",
                "1996"
            };

            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", null, null, true)).Returns(donations.Concat(softCreditDonations).ToList());

            var response = _fixture.GetDonationYearsForAuthenticatedUser("auth token");
            _mpDonorService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.AvailableDonationYears);
            Assert.AreEqual(expectedYears.Count, response.AvailableDonationYears.Count);
        }

        [Test]
        public void TestSoftCreditGetDonationsForDonor()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash
                    softCreditDonorId = 123,
                    donorDisplayName = "Fidelity",
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 123,
                    donorDisplayName = "US Bank",
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, //bank
                    transactionCode = "tx_78",
                    softCreditDonorId = 123,
                    donorDisplayName = "Citi",
                }
            };

            var donor = new MpContactDonor
            {
                ContactId = 987,
                DonorId = 123,
                StatementTypeId = 456,
                Details = new MpContactDetails
                {
                    HouseholdId = 901
                }
            };


            var householdMembers = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    DonorId = 678,
                    StatementTypeId = 456
                },
                new MpHouseholdMember
                {
                    DonorId = 123,
                    StatementTypeId = 456
                },
                new MpHouseholdMember
                {
                    DonorId = 444,
                    StatementTypeId = 455
                },
                new MpHouseholdMember
                {
                    DonorId = 345,
                    StatementTypeId = 456
                }
            };

            _contactService.Setup(mocked => mocked.GetHouseholdFamilyMembers(901)).Returns(householdMembers);
            _mpDonorService.Setup(mocked => mocked.GetEmailViaDonorId(123)).Returns(donor);
            _mpDonorService.Setup(mocked => mocked.GetSoftCreditDonations(new[] { 123, 678, 345 }, "1999")).Returns(donations);
            var response = _fixture.GetDonationsForDonor(123, "1999", true);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(3, response.Donations.Count);
            Assert.AreEqual(donations[0].donationAmt + donations[1].donationAmt + donations[2].donationAmt, response.DonationTotalAmount);

            Assert.AreEqual(donations[2].donationDate, response.Donations[0].DonationDate);
            Assert.AreEqual(null, response.Donations[0].Source.AccountNumberLast4);
            Assert.AreEqual(null, response.Donations[0].Source.CardType);
            Assert.AreEqual("Citi", response.Donations[0].Source.AccountHolderName);
            Assert.AreEqual(PaymentType.SoftCredit, response.Donations[0].Source.SourceType);

            Assert.AreEqual(donations[1].donationDate, response.Donations[1].DonationDate);
            Assert.AreEqual(null, response.Donations[1].Source.AccountNumberLast4);
            Assert.AreEqual("US Bank", response.Donations[1].Source.AccountHolderName);

            Assert.AreEqual(donations[0].donationDate, response.Donations[2].DonationDate);
            Assert.AreEqual("Fidelity", response.Donations[2].Source.AccountHolderName);
        }

        [Test]
        public void TestSoftCreditGetDonationsForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash
                    softCreditDonorId = 123,
                    donorDisplayName = "Fidelity",
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 123,
                    donorDisplayName = "US Bank",
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 123,
                    donorDisplayName = "Citi",
                }
            };

            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", true, "1999", true)).Returns(donations);
            var response = _fixture.GetDonationsForAuthenticatedUser("auth token", "1999", null, true);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(3, response.Donations.Count);
            Assert.AreEqual(donations[0].donationAmt + donations[1].donationAmt + donations[2].donationAmt, response.DonationTotalAmount);

            Assert.AreEqual(donations[2].donationDate, response.Donations[0].DonationDate);
            Assert.AreEqual(null, response.Donations[0].Source.AccountNumberLast4);
            Assert.AreEqual(null, response.Donations[0].Source.CardType);
            Assert.AreEqual("Citi", response.Donations[0].Source.AccountHolderName);

            Assert.AreEqual(donations[1].donationDate, response.Donations[1].DonationDate);
            Assert.AreEqual(null, response.Donations[1].Source.AccountNumberLast4);
            Assert.AreEqual("US Bank", response.Donations[1].Source.AccountHolderName);

            Assert.AreEqual(donations[0].donationDate, response.Donations[2].DonationDate);
            Assert.AreEqual("Fidelity", response.Donations[2].Source.AccountHolderName);
        }

        [Test]
        public void TestGetAllDonationsForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash,
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 79,
                    donationDate = DateTime.Parse("1999-09-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 123,
                    donorDisplayName = "US Bank",
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 80,
                    donationDate = DateTime.Parse("1999-08-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 123,
                    donorDisplayName = "Citi",
                }
            };

            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", null, "1999", true)).Returns(donations);
            _paymentService.Setup(mocked => mocked.GetCharge("tx_67")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "9876"
                }
            });
            _paymentService.Setup(mocked => mocked.GetCharge("tx_78")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "8765",
                    Brand = CardBrand.AmericanExpress
                }
            });
            var response = _fixture.GetDonationsForAuthenticatedUser("auth token", "1999");
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(5, response.Donations.Count);
            Assert.AreEqual(donations[0].donationAmt + donations[1].donationAmt + donations[2].donationAmt + donations[3].donationAmt + donations[4].donationAmt, response.DonationTotalAmount);

            Assert.AreEqual(donations[0].donationDate, response.Donations[4].DonationDate);
            Assert.AreEqual("cash", response.Donations[4].Source.AccountHolderName);

            Assert.AreEqual(donations[1].donationDate, response.Donations[3].DonationDate);
            Assert.AreEqual("9876", response.Donations[3].Source.AccountNumberLast4);

            Assert.AreEqual(donations[2].donationDate, response.Donations[2].DonationDate);
            Assert.AreEqual("8765", response.Donations[2].Source.AccountNumberLast4);
            Assert.AreEqual(CreditCardType.AmericanExpress, response.Donations[2].Source.CardType);

            Assert.AreEqual(donations[4].donationDate, response.Donations[0].DonationDate);
            Assert.AreEqual(null, response.Donations[0].Source.AccountNumberLast4);
            Assert.AreEqual(null, response.Donations[0].Source.CardType);
            Assert.AreEqual("Citi", response.Donations[0].Source.AccountHolderName);

            Assert.AreEqual(donations[3].donationDate, response.Donations[1].DonationDate);
            Assert.AreEqual(null, response.Donations[1].Source.AccountNumberLast4);
            Assert.AreEqual("US Bank", response.Donations[1].Source.AccountHolderName);
        }

        [Test]
        public void TestGetLimitedDonationsForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash,
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 0,
                }
            };

            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 123,
                    donationId = 67,
                    donationDistributionAmt = 367,
                    donationDistributionProgram = "Crossroads",
                }
            );
            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 124,
                    donationId = 67,
                    donationDistributionAmt = 100,
                    donationDistributionProgram = "Beans & Rice",
                }
            );
            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 125,
                    donationId = 67,
                    donationDistributionAmt = 100,
                    donationDistributionProgram = "Super Bowl Party",
                }
            );

            donations[1].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 126,
                    donationId = 45,
                    donationDistributionAmt = 103,
                    donationDistributionProgram = "Beans & Rice",
                }
            );
            donations[1].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 127,
                    donationId = 45,
                    donationDistributionAmt = 20,
                    donationDistributionProgram = "Crossroads",
                }
            );

            donations[2].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 128,
                    donationId = 78,
                    donationDistributionAmt = 678,
                    donationDistributionProgram = "Crossroads",
                }
            );

            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", null, "1999", true)).Returns(donations);
            _paymentService.Setup(mocked => mocked.GetCharge("tx_67")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "9876"
                }
            });
            _paymentService.Setup(mocked => mocked.GetCharge("tx_78")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "8765",
                    Brand = CardBrand.AmericanExpress
                }
            });
            var response = _fixture.GetDonationsForAuthenticatedUser("auth token", "1999", 4);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(2, response.Donations.Count);
            Assert.AreEqual(2, response.Donations[0].Distributions.Count);
            Assert.AreEqual(2, response.Donations[1].Distributions.Count);
            Assert.AreEqual("45", response.Donations[0].Id);
            Assert.AreEqual("67", response.Donations[1].Id);
        }


        [Test]
        public void TestGetLimitedDonationsAgainForAuthenticatedUser()
        {
            var donations = new List<MpDonation>
            {
                new MpDonation
                {
                    donationAmt = 567,
                    donationId = 67,
                    donationDate = DateTime.Parse("1999-10-30 23:59:59"),
                    paymentTypeId = 5, //bank
                    transactionCode = "tx_67",
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 123,
                    donationId = 45,
                    donationDate = DateTime.Parse("1999-12-31 23:59:59"),
                    paymentTypeId = 2, // Cash,
                    softCreditDonorId = 0,
                },
                new MpDonation
                {
                    donationAmt = 678,
                    donationId = 78,
                    donationDate = DateTime.Parse("1999-11-30 23:59:59"),
                    paymentTypeId = 4, // credit card
                    transactionCode = "tx_78",
                    softCreditDonorId = 0,
                }
            };

            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 123,
                    donationId = 67,
                    donationDistributionAmt = 367,
                    donationDistributionProgram = "Crossroads",
                }
            );
            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 124,
                    donationId = 67,
                    donationDistributionAmt = 100,
                    donationDistributionProgram = "Beans & Rice",
                }
            );
            donations[0].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 125,
                    donationId = 67,
                    donationDistributionAmt = 100,
                    donationDistributionProgram = "Super Bowl Party",
                }
            );

            donations[1].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 126,
                    donationId = 45,
                    donationDistributionAmt = 103,
                    donationDistributionProgram = "Beans & Rice",
                }
            );
            donations[1].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 127,
                    donationId = 45,
                    donationDistributionAmt = 20,
                    donationDistributionProgram = "Crossroads",
                }
            );

            donations[2].Distributions.Add(
                new MpDonationDistribution()
                {
                    donationDistributionId = 128,
                    donationId = 78,
                    donationDistributionAmt = 678,
                    donationDistributionProgram = "Crossroads",
                }
            );

            _mpDonorService.Setup(mocked => mocked.GetDonationsForAuthenticatedUser("auth token", null, "1999", true)).Returns(donations);
            _paymentService.Setup(mocked => mocked.GetCharge("tx_67")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "9876"
                }
            });
            _paymentService.Setup(mocked => mocked.GetCharge("tx_78")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    AccountNumberLast4 = "8765",
                    Brand = CardBrand.AmericanExpress
                }
            });
            var response = _fixture.GetDonationsForAuthenticatedUser("auth token", "1999", 4);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(response);
            Assert.NotNull(response.Donations);
            Assert.AreEqual(3, response.Donations.Count);
            Assert.AreEqual(2, response.Donations[0].Distributions.Count);
            Assert.AreEqual(1, response.Donations[1].Distributions.Count);
            Assert.AreEqual(1, response.Donations[2].Distributions.Count);
            Assert.AreEqual("45", response.Donations[0].Id);
            Assert.AreEqual("78", response.Donations[1].Id);
            Assert.AreEqual("67", response.Donations[2].Id);
        }

        [Test]
        public void TestCreateDonationForBankAccountErrorRefundNoBalanceTransaction()
        {
            var refund = new StripeRefund
            {
                Data = new List<StripeRefundData>
                {
                    new StripeRefundData()
                }
            };

            Assert.IsNull(_fixture.CreateDonationForBankAccountErrorRefund(refund));
            _mpDonorService.VerifyAll();
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForBankAccountErrorRefundWrongBalanceTransactionType()
        {
            var refund = new StripeRefund
            {
                Data = new List<StripeRefundData>
                {
                    new StripeRefundData
                    {
                        BalanceTransaction = new StripeBalanceTransaction
                        {
                            Type = "not_payment_failure_refund"
                        }
                    }
                }
            };

            Assert.IsNull(_fixture.CreateDonationForBankAccountErrorRefund(refund));
            _mpDonorService.VerifyAll();
            _mpDonationService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForBankAccountErrorRefundNoDonationFound()
        {
            var refund = new StripeRefund
            {
                Data = new List<StripeRefundData>
                {
                    new StripeRefundData
                    {
                        BalanceTransaction = new StripeBalanceTransaction
                        {
                            Type = "payment_failure_refund"
                        },
                        Charge = new StripeCharge
                        {
                            Id = "py_123"
                        }
                    }
                }
            };

            var donationNotFound = new DonationNotFoundException("py_123");
            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("py_123", true)).Throws(donationNotFound);

            Assert.IsNull(_fixture.CreateDonationForBankAccountErrorRefund(refund));
            _mpDonorService.VerifyAll();
            _mpDonationService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForBankAccountError()
        {
            var refund = new StripeRefund
            {
                Data = new List<StripeRefundData>
                {
                    new StripeRefundData
                    {
                        Amount = "12345",
                        Id = "pyr_123",
                        BalanceTransaction = new StripeBalanceTransaction
                        {
                            Type = "payment_failure_refund",
                            Fee = 678,
                            Created = DateTime.Today.AddDays(-1)
                        },
                        Charge = new StripeCharge
                        {
                            Id = "py_123"
                        }
                    }
                }
            };


            var donation = new MpDonation
            {
                Distributions =
                {
                    new MpDonationDistribution
                    {
                        donationDistributionProgram = "9",
                        donationDistributionAmt = 9,
                        PledgeId = 9
                    },
                    new MpDonationDistribution
                    {
                        donationDistributionProgram = "8",
                        donationDistributionAmt = 8,
                        PledgeId = 8
                    }

                },
                donationId = 987,
                paymentTypeId = 4
            };
            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("py_123", true)).Returns(donation);

            _mpDonorService.Setup(mocked => mocked.CreateDonationAndDistributionRecord(It.Is<MpDonationAndDistributionRecord>(d => 
                !d.Anonymous 
                && d.ChargeId.Equals(refund.Data[0].Id) 
                && d.CheckNumber == null 
                && d.CheckScannerBatchName == null 
                && d.DonationAmt == -(int.Parse(refund.Data[0].Amount) / Constants.StripeDecimalConversionValue)
                && d.DonationStatus == (int)crds_angular.Models.Crossroads.Stewardship.DonationStatus.Declined 
                && d.DonorAcctId.Equals(string.Empty) 
                && d.DonorId == 987
                && d.FeeAmt == refund.Data[0].BalanceTransaction.Fee 
                && d.PledgeId == null 
                && !d.RecurringGift 
                && d.ProcessorId.Equals(string.Empty) 
                && d.ProgramId.Equals(donation.Distributions[0].donationDistributionProgram)
                && d.PymtType.Equals(MinistryPlatform.Translation.Enum.PaymentType.GetPaymentType(donation.paymentTypeId).name) 
                && d.RecurringGiftId == null 
                && !d.RegisteredDonor 
                && d.SetupDate == refund.Data[0].BalanceTransaction.Created
                && d.Notes.Equals(string.Format("Reversed from DonationID {0}", donation.donationId))
                && d.HasDistributions
                && d.Distributions.Count == 2
                && d.Distributions[0].donationDistributionAmt == -donation.Distributions[0].donationDistributionAmt
                && d.Distributions[0].donationDistributionProgram.Equals(donation.Distributions[0].donationDistributionProgram)
                && d.Distributions[0].PledgeId == donation.Distributions[0].PledgeId
                && d.Distributions[1].donationDistributionAmt == -donation.Distributions[1].donationDistributionAmt
                && d.Distributions[1].donationDistributionProgram.Equals(donation.Distributions[1].donationDistributionProgram)
                && d.Distributions[1].PledgeId == donation.Distributions[1].PledgeId
            ), false)).Returns(999);

            var result = _fixture.CreateDonationForBankAccountErrorRefund(refund);
            Assert.IsNotNull(result);
            Assert.AreEqual(999, result.Value);

            _mpDonorService.VerifyAll();
            _mpDonationService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForInvoiceNoAmount()
        {
            var invoice = new StripeInvoice
            {
                Subscription = "sub_123",
                Amount = 0,
                Charge = "ch_123",
            };

            _fixture.CreateDonationForInvoice(invoice);
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForInvoiceNoCharge()
        {
            var invoice = new StripeInvoice
            {
                Subscription = "sub_123",
                Amount = 123,
                Charge = "   ",
            };

            _fixture.CreateDonationForInvoice(invoice);
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
        }

        [Test]
        public void TestCreateDonationForInvoiceDonationAlreadyExists()
        {
            const string processorId = "cus_123";
            const string subscriptionId = "sub_123";
            const string chargeId = "ch_123";

            var invoice = new StripeInvoice
            {
                Subscription = subscriptionId,
                Amount = 12300,
                Charge = chargeId,
                Customer = processorId,
            };

            var donation = new MpDonation
            {
                donationId = 123
            };

            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId(chargeId, false)).Returns(donation);

            var donationId = _fixture.CreateDonationForInvoice(invoice);
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
            _mpDonationService.VerifyAll();

            Assert.AreEqual(donation.donationId, donationId);
        }

        [Test]
        public void TestCreateDonationForInvoice()
        {
            const string processorId = "cus_123";
            const string subscriptionId = "sub_123";
            const string chargeId = "ch_123";

            DateTime invoiceDate = new DateTime(2016, 3, 16);

            var invoice = new StripeInvoice
            {
                Subscription = subscriptionId,
                Amount = 12300,
                Charge = chargeId,
                Customer = processorId,
                Date = invoiceDate
            };

            const int chargeAmount = 45600;
            int? feeAmount = 987;

            var charge = new StripeCharge
            {
                Amount = chargeAmount,
                BalanceTransaction = new StripeBalanceTransaction
                {
                    Amount = 78900,
                    Fee = feeAmount
                },
                Status = "succeeded",
                ProcessorId = processorId
            };

            _paymentService.Setup(mocked => mocked.GetCharge(chargeId)).Returns(charge);

            const int donorId = 321;
            const string programId = "3";
            const string paymentType = "Bank";
            const int recurringGiftId = 654;
            const int donorAccountId = 987;
            const int donationStatus = 4;
            const decimal amt = 789;

            var recurringGift = new MpCreateDonationDistDto
            {
                Amount = amt,
                DonorAccountId = donorAccountId,
                DonorId = donorId,
                PaymentType = paymentType,
                ProgramId = programId,
                RecurringGiftId = recurringGiftId,
                SubscriptionId = subscriptionId, 
                StripeCustomerId = processorId 
            };
            _mpDonationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId(chargeId, false)).Throws(new DonationNotFoundException(chargeId));
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftForSubscription(subscriptionId, processorId)).Returns(recurringGift);
            _mpDonorService.Setup(mocked => mocked.UpdateRecurringGiftFailureCount(recurringGift.RecurringGiftId.Value, Constants.ResetFailCount));

            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonationAndDistributionRecord(
                        It.Is<MpDonationAndDistributionRecord>(
                            d => d.DonationAmt == (int)(chargeAmount / Constants.StripeDecimalConversionValue) &&
                                 d.FeeAmt == feeAmount &&
                                 d.DonorId == donorId &&
                                 d.ProgramId.Equals(programId) &&
                                 d.PledgeId == null &&
                                 d.ChargeId.Equals(chargeId) &&
                                 d.PymtType.Equals(paymentType) &&
                                 d.ProcessorId.Equals(processorId) &&
                                 d.RegisteredDonor &&
                                 !d.Anonymous &&
                                 d.RecurringGift &&
                                 d.RecurringGiftId == recurringGiftId &&
                                 d.DonorAcctId.Equals(donorAccountId + "") &&
                                 d.CheckScannerBatchName == null &&
                                 d.DonationStatus == donationStatus &&
                                 d.CheckNumber == null &&
                                 d.SetupDate == invoiceDate), false)).Returns(123);

            _fixture.CreateDonationForInvoice(invoice);
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
        }

    }
}