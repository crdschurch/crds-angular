using System;
using System.Collections.Generic;
using System.IO;
using crds_angular.Models.Crossroads.Stewardship;

namespace crds_angular.Services.Interfaces
{
    public interface IDonationService
    {
        int UpdateDonationStatus(int donationId, int statusId, DateTime? statusDate, string statusNote = null);
        int UpdateDonationStatus(string processorPaymentId, int statusId, DateTime? statusDate, string statusNote = null);
        DonationDTO GetDonationByProcessorPaymentId(string processorPaymentId);
        DonationBatchDTO CreateDonationBatch(DonationBatchDTO batch);
        DonationBatchDTO GetDonationBatchByDepositId(int depositId);
        List<DepositDTO> GetSelectedDonationBatches(int selectionId);
        void ProcessDeclineEmail(string processorPaymentId);
        DepositDTO CreateDeposit(DepositDTO deposit);
        void CreatePaymentProcessorEventError(StripeEvent stripeEvent, StripeEventResponseDTO stripeEventResponse);
        DonationBatchDTO GetDonationBatch(int batchId);
        DonationsDTO GetDonationsForAuthenticatedUser(string userToken, string donationYear = null, int? limit = null, bool? softCredit = null, bool? includeRecurring = true);
        DonationYearsDTO GetDonationYearsForAuthenticatedUser(string userToken);
        DonationsDTO GetDonationsForDonor(int donorId, string donationYear = null, bool softCredit = false);
        DonationYearsDTO GetDonationYearsForDonor(int donorId);
        int? CreateDonationForInvoice(StripeInvoice invoice);
        int? CreateDonationForBankAccountErrorRefund(StripeRefund refund);
        
            // ReSharper disable once InconsistentNaming
        List<GPExportDatumDTO> GetGpExport(int depositId);
        // ReSharper disable once InconsistentNaming
        MemoryStream CreateGPExport(int selectionId, int depositId);
        // ReSharper disable once InconsistentNaming
        string GPExportFileName(DepositDTO deposit);
        // ReSharper disable once InconsistentNaming
        List<DepositDTO> GenerateGPExportFileNames(int selectionId);

        void SendMessageToDonor(int donorId, int donationDistributionId, int fromContactId, string body, string tripName);
        DepositDTO GetDepositByProcessorTransferId(string processorTransferId);
        DepositDTO GetDepositById(int depositId);
    }
}