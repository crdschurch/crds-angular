using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IDonorRepository
    {
        int CreateDonorRecord(int contactId, string processorId, DateTime setupTime,
            int? statementFrequencyId = 1, // default to quarterly
            int? statementTypeId = 1, //default to individual
            int? statementMethodId = 2, // default to email/online
            MpDonorAccount mpDonorAccount = null
            );

        int CreateDonationAndDistributionRecord(MpDonationAndDistributionRecord donationAndDistribution, bool sendConfirmationEmail = true);
        MpContactDonor GetContactDonor(int contactId);
        MpContactDonor GetPossibleGuestContactDonor(string email);
        MpContactDonor GetContactDonorForDonorAccount(string accountNumber, string routingNumber);
        int UpdatePaymentProcessorCustomerId(int donorId, string paymentProcessorCustomerId);
        void SetupConfirmationEmail(int programId, int donorId, decimal donationAmount, DateTime setupDate, string pymtType, int pledgeId);
        MpContactDonor GetEmailViaDonorId(int donorId);
        void SendEmail(int emailTemplate, int donorId, decimal donationAmount, string donationType, DateTime donationDate, DateTime startDate, string programName, string emailReason, string frequency = null, string pledgeName = null);
        MpContactDonor GetContactDonorForCheckAccount(string encryptedKey);
        string CreateHashedAccountAndRoutingNumber(string accountNumber, string routingNumber);
        string DecryptCheckValue(string value);
        string UpdateDonorAccount(string encryptedKey, string customerId, string sourceId);
        List<MpDonation> GetDonations(int donorId, string donationYear = null);
        List<MpDonation> GetDonations(IEnumerable<int> donorIds, string donationYear = null);
        List<MpDonation> GetSoftCreditDonations(IEnumerable<int> donorIds, string donationYear = null);
        List<MpDonation> GetDonationsForAuthenticatedUser(string userToken, bool? softCredit = null, string donationYear = null, bool? includeRecurring = true);
        MpCreateDonationDistDto GetRecurringGiftForSubscription(string subscription, string optionalSourceId = "");
        MpCreateDonationDistDto GetRecurringGiftById(string authorizedUserToken, int recurringGiftId);
        MpCreateDonationDistDto GetRecurringGiftById(int recurringGiftId);
        int CreateRecurringGiftRecord(int donorId, int donorAccountId, string planInterval, decimal planAmount, DateTime startDate, string program, string subscriptionId, int congregationId, string sourceUrl = null, decimal? predefinedAmount = null);
        void UpdateRecurringGiftDonorAccount(int recurringGiftId, int donorAccountId);
        void CancelRecurringGift(string authorizedUserToken, int recurringGiftId);
        void CancelRecurringGift(int recurringGiftId);
        int CreateDonorAccount(string institutionName, string routingNumber, string acctNumber, string encryptedAcct, int donorId, string processorAcctId, string processorId);
        void DeleteDonorAccount(int donorAccountId);
        List<MpRecurringGift> GetRecurringGiftsForAuthenticatedUser(string userToken);
        void ProcessRecurringGiftDecline(string subscription_id, string error);
        void UpdateRecurringGiftFailureCount(int recurringGiftId, int failureCount);
        void UpdateRecurringGift(int pageView, int recurringGiftId, Dictionary<string, object> recurringGiftValues);
        int GetDonorAccountPymtType(int donorAccountId);

        MpDonorStatement GetDonorStatement(int contactId);
        void UpdateDonorStatement(MpDonorStatement statement);
    }
}
