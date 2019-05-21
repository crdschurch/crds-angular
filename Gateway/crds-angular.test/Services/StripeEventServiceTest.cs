using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models.DTO;
using Moq;
using MvcContrib;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp.Extensions;
using IDonationService = crds_angular.Services.Interfaces.IDonationService;
using Constants = Crossroads.Utilities.Constants;
using System.Text.RegularExpressions;

namespace crds_angular.test.Services
{
    public class StripeEventServiceTest
    {
        private StripeEventService _fixture;
        private Mock<IPaymentProcessorService> _paymentProcessorService;
        private Mock<IDonationService> _donationService;
        private Mock<IPaymentService> _paymentService;
        private Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository> _mpDonorService;

        [SetUp]
        public void SetUp()
        {
            var configuration = new Mock<IConfigurationWrapper>();
            configuration.Setup(mocked => mocked.GetConfigIntValue("DonationStatusDeposited")).Returns(999);
            configuration.Setup(mocked => mocked.GetConfigIntValue("DonationStatusSucceeded")).Returns(888);
            configuration.Setup(mocked => mocked.GetConfigIntValue("DonationStatusDeclined")).Returns(777);
            configuration.Setup(mocked => mocked.GetConfigIntValue("BatchEntryTypePaymentProcessor")).Returns(555);

            _paymentService = new Mock<IPaymentService>(MockBehavior.Strict);
            _paymentProcessorService = new Mock<IPaymentProcessorService>(MockBehavior.Strict);
            _donationService = new Mock<IDonationService>(MockBehavior.Strict);
            _mpDonorService = new Mock<MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository>(MockBehavior.Strict);

            _fixture = new StripeEventService(_paymentProcessorService.Object, _donationService.Object, _paymentService.Object, _mpDonorService.Object, configuration.Object);
        }

        [Test]
        public void TestProcessStripeEventNoMatchingEventHandler()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "not.this.one"
            };

            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestChargeSucceeded()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "charge.succeeded",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeCharge
                    {
                        Id = "9876"
                    })
                }
            };

            _donationService.Setup(mocked => mocked.UpdateDonationStatus("9876", 888, e.Created, null)).Returns(123);
            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestChargeFailed()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "charge.failed",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeCharge
                    {
                        Id = "9876",
                        FailureCode = "invalid_routing_number",
                        FailureMessage = "description from stripe"
                    })
                }
            };

            _donationService.Setup(mocked => mocked.UpdateDonationStatus("9876", 777, e.Created, "invalid_routing_number: description from stripe")).Returns(123);
            _donationService.Setup(mocked => mocked.ProcessDeclineEmail("9876"));
            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestChargeFailedBankAccountError()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "charge.failed",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeCharge
                    {
                        Id = "9876",
                        FailureCode = "invalid_routing_number",
                        FailureMessage = "description from stripe",
                        Source = new StripeSource()
                        {
                            Object = "bank_account"
                        },
                        Refunds = new StripeList<StripeRefund>
                        {
                            Data = new List<StripeRefund>
                            {
                                new StripeRefund
                                {
                                    Id = "re999"
                                }
                            }
                        }
                    })
                }
            };

            var stripeRefund = new StripeRefundData();

            _donationService.Setup(mocked => mocked.UpdateDonationStatus("9876", 777, e.Created, "invalid_routing_number: description from stripe")).Returns(123);
            _donationService.Setup(mocked => mocked.ProcessDeclineEmail("9876"));
            _paymentProcessorService.Setup(mocked => mocked.GetRefund("re999")).Returns(stripeRefund);
            _donationService.Setup(mocked => mocked.CreateDonationForBankAccountErrorRefund(It.Is<StripeRefund>(r => r.Data != null && r.Data.Any() && r.Data[0] == stripeRefund)))
                .Returns(123);

            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestTransferPaidNoChargesFound()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "transfer.paid",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeTransfer
                    {
                        Id = "tx9876",
                    })
                }
            };

            _donationService.Setup(mocked => mocked.GetDepositByProcessorTransferId("tx9876")).Returns((DepositDTO)null);
            _paymentProcessorService.Setup(mocked => mocked.GetChargesForTransfer("tx9876")).Returns(new List<StripeCharge>());


       
            var result = _fixture.ProcessStripeEvent(e);
            Assert.IsNotNull(_fixture.ProcessStripeEvent(e));
            Assert.IsInstanceOf<TransferPaidResponseDTO>(result);
            var tp = (TransferPaidResponseDTO)result;
            Assert.AreEqual(0, tp.TotalTransactionCount);
            Assert.AreEqual(0, tp.SuccessfulUpdates.Count);
            Assert.AreEqual(0, tp.FailedUpdates.Count);

            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestTransferPaid()
        {
            var transfer = new StripeTransfer
            {
                Id = "tx9876",
                Amount = 1443
            };

            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "transfer.paid",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(transfer)
                }
            };

            var paymentmetadata = new Dictionary<string, object> {{"crossroads_transaction_type", "payment"}};
            var donationmetadata = new Dictionary<string, object> { { "crossroads_transaction_type", "donation" } };

            var dateCreated = DateTime.Now;
            var charges = new List<StripeCharge>
            {
                new StripeCharge
                {
                    Id = "ch111",
                    Amount = 111,
                    Fee = 1,
                    Type = "charge",
                    Created = dateCreated.AddDays(1),
                    Metadata = paymentmetadata
                },
                new StripeCharge
                {
                    Id = "ch222",
                    Amount = 222,
                    Fee = 2,
                    Type = "charge",
                    Created = dateCreated.AddDays(2),
                    Metadata = paymentmetadata
                },
                new StripeCharge
                {
                    Id = "ch333",
                    Amount = 333,
                    Fee = 3,
                    Type = "charge",
                    Created = dateCreated.AddDays(3),
                    Metadata = donationmetadata
                },
                new StripeCharge
                {
                    Id = "ch777",
                    Amount = 777, 
                    Fee = 7,
                    Type = "charge",
                    Created = dateCreated.AddDays(4),
                    Metadata = donationmetadata
                },
                new StripeCharge
                {
                    Id = "ch888",
                    Amount = 888, 
                    Fee = 8,
                    Type = "charge",
                    Created = dateCreated.AddDays(5),
                    Metadata = donationmetadata
                },
                new StripeCharge
                {
                    Id = "re999",
                    Amount = 999,
                    Fee = 9,
                    Type = "payment_refund",
                    Created = dateCreated.AddDays(8),
                    Metadata = donationmetadata
                },
                new StripeCharge
                {
                    Id = "ch444",
                    Amount = 444,
                    Fee = 4,
                    Type = "charge",
                    Created = dateCreated.AddDays(6),
                    Metadata = donationmetadata
                },
                new StripeCharge
                {
                    Id = "ch555",
                    Amount = 555,
                    Fee = 5,
                    Type = "refund",
                    Created = dateCreated.AddDays(7),
                    Metadata = donationmetadata
                }
            };

           
            _paymentService.Setup(mocked => mocked.GetPaymentByTransactionCode("ch111")).Returns(new PaymentDTO
            {
                PaymentId = 1111,
                BatchId = null,
                Status = DonationStatus.Pending
            });

            _paymentService.Setup(mocked => mocked.GetPaymentByTransactionCode("ch222")).Returns(new PaymentDTO
            {
                PaymentId = 2222,
                BatchId = null,
                Status = DonationStatus.Pending
            });

            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("ch333")).Returns(new DonationDTO
            {
                Id = "3333",
                BatchId = null,
                Status = DonationStatus.Pending
            });

            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("ch444")).Throws(new Exception("Not gonna do it, wouldn't be prudent."));

            _paymentProcessorService.Setup(mocked => mocked.GetChargeRefund("ch555")).Returns(new StripeRefund
            {
                Data = new List<StripeRefundData>
                { new StripeRefundData()
                    {
                        Id = "ch555",
                        Amount = "987",
                        Charge = new StripeCharge {
                            Id = "re_123456"
                        }
                    }
                }
            });

            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("ch555")).Returns(new DonationDTO
            {
                Id = "5555",
                BatchId = 1984
            });
            _donationService.Setup(mocked => mocked.GetDonationBatch(1984)).Returns(new DonationBatchDTO
            {
                Id = 5150,
                ProcessorTransferId = "OU812"
            });

            var stripeRefundData = new StripeRefundData
            {
                Id = "re999",
                Amount = "999",
                ChargeId = "ch999",
                Charge = new StripeCharge
                {
                    Id = "ch999"
                },
                BalanceTransaction = new StripeBalanceTransaction
                {
                   Created = DateTime.Now
                }
            };
            var refund = new StripeRefund
            {
                Data = new List<StripeRefundData>
                {
                    stripeRefundData
                }
            };
            _paymentProcessorService.Setup(mocked => mocked.GetRefund("re999")).Returns(stripeRefundData);

            _donationService.Setup(
                mocked => mocked.CreateDonationForBankAccountErrorRefund(It.Is<StripeRefund>(r => r.Data != null && r.Data.Count == 1 && r.Data[0].Equals(stripeRefundData))))
                .Returns(876);

            var firstRefund = true;
            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("re999")).Returns(() =>
            {
                if (firstRefund)
                {
                    firstRefund = false;
                    throw (new DonationNotFoundException("re999"));
                }

                return (new DonationDTO()
                {
                    Id = "9999",
                    BatchId = null,
                    Amount = 999
                });
            });

            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("ch777")).Returns(new DonationDTO
            {
                Id = "7777",
                BatchId = 2112
            });

            _donationService.Setup(mocked => mocked.GetDonationBatch(2112)).Returns(new DonationBatchDTO
            {
                Id = 2112,
                ProcessorTransferId = null
            });

            var first = true;
            _donationService.Setup(mocked => mocked.GetDonationByProcessorPaymentId("ch888")).Returns(() =>
            {
                if (first)
                {
                    first = false;
                    throw (new DonationNotFoundException("ch888"));
                }

                return (new DonationDTO()
                {
                    Id = "8888",
                    BatchId = null,
                    Amount = 888,
                    Status = DonationStatus.Declined
                });
            });

            _donationService.Setup(mocked => mocked.UpdateDonationStatus(refund.Data[0].ChargeId, 777, refund.Data[0].BalanceTransaction.Created, null)).Returns(9999);

            var invoice = new StripeInvoice
            {
                Amount = 100,
                Id = "in_888"
            };
            _paymentProcessorService.Setup(mocked => mocked.GetCharge("ch888")).Returns(new StripeCharge
            {
                Source = new StripeSource
                {
                    Object = "bank_account"
                },
                Invoice = invoice
            });

            _donationService.Setup(mocked => mocked.CreateDonationForInvoice(invoice)).Returns(88);

            _paymentProcessorService.Setup(mocked => mocked.GetChargesForTransfer("tx9876")).Returns(charges);
            _donationService.Setup(mocked => mocked.GetDepositByProcessorTransferId("tx9876")).Returns((DepositDTO)null);

            _donationService.Setup(
                mocked => mocked.CreatePaymentProcessorEventError(e, It.IsAny<StripeEventResponseDTO>()));
            _paymentService.Setup(mocked => mocked.UpdatePaymentStatus(1111, 999, e.Created, null)).Returns(1111);
            _paymentService.Setup(mocked => mocked.UpdatePaymentStatus(2222, 999, e.Created, null)).Returns(2222);
            _donationService.Setup(mocked => mocked.UpdateDonationStatus(3333, 999, e.Created, null)).Returns(3333);
            _donationService.Setup(mocked => mocked.UpdateDonationStatus(7777, 999, e.Created, null)).Returns(7777);
            _donationService.Setup(mocked => mocked.UpdateDonationStatus(9999, 999, e.Created, null)).Returns(9999);
            _donationService.Setup(mocked => mocked.CreateDeposit(It.IsAny<DepositDTO>())).Returns(
                (DepositDTO o) =>
                {
                    o.Id = 98765;
                    return (o);
                });
            _donationService.Setup(mocked => mocked.CreateDonationBatch(It.IsAny<DonationBatchDTO>())).Returns((DonationBatchDTO o) => o);
            _paymentService.Setup(mocked => mocked.CreatePaymentBatch(It.IsAny<DonationBatchDTO>())).Returns((DonationBatchDTO o) => o);

            var result = _fixture.ProcessStripeEvent(e);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<TransferPaidResponseDTO>(result);
            var tp = (TransferPaidResponseDTO)result;
            Assert.AreEqual(8, tp.TotalTransactionCount);
            Assert.AreEqual(6, tp.SuccessfulUpdates.Count);
            Assert.AreEqual(charges.Take(6).Select(charge => charge.Id), tp.SuccessfulUpdates);
            Assert.AreEqual(2, tp.FailedUpdates.Count);
            Assert.AreEqual("ch555", tp.FailedUpdates[1].Key);
            Assert.AreEqual("ch444", tp.FailedUpdates[0].Key);
            Assert.AreEqual("Not gonna do it, wouldn't be prudent.", tp.FailedUpdates[0].Value);
            Assert.IsNotNull(tp.Batch);
            Assert.IsNotNull(tp.Deposit);
            Assert.IsNotNull(tp.Exception);

            _donationService.Verify(mocked => mocked.CreateDonationBatch(It.Is<DonationBatchDTO>(o =>
                Regex.Match(o.BatchName, @"MP\d{12}D").Success &&
                o.SetupDateTime == o.FinalizedDateTime &&
                o.BatchEntryType == 555 &&
                o.ItemCount == 4 &&
                o.BatchTotalAmount == ((333 + 777 + 888 + 999) / Constants.StripeDecimalConversionValue) &&
                o.Donations != null &&
                o.Donations.Count == 4 &&
                o.DepositId == 98765 &&
                o.ProcessorTransferId.Equals("tx9876")
            )));

            _donationService.Verify(mocked => mocked.CreateDeposit(It.Is<DepositDTO>(o =>
                Regex.Match(o.DepositName, @"MP\d{12}").Success &&
                !o.Exported &&
                o.AccountNumber.Equals(" ") &&
                o.BatchCount == 2 &&
                o.DepositDateTime != null &&
                o.DepositTotalAmount == ((transfer.Amount + 30) / Constants.StripeDecimalConversionValue) &&
                o.ProcessorFeeTotal == (30 / Constants.StripeDecimalConversionValue) &&
                o.DepositAmount == (transfer.Amount / Constants.StripeDecimalConversionValue) &&
                o.Notes == null &&
                o.ProcessorTransferId.Equals("tx9876")
            )));

            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestInvoicePaymentSucceeded()
        {
            var invoice = new StripeInvoice
            {
                Subscription = "sub_123",
                Amount = 123,
                Charge = "ch_123",
            };

            _donationService.Setup(mocked => mocked.CreateDonationForInvoice(invoice)).Returns(987);

            _fixture.InvoicePaymentSucceeded(DateTime.Now, invoice);
            _paymentProcessorService.VerifyAll();
            _donationService.VerifyAll();
        }

        [Test]
        public void TestInvoicePaymentFailedNoCancel()
        {
            const string processorId = "cus_123";
            const string id = "9876";
            const string subscriptionId = "sub_123";
            const string chargeId = "ch_2468";
            const int recurringGiftId = 123456;

            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "invoice.payment_failed",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeInvoice()
                    {
                        Id = id,
                        Customer = processorId,
                        Charge = chargeId,
                        Subscription = subscriptionId
                    })
                }
            };

            var gift = new MpCreateDonationDistDto
            {
                Frequency = 1,
                RecurringGiftId = recurringGiftId,
                StripeCustomerId = processorId
            };


            var charge = new StripeCharge
            {
                Id = chargeId,
                FailureMessage = "Your card was declined.",
                FailureCode = "card_declined"
            };

            _paymentProcessorService.Setup(m => m.GetCharge(It.IsAny<string>())).Returns(charge);
            _mpDonorService.Setup(mocked => mocked.ProcessRecurringGiftDecline(subscriptionId, "card_declined: Your card was declined."));
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftForSubscription(subscriptionId, "")).Returns(gift);
           
            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _fixture.ProcessStripeEvent(e);
            _mpDonorService.VerifyAll();
        }

        [Test]
        public void TestInvoicePaymentFailedCancelPlanAndSubscription()
        {
            const string processorId = "cus_123";
            const string donorAccountProcessorId = "cus_456";
            const string subscriptionId = "sub_123";
            const int failCount = 3;
            const int recurringGiftId = 123456;
            const int donorId = 3421;
            const int frequency = 2;
            const string id = "9876";
            const string chargeId = "ch_2468";
            const string planId = "Donor ID #3421 weekly"; 

            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "invoice.payment_failed",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeInvoice()
                    {
                        Id = id,
                        Customer = processorId,
                        Charge = chargeId,
                        Subscription = subscriptionId
                    })
                }
            };

            var gift = new MpCreateDonationDistDto
            {
                Frequency = frequency,
                RecurringGiftId = recurringGiftId,
                SubscriptionId = subscriptionId,
                ConsecutiveFailureCount =  failCount,
                DonorId =  donorId,
                StripeCustomerId = donorAccountProcessorId
            };

             var plan = new StripePlan
             {
                 Id = planId
             };

             var subscription = new StripeSubscription
             {
                 Plan = plan
             };

            var charge = new StripeCharge
            {
                Id = chargeId,
                FailureMessage = "Your card was declined.",
                FailureCode = "card_declined"
            };

            _mpDonorService.Setup(mocked => mocked.ProcessRecurringGiftDecline(subscriptionId, "card_declined: Your card was declined."));
            _paymentProcessorService.Setup(m => m.GetCharge(It.IsAny<string>())).Returns(charge);
            _mpDonorService.Setup(mocked => mocked.GetRecurringGiftForSubscription(subscriptionId, "")).Returns(gift);
            _paymentProcessorService.Setup(mocked => mocked.CancelSubscription(donorAccountProcessorId, subscriptionId)).Returns(subscription);
            _paymentProcessorService.Setup(mocked => mocked.CancelPlan(subscription.Plan.Id)).Returns(plan);
            _mpDonorService.Setup(mocked => mocked.CancelRecurringGift(recurringGiftId));
            
            Assert.IsNull(_fixture.ProcessStripeEvent(e));
            _fixture.ProcessStripeEvent(e);
            _mpDonorService.VerifyAll();
            _paymentProcessorService.VerifyAll();
        }
    }
}
