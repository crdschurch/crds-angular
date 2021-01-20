using System;
using System.Collections.Generic;
using crds_angular.Models;
using crds_angular.Models.Crossroads.Childcare;
using crds_angular.Models.Crossroads.Serve;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;

namespace crds_angular.Services.Interfaces
{
    public interface IChildcareService

    {
        void SendRequestForRsvp();
        List<FamilyMember> MyChildren(int contactId);
        void SaveRsvp(ChildcareRsvpDto saveRsvp);
        void CreateChildcareRequest(ChildcareRequestDto request);
        void ApproveChildcareRequest(int childcareRequestId, ChildcareRequestDto childcareRequest);
        MpChildcareRequest GetChildcareRequestForReview(int childcareRequestId);
        void RejectChildcareRequest(int requestId, ChildcareRequestDto childcareRequest);
        ChildcareDashboardDto GetChildcareDashboard(int contactId, int householdId, HouseHoldData houseHoldData);
        void CancelRsvp(ChildcareRsvpDto cancelRsvp);
        HouseHoldData GetHeadsOfHousehold(int contactId, int householdId);
        void SendChildcareCancellationNotification();
        void UpdateChildcareRequest(ChildcareRequestDto request);
        List<ChildCareDate> UpdateAvailableChildCareDates(List<ChildCareDate> currentDates, DateTime dateToAdd, bool hasBeenCancelled);
        void SendChildcareReminders();
        MpCommunication SetupChilcareReminderCommunication(MpContact recipient, Dictionary<string, object> mergeData);
        Dictionary<string, object> SetMergeDataForChildcareReminder(MpContact toContact, DateTime threeDaysOut);
    }
}
