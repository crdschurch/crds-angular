using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class EventRepository : BaseRepository, IEventRepository
    {
        private readonly log4net.ILog _logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _eventParticipantSubPageId = Convert.ToInt32(AppSettings("EventsParticipants"));
        private readonly int _eventParticipantPageId = Convert.ToInt32(AppSettings("EventParticipant"));
        private readonly int _eventGroupsPageViewId = Convert.ToInt32(AppSettings("GroupsByEventId"));
        private readonly int _eventGroupsPageId = Convert.ToInt32(AppSettings("EventsGroups"));

        private readonly int _eventParticipantStatusDefaultId =
            Convert.ToInt32(AppSettings("Event_Participant_Status_Default_ID"));

        private readonly int _eventPageNeedReminders =
            Convert.ToInt32(AppSettings("EventsReadyForReminder"));

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IGroupRepository _groupService;
        private readonly IEventParticipantRepository _eventParticipantRepository;

        public EventRepository(IMinistryPlatformService ministryPlatformService,
                            IAuthenticationRepository authenticationService,
                            IConfigurationWrapper configurationWrapper,
                            IGroupRepository groupService,
                            IMinistryPlatformRestRepository ministryPlatformRestRepository,
                            IEventParticipantRepository eventParticipantRepository)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _groupService = groupService;
            _eventParticipantRepository = eventParticipantRepository;           
        }

        public int CreateEvent(MpEventReservationDto eventReservationReservation)
        {
            var token = ApiLogin();
            var eventPageId = _configurationWrapper.GetConfigIntValue("Events");

            var eventDictionary = new Dictionary<string, object>
            {
                {"Congregation_ID", eventReservationReservation.CongregationId},
                {"Primary_Contact", eventReservationReservation.ContactId},
                {"Description", eventReservationReservation.Description},
                {"On_Donation_Batch_Tool", eventReservationReservation.DonationBatchTool},
                {"Event_End_Date", eventReservationReservation.EndDateTime},
                {"Event_Type_ID", eventReservationReservation.EventTypeId},
                {"Meeting_Instructions", eventReservationReservation.MeetingInstructions},
                {"Minutes_for_Setup", eventReservationReservation.MinutesSetup},
                {"Minutes_for_Cleanup", eventReservationReservation.MinutesTeardown},
                {"Program_ID", eventReservationReservation.ProgramId},
                {"Reminder_Days_Prior_ID", eventReservationReservation.ReminderDaysId},
                {"Send_Reminder", eventReservationReservation.SendReminder},
                {"Event_Start_Date", eventReservationReservation.StartDateTime},
                {"Event_Title", eventReservationReservation.Title},
                {"Visibility_Level_ID", _configurationWrapper.GetConfigIntValue("EventVisibilityLevel")},
                {"Participants_Expected", eventReservationReservation.ParticipantsExpected }
            };

            try
            {
                return (_ministryPlatformService.CreateRecord(eventPageId, eventDictionary, token, true));
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Event Reservation, eventReservationReservation: {0}", eventReservationReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void UpdateEvent(MpEventReservationDto eventReservationReservation)
        {
            var token = ApiLogin();
            var eventPageId = _configurationWrapper.GetConfigIntValue("Events");

            var eventDictionary = new Dictionary<string, object>
            {
                {"Congregation_ID", eventReservationReservation.CongregationId},
                {"Primary_Contact", eventReservationReservation.ContactId},
                {"Description", eventReservationReservation.Description},
                {"On_Donation_Batch_Tool", eventReservationReservation.DonationBatchTool},
                {"Event_End_Date", eventReservationReservation.EndDateTime},
                {"Event_Type_ID", eventReservationReservation.EventTypeId},
                {"Meeting_Instructions", eventReservationReservation.MeetingInstructions},
                {"Minutes_for_Setup", eventReservationReservation.MinutesSetup},
                {"Minutes_for_Cleanup", eventReservationReservation.MinutesTeardown},
                {"Program_ID", eventReservationReservation.ProgramId},
                {"Reminder_Days_Prior_ID", eventReservationReservation.ReminderDaysId},
                {"Send_Reminder", eventReservationReservation.SendReminder},
                {"Event_Start_Date", eventReservationReservation.StartDateTime},
                {"Event_Title", eventReservationReservation.Title},
                {"Visibility_Level_ID", _configurationWrapper.GetConfigIntValue("EventVisibilityLevel")},
                {"Participants_Expected", eventReservationReservation.ParticipantsExpected },
                {"Event_ID", eventReservationReservation.EventId },
                {"Cancelled", eventReservationReservation.Cancelled }

            };

            try
            {
                _ministryPlatformService.UpdateRecord(eventPageId, eventDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating Event Reservation, eventReservationReservation: {0}", eventReservationReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public int SafeRegisterParticipant(int eventId, int participantId, int groupId = 0, int groupParticipantId = 0)
        {
            var eventParticipantId = GetEventParticipantRecordId(eventId, participantId);
            if (eventParticipantId != 0)
            {
                return eventParticipantId;
            }
            eventParticipantId = RegisterParticipantForEvent(participantId, eventId, groupId, groupParticipantId);
            return eventParticipantId;
        }

        public int RegisterInterestedParticipantWithEndDate(int participantId, int eventId, DateTime? endDate)
        {
            var values = new Dictionary<string, object>
            {
                {"Participation_Status_ID", _configurationWrapper.GetConfigIntValue("Participant_Status_Interested")}
            };

            if (endDate != null)
            {
                values["End_Date"] = endDate;
            }

            return RegisterParticipantForEvent(participantId, eventId, values);
        }

        public int RegisterParticipantForEvent(int participantId, int eventId, int groupId = 0, int groupParticipantId = 0)
        {
            var values = new Dictionary<string, object>();

            if (groupId != 0)
            {
                values.Add("Group_ID", groupId);
            }
            if (groupParticipantId != 0)
            {
                values.Add("Group_Participant_ID", groupParticipantId);
            }

            return RegisterParticipantForEvent(participantId, eventId, values);
        }

        private int RegisterParticipantForEvent(int participantId, int eventId, Dictionary<string, object> values)
        {
            _logger.Debug("Adding participant " + participantId + " to event " + eventId);

            if (values == null)
            {
                values = new Dictionary<string, object>();
            }

            values["Participant_ID"] = participantId;
            values["Event_ID"] = eventId;

            if (!values.ContainsKey("Participation_Status_ID"))
            {
                values["Participation_Status_ID"] = _eventParticipantStatusDefaultId;
            }

            int eventParticipantId;
            try
            {
                eventParticipantId =
                    WithApiLogin<int>(
                        apiToken =>
                        {
                            return
                                (_ministryPlatformService.CreateSubRecord(_eventParticipantSubPageId,
                                                                          eventId,
                                                                          values,
                                                                          apiToken,
                                                                          true));
                        });
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("registerParticipantForEvent failed.  Participant Id: {0}, Event Id: {1}",
                                  participantId,
                                  eventId),
                    ex.InnerException);
            }

            _logger.Debug(string.Format("Added participant {0} to event {1}; record id: {2}",
                                        participantId,
                                        eventId,
                                        eventParticipantId));
            return (eventParticipantId);
        }

        public int UnregisterParticipantForEvent(int participantId, int eventId)
        {
            _logger.Debug("Removing participant " + participantId + " from event " + eventId);

            int eventParticipantId;
            try
            {
                // go get record id to delete
                var recordId = GetEventParticipantRecordId(eventId, participantId);
                eventParticipantId = _ministryPlatformService.DeleteRecord(_eventParticipantPageId, recordId, null, ApiLogin());
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"unRegisterParticipantForEvent failed.  Participant Id: {participantId}, Event Id: {eventId}",
                    ex.InnerException);
            }

            _logger.Debug($"Removed participant {participantId} from event {eventId}; record id: {eventParticipantId}");
            return (eventParticipantId);
        }

        public void SetParticipantAsRegistered(int eventId, int participantId)
        {
            try
            {
                Console.WriteLine("SetParticipantAsRegistered");
                var apiToken = ApiLogin();
                var eventParticipantId = GetEventParticipantRecordId(eventId, participantId);

                var fields = new Dictionary<string, object>
                {
                    {"Event_Participant_ID", eventParticipantId},
                    {"End_Date", null},
                    {"Participation_Status_ID", _configurationWrapper.GetConfigIntValue("Participant_Status_Registered")}
                };

                _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).UpdateRecord("Event_Participants", eventParticipantId, fields);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"SetParticipantAsRegistered failed.  Participant Id: {participantId}, Event Id: {eventId}",
                    ex.InnerException);
            }       
        }

        public void UpdateParticipantEndDate(int eventParticipantId, DateTime? endDate)
        {
            try
            {
                Console.WriteLine("UpdateParticipantEndDate");
                var apiToken = ApiLogin();

                var fields = new Dictionary<string, object>
                {
                    {"Event_Participant_ID", eventParticipantId},
                    {"End_Date", endDate}
                };

                _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).UpdateRecord("Event_Participants", eventParticipantId, fields);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"UpdateParticipantEndDate failed.  Event Participant Id: {eventParticipantId}",
                    ex.InnerException);
            }
        }

        public MpEvent GetEvent(int eventId)
        {
            var apiToken = ApiLogin();
            
            var mpevent = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MpEvent>(eventId);
            mpevent.PrimaryContact = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MpContact>(mpevent.PrimaryContactId, "Email_Address, Contact_ID, Display_Name");

            return mpevent;
        }

        public int GetEventParticipantRecordId(int eventId, int participantId)
        {
            return GetEventParticipantId(eventId, participantId);
        }
    
    public bool EventHasParticipant(int eventId, int participantId)
        {
            return GetEventParticipantId(eventId, participantId) > 0;
        }

        private int GetEventParticipantId(int eventId, int participantId)
        {
            var apiToken = ApiLogin();
            const string COLUMNS =
                "Event_Participants.Event_Participant_ID";
            string search = $"Event_Participants.Event_ID = {eventId} And Event_Participants.Participant_ID = {participantId}";
            var eventParticipant = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpEventParticipant>(search, COLUMNS).FirstOrDefault();
            return eventParticipant?.EventParticipantId ?? 0;
        }

        public List<MpEvent> GetEvents(string eventType, string token)
        {
            //this is using the basic Events page, any concern there?
            var pageId = Convert.ToInt32(ConfigurationManager.AppSettings["Events"]);
            var search = ",," + eventType;
            var records = _ministryPlatformService.GetRecordsDict(pageId, token, search);

            return records.Select(record => new MpEvent
            {
                EventTitle = (string) record["Event_Title"],
                EventType = (string) record["Event_Type"],
                EventStartDate = (DateTime) record["Event_Start_Date"],
                EventEndDate = (DateTime) record["Event_End_Date"],
                EventId = (int) record["dp_RecordID"]
            }).ToList();
        }

        public List<MpEvent> GetEventsByTypeForRange(int eventTypeId, DateTime startDate, DateTime endDate, string token, bool includeCancelledEvents = true)
        {
            string columns = string.Join(", ",
                "Event_ID",
                "Event_Title",
                "Event_Type_ID_Table.Event_Type AS Event_Type_ID",   // aliased to Event_Type_ID to match JsonProperty on MpEvent.Event_Type!
                "Event_Start_Date",
                "Event_End_Date",
                "Congregation_ID",
                "Cancelled"
            );

            string search = $"Events.Event_Type_ID = {eventTypeId}";

            string startDateString = startDate.Date.ToString("yyyy-MM-dd");
            string endDateString = endDate.Date.AddDays(1).ToString("yyyy-MM-dd");
            search += $" AND Events.Event_Start_Date >= '{startDateString}' AND Events.Event_End_Date < '{endDateString}'";

            if (!includeCancelledEvents)
                search += " AND Events.Cancelled = 0";

            string orderBy = "Events.Event_ID";

            List<MpEvent> eventList = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpEvent>(search, columns, orderBy, false);
            return eventList;
        }

        public List<MpEvent> GetEventsByParentEventId(int parentEventId)
        {
            var token = ApiLogin();
            var searchStr = string.Format(",,,{0}", parentEventId);
            var records = _ministryPlatformService.GetPageViewRecords("EventsByParentEventID", token, searchStr);

            var events = records.Select(record => new MpEvent
            {
                EventTitle = record.ToString("Event_Title"),
                EventType = record.ToString("Event_Type"),
                EventStartDate = record.ToDate("Event_Start_Date", true),
                EventEndDate = record.ToDate("Event_End_Date", true),
                EventId = record.ToInt("Event_ID"),
                PrimaryContact = new MpContact
                {
                    ContactId = record.ToInt("Contact_ID"),
                    EmailAddress = record.ToString("Email_Address")
                }
            }).ToList();

            return events;
        }

        public IEnumerable<MpEvent> EventsByPageId(string token, int pageViewId)
        {
            return _ministryPlatformService.GetRecordsDict(pageViewId, token).Select(record => new MpEvent()
            {
                EventId = (int) record["Event_ID"],
                EventTitle = (string) record["Event_Title"],
                EventStartDate = (DateTime) record["Event_Start_Date"],
                EventEndDate = (DateTime) record["Event_End_Date"],
                EventType = record.ToString("Event_Type"),
                PrimaryContact = new MpContact()
                {
                    ContactId = record.ToInt("Primary_Contact_ID"),
                    EmailAddress = record.ToString("Primary_Contact_Email_Address")
                }
            }).ToList();
        }

        public IEnumerable<MpEvent> EventsByPageViewId(string token, int pageViewId, string searchString)
        {
            return _ministryPlatformService.GetPageViewRecords(pageViewId, token, searchString).Select(record => new MpEvent()
            {
                EventId = (int) record["Event_ID"],
                EventTitle = (string) record["Event_Title"],
                EventStartDate = (DateTime) record["Event_Start_Date"],
                EventEndDate = (DateTime) record["Event_End_Date"],
                EventType = record.ToString("Event_Type"),
                PrimaryContact = new MpContact()
                {
                    ContactId = record.ToInt("Primary_Contact_ID"),
                    EmailAddress = record.ToString("Primary_Contact_Email_Address")
                }
            }).ToList();
        }

        public IEnumerable<MinistryPlatform.Translation.Models.MpParticipant> EventParticipants(string token, int eventId)
        {
            return
                _ministryPlatformService.GetSubpageViewRecords("EventParticipantSubpageRegisteredView", eventId, token)
                    .Select(person => new MinistryPlatform.Translation.Models.MpParticipant()
                    {
                        ParticipantId = person.ToInt("Participant_ID"),
                        ContactId = person.ToInt("Contact_ID"),
                        EmailAddress = person.ToString("Email_Address"),
                        DisplayName = person.ToString("Display_Name"),
                        Nickname = person.ToString("Nickname"),
                        GroupName = person.ToString("Group_Name")
                    });
        }

        public void SetReminderFlag(int eventId, string token)
        {
            var dict = new Dictionary<string, object>
            {
                {"Event_ID", eventId},
                {"Reminder_Sent", 1}
            };
            _ministryPlatformService.UpdateRecord(_eventPageNeedReminders, dict, token);
        }

        public List<MpGroup> GetGroupsForEvent(int eventId)
        {
            return _groupService.GetGroupsForEvent(eventId);
        }

        public List<MpEventGroup> GetEventGroupsForEventAPILogin(int eventId)
        {
            return GetEventGroupsForEvent(eventId, ApiLogin());
        }

        public List<MpEventGroup> GetEventGroupsForEvent(int eventId, string token)
        {
            var searchString =  string.Format("\"{0}\",", eventId);
            var records = _ministryPlatformService.GetPageViewRecords(_eventGroupsPageViewId, token, searchString);

            return records?.Select(record => new MpEventGroup
                                   {
                                       EventGroupId = record.ToInt("Event_Group_ID"),
                                       EventId = record.ToInt("Event_ID"),
                                       GroupId = record.ToInt("Group_ID"),
                                       RoomId = record.ToNullableInt("Room_ID"),
                                       Closed = record.ToBool("Closed"),
                                       EventRoomId = record.ToNullableInt("Event_Room_ID"),
                                       GroupTypeId = record.ToInt("Group_Type_ID")
                                   }).ToList();
        } 

        public List<MpEventGroup> GetEventGroupsForGroup(int groupId, string token)
        {
            var searchString = string.Format(",,,\"{0}\"", groupId);
            var records = _ministryPlatformService.GetPageViewRecords(_eventGroupsPageViewId, token, searchString);

            if (records == null)
            {
                return null;
            }
            return records.Select(record => new MpEventGroup
            {
                EventGroupId = record.ToInt("Event_Group_ID"),
                EventId = record.ToInt("Event_ID"),
                GroupId = record.ToInt("Group_ID"),
                RoomId = record.ToNullableInt("Room_ID"),
                Closed = record.ToBool("Closed"),
                EventRoomId = record.ToNullableInt("Event_Room_ID")
            }).ToList();
        }

        public void DeleteEventGroup(MpEventGroup eventGroup, string token)
        {
            _ministryPlatformService.DeleteRecord(_eventGroupsPageId, eventGroup.EventGroupId, null, token);
        }

        public void DeleteEventGroupsForEvent(int eventId, string token, int? groupTypeID = null)
        {
            // get event group ids
            var discardedEventGroupIds = groupTypeID == null 
                ? GetEventGroupsForEvent(eventId, token).Select(r => r.EventGroupId).ToArray() 
                : GetEventGroupsForEvent(eventId, token).Where(r => r.GroupTypeId == groupTypeID).Select(r => r.EventGroupId).ToArray();

            // MP will throw an error if there are no elements to delete, so we need to exit the function before then
            if (discardedEventGroupIds.Length == 0)
            {
                return;
            }

            // create selection for event groups
            SelectionDescription eventGroupSelDesc = new SelectionDescription();
            eventGroupSelDesc.DisplayName = "DiscardedEventGroups " + DateTime.Now;
            eventGroupSelDesc.Kind = SelectionKind.Normal;
            eventGroupSelDesc.PageId = _eventGroupsPageId;
            var eventGroupSelId = _ministryPlatformService.CreateSelection(eventGroupSelDesc, token);

            // add events to selection
            _ministryPlatformService.AddToSelection(eventGroupSelId, discardedEventGroupIds, token);

            // delete the selection records
            _ministryPlatformService.DeleteSelectionRecords(eventGroupSelId, token);

            // delete the selection
            _ministryPlatformService.DeleteSelection(eventGroupSelId, token);
        }

        public List<MpEvent> GetEventsBySite(string site, string token, DateTime startDate, DateTime endDate)
        {
            StringBuilder dateSearchString = new StringBuilder();

            var dayOffset = endDate - startDate;

            // if the days at the same, this is done to create the basic search string
            if (dayOffset.Days == 0)
            {
                dateSearchString.Append(startDate.ToShortDateString());
            }

            for (int day = 0; day < dayOffset.Days; day++)
            {
                DateTime newDate = startDate.Date.AddDays(day);

                if (day != (dayOffset.Days - 1))
                {
                    dateSearchString.Append(newDate.ToShortDateString() + " OR ");
                }
                else if (day == (dayOffset.Days - 1))
                {
                    dateSearchString.Append(newDate.ToShortDateString());
                }
            }

            var searchString = string.Format(",,\"{0}\",,False,{1},{1}", site, dateSearchString);

            return GetEventsData(token, searchString);
        }

        public List<MpEvent> GetEventTemplatesBySite(string site, string token)
        {
            var searchString = string.Format(",,\"{0}\",,True,", site);

            return GetEventsData(token, searchString);
        }

        public int CreateEventGroup(MpEventGroup eventGroup, string token = "")
        {
            if (token == "")
                token = ApiLogin();
            var groupDictionary = new Dictionary<string, object>
            {
                {"Event_ID", eventGroup.EventId},
                {"Group_ID", eventGroup.GroupId},
                {"Room_ID", eventGroup.RoomId},
                {"Domain_ID", 1},
                {"Closed", eventGroup.Closed},
                {"Event_Room_ID", eventGroup.EventRoomId}
            };

            try
            {
                return (_ministryPlatformService.CreateRecord(_eventGroupsPageId, groupDictionary, token, true));
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Event Group, eventGroup: {0}", eventGroup);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void UpdateEventGroup(MpEventGroup eventGroup, string token)
        {
            var groupDictionary = new Dictionary<string, object>
            {
                {"Event_ID", eventGroup.EventId},
                {"Group_ID", eventGroup.GroupId},
                {"Room_ID", eventGroup.RoomId},
                {"Domain_ID", eventGroup.DomainId},
                {"Closed", eventGroup.Closed},
                {"Event_Room_ID", eventGroup.EventRoomId}
            };

            try
            {
                _ministryPlatformService.UpdateRecord(_eventGroupsPageViewId, groupDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating Event Group, eventGroup: {0}", eventGroup);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        private List<MpEvent> GetEventsData(string token, string searchString)
        {
            var pageViewId = _configurationWrapper.GetConfigIntValue("EventsBySite");
            var records = _ministryPlatformService.GetPageViewRecords(pageViewId, token, searchString);

            if (records == null || records.Count == 0)
            {
                return null;
            }

            return records.Select(record => new MpEvent
            {
                // this isn't a complete list of all event fields - we may need more for user info purposes
                EventId = record.ToInt("Event_ID"),
                Congregation = record.ToString("Congregation_Name"),
                EventTitle = record.ToString("Event_Title"),
            }).ToList();
        }

        public List<MpEventWaivers> GetWaivers(int eventId, int contactId)
        {
            var apiToken = ApiLogin();
            var eventParticipantId = _eventParticipantRepository.GetEventParticipantByContactId(eventId, contactId);
            if (eventParticipantId == 0)
            {
                throw new ApplicationException("Event Participant not found for Contact");
            }

            const string columnList = "Waiver_ID_Table.[Waiver_ID], Waiver_ID_Table.[Waiver_Name], Waiver_ID_Table.[Waiver_Text], cr_Event_Waivers.[Required]";
            var campWaivers = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpEventWaivers>($"Event_ID = {eventId} AND Active=1", columnList).ToList();
            
            //for each event/waiver, see if someone has signed. 
            const string columns = "cr_Event_Participant_Waivers.Waiver_ID, cr_Event_Participant_Waivers.Event_Participant_ID, Accepted, Signee_Contact_ID";
            foreach (var waiver in campWaivers)
            {
                var searchString = $"Waiver_ID_Table.Waiver_ID = {waiver.WaiverId} AND Event_Participant_ID_Table_Event_ID_Table.Event_ID = {eventId} AND cr_Event_Participant_Waivers.Event_Participant_ID = {eventParticipantId}";
                var response = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpWaiverResponse>(searchString, columns).FirstOrDefault();
                if (response != null)
                {
                    waiver.Accepted = response.Accepted;
                    waiver.SigneeContactId = response.SigneeContactId;
                }
            }

            return campWaivers;
        }

        public List<MpEventWaivers> GetWaivers(int eventId)
        {            
            var apiToken = ApiLogin();
            const string columnList = "Waiver_ID_Table.[Waiver_ID], Waiver_ID_Table.[Waiver_Name], Waiver_ID_Table.[Waiver_Text], cr_Event_Waivers.[Required]";
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpEventWaivers>($"Event_ID = {eventId} AND Active=1", columnList).ToList();
        }

        public void SetWaivers(List<MpWaiverResponse> waiverResponses)
        {
            var apiToken = ApiLogin();

            foreach (var waiver in waiverResponses)
            {
                var searchString = $"Event_Participant_ID={waiver.EventParticipantId} AND Waiver_ID={waiver.WaiverId}";
                waiver.EventParticipantWaiverId = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<int>("cr_Event_Participant_Waivers", searchString, "Event_Participant_Waiver_ID", null, false);
            }
            
            _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Post(waiverResponses.Where(w => w.EventParticipantWaiverId == 0).ToList());

            _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Put(waiverResponses.Where(w => w.EventParticipantWaiverId != 0).ToList());
        }

        public bool IsEventSeries(int eventId)
        {
            var apiToken = ApiLogin();
            var result = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MpSequenceRecord>(eventId);

            return result != null;
        }        

        public Result<int> GetProductEmailTemplate(int eventId)
        {
            var apiToken = ApiLogin();

            var filter = $"Events.[Event_ID] = {eventId}";
            const string column = "Online_Registration_Product_Table_Program_ID_Table_Communication_ID_Table.[Communication_ID]";
            try
            {
                var result = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<int>("Events", filter, column, null, false);
                if (result == 0)
                {
                    return new Err<int>("No Email Template Found");
                }
                return new Ok<int>(result);
            }
            catch (Exception e)
            {
                return new Err<int>(e);                
            }
        }
    }
}
