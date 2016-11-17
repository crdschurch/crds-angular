﻿using System;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Repositories.Interfaces;
using PaymentType = MinistryPlatform.Translation.Enum.PaymentType;

namespace crds_angular.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IPaymentTypeRepository _paymentTypeRepository;

        private readonly int _paidinfullStatus;
        private readonly int _somepaidStatus;
        private readonly int _defaultPaymentStatus;

        public PaymentService(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository, IConfigurationWrapper configurationWrapper, IContactRepository contactRepository, IPaymentTypeRepository paymentTypeRepository)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _contactRepository = contactRepository;
            _paymentTypeRepository = paymentTypeRepository;
            
            _paidinfullStatus = configurationWrapper.GetConfigIntValue("PaidInFull");
            _somepaidStatus = configurationWrapper.GetConfigIntValue("SomePaid");
            _defaultPaymentStatus = configurationWrapper.GetConfigIntValue("DonationStatusPending");
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
                PaymentStatus = _defaultPaymentStatus
            };
            var paymentDetail = new MpPaymentDetail
            {
                Payment = payment,
                PaymentAmount = paymentRecord.DonationAmt,
                InvoiceDetailId = _invoiceRepository.GetInvoiceDetailForInvoice(paymentRecord.InvoiceId).InvoiceDetailId
                
            };

            var result = _paymentRepository.CreatePaymentAndDetail(paymentDetail);
            if (result.Status)
            {
                //update invoice payment status
                var invoice = _invoiceRepository.GetInvoice(paymentRecord.InvoiceId);
                var payments = _paymentRepository.GetPaymentsForInvoice(paymentRecord.InvoiceId);
                var paymentTotal = payments.Sum(p => p.PaymentTotal);
            
                _invoiceRepository.SetInvoiceStatus(paymentRecord.InvoiceId, paymentTotal >= invoice.InvoiceTotal ? _paidinfullStatus : _somepaidStatus);
                return result.Value;
            }
            else
            {
                throw new Exception("Unable to save payment data");
            }
        }

        public PaymentDetailDTO GetPaymentDetails(int paymentId, int invoiceId, string token)
        {
            var me = _contactRepository.GetMyProfile(token);
            var invoice = _invoiceRepository.GetInvoice(invoiceId);
            var payments = _paymentRepository.GetPaymentsForInvoice(invoiceId);
            
            var currentPayment = payments.Where(p => p.PaymentId == paymentId && p.ContactId == me.Contact_ID).ToList();

            if (currentPayment.Any())
            {
                var totalPaymentsMade = payments.Sum(p => p.PaymentTotal);
                var leftToPay = invoice.InvoiceTotal - totalPaymentsMade;
                return new PaymentDetailDTO()
                {
                    PaymentAmount = currentPayment.Any() ? currentPayment.First().PaymentTotal : 0M,
                    RecipientEmail = me.Email_Address,
                    TotalToPay = leftToPay
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
            return (_paymentRepository.UpdateDonationStatus(paymentId, statusId, statusDate ?? DateTime.Now, statusNote));
        }

        public DonationBatchDTO GetPaymentBatch(int batchId)
        {
            var batch = _paymentRepository.GetPaymentBatch(batchId);
            return new DonationBatchDTO
            {
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
    }
}