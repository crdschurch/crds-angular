using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class EventParticipantRepository : BaseRepository, IEventParticipantRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public EventParticipantRepository(IMinistryPlatformService ministryPlatformService, IMinistryPlatformRestRepository ministryPlatformRestRepository, IAuthenticationRepository authenticationService, IConfigurationWrapper configurationWrapper)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public bool AddDocumentsToTripParticipant(List<MpTripDocuments> documents, int eventParticipantId)
        {
            try
            {
                var token = ApiLogin();
                foreach (var d in documents)
                {
                    var values = new Dictionary<string, object>
                    {
                        {"Document_ID", d.DocumentId},
                        {"Received", false}
                    };
                    _ministryPlatformService.CreateSubRecord("EventParticipantDocuments", eventParticipantId, values, token, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("AddDocumentsToTripParticipant failed.  Event Participant: {0}", eventParticipantId),
                    ex);
            }
        }

        public List<MpTripParticipant> TripParticipants(string search)
        {
            try
            {
                var records =
                    WithApiLogin(
                        apiToken =>
                            (_ministryPlatformService.GetPageViewRecords("GoTripParticipants",
                                                                         apiToken,
                                                                         search)));
                return records.Select(viewRecord => new MpTripParticipant
                {
                    EventParticipantId = viewRecord.ToInt("Event_Participant_ID"),
                    EventId = viewRecord.ToInt("Event_ID"),
                    EventTitle = viewRecord.ToString("Event_Title"),
                    Nickname = viewRecord.ToString("Nickname"),
                    Lastname = viewRecord.ToString("Last_Name"),
                    EmailAddress = viewRecord.ToString("Email_Address"),
                    EventStartDate = viewRecord.ToDate("Event_Start_Date"),
                    EventEndDate = viewRecord.ToDate("Event_End_Date"),
                    EventType = viewRecord.ToString("Event_Type"),
                    ParticipantId = viewRecord.ToInt("Participant_ID"),
                    ProgramId = viewRecord.ToInt("Program_ID"),
                    ProgramName = viewRecord.ToString("Program_Name"),
                    CampaignId = viewRecord.ToInt("Campaign_ID"),
                    CampaignName = viewRecord.ToString("Campaign_Name"),
                    DonorId = viewRecord.ToInt("Donor_ID"),
                    ContactId = viewRecord.ToInt("Contact_ID"),
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("TripParticipants failed.  search: {0}", search),
                    ex);
            }
        }

        public List<MpEventParticipant> GetChildCareParticipants(int daysBeforeEvent)
        {
            try
            {
                var search = string.Format("\"{0}\",", daysBeforeEvent);

                var records =
                    WithApiLogin(
                        apiToken =>
                            (_ministryPlatformService.GetPageViewRecords("EventParticipantsChildCarePageView",
                                                                         apiToken,
                                                                         search)));

                return records.Select(viewRecord => new MpEventParticipant
                {
                    ChildcareRequired = viewRecord.ToBool("Child_Care_Requested"),
                    ContactId = viewRecord.ToInt("Contact_ID"),
                    EventId = viewRecord.ToInt("Event_ID"),
                    EventParticipantId = viewRecord.ToInt("Event_Participant_ID"),
                    EventStartDateTime = viewRecord.ToDate("Event_Start_Date"),
                    EventTitle = viewRecord.ToString("Event_Title"),
                    GroupId = viewRecord.ToInt("Group_ID"),
                    GroupName = viewRecord.ToString("Group_Name"),
                    GroupParticipantId = viewRecord.ToInt("Group_Participant_ID"),
                    ParticipantEmail = viewRecord.ToString("Email_Address"),
                    ParticipantId = viewRecord.ToInt("Participant_ID")
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("GetChildCareParticipants failed.", ex);
            }
        }

        public List<MpEventParticipant> GetEventParticipants(int eventId, int? roomId = null)
        {
            var searchString = roomId == null ? null : string.Format(",,,\"{0}\"", roomId);

            try
            {
                var records =
                    WithApiLogin(
                        apiToken => _ministryPlatformService.GetSubpageViewRecords("EventParticipantAssignedToRoomApiSubPageView", eventId, apiToken, searchString));
                return records.Select(viewRecord => new MpEventParticipant
                {
                    EventParticipantId = viewRecord.ToInt("Event_Participant_ID"),
                    ParticipantId = viewRecord.ToInt("Participant_ID"),
                    ParticipantStatus = viewRecord.ToInt("Participation_Status_ID"),
                    RoomId = viewRecord.ToInt("Room_ID")
                }).ToList();
            }
            catch (Exception e)
            {
                throw new ApplicationException("GetEventParticipants failed", e);
            }
        }

        public IObservable<MpEventParticipant> GetEventParticpant(int eventParticipantId)
        {
            try
            {
                return Observable.Start(() =>
                {
                    var columns = "Event_Participants.[Event_Participant_ID],Event_ID_Table.[Event_Title],Participant_ID_Table.[Participant_ID],Participant_ID_Table_Contact_ID_Table.[Contact_ID],Event_ID_Table.[Event_ID],Event_ID_Table.[Event_Start_Date],Participation_Status_ID_Table.[Participation_Status_ID]";
                    return _ministryPlatformRestRepository.UsingAuthenticationToken(ApiLogin()).Get<MpEventParticipant>(eventParticipantId, columns);
                });
            }
            catch (Exception e)
            {
                return Observable.Throw<MpEventParticipant>(new Exception("Unable to get the event particpant"));
            }
        }

        public int GetEventParticipantByContactId(int eventId, int contactId)
        {
            const string tableName = "Event_Participants";
            var searchString = $"Event_ID_Table.Event_ID={eventId} AND Participant_ID_Table_Contact_ID_Table.Contact_ID={contactId}";
            const string column = "Event_Participant_ID";
            var eventParticipant = _ministryPlatformRestRepository.UsingAuthenticationToken(ApiLogin()).Search<int>(tableName, searchString, column, null, false);
            return eventParticipant;
        }

        public MpEventParticipant GetEventParticipantEligibility(int eventId, int contactId)
        {
            var apiToken = ApiLogin();

            var filter = $"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}";
            var participants = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken)
                .Search<MpEventParticipant>(filter, "Event_Participants.[Event_Participant_ID], Event_Participants.[_Setup_Date] as [Setup_Date], Event_Participants.[End_Date], Event_Participants.[Participation_Status_ID]");

            return participants.FirstOrDefault();
        }

        public DateTime? EventParticipantSignupDate(int contactId, int eventId, string apiToken)
        {

            var cancelledcamperstatus = _configurationWrapper.GetConfigIntValue("Event_Participant_Status_ID_Cancelled");
            var filter = $"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId} AND Participation_Status_ID_Table.[Participation_Status_ID] <> {cancelledcamperstatus}";
            var participants = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken)
                .Search<MpEventParticipant>(filter, "Event_Participants.[Event_Participant_ID],Event_Participants.[_Setup_Date] as [Setup_Date]");
            if (participants.Count > 0)
            {
                var ret = participants.FirstOrDefault();
                if (ret != null) return ret.SetupDate;
            }
            return null;
        }

        public Result<MpEventParticipant> GetEventParticipantByContactAndEvent(int contactId, int eventId, string token)
        {
            try
            {
                var filter = $"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}";
                var columns = new List<string>
                {
                    "Participant_ID_Table_Contact_ID_Table.[Contact_ID]",
                    "Event_ID_Table.[Event_ID]",
                    "Event_Participant_ID",
                    "Event_ID_Table.Event_Title",
                    "Participation_Status_ID",
                    "End_Date"
                };
                var participants = _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                    .Search<MpEventParticipant>(filter, columns);
                if (participants.Count > 0)
                {
                    return new Ok<MpEventParticipant>(participants.FirstOrDefault());
                }
                return new Err<MpEventParticipant>("No Participants Found");
            }
            catch (Exception e)
            {
                return new Err<MpEventParticipant>(e);
            }
        }

        public int GetEventParticipantCountByGender(int eventId, int genderId)
        {
            var apiToken = ApiLogin();
            const string tableName = "Event_Participants";
            var searchString = $"Event_ID = {eventId} AND Participant_ID_Table_Contact_ID_Table_Gender_ID_Table.Gender_ID = {genderId} AND (End_Date IS NULL OR End_Date >= GETDATE())";
            const string columnName = "Count(*)";

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<int>(tableName, searchString, columnName, null, false);
        }

        public IObservable<MpEventParticipant> GetEventParticpantByEventParticipantWaiver(int eventParticipantWaiverId)
        {
            return Observable.Create<MpEventParticipant>(observer =>
            {
                try
                {
                    var columns = "Event_Participant_ID_Table.[Event_Participant_ID], Event_Participant_ID_Table_Event_ID_Table.[Event_Title] AS [Event_Title]";
                    var filter = $"cr_Event_Participant_Waivers.[Event_Participant_Waiver_ID] = {eventParticipantWaiverId}";

                    var result = _ministryPlatformRestRepository.UsingAuthenticationToken(ApiLogin())
                        .Search<MpEventParticipantWaiver, MpEventParticipant>(filter, columns);
                    if (result.Count > 0)
                    {
                        observer.OnNext(result.First());
                        observer.OnCompleted();
                    }
                    else
                    {
                        observer.OnError(new Exception($"Unable to find event participant waiver with id {eventParticipantWaiverId}"));
                    }
                   
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                return Disposable.Empty;
            });
        }
    }
}
