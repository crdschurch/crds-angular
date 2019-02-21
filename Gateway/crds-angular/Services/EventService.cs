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

        public EventToolDto GetEventRoomDetails(int eventId)
        {
            return GetEventDetails(eventId, false, true);
        }

        public EventToolDto GetEventReservation(int eventId)
        {
            var details = GetEventDetails(eventId, true, false);
            details.IsSeries = _eventService.IsEventSeries(eventId);
            return details;
        }

        private EventToolDto GetEventDetails(int eventId, bool includeEquipment, bool includeParticipants)
        {
            try
            {
                var e = GetEvent(eventId);
                var dto = Mapper.Map<EventToolDto>(e);

                dto.Rooms = PopulateRoomReservations(eventId, includeEquipment, includeParticipants);

                var groups = _eventService.GetEventGroupsForEventAPILogin(eventId);

                if (groups.Any(childcareGroups => childcareGroups.GroupTypeId == childcareGroupTypeID))
                {
                    var group = _groupService.getGroupDetails(groups.First(childcareGroup => childcareGroup.GroupTypeId == childcareGroupTypeID).GroupId);
                    dto.Group = Mapper.Map<GroupDTO>(group);
                }



                return dto;
            }
            catch (Exception ex)
            {
                var msg = "Event Service: CreateEventReservation";
                _logger.Error(msg, ex);
                throw new Exception(msg, ex);
            }
        }

        private List<EventRoomDto> PopulateRoomReservations(int eventId, bool includeEquipment, bool includeParticipants)
        {
            var rooms = _roomService.GetRoomReservations(eventId);
            var roomDto = new List<EventRoomDto>();

            foreach (var room in rooms)
            {
                var r = new EventRoomDto();
                r.Cancelled = room.Cancelled;
                r.Hidden = room.Hidden;
                r.LayoutId = room.RoomLayoutId;
                r.Notes = room.Notes;
                r.RoomId = room.RoomId;
                r.RoomReservationId = room.EventRoomId;
                r.Capacity = room.Capacity;
                r.CheckinAllowed = room.CheckinAllowed;
                r.Name = room.Name;
                r.Label = room.Label;
                r.Volunteers = room.Volunteers;

                if (includeEquipment)
                {
                    var equipmentDto = new List<EventRoomEquipmentDto>();
                    var equipment = _equipmentService.GetEquipmentReservations(eventId, room.RoomId);
                    foreach (var equipmentReservation in equipment)
                    {
                        var eq = new EventRoomEquipmentDto();
                        eq.Cancelled = equipmentReservation.Cancelled;
                        eq.EquipmentId = equipmentReservation.EquipmentId;
                        eq.QuantityRequested = equipmentReservation.QuantityRequested;
                        eq.EquipmentReservationId = equipmentReservation.EventEquipmentId;
                        eq.Notes = equipmentReservation.Notes;
                        equipmentDto.Add(eq);
                    }
                    r.Equipment = equipmentDto;
                }

                if (includeParticipants)
                {
                    var p = _eventParticipantService.GetEventParticipants(eventId, room.RoomId);
                    r.ParticipantsAssigned = p == null ? 0 : p.Count;
                    r.ParticipantsCheckedIn = p == null ? 0 : p.Where(participant => participant.ParticipantStatus == 3).ToList().Count;
                    r.ParticipantsSignedIn = p == null ? 0 : p.Where(participant => participant.ParticipantStatus == 4).ToList().Count;
                }

                roomDto.Add(r);
            }
            return roomDto;
        }

        public bool UpdateEventReservation(EventToolDto eventReservation, int eventId, string token)
        {
            try
            {
                var oldEventDetails = GetEventDetails(eventId, true, false);

                foreach (var room in oldEventDetails.Rooms)
                {
                    if (!room.Cancelled)
                    {
                        if (!eventReservation.Rooms.Any(r => r.RoomId == room.RoomId) || oldEventDetails.StartDateTime != eventReservation.StartDateTime)
                        {
                            room.Cancelled = true;
                            foreach (var eq in room.Equipment)
                            {
                                eq.Cancelled = true;
                                eq.Notes = "***Cancelled***" + eq.Notes;
                            }
                            UpdateEventRoom(room, eventId, token);
                        }
                    }
                }

                UpdateEventChildcareGroup(oldEventDetails, eventReservation, eventId, token);
                UpdateEvent(eventReservation, eventId, token);

                foreach (var room in eventReservation.Rooms)
                {
                    UpdateEventRoom(room, eventId, token);
                }
            }
            catch (Exception ex)
            {
                var msg = "Event Service: UpdateEventReservation";
                _logger.Error(msg, ex);
                throw new Exception(msg, ex);
            }
            return true;
        }

        private void UpdateEventChildcareGroup(EventToolDto oldEventDetails, EventToolDto eventReservation, int eventId, string token)
        {
            bool wasChildcare = oldEventDetails.EventTypeId == childcareEventTypeID;
            bool isChildcare = eventReservation.EventTypeId == childcareEventTypeID;

            //if it use to be a childcare event, but isn't anymore, remove the group
            if (wasChildcare && !isChildcare)
            {
                _eventService.DeleteEventGroupsForEvent(eventId, token, childcareGroupTypeID);
                _groupService.EndDateGroup(oldEventDetails.Group.GroupId, null, null);
            }
            //now is a childcare event but was not before so add a group
            else if (!wasChildcare && isChildcare)
            {
                eventReservation.Group.CongregationId = eventReservation.CongregationId;
                var groupid = AddGroup(eventReservation.Group);
                AddEventGroup(eventId, groupid, token);
            }
            //it was and still is a childcare event
            else if (wasChildcare && isChildcare)
            {
                var group = _eventService.GetEventGroupsForEventAPILogin(eventId).FirstOrDefault(i => i.GroupTypeId == childcareGroupTypeID);

                eventReservation.Group.GroupId = group.GroupId;
                eventReservation.Group.CongregationId = eventReservation.CongregationId;
                UpdateGroup(eventReservation.Group);
            }
        }

        public EventRoomDto UpdateEventRoom(EventRoomDto eventRoom, int eventId, string token)
        {
            try
            {
                if (eventRoom.RoomReservationId == 0)
                {
                    AddRoom(eventId, eventRoom, token);
                }
                else
                {
                    UpdateRoom(eventId, eventRoom, token);
                }

                foreach (var equipment in eventRoom.Equipment)
                {
                    if (equipment.EquipmentReservationId == 0)
                    {
                        AddEquipment(equipment, eventId, eventRoom, token);
                    }
                    else
                    {
                        UpdateEquipment(equipment, eventId, eventRoom, token);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Event Service: UpdateEventReservation";
                _logger.Error(msg, ex);
                throw new Exception(msg, ex);
            }

            return eventRoom;
        }

        public bool CreateEventReservation(EventToolDto eventTool, string token)
        {
            try
            {
                var eventId = AddEvent(eventTool);

                foreach (var room in eventTool.Rooms)
                {
                    AddRoom(eventId, room, token);
                    foreach (var equipment in room.Equipment)
                    {
                        AddEquipment(equipment, eventId, room, token);
                    }
                }

                if (eventTool.Group != null)
                {
                    var groupid = AddGroup(eventTool.Group);
                    AddEventGroup(eventId, groupid, token);
                }
            }
            catch (Exception ex)
            {
                var msg = "Event Service: CreateEventReservation";
                _logger.Error(msg, ex);
                throw new Exception(msg, ex);
            }
            return true;
        }

        private int AddGroup(GroupDTO group)
        {
            //translate the dto to the mp object
            var mpgroup = new MpGroup
            {
                Name = @group.GroupName,
                GroupType = @group.GroupTypeId,
                Full = @group.GroupFullInd,
                WaitList = @group.WaitListInd,
                WaitListGroupId = @group.WaitListGroupId,
                PrimaryContactName = @group.PrimaryContactName,
                PrimaryContactEmail = @group.PrimaryContactEmail,
                ChildCareAvailable = @group.ChildCareAvailable,
                MinimumAge = @group.MaximumAge,
                GroupDescription = @group.GroupDescription,
                MinistryId = @group.MinistryId,
                MeetingTime = @group.MeetingTime,
                MeetingDayId = @group.MeetingDayId,
                CongregationId = @group.CongregationId,
                StartDate = @group.StartDate,
                EndDate = @group.EndDate,
                AvailableOnline = @group.AvailableOnline,
                RemainingCapacity = @group.RemainingCapacity,
                ContactId = @group.ContactId,
                GroupRoleId = @group.GroupRoleId,
                MaximumAge = @group.MaximumAge,
                MinimumParticipants = @group.MinimumParticipants,
                TargetSize = @group.TargetSize
            };

           return  _groupService.CreateGroup(mpgroup);
        }

        private void UpdateGroup(GroupDTO group)
        {
            //translate the dto to the mp object
            var mpgroup = new MpGroup
            {
                GroupId = @group.GroupId,
                Name = @group.GroupName,
                GroupType = @group.GroupTypeId,
                Full = @group.GroupFullInd,
                WaitList = @group.WaitListInd,
                WaitListGroupId = @group.WaitListGroupId,
                PrimaryContactName = @group.PrimaryContactName,
                PrimaryContactEmail = @group.PrimaryContactEmail,
                ChildCareAvailable = @group.ChildCareAvailable,
                MinimumAge = @group.MaximumAge,
                GroupDescription = @group.GroupDescription,
                MinistryId = @group.MinistryId,
                MeetingTime = @group.MeetingTime,
                MeetingDayId = @group.MeetingDayId,
                CongregationId = @group.CongregationId,
                StartDate = @group.StartDate,
                EndDate = @group.EndDate,
                AvailableOnline = @group.AvailableOnline,
                RemainingCapacity = @group.RemainingCapacity,
                ContactId = @group.ContactId,
                GroupRoleId = @group.GroupRoleId,
                MaximumAge = @group.MaximumAge,
                MinimumParticipants = @group.MinimumParticipants,
                TargetSize = @group.TargetSize
            };

            _groupService.UpdateGroup(mpgroup);
        }

        public int AddEventGroup(int eventId, int groupId, string token)
        {
            var eventGroup = new MpEventGroup
            {
                EventId = eventId,
                GroupId = groupId,
                DomainId = 1
            };

            return _eventService.CreateEventGroup(eventGroup);
        }

        private void AddEquipment(EventRoomEquipmentDto equipment, int eventId, EventRoomDto room, string token)
        {
            var equipmentReservation = new MpEquipmentReservationDto();
            equipmentReservation.Cancelled = false;
            equipmentReservation.EquipmentId = equipment.EquipmentId;
            equipmentReservation.EventId = eventId;
            equipmentReservation.QuantityRequested = equipment.QuantityRequested;
            equipmentReservation.RoomId = room.RoomId;
            equipmentReservation.Notes = room.Notes;
            _equipmentService.CreateEquipmentReservation(equipmentReservation);
        }

        private void UpdateEquipment(EventRoomEquipmentDto equipment, int eventId, EventRoomDto room, string token)
        {
            var equipmentReservation = new MpEquipmentReservationDto();
            equipmentReservation.Cancelled = equipment.Cancelled;
            equipmentReservation.EquipmentId = equipment.EquipmentId;
            equipmentReservation.EventEquipmentId = equipment.EquipmentReservationId;
            equipmentReservation.EventId = eventId;
            equipmentReservation.QuantityRequested = equipment.QuantityRequested;
            equipmentReservation.RoomId = room.RoomId;
            equipmentReservation.Notes = equipment.Notes ?? room.Notes;
            _equipmentService.UpdateEquipmentReservation(equipmentReservation);
        }

        private int AddRoom(int eventId, EventRoomDto room, string token)
        {
            var roomReservation = new MpRoomReservationDto();
            roomReservation.Cancelled = false;
            roomReservation.EventId = eventId;
            roomReservation.Hidden = room.Hidden;
            roomReservation.Notes = room.Notes;
            roomReservation.RoomId = room.RoomId;
            roomReservation.RoomLayoutId = room.LayoutId;
            roomReservation.Capacity = room.Capacity;
            roomReservation.Label = room.Label;
            roomReservation.CheckinAllowed = room.CheckinAllowed;
            roomReservation.Volunteers = room.Volunteers;
            return _roomService.CreateRoomReservation(roomReservation);
        }

        private void UpdateRoom(int eventId, EventRoomDto room, string token)
        {
            var roomReservation = new MpRoomReservationDto();
            roomReservation.Cancelled = room.Cancelled;
            roomReservation.EventId = eventId;
            roomReservation.EventRoomId = room.RoomReservationId;
            roomReservation.Hidden = room.Hidden;
            roomReservation.Notes = room.Notes;
            roomReservation.RoomId = room.RoomId;
            roomReservation.RoomLayoutId = room.LayoutId;
            roomReservation.Capacity = room.Capacity;
            roomReservation.Label = room.Label;
            roomReservation.CheckinAllowed = room.CheckinAllowed;
            roomReservation.Volunteers = room.Volunteers;
            _roomService.UpdateRoomReservation(roomReservation);
        }

        public int AddEvent(EventToolDto eventReservation)
        {
            var eventDto = PopulateReservationDto(eventReservation);
            var eventId = _eventService.CreateEvent(eventDto);
            return eventId;
        }

        public void UpdateEvent(EventToolDto eventReservation, int eventId, string token)
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

        public void RegisterForEvent(EventRsvpDto eventDto, string token)
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

        private void SendRsvpMessage(List<RegisterEventObj> saved, string token)
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

        public bool CopyEventSetup(int eventTemplateId, int eventId, string token)
        {
            // event groups and event rooms need to be removed before adding new ones
            _eventService.DeleteEventGroupsForEvent(eventId, token);
            _roomService.DeleteEventRoomsForEvent(eventId, token);

            // get event rooms (room reservation DTOs) and event groups for the template
            var eventRooms = _roomService.GetRoomReservations(eventTemplateId);
            var eventGroups = _eventService.GetEventGroupsForEvent(eventTemplateId, token);

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

        public List<MpEvent> GetEventsBySite(string site, string token, DateTime startDate, DateTime endDate)
        {
            var eventTemplates = _eventService.GetEventsBySite(site, token, startDate, endDate);

            return eventTemplates;
        }

        public List<MpEvent> GetEventTemplatesBySite(string site, string token)
        {
            var eventTemplates = _eventService.GetEventTemplatesBySite(site, token);

            return eventTemplates;
        }

        public int GetEventParticipantByEventAndContact(int eventId, int contactId)
        {
            return _eventParticipantService.GetEventParticipantByContactId(eventId, contactId);
        }
    }
}