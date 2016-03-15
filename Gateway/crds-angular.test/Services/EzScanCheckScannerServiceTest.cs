﻿using System;
using System.Collections.Generic;
using crds_angular.App_Start;
using crds_angular.DataAccess.Interfaces;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Models;
using Moq;
using NUnit.Framework;
using MPServices=MinistryPlatform.Translation.Services.Interfaces;

namespace crds_angular.test.Services
{
    public class EzScanCheckScannerServiceTest
    {
        private EzScanCheckScannerService _fixture;
        private Mock<ICheckScannerDao> _checkScannerDao;
        private Mock<IDonorService> _donorService;
        private Mock<IPaymentService> _paymentService;
        private Mock<MPServices.IDonorService> _mpDonorService;

        [SetUp]
        public void SetUp()
        {
            _checkScannerDao = new Mock<ICheckScannerDao>(MockBehavior.Strict);
            _donorService = new Mock<IDonorService>(MockBehavior.Strict);
            _paymentService = new Mock<IPaymentService>(MockBehavior.Strict);
            _mpDonorService = new Mock<MPServices.IDonorService>(MockBehavior.Strict);
            _fixture = new EzScanCheckScannerService(_checkScannerDao.Object, _donorService.Object, _paymentService.Object, _mpDonorService.Object);
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void TestGetOpenBatches()
        {
            var batches = new List<CheckScannerBatch>();
            _checkScannerDao.Setup(mocked => mocked.GetBatches(true)).Returns(batches);

            var result = _fixture.GetBatches();
            _checkScannerDao.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(batches, result);
        }

        [Test]
        public void TestGetAllBatches()
        {
            var batches = new List<CheckScannerBatch>();
            _checkScannerDao.Setup(mocked => mocked.GetBatches(false)).Returns(batches);

            var result = _fixture.GetBatches(false);
            _checkScannerDao.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(batches, result);
        }

        [Test]
        public void TestGetChecksForBatch()
        {
            var checks = new List<CheckScannerCheck>();

            _checkScannerDao.Setup(m => m.GetChecksForBatch("batch123")).Returns(checks);

            var result = _fixture.GetChecksForBatch("batch123");
            _checkScannerDao.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(checks, result);
        }

        [Test]
        public void TestUpdateBatchStatus()
        {
            var batch = new CheckScannerBatch
            {
                Status = BatchStatus.NotExported
            };
            _checkScannerDao.Setup(mocked => mocked.UpdateBatchStatus("batch123", BatchStatus.Exported)).Returns(batch);

            var result = _fixture.UpdateBatchStatus("batch123", BatchStatus.Exported);
            _checkScannerDao.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(batch, result);
        }

        [Test]
        public void TestCreateDonationsForBatch()
        {
            const string encryptedKey = "PH/rty1234";
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";
            const string donorAcctId = "4321";

            var checks = new List<CheckScannerCheck>
            {
                new CheckScannerCheck
                {
                    Id = 11111,
                    AccountNumber = "111",
                    Address = new crds_angular.Models.Crossroads.Stewardship.Address
                    {
                        Line1 = "1 line 1",
                        Line2 = "1 line 2",
                        City = "1 city",
                        State = "1 state",
                        PostalCode = "1 postal"
                    },
                    Amount = 111100,
                    CheckDate = DateTime.Now.AddHours(1),
                    CheckNumber = " 0 0 00000222111111111111111",
                    Name1 = "1 name 1",
                    Name2 = "1 name 2",
                    RoutingNumber = "1010",
                    ScanDate = DateTime.Now.AddHours(2)
                },
                new CheckScannerCheck
                {
                    Id = 22222,
                    AccountNumber =  "222",
                    Address = new crds_angular.Models.Crossroads.Stewardship.Address
                    {
                        Line1 = "2 line 1",
                        Line2 = "2 line 2",
                        City = "2 city",
                        State = "2 state",
                        PostalCode = "2 postal"
                    },
                    Amount = 222200,
                    CheckDate = DateTime.Now.AddHours(3),
                    CheckNumber = "22222",
                    Name1 = "2 name 1",
                    Name2 = "2 name 2",
                    RoutingNumber = "2020",
                    ScanDate = DateTime.Now.AddHours(4)
                },
                new CheckScannerCheck
                {
                    Id = 33333,
                    DonorId = 333333,
                    AccountNumber =  "333",
                    Address = new crds_angular.Models.Crossroads.Stewardship.Address
                    {
                        Line1 = "3 line 1",
                        Line2 = "3 line 2",
                        City = "3 city",
                        State = "3 state",
                        PostalCode = "3 postal"
                    },
                    Amount = 333300,
                    CheckDate = DateTime.Now.AddHours(3),
                    CheckNumber = "33333",
                    Name1 = "3 name 1",
                    Name2 = "3 name 2",
                    RoutingNumber = "3030",
                    ScanDate = DateTime.Now.AddHours(4)
                }
            };

            var contactDonorExisting = new ContactDonor
            {
                ProcessorId = "111000111",
                DonorId = 111111,
                RegisteredUser = true,
                Account = new DonorAccount
                {
                    DonorAccountId = Int32.Parse(donorAcctId),
                    ProcessorId = "cus_aeirhsjidhriuewiwq",
                    ProcessorAccountId = "py_dgsttety6737hjjhweiu3"
                }
            };
          
            _checkScannerDao.Setup(mocked => mocked.GetChecksForBatch("batch123")).Returns(checks);
            _checkScannerDao.Setup(mocked => mocked.UpdateBatchStatus("batch123", BatchStatus.Exported)).Returns(new CheckScannerBatch());
            _checkScannerDao.Setup(mocked => mocked.UpdateCheckStatus(11111, true, null));
            _checkScannerDao.Setup(mocked => mocked.UpdateCheckStatus(22222, true, null));
            _checkScannerDao.Setup(mocked => mocked.UpdateCheckStatus(33333, true, null));
            
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(checks[0].AccountNumber, checks[0].RoutingNumber)).Returns(contactDonorExisting);
           
            _paymentService.Setup(mocked => mocked.ChargeCustomer(contactDonorExisting.Account.ProcessorId, contactDonorExisting.Account.ProcessorAccountId, checks[0].Amount, contactDonorExisting.DonorId)).Returns(new StripeCharge
            {
                Id = "1020304",
                Source = new StripeSource()
                {
                    id = "py_dgsttety6737hjjhweiu3"
                },
                BalanceTransaction = new StripeBalanceTransaction
                {
                    Fee = 123
                }
            });

            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[1].AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[1].RoutingNumber)).Returns(decryptRout);
            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[0].AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[0].RoutingNumber)).Returns(decryptRout);
            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            //_mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey, contactDonorExisting.Account.ProcessorAccountId, contactDonorExisting.Account.ProcessorId)).Returns(donorAcctId);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonationAndDistributionRecord(
                        It.Is<DonationAndDistributionRecord>(d =>
                                                                 d.DonationAmt == checks[0].Amount &&
                                                                 d.FeeAmt == 123 &&
                                                                 d.DonorId == contactDonorExisting.DonorId &&
                                                                 d.ProgramId.Equals("9090") &&
                                                                 d.ChargeId.Equals("1020304") &&
                                                                 d.PymtType.Equals("check") &&
                                                                 d.ProcessorId.Equals(contactDonorExisting.Account.ProcessorId) &&
                                                                 d.SetupDate.Equals(checks[0].CheckDate) &&
                                                                 d.RegisteredDonor &&
                                                                 d.DonorAcctId == donorAcctId &&
                                                                 d.CheckScannerBatchName.Equals("batch123") &&
                                                                 d.CheckNumber.Equals("111111111111111")), false))
                .Returns(321);

            var contactDonorNew = new ContactDonor
            {
                ProcessorId = "cus_222000222",
                DonorId = 222222,
                RegisteredUser = false,
                Account = new DonorAccount
                {
                    DonorAccountId = 220022,
                    ProcessorId = "cus_222000222",
                    ProcessorAccountId = "card_123456789"
                }
            };

            var mockCharge = new StripeCharge
            {
                Id = "40302010",
                Source = new StripeSource()
                {
                    id = "ba_dgsttety6737hjjhweiu3"
                }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(checks[1].AccountNumber, checks[1].RoutingNumber)).Returns((ContactDonor) null);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[1].AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[1].RoutingNumber)).Returns(decryptRout);
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct, decryptRout)).Returns(new StripeToken { Id = "tok123"});
            _donorService.Setup(
                mocked =>
                    mocked.CreateOrUpdateContactDonor(
                        It.Is<ContactDonor>(
                            o =>
                                o.Details.DisplayName.Equals("2 name 1") && o.Details.Address.Line1.Equals("2 line 1") && o.Details.Address.Line2.Equals("2 line 2") &&
                                o.Details.Address.City.Equals("2 city") && o.Details.Address.State.Equals("2 state") && o.Details.Address.PostalCode.Equals("2 postal") &&
                                o.Account.RoutingNumber.Equals(decryptRout) && o.Account.AccountNumber.Equals(decrypAcct) && o.Account.Type == AccountType.Checking),
                        It.IsAny<string>(),
                        string.Empty,
                        "tok123",
                        It.IsAny<DateTime>()))
                .Returns(contactDonorNew);
            _paymentService.Setup(mocked => mocked.ChargeCustomer(contactDonorNew.Account.ProcessorId, contactDonorNew.Account.ProcessorAccountId, checks[1].Amount, contactDonorNew.DonorId)).Returns(mockCharge);

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            //_mpDonorService.Setup(mocked => mocked.CreateDonorAccount(encryptedKey, mockCharge.Source.id, contactDonorNew.ProcessorId)).Returns(donorAcctId);

            //_mpDonorService.Setup(
            //    mocked =>
            //        mocked.CreateDonorAccount(null,
            //                                  checks[1].RoutingNumber,
            //                                  checks[1].AccountNumber,
            //                                  encryptedKey,
            //                                  222222,
            //                                  mockCharge.Source.id,
            //                                  contactDonorNew.ProcessorId)).Returns(int.Parse(donorAcctId));

            //_mpDonorService.Setup(
            //    mocked =>
            //        mocked.CreateDonorAccount(null,
            //                                  checks[0].RoutingNumber,
            //                                  checks[0].AccountNumber,
            //                                  encryptedKey + "88",
            //                                  444444,
            //                                  nonAccountProcessorAccountId,
            //                                  nonAccountProcessorId)).Returns(1);

            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonationAndDistributionRecord(
                        It.Is<DonationAndDistributionRecord>(d =>
                                                                 d.DonationAmt == checks[1].Amount &&
                                                                 d.FeeAmt == null &&
                                                                 d.DonorId == contactDonorNew.DonorId &&
                                                                 d.ProgramId.Equals("9090") &&
                                                                 d.ChargeId.Equals("40302010") &&
                                                                 d.PymtType.Equals("check") &&
                                                                 d.ProcessorId.Equals(contactDonorNew.Account.ProcessorId) &&
                                                                 d.SetupDate.Equals(checks[1].CheckDate) &&
                                                                 !d.RegisteredDonor &&
                                                                 d.DonorAcctId == 220022.ToString() &&
                                                                 d.CheckScannerBatchName.Equals("batch123") &&
                                                                 d.CheckNumber.Equals("22222")), false))
                .Returns(654);

            var contactDonorExistingStripeCustomer = new ContactDonor
            {
                ProcessorId = "cus_333000333",
                DonorId = 333333,
                ContactId = 3333333,
                RegisteredUser = true
            };

            var contactDonorExistingStripeCustomerWithAccount = new ContactDonor
            {
                ProcessorId = "cus_333000333",
                DonorId = 333333,
                ContactId = 3333333,
                RegisteredUser = true,
                Account = new DonorAccount
                {
                    DonorAccountId = 330033,
                    ProcessorAccountId = "src_123",
                    ProcessorId = "cus_333000333"
                }
            };

            var mockChargeExistingStripeCustomer = new StripeCharge
            {
                Id = "9080706050",
                Source = new StripeSource()
                {
                    id = "ba_dgsttety6737hjjhweiu398765"
                }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(checks[2].AccountNumber, checks[2].RoutingNumber)).Returns((ContactDonor)null);
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorId(333333)).Returns(contactDonorExistingStripeCustomer);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[2].AccountNumber)).Returns(decrypAcct + "99");
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[2].RoutingNumber)).Returns(decryptRout + "99");
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct + "99", decryptRout + "99")).Returns(new StripeToken { Id = "tok987" });
            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct + "99", decryptRout + "99")).Returns(encryptedKey + "99");
            _donorService.Setup(
                mocked =>
                    mocked.CreateOrUpdateContactDonor(
                        contactDonorExistingStripeCustomer,
                        encryptedKey + "99",
                        string.Empty,
                        "tok987",
                        It.IsAny<DateTime>()))
                .Returns(contactDonorExistingStripeCustomerWithAccount);
            _paymentService.Setup(
                mocked =>
                    mocked.ChargeCustomer(contactDonorExistingStripeCustomerWithAccount.Account.ProcessorId,
                                          contactDonorExistingStripeCustomerWithAccount.Account.ProcessorAccountId,
                                          checks[2].Amount,
                                          contactDonorExistingStripeCustomerWithAccount.DonorId)).Returns(mockChargeExistingStripeCustomer);

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct + "99", decryptRout + "99")).Returns(encryptedKey + "99");
            //_mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey + "99", mockChargeExistingStripeCustomer.Source.id, contactDonorExistingStripeCustomer.ProcessorId)).Returns(donorAcctId);

            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonationAndDistributionRecord(
                        It.Is<DonationAndDistributionRecord>(d =>
                                                                 d.DonationAmt == checks[2].Amount &&
                                                                 d.FeeAmt == null &&
                                                                 d.DonorId == contactDonorExistingStripeCustomerWithAccount.DonorId &&
                                                                 d.ProgramId.Equals("9090") &&
                                                                 d.ChargeId.Equals("9080706050") &&
                                                                 d.PymtType.Equals("check") &&
                                                                 d.ProcessorId.Equals(contactDonorExistingStripeCustomerWithAccount.Account.ProcessorId) &&
                                                                 d.SetupDate.Equals(checks[2].CheckDate) &&
                                                                 d.RegisteredDonor &&
                                                                 d.DonorAcctId == 330033.ToString() &&
                                                                 d.CheckScannerBatchName.Equals("batch123") &&
                                                                 d.CheckNumber.Equals("33333")), false))
                .Returns(654);


             var result = _fixture.CreateDonationsForBatch(new CheckScannerBatch
            {
                Name = "batch123",
                ProgramId = 9090
            });
            _checkScannerDao.VerifyAll();
            _donorService.VerifyAll();
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.Checks);
            Assert.AreEqual(3, result.Checks.Count);
            Assert.AreEqual(BatchStatus.Exported, result.Status);
        }

        [Test]
        public void TestCreateDonationsForBatchNoDonorAccount()
        {
            var checks = new List<CheckScannerCheck>
            {
                new CheckScannerCheck
                {
                    Id = 44444,
                    DonorId = 444444,
                    AccountNumber =  "444",
                    Address = new crds_angular.Models.Crossroads.Stewardship.Address
                    {
                        Line1 = "4 line 1",
                        Line2 = "4 line 2",
                        City = "4 city",
                        State = "4 state",
                        PostalCode = "4 postal"
                    },
                    Amount = 444400,
                    CheckDate = DateTime.Now.AddHours(3),
                    CheckNumber = "44444",
                    Name1 = "4 name 1",
                    Name2 = "4 name 2",
                    RoutingNumber = "4040",
                    ScanDate = DateTime.Now.AddHours(4)
                }
            };

            const string encryptedKey = "PH/rty1234";
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";
            const string donorAcctId = "440044";

            string nonAccountProcessorAccountId = "src_789";
            string nonAccountProcessorId = "856";

            _checkScannerDao.Setup(mocked => mocked.GetChecksForBatch("batch123")).Returns(checks);
            _checkScannerDao.Setup(mocked => mocked.UpdateBatchStatus("batch123", BatchStatus.Exported)).Returns(new CheckScannerBatch());
            _checkScannerDao.Setup(mocked => mocked.UpdateCheckStatus(44444, true, null));

            //// donor without account for ez scan check
            var contactDonorNonExistingStripeCustomer = new ContactDonor
            {
                ProcessorId = "cus_444000444",
                DonorId = 444444,
                ContactId = 444444,
                RegisteredUser = true,
                Account = new DonorAccount
                {
                    DonorAccountId = 440044,
                    Token = "tok986"
                    //ProcessorAccountId = "src_789",
                    //ProcessorId = "856"
                }
            };

            var contactDonorNonExistingStripeCustomerWithoutAccount = new ContactDonor
            {
                ProcessorId = "cus_444000444",
                DonorId = 444444,
                ContactId = 444444,
                RegisteredUser = true
            };

            var stripeCustomer = new StripeCustomer
            {
                id = "856",
                default_source = "src_789"
            };

            var nonAccountStripeCustomer = new StripeCustomer
            {
                id = "src_789"
            };

            var mockChargeNonExistingStripeCustomer = new StripeCharge
            {
                Id = "9080706050",
                Source = new StripeSource()
                {
                    id = "ba_dgsttety6737hjjhweiu398765"
                }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(checks[0].AccountNumber, checks[0].RoutingNumber)).Returns((ContactDonor)null);
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorId(444444)).Returns(contactDonorNonExistingStripeCustomerWithoutAccount);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[0].AccountNumber)).Returns(decrypAcct + "88");
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(checks[0].RoutingNumber)).Returns(decryptRout + "88");
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct + "88", decryptRout + "88")).Returns(new StripeToken { Id = "tok986" });
            _paymentService.Setup(mocked => mocked.CreateCustomer(null, contactDonorNonExistingStripeCustomer.ContactId.ToString())).Returns(stripeCustomer);
            _paymentService.Setup(mocked => mocked.AddSourceToCustomer(stripeCustomer.id, "tok986")).Returns(nonAccountStripeCustomer);

            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonorAccount(null,
                                              checks[0].RoutingNumber,
                                              checks[0].AccountNumber,
                                              encryptedKey + "88",
                                              444444,
                                              nonAccountProcessorAccountId,
                                              nonAccountProcessorId)).Returns(int.Parse(donorAcctId));

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct + "88", decryptRout + "88")).Returns(encryptedKey + "88");

            _donorService.Setup(
               mocked =>
                   mocked.CreateOrUpdateContactDonor(
                       It.IsAny<ContactDonor>(),
                       It.IsAny<string>(),
                       string.Empty,
                       It.IsAny<string>(),
                       It.IsAny<DateTime>()))
               .Returns(contactDonorNonExistingStripeCustomerWithoutAccount);

            _paymentService.Setup(
                mocked =>
                    mocked.ChargeCustomer(stripeCustomer.id,
                                          stripeCustomer.default_source,
                                          checks[0].Amount,
                                          contactDonorNonExistingStripeCustomer.DonorId)).Returns(mockChargeNonExistingStripeCustomer);

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct + "88", decryptRout + "88")).Returns(encryptedKey + "88");
            //_mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey + "88", mockChargeNonExistingStripeCustomer.Source.id, contactDonorNonExistingStripeCustomer.Account.ProcessorId)).Returns(donorAcctId);

            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonationAndDistributionRecord(
                        It.Is<DonationAndDistributionRecord>(d =>
                                                                 d.DonationAmt == checks[0].Amount &&
                                                                 d.FeeAmt == null &&
                                                                 d.DonorId == contactDonorNonExistingStripeCustomerWithoutAccount.DonorId &&
                                                                 d.ProgramId.Equals("9090") &&
                                                                 d.ChargeId.Equals("9080706050") &&
                                                                 d.PymtType.Equals("check") &&
                                                                 d.ProcessorId.Equals(nonAccountProcessorId) &&
                                                                 d.SetupDate.Equals(checks[0].CheckDate) &&
                                                                 d.RegisteredDonor &&
                                                                 d.DonorAcctId == donorAcctId &&
                                                                 d.CheckScannerBatchName.Equals("batch123") &&
                                                                 d.CheckNumber.Equals("44444")), false))
                .Returns(654);


            var result = _fixture.CreateDonationsForBatch(new CheckScannerBatch
            {
                Name = "batch123",
                ProgramId = 9090
            });
            _checkScannerDao.VerifyAll();
            _donorService.VerifyAll();
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.Checks);
            Assert.AreEqual(1, result.Checks.Count);
            Assert.AreEqual(BatchStatus.Exported, result.Status);
        }

        [Test]
        public void TestGetContactDonorForCheck()
        {
            const string encryptedKey = "disCv2kF/8HlRCWeTqolok1G4imf1cNZershgmCCFDI=";
            const string addr1 = "12 Scenic Dr";
            const string addr2 = "Penthouse Suite";
            const string city = "Honolulu";
            const string state = "HI";
            const string zip = "68168-1618";
            const string displayName = "Vacationing Vera";
            const int donorId = 123456789;
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";
            const string accountNumber = "P/H+3ccB0ZssORkd+YyJzA==";
            const string routingNumber = "TUbiKZ/Vw1l6uyGCYIIUMg==";

            var check = new CheckAccount
            {
                AccountNumber = accountNumber,
                RoutingNumber = routingNumber
            };

            var contactDonor = new ContactDonor
            {
                DonorId = donorId,
                Details = new ContactDetails
             {
                DisplayName = displayName,
                Address = new PostalAddress()
                {
                    Line1 = addr1,
                    Line2 = addr2,
                    City = city,
                    State = state,
                    PostalCode = zip
                }
              }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForCheckAccount(encryptedKey)).Returns(contactDonor);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(accountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(routingNumber)).Returns(decryptRout);
            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
        
            var result = _fixture.GetContactDonorForCheck(accountNumber, routingNumber);

            _donorService.VerifyAll();
            Assert.IsNotNull(contactDonor);
            Assert.AreEqual(result.DisplayName, contactDonor.Details.DisplayName);
            Assert.AreEqual(result.Address, contactDonor.Details.Address);
        }

        [Test]
        public void TestExistingCreateDonor()
        {
            var check = new CheckScannerCheck
            {
                Id = 11111,
                AccountNumber = "111",
                Address = new crds_angular.Models.Crossroads.Stewardship.Address
                {
                    Line1 = "1 line 1",
                    Line2 = "1 line 2",
                    City = "1 city",
                    State = "1 state",
                    PostalCode = "1 postal"
                },
                Amount = 1111,
                CheckDate = DateTime.Now.AddHours(1),
                CheckNumber = "11111",
                Name1 = "1 name 1",
                Name2 = "1 name 2",
                RoutingNumber = "1010",
                ScanDate = DateTime.Now.AddHours(2)
            };

            var contactDonorExisting = new ContactDonor
            {
                ProcessorId = "111000111",
                DonorId = 111111,
                RegisteredUser = true,
                Account = new DonorAccount
                {
                    ProcessorId = "111000111",
                    ProcessorAccountId = "111111"
                }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber)).Returns(contactDonorExisting);

            var result = _fixture.CreateDonor(check);
            _donorService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(contactDonorExisting, result);
        }

        [Test]
        public void TestCreateDonorWithDonorId()
        {
            var check = new CheckScannerCheck
            {
                Id = 11111,
                DonorId = 222,
                AccountNumber = "111",
                Address = new crds_angular.Models.Crossroads.Stewardship.Address
                {
                    Line1 = "1 line 1",
                    Line2 = "1 line 2",
                    City = "1 city",
                    State = "1 state",
                    PostalCode = "1 postal"
                },
                Amount = 1111,
                CheckDate = DateTime.Now.AddHours(1),
                CheckNumber = "11111",
                Name1 = "1 name 1",
                Name2 = "1 name 2",
                RoutingNumber = "1010",
                ScanDate = DateTime.Now.AddHours(2)
            };

            var contactDonorExisting = new ContactDonor
            {
                ContactId = 123,
                ProcessorId = "111000111",
                DonorId = 111111,
                RegisteredUser = true,
                Account = new DonorAccount
                {
                    ProcessorId = "111000111",
                    ProcessorAccountId = "111111"
                }
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorId(check.DonorId.Value)).Returns(contactDonorExisting);
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber)).Returns(contactDonorExisting);
           
            var result = _fixture.CreateDonor(check);
            _donorService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(contactDonorExisting, result);
        }

        [Test]
        public void TestCreateForCreateDonor()
        {
            var check = new CheckScannerCheck
            {
                Id = 11111,
                AccountNumber =  "P/H+3ccB0ZssORkd+YyJzA==",
                Address = new crds_angular.Models.Crossroads.Stewardship.Address
                {
                    Line1 = "1 line 1",
                    Line2 = "1 line 2",
                    City = "1 city",
                    State = "1 state",
                    PostalCode = "1 postal"
                },
                Amount = 1111,
                CheckDate = DateTime.Now.AddHours(1),
                CheckNumber = "11111",
                Name1 = "1 name 1",
                Name2 = "1 name 2",
                RoutingNumber = "TUbiKZ/Vw1l6uyGCYIIUMg==",
                ScanDate = DateTime.Now.AddHours(2)
            };


            var contactDonorNew = new ContactDonor
            {
                ProcessorId = "222000222",
                DonorId = 222222,
                RegisteredUser = false,
                Account = new DonorAccount
                {
                    ProcessorAccountId = "py_dgsttety6737hjjhweiu3"
                }
            };
            var token = new StripeToken
            {
                Id = "12t4token"
            };
            const string encryptedKey = "PH/rty1234";
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";
      
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber)).Returns((ContactDonor)null);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.RoutingNumber)).Returns(decryptRout);
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct, decryptRout)).Returns(token);

            _donorService.Setup(
                mocked =>
                    mocked.CreateOrUpdateContactDonor(
                        It.Is<ContactDonor>(
                            o => 
                                o.Details.DisplayName.Equals(check.Name1) && o.Details.Address.Line1.Equals(check.Address.Line1) && o.Details.Address.Line2.Equals(check.Address.Line2) &&
                                o.Details.Address.City.Equals(check.Address.City) && o.Details.Address.State.Equals(check.Address.State) && o.Details.Address.PostalCode.Equals(check.Address.PostalCode) &&
                                o.Account.RoutingNumber.Equals(decryptRout) && o.Account.AccountNumber.Equals(decrypAcct) && o.Account.Type == AccountType.Checking),
                        It.IsAny<string>(),
                        string.Empty,
                        token.Id,
                        It.IsAny<DateTime>()))
                .Returns(contactDonorNew);
            
            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            _mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey, contactDonorNew.Account.ProcessorAccountId, contactDonorNew.ProcessorId));

            var result = _fixture.CreateDonor(check);
            _donorService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(contactDonorNew, result);
        }

        [Test]
        public void TestCreateForCreateDonorWithDonorId()
        {
            var check = new CheckScannerCheck
            {
                Id = 11111,
                DonorId = 222,
                AccountNumber = "P/H+3ccB0ZssORkd+YyJzA==",
                Address = new crds_angular.Models.Crossroads.Stewardship.Address
                {
                    Line1 = "1 line 1",
                    Line2 = "1 line 2",
                    City = "1 city",
                    State = "1 state",
                    PostalCode = "1 postal"
                },
                Amount = 1111,
                CheckDate = DateTime.Now.AddHours(1),
                CheckNumber = "11111",
                Name1 = "1 name 1",
                Name2 = "1 name 2",
                RoutingNumber = "TUbiKZ/Vw1l6uyGCYIIUMg==",
                ScanDate = DateTime.Now.AddHours(2)
            };


            var contactDonorNew = new ContactDonor
            {
                ProcessorId = "222000222",
                DonorId = 222222,
                RegisteredUser = false,
                Account = new DonorAccount
                {
                    ProcessorAccountId = "py_dgsttety6737hjjhweiu3"
                }
            };
            var token = new StripeToken
            {
                Id = "12t4token"
            };
            const string encryptedKey = "PH/rty1234";
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorId(check.DonorId.Value)).Returns((ContactDonor)null);
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber)).Returns((ContactDonor)null);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.RoutingNumber)).Returns(decryptRout);
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct, decryptRout)).Returns(token);

            _donorService.Setup(
                mocked =>
                    mocked.CreateOrUpdateContactDonor(
                        It.Is<ContactDonor>(
                            o =>
                                o.Details.DisplayName.Equals(check.Name1) && o.Details.Address.Line1.Equals(check.Address.Line1) && o.Details.Address.Line2.Equals(check.Address.Line2) &&
                                o.Details.Address.City.Equals(check.Address.City) && o.Details.Address.State.Equals(check.Address.State) && o.Details.Address.PostalCode.Equals(check.Address.PostalCode) &&
                                o.Account.RoutingNumber.Equals(decryptRout) && o.Account.AccountNumber.Equals(decrypAcct) && o.Account.Type == AccountType.Checking),
                        It.IsAny<string>(),
                        string.Empty,
                        token.Id,
                        It.IsAny<DateTime>()))
                .Returns(contactDonorNew);

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            _mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey, contactDonorNew.Account.ProcessorAccountId, contactDonorNew.ProcessorId));

            var result = _fixture.CreateDonor(check);
            _donorService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(contactDonorNew, result);
        }

        [Test]
        public void TestCreateForCreateDonorAccount()
        {
            var check = new CheckScannerCheck
            {
                Id = 11111,
                DonorId = 222,
                AccountNumber = "P/H+3ccB0ZssORkd+YyJzA==",
                Address = new crds_angular.Models.Crossroads.Stewardship.Address
                {
                    Line1 = "1 line 1",
                    Line2 = "1 line 2",
                    City = "1 city",
                    State = "1 state",
                    PostalCode = "1 postal"
                },
                Amount = 1111,
                CheckDate = DateTime.Now.AddHours(1),
                CheckNumber = "11111",
                Name1 = "1 name 1",
                Name2 = "1 name 2",
                RoutingNumber = "TUbiKZ/Vw1l6uyGCYIIUMg==",
                ScanDate = DateTime.Now.AddHours(2)
            };

            var contactDonorByDonorId = new ContactDonor
            {
                ProcessorId = "333000333",
                DonorId = 333333,
                ContactId = 3333
            };

            var contactDonorNew = new ContactDonor
            {
                ProcessorId = "222000222",
                DonorId = 222222,
                RegisteredUser = false,
                Account = new DonorAccount
                {
                    ProcessorAccountId = "py_dgsttety6737hjjhweiu3"
                }
            };
            var token = new StripeToken
            {
                Id = "12t4token"
            };
            const string encryptedKey = "PH/rty1234";
            const string decrypAcct = "6015268542";
            const string decryptRout = "042000314";

            _donorService.Setup(mocked => mocked.GetContactDonorForDonorId(check.DonorId.Value)).Returns(contactDonorByDonorId);
            _donorService.Setup(mocked => mocked.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber)).Returns((ContactDonor)null);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.AccountNumber)).Returns(decrypAcct);
            _mpDonorService.Setup(mocked => mocked.DecryptCheckValue(check.RoutingNumber)).Returns(decryptRout);
            _paymentService.Setup(mocked => mocked.CreateToken(decrypAcct, decryptRout)).Returns(token);

            _donorService.Setup(
                mocked =>
                    mocked.CreateOrUpdateContactDonor(
                        It.Is<ContactDonor>(
                            o =>
                                o.Details.DisplayName.Equals(check.Name1) && o.Details.Address.Line1.Equals(check.Address.Line1) && o.Details.Address.Line2.Equals(check.Address.Line2) &&
                                o.Details.Address.City.Equals(check.Address.City) && o.Details.Address.State.Equals(check.Address.State) && o.Details.Address.PostalCode.Equals(check.Address.PostalCode) &&
                                o.Account.RoutingNumber.Equals(decryptRout) && o.Account.AccountNumber.Equals(decrypAcct) && o.Account.Type == AccountType.Checking && o.Account.Token.Equals(token.Id)),
                        It.IsAny<string>(),
                        string.Empty,
                        token.Id,
                        It.IsAny<DateTime>()))
                .Returns(contactDonorNew);

            _mpDonorService.Setup(mocked => mocked.CreateHashedAccountAndRoutingNumber(decrypAcct, decryptRout)).Returns(encryptedKey);
            _mpDonorService.Setup(mocked => mocked.UpdateDonorAccount(encryptedKey, contactDonorNew.Account.ProcessorAccountId, contactDonorNew.ProcessorId));

            var result = _fixture.CreateDonor(check);
            _donorService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(contactDonorNew, result);
        }

    }
}
