using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Product;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        bool InvoiceExists(int invoiceId);
        bool InvoiceExistsForEventParticipant(int participantId);
        MpInvoice GetInvoice(int invoiceId);
        void SetInvoiceStatus(int invoiceId, int statusId);
        MpInvoiceDetail GetInvoiceDetailForInvoice(int invoiceId);
        bool CreateInvoiceAndDetail(int productId, int? productOptionPriceId, int purchaserContactId, int recipientContactId, int eventParticiapntId);
        void UpdateInvoiceAndDetail(int invoiceId, MpProduct product, int? productOptionPriceId, int contactId, int recipientContactId, int eventParticipantId);
        Result<MpInvoiceDetail> GetInvoiceDetailsForProductAndCamper(int productId, int camperId, int eventId);
        int GetInvoiceIdForPayment(int paymentId);
    }
}
