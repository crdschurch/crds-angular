using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Stewardship;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;

namespace crds_angular.Services.Interfaces
{
    public interface IDonorService
    {
        MpContactDonor GetContactDonorForEmail(string emailAddress);

        MpContactDonor GetContactDonorForAuthenticatedUser(string authToken);

        MpContactDonor GetContactDonorForDonorAccount(string accountNumber, string routingNumber);

        MpContactDonor GetContactDonorForDonorId(int donorId);
            
        MpContactDonor GetContactDonorForCheckAccount(string encryptedKey);

        MpContactDonor CreateOrUpdateContactDonor(MpContactDonor existingDonor,  string encryptedKey, string firstName, string lastName, string emailAddress, string paymentProcessorToken = null, DateTime? setupDate = null);

        string DecryptValues(string value);

        int CreateRecurringGift(string authorizedUserToken, RecurringGiftDto recurringGiftDto, MpContactDonor mpContact, string email, string displayName);

        RecurringGiftDto EditRecurringGift(string authorizedUserToken, RecurringGiftDto editGift, MpContactDonor donor);

        Boolean CancelRecurringGift(string authorizedUserToken, int recurringGiftId, bool sendEmail);

        MpCreateDonationDistDto GetRecurringGiftForSubscription(string subscriptionId);

        List<RecurringGiftDto> GetRecurringGiftsForAuthenticatedUser(string userToken);

        List<PledgeDto> GetCapitalCampaignPledgesForAuthenticatedUser(string userToken);
    }
}
