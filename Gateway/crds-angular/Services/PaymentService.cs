using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Product;
using MinistryPlatform.Translation.Repositories.Interfaces;
using PaymentType = MinistryPlatform.Translation.Enum.PaymentType;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(DonationService));

        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IPaymentTypeRepository _paymentTypeRepository;
        private readonly IEventRepository _eventPRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IConfigurationWrapper _configWrapper;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProgramRepository _programRepository;

        private readonly IPaymentProcessorService _paymentProcessorService;

        private readonly int _paidinfullStatus;
        private readonly int _somepaidStatus;
        private readonly int _nonePaidStatus;
        private readonly int _defaultPaymentStatus;
        private readonly int _declinedPaymentStatus;
        private readonly int _bankErrorRefundContactId;
        private readonly int _defaultPaymentEmailTemplate;

        public PaymentService(IInvoiceRepository invoiceRepository, 
            IPaymentRepository paymentRepository, 
            IConfigurationWrapper configurationWrapper, 
            IContactRepository contactRepository, 
            IPaymentTypeRepository paymentTypeRepository, 
            IEventRepository eventRepository,
            ICommunicationRepository communicationRepository,
            IApiUserRepository apiUserRepository,
            IProductRepository productRepository,
            IProgramRepository programRepository,
            IPaymentProcessorService paymentProcessorService)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _contactRepository = contactRepository;
            _paymentTypeRepository = paymentTypeRepository;
            _communicationRepository = communicationRepository;
            _configWrapper = configurationWrapper;
            _eventPRepository = eventRepository;
            _apiUserRepository = apiUserRepository;
            _productRepository = productRepository;
            _programRepository = programRepository;

            _paymentProcessorService = paymentProcessorService;

            _paidinfullStatus = configurationWrapper.GetConfigIntValue("PaidInFull");
            _somepaidStatus = configurationWrapper.GetConfigIntValue("SomePaid");
            _nonePaidStatus = configurationWrapper.GetConfigIntValue("NonePaid");
            _defaultPaymentStatus = configurationWrapper.GetConfigIntValue("DonationStatusPending");
            _declinedPaymentStatus = configurationWrapper.GetConfigIntValue("DonationStatusDeclined");
            _bankErrorRefundContactId = configurationWrapper.GetConfigIntValue("ContactIdForBankErrorRefund");
            _defaultPaymentEmailTemplate = configurationWrapper.GetConfigIntValue("DefaultInvoicePaymentEmailTemplate");
    }

        public MpPaymentDetailReturn PostPayment(MpDonationAndDistributionRecord paymentRecord)
        {
            //check if invoice exists
            if (!_invoiceRepository.InvoiceExists(paymentRecord.InvoiceId))
            {
                throw new InvoiceNotFoundException(paymentRecord.InvoiceId);
            }

            //check if contact exists
            if (_contactRepository.GetContactById(paymentRecord.ContactId) == null)
            {
                throw new ContactNotFoundException(paymentRecord.ContactId);
            }

            if (paymentRecord.ProcessorId.Length > 50)
            {
                throw new Exception("Max length of 50 exceeded for transaction code");
            }

            var pymtId = PaymentType.GetPaymentType(paymentRecord.PymtType).id;
            var fee = paymentRecord.FeeAmt.HasValue ? paymentRecord.FeeAmt / Constants.StripeDecimalConversionValue : null;

            //check if payment type exists
            if (!_paymentTypeRepository.PaymentTypeExists(pymtId))
            {
                throw new PaymentTypeNotFoundException(pymtId);
            }            

            //create payment -- send model
            var payment = new MpPayment
            {
                InvoiceNumber = paymentRecord.InvoiceId.ToString(),
                ContactId = paymentRecord.ContactId,
                TransactionCode = paymentRecord.ProcessorId,
                PaymentDate = DateTime.Now,
                PaymentTotal = paymentRecord.DonationAmt,
                PaymentTypeId = pymtId,
                PaymentStatus = _defaultPaymentStatus,
                ProcessorFeeAmount = fee
            };
            var invoiceDetail = _invoiceRepository.GetInvoiceDetailForInvoice(paymentRecord.InvoiceId);
            var paymentDetail = new MpPaymentDetail
            {
                Payment = payment,
                PaymentAmount = paymentRecord.DonationAmt,
                InvoiceDetailId = invoiceDetail.InvoiceDetailId,
                CongregationId = _contactRepository.GetContactById(invoiceDetail.RecipientContactId).Congregation_ID ?? _configWrapper.GetConfigIntValue("Congregation_Default_ID")
            };

            var result = _paymentRepository.CreatePaymentAndDetail(paymentDetail);
            if (result.Status)
            {
                //update invoice payment status
                var invoice = _invoiceRepository.GetInvoice(paymentRecord.InvoiceId);
                var payments = _paymentRepository.GetPaymentsForInvoice(paymentRecord.InvoiceId);
                payments = payments.Where(p => p.PaymentStatus != _declinedPaymentStatus).ToList();
                var paymentTotal = payments.Sum(p => p.PaymentTotal);
            
                _invoiceRepository.SetInvoiceStatus(paymentRecord.InvoiceId, paymentTotal >= invoice.InvoiceTotal ? _paidinfullStatus : _somepaidStatus);
                return result.Value;
            }
            else
            {
                throw new Exception("Unable to save payment data");
            }
        }

        public PaymentDetailDTO GetPaymentDetails(int invoiceId)
        {
            return GetPaymentDetails(0, invoiceId, null, true);
        }

        public PaymentDetailDTO GetPaymentDetails(int paymentId, int invoiceId, AuthDTO token, bool useInvoiceContact = false)
        {

            var invoice = _invoiceRepository.GetInvoice(invoiceId);
            var me = new MpMyContact();
             if (useInvoiceContact)
            {
                me = _contactRepository.GetContactById(invoice.PurchaserContactId);
            }
            else
            {
                me = _contactRepository.GetMyProfile(token);
            }

            var payments = _paymentRepository.GetPaymentsForInvoice(invoiceId);
            
            var currentPayment = payments.Where(p => p.PaymentId == paymentId && p.ContactId == me.Contact_ID).ToList();

            if (currentPayment.Any() || paymentId == 0)
            {
                var totalPaymentsMade = payments.Sum(p => p.PaymentTotal);
                var leftToPay = invoice.InvoiceTotal - totalPaymentsMade;
                StripeCharge charge = null;
                if (payments.Count > 0)
                {
                    charge = _paymentProcessorService.GetCharge(payments.First().TransactionCode);
                }
                return new PaymentDetailDTO()
                {
                    PaymentAmount = currentPayment.Any() ? currentPayment.First().PaymentTotal : 0M,
                    RecipientEmail = me.Email_Address,
                    TotalToPay = leftToPay,
                    InvoiceTotal = invoice.InvoiceTotal,
                    RecentPaymentId = payments.Any() ? payments.First().PaymentId : 0,
                    RecentPaymentAmount = payments.Any() ? payments.First().PaymentTotal : 0,
                    RecentPaymentLastFour = charge != null ? charge.Source?.AccountNumberLast4 : ""
                };
            }
            throw new Exception("No Payment found for " + me.Email_Address + " with id " + paymentId);
        }


        public DonationBatchDTO CreatePaymentBatch(DonationBatchDTO batch)
        {
            var batchId = _paymentRepository.CreatePaymentBatch(batch.BatchName, batch.SetupDateTime, batch.BatchTotalAmount, batch.ItemCount, batch.BatchEntryType, batch.DepositId, batch.FinalizedDateTime, batch.ProcessorTransferId);

            batch.Id = batchId;

            foreach (var payment in batch.Payments)
            {
                _paymentRepository.AddPaymentToBatch(batchId, payment.PaymentId);
            }

            return (batch);
        }

        public PaymentDTO GetPaymentByTransactionCode(string stripePaymentId)
        {
            try
            {
                var payment = _paymentRepository.GetPaymentByTransactionCode(stripePaymentId);

                return new PaymentDTO
                {
                    Amount = (double) payment.PaymentTotal,
                    ContactId = payment.ContactId,
                    InvoiceId = int.Parse(payment.InvoiceNumber),
                    PaymentId = payment.PaymentId,
                    PaymentTypeId = payment.PaymentTypeId,
                    StripeTransactionId = payment.TransactionCode,
                    BatchId = payment.BatchId
                };
            }
            catch (Exception)
            {
                throw new PaymentNotFoundException(stripePaymentId);
            }
        }

        public int UpdatePaymentStatus(int paymentId, int statusId, DateTime? statusDate, string statusNote = null)
        {
            var retVal = _paymentRepository.UpdatePaymentStatus(paymentId, statusId);
            if (statusId == _declinedPaymentStatus)
            {
                var invoiceId = _invoiceRepository.GetInvoiceIdForPayment(paymentId);
                UpdateInvoiceStatusAfterDecline(invoiceId);
            }
            return retVal;
        }

        public DonationBatchDTO GetPaymentBatch(int batchId)
        {
            var batch = _paymentRepository.GetPaymentBatch(batchId);
            return new DonationBatchDTO
            {
                Id = batch.BatchId,
                BatchEntryType = batch.BatchEntryTypeId,
                BatchName = batch.BatchName,
                BatchTotalAmount = batch.BatchTotal,
                DepositId = batch.DepositId,
                FinalizedDateTime = batch.FinalizeDate,
                ItemCount = batch.ItemCount,
                ProcessorTransferId = batch.ProcessorTransferId,
                SetupDateTime = batch.SetupDate
            };
        }

        public int? CreatePaymentForBankAccountErrorRefund(StripeRefund refund)
        {
            if (refund.Data[0].BalanceTransaction == null || !"payment_failure_refund".Equals(refund.Data[0].BalanceTransaction.Type))
            {
                _logger.Error($"Balance transaction was not set, or was not a payment_failure_refund for refund ID {refund.Data[0].Id}");
                return (null);
            }

            if (string.IsNullOrWhiteSpace(refund.Data[0].Charge?.Id))
            {
                _logger.Error($"No associated Charge for Refund {refund.Data[0].Id}");
                return (null);
            }

            MpPayment payment;
            try
            {
                payment = _paymentRepository.GetPaymentByTransactionCode(refund.Data[0].Charge.Id);
            }
            catch (PaymentNotFoundException)
            {
                _logger.Error($"No Payment with payment processor ID {refund.Data[0].Charge.Id} in MP for Refund {refund.Data[0].Id}");
                return (null);
            }
            _paymentRepository.UpdatePaymentStatus(payment.PaymentId, (int) DonationStatus.Declined); // update the original payment to declined 
            var paymentReverse = new MpPayment
            {
                InvoiceNumber = payment.InvoiceNumber,
                PaymentDate = DateTime.Now,
                PaymentStatus = (int) DonationStatus.Declined,
                ContactId = _bankErrorRefundContactId, 
                ProcessorFeeAmount = refund.Data[0].BalanceTransaction.Fee / Constants.StripeDecimalConversionValue,
                Notes = $"Reversed from PaymentID {payment.PaymentId}",
                PaymentTypeId = payment.PaymentTypeId,
                TransactionCode = refund.Data[0].Id,
                PaymentTotal = -(int.Parse(refund.Data[0].Amount) / Constants.StripeDecimalConversionValue),
                BatchId = payment.BatchId
            };

            var invoicedetail = _invoiceRepository.GetInvoiceDetailForInvoice(Convert.ToInt32(payment.InvoiceNumber));

            var detail = new MpPaymentDetail
            {
                PaymentAmount = -(int.Parse(refund.Data[0].Amount)/Constants.StripeDecimalConversionValue),
                InvoiceDetailId = invoicedetail.InvoiceDetailId,
                Payment = paymentReverse,
                CongregationId = _contactRepository.GetContactById(invoicedetail.RecipientContactId).Congregation_ID ?? _configWrapper.GetConfigIntValue("Congregation_Default_ID")
            };
            return _paymentRepository.CreatePaymentAndDetail(detail).Value.PaymentId;
        }

        public void UpdateInvoiceStatusAfterDecline(int invoiceId)
        {
            var payments = _paymentRepository.GetPaymentsForInvoice(invoiceId);
            var paymentTotal = payments.Sum(p => p.PaymentTotal);

            _invoiceRepository.SetInvoiceStatus(invoiceId, paymentTotal > 0 ? _somepaidStatus : _nonePaidStatus);
        }

        public bool DepositExists(int invoiceId, AuthDTO token)
        {
            var me = _contactRepository.GetMyProfile(token);
            var payments = _paymentRepository.GetPaymentsForInvoice(invoiceId);
            payments = payments.Where(p => p.ContactId == me.Contact_ID).ToList();
            return payments.Any();
        }

        public void SendPaymentConfirmation(int paymentId , int eventId , AuthDTO token )
        {
            var payment = _paymentRepository.GetPaymentById(paymentId);
            var mpEvent = _eventPRepository.GetEvent(eventId);
            var me = _contactRepository.GetMyProfile(token);

            var templateIdResult = _eventPRepository.GetProductEmailTemplate(eventId);
            var templateId = (templateIdResult.Status) ? templateIdResult.Value : _configWrapper.GetConfigIntValue("DefaultPaymentEmailTemplate");
            var mergeData = new Dictionary<string, object>
            {
                {"Event_Title", mpEvent.EventTitle},
                {"Payment_Total", payment.PaymentTotal.ToString(".00") },
                {"Primary_Contact_Email", mpEvent.PrimaryContact.EmailAddress },
                {"Primary_Contact_Display_Name", mpEvent.PrimaryContact.PreferredName},
                {"Base_Url", _configWrapper.GetConfigValue("BaseUrl") }
            };

            var comm = _communicationRepository.GetTemplateAsCommunication(templateId,
                                                                mpEvent.PrimaryContactId,
                                                                mpEvent.PrimaryContact.EmailAddress,
                                                                mpEvent.PrimaryContactId,
                                                                mpEvent.PrimaryContact.EmailAddress,
                                                                me.Contact_ID,
                                                                me.Email_Address,
                                                                mergeData);
            _communicationRepository.SendMessage(comm);
        }
    
        public InvoiceDetailDTO GetInvoiceDetail(int invoiceId)
        {
            var invoiceDetail = Mapper.Map<InvoiceDetailDTO>(_invoiceRepository.GetInvoiceDetailForInvoice(invoiceId));
            invoiceDetail.Product = Mapper.Map<ProductDTO>(_productRepository.GetProduct(invoiceDetail.ProductId));

            return invoiceDetail;
        }

        public void SendInvoicePaymentConfirmation(int paymentId, int invoiceId, AuthDTO token)
        {
            var payment = _paymentRepository.GetPaymentById(paymentId);
            var me = _contactRepository.GetMyProfile(token);
            var invoiceDetail = Mapper.Map<MpInvoiceDetail, InvoiceDetailDTO>(_invoiceRepository.GetInvoiceDetailForInvoice(invoiceId));
            var product = _productRepository.GetProduct(invoiceDetail.ProductId);
            invoiceDetail.Product = Mapper.Map<MpProduct, ProductDTO>(product);
            var mpProgram = product.ProgramId != null ? _programRepository.GetProgramById((int)product.ProgramId) : null ;

            int templateId = mpProgram == null ? _defaultPaymentEmailTemplate : mpProgram.CommunicationTemplateId ?? _defaultPaymentEmailTemplate;

            var primaryContactId = _configWrapper.GetConfigIntValue("CrossroadsFinanceClerkContactId");
            var primaryContact = _contactRepository.GetContactById(primaryContactId);

            var mergeData = new Dictionary<string, object>
            {
              {"Product_Name", invoiceDetail.Product.ProductName},
              {"Payment_Total", payment.PaymentTotal.ToString(".00") },
              {"Primary_Contact_Email", primaryContact.Email_Address },
              {"Primary_Contact_Display_Name", primaryContact.Display_Name}
            };

            var comm = _communicationRepository.GetTemplateAsCommunication(templateId,
              me.Contact_ID,
              me.Email_Address,
              mergeData);
            _communicationRepository.SendMessage(comm);
        }
    }
}
