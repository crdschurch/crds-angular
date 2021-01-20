using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Extensions;
using Crossroads.Utilities.Services;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using IDonorRepository = MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository;

namespace crds_angular.test.Services
{
    public class DonorServiceTest
    {
        private DonorService _fixture;

        private Mock<IDonorRepository> _mpDonorService;
        private Mock<IContactRepository> _mpContactService;
        private Mock<IPaymentProcessorService> _paymentService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IPledgeRepository> _pledgeService; 
        private Mock<IUserRepository> _userRepositoryMock; 
        private Mock<IApiUserRepository> _apiUserRepositoryMock; 
        private const string GuestGiverDisplayName = "Guest Giver";

        private const int StatementFrequencyNever = 9;
        private const int StatementFrequencyQuarterly = 99;
        private const int StatementTypeIndividual = 8;
        private const int StatementMethodNone = 7;
        private const int StatementMethodPostalMail = 77;
        private const int NotSiteSpecificCongregation = 88;
        private const string EncryptedKey =  "S/Hhsdgsydgs67733+jjdhjsdbnv332387uhrjfk";

        private const int RecurringGiftSetupEmailTemplateId = 9000;
        private const int RecurringGiftUpdateEmailTemplateId = 9001;
        private const int RecurringGiftCancelEmailTemplateId = 9002;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _mpDonorService = new Mock<IDonorRepository>(MockBehavior.Strict);
            _mpContactService = new Mock<IContactRepository>(MockBehavior.Strict);
            _paymentService = new Mock<IPaymentProcessorService>(MockBehavior.Strict);
            _authenticationService = new Mock<IAuthenticationRepository>(MockBehavior.Strict);
            _pledgeService = new Mock<IPledgeRepository>(MockBehavior.Strict);
            _userRepositoryMock = new Mock<IUserRepository>();
            _apiUserRepositoryMock = new Mock<IApiUserRepository>();

            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementFrequencyNever")).Returns(StatementFrequencyNever);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementFrequencyQuarterly")).Returns(StatementFrequencyQuarterly);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementTypeIndividual")).Returns(StatementTypeIndividual);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementMethodNone")).Returns(StatementMethodNone);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("DonorStatementMethodPostalMail")).Returns(StatementMethodPostalMail);
            _configurationWrapper.Setup(mocked => mocked.GetConfigValue("GuestGiverContactDisplayName")).Returns(GuestGiverDisplayName);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("NotSiteSpecificCongregation")).Returns(NotSiteSpecificCongregation);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("RecurringGiftSetupEmailTemplateId")).Returns(RecurringGiftSetupEmailTemplateId);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("RecurringGiftUpdateEmailTemplateId")).Returns(RecurringGiftUpdateEmailTemplateId);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("RecurringGiftCancelEmailTemplateId")).Returns(RecurringGiftCancelEmailTemplateId);

            _fixture = new DonorService(_mpDonorService.Object, _mpContactService.Object, _paymentService.Object, _configurationWrapper.Object, _pledgeService.Object, _userRepositoryMock.Object);

        }

        [Test]
        public void ShouldGetDonorForEmail()
        {
            var donor = new MpContactDonor();
            _mpDonorService.Setup(mocked => mocked.GetPossibleGuestContactDonor("me@here.com")).Returns(donor);
            var response = _fixture.GetContactDonorForEmail("me@here.com");

            _mpDonorService.VerifyAll();
            Assert.AreSame(donor, response);
            Assert.IsFalse(response.RegisteredUser);
        }

        [Test]
        public void ShouldGetDonorForAuthenticatedUser()
        {
            var donor = new MpContactDonor();
            _mpContactService.Setup(mocked => mocked.GetContactId("authToken")).Returns(123);
            _mpDonorService.Setup(mocked => mocked.GetContactDonor(123)).Returns(donor);
            var response = _fixture.GetContactDonorForAuthenticatedUser("authToken");

            _mpContactService.VerifyAll();
            _mpDonorService.VerifyAll();

            Assert.AreSame(donor, response);
        }

        [Test]
        public void ShouldReturnExistingDonorWithExistingStripeId()
        {
            var donor = new MpContactDonor
            {
                ContactId = 12345,
                DonorId = 67890,
                ProcessorId = "Processor_ID",
                RegisteredUser = true,
                Email = "me@here.com"
            };

            var response = _fixture.CreateOrUpdateContactDonor(donor, EncryptedKey, string.Empty, string.Empty, "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(donor.ContactId, response.ContactId);
            Assert.AreEqual(donor.DonorId, response.DonorId);
            Assert.AreEqual(donor.ProcessorId, response.ProcessorId);
            Assert.AreEqual(donor.RegisteredUser, response.RegisteredUser);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void ShouldCreateNewContactAndDonor()
        {
            var stripeCust = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };
            
           
            _mpContactService.Setup(mocked => mocked.CreateContactForGuestGiver("me@here.com", GuestGiverDisplayName, It.IsAny<string>(), It.IsAny<string>())).Returns(123);
            _paymentService.Setup(mocked => mocked.CreateCustomer("stripe_token", It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>() )).Returns(stripeCust); 
            _mpDonorService.Setup(mocked => mocked.CreateDonorRecord(123, stripeCust.id, It.IsAny<DateTime>(), StatementFrequencyNever, StatementTypeIndividual, StatementMethodNone, null)).Returns(456);
            _paymentService.Setup(mocked => mocked.UpdateCustomerDescription(stripeCust.id, 456)).Returns("456");

            var response = _fixture.CreateOrUpdateContactDonor(null, EncryptedKey, It.IsAny<string>(), It.IsAny<string>(), "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(123, response.ContactId);
            Assert.AreEqual(456, response.DonorId);
            Assert.AreEqual(stripeCust.id, response.ProcessorId);
            Assert.IsFalse(response.RegisteredUser);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void ShouldCreateNewDonorForExistingContact()
        {
            var donor = new MpContactDonor
            {
                ContactId = 12345,
                DonorId = 0,
                Email = "me@here.com",
                Details = new MpContactDetails
                {
                    DisplayName = "Bart Simpson"
                }
            };

            var stripeCust = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };

            _paymentService.Setup(mocked => mocked.CreateCustomer("stripe_token", It.IsAny<string>(), "me@here.com", "Bart Simpson")).Returns(stripeCust);
            _mpDonorService.Setup(mocked => mocked.CreateDonorRecord(12345, stripeCust.id, It.IsAny<DateTime>(), StatementFrequencyNever, StatementTypeIndividual, StatementMethodNone, null)).Returns(456);
            _paymentService.Setup(mocked => mocked.UpdateCustomerDescription(stripeCust.id, 456)).Returns("456");

            var response = _fixture.CreateOrUpdateContactDonor(donor, EncryptedKey, It.IsAny<string>(), It.IsAny<string>(),  "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(12345, response.ContactId);
            Assert.AreEqual(456, response.DonorId);
            Assert.AreEqual(stripeCust.id, response.ProcessorId);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void ShouldCreateNewDonorForExistingRegisteredContact()
        {
            var donor = new MpContactDonor
            {
                ContactId = 12345,
                DonorId = 0,
                RegisteredUser = true,
                Email = "me@here.com",
                Details = new MpContactDetails
                {
                    DisplayName = "Bart Simpson"
                }

            };

            var stripeCust = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };


            _paymentService.Setup(mocked => mocked.CreateCustomer("stripe_token", string.Empty, "me@here.com", "Bart Simpson")).Returns(stripeCust);
            _mpDonorService.Setup(mocked => mocked.CreateDonorRecord(12345, stripeCust.id, It.IsAny<DateTime>(), 1, 1, 2, null)).Returns(456);
            _mpDonorService.Setup(mocked => mocked.GetEmailViaDonorId(456)).Returns(donor);
            _paymentService.Setup(mocked => mocked.UpdateCustomerDescription(stripeCust.id, 456)).Returns("456");

            var response = _fixture.CreateOrUpdateContactDonor(donor, EncryptedKey, It.IsAny<string>(), It.IsAny<string>(), "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(12345, response.ContactId);
            Assert.AreEqual(456, response.DonorId);
            Assert.AreEqual(stripeCust.id, response.ProcessorId);
            Assert.AreEqual(donor.RegisteredUser, response.RegisteredUser);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void ShouldUpdateExistingDonorForExistingContact()
        {
            var donor = new MpContactDonor
            {
                ContactId = 12345,
                DonorId = 456,
                Email = "me@here.com",
                Details = new MpContactDetails
                {
                    DisplayName = "Bart Simpson"
                }
            };

            var stripeCust = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };

           
            _paymentService.Setup(mocked => mocked.CreateCustomer("stripe_token", It.IsAny<string>(), "me@here.com", "Bart Simpson")).Returns(stripeCust);
            _mpDonorService.Setup(mocked => mocked.UpdatePaymentProcessorCustomerId(456, stripeCust.id)).Returns(456);
            _paymentService.Setup(mocked => mocked.UpdateCustomerDescription(stripeCust.id, 456)).Returns("456");

            var response = _fixture.CreateOrUpdateContactDonor(donor, EncryptedKey, It.IsAny<string>(), It.IsAny<string>(), "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(12345, response.ContactId);
            Assert.AreEqual(456, response.DonorId);
            Assert.AreEqual(stripeCust.id, response.ProcessorId);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void ShouldAddNewDonorAccountToExistingDonor()
        {
            var donor = new MpContactDonor
            {
                ContactId = 12345,
                DonorId = 456,
                Email = "me@here.com",
                Account = new MpDonorAccount
                {
                    AccountNumber = "123456789",
                    EncryptedAccount = "enc12345",
                    RoutingNumber = "110000000",
                    Token = "stripe_token",
                    Type = AccountType.Checking
                },
                ProcessorId = "cus_90210"
            };

            var stripeSource = new StripeCustomer
            {
                id = "src_123"
            };

            _paymentService.Setup(mocked => mocked.AddSourceToCustomer("cus_90210", "stripe_token")).Returns(stripeSource);
            _mpDonorService.Setup(
                mocked => mocked.CreateDonorAccount(null, donor.Account.RoutingNumber, donor.Account.AccountNumber.Right(4), "enc12345", 456, "src_123", "cus_90210")).Returns(987);

            var response = _fixture.CreateOrUpdateContactDonor(donor, EncryptedKey, string.Empty, string.Empty, "me@here.com", "stripe_token", DateTime.Now);

            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.AreEqual(12345, response.ContactId);
            Assert.AreEqual(456, response.DonorId);
            Assert.AreEqual(donor.ProcessorId, response.ProcessorId);
            Assert.AreEqual("me@here.com", response.Email);
        }

        [Test]
        public void TestCreateRecurringGift()
        {
            var recurringGiftDto = new RecurringGiftDto
            {
                StripeTokenId = "tok_123",
                PlanAmount = 123.45M,
                PlanInterval = PlanInterval.Weekly,
                Program = "987",
                StartDate = DateTime.Parse("1973-10-15")
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 678,
                ProcessorId = "cus_123",
                ContactId = 909090
            };

            var defaultSource = new SourceData
            {
                id = "card_123",
                brand = "Visa",
                last4 = "5150"
            };

            var stripeCustomer = new StripeCustomer
            {
                brand = "visa",
                last4 = "9876",
                id = "cus_123",
                default_source = "card_123",
                sources = new Sources
                {
                    data = new List<SourceData>
                    {
                        new SourceData
                        {
                            id = "bank_123",
                            bank_last4 = "5678"
                        },
                        defaultSource
                    }
                }
            };

            var stripePlan = new StripePlan
            {
                Id = "plan_123"
            };

            const int donorAccountId = 999;

            var stripeSubscription = new StripeSubscription
            {
                Id = "sub_123"
            };

            var contact = new MpMyContact()
            {
                Congregation_ID = 1
            };
            const int recurringGiftId = 888;

            var recurringGift = new MpCreateDonationDistDto
            {
                ProgramName = "Crossroads",
                Amount = 123.45M,
                Recurrence = "12th of the month",
                DonorAccountId = 90,
                StartDate = DateTime.Now
            };

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns("token");
            _paymentService.Setup(mocked => mocked.CreateCustomer(recurringGiftDto.StripeTokenId, "678, Recurring Gift Subscription", "bart_simpson@crossroads.net", "Bart Simpson")).Returns(stripeCustomer);
            _paymentService.Setup(mocked => mocked.CreatePlan(recurringGiftDto, contactDonor, "bart_simpson@crossroads.net", "Bart Simpson")).Returns(stripePlan);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonorAccount(defaultSource.brand,
                                              It.IsAny<string>(),
                                              defaultSource.last4,
                                              null,
                                              contactDonor.DonorId,
                                              defaultSource.id,
                                              stripeCustomer.id)).Returns(donorAccountId);
            _paymentService.Setup(mocked => mocked.CreateSubscription(stripePlan.Id, stripeCustomer.id, recurringGiftDto.StartDate, "bart_simpson@crossroads.net", "Bart Simpson")).Returns(stripeSubscription);
            _mpContactService.Setup(mocked => mocked.GetContactById(contactDonor.ContactId)).Returns(contact);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateRecurringGiftRecord(contactDonor.DonorId,
                                                     donorAccountId,
                                                     EnumMemberSerializationUtils.ToEnumString(recurringGiftDto.PlanInterval),
                                                     recurringGiftDto.PlanAmount,
                                                     recurringGiftDto.StartDate,
                                                     recurringGiftDto.Program,
                                                     stripeSubscription.Id, contact.Congregation_ID.Value, 
                                                     null, 
                                                     null)).Returns(recurringGiftId);

            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(recurringGiftId)).Returns(recurringGift);
            _mpDonorService.Setup(mocked => mocked.GetDonorAccountPymtType(recurringGift.DonorAccountId.Value)).Returns(1);
            _mpDonorService.Setup(
                mocked =>
                    mocked.SendEmail(RecurringGiftSetupEmailTemplateId, recurringGift.DonorId, (int)(123.45M/100), "Bank", It.IsAny<DateTime>(), recurringGift.StartDate.Value, "Crossroads", string.Empty, "12th of the month", null));

            var response = _fixture.CreateRecurringGift(recurringGiftDto, contactDonor, "bart_simpson@crossroads.net", "Bart Simpson");
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
            Assert.AreEqual(recurringGiftId, response);
        }

        [Test]
        public void TestCreateRecurringGiftWholeLottaFailures()
        {
            var recurringGiftDto = new RecurringGiftDto
            {
                StripeTokenId = "tok_123",
                PlanAmount = 123.45M,
                PlanInterval = PlanInterval.Weekly,
                Program = "987",
                StartDate = DateTime.Parse("1973-10-15")
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 678,
                ProcessorId = "cus_123",
                ContactId = 909090
            };

            var defaultSource = new SourceData
            {
                id = "card_123",
                brand = "Visa",
                last4 = "5150"
            };

            var stripeCustomer = new StripeCustomer
            {
                brand = "visa",
                last4 = "9876",
                id = "cus_123",
                default_source = "card_123",
                sources = new Sources
                {
                    data = new List<SourceData>
                    {
                        new SourceData
                        {
                            id = "bank_123",
                            bank_last4 = "5678"
                        },
                        defaultSource
                    }
                }
            };

            var stripePlan = new StripePlan
            {
                Id = "plan_123"
            };

            const int donorAccountId = 999;

            var stripeSubscription = new StripeSubscription
            {
                Id = "sub_123"
            };

            var contact = new MpMyContact()
            {
                Congregation_ID = 1
            };

            _paymentService.Setup(mocked => mocked.CreateCustomer(recurringGiftDto.StripeTokenId, "678, Recurring Gift Subscription", "me@here.com", "Bart Simpson")).Returns(stripeCustomer);
            _paymentService.Setup(mocked => mocked.CreatePlan(recurringGiftDto, contactDonor, "me@here.com", "Bart Simpson")).Returns(stripePlan);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonorAccount(defaultSource.brand,
                                              It.IsAny<string>(),
                                              defaultSource.last4,
                                              null,
                                              contactDonor.DonorId,
                                              defaultSource.id,
                                              stripeCustomer.id)).Returns(donorAccountId);
            _paymentService.Setup(mocked => mocked.CreateSubscription(stripePlan.Id, stripeCustomer.id, recurringGiftDto.StartDate, "me@here.com", "Bart Simpson")).Returns(stripeSubscription);
            _mpContactService.Setup(mocked => mocked.GetContactById(contactDonor.ContactId)).Returns(contact);
            var exception = new ApplicationException("Do it to it Lars");
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateRecurringGiftRecord(contactDonor.DonorId,
                                                     donorAccountId,
                                                     EnumMemberSerializationUtils.ToEnumString(recurringGiftDto.PlanInterval),
                                                     recurringGiftDto.PlanAmount,
                                                     recurringGiftDto.StartDate,
                                                     recurringGiftDto.Program,
                                                     stripeSubscription.Id,
                                                     contact.Congregation_ID.Value, 
                                                     null, 
                                                     null)).Throws(exception);

            _paymentService.Setup(mocked => mocked.CancelSubscription(stripeCustomer.id, stripeSubscription.Id)).Throws(new Exception());
            _paymentService.Setup(mocked => mocked.CancelPlan(stripePlan.Id)).Throws(new Exception());
            _paymentService.Setup(mocked => mocked.DeleteCustomer(stripeCustomer.id)).Throws(new Exception());
            _mpDonorService.Setup(mocked => mocked.DeleteDonorAccount(donorAccountId)).Throws(new Exception());

            try
            {
                _fixture.CreateRecurringGift(recurringGiftDto, contactDonor, "me@here.com", "Bart Simpson");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ApplicationException e)
            {
                Assert.AreSame(exception, e);
            }
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
        }

        public void TestCreateRecurringGiftNoCongregation()
        {
            var recurringGiftDto = new RecurringGiftDto
            {
                StripeTokenId = "tok_123",
                PlanAmount = 123.45M,
                PlanInterval = PlanInterval.Weekly,
                Program = "987",
                StartDate = DateTime.Parse("1973-10-15")
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 678,
                ProcessorId = "cus_123"
            };

            var stripeCustomer = new StripeCustomer
            {
                brand = "visa",
                last4 = "9876",
                id = "card_123"
            };

            var stripePlan = new StripePlan
            {
                Id = "plan_123"
            };

            const int donorAccountId = 999;

            var stripeSubscription = new StripeSubscription
            {
                Id = "sub_123"
            };

            var contact = new MpMyContact()
            {
                Congregation_ID = null
            };

           
            const int recurringGiftId = 888;

            _paymentService.Setup(mocked => mocked.AddSourceToCustomer(contactDonor.ProcessorId, recurringGiftDto.StripeTokenId)).Returns(stripeCustomer);
            _paymentService.Setup(mocked => mocked.CreatePlan(recurringGiftDto, contactDonor, "me@here.com", "Bart Simpson")).Returns(stripePlan);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateDonorAccount(stripeCustomer.brand,
                                              It.IsAny<string>(),
                                              stripeCustomer.last4,
                                              null,
                                              contactDonor.DonorId,
                                              stripeCustomer.id,
                                              contactDonor.ProcessorId)).Returns(donorAccountId);
            _paymentService.Setup(mocked => mocked.CreateSubscription(stripePlan.Id, contactDonor.ProcessorId, recurringGiftDto.StartDate, "me@here.com", "Bart Simpson")).Returns(stripeSubscription);
            _mpContactService.Setup(mocked => mocked.GetContactById(contactDonor.DonorId)).Returns(contact);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateRecurringGiftRecord(contactDonor.DonorId,
                                                     donorAccountId,
                                                     EnumMemberSerializationUtils.ToEnumString(recurringGiftDto.PlanInterval),
                                                     recurringGiftDto.PlanAmount,
                                                     recurringGiftDto.StartDate,
                                                     recurringGiftDto.Program,
                                                     stripeSubscription.Id, NotSiteSpecificCongregation, 
                                                     null, 
                                                     null)).Returns(recurringGiftId);
            var response = _fixture.CreateRecurringGift(recurringGiftDto, contactDonor, "me@here.com", "Bart Simpson");
            _paymentService.VerifyAll();
            _mpDonorService.VerifyAll();
            _mpContactService.VerifyAll();
            Assert.AreEqual(recurringGiftId, response);
        }

        [Test]
        public void TestGetRecurringGiftsForAuthenticatedUser()
        {
            var records = new List<MpRecurringGift>
            {
                new MpRecurringGift
                {
                    RecurringGiftId = 123,
                    DonorID = 123123,
                    EmailAddress = "test@example.com",
                    Frequency = "Weekly",
                    Recurrence = "Fridays Weekly",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    Amount = 950M,
                    ProgramID = 2,
                    ProgramName = "Beans & Rice",
                    CongregationName = "Uptown",
                    AccountTypeID = 3,
                    AccountNumberLast4= "4433",
                    InstitutionName = "Visa",
                    SubscriptionID = "sub_77L7hDGjQdoxRE",
                    ProcessorId = "cus_123",
                    ProcessorAccountId = "card_123"
                },
                new MpRecurringGift
                {
                    RecurringGiftId = 124,
                    DonorID = 123123,
                    EmailAddress = "test@example.com",
                    Frequency = "Monthly",
                    Recurrence = "8th Monthly",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    Amount = 190M,
                    ProgramID = 1,
                    ProgramName = "Crossroads",
                    CongregationName = "",
                    AccountTypeID = 1,
                    AccountNumberLast4= "4093",
                    InstitutionName = "Bank",
                    SubscriptionID = "sub_77uaEIZLssR6xN",
                    ProcessorId = "cus_456",
                    ProcessorAccountId = "ba_456"
                },
                new MpRecurringGift
                {
                    RecurringGiftId = 125,
                    DonorID = 123123,
                    EmailAddress = "test@example.com",
                    Frequency = "Weekly",
                    Recurrence = "Tuesdays Weekly",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    Amount = 50M,
                    ProgramID = 3,
                    ProgramName = "Old St. George",
                    CongregationName = "General",
                    AccountTypeID = 3,
                    AccountNumberLast4= "1984",
                    InstitutionName = "American Express",
                    SubscriptionID = "sub_77L8qFUF6QFZsO",
                    ProcessorId = "cus_789",
                    ProcessorAccountId = "card_789"
                },
            };

            var sourceData = new []
            {
                new SourceData
                {
                    address_zip = "12345",
                    exp_month = "8",
                    exp_year = "2012"
                },
                new SourceData
                {
                    routing_number = "123456789"
                },
                new SourceData
                {
                    address_zip = "67890",
                    exp_month = "12",
                    exp_year = "2013"
                }
            };

            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftsForAuthenticatedUser(It.IsAny<string>())).Returns(records);
            _paymentService.Setup(mocked => mocked.GetSource(records[0].ProcessorId, records[0].ProcessorAccountId)).Returns(sourceData[0]);
            _paymentService.Setup(mocked => mocked.GetSource(records[1].ProcessorId, records[1].ProcessorAccountId)).Returns(sourceData[1]);
            _paymentService.Setup(mocked => mocked.GetSource(records[2].ProcessorId, records[2].ProcessorAccountId)).Returns(sourceData[2]);

            var result = _fixture.GetRecurringGiftsForAuthenticatedUser("afdafsaaatewjrtjeretewtr");

            _mpDonorService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(records[0].RecurringGiftId, result[0].RecurringGiftId);
            Assert.AreEqual(records[0].DonorID, result[0].DonorID);
            Assert.AreEqual(records[0].EmailAddress, result[0].EmailAddress);
            Assert.AreEqual(Regex.Match(records[0].Frequency, "^.*Weekly").Success ? PlanInterval.Weekly : PlanInterval.Monthly, result[0].PlanInterval);
            Assert.AreEqual(records[0].Recurrence, result[0].Recurrence);
            Assert.AreEqual(records[0].StartDate, result[0].StartDate);
            Assert.AreEqual(records[0].EndDate, result[0].EndDate);
            Assert.AreEqual(records[0].Amount, result[0].PlanAmount);
            Assert.AreEqual(records[0].ProgramID.ToString(), result[0].Program);
            Assert.AreEqual(records[0].ProgramName, result[0].ProgramName);
            Assert.AreEqual(records[0].CongregationName, result[0].CongregationName);
            Assert.AreEqual(PaymentType.CreditCard, result[0].Source.SourceType);
            Assert.AreEqual(records[0].AccountNumberLast4, result[0].Source.AccountNumberLast4);
            Assert.AreEqual(CreditCardType.Visa, result[0].Source.CardType);
            Assert.AreEqual(records[0].SubscriptionID, result[0].SubscriptionID);
            Assert.AreEqual(sourceData[0].address_zip, result[0].Source.PostalCode);
            Assert.AreEqual(DateTime.ParseExact(string.Format("{0}/01/{1}", sourceData[0].exp_month, sourceData[0].exp_year), "M/dd/yyyy", DateTimeFormatInfo.CurrentInfo), result[0].Source.ExpirationDate);

            Assert.AreEqual(records[1].RecurringGiftId, result[1].RecurringGiftId);
            Assert.AreEqual(records[1].DonorID, result[1].DonorID);
            Assert.AreEqual(records[1].EmailAddress, result[1].EmailAddress);
            Assert.AreEqual(Regex.Match(records[1].Frequency, "^.*Weekly").Success ? PlanInterval.Weekly : PlanInterval.Monthly, result[1].PlanInterval);
            Assert.AreEqual(records[1].Recurrence, result[1].Recurrence);
            Assert.AreEqual(records[1].StartDate, result[1].StartDate);
            Assert.AreEqual(records[1].EndDate, result[1].EndDate);
            Assert.AreEqual(records[1].Amount, result[1].PlanAmount);
            Assert.AreEqual(records[1].ProgramID.ToString(), result[1].Program);
            Assert.AreEqual(records[1].ProgramName, result[1].ProgramName);
            Assert.AreEqual(records[1].CongregationName, result[1].CongregationName);
            Assert.AreEqual(PaymentType.Bank, result[1].Source.SourceType);
            Assert.AreEqual(records[1].AccountNumberLast4, result[1].Source.AccountNumberLast4);
            Assert.AreEqual(null, result[1].Source.CardType);
            Assert.AreEqual(records[1].SubscriptionID, result[1].SubscriptionID);
            Assert.AreEqual(sourceData[1].routing_number, result[1].Source.RoutingNumber);

            Assert.AreEqual(records[2].RecurringGiftId, result[2].RecurringGiftId);
            Assert.AreEqual(records[2].DonorID, result[2].DonorID);
            Assert.AreEqual(records[2].EmailAddress, result[2].EmailAddress);
            Assert.AreEqual(Regex.Match(records[2].Frequency, "^.*Weekly").Success ? PlanInterval.Weekly : PlanInterval.Monthly, result[2].PlanInterval);
            Assert.AreEqual(records[2].Recurrence, result[2].Recurrence);
            Assert.AreEqual(records[2].StartDate, result[2].StartDate);
            Assert.AreEqual(records[2].EndDate, result[2].EndDate);
            Assert.AreEqual(records[2].Amount, result[2].PlanAmount);
            Assert.AreEqual(records[2].ProgramID.ToString(), result[2].Program);
            Assert.AreEqual(records[2].ProgramName, result[2].ProgramName);
            Assert.AreEqual(records[2].CongregationName, result[2].CongregationName);
            Assert.AreEqual(PaymentType.CreditCard, result[2].Source.SourceType);
            Assert.AreEqual(records[2].AccountNumberLast4, result[2].Source.AccountNumberLast4);
            Assert.AreEqual(CreditCardType.AmericanExpress, result[2].Source.CardType);
            Assert.AreEqual(records[2].SubscriptionID, result[2].SubscriptionID);
            Assert.AreEqual(sourceData[2].address_zip, result[2].Source.PostalCode);
            Assert.AreEqual(DateTime.ParseExact(string.Format("{0}/01/{1}", sourceData[2].exp_month, sourceData[2].exp_year), "M/dd/yyyy", DateTimeFormatInfo.CurrentInfo), result[2].Source.ExpirationDate);
        }

        [Test]
        public void TestCancelRecurringGift()
        {
            const string authUserToken = "auth";
            const int recurringGiftId = 123;
            const bool sendEmail = true;
            var gift = new MpCreateDonationDistDto
            {
                DonorId = 456,
                SubscriptionId = "sub_123",
                StripeCustomerId = "cus_456",
                StripeAccountId = "card_789",
                ProgramName = "Crossroads",
                Amount = 123.45M,
                Recurrence = "12th of the month",
                DonorAccountId = 90,
                StartDate = DateTime.Now
            };

            var plan = new StripePlan
            {
                Id = "plan_123"
            };

            var subscription = new StripeSubscription
            {
                Plan = plan
            };

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(authUserToken);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(recurringGiftId)).Returns(gift);
            _paymentService.Setup(mocked => mocked.CancelSubscription(gift.StripeCustomerId, gift.SubscriptionId)).Returns(subscription);
            _paymentService.Setup(mocked => mocked.CancelPlan(subscription.Plan.Id)).Returns(plan);
            _mpDonorService.Setup(mocked => mocked.CancelRecurringGift(recurringGiftId));

            _mpDonorService.Setup(mocked => mocked.GetDonorAccountPymtType(gift.DonorAccountId.Value)).Returns(3);
            _mpDonorService.Setup(
                mocked =>
                    mocked.SendEmail(RecurringGiftCancelEmailTemplateId, gift.DonorId, (int)(123.45M / 100), "Credit Card", It.IsAny<DateTime>(), gift.StartDate.Value, "Crossroads", string.Empty, "12th of the month", null));

            _fixture.CancelRecurringGift(recurringGiftId, sendEmail);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestEditRecurringGiftNoEdits()
        {
            const string authUserToken = "auth";
            var today = DateTime.Today;

            var editGift = new RecurringGiftDto
            {
                RecurringGiftId = 345,
                StripeTokenId = string.Empty,
                PlanAmount = 500M,
                Program = "3",
                PlanInterval = PlanInterval.Weekly,
                StartDate = today
            };

            var donor = new MpContactDonor
            {
                DonorId = 456
            };

            var existingGift = new MpCreateDonationDistDto
            {
                Amount = 50000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today,
                DonorAccountId = 234,
                SubscriptionId = "sub_123",
                DayOfWeek = (int)today.DayOfWeek,
                RecurringGiftId = 345
            };

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(authUserToken);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(editGift.RecurringGiftId)).Returns(existingGift);

            var result = _fixture.EditRecurringGift(editGift, donor);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.IsNotNull(result);
        }

        [Test]
        public void TestEditRecurringGiftOnlyChangePayment()
        {
            const string authUserToken = "auth";
            var today = DateTime.Today;

            var editGift = new RecurringGiftDto
            {
                RecurringGiftId = 345,
                StripeTokenId = "tok_123",
                PlanAmount = 500M,
                Program = "3",
                PlanInterval = PlanInterval.Weekly,
                StartDate = today
            };

            var donor = new MpContactDonor
            {
                DonorId = 456,
                ProcessorId = "cus_123"
            };

            var existingGift = new MpCreateDonationDistDto
            {
                Amount = 50000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today,
                DonorAccountId = 234,
                SubscriptionId = "sub_123",
                DayOfWeek = (int)today.DayOfWeek,
                RecurringGiftId = 345,
                DonorId = 789,
                StripeCustomerId = "cus_456",
                StripeAccountId = "card_456"
            };

            var stripeSource = new SourceData
            {
                id = "bank_1234",
                bank_last4 = "5678"
            };


            const int newDonorAccountId = 987;

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(authUserToken);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(editGift.RecurringGiftId)).Returns(existingGift);
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(existingGift.StripeCustomerId, editGift.StripeTokenId)).Returns(stripeSource);
            _mpDonorService.Setup(mocked => mocked.CreateDonorAccount(null, "0", stripeSource.bank_last4, null, existingGift.DonorId, stripeSource.id, existingGift.StripeCustomerId)).Returns(newDonorAccountId);
            _mpDonorService.Setup(mocked => mocked.UpdateRecurringGiftDonorAccount(existingGift.RecurringGiftId.Value, newDonorAccountId));

            var result = _fixture.EditRecurringGift(editGift, donor);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.IsNotNull(result);
        }

        [Test]
        public void TestEditRecurringGiftChangePaymentAndAmount()
        {
            const string authUserToken = "auth";
            var today = DateTime.Today;
            const int congregationId = 1;

            var editGift = new RecurringGiftDto
            {
                RecurringGiftId = 345,
                StripeTokenId = "tok_123",
                PlanAmount = 800M,
                Program = "3",
                PlanInterval = PlanInterval.Weekly,
                StartDate = today
            };

            var donor = new MpContactDonor
            {
                DonorId = 456,
                ProcessorId = "cus_123",
                Email = "me@here.com",
                Details = new MpContactDetails
                {
                    EmailAddress = "me@here.com",
                    DisplayName = "Bart Simpson"
                }
            };

            var existingGift = new MpCreateDonationDistDto
            {
                Amount = 50000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today,
                DonorAccountId = 234,
                SubscriptionId = "sub_123",
                DayOfWeek = (int)today.DayOfWeek,
                RecurringGiftId = 345,
                DonorId = 789,
                StripeCustomerId = "cus_456",
                StripeAccountId = "card_456"
            };

            var stripeSource = new SourceData
            {
                brand = "Visa",
                last4 = "1234",
                id = "card_123"
            };

            const int newDonorAccountId = 987;

            DateTime? trialEndDate = DateTime.Now.AddDays(3);
            var oldSubscription = new StripeSubscription
            {
                Id = "sub_123",
                Plan = new StripePlan
                {
                    Id = "plan_123"
                },
                TrialEnd = trialEndDate
            };

            var newPlan = new StripePlan
            {
                Id = "plan_456"
            };

            var newSubscription = new StripeSubscription
            {
                Id = "sub_123"
            };

            const int newRecurringGiftId = 765;

            var newRecurringGift = new MpCreateDonationDistDto
            {
                Amount = 80000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today,
                DonorAccountId = 234,
                SubscriptionId = "sub_456",
                DayOfWeek = (int)today.DayOfWeek,
                RecurringGiftId = newRecurringGiftId,
                DonorId = 789,
                ProgramName = "Crossroads",
                Recurrence = "12th of the month",
            };

            var contact = new MpMyContact()
            {
                Congregation_ID = congregationId
            };


            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(authUserToken);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(editGift.RecurringGiftId)).Returns(existingGift);
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(existingGift.StripeCustomerId, editGift.StripeTokenId)).Returns(stripeSource);
            _mpDonorService.Setup(mocked => mocked.CreateDonorAccount(stripeSource.brand, "0", stripeSource.last4, null, existingGift.DonorId, stripeSource.id, existingGift.StripeCustomerId)).Returns(newDonorAccountId);
            _paymentService.Setup(mocked => mocked.GetSubscription(existingGift.StripeCustomerId, existingGift.SubscriptionId)).Returns(oldSubscription);
            _paymentService.Setup(mocked => mocked.UpdateSubscriptionPlan(existingGift.StripeCustomerId, existingGift.SubscriptionId, newPlan.Id, trialEndDate)).Returns(oldSubscription);
            _paymentService.Setup(mocked => mocked.CancelPlan(oldSubscription.Plan.Id)).Returns(oldSubscription.Plan);
            _paymentService.Setup(mocked => mocked.CreatePlan(editGift, donor, "me@here.com", "Bart Simpson")).Returns(newPlan);
            _mpDonorService.Setup(mocked => mocked.CancelRecurringGift(existingGift.RecurringGiftId.Value));
            _mpContactService.Setup(mocked => mocked.GetContactById(donor.ContactId)).Returns(contact);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateRecurringGiftRecord(donor.DonorId,
                                                     newDonorAccountId,
                                                     EnumMemberSerializationUtils.ToEnumString(editGift.PlanInterval),
                                                     editGift.PlanAmount,
                                                     editGift.StartDate,
                                                     editGift.Program,
                                                     newSubscription.Id, contact.Congregation_ID.Value, 
                                                     null, 
                                                     null)).Returns(newRecurringGiftId);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(newRecurringGiftId)).Returns(newRecurringGift);

            _mpDonorService.Setup(mocked => mocked.GetDonorAccountPymtType(234)).Returns(1);
            _mpDonorService.Setup(
                mocked =>
                    mocked.SendEmail(RecurringGiftUpdateEmailTemplateId, newRecurringGift.DonorId, (int)(80000M / 100), "Bank", It.IsAny<DateTime>(), It.IsAny<DateTime>(), "Crossroads", string.Empty, "12th of the month", null));

            var result = _fixture.EditRecurringGift(editGift, donor);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(newRecurringGift.RecurringGiftId, result.RecurringGiftId);
            Assert.AreEqual(newRecurringGift.StartDate, result.StartDate);
            Assert.AreEqual(newRecurringGift.Amount, result.PlanAmount);
            Assert.AreEqual(PlanInterval.Weekly, result.PlanInterval);
            Assert.AreEqual(newRecurringGift.ProgramId, result.Program);
            Assert.AreEqual(newRecurringGift.DonorId, result.DonorID);
            Assert.AreEqual(donor.Email, result.EmailAddress);
            Assert.AreEqual(newSubscription.Id, result.SubscriptionID);
        }

        [Test]
        public void TestEditRecurringGiftChangePaymentAndStartDate()
        {
            const string authUserToken = "auth";
            var today = DateTime.Today;
            const int congregationId = 1;

            var editGift = new RecurringGiftDto
            {
                RecurringGiftId = 345,
                StripeTokenId = "tok_123",
                PlanAmount = 800M,
                Program = "3",
                PlanInterval = PlanInterval.Weekly,
                StartDate = today
            };

            var donor = new MpContactDonor
            {
                DonorId = 456,
                ProcessorId = "cus_123",
                Email = "me@here.com",
                Details = new MpContactDetails
                {
                    EmailAddress = "me@here.com",
                    DisplayName = "Bart Simpson"
                }
            };

            var existingGift = new MpCreateDonationDistDto
            {
                Amount = 50000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today.AddDays(-7),
                DonorAccountId = 234,
                SubscriptionId = "sub_123",
                DayOfWeek = (int)today.AddDays(-7).DayOfWeek,
                RecurringGiftId = 345,
                DonorId = 789,
                StripeCustomerId = "cus_456",
                StripeAccountId = "card_456"
            };

            var stripeSource = new SourceData
            {
                brand = "Visa",
                last4 = "1234",
                id = "card_123"
            };

            const int newDonorAccountId = 987;

            var oldSubscription = new StripeSubscription
            {
                Id = "sub_123",
                Plan = new StripePlan
                {
                    Id = "plan_123"
                }
            };

            var newPlan = new StripePlan
            {
                Id = "plan_456"
            };

            var newSubscription = new StripeSubscription
            {
                Id = "sub_456"
            };

            const int newRecurringGiftId = 765;

            var newRecurringGift = new MpCreateDonationDistDto
            {
                Amount = 80000,
                ProgramId = "3",
                Frequency = 1,
                StartDate = today,
                DonorAccountId = 234,
                SubscriptionId = "sub_456",
                DayOfWeek = (int)today.DayOfWeek,
                RecurringGiftId = newRecurringGiftId,
                DonorId = 789
            };

            var contact = new MpMyContact()
            {
                Congregation_ID = congregationId
            };

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(authUserToken);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(editGift.RecurringGiftId)).Returns(existingGift);
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(existingGift.StripeCustomerId, editGift.StripeTokenId)).Returns(stripeSource);
            _mpDonorService.Setup(mocked => mocked.CreateDonorAccount(stripeSource.brand, "0", stripeSource.last4, null, existingGift.DonorId, stripeSource.id, existingGift.StripeCustomerId)).Returns(newDonorAccountId);
            _paymentService.Setup(mocked => mocked.CancelSubscription(existingGift.StripeCustomerId, existingGift.SubscriptionId)).Returns(oldSubscription);
            _paymentService.Setup(mocked => mocked.CreateSubscription(newPlan.Id, existingGift.StripeCustomerId, newRecurringGift.StartDate.Value, "me@here.com", "Bart Simpson")).Returns(newSubscription);
            _paymentService.Setup(mocked => mocked.CancelPlan(oldSubscription.Plan.Id)).Returns(oldSubscription.Plan);
            _paymentService.Setup(mocked => mocked.CreatePlan(editGift, donor, "me@here.com", "Bart Simpson")).Returns(newPlan);
            _mpDonorService.Setup(mocked => mocked.CancelRecurringGift(existingGift.RecurringGiftId.Value));
            _mpContactService.Setup(mocked => mocked.GetContactById(donor.ContactId)).Returns(contact);
            _mpDonorService.Setup(
                mocked =>
                    mocked.CreateRecurringGiftRecord(donor.DonorId,
                                                     newDonorAccountId,
                                                     EnumMemberSerializationUtils.ToEnumString(editGift.PlanInterval),
                                                     editGift.PlanAmount,
                                                     editGift.StartDate,
                                                     editGift.Program,
                                                     newSubscription.Id, contact.Congregation_ID.Value, 
                                                     null, 
                                                     null)).Returns(newRecurringGiftId);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftById(newRecurringGiftId)).Returns(newRecurringGift);

            var result = _fixture.EditRecurringGift(editGift, donor);
            _mpDonorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(newRecurringGift.RecurringGiftId, result.RecurringGiftId);
            Assert.AreEqual(newRecurringGift.StartDate, result.StartDate);
            Assert.AreEqual(newRecurringGift.Amount, result.PlanAmount);
            Assert.AreEqual(PlanInterval.Weekly, result.PlanInterval);
            Assert.AreEqual(newRecurringGift.ProgramId, result.Program);
            Assert.AreEqual(newRecurringGift.DonorId, result.DonorID);
            Assert.AreEqual(donor.Email, result.EmailAddress);
            Assert.AreEqual(newSubscription.Id, result.SubscriptionID);
        }

        [Test]
        public void ShouldGetPledgesInDateOrder()
        {

            var userAuthToken = "auth";

            var pledgeList = new List<MpPledge>
            {
                new MpPledge(){CampaignName = "Oldest Campaign", PledgeStatus = "Active", CampaignStartDate = DateTime.Parse("1/1/2000")},
                new MpPledge(){CampaignName = "Youngest Campaign", PledgeStatus = "Active", CampaignStartDate = DateTime.Parse("1/1/2016")},
                new MpPledge(){CampaignName = "Middle Campaign", PledgeStatus = "Active",CampaignStartDate = DateTime.Parse("1/1/2010")}
            }; 

            _pledgeService.Setup(mocked => mocked.GetPledgesForAuthUser(userAuthToken, new System.Int32 [1] )).Returns(pledgeList);

            var pledges = _fixture.GetCapitalCampaignPledgesForAuthenticatedUser(userAuthToken);
            _pledgeService.VerifyAll();

            Assert.AreEqual(pledges[0].PledgeCampaign, "Youngest Campaign");
            Assert.AreEqual(pledges[0].CampaignStartDate, "January 1, 2016");
            Assert.AreEqual(pledges[1].PledgeCampaign, "Middle Campaign");
            Assert.AreEqual(pledges[1].CampaignStartDate, "January 1, 2010");
            Assert.AreEqual(pledges[2].PledgeCampaign, "Oldest Campaign");
            Assert.AreEqual(pledges[2].CampaignStartDate, "January 1, 2000");
        }

        [Test]
        public void ShouldGetPledgesThatAreActive()
        {

            var userAuthToken = "auth";

            var pledgeList = new List<MpPledge>
            {
                new MpPledge(){CampaignName = "Active Campaign", PledgeStatus = "Active", CampaignStartDate = DateTime.Parse("1/1/2016") },
                new MpPledge(){CampaignName = "Completed Campaign", PledgeStatus = "Completed", CampaignStartDate = DateTime.Parse("1/1/2010")},
                new MpPledge(){CampaignName = "Inactive Campaign", PledgeStatus = "Discontinued", CampaignStartDate = DateTime.Parse("1/1/2000")}
            };

            _pledgeService.Setup(mocked => mocked.GetPledgesForAuthUser(userAuthToken, new System.Int32[1])).Returns(pledgeList);

            var pledges = _fixture.GetCapitalCampaignPledgesForAuthenticatedUser(userAuthToken);
            _pledgeService.VerifyAll();

            Assert.AreEqual(pledges.Count, 2);
            Assert.AreEqual(pledges[0].PledgeCampaign, "Active Campaign");
            Assert.AreEqual(pledges[0].CampaignStartDate, "January 1, 2016");
            Assert.AreEqual(pledges[1].PledgeCampaign, "Completed Campaign");
            Assert.AreEqual(pledges[1].CampaignStartDate, "January 1, 2010");

        }
    }
}
