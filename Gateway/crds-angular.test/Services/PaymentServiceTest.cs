using System;
using System.Collections.Generic;
using crds_angular.App_Start;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using crds_angular.test.Helpers;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Product;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class PaymentServiceTest
    {

        private readonly Mock<IInvoiceRepository> _invoiceRepository;
        private readonly Mock<IPaymentRepository> _paymentRepository;
        private readonly Mock<IContactRepository> _contactRepository;
        private readonly Mock<IPaymentTypeRepository> _paymentTypeRepository;
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<ICommunicationRepository> _communicationRepository;
        private readonly Mock<IConfigurationWrapper> _configWrapper;
        private readonly Mock<IApiUserRepository> _apiUserRepository;
        private readonly Mock<IProductRepository> _productRepository;
        private readonly Mock<IProgramRepository> _programRepository;
        private readonly Mock<IPaymentProcessorService> _paymentProcessorService;

        private readonly IPaymentService _fixture;

        private readonly int paidInFull = 54;
        private readonly int somePaid = 45;
        private readonly int nonePaid = 12;
        private readonly int defaultPaymentStatus = 15;
        private readonly int declinedPaymentStatus = 20;
        private readonly int defaultCongregation = 5;
        private readonly int defaultInvoicePaymentTemplate = 2022;

        public PaymentServiceTest()
        {
            AutoMapperConfig.RegisterMappings();

            Factories.MpPayment();
            Factories.MpEvent();
            Factories.MpMyContact();

            _invoiceRepository = new Mock<IInvoiceRepository>();
            _paymentRepository = new Mock<IPaymentRepository>();
            _contactRepository = new Mock<IContactRepository>();
            _eventRepository = new Mock<IEventRepository>();
            _paymentTypeRepository = new Mock<IPaymentTypeRepository>();
            _communicationRepository = new Mock<ICommunicationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _productRepository = new Mock<IProductRepository>();
            _programRepository = new Mock<IProgramRepository>();
            _paymentProcessorService = new Mock<IPaymentProcessorService>();
            _configWrapper.Setup(m => m.GetConfigIntValue("PaidInFull")).Returns(paidInFull);
            _configWrapper.Setup(m => m.GetConfigIntValue("SomePaid")).Returns(somePaid);
            _configWrapper.Setup(m => m.GetConfigIntValue("NonePaid")).Returns(nonePaid);
            _configWrapper.Setup(m => m.GetConfigIntValue("DonationStatusPending")).Returns(defaultPaymentStatus);
            _configWrapper.Setup(m => m.GetConfigIntValue("DonationStatusDeclined")).Returns(declinedPaymentStatus);
            _configWrapper.Setup(m => m.GetConfigIntValue("DefaultInvoicePaymentEmailTemplate")).Returns(defaultInvoicePaymentTemplate);

            _fixture = new PaymentService(_invoiceRepository.Object, _paymentRepository.Object, _configWrapper.Object, _contactRepository.Object,
              _paymentTypeRepository.Object, _eventRepository.Object, _communicationRepository.Object, _apiUserRepository.Object, _productRepository.Object,
              _programRepository.Object, _paymentProcessorService.Object);            
        }         

        [Test]
        public void shouldGetPaymentDetailDTO()
        {
            const int paymentId = 12345;
            const int invoiceId = 3389753;
            const int contactId = 12323354;
            const string emailAddress = "help_me@usa.com";
            const string token = "NOOOOO";
            StripeCharge charge = new StripeCharge()
            {
                Source = new StripeSource()
                {
                    AccountNumberLast4 = "2342"
                }
            };

            var me = fakeMyContact(contactId, emailAddress);
            var invoice = fakeInvoice(invoiceId, contactId, 500.00M);
            var payments = fakePayments(contactId, 24M, paymentId);

            _invoiceRepository.Setup(m => m.GetInvoice(invoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(payments);
            _paymentProcessorService.Setup(m => m.GetCharge(It.IsAny<string>())).Returns(charge);
            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);

            PaymentDetailDTO ret = _fixture.GetPaymentDetails(paymentId, invoiceId, token, false);
            Assert.AreEqual(24M, ret.PaymentAmount);
            Assert.AreEqual(emailAddress, ret.RecipientEmail);
            Assert.AreEqual(452M, ret.TotalToPay);

            _contactRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
            _paymentRepository.VerifyAll();
        }

        [Test]
        public void ShouldGetPaymentDetailDtOwithPaymentId0()
        {
            const int paymentId = 0;
            const int invoiceId = 3389753;
            const int contactId = 12323354;
            const string emailAddress = "help_me@usa.com";
            const string token = "NOOOOO";
            StripeCharge charge = new StripeCharge()
            {
                Source = new StripeSource()
                {
                    AccountNumberLast4 = "2342"
                }
            };

            var me = fakeMyContact(contactId, emailAddress);
            var invoice = fakeInvoice(invoiceId, contactId, 500.00M);
            var payments = fakePayments(contactId, 24M, paymentId);

            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _invoiceRepository.Setup(m => m.GetInvoice(invoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(payments);
            _paymentProcessorService.Setup(m => m.GetCharge(It.IsAny<string>())).Returns(charge);

            PaymentDetailDTO ret = _fixture.GetPaymentDetails(paymentId, invoiceId, token, false);
            Assert.AreEqual(24M, ret.PaymentAmount);
            Assert.AreEqual(emailAddress, ret.RecipientEmail);
            Assert.AreEqual(452M, ret.TotalToPay);

            _contactRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
            _paymentRepository.VerifyAll();
        }

        [Test]
        public void shouldNotHavePaymentDetails()
        {
            const int paymentId = 12345;
            const int invoiceId = 3389753;
            const int contactId = 12323354;
            const string emailAddress = "help_me@usa.com";
            const string token = "NOOOOO";

            var me = fakeMyContact(contactId, emailAddress);
            var invoice = fakeInvoice(invoiceId, contactId, 500.00M);
            var payments = fakePayments(1, 24M, paymentId);

            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _invoiceRepository.Setup(m => m.GetInvoice(invoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(payments);

            Assert.Throws<Exception>(() =>
            {
                _fixture.GetPaymentDetails(paymentId, invoiceId, token);
                _contactRepository.VerifyAll();
                _invoiceRepository.VerifyAll();
                _paymentRepository.VerifyAll();
            });


        }

        [Test]
        public void shouldNotFindInvoice()
        {
            var paymentDto = fakePaymentDto();
            //MpDonationAndDistributionRecord

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(false);            
            Assert.Throws<InvoiceNotFoundException>(() =>
            {
                _fixture.PostPayment(paymentDto);
                _invoiceRepository.VerifyAll();
                _configWrapper.VerifyAll();
            });
        }

        [Test]
        public void shouldNotFindContact()
        {
            var paymentDto = fakePaymentDto();

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns((MpMyContact) null);
            Assert.Throws<ContactNotFoundException>(() =>
            {
                _fixture.PostPayment(paymentDto);
                _invoiceRepository.VerifyAll();
                _contactRepository.VerifyAll();
                _configWrapper.VerifyAll();
            });
        }

        [Test]
        public void shouldNotFindPaymentType()
        {
            var paymentDto = fakePaymentDto();

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns(new MpMyContact()
            {
                Contact_ID = paymentDto.ContactId
            });
            _paymentTypeRepository.Setup(m => m.PaymentTypeExists(5)).Returns(false);

            Assert.Throws<PaymentTypeNotFoundException>(() =>
            {
                _fixture.PostPayment(paymentDto);
                _invoiceRepository.VerifyAll();
                _contactRepository.VerifyAll();
                _paymentTypeRepository.VerifyAll();
                _configWrapper.VerifyAll();
            });
        }

        [Test]
        public void shouldFailToMakePayment()
        {
            var paymentDto = fakePaymentDto();
            var invoiceDetail = fakeInvoiceDetail(paymentDto.InvoiceId);

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns(new MpMyContact()
            {
                Contact_ID = paymentDto.ContactId
            });
            _contactRepository.Setup(m => m.GetContactById(invoiceDetail.RecipientContactId)).Returns(new MpMyContact()
            {
                Congregation_ID = 1
            });
            _paymentTypeRepository.Setup(m => m.PaymentTypeExists(5)).Returns(true);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailForInvoice(paymentDto.InvoiceId)).Returns(invoiceDetail);            

        

            _paymentRepository.Setup(m => m.CreatePaymentAndDetail(It.IsAny<MpPaymentDetail>())).Returns(new Result<MpPaymentDetailReturn>(false, "error"));

            Assert.Throws<Exception>(() =>
            {
                _fixture.PostPayment(paymentDto);
                _invoiceRepository.VerifyAll();
                _contactRepository.VerifyAll();
                _paymentTypeRepository.VerifyAll();
                _paymentRepository.VerifyAll();
                _configWrapper.VerifyAll();

            });            
        }

        [Test]
        public void shouldMakePaymentOfSomePaid()
        {
            var paymentDto = fakePaymentDto();
            var invoiceDetail = fakeInvoiceDetail(paymentDto.InvoiceId);

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns(new MpMyContact()
            {
                Contact_ID = paymentDto.ContactId
            });
            _contactRepository.Setup(m => m.GetContactById(invoiceDetail.RecipientContactId)).Returns(new MpMyContact()
            {
                Congregation_ID = 1
            });
            _paymentTypeRepository.Setup(m => m.PaymentTypeExists(5)).Returns(true);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailForInvoice(paymentDto.InvoiceId)).Returns(invoiceDetail);

            var retVal = new MpPaymentDetailReturn()
            {
                InvoiceDetailId = invoiceDetail.InvoiceDetailId,
                PaymentAmount = paymentDto.DonationAmt,
                PaymentDetailId = 398457,
                PaymentId = 2897234
            };

            _paymentRepository.Setup(m => m.CreatePaymentAndDetail(It.IsAny<MpPaymentDetail>())).Returns(new Result<MpPaymentDetailReturn>(true, retVal));

            var invoice = new MpInvoice()
            {
                InvoiceId = paymentDto.InvoiceId,
                InvoiceTotal = 500
            };                        

            var paymentsSoFar = new List<MpPayment>
            {
                new MpPayment()
                {
                    PaymentTotal = 20,
                    PaymentStatus = 3
                },
                 new MpPayment()
                {
                    PaymentTotal = 200,
                    PaymentStatus = 3
                }
            };

            _invoiceRepository.Setup(m => m.GetInvoice(invoiceDetail.InvoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(paymentDto.InvoiceId)).Returns(paymentsSoFar);
            _invoiceRepository.Setup(m => m.SetInvoiceStatus(paymentDto.InvoiceId, somePaid));

            var val = _fixture.PostPayment(paymentDto);
            Assert.AreEqual(retVal, val);
            _invoiceRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _paymentTypeRepository.VerifyAll();
            _paymentRepository.VerifyAll();
            _configWrapper.VerifyAll();
        }

        [Test]
        public void shouldMakePaymentOfFullPaid()
        {
            var paymentDto = fakePaymentDto();
            var invoiceDetail = fakeInvoiceDetail(paymentDto.InvoiceId);

            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns(new MpMyContact()
            {
                Contact_ID = paymentDto.ContactId
            });
            _contactRepository.Setup(m => m.GetContactById(invoiceDetail.RecipientContactId)).Returns(new MpMyContact()
            {
                Congregation_ID = 1
            });
            _paymentTypeRepository.Setup(m => m.PaymentTypeExists(5)).Returns(true);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailForInvoice(paymentDto.InvoiceId)).Returns(invoiceDetail);

            var retVal = new MpPaymentDetailReturn()
            {
                InvoiceDetailId = invoiceDetail.InvoiceDetailId,
                PaymentAmount = paymentDto.DonationAmt,
                PaymentDetailId = 398457,
                PaymentId = 2897234
            };

            _paymentRepository.Setup(m => m.CreatePaymentAndDetail(It.IsAny<MpPaymentDetail>())).Returns(new Result<MpPaymentDetailReturn>(true, retVal));

            var invoice = new MpInvoice()
            {
                InvoiceId = paymentDto.InvoiceId,
                InvoiceTotal = 500
            };

            var paymentsSoFar = new List<MpPayment>
            {
                new MpPayment()
                {
                    PaymentTotal = 300,
                    PaymentStatus = 3
                },
                 new MpPayment()
                {
                    PaymentTotal = 200,
                    PaymentStatus = 3
                }
            };

            _invoiceRepository.Setup(m => m.GetInvoice(invoiceDetail.InvoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(paymentDto.InvoiceId)).Returns(paymentsSoFar);
            _invoiceRepository.Setup(m => m.SetInvoiceStatus(paymentDto.InvoiceId, paidInFull));

            var val = _fixture.PostPayment(paymentDto);
            Assert.AreEqual(retVal, val);
            _invoiceRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _paymentTypeRepository.VerifyAll();
            _paymentRepository.VerifyAll();
            _configWrapper.VerifyAll();
        }

        [Test]
        public void GetPaymentByTransactionCodeShoudThrow()
        {
            var stripepaymentid = "qwerty";
            var e = new Exception("bad things");
            _paymentRepository.Setup(w => w.GetPaymentByTransactionCode(stripepaymentid)).Throws(e);

            Assert.Throws<PaymentNotFoundException>(() => _fixture.GetPaymentByTransactionCode(stripepaymentid));
        }

        public void DepositAlreadyExists()
        {
            const string token = "letmein";
            const int payerId = 3333;           
            const int invoiceId = 89989;

            var payments = fakePayments(payerId, 500);
            var me = fakeMyContact(payerId, "faekemail@jon.com");

            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(payments);            
                
            var result = _fixture.DepositExists(invoiceId, token);
            Assert.IsTrue(result);
        }

        [Test]
        public void DepositDoesNotExist()
        {
            const string token = "letmein";
            const int payerId = 3333;
            const int invoiceId = 89989;

            var payments = fakePayments(1234, 500);
            var me = fakeMyContact(payerId, "faekemail@jon.com");

            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(payments);

            var result = _fixture.DepositExists(invoiceId, token);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldSendPaymentConfirmation()
        {
            const int paymentId = 345;
            const int eventId = 231;
            const int emailTemplateId = 555;
            const string token = "testingapi";
            const int contactId = 8484;
            const string baseUrl = "some url.com";

            const string eventTitle = "My Awesome Event";
            const decimal paymentTotal = 56M;

            var me = FactoryGirl.NET.FactoryGirl.Build<MpMyContact>(m =>
            {
                m.Contact_ID = contactId;
                m.Email_Address = "me.com@.com";
            });
            var payment = FactoryGirl.NET.FactoryGirl.Build<MpPayment>(m => { m.PaymentId = paymentId; m.PaymentTotal = paymentTotal; });
            var mpEvent = FactoryGirl.NET.FactoryGirl.Build<MpEvent>(m =>
            {
                m.EventId = eventId;
                m.EventTitle = eventTitle;
                m.PrimaryContactId = 1234;
                m.PrimaryContact = new MpContact
                {
                    EmailAddress = "Lucille@bluth.com",
                    PreferredName = "Lucille",
                    ContactId = 1234
                };
            });

            var mergeData = new Dictionary<string, object>
            {
                {"Event_Title", mpEvent.EventTitle},
                {"Payment_Total", "56.00"},
                {"Primary_Contact_Email", mpEvent.PrimaryContact.EmailAddress },
                {"Primary_Contact_Display_Name", mpEvent.PrimaryContact.PreferredName },
                {"Base_Url",  baseUrl}
            };

            Assert.AreEqual("56.00", 56M.ToString(".00"));

            _paymentRepository.Setup(m => m.GetPaymentById(paymentId)).Returns(payment);
            _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(mpEvent);
            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _eventRepository.Setup(m => m.GetProductEmailTemplate(eventId)).Returns(new Ok<int>(emailTemplateId));
            _configWrapper.Setup(m => m.GetConfigValue("BaseUrl")).Returns(baseUrl);
            //_configWrapper.Setup(m => m.GetConfigIntValue("DefaultPaymentEmailTempalte")).Returns(defaultTemplateId);

            _communicationRepository.Setup(
                m =>
                    m.GetTemplateAsCommunication(emailTemplateId,
                                                 mpEvent.PrimaryContactId,
                                                 mpEvent.PrimaryContact.EmailAddress,
                                                 mpEvent.PrimaryContactId,
                                                 mpEvent.PrimaryContact.EmailAddress,
                                                 me.Contact_ID,
                                                 me.Email_Address,
                                                 mergeData));
            _communicationRepository.Setup(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false));
            _fixture.SendPaymentConfirmation(paymentId, eventId, token);

            _paymentRepository.VerifyAll();
            _eventRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _communicationRepository.VerifyAll();
        }

        [Test]
        public void ShouldSendEmailWithDefaultTemplateId()
        {
            const int paymentId = 345;
            const int eventId = 231;
            const int emailTemplateId = 555;
            const string token = "testingapi";
            const int contactId = 8484;
            const string baseUrl = "some url.com";

            const string eventTitle = "My Awesome Event";
            const decimal paymentTotal = 56.1M;

            var me = FactoryGirl.NET.FactoryGirl.Build<MpMyContact>(m => m.Contact_ID = contactId);
            var payment = FactoryGirl.NET.FactoryGirl.Build<MpPayment>(m => { m.PaymentId = paymentId; m.PaymentTotal = paymentTotal; });
            var mpEvent = FactoryGirl.NET.FactoryGirl.Build<MpEvent>(m => {
                m.EventId = eventId;
                m.EventTitle = eventTitle;
                m.PrimaryContactId = 1234;
                m.PrimaryContact = new MpContact
                {
                    EmailAddress = "Lucille@bluth.com",
                    PreferredName = "Lucille",
                    ContactId = 1234
                };
            });

            var mergeData = new Dictionary<string, object>
            {
                {"Event_Title", mpEvent.EventTitle},
                {"Payment_Total", "56.10"},
                {"Primary_Contact_Email", mpEvent.PrimaryContact.EmailAddress },
                {"Primary_Contact_Display_Name", mpEvent.PrimaryContact.PreferredName },
                {"Base_Url",  baseUrl}
            };

            _paymentRepository.Setup(m => m.GetPaymentById(paymentId)).Returns(payment);
            _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(mpEvent);
            _contactRepository.Setup(m => m.GetMyProfile(token)).Returns(me);
            _eventRepository.Setup(m => m.GetProductEmailTemplate(eventId)).Returns(new Err<int>("Template Not Found"));

            _configWrapper.Setup(m => m.GetConfigValue("BaseUrl")).Returns(baseUrl);
            _configWrapper.Setup(m => m.GetConfigIntValue("DefaultPaymentEmailTemplate")).Returns(emailTemplateId);

            _communicationRepository.Setup(
                m =>
                    m.GetTemplateAsCommunication(emailTemplateId,
                                                 mpEvent.PrimaryContactId,
                                                 mpEvent.PrimaryContact.EmailAddress,
                                                 mpEvent.PrimaryContactId,
                                                 mpEvent.PrimaryContact.EmailAddress,
                                                 me.Contact_ID,
                                                 me.Email_Address,
                                                 mergeData));
            _communicationRepository.Setup(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false));
            _fixture.SendPaymentConfirmation(paymentId, eventId, token);

            _paymentRepository.VerifyAll();
            _eventRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _communicationRepository.VerifyAll();
            _configWrapper.VerifyAll();
        }

        [Test]
        public void ShouldSetStatusToSomePaid()
        {
            const int invoiceId = 1234;

            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(fakePayments(12, 100, 34525, defaultPaymentStatus));
            _invoiceRepository.Setup(m => m.SetInvoiceStatus(invoiceId, somePaid));

            _fixture.UpdateInvoiceStatusAfterDecline(invoiceId);
            _paymentRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
        }

        [Test]
        public void ShouldSetStatusToNonePaid()
        {
            const int invoiceId = 1234;

            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(fakePayments(12, -24, 34525, declinedPaymentStatus));
            _invoiceRepository.Setup(m => m.SetInvoiceStatus(invoiceId, nonePaid));

            _fixture.UpdateInvoiceStatusAfterDecline(invoiceId);
            _paymentRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
        }

        [Test]
        public void ShouldUseDefaultCongregationOnPayment()
        {
            var paymentDto = fakePaymentDto();
            var invoiceDetail = fakeInvoiceDetail(paymentDto.InvoiceId);

            _configWrapper.Setup(m => m.GetConfigIntValue("Congregation_Default_ID")).Returns(defaultCongregation);
            _invoiceRepository.Setup(m => m.InvoiceExists(paymentDto.InvoiceId)).Returns(true);
            _contactRepository.Setup(m => m.GetContactById(paymentDto.ContactId)).Returns(new MpMyContact()
            {
                Contact_ID = paymentDto.ContactId
            });
            _contactRepository.Setup(m => m.GetContactById(invoiceDetail.RecipientContactId)).Returns(new MpMyContact()
            {
                Congregation_ID = null
            });
            _paymentTypeRepository.Setup(m => m.PaymentTypeExists(5)).Returns(true);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailForInvoice(paymentDto.InvoiceId)).Returns(invoiceDetail);

            var retVal = new MpPaymentDetailReturn()
            {
                InvoiceDetailId = invoiceDetail.InvoiceDetailId,
                PaymentAmount = paymentDto.DonationAmt,
                PaymentDetailId = 398457,
                PaymentId = 2897234
            };

            _paymentRepository.Setup(m => m.CreatePaymentAndDetail(It.IsAny<MpPaymentDetail>())).Returns(new Result<MpPaymentDetailReturn>(true, retVal));

            var invoice = new MpInvoice()
            {
                InvoiceId = paymentDto.InvoiceId,
                InvoiceTotal = 500
            };

            var paymentsSoFar = new List<MpPayment>
            {
                new MpPayment()
                {
                    PaymentTotal = 20,
                    PaymentStatus = 3
                },
                 new MpPayment()
                {
                    PaymentTotal = 200,
                    PaymentStatus = 3
                }
            };

            _invoiceRepository.Setup(m => m.GetInvoice(invoiceDetail.InvoiceId)).Returns(invoice);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(paymentDto.InvoiceId)).Returns(paymentsSoFar);
            _invoiceRepository.Setup(m => m.SetInvoiceStatus(paymentDto.InvoiceId, somePaid));

            var val = _fixture.PostPayment(paymentDto);
            Assert.AreEqual(retVal, val);
            _invoiceRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _paymentTypeRepository.VerifyAll();
            _paymentRepository.VerifyAll();
            _configWrapper.VerifyAll();
        }

        [Test]
        public void ShouldGetInvoiceDetailWithProductInfo()
        {
            const int invoiceId = 1234;
            var invoiceDetail = fakeInvoiceDetail(invoiceId);
            var product = fakeProduct(invoiceDetail.ProductId);

            _invoiceRepository.Setup(m => m.GetInvoiceDetailForInvoice(invoiceId)).Returns(invoiceDetail);
            _productRepository.Setup(m => m.GetProduct(invoiceDetail.ProductId)).Returns(product);


            var val = _fixture.GetInvoiceDetail(invoiceId);
            Assert.AreEqual(invoiceDetail.RecipientContactId, val.RecipientContactId);
            Assert.AreEqual(product.ProductName, val.Product.ProductName);
            _invoiceRepository.VerifyAll();
            _productRepository.VerifyAll();
        }

        [Test]
        public void ShoudlDetermineCorrectEmailTemplate()
        {
          const int productId = 1234;

          var programWithTemplate = fakeProgram1(productId);
          int template = (programWithTemplate.CommunicationTemplateId ?? defaultInvoicePaymentTemplate);
          Assert.AreEqual(template, programWithTemplate.CommunicationTemplateId);

          var programNoTemplate = fakeProgram2(productId);
          int templateId = (programNoTemplate.CommunicationTemplateId ?? defaultInvoicePaymentTemplate);
          Assert.AreEqual(templateId, defaultInvoicePaymentTemplate);

          var noProgram = fakeProgram3(productId);
          int templateId2 = noProgram == null ? defaultInvoicePaymentTemplate : noProgram.CommunicationTemplateId ?? defaultInvoicePaymentTemplate;
          Assert.AreEqual(templateId2, defaultInvoicePaymentTemplate);
    }

    private static List<MpPayment> fakePayments(int payerId, decimal paymentTotal, int paymentIdOfOne = 34525, int paymentStatus = 0)
        {
            return new List<MpPayment>
            {
                new MpPayment()
                {
                    PaymentId = paymentIdOfOne,
                    ContactId = payerId,                  
                    PaymentTotal = paymentTotal,
                    PaymentStatus = paymentStatus
                }, 
                new MpPayment()
                {
                    PaymentTotal = 24.0M,
                    PaymentStatus = paymentStatus
                }
            };
        }


        private static MpDonationAndDistributionRecord fakePaymentDto(int contactId = 1234, int invoiceId = 8970)
        {
            return new MpDonationAndDistributionRecord()
            {
                DonationAmt = 120.0M,
                ContactId = contactId,
                InvoiceId = invoiceId,
                PymtType = "bank",
                ProcessorId = "some meaningless string"
            };
        }

        private static MpInvoiceDetail fakeInvoiceDetail(int invoiceId)
        {
            return new MpInvoiceDetail()
            {
                InvoiceDetailId = 1346,
                InvoiceId = invoiceId,
                RecipientContactId = 9428,
                ProductId = 1231
            };
        }

        private static MpInvoice fakeInvoice(int invoiceId, int contactId, decimal total)
        {
            return new MpInvoice()
            {
                InvoiceId = invoiceId,
                InvoiceTotal = total,
                PurchaserContactId = contactId,
                InvoiceStatusId = 4
            };
        }

        private static MpMyContact fakeMyContact(int contactId, string emailAddress)
        {
            // all I really care about is contactId and emailAddress
            return new MpMyContact()
            {
                Contact_ID = contactId,
                Email_Address = emailAddress
            };
        }

        private static MpProduct fakeProduct(int productId)
        {
            return new MpProduct()
            {
                ProductId = productId,
                ProductName = "Test Product",
            };
        }

        private static MpProgram fakeProgram1(int productId)
        {
          return new MpProgram()
          {
            CommunicationTemplateId = 987
          };
        }

        private static MpProgram fakeProgram2(int productId)
        {
          return new MpProgram()
          {
            CommunicationTemplateId = null
          };
        }

        private static MpProgram fakeProgram3(int productId)
        {
          return new MpProgram();
        }

  }
}

