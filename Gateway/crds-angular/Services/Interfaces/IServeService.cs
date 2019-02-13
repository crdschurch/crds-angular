using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Opportunity;
using crds_angular.Models.Crossroads.Serve;
using MinistryPlatform.Translation.Models;


namespace crds_angular.Services.Interfaces
{
    public interface IServeService
    {
        List<int> GetUpdatedOpportunities(string token, SaveRsvpDto dto, Func<MpParticipant, MpEvent, Boolean> saveFunc = null);
        List<FamilyMember> GetImmediateFamilyParticipants(int contactId);
        List<FamilyMember> GetImmediateFamilyParticipants(string token);
        DateTime GetLastServingDate(int opportunityId, string token);
        List<QualifiedServerDto> GetQualifiedServers(int groupId, int opportunityId, int contactId);
        List<QualifiedServerDto> GetQualifiedServers(int groupId, int opportunityId, string token);
        List<ServingDay> GetServingDays(string token, int contactId, long from, long to);
        Capacity OpportunityCapacity(int opportunityId, int eventId, int? minNeeded, int? maxNeeded);
        List<int> SaveServeRsvp(string token, SaveRsvpDto dto);
        void SendReminderEmails();
        List<GroupContactDTO> PotentialVolunteers(int groupId, crds_angular.Models.Crossroads.Events.Event evt, List<MpGroupParticipant> groupMembers );
        List<GroupDTO> GetLeaderGroups(string token);
        ServingTeam GetServingTeamRsvps(ServingTeam team);
        bool GetIsLeader(string token, int? groupId);
        List<GroupParticipantDTO> GetLeaderGroupsParticipants(string token, int? groupId);
    }
}