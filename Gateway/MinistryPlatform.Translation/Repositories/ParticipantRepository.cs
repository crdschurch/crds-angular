using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ParticipantRepository : BaseRepository, IParticipantRepository
    {
        public const string UpdateHuddleGroupParticipantStatusProc = "crds_Huddle_Participant_Status_Refresh";
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public ParticipantRepository(IMinistryPlatformService ministryPlatformService, IMinistryPlatformRestRepository ministryPlatformRestRepository, IAuthenticationRepository authenticationService , IConfigurationWrapper configurationWrapper)
            : base(authenticationService, configurationWrapper)
        {
            this._ministryPlatformService = ministryPlatformService;
            this._ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public int CreateParticipantRecord(int contactId)
        {
            var token = ApiLogin();
            var pageId = _configurationWrapper.GetConfigIntValue("Participants");

            var participantDictionary = new Dictionary<string, object>();
            participantDictionary["Participant_Type_ID"] = _configurationWrapper.GetConfigIntValue("Participant_Type_Default_ID");
            participantDictionary["Participant_Start_Date"] = DateTime.Now;
            participantDictionary["Contact_Id"] = contactId;

            return _ministryPlatformService.CreateRecord(pageId, participantDictionary, token);
        }

        //Get Participant IDs of a contact
        public MpParticipant GetParticipantRecord(string token)
        {
            var results = _ministryPlatformService.GetRecordsDict("MyParticipantRecords", token);
            Dictionary<string, object> result = null;
            try
            {
                result = results.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Sequence contains more than one element")
                {
                    throw new MultipleRecordsException("Multiple Participant records found! Only one participant allowed per Contact.");
                }
            }

            if (result == null)
            {
                return null;
            }
            var participant = new MpParticipant
            {
                ContactId = result.ToInt("Contact_ID"),
                ParticipantId = result.ToInt("dp_RecordID"),
                EmailAddress = result.ToString("Email_Address"),
                PreferredName = result.ToString("Nickname"),
                DisplayName = result.ToString("Display_Name"),
                ApprovedSmallGroupLeader = result.ToInt("Group_Leader_Status_ID") == _configurationWrapper.GetConfigIntValue("GroupLeaderApproved"),
                GroupLeaderStatus = result.ToInt("Group_Leader_Status_ID")
            };

            return participant;
        }

        public MpParticipant GetParticipant(int contactId)
        {             
            try
            {
                var searchString = $"Contact_ID_Table.[Contact_ID]={contactId}";
                var columnList = new List<string>
                {

                    "Contact_ID_Table.Contact_ID",
                    "Participants.[Participant_ID]",
                    "Contact_ID_Table.Email_Address",
                    "Contact_ID_Table.Nickname as [NickName]",
                    "Contact_ID_Table.Display_Name",
                    "Contact_ID_Table.Nickname",
                    "Contact_ID_Table.__Age as [Age]",
                    "Participants.Attendance_Start_Date",
                    "Participant_Type_ID_Table.Participant_Type",
                    "Group_Leader_Status_ID_Table.Group_Leader_Status_ID",
                    "Host_Status_ID_Table.[Host_Status_ID]",
                    "Show_On_Map"
                };
                var token = ApiLogin();
                var participant = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpParticipant>(searchString, columnList);
                if (participant.Count == 1)
                {
                    var p = participant.First();
                    p.PreferredName = p.Nickname;
                    return p;
                }
                throw new ApplicationException($"GetParticipant failed.  Contact Id: {contactId}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"GetParticipant failed.  Contact Id: {contactId}", ex);
            }
        }

        public void UpdateParticipant(MpParticipant participant)
        {
            var participantDict = new Dictionary<string, object>()
            {
                {"Participant_ID", participant.ParticipantId },
                {"Attendance_Start_Date", participant.AttendanceStart },
                {"Group_Leader_Status_ID", participant.GroupLeaderStatus }
            };
            UpdateParticipant(participantDict);
        }

        public void UpdateParticipantHostStatus(MpParticipant participant)
        {
            var participantDict = new Dictionary<string, object>()
            {
                {"Participant_ID", participant.ParticipantId },
                {"Host_Status_ID", participant.HostStatus }
            };
            UpdateParticipant(participantDict);
        }

        public void UpdateParticipant(Dictionary<string, object> participant)
        {
            var apiToken = ApiLogin();
            try
            {
                _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Participants"), participant, apiToken);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                   string.Format("Unable to update the participant.  Participant Id: {0}", participant["Participant_ID"]), e);
            }

        }

        public List<MpResponse> GetParticipantResponses(int participantId)
        {
            try
            {
                var records =
                    WithApiLogin<List<Dictionary<string, object>>>(
                        apiToken =>
                            (_ministryPlatformService.GetSubpageViewRecords("ParticipantResponsesWithEventId",
                                participantId, apiToken, "", "")));
                return records.Select(viewRecord => new MpResponse
                {
                    Opportunity_ID = viewRecord.ToInt("Opportunity ID"),
                    Participant_ID = viewRecord.ToInt("Participant ID"),
                    Response_Result_ID = viewRecord.ToInt("Response Result ID"),
                    Event_ID = viewRecord.ToInt("Event ID")
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("GetParticipantResponses failed.  Participant Id: {0}", participantId), ex);
            }
        }
        public void UpdateHuddleGroupParticipantStatus()
        {
            var param = new Dictionary<string, object>(){};
            var token = ApiLogin();
            _ministryPlatformRestRepository.UsingAuthenticationToken(token).PostStoredProc(UpdateHuddleGroupParticipantStatusProc, param);
        }
    }
}