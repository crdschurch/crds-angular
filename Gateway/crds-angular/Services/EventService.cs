using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Crossroads.Groups;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Services;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.Repositories.Interfaces;
using WebGrease.Css.Extensions;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using IEventService = crds_angular.Services.Interfaces.IEventService;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using Participant = MinistryPlatform.Translation.Models.MpParticipant;
using TranslationEventService = MinistryPlatform.Translation.Repositories.Interfaces.IEventRepository;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services
{
    public class EventService : MinistryPlatformBaseService, IEventService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (EventService));

        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly TranslationEventService _eventService;
        private readonly IGroupRepository _groupService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IContactRepository _contactService;
        private readonly IContentBlockService _contentBlockService;
        private readonly IApiUserRepository _apiUserService;
        private readonly IContactRelationshipRepository _contactRelationshipService;
        private readonly IGroupParticipantRepository _groupParticipantService;
        private readonly IParticipantRepository _participantService;
        private readonly IRoomRepository _roomService;
        private readonly IEquipmentRepository _equipmentService;
        private readonly IEventParticipantRepository _eventParticipantService;
        private readonly int childcareEventTypeID;
        private readonly int childcareGroupTypeID;

        private readonly List<string> _tableHeaders = new List<string>()
        {
            "Event Date",
            "Registered User",
            "Start Time",
            "End Time",
            "Location"
        };


        public EventService(TranslationEventService eventService,
                            IGroupRepository groupService,
                            ICommunicationRepository communicationService,
                            IContactRepository contactService,
                            IContentBlockService contentBlockService,
                            IConfigurationWrapper configurationWrapper,
                            IApiUserRepository apiUserService,
                            IContactRelationshipRepository contactRelationshipService,
                            IGroupParticipantRepository groupParticipantService,
                            IParticipantRepository participantService,
                            IRoomRepository roomService,
                            IEquipmentRepository equipmentService,
                            IEventParticipantRepository eventParticipantService)
        {
            _eventService = eventService;
            _groupService = groupService;
            _communicationService = communicationService;
            _contactService = contactService;
            _contentBlockService = contentBlockService;
            _configurationWrapper = configurationWrapper;
            _apiUserService = apiUserService;
            _contactRelationshipService = contactRelationshipService;
            _groupParticipantService = groupParticipantService;
            _participantService = participantService;
            _roomService = roomService;
            _equipmentService = equipmentService;
            _eventParticipantService = eventParticipantService;

            childcareEventTypeID = configurationWrapper.GetConfigIntValue("ChildcareEventType");
            childcareGroupTypeID = configurationWrapper.GetConfigIntValue("ChildcareGroupType");
        }

        public int AddEventGroup(int eventId, int groupId)
        {
            var eventGroup = new MpEventGroup
            {
                EventId = eventId,
                GroupId = groupId,
                DomainId = 1
            };

            return _eventService.CreateEventGroup(eventGroup);
        }

        public int AddEvent(EventToolDto eventReservation)
        {
            var eventDto = PopulateReservationDto(eventReservation);
            var eventId = _eventService.CreateEvent(eventDto);
            return eventId;
        }

        public void UpdateEvent(EventToolDto eventReservation, int eventId)
        {
            var eventDto = PopulateReservationDto(eventReservation);
            eventDto.EventId = eventId;
            _eventService.UpdateEvent(eventDto);
        }

        private MpEventReservationDto PopulateReservationDto(EventToolDto eventTool)
        {
            var eventDto = new MpEventReservationDto();
            eventDto.CongregationId = eventTool.CongregationId;
            eventDto.ContactId = eventTool.ContactId;
            eventDto.Description = eventTool.Description;
            eventDto.DonationBatchTool = eventTool.DonationBatchTool;
            eventDto.EndDateTime = eventTool.EndDateTime;
            eventDto.EventTypeId = eventTool.EventTypeId;
            eventDto.MeetingInstructions = eventTool.MeetingInstructions;
            eventDto.MinutesSetup = eventTool.MinutesSetup;
            eventDto.MinutesTeardown = eventTool.MinutesTeardown;
            eventDto.ProgramId = eventTool.ProgramId;
            eventDto.ParticipantsExpected = eventTool.ParticipantsExpected;
            if (eventTool.ReminderDaysId > 0)
            {
                eventDto.ReminderDaysId = eventTool.ReminderDaysId;
            }
            eventDto.Cancelled = eventTool.Cancelled;
            eventDto.SendReminder = eventTool.SendReminder;
            eventDto.StartDateTime = eventTool.StartDateTime;
            eventDto.Title = eventTool.Title;
            return eventDto;

        }

        public MpEvent GetEvent(int eventId)
        {
            return _eventService.GetEvent(eventId);
        }

        public void RegisterForEvent(EventRsvpDto eventDto, AuthDTO token)
        {
            var defaultGroupRoleId = AppSetting("Group_Role_Default_ID");
            var today = DateTime.Today;
            try
            {
                var saved = eventDto.Participants.Select(participant =>
                {
                    var groupParticipantId = _groupParticipantService.Get(eventDto.GroupId, participant.ParticipantId);
                    if (groupParticipantId == 0)
                    {
                        groupParticipantId = _groupService.AddParticipantToGroup(participant.ParticipantId,
                                                                                 eventDto.GroupId,
                                                                                 defaultGroupRoleId,
                                                                                 participant.ChildcareRequested,
                                                                                 false,
                                                                                 today);
                    }

                    // validate that there is not a participant record before creating
                    var retVal =
                        Functions.IntegerReturnValue(
                            () =>
                                !_eventService.EventHasParticipant(eventDto.EventId, participant.ParticipantId)
                                    ? _eventService.RegisterParticipantForEvent(participant.ParticipantId, eventDto.EventId, eventDto.GroupId, groupParticipantId)
                                    : 1);

                    return new RegisterEventObj()
                    {
                        EventId = eventDto.EventId,
                        ParticipantId = participant.ParticipantId,
                        RegisterResult = retVal,
                        ChildcareRequested = participant.ChildcareRequested
                    };
                }).ToList();

                SendRsvpMessage(saved, token);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Unable to add event participant: " + e.Message);
            }
        }

        public IList<Models.Crossroads.Events.Event> EventsReadyForPrimaryContactReminder(string token)
        {
            var pageViewId = AppSetting("EventsReadyForPrimaryContactReminder");
            var search = "";
            var events = _eventService.EventsByPageViewId(token, pageViewId, search);
            var eventList = events.Select(evt => new Models.Crossroads.Events.Event()
            {
                name = evt.EventTitle,
                EventId = evt.EventId,
                EndDate = evt.EventEndDate,
                StartDate = evt.EventStartDate,
                EventType = evt.EventType,
                location = evt.Congregation,
                PrimaryContactEmailAddress = evt.PrimaryContact.EmailAddress,
                PrimaryContactId = evt.PrimaryContact.ContactId
            });

            return eventList.ToList();
        }

        public IList<Models.Crossroads.Events.Event> EventsReadyForReminder(string token)
        {
            var pageId = AppSetting("EventsReadyForReminder");
            var events = _eventService.EventsByPageId(token, pageId);
            var eventList = events.Select(evt => new Models.Crossroads.Events.Event()
            {
                name = evt.EventTitle,
                EventId = evt.EventId,
                EndDate = evt.EventEndDate,
                StartDate = evt.EventStartDate,
                EventType = evt.EventType,
                location = evt.Congregation,
                PrimaryContactEmailAddress = evt.PrimaryContact.EmailAddress,
                PrimaryContactId = evt.PrimaryContact.ContactId
            });
            // Childcare will be included in the email for event, so don't send a duplicate.
            return eventList.Where(evt => evt.EventType != "Childcare").ToList();
        }

        public IList<Participant> EventParticpants(int eventId, string token)
        {
            return _eventService.EventParticipants(token, eventId).ToList();
        }

        public void SendReminderEmails()
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            var eventList = EventsReadyForReminder(token);

            eventList.ForEach(evt =>
            {
                try
                {
                    // get the participants...
                    var participants = EventParticpants(evt.EventId, token);

                    // does the event have a childcare event?
                    var childcare = GetChildcareEvent(evt.EventId);
                    var childcareParticipants = childcare != null ? EventParticpants(childcare.EventId, token) : new List<Participant>();

                    participants.ForEach(participant => SendEventReminderEmail(evt, participant, childcare, childcareParticipants, token));
                    _eventService.SetReminderFlag(evt.EventId, token);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error sending Event Reminder email.", ex);
                }
            });
        }

        public void SendPrimaryContactReminderEmails()
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            var eventList = EventsReadyForPrimaryContactReminder(token);

            eventList.ForEach(evt =>
            {
                var actionNeeded = "";
                var roomsString = "No Room Reserved under the Date and Time";
                var rooms = _roomService.GetRoomReservations(evt.EventId).Where(r => !r.Cancelled).ToList();
                if (rooms.Any()) //there are non-cancelled rooms
                {
                    //if there are approved rooms, show those
                    var approvedRooms = rooms.Where(r => r.Approved).ToList();
                    if (approvedRooms.Any())
                        roomsString = string.Join(", ", approvedRooms.Select((s => s.Name)).ToArray());
                    else //there aren't approved rooms - look for pending (not rejected) rooms
                    {
                        var pendingRooms = rooms.Where(r => !r.Rejected).ToList();
                        if (pendingRooms.Any())
                            roomsString = string.Join(", ", pendingRooms.Select((s => s.Name)).ToArray()) + " (Room Pending Approval)";
                        else
                        {
                            roomsString = "NONE-Room Reservation was Rejected";
                            actionNeeded = " - ACTION REQUIRED";
                        }
                    }
                }

                SendPrimaryContactReminderEmail(evt, roomsString, token, actionNeeded);
            });
        }

        private void SendEventReminderEmail(Models.Crossroads.Events.Event evt, Participant participant, MpEvent childcareEvent, IList<Participant> children, string token)
        {
            try
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Nickname", participant.Nickname},
                    {"Event_Title", evt.name},
                    {"Event_Start_Date", evt.StartDate.ToShortDateString()},
                    {"Event_Start_Time", evt.StartDate.ToShortTimeString()},
                    {"cmsChildcareEventReminder", string.Empty},
                    {"Childcare_Children", string.Empty},
                    {"Childcare_Contact", string.Empty} // Set these three parameters no matter what...
                };

                if (children.Any())
                {
                    // determine if any of the children are related to the participant
                    var mine = MyChildrenParticipants(participant.ContactId, children, token);
                    // build the HTML for the [Childcare] data
                    if (mine.Any())
                    {
                        mergeData["cmsChildcareEventReminder"] = _contentBlockService["cmsChildcareEventReminder"].Content;
                        var childcareString = ChildcareData(mine);
                        mergeData["Childcare_Children"] = childcareString;
                        mergeData["Childcare_Contact"] = new HtmlElement("span", "If you need to cancel, please email " + childcareEvent.PrimaryContact.EmailAddress).Build();
                    }
                }
                var defaultContact = _contactService.GetContactById(AppSetting("DefaultContactEmailId"));
                var comm = _communicationService.GetTemplateAsCommunication(
                    AppSetting("EventReminderTemplateId"),
                    defaultContact.Contact_ID,
                    defaultContact.Email_Address,
                    evt.PrimaryContactId,
                    evt.PrimaryContactEmailAddress,
                    participant.ContactId,
                    participant.EmailAddress,
                    mergeData);
                _communicationService.SendMessage(comm);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending Event Reminder email.", ex);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void SendPrimaryContactReminderEmail(Models.Crossroads.Events.Event evt, string rooms, string token, string actionText)
        {
            try
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Event_ID", evt.EventId},
                    {"Event_Title", evt.name},
                    {"Event_Start_Date", evt.StartDate.ToShortDateString()},
                    {"Event_Start_Time", evt.StartDate.ToShortTimeString()},
                    {"Room_Name", rooms },
                    {"Base_Url", _configurationWrapper.GetConfigValue("BaseMPUrl")},
                    {"Action_Needed", actionText}
                };

                var defaultContact = _contactService.GetContactById(AppSetting("DefaultContactEmailId"));
                var comm = _communicationService.GetTemplateAsCommunication(
                    AppSetting("EventPrimaryContactReminderTemplateId"),
                    defaultContact.Contact_ID,
                    defaultContact.Email_Address,
                    evt.PrimaryContactId,
                    evt.PrimaryContactEmailAddress,
                    evt.PrimaryContactId,
                    evt.PrimaryContactEmailAddress,
                    mergeData);
                _communicationService.SendMessage(comm);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending Event Reminder email.", ex);
            }
        }

        public List<Participant> MyChildrenParticipants(int contactId, IList<Participant> children, string token)
        {
            var relationships = _contactRelationshipService.GetMyCurrentRelationships(contactId, token);
            var mine = children.Where(child => relationships.Any(rel => rel.Contact_Id == child.ContactId)).ToList();
            return mine;
        }

        private String ChildcareData(IList<Participant> children)
        {
            var el = new HtmlElement("span",
                                     new Dictionary<string, string>(),
                                     "You have indicated that you need childcare for the following children:")
                .Append(new HtmlElement("ul").Append(children.Select(child => new HtmlElement("li", child.DisplayName)).ToList()));
            return el.Build();
        }

        private void SendRsvpMessage(List<RegisterEventObj> saved, AuthDTO token)
        {
            var evnt = _eventService.GetEvent(saved.First().EventId);
            var childcareRequested = saved.Any(s => s.ChildcareRequested);
            var loggedIn = _contactService.GetMyProfile(token);

            var childcareHref = new HtmlElement("a",
                                                new Dictionary<string, string>()
                                                {
                                                    {
                                                        "href",
                                                        string.Format("https://{0}/childcare/{1}", _configurationWrapper.GetConfigValue("BaseUrl"), evnt.EventId)
                                                    }
                                                },
                                                "this link").Build();
            var childcare = _contentBlockService["eventRsvpChildcare"].Content.Replace("[url]", childcareHref);

            var mergeData = new Dictionary<string, object>
            {
                {"Event_Name", evnt.EventTitle},
                {"HTML_Table", SetupTable(saved, evnt).Build()},
                {"Childcare", (childcareRequested) ? childcare : ""}
            };
            var defaultContact = _contactService.GetContactById(AppSetting("DefaultContactEmailId"));
            var comm = _communicationService.GetTemplateAsCommunication(
                AppSetting("OneTimeEventRsvpTemplate"),
                defaultContact.Contact_ID,
                defaultContact.Email_Address,
                evnt.PrimaryContact.ContactId,
                evnt.PrimaryContact.EmailAddress,
                loggedIn.Contact_ID,
                loggedIn.Email_Address,
                mergeData
                );

            _communicationService.SendMessage(comm);
        }

        private HtmlElement SetupTable(List<RegisterEventObj> regData, MpEvent evnt)
        {
            var tableAttrs = new Dictionary<string, string>()
            {
                {"width", "100%"},
                {"border", "1"},
                {"cellspacing", "0"},
                {"cellpadding", "5"}
            };

            var cellAttrs = new Dictionary<string, string>()
            {
                {"align", "center"}
            };

            var htmlrows = regData.Select(rsvp =>
            {
                var p = _contactService.GetContactByParticipantId(rsvp.ParticipantId);
                return new HtmlElement("tr")
                    .Append(new HtmlElement("td", cellAttrs, evnt.EventStartDate.ToShortDateString()))
                    .Append(new HtmlElement("td", cellAttrs, p.First_Name + " " + p.Last_Name))
                    .Append(new HtmlElement("td", cellAttrs, evnt.EventStartDate.ToShortTimeString()))
                    .Append(new HtmlElement("td", cellAttrs, evnt.EventEndDate.ToShortTimeString()))
                    .Append(new HtmlElement("td", cellAttrs, evnt.Congregation));
            }).ToList();

            return new HtmlElement("table", tableAttrs)
                .Append(SetupTableHeader)
                .Append(htmlrows);
        }

        private HtmlElement SetupTableHeader()
        {
            var headers = _tableHeaders.Select(el => new HtmlElement("th", el)).ToList();
            return new HtmlElement("tr", headers);
        }

        private class RegisterEventObj
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int RegisterResult { get; set; }
            public int ParticipantId { get; set; }
            public int EventId { get; set; }
            public bool ChildcareRequested { get; set; }
        }

        public MpEvent GetMyChildcareEvent(int parentEventId, AuthDTO token)
        {
            if (!_eventService.EventHasParticipant(parentEventId, token.UserInfo.Mp.ParticipantId.GetValueOrDefault()))
            {
                return null;
            }
            // token user is part of parent event, retrieve childcare event
            var childcareEvent = GetChildcareEvent(parentEventId);
            return childcareEvent;
        }

        public MpEvent GetChildcareEvent(int parentEventId)
        {
            var childEvents = _eventService.GetEventsByParentEventId(parentEventId);
            var childcareEvents = childEvents.Where(childEvent => childEvent.EventType == "Childcare").ToList();

            if (childcareEvents.Count == 0)
            {
                return null;
            }
            if (childcareEvents.Count > 1)
            {
                throw new ApplicationException(string.Format("Mulitple Childcare Events Exist, parent event id: {0}", parentEventId));
            }
            return childcareEvents.First();
        }

        public bool CopyEventSetup(int eventTemplateId, int eventId)
        {
            // event groups and event rooms need to be removed before adding new ones
            _eventService.DeleteEventGroupsForEvent(eventId);
            _roomService.DeleteEventRoomsForEvent(eventId);

            // get event rooms (room reservation DTOs) and event groups for the template
            var eventRooms = _roomService.GetRoomReservations(eventTemplateId);
            var eventGroups = _eventService.GetEventGroupsForEvent(eventTemplateId);

            // step 2 - create new room reservations and assign event groups to them
            foreach (var eventRoom in eventRooms)
            {
                eventRoom.EventId = eventId;

                // this is the new room reservation id for the copied room
                int roomReservationId = _roomService.CreateRoomReservation(eventRoom);

                // get the template event group which matched the template event room, and assign the reservation id to this object
                var eventGroupsForRoom = (from r in eventGroups where r.EventRoomId == eventRoom.EventRoomId select r);

                foreach (var eventGroup in eventGroupsForRoom)
                {
                    // create the copied event group and assign the new room reservation id here
                    eventGroup.EventId = eventId;
                    eventGroup.EventRoomId = roomReservationId;
                    _eventService.CreateEventGroup(eventGroup);
                    eventGroup.Created = true;
                }
            }

            foreach (var eventGroup in (from groups in eventGroups where groups.Created != true select groups))
            {
                // create the copied event group and assign the new room reservation id here
                eventGroup.EventId = eventId;
                _eventService.CreateEventGroup(eventGroup);
                eventGroup.Created = true;
            }

            return true;
        }

        public List<MpEvent> GetEventsBySite(string site, DateTime startDate, DateTime endDate)
        {
            var eventTemplates = _eventService.GetEventsBySite(site, startDate, endDate);

            return eventTemplates;
        }

        public List<MpEvent> GetEventTemplatesBySite(string site)
        {
            var eventTemplates = _eventService.GetEventTemplatesBySite(site);

            return eventTemplates;
        }
    }
}