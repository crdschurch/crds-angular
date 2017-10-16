
using System;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Models.Crossroads.Stewardship;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;

namespace crds_angular.Services.Interfaces
{
    public interface IPaymentService
    {
        MpPaymentDetailReturn PostPayment(MpDonationAndDistributionRecord payment);
        PaymentDetailDTO GetPaymentDetails(int invoiceId);
        PaymentDetailDTO GetPaymentDetails(int paymentId, int invoiceId, string token, bool useInvoiceContact = false);
        PaymentDTO GetPaymentByTransactionCode(string stripePaymentId);
        int UpdatePaymentStatus(int paymentId, int statusId, DateTime? statusDate, string statusNote = null);
        DonationBatchDTO GetPaymentBatch(int batchId);
        DonationBatchDTO CreatePaymentBatch(DonationBatchDTO batch);
        int? CreatePaymentForBankAccountErrorRefund(StripeRefund refund);
        bool DepositExists(int invoiceId, string token);
        void SendPaymentConfirmation(int paymentId, int eventId, string token);
        void UpdateInvoiceStatusAfterDecline(int invoiceId);
        InvoiceDetailDTO GetInvoiceDetail(int invoiceId);
        void SendInvoicePaymentConfirmation(int paymentId, int invoiceId, string token);
    }
}