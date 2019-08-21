using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Event = crds_angular.Models.Crossroads.Events.Event;
using IAttributeRepository = MinistryPlatform.Translation.Repositories.Interfaces.IAttributeRepository;
using IEventRepository = MinistryPlatform.Translation.Repositories.Interfaces.IEventRepository;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using IObjectAttributeService = crds_angular.Services.Interfaces.IObjectAttributeService;
using crds_angular.Util.Interfaces;

namespace crds_angular.Services
{
    public class GroupService : IGroupService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (GroupService));
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IGroupRepository _mpGroupRepository;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IEventRepository _eventService;
        private readonly IContactRelationshipRepository _contactRelationshipService;
        private readonly IServeService _serveService;
        private readonly IParticipantRepository _participantService;
        private readonly ICommunicationRepository _communicationService;
        private readonly IContactRepository _contactService;
        private readonly IObjectAttributeService _objectAttributeService;
        private readonly IApiUserRepository _apiUserService;
        private readonly IAttributeRepository _attributeRepository;
        private readonly IEmailCommunication _emailCommunicationService;
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IAttributeService _attributeService;
        private readonly int  _onsiteGroupTypeId;
        private readonly int _childcareEventTypeId;
        private readonly IDateTime _dateTimeWrapper;



        /// <summary>
        /// This is retrieved in the constructor from AppSettings
        /// </summary>
        private readonly int _groupRoleDefaultId;
        private readonly int _defaultContactEmailId;
        private readonly int _defaultAuthorUserId;
        private readonly int _journeyGroupTypeId;
        private readonly int _groupCategoryAttributeTypeId;
        private readonly int _groupSubcategoryAttributeTypeId;
        private readonly int _groupAutoJourneyAttributeTypeId;
        private readonly int _groupTypeAttributeTypeId;
        private readonly int _groupAgeRangeAttributeTypeId;
        private readonly int _groupRoleLeader;        
        private readonly int _domainId;
        private readonly int _removeSelfFromGroupTemplateId;

        public GroupService(IAwsCloudsearchService awsCloudsearchService,
                            IGroupRepository mpGroupRepository,
                            IConfigurationWrapper configurationWrapper,
                            IEventRepository eventService,
                            IContactRelationshipRepository contactRelationshipService,
                            IServeService serveService,
                            IParticipantRepository participantService,
                            ICommunicationRepository communicationService,
                            IContactRepository contactService, 
                            IObjectAttributeService objectAttributeService, 
                            IApiUserRepository apiUserService, 
                            IAttributeRepository attributeRepository,
                            IEmailCommunication emailCommunicationService,
                            IUserRepository userRepository,
                            IInvitationRepository invitationRepository,
                            IAttributeService attributeService,
                            IDateTime dateTimeWrapper)

        {
            _awsCloudsearchService = awsCloudsearchService;
            _mpGroupRepository = mpGroupRepository;
            _configurationWrapper = configurationWrapper;
            _eventService = eventService;
            _contactRelationshipService = contactRelationshipService;
            _serveService = serveService;
            _participantService = participantService;
            _communicationService = communicationService;
            _contactService = contactService;
            _objectAttributeService = objectAttributeService;
            _apiUserService = apiUserService;
            _attributeRepository = attributeRepository;
            _emailCommunicationService = emailCommunicationService;
            _userRepository = userRepository;
            _invitationRepository = invitationRepository;
            _attributeService = attributeService;
            _domainId = configurationWrapper.GetConfigIntValue("DomainId");

            _groupRoleDefaultId = _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
            _defaultAuthorUserId = configurationWrapper.GetConfigIntValue("DefaultAuthorUser");
            _defaultContactEmailId = _configurationWrapper.GetConfigIntValue("DefaultContactEmailId");
            _journeyGroupTypeId = configurationWrapper.GetConfigIntValue("JourneyGroupTypeId");
            _groupCategoryAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupCategoryAttributeTypeId");
            _groupSubcategoryAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupSubcategoryAttributeTypeId");
            _groupAutoJourneyAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupAutoJourneyAttributeTypeId");
            _groupTypeAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupTypeAttributeTypeId");
            _groupAgeRangeAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupAgeRangeAttributeTypeId");
            _groupRoleLeader = configurationWrapper.GetConfigIntValue("GroupRoleLeader");            
            _onsiteGroupTypeId = _configurationWrapper.GetConfigIntValue("OnsiteGroupTypeId");
            _childcareEventTypeId = _configurationWrapper.GetConfigIntValue("ChildcareEventType");
            _removeSelfFromGroupTemplateId = _configurationWrapper.GetConfigIntValue("RemoveSelfFromGroupTemplateId");            
            _dateTimeWrapper = dateTimeWrapper;

        }


        public GroupDTO CreateGroup(GroupDTO group)
        {
            try
            {
                var mpGroup = Mapper.Map<MpGroup>(group);

                if (group.AttributeTypes.ContainsKey(_groupCategoryAttributeTypeId) && group.AttributeTypes[90].Attributes.Any(a => a.AttributeId == 0))
                {
                    var categoryAttributes = Mapper.Map<List<MpAttribute>>(group.AttributeTypes[90].Attributes);

                    categoryAttributes = _attributeService.CreateMissingAttributes(categoryAttributes, _groupCategoryAttributeTypeId);
                    group.AttributeTypes[_groupCategoryAttributeTypeId].Attributes = Mapper.Map<List<ObjectAttributeDTO>>(categoryAttributes);
                }

                if (group.AttributeTypes.ContainsKey(_groupSubcategoryAttributeTypeId) && group.AttributeTypes[92].Attributes.Any(a => a.AttributeId == 0))
                {
                    var categoryAttributes = Mapper.Map<List<MpAttribute>>(group.AttributeTypes[92].Attributes);

                    categoryAttributes = _attributeService.CreateMissingAttributes(categoryAttributes, _groupSubcategoryAttributeTypeId);
                    group.AttributeTypes[_groupSubcategoryAttributeTypeId].Attributes = Mapper.Map<List<ObjectAttributeDTO>>(categoryAttributes);
                }

                group.GroupId = _mpGroupRepository.CreateGroup(mpGroup);
                //save group attributes
                var configuration = MpObjectAttributeConfigurationFactory.Group();            
                _objectAttributeService.SaveObjectAttributes(group.GroupId, group.AttributeTypes, group.SingleAttributes, configuration);

                if (group.MinorAgeGroupsAdded)
                {
                    _mpGroupRepository.SendNewStudentMinistryGroupAlertEmail((List<MpGroupParticipant>) mpGroup.Participants, mpGroup);
                }

                _awsCloudsearchService.UploadSingleGroupToAwsFromMp(group.GroupId);

            }
            catch (Exception e)
            {
                var message = String.Format("Could not create group {0}", group.GroupName);
                _logger.Error(message, e);
                throw (new ApplicationException(message, e));
            }

            return group;
        }

        public void addParticipantToGroupNoEvents(int groupId, ParticipantSignup participant)
        {
            MpGroup group;
            try
            {
                group = _mpGroupRepository.getGroupDetails(groupId);
            }
            catch (Exception e)
            {
                var message = String.Format("Could not retrieve group details for group {0}: {1}", groupId, e.Message);
                _logger.Error(message, e);
                throw (new ApplicationException(message, e));
            }

            checkSpaceRemaining(new List<ParticipantSignup> {participant}, group);

            try
            {
                var roleId = participant.groupRoleId ?? _groupRoleDefaultId;

                var participantId = participant.particpantId.Value;
                var groupParticipantId = _mpGroupRepository.AddParticipantToGroup(participantId,
                                                                           Convert.ToInt32(groupId),
                                                                           roleId,
                                                                           participant.childCareNeeded, false,
                                                                           DateTime.Now,
                                                                           null, participant.EnrolledBy);

                var configuration = MpObjectAttributeConfigurationFactory.GroupParticipant();
                _objectAttributeService.SaveObjectAttributes(groupParticipantId, participant.AttributeTypes, participant.SingleAttributes, configuration);

                if (participant.capacityNeeded > 0)
                {
                    DecrementCapacity(participant.capacityNeeded, group);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Could not add user to group", e);
                throw;
            }
        }

        public void endDateGroupParticipant(int groupId, int groupParticipantId)
        {
            _mpGroupRepository.endDateGroupParticipant(groupParticipantId,groupId, DateTime.Now);
        }

        public void addParticipantsToGroup(int groupId, List<ParticipantSignup> participants)
        {
            MpGroup group;
            try
            {
                group = _mpGroupRepository.getGroupDetails(groupId);
            }
            catch (Exception e)
            {
                var message = String.Format("Could not retrieve group details for group {0}: {1}", groupId, e.Message);
                _logger.Error(message, e);
                throw (new ApplicationException(message, e));
            }

            checkSpaceRemaining(participants, group);

            try
            {
                foreach (var participant in participants)
                {
                    var roleId = participant.groupRoleId ?? _groupRoleDefaultId;

                    var participantId = participant.particpantId.Value;

                    int groupParticipantId = _mpGroupRepository.GetParticipantGroupMemberId(Convert.ToInt32(groupId), participantId);

                    if (groupParticipantId < 0) 
                    {
                        groupParticipantId = _mpGroupRepository.AddParticipantToGroup(participantId,
                                                                                      Convert.ToInt32(groupId),
                                                                                      roleId,
                                                                                      participant.childCareNeeded,
                                                                                      false,
                                                                                      DateTime.Now);

                        var configuration = MpObjectAttributeConfigurationFactory.GroupParticipant();
                        _objectAttributeService.SaveObjectAttributes(groupParticipantId, participant.AttributeTypes, participant.SingleAttributes, configuration);

                        if (participant.capacityNeeded > 0)
                        {
                            DecrementCapacity(participant.capacityNeeded, group);
                        }

                        _logger.Debug("Added user - group/participant id = " + groupParticipantId);
                    }
                    else
                    {
                        _logger.Debug("User "+participantId+ " was already a member of group "+groupId);
                    }

                    var events = _mpGroupRepository.getAllEventsForGroup(Convert.ToInt32(groupId), _dateTimeWrapper.Today);
                    _logger.Debug("Scheduled events for this group: " + events);
                    if (events != null && events.Count > 0)
                    {
                        foreach (var e in events.Where(x => x.EventType != (Convert.ToString(_childcareEventTypeId))))
                        {
                            //SafeRegisterParticipant will not register again if they are already registered
                            _eventService.SafeRegisterParticipant( e.EventId, participantId, groupId, groupParticipantId);
                            _logger.Debug("Added participant " + participant + " to group event " + e.EventId);
                        }
                    }

                    if (participant.SendConfirmationEmail)
                    {
                        var waitlist = group.GroupType == _configurationWrapper.GetConfigIntValue("GroupType_Waitlist");
                        _mpGroupRepository.SendCommunityGroupConfirmationEmail(participantId, groupId, waitlist, participant.childCareNeeded);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Could not add user to group", e);
                throw;
            }
        }

        private void checkSpaceRemaining(List<ParticipantSignup> participants, MpGroup group)
        {
            var numParticipantsToAdd = participants.Count;
            var spaceRemaining = group.TargetSize - group.Participants.Count;
            if (((group.TargetSize > 0) && (numParticipantsToAdd > spaceRemaining)) || (group.Full))
            {
                throw (new GroupFullException(group));
            }
        }

        private void DecrementCapacity(int capacityNeeded, MpGroup group)
        {
            group.RemainingCapacity = group.RemainingCapacity - capacityNeeded;
            _logger.Debug("Remaining Capacity After decrement: " + capacityNeeded + " : " + group.RemainingCapacity);
            _mpGroupRepository.UpdateGroupRemainingCapacity(group);
        }

        public int addContactToGroup(int groupId, int contactId, int roleId)
        {
            MpParticipant participant;

            try
            {
                participant = _participantService.GetParticipant(contactId);
            }
            catch (Exception e)
            {
                var message = $"Could not retrieve particpant for contact {contactId}: {e.Message}";
                _logger.Error(message, e);
                throw (new ApplicationException(message, e));
            }

            try
            {
                return _mpGroupRepository.AddParticipantToGroup(participant.ParticipantId, groupId, roleId, false, false, DateTime.Now);
            }
            catch (Exception e)
            {
                _logger.Error("Could not add contact to group", e);
                throw;
            }
        }

        public List<Event> GetGroupEvents(int groupId)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();
            var eventTypes = _mpGroupRepository.GetEventTypesForGroup(groupId, apiToken);
            var events = new List<MpEvent>();
            foreach (var eventType in eventTypes.Where(eventType => !string.IsNullOrEmpty(eventType)))
            {
                events.AddRange(_eventService.GetEvents(eventType, apiToken));
            }
            var futureEvents = events.Where(e => e.EventStartDate >= DateTime.Now).OrderBy(e => e.EventStartDate);
            var eventList = Mapper.Map<List<Event>>(futureEvents.GroupBy(x => x.EventId).Select(y => y.First()));
            return eventList;
        }

        public List<GroupContactDTO> GetGroupMembersByEvent(int groupId, int eventId, string recipients)
        {
            return recipients == "current" ? GroupMembersThatAreServingForEvent(groupId, eventId) : GroupMembersThatHaveNotRepliedToEvent(groupId, eventId);
        }

        private List<GroupContactDTO> GroupMembersThatAreServingForEvent(int groupId, int eventId)
        {
            return _mpGroupRepository.getEventParticipantsForGroup(groupId, eventId)
                .Select(part => new GroupContactDTO
                {
                    ContactId = part.ContactId,
                    DisplayName = part.LastName + ", " + part.NickName
                }).ToList();
        }

        private List<GroupContactDTO> GroupMembersThatHaveNotRepliedToEvent(int groupId, int eventId)
        {
            try
            {
                var groupMembers = _mpGroupRepository.getGroupDetails(groupId).Participants.Select(p =>
                                                                                                    new MpGroupParticipant
                                                                                                    {
                                                                                                        ContactId = p.ContactId,
                                                                                                        LastName = p.LastName,
                                                                                                        NickName = p.NickName,
                                                                                                        ParticipantId = p.ParticipantId
                                                                                                    }
                    ).ToList();
                var evt = Mapper.Map<Event>(_eventService.GetEvent(eventId));
                return _serveService.PotentialVolunteers(groupId, evt, groupMembers);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public GroupDTO GetGroupDetails(int groupId)
        {
            return Mapper.Map<MpGroup, GroupDTO>(_mpGroupRepository.getGroupDetails(groupId));
        }

        public GroupDTO GetGroupDetailsWithAttributes(int groupId)
        {
            var group = GetGroupDetails(groupId);
            GetGroupAttributes(new List<GroupDTO> { group });
            return group;
        }

        public GroupDTO GetGroupDetailsByInvitationGuid(string invitationGuid)
        {
            var invitation = _invitationRepository.GetOpenInvitation(invitationGuid);

            var group = Mapper.Map<MpGroup, GroupDTO>(_mpGroupRepository.GetSmallGroupDetailsById(invitation.SourceId));
            var groups = new List<GroupDTO> {@group};

            GetGroupAttributes(groups);
            GetGroupParticipants(groups);

            return groups[0];
        }

        public GroupDTO getGroupDetails(int groupId, int contactId, MpParticipant participant)
        {
            int participantId = participant.ParticipantId;
            MpGroup g = _mpGroupRepository.getGroupDetails(groupId);
            var apiToken = _apiUserService.GetDefaultApiClientToken();

            var signupRelations = _mpGroupRepository.GetGroupSignupRelations(g.GroupType);

            var currRelationships = _contactRelationshipService.GetMyCurrentRelationships(contactId, apiToken);

            var events = _mpGroupRepository.getAllEventsForGroup(groupId);

            MpContactRelationship[] familyToReturn = null;

            if (currRelationships != null)
            {
                familyToReturn = currRelationships.Where(
                    c => signupRelations.Select(s => s.RelationshipId).Contains(c.Relationship_Id)).ToArray();
            }

            
            var configuration = MpObjectAttributeConfigurationFactory.Group();
            var attributesTypes = _objectAttributeService.GetObjectAttributes(groupId, configuration);

            var detail = new GroupDTO();
            {
                detail.ContactId = g.ContactId;
                detail.CongregationId = g.CongregationId;
                detail.KidsWelcome = g.KidsWelcome;
                detail.GroupName = g.Name;
                detail.GroupDescription = g.GroupDescription;
                detail.GroupId = g.GroupId;
                detail.GroupFullInd = g.Full;
                detail.WaitListInd = g.WaitList ?? false;
                detail.ChildCareAvailable = g.ChildCareAvailable;
                detail.WaitListGroupId = g.WaitListGroupId;
                detail.OnlineRsvpMinimumAge = g.MinimumAge;
                detail.MeetingFrequencyID = g.MeetingFrequencyID;
                detail.AvailableOnline = g.AvailableOnline;
                detail.MeetingTime = g.MeetingTime;
                detail.MeetingDayId = g.MeetingDayId;
                detail.Address = Mapper.Map<MpAddress, AddressDTO>(g.Address);
                detail.StartDate = g.StartDate;
                detail.Participants = (g.Participants.Count > 0) ? g.Participants.Select(p => Mapper.Map<MpGroupParticipant, GroupParticipantDTO>(p)).ToList() : null;

                if (events != null)
                {
                    detail.Events = events.Select(Mapper.Map<MpEvent, Event>).ToList();
                }
                //the first instance of family must always be the logged in user
                var fam = new SignUpFamilyMembers
                {
                    EmailAddress = participant.EmailAddress,
                    PreferredName = participant.PreferredName,
                    UserInGroup = _mpGroupRepository.checkIfUserInGroup(participantId, g.Participants),
                    ParticpantId = participantId,
                    ChildCareNeeded = false
                };
                detail.SignUpFamilyMembers = new List<SignUpFamilyMembers> {fam};

                if (familyToReturn != null)
                {
                    foreach (var f in familyToReturn)
                    {
                        var fm = new SignUpFamilyMembers
                        {
                            EmailAddress = f.Email_Address,
                            PreferredName = f.Preferred_Name,
                            UserInGroup = _mpGroupRepository.checkIfUserInGroup(f.Participant_Id, g.Participants),
                            ParticpantId = f.Participant_Id,
                        };
                        detail.SignUpFamilyMembers.Add(fm);
                    }
                }

                detail.AttributeTypes = attributesTypes.MultiSelect;
                detail.SingleAttributes = attributesTypes.SingleSelect;
            }

            return (detail);
        }

        public List<GroupDTO> GetGroupsForParticipant(int participantId)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();
            var groups = _mpGroupRepository.GetGroupsForParticipant(participantId);
            if (groups == null)
                return null;

            var groupDetail = groups.Select(Mapper.Map<MpGroup, GroupDTO>).ToList();
            var configuration = MpObjectAttributeConfigurationFactory.Group();
            var mpAttributes = _attributeRepository.GetAttributes(null);
            foreach (var group in groupDetail)
            {
                var attributesTypes = _objectAttributeService.GetObjectAttributes(group.GroupId, configuration, mpAttributes);
                group.AttributeTypes = attributesTypes.MultiSelect;
                group.SingleAttributes = attributesTypes.SingleSelect;
            }

            return groupDetail;
        }

        public List<GroupDTO> GetGroupsByTypeForParticipant(int participantId, int groupTypeId)
        {
            var groupsByType = _mpGroupRepository.GetGroupsByTypeForParticipant(participantId, groupTypeId);
            if (groupsByType == null)
            {
                return null;
            }

            var groupDetail = groupsByType.Select(Mapper.Map<MpGroup, GroupDTO>).ToList();

            var configuration = MpObjectAttributeConfigurationFactory.Group();
            var mpAttributes = _attributeRepository.GetAttributes(null);
            foreach (var group in groupDetail)
            {               
                var attributesTypes = _objectAttributeService.GetObjectAttributes(group.GroupId, configuration, mpAttributes);
                group.AttributeTypes = attributesTypes.MultiSelect;
                group.SingleAttributes = attributesTypes.SingleSelect;
            }

            return groupDetail;
        }

        /// <summary>
        /// Gets a list of groups with group attributes for either the logged in user or a participant you pass in.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="participantId">if null we load the calling user's participant record</param>
        /// <param name="groupTypeId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<GroupDTO> GetGroupsByTypeOrId(int contactId, int? participantId = null, int[] groupTypeIds = null, int? groupId = null, bool? withParticipants = true, bool? withAttributes = true)
        {
            if (participantId == null) participantId = _participantService.GetParticipant(contactId).ParticipantId;
            var groupsByType = _mpGroupRepository.GetGroupsForParticipantByTypeOrID(participantId.Value, null, groupTypeIds, groupId);

            if (groupsByType == null)
            {
                return null;
            }

            var groupDetail = groupsByType.Select(Mapper.Map<MpGroup, GroupDTO>).ToList();

            if (withAttributes == true)
            {
                var configuration = MpObjectAttributeConfigurationFactory.Group();
                var mpAttributes = _attributeRepository.GetAttributes(null);
                foreach (var group in groupDetail)
                {
                    var attributesTypes = _objectAttributeService.GetObjectAttributes(group.GroupId, configuration, mpAttributes);
                    group.AttributeTypes = attributesTypes.MultiSelect;
                    group.SingleAttributes = attributesTypes.SingleSelect;
                }
            }
            if (withParticipants == true)
            {
                GetGroupParticipants(groupDetail);
            }

            return groupDetail;
        }

        public List<GroupDTO> RemoveOnsiteParticipantsIfNotLeader(List<GroupDTO> groups, int contactId)
        {
            var participant = _participantService.GetParticipant(contactId);
            foreach (var group in groups)
            {
                if (group.GroupTypeId == _onsiteGroupTypeId)
                {
                    var groupLeader = group.Participants.Find(p => p.ParticipantId == participant.ParticipantId && p.GroupRoleId == _groupRoleLeader);
                    if (groupLeader == null)
                        group.Participants = new List<GroupParticipantDTO>();
                }
            }

            return groups;
        }

        public List<GroupDTO> GetGroupByIdForAuthenticatedUser(int contactId, int groupId)
        {
            var participant = _participantService.GetParticipant(contactId);
            var groups = _mpGroupRepository.GetGroupsForParticipant(participant.ParticipantId);
            if (groups == null)
            {
                return null;
            }
            return GetAttributesAndParticipants(groups);
        }

        public List<GroupParticipantDTO> GetGroupParticipantsWithoutAttributes(int groupId)
        {
            var p = _mpGroupRepository.GetGroupParticipants(groupId, true);
            if (p != null && p.Any())
            {
                return p.Select(Mapper.Map<MpGroupParticipant, GroupParticipantDTO>).ToList();
            }

            return new List<GroupParticipantDTO>();
        }

        private List<GroupDTO> GetAttributesAndParticipants(List<MpGroup> groups)
        {
            var groupDetail = groups.Select(Mapper.Map<MpGroup, GroupDTO>).ToList();

            GetGroupAttributes(groupDetail);
            GetGroupParticipants(groupDetail);

            return groupDetail;
        }

        private void GetGroupParticipants(List<GroupDTO> groups)
        {
            foreach (var group in groups)
            {
                var p = _mpGroupRepository.GetGroupParticipants(group.GroupId, true);
                if (p != null && p.Any())
                {
                    group.Participants = p.Select(Mapper.Map<MpGroupParticipant, GroupParticipantDTO>).ToList();
                }
            }
        }

        private void GetGroupAttributes(List<GroupDTO> groups)
        {
            var configuration = MpObjectAttributeConfigurationFactory.Group();

            foreach (var attributeType in new[] { _groupCategoryAttributeTypeId, _groupSubcategoryAttributeTypeId, _groupAutoJourneyAttributeTypeId, _groupTypeAttributeTypeId, _groupAgeRangeAttributeTypeId })
            {
                var types = _attributeRepository.GetAttributes(attributeType);
                foreach (var group in groups)
                {
                    var attributesTypes = _objectAttributeService.GetObjectAttributes(group.GroupId, configuration, types);
                    foreach (var multi in attributesTypes.MultiSelect)
                    {
                        group.AttributeTypes.Add(multi.Key, multi.Value);
                    }
                    foreach (var single in attributesTypes.SingleSelect)
                    {
                        group.SingleAttributes.Add(single.Key, single.Value);
                    }
                }
            }
        }

        public void EndDateGroup(int groupId, int? reasonEndedId)
        {
            _mpGroupRepository.EndDateGroup(groupId, DateTime.Now, reasonEndedId);
        }

        public Participant GetParticipantRecord(int contactId)
        {
            var participant = _participantService.GetParticipant(contactId);
            return new Participant
            {
                Age = participant.Age,
                ApprovedSmallGroupLeader = participant.ApprovedSmallGroupLeader,
                AttendanceStart = participant.AttendanceStart,
                ContactId = participant.ContactId,
                DisplayName = participant.DisplayName,
                EmailAddress = participant.EmailAddress,
                GroupLeaderStatus = participant.GroupLeaderStatus,
                GroupName = participant.GroupName,
                Nickname = participant.Nickname,
                ParticipantId = participant.ParticipantId,
                PreferredName = participant.PreferredName,
                Role = participant.Role
            };
        }

        public void SendJourneyEmailInvite(EmailCommunicationDTO communication, int contactId)
        {
            var participant = GetParticipantRecord(contactId);
            var groups = GetGroupsByTypeForParticipant(participant.ParticipantId, _journeyGroupTypeId);

            if (groups == null ||  groups.Count == 0)
            {
                throw new InvalidOperationException();
            }

            var membership = groups.Where(group => @group.GroupId == communication.groupId).ToList();
            if (membership.Count <= 0)
            {
                throw new InvalidOperationException();
            }

            var invitation = CreateJourneyInvitation(communication, participant);
            _communicationService.SendMessage(invitation);
        }


        private MinistryPlatform.Translation.Models.MpCommunication CreateJourneyInvitation(EmailCommunicationDTO communication, Participant particpant)
        {
            var template = _communicationService.GetTemplate(communication.TemplateId);
            var fromContact = _contactService.GetContactById(_configurationWrapper.GetConfigIntValue("DefaultContactEmailId"));
            var replyTo = _contactService.GetContactById(particpant.ContactId);
            var mergeData = SetupMergeData(particpant.PreferredName, communication.groupId.Value);

            return new MinistryPlatform.Translation.Models.MpCommunication
            {
                AuthorUserId = 5,
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact { ContactId = _defaultContactEmailId, EmailAddress = fromContact.Email_Address },
                ReplyToContact = new MpContact {ContactId = _defaultContactEmailId, EmailAddress = replyTo.Email_Address },
                ToContacts = new List<MpContact> { new MpContact { ContactId = fromContact.Contact_ID, EmailAddress = communication.emailAddress } },
                MergeData = mergeData
            };
        }

        private Dictionary<string, object> SetupMergeData(string preferredName, int groupId)
        {
            var mergeData = new Dictionary<string, object>
            {
                {"BaseUrl", _configurationWrapper.GetConfigValue("BaseUrl")},
                {"GroupId", groupId},
                {"PreferredName", preferredName},
            };
            return mergeData;
        }

        public List<GroupParticipantDTO> GetGroupParticipants(int groupId, bool active = true)
        {
            var groupParticipants = _mpGroupRepository.GetGroupParticipants(groupId, active);       
            if (groupParticipants == null)
            {
                return null;
            }
            var participants = groupParticipants.Select(Mapper.Map<MpGroupParticipant, GroupParticipantDTO>).ToList();

            var configuration = MpObjectAttributeConfigurationFactory.GroupParticipant();

            var apiToken = _apiUserService.GetDefaultApiClientToken();

            foreach (var participant in participants)
            {
                var attributesTypes = _objectAttributeService.GetObjectAttributes(participant.GroupParticipantId, configuration);
                participant.AttributeTypes = attributesTypes.MultiSelect;
                participant.SingleAttributes = attributesTypes.SingleSelect;
            }

            return participants;
        }

        public void LookupParticipantIfEmpty(int loggedInPartId, List<ParticipantSignup> partId)
        {
            var participantsToLookup = partId.Where(x => x.particpantId == null).ToList();
            if (participantsToLookup.Count <= 0)
            {
                return;
            }

            foreach (var currentParticpant in participantsToLookup)
            {
                currentParticpant.particpantId = loggedInPartId;
            }
        }

        public void RemoveParticipantFromGroup(int contactId, int groupId, int groupParticipantId)
        {
            try
            {
                this.endDateGroupParticipant(groupId, groupParticipantId);
                this.SendAllGroupLeadersMemberRemovedEmail(contactId, groupId);
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException($"Could not remove group participant {groupParticipantId} from group {groupId}", e);
            }
        }

        public void SendAllGroupLeadersMemberRemovedEmail(int contactId, int groupId)
        {
            var requestorParticipant = _participantService.GetParticipant(contactId);
            var group = this.GetGroupDetails(groupId);

            var leaders = @group.Participants.
                Where(groupParticipant => groupParticipant.GroupRoleId == _groupRoleLeader).
                Select(groupParticipant => new MpContact
                {
                    ContactId = groupParticipant.ContactId,
                    EmailAddress = groupParticipant.Email,
                    LastName = groupParticipant.LastName,
                    Nickname = groupParticipant.NickName
                }).ToList();

            var mergeData = new Dictionary<string, object>
                {
                    {"Group_Participant_Name", requestorParticipant.DisplayName},
                    {"Group_Name", group.GroupName},
                };

            int emailTemplateId = _removeSelfFromGroupTemplateId;            
            var emailTemplate = _communicationService.GetTemplate(emailTemplateId);

            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };
            var replyTo = new MpContact
            {
                ContactId = emailTemplate.ReplyToContactId,
                EmailAddress = emailTemplate.ReplyToEmailAddress
            };

            var message = new MinistryPlatform.Translation.Models.MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = _defaultAuthorUserId,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyTo,
                TemplateId = emailTemplateId,
                ToContacts = leaders,
                MergeData = mergeData
            };
            _communicationService.SendMessage(message);
        }

        public void UpdateHuddleGroupParticipantStatus()
        {
            _participantService.UpdateHuddleGroupParticipantStatus();
        }

        public GroupDTO UpdateGroup(GroupDTO group)
        {
            try
            {
                var mpGroup = Mapper.Map<MpGroup>(group);
                _mpGroupRepository.UpdateGroup(mpGroup);

                var groupParticipants = _mpGroupRepository.GetGroupParticipants(group.GroupId, true);

                if (groupParticipants.Count(participant => participant.StartDate < group.StartDate) > 0)
                {
                    UpdateGroupParticipantStartDate(groupParticipants.Where(part => part.StartDate < group.StartDate).ToList(), group.StartDate);
                }

                if (group.AttributeTypes.ContainsKey(_groupCategoryAttributeTypeId) && group.AttributeTypes[90].Attributes.Any(a => a.AttributeId == 0))
                {
                    var categoryAttributes = Mapper.Map<List<MpAttribute>>(group.AttributeTypes[90].Attributes);

                    categoryAttributes = _attributeService.CreateMissingAttributes(categoryAttributes, _groupCategoryAttributeTypeId);
                    group.AttributeTypes[_groupCategoryAttributeTypeId].Attributes = Mapper.Map<List<ObjectAttributeDTO>>(categoryAttributes);
                }

                if (group.AttributeTypes.ContainsKey(_groupSubcategoryAttributeTypeId) && group.AttributeTypes[92].Attributes.Any(a => a.AttributeId == 0))
                {
                    var categoryAttributes = Mapper.Map<List<MpAttribute>>(group.AttributeTypes[92].Attributes);

                    categoryAttributes = _attributeService.CreateMissingAttributes(categoryAttributes, _groupSubcategoryAttributeTypeId);
                    group.AttributeTypes[_groupSubcategoryAttributeTypeId].Attributes = Mapper.Map<List<ObjectAttributeDTO>>(categoryAttributes);
                }

                var configuration = MpObjectAttributeConfigurationFactory.Group();
                _objectAttributeService.SaveObjectAttributes(group.GroupId, group.AttributeTypes, group.SingleAttributes, configuration);

                if (group.MinorAgeGroupsAdded)
                {
                    var leaders =groupParticipants.Where(p => p.GroupRoleId == _groupRoleLeader).ToList();
                    _mpGroupRepository.SendNewStudentMinistryGroupAlertEmail(leaders, mpGroup);
                }

                _awsCloudsearchService.UpdateGroupInAws(group.GroupId);
            }
            catch (Exception e)
            {
                var message = $"Could not update group {@group.GroupName}";
                _logger.Error(message, e);
                throw (new ApplicationException(message, e));
            }

            return group;
        }

        public void UpdateGroupParticipantRole(GroupParticipantDTO participant)
        {
            try
            {
                var apiToken = _apiUserService.GetDefaultApiClientToken();
                var group = new MpGroup()
                {
                    GroupDescription = "this is the description",
                    Name = "This is the name of the group"
                };
                var mpParticipant = Mapper.Map<MpGroupParticipant>(participant);
                List<MpGroupParticipant> part = new List<MpGroupParticipant>();
                part.Add(mpParticipant);
                _mpGroupRepository.UpdateGroupParticipant(part);
                if (participant.GroupRoleId == _groupRoleLeader)
                {
                    if (_mpGroupRepository.ParticipantGroupHasStudents(apiToken, mpParticipant.ParticipantId, mpParticipant.GroupParticipantId))
                    {
                        _mpGroupRepository.SendNewStudentMinistryGroupAlertEmail(part, group);
                    }
                }
            }
            catch (Exception e)
            {
                var message = String.Format("Could not update group participant {0}", participant.ParticipantId);
                _logger.Error(message, e);
            }
        }

        public void UpdateGroupParticipantRole(int groupId, int participantId, int roleId)
        {
            try
            {
                var apiToken = _apiUserService.GetDefaultApiClientToken();
                var participantList = GetGroupParticipantsWithoutAttributes(groupId);
                var groupParticipant = participantList.Single(s => s.ParticipantId == participantId);
                var mpParticipant = Mapper.Map<MpGroupParticipant>(groupParticipant);
                mpParticipant.GroupRoleId = roleId;


                List<MpGroupParticipant> part = new List<MpGroupParticipant>();
                part.Add(mpParticipant);
                _mpGroupRepository.UpdateGroupParticipant(part);
            }
            catch (Exception e)
            {
                var message = $"Could not update group participant {participantId}";
                _logger.Error(message, e);
                throw e;
            }
        }

        private void UpdateGroupParticipantStartDate(List<MpGroupParticipant> participants, DateTime groupStartDate)
        {
            foreach (var part in participants)
            {
                part.StartDate = groupStartDate;
            }

            _mpGroupRepository.UpdateGroupParticipant(participants);
        }

        public int GetPrimaryContactParticipantId(int groupId)
        {
            return _mpGroupRepository.GetParticipantIdFromGroup(groupId, _apiUserService.GetDefaultApiClientToken());
        }

        public void SendParticipantsEmail(int contactId, List<GroupParticipantDTO> participants, string subject, string body)
        {
            var senderRecord = _participantService.GetParticipant(contactId);

            var fromContact = new MpContact
            {
                ContactId = 1519180,
                EmailAddress = "updates@crossroads.net"
            };

            var replyToContact = new MpContact
            {
                ContactId = senderRecord.ContactId,
                EmailAddress = senderRecord.EmailAddress
            };

            var toContacts = participants.Select(p => new MpContact
            {
                ContactId = p.ContactId,
                EmailAddress = p.Email
            }).ToList();

            var email = new MinistryPlatform.Translation.Models.MpCommunication
            {
                EmailBody = body,
                EmailSubject = subject,
                AuthorUserId = 5,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyToContact,
                ToContacts = toContacts
            };

            _communicationService.SendMessage(email);
        }
    }
}
