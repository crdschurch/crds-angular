using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;
using Moq;
using NUnit.Framework;
using DonationStatus = crds_angular.Models.Crossroads.Stewardship.DonationStatus;
using IDonorService = crds_angular.Services.Interfaces.IDonorService;
using IDonationService = crds_angular.Services.Interfaces.IDonationService;
using IAnalyticsService = crds_angular.Services.Interfaces.IAnalyticsService;

namespace crds_angular.test.controllers
{
    class DonationControllerTest
    {
        private DonationController fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository> donorServiceMock;
        private Mock<MinistryPlatform.Translation.Repositories.Interfaces.IInvoiceRepository> invoiceServiceMock;
        private Mock<IPaymentProcessorService> stripeServiceMock;
        private Mock<IAuthenticationRepository> authenticationServiceMock;
        private Mock<IContactRepository> contactRepositoryMock;
        private Mock<IDonorService> gatewayDonorServiceMock;
        private Mock<IDonationService> gatewayDonationServiceMock;
        private Mock<IUserImpersonationService> impersonationService;
        private Mock<IPaymentService> paymentServiceMock;
        private Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonationRepository> mpDonationService; 
        private Mock<IPledgeRepository> mpPledgeService;
        private Mock<IAnalyticsService> analyticsService;
        private string authToken;
        private string authType;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            donorServiceMock = new Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository>();
            invoiceServiceMock = new Mock<MinistryPlatform.Translation.Repositories.Interfaces.IInvoiceRepository>();
            gatewayDonorServiceMock = new Mock<IDonorService>();
            stripeServiceMock = new Mock<IPaymentProcessorService>(MockBehavior.Strict);
            authenticationServiceMock = new Mock<IAuthenticationRepository>();
            contactRepositoryMock = new Mock<IContactRepository>();
            gatewayDonationServiceMock = new Mock<IDonationService>();
            mpPledgeService = new Mock<IPledgeRepository>();
            impersonationService = new Mock<IUserImpersonationService>();
            paymentServiceMock = new Mock<IPaymentService>();
            mpDonationService = new Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonationRepository>();
            analyticsService = new Mock<IAnalyticsService>();

            fixture = new DonationController(_authTokenExpiryService.Object, donorServiceMock.Object, stripeServiceMock.Object
                , authenticationServiceMock.Object, contactRepositoryMock.Object, gatewayDonorServiceMock.Object, gatewayDonationServiceMock.Object
                , mpDonationService.Object, mpPledgeService.Object, impersonationService.Object, paymentServiceMock.Object, invoiceServiceMock.Object, analyticsService.Object);

            authType = "auth_type";
            authToken = "auth_token";
            fixture.Request = new HttpRequestMessage();
            fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void TestGetDonations()
        {
            var donations = new List<DonationDTO>
            {
                new DonationDTO
                {
                    BatchId = 123,
                    Amount = 78900,
                    DonationDate = DateTime.Now,
                    Id = "456",
                    Source = new DonationSourceDTO
                    {
                    SourceType = PaymentType.CreditCard,
                    CardType = CreditCardType.AmericanExpress,
                    AccountHolderName = "ending in 1234",
                    PaymentProcessorId = "tx_123",
                    },
                    Email = "me@here.com",
                    ProgramId = "3",
                    Status = DonationStatus.Succeeded
                }
            };
            var dto = new DonationsDTO();
            dto.Donations.AddRange(donations);

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            gatewayDonationServiceMock.Setup(mocked => mocked.GetDonationsForAuthenticatedUser(authType + " " + authToken, "1999", null, true, true)).Returns(dto);
            var response = fixture.GetDonations("1999", null, true);
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<DonationsDTO>>(response);
            var r = (OkNegotiatedContentResult<DonationsDTO>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreSame(dto, r.Content);
        }

        [Test]
        public void TestGetDonationsNoDonationsFound()
        {
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            gatewayDonationServiceMock.Setup(mocked => mocked.GetDonationsForAuthenticatedUser(authType + " " + authToken, "1999", null, true, true)).Returns((DonationsDTO)null);
            var response = fixture.GetDonations("1999", null, true);
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(response);
            var r = (RestHttpActionResult<ApiErrorDto>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("No matching donations found", r.Content.Message);
        }

        [Test]
        public void TestGetDonationsImpersonationNotAllowed()
        {
            donorServiceMock.Setup(mocked => mocked.GetEmailViaDonorId(123)).Returns(new MpContactDonor
            {
                Email = "me@here.com"
            });
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true); 
            impersonationService.Setup(mocked => mocked.WithImpersonation(authType + " " + authToken, "me@here.com", It.IsAny<Func<DonationsDTO>>()))
                .Throws<ImpersonationNotAllowedException>();
            var response = fixture.GetDonations("1999", null, true, 123);
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(response);
            var r = (RestHttpActionResult<ApiErrorDto>)response;
            Assert.AreEqual(HttpStatusCode.Forbidden, r.StatusCode);
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("User is not authorized to impersonate other users.", r.Content.Message);
        }

        [Test]
        public void TestGetDonationsImpersonationUserNotFound()
        {
            donorServiceMock.Setup(mocked => mocked.GetEmailViaDonorId(123)).Returns(new MpContactDonor
            {
                Email = "me@here.com"
            });
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            impersonationService.Setup(mocked => mocked.WithImpersonation(authType + " " + authToken, "me@here.com", It.IsAny<Func<DonationsDTO>>()))
                .Throws(new ImpersonationUserNotFoundException("me@here.com"));
            var response = fixture.GetDonations("1999", null, true, 123);
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(response);
            var r = (RestHttpActionResult<ApiErrorDto>)response;
            Assert.AreEqual(HttpStatusCode.Conflict, r.StatusCode);
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("Could not locate user 'me@here.com' to impersonate.", r.Content.Message);
        }

        [Test]
        public void TestGetDonationYears()
        {
            var donationYears = new List<string>
            {
                "1999",
                "2010",
                "2038"
            };
            var dto = new DonationYearsDTO();
            dto.AvailableDonationYears.AddRange(donationYears);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            gatewayDonationServiceMock.Setup(mocked => mocked.GetDonationYearsForAuthenticatedUser(authType + " " + authToken)).Returns(dto);
            var response = fixture.GetDonationYears();
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<DonationYearsDTO>>(response);
            var r = (OkNegotiatedContentResult<DonationYearsDTO>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreSame(dto, r.Content);
        }

        [Test]
        public void TestGetDonationYearsNoYearsFound()
        {
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            gatewayDonationServiceMock.Setup(mocked => mocked.GetDonationYearsForAuthenticatedUser(authType + " " + authToken)).Returns((DonationYearsDTO)null);
            var response = fixture.GetDonationYears();
            gatewayDonationServiceMock.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(response);
            var r = (RestHttpActionResult<ApiErrorDto>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("No donation years found", r.Content.Message);
        }


        [Test]
        public void testPostToCreateDonationAndDistributionAuthenticated()
        {
            var contactId = 999999;
            var donationId = 6186818;
            var charge = new StripeCharge()
            {
                Id = "ch_crdscharge86868",
                BalanceTransaction = new StripeBalanceTransaction()
                {
                    Fee = 987
                }
            };

            var createDonationDTO = new CreateDonationDTO
            {
                ProgramId = "3", //crossroads
                Amount = 86868,
                DonorId = 394256,
                EmailAddress = "test@test.com",
                PaymentType = "bank"
            };

            var donor = new MpContactDonor
            {
                ContactId = contactId,
                DonorId = 424242,
                SetupDate = new DateTime(),
                StatementFreq = "1",
                StatementMethod = "2",
                StatementType = "3",
                ProcessorId = "cus_test1234567",
                Email = "moc.tset@tset",
                Details = new MpContactDetails
                {
                    FirstName = "Bart",
                    LastName = "Simpson",
                    EmailAddress = "me@here.com",
                    DisplayName = "Bart Simpson"
                }
            };

            contactRepositoryMock.Setup(mocked => mocked.GetContactId(authType + " " + authToken)).Returns(contactId);

            donorServiceMock.Setup(mocked => mocked.GetContactDonor(contactId))
                .Returns(donor);

            stripeServiceMock.Setup(
                mocked => mocked.ChargeCustomer(donor.ProcessorId, createDonationDTO.Amount, donor.DonorId, false, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(charge);

            donorServiceMock.Setup(mocked => mocked.
                                       CreateDonationAndDistributionRecord(
                                           It.Is<MpDonationAndDistributionRecord>(
                                               d => d.DonationAmt == createDonationDTO.Amount &&
                                                    d.FeeAmt == charge.BalanceTransaction.Fee &&
                                                    d.DonorId == donor.DonorId &&
                                                    d.ProgramId.Equals(createDonationDTO.ProgramId) &&
                                                    d.PledgeId == null &&
                                                    d.ChargeId.Equals(charge.Id) &&
                                                    d.PymtType.Equals(createDonationDTO.PaymentType) &&
                                                    d.ProcessorId.Equals(donor.ProcessorId) &&
                                                    d.RegisteredDonor &&
                                                    !d.Anonymous &&
                                                    !d.RecurringGift &&
                                                    d.RecurringGiftId == null &&
                                                    d.DonorAcctId == null &&
                                                    d.CheckScannerBatchName == null &&
                                                    d.DonationStatus == null &&
                                                    d.CheckNumber == null), true))
                .Returns(donationId);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);  
            IHttpActionResult result = fixture.Post(createDonationDTO);

            authenticationServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonationDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonationDTO>)result;
            Assert.AreEqual(6186818, donationId);

            var resultDto = ((OkNegotiatedContentResult<DonationDTO>) result).Content;
            Assert.IsNotNull(resultDto);
            Assert.AreEqual(donor.Email, resultDto.Email);
        }

        [Test]
        public void testPostToCreateDonationAndDistributionWithPledgeAuthenticated()
        {
            var contactId = 999999;
            var donationId = 6186818;
            var charge = new StripeCharge()
            {
                Id = "ch_crdscharge86868",
                BalanceTransaction = new StripeBalanceTransaction()
                {
                    Fee = 987
                }
            };

            var createDonationDTO = new CreateDonationDTO
            {
                ProgramId = "3", //crossroads
                Amount = 86868,
                DonorId = 394256,
                EmailAddress = "test@test.com",
                PledgeCampaignId = 23,
                PledgeDonorId = 42,
                GiftMessage = "Don't look a Gift Horse in the Mouth!",
                PaymentType = "junk bonds"
            };

            var donor = new MpContactDonor
            {
                ContactId = contactId,
                DonorId = 424242,
                SetupDate = new DateTime(),
                StatementFreq = "1",
                StatementMethod = "2",
                StatementType = "3",
                ProcessorId = "cus_test1234567",
                Email = "moc.tset@tset",
                Details = new MpContactDetails
                {
                    FirstName = "Bart",
                    LastName = "Simpson"
                }
            };

            var pledgeId = 3456;
            var pledge = new MpPledge
            {
                DonorId = 1,
                PledgeCampaignId = 2,
                PledgeId = pledgeId,
                PledgeStatusId = 1
            };

            contactRepositoryMock.Setup(mocked => mocked.GetContactId(authType + " " + authToken)).Returns(contactId);

            donorServiceMock.Setup(mocked => mocked.GetContactDonor(contactId))
                .Returns(donor);

            mpPledgeService.Setup(mocked => mocked.GetPledgeByCampaignAndDonor(createDonationDTO.PledgeCampaignId.Value, createDonationDTO.PledgeDonorId.Value)).Returns(pledge);

            // it doesn't seem right to have donationId passed into this, but it's in the function now
            mpDonationService.Setup(mocked => mocked.SendMessageFromDonor(pledgeId, donationId, createDonationDTO.GiftMessage, "Daddy Warbucks"));

            stripeServiceMock.Setup(
                mocked => mocked.ChargeCustomer(donor.ProcessorId, createDonationDTO.Amount, donor.DonorId, false, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(charge);

            donorServiceMock.Setup(mocked => mocked.
                CreateDonationAndDistributionRecord(It.Is<MpDonationAndDistributionRecord>(
                                               d => d.DonationAmt == createDonationDTO.Amount &&
                                                    d.FeeAmt == charge.BalanceTransaction.Fee &&
                                                    d.DonorId == donor.DonorId &&
                                                    d.ProgramId.Equals(createDonationDTO.ProgramId) &&
                                                    d.PledgeId == pledgeId &&
                                                    d.ChargeId.Equals(charge.Id) &&
                                                    d.PymtType.Equals(createDonationDTO.PaymentType) &&
                                                    d.ProcessorId.Equals(donor.ProcessorId) &&
                                                    d.RegisteredDonor &&
                                                    !d.Anonymous &&
                                                    !d.RecurringGift &&
                                                    d.RecurringGiftId == null &&
                                                    d.DonorAcctId == null &&
                                                    d.CheckScannerBatchName == null &&
                                                    d.DonationStatus == null &&
                                                    d.CheckNumber == null), true))
                    .Returns(donationId);

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            IHttpActionResult result = fixture.Post(createDonationDTO);

            authenticationServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();
            mpPledgeService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonationDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonationDTO>)result;
            Assert.AreEqual(6186818, donationId);

            var resultDto = ((OkNegotiatedContentResult<DonationDTO>)result).Content;
            Assert.IsNotNull(resultDto);
            Assert.AreEqual(donor.Email, resultDto.Email);
        }

        [Test]
        public void testPostToCreateDonationAndDistributionWithPledgeUnauthenticated()
        {
            var contactId = 999999;
            var donationId = 6186818;
            var charge = new StripeCharge()
            {
                Id = "ch_crdscharge86868",
                BalanceTransaction = new StripeBalanceTransaction()
                {
                    Fee = 987
                }
            };

            var createDonationDTO = new CreateDonationDTO
            {
                ProgramId = "3", //crossroads
                Amount = 86868,
                DonorId = 394256,
                EmailAddress = "test@test.com",
                PledgeCampaignId = 23,
                PledgeDonorId = 42,
                GiftMessage = "Don't look a Gift Horse in the Mouth!",
                PaymentType = "card",
                SourceUrl = "www.ninjas.com",
                PredefinedAmount = 86868
            };

            var donor = new MpContactDonor
            {
                ContactId = contactId,
                DonorId = 424242,
                SetupDate = new DateTime(),
                StatementFreq = "1",
                StatementMethod = "2",
                StatementType = "3",
                ProcessorId = "cus_test1234567",
                Email = "moc.tset@tset",
                Details = new MpContactDetails
                {
                    DisplayName = "Bart Simpson"
                }
            };

            var pledgeId = 3456;
            var pledge = new MpPledge
            {
                DonorId = 1,
                PledgeCampaignId = 2,
                PledgeId = pledgeId,
                PledgeStatusId = 1
            };

            fixture.Request.Headers.Authorization = null;
            gatewayDonorServiceMock.Setup(mocked => mocked.GetContactDonorForEmail(createDonationDTO.EmailAddress)).Returns(donor);

            mpPledgeService.Setup(mocked => mocked.GetPledgeByCampaignAndDonor(createDonationDTO.PledgeCampaignId.Value, createDonationDTO.PledgeDonorId.Value)).Returns(pledge);

            // it doesn't seem right to have donationId passed into this, but it's in the function now
            mpDonationService.Setup(mocked => mocked.SendMessageFromDonor(pledgeId, donationId, createDonationDTO.GiftMessage, "Daddy Warbucks"));

            stripeServiceMock.Setup(mocked => mocked.ChargeCustomer(donor.ProcessorId, createDonationDTO.Amount, donor.DonorId, false, "moc.tset@tset","Bart Simpson")).
                Returns(charge);


            donorServiceMock.Setup(mocked => mocked.
                CreateDonationAndDistributionRecord(It.Is<MpDonationAndDistributionRecord>(
                                           d => d.DonationAmt == createDonationDTO.Amount &&
                                                d.FeeAmt == charge.BalanceTransaction.Fee &&
                                                d.DonorId == donor.DonorId &&
                                                d.ProgramId.Equals(createDonationDTO.ProgramId) &&
                                                d.PledgeId == pledgeId &&
                                                d.ChargeId.Equals(charge.Id) &&
                                                d.PymtType.Equals(createDonationDTO.PaymentType) &&
                                                d.ProcessorId.Equals(donor.ProcessorId) &&
                                                !d.RegisteredDonor &&
                                                !d.Anonymous &&
                                                !d.RecurringGift &&
                                                d.RecurringGiftId == null &&
                                                d.DonorAcctId == null &&
                                                d.CheckScannerBatchName == null &&
                                                d.DonationStatus == null &&
                                                d.CheckNumber == null && 
                                                d.PredefinedAmount == createDonationDTO.PredefinedAmount &&
                                                d.SourceUrl == createDonationDTO.SourceUrl), true))
                    .Returns(donationId);


            IHttpActionResult result = fixture.Post(createDonationDTO);

           
            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();
            mpPledgeService.VerifyAll();

            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonationDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonationDTO>)result;
            Assert.AreEqual(6186818, donationId);

            var resultDto = ((OkNegotiatedContentResult<DonationDTO>)result).Content;
            Assert.IsNotNull(resultDto);
            Assert.AreEqual(donor.Email, resultDto.Email);
        }

        [Test]
        public void testPostToCreateDonationAndDistributionUnauthenticated()
        {
            var contactId = 999999;
            var donationId = 6186818;
            var charge = new StripeCharge()
            {
                Id = "ch_crdscharge86868",
                BalanceTransaction = new StripeBalanceTransaction()
                {
                    Fee = 987
                }
            };
           
            var createDonationDTO = new CreateDonationDTO
            {
                ProgramId = "3", //crossroads
                Amount = 86868,
                DonorId = 394256,
                EmailAddress = "test@test.com",
                PaymentType = "bank"
            };

            var donor = new MpContactDonor
            {
                ContactId = contactId,
                DonorId = 424242,
                SetupDate = new DateTime(),
                StatementFreq = "1",
                StatementMethod = "2",
                StatementType = "3",
                ProcessorId = "cus_test1234567",
                Email = "moc.tset@tset",
                Details = new MpContactDetails
                {
                    FirstName = "Bart",
                    LastName = "Simpson",
                    EmailAddress = "me@here.com",
                    DisplayName = "Bart Simpson"
                }
            };

            fixture.Request.Headers.Authorization = null;
            gatewayDonorServiceMock.Setup(mocked => mocked.GetContactDonorForEmail(createDonationDTO.EmailAddress)).Returns(donor);

            stripeServiceMock.Setup(mocked => mocked.ChargeCustomer(donor.ProcessorId, createDonationDTO.Amount, donor.DonorId,false, "moc.tset@tset", "Bart Simpson")).
                Returns(charge);

            donorServiceMock.Setup(mocked => mocked.
                                       CreateDonationAndDistributionRecord(It.Is<MpDonationAndDistributionRecord>(
                                           d => d.DonationAmt == createDonationDTO.Amount &&
                                                d.FeeAmt == charge.BalanceTransaction.Fee &&
                                                d.DonorId == donor.DonorId &&
                                                d.ProgramId.Equals(createDonationDTO.ProgramId) &&
                                                d.PledgeId == null &&
                                                d.ChargeId.Equals(charge.Id) &&
                                                d.PymtType.Equals(createDonationDTO.PaymentType) &&
                                                d.ProcessorId.Equals(donor.ProcessorId) &&
                                                !d.RegisteredDonor &&
                                                !d.Anonymous &&
                                                !d.RecurringGift &&
                                                d.RecurringGiftId == null &&
                                                d.DonorAcctId == null &&
                                                d.CheckScannerBatchName == null &&
                                                d.DonationStatus == null &&
                                                d.CheckNumber == null), true))
                .Returns(donationId);

            IHttpActionResult result = fixture.Post(createDonationDTO);

            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonationDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonationDTO>)result;
            Assert.AreEqual(6186818, donationId);

            var resultDto = ((OkNegotiatedContentResult<DonationDTO>)result).Content;
            Assert.IsNotNull(resultDto);
            Assert.AreEqual(donor.Email, resultDto.Email);
        }

        [Test]
        public void testPostToCreateDonationAndDistributionAuthenticatedPayment()
        {
            var contactId = 999999;
            var payment = new MpPaymentDetailReturn();
            payment.PaymentId = 46546;

            var charge = new StripeCharge()
            {
                Id = "ch_crdscharge86868",
                BalanceTransaction = new StripeBalanceTransaction()
                {
                    Fee = 987
                }
            };

            var createDonationDTO = new CreateDonationDTO
            {               
                Amount = 86868,
                DonorId = 394256,
                EmailAddress = "test@test.com",
                PaymentType = "bank",
                TransactionType = "PAYMENT",
                InvoiceId = 88
            };

            var donor = new MpContactDonor
            {
                ContactId = contactId,
                DonorId = 424242,
                SetupDate = new DateTime(),
                StatementFreq = "1",
                StatementMethod = "2",
                StatementType = "3",
                ProcessorId = "cus_test1234567",
                Email = "moc.tset@tset",
                Details = new MpContactDetails
                {
                    FirstName = "Bart",
                    LastName = "Simpson",
                    EmailAddress = "me@here.com",
                    DisplayName = "Bart Simpson"
                }

            };

            contactRepositoryMock.Setup(mocked => mocked.GetContactId(authType + " " + authToken)).Returns(contactId);

            donorServiceMock.Setup(mocked => mocked.GetContactDonor(contactId))
                .Returns(donor);

            stripeServiceMock.Setup(
                mocked => mocked.ChargeCustomer(donor.ProcessorId, createDonationDTO.Amount, donor.DonorId, true, "moc.tset@tset", "Bart Simpson")) 
                .Returns(charge);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            invoiceServiceMock.Setup(mocked => mocked.InvoiceExists(It.IsAny<int>()))
                .Returns(true);

            paymentServiceMock.Setup(mocked => mocked.PostPayment(It.IsAny<MpDonationAndDistributionRecord>()))
                .Returns(payment);

            IHttpActionResult result = fixture.Post(createDonationDTO);

            authenticationServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();
            stripeServiceMock.VerifyAll();
            donorServiceMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonationDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonationDTO>)result;
            Assert.AreEqual(46546, payment.PaymentId);
        }

    }
}
