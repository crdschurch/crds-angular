using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IDonationRepository
    {
        int UpdateDonationStatus(int donationId, int statusId, DateTime statusDate, string statusNote = null);
        int UpdateDonationStatus(string processorPaymentId, int statusId, DateTime statusDate, string statusNote = null);
        int CreateDonationBatch(string batchName, DateTime setupDateTime, decimal batchTotalAmount, int itemCount, int batchEntryType, int? depositId, DateTime finalizedDateTime, string processorTransferId);
        MpDonationBatch GetDonationBatchByProcessorTransferId(string processorTransferId);
        MpDonationBatch GetDonationBatch(int batchId);
        MpDonationBatch GetDonationBatchByDepositId(int depositId);
        MpDonation GetDonationByProcessorPaymentId(string processorPaymentId, bool retrieveDistributions = false);
        List<MpDeposit> GetSelectedDonationBatches(int selectionId); 
        void AddDonationToBatch(int batchId, int donationId);
        void ProcessDeclineEmail(string processorPaymentId);
        int CreateDeposit(string depositName, decimal depositTotalAmount, decimal depositAmount, decimal depositProcessorFee, DateTime depositDateTime, string accountNumber, int batchCount, bool exported, string notes, string processorTransferId);
        void CreatePaymentProcessorEventError(DateTime? eventDateTime, string eventId, string eventType,
            string eventMessage, string responseMessage);

        List<MpTripDistribution> GetMyTripDistributions(int contactId);
        List<MpGPExportDatum> GetGpExport(int depositId);
        void UpdateDepositToExported(int selectionId, int depositId);
        void SendMessageToDonor(int donorId, int donationDistributionId, int fromContactId, string body, string tripName);
        void SendMessageFromDonor(int pledgeId, int donationId, string message, string fromDonor);
        void FinishSendMessageFromDonor(int donationId, bool success);
        void AddDonationCommunication(int donationId, int communicationId);
        List<int> GetPredefinedDonationAmounts();
        MpDeposit GetDepositByProcessorTransferId(string processorTransferId);
        List<MpGPExportDatum> GetGPExportDataForPayments(int depositId, string token);
        int GetProcessingFeeMappingID(int programId, int congregationId, string token);
        MPGLAccountMapping GetProcessingFeeGLMapping(int processingFeeMapping, string token);
        MpDeposit GetDepositById(int depositId);
    }
}