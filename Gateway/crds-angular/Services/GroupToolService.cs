using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System.Text.RegularExpressions;
using crds_angular.Services.Analytics;
using Crossroads.ApiVersioning;
using Crossroads.Utilities.Extensions;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models.Finder;

namespace crds_angular.Services
{
    public class GroupToolService : MinistryPlatformBaseService, IGroupToolService
    {
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IGroupToolRepository _groupToolRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupService _groupService;
        private readonly IParticipantRepository _participantRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IContentBlockService _contentBlockService;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IAddressProximityService _addressProximityService;
        private readonly IContactRepository _contactRepository;
        private readonly IAddressProximityService _addressMatrixService;
        private readonly IEmailCommunication _emailCommunicationService;
        private readonly IAttributeService _attributeService;
        private readonly IFinderRepository _finderRepository;
        private readonly IAnalyticsService _analyticsService;


        private readonly int _defaultGroupContactEmailId;
        private readonly int _defaultAuthorUserId;
        private readonly int _defaultGroupRoleId;
        private readonly int _groupRoleLeaderId;
        private readonly int _connectCommunicationTypeEmailSmallGroupLeader; 
        private readonly int _connectCommunicationStatusNA; 
        private readonly int _domainId;
        private readonly int _groupEndedParticipantEmailTemplate;
        private readonly string _baseUrl;
        private readonly int _addressMatrixSearchDepth;
        private readonly int _groupRequestToJoinEmailTemplate;
        private readonly int _anywhereGroupRequestToJoinEmailTemplate;
        private readonly int _groupRequestPendingReminderEmailTemplateId;
        private readonly int _gatheringRequestPendingReminderEmailTemplateId;
        private readonly int _attributeTypeGroupCategory;
        private readonly int _smallGroupTypeId;
        private readonly int _onsiteGroupTypeId;
        private readonly int _anywhereGroupType;
        private readonly int _emailAuthorId;
        private readonly int _genericGroupForCMSMergeEmailTemplateId;


        private const string GroupToolRemoveParticipantEmailTemplateTextTitle = "groupToolRemoveParticipantEmailTemplateText";
        private const string GroupToolRemoveParticipantSubjectTemplateText = "groupToolRemoveParticipantSubjectTemplateText";

        private readonly ILog _logger = LogManager.GetLogger(typeof(GroupToolService));

        public GroupToolService(
            IAwsCloudsearchService awsCloudsearchService,
            IGroupToolRepository groupToolRepository,
            IGroupRepository groupRepository,
            IGroupService groupService,
            IParticipantRepository participantRepository,
            ICommunicationRepository communicationRepository,
            IContentBlockService contentBlockService,
            IConfigurationWrapper configurationWrapper,
            IInvitationRepository invitationRepository,
            IAddressProximityService addressProximityService,
            IContactRepository contactRepository,
            IAddressProximityService addressMatrixService,
            IEmailCommunication emailCommunicationService,
            IAttributeService attributeService,
            IAnalyticsService analyticsService,
            IFinderRepository finderRepository
            )
        {
            _awsCloudsearchService = awsCloudsearchService;
            _groupToolRepository = groupToolRepository;
            _groupRepository = groupRepository;
            _groupService = groupService;
            _participantRepository = participantRepository;
            _communicationRepository = communicationRepository;
            _contentBlockService = contentBlockService;
            _invitationRepository = invitationRepository;
            _addressProximityService = addressProximityService;
            _contactRepository = contactRepository;
            _addressMatrixService = addressMatrixService;
            _emailCommunicationService = emailCommunicationService;
            _attributeService = attributeService;
            _analyticsService = analyticsService;
            _finderRepository = finderRepository;

            _defaultGroupContactEmailId = configurationWrapper.GetConfigIntValue("DefaultGroupContactEmailId");
            _defaultAuthorUserId = configurationWrapper.GetConfigIntValue("DefaultAuthorUser");
            _groupRoleLeaderId = configurationWrapper.GetConfigIntValue("GroupRoleLeader");
            _defaultGroupRoleId = configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
            _groupRequestPendingReminderEmailTemplateId = configurationWrapper.GetConfigIntValue("GroupRequestPendingReminderEmailTemplateId");
            _gatheringRequestPendingReminderEmailTemplateId = configurationWrapper.GetConfigIntValue("GatheringRequestPendingReminderEmailTemplateId");
            _attributeTypeGroupCategory = configurationWrapper.GetConfigIntValue("GroupCategoryAttributeTypeId");

            _genericGroupForCMSMergeEmailTemplateId = configurationWrapper.GetConfigIntValue("GenericGroupForCMSMergeEmailTemplateId");

            _connectCommunicationTypeEmailSmallGroupLeader = configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeEmailSmallGroupLeader");
            _connectCommunicationStatusNA = configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusNA");
            _domainId = configurationWrapper.GetConfigIntValue("DomainId");
            _groupEndedParticipantEmailTemplate = configurationWrapper.GetConfigIntValue("GroupEndedParticipantEmailTemplate");
            _groupRequestToJoinEmailTemplate = configurationWrapper.GetConfigIntValue("GroupRequestToJoinEmailTemplate");
            _anywhereGroupRequestToJoinEmailTemplate = configurationWrapper.GetConfigIntValue("AnywhereGroupRequestToJoinEmailTemplate");
            _baseUrl = configurationWrapper.GetConfigValue("BaseURL");
            _addressMatrixSearchDepth = configurationWrapper.GetConfigIntValue("AddressMatrixSearchDepth");
            
            _smallGroupTypeId = configurationWrapper.GetConfigIntValue("SmallGroupTypeId");
            _onsiteGroupTypeId = configurationWrapper.GetConfigIntValue("OnsiteGroupTypeId");
            _anywhereGroupType = configurationWrapper.GetConfigIntValue("AnywhereGroupTypeId");
            _emailAuthorId = configurationWrapper.GetConfigIntValue("EmailAuthorId");
        }

        public List<Invitation> GetInvitations(int sourceId, int invitationTypeId, string token)
        {
            var invitations = new List<Invitation>();
            try
            {
                VerifyCurrentUserIsGroupLeader(token, sourceId);

                var mpInvitations = _groupToolRepository.GetInvitations(sourceId, invitationTypeId);
                mpInvitations.ForEach(x => invitations.Add(Mapper.Map<Invitation>(x)));
            }
            catch (Exception e)
            {
                var message = $"Exception retrieving invitations for SourceID = {sourceId}, InvitationTypeID = {invitationTypeId}.";
                _logger.Error(message, e);
                throw;
            }
            return invitations;
        }

        public Inquiry GetGroupInquiryForContactId(int groupId, int contactId)
        {
            Inquiry request;
            try
            {
                var mpRequests = _groupToolRepository.GetInquiries(groupId);
                request = Mapper.Map<Inquiry>(mpRequests.Find(x => x.ContactId == contactId));
            }
            catch (Exception e)
            {
                var message = $"Exception retrieving inquiries for group = {groupId} and user = {contactId}.";
                _logger.Error(message, e);
                throw;
            }
            return request;
        }

        public string GetCurrentJourney()
        {
            return _groupToolRepository.GetCurrentJourney();
        }

        public List<Inquiry> GetInquiries(int groupId, string token)
        {
            var requests = new List<Inquiry>();
            try
            {
                VerifyCurrentUserIsGroupLeader(token, groupId);

                var mpRequests = _groupToolRepository.GetInquiries(groupId);
                mpRequests.ForEach(x => requests.Add(Mapper.Map<Inquiry>(x)));
            }
            catch (Exception e)
            {
                var message = $"Exception retrieving inquiries for group id = {groupId}.";
                _logger.Error(message, e);
                throw;
            }
            return requests;
        }

        public void RemoveParticipantFromMyGroup(string token, int groupId, int groupParticipantId, string message = null)
        {
            try
            {
                var myGroup = GetMyGroupInfo(token, groupId);

                _groupService.endDateGroupParticipant(groupId, groupParticipantId);
               
                var participant = myGroup.Group.Participants.Find(p => p.GroupParticipantId == groupParticipantId);
                MpParticipant toParticipant = new MpParticipant
                {
                    ContactId = participant.ContactId,
                    EmailAddress = participant.Email,
                    PreferredName = participant.NickName,
                    ParticipantId = participant.ParticipantId
                };

                try
                {
                    SendGroupParticipantEmail(groupId,
                                              myGroup.Group,
                                              _genericGroupForCMSMergeEmailTemplateId,
                                              toParticipant, 
                                              GroupToolRemoveParticipantSubjectTemplateText,
                                              GroupToolRemoveParticipantEmailTemplateTextTitle,
                                              message,
                                              myGroup.Me);
                }
                catch (Exception e)
                {
                    _logger.Error($"Could not send email to group participant {groupParticipantId} notifying of removal from group {groupId}", e);
                }
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

        public void SendGroupParticipantEmail(int groupId,                                             
                                              GroupDTO group,
                                              int emailTemplateId,
                                              MpParticipant toParticipant,
                                              string subjectTemplateContentBlockTitle = null,
                                              string emailTemplateContentBlockTitle = null,
                                              string message = null,
                                              MpParticipant fromParticipant = null)
        {
            var participant = new GroupParticipantDTO
            {
                ContactId = toParticipant.ContactId,
                Email = toParticipant.EmailAddress,
                NickName = toParticipant.PreferredName,
                ParticipantId = toParticipant.ParticipantId
            };

            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);

            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };

            var replyTo = new MpContact
            {
                ContactId = fromParticipant?.ContactId ?? emailTemplate.ReplyToContactId,
                EmailAddress = fromParticipant == null ? emailTemplate.ReplyToEmailAddress : fromParticipant.EmailAddress
            };

            var leader = _contactRepository.GetContactById(replyTo.ContactId);

            var to = new List<MpContact>
            {
                new MpContact
                {
                    ContactId = participant.ContactId,
                    EmailAddress = participant.Email
                }
            };

            var subjectTemplateText = string.IsNullOrWhiteSpace(subjectTemplateContentBlockTitle)
                ? string.Empty
                : _contentBlockService[subjectTemplateContentBlockTitle].Content ?? string.Empty;

            var emailTemplateText = string.IsNullOrWhiteSpace(emailTemplateContentBlockTitle) ? string.Empty : _contentBlockService[emailTemplateContentBlockTitle].Content;

            var mergeData = getDictionary(participant);
            mergeData["Email_Custom_Message"] = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            mergeData["Group_Name"] = group.GroupName;
            mergeData["Group_Description"] = group.GroupDescription;
            mergeData["Leader_Name"] = leader.Nickname != null && leader.Last_Name != null ? leader.Nickname + " " + leader.Last_Name : replyTo.EmailAddress;
            mergeData["City"] = group.Address.City;
            mergeData["State"] = group.Address.State;
            if (fromParticipant != null)
            {
                mergeData["From_Display_Name"] = fromParticipant.DisplayName;
                mergeData["From_Preferred_Name"] = fromParticipant.PreferredName;
            }

            // Since the templates are coming from content blocks, they may have replacement tokens in them as well.
            // These will not get replaced with merge data in _communicationRepository.SendMessage(), (it doesn't doubly
            // replace) so we'll parse them here before adding them to the merge data. 
            mergeData["Subject_Template_Text"] = _communicationRepository.ParseTemplateBody(Regex.Replace(subjectTemplateText, "<.*?>", string.Empty), mergeData);
            mergeData["Email_Template_Text"] = _communicationRepository.ParseTemplateBody(emailTemplateText, mergeData);

            var email = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = _emailAuthorId,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyTo,
                TemplateId = emailTemplateId,
                ToContacts = to,
                MergeData = mergeData
            };
            _communicationRepository.SendMessage(email);
        }

        public MyGroup VerifyCurrentUserIsGroupLeader(string token, int groupId)
        {
            var groupParticipant = _groupRepository.GetAuthenticatedUserParticipationByGroupID(token, groupId);

            if (groupParticipant == null)
                throw new GroupNotFoundForParticipantException($"Could not find group {groupId} for user");

            if (groupParticipant.GroupRoleId != _groupRoleLeaderId)
                throw new NotGroupLeaderException($"User is not a leader of group {groupId}");

            return new MyGroup
            {
                Group = new GroupDTO
                {
                    GroupId = groupId
                },
                Me = new MpParticipant
                {
                    ParticipantId = groupParticipant.ParticipantId
                }
            };
        }

        public MyGroup GetMyGroupInfo(string token, int groupId)
        {
            var groups = _groupService.GetGroupByIdForAuthenticatedUser(token, groupId);
            var group = groups == null || !groups.Any() ? null : groups.FirstOrDefault();

            if (group == null)
            {
                throw new GroupNotFoundForParticipantException($"Could not find group {groupId} for user");
            }

            var groupParticipants = group.Participants;
            var me = _participantRepository.GetParticipantRecord(token);

            if (groupParticipants?.Find(p => p.ParticipantId == me.ParticipantId) == null ||
                groupParticipants.Find(p => p.ParticipantId == me.ParticipantId).GroupRoleId != _groupRoleLeaderId)
            {
                throw new NotGroupLeaderException($"User is not a leader of group {groupId}");
            }

            return new MyGroup
            {
                Group = group,
                Me = me
            };
        }


        private void RecordConnectInteraction(int groupId, int fromContactId, int toContactId, int connectionType, int connectionStatus)
        {
            //only record anywhere group type interactions
            var group = _groupService.GetGroupDetails(groupId);
            if (group.GroupTypeId != _anywhereGroupType && group.GroupTypeId != _smallGroupTypeId)
            {
                return;
            }

            var connection = new MpConnectCommunication
            {
                GroupId = groupId,
                FromContactId = fromContactId,
                ToContactId = toContactId,
                CommunicationTypeId = connectionType,
                CommunicationStatusId = connectionStatus
            };
            _finderRepository.RecordConnection(connection);
        }


        public void AcceptDenyGroupInvitation(string token, int groupId, string invitationGuid, bool accept)
        {
            try
            {
                //If they accept the invite get their participant record and them to the group as a member.
                if (accept)
                {
                    var participant = _participantRepository.GetParticipantRecord(token);

                    // make sure the person isn't already in a group
                    var groupParticipants = _groupRepository.GetGroupParticipants(groupId, true);

                    if (groupParticipants.Any(p => p.ParticipantId == participant.ParticipantId))
                    {
                        // mark as used here, too, because of exception flow
                        _invitationRepository.MarkInvitationAsUsed(invitationGuid);
                        throw new DuplicateGroupParticipantException("Cannot accept invite - already member of group");
                    }

                    _groupRepository.AddParticipantToGroup(participant.ParticipantId, groupId, _defaultGroupRoleId, false, false, DateTime.Now);
                }

                _invitationRepository.MarkInvitationAsUsed(invitationGuid);
            }
            catch (DuplicateGroupParticipantException e)
            {
                throw e;
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException($"Could not accept = {accept} from group {groupId}", e);
            }
        }

        public void SendAllGroupLeadersEmail(string token, int groupId, GroupMessageDTO message)
        {
            var requestor = _participantRepository.GetParticipantRecord(token);
            var requestorContact = _contactRepository.GetContactById(requestor.ContactId);
            var group = _groupService.GetGroupDetails(groupId);

            var fromContact = new MpContact
            {
                ContactId = _defaultGroupContactEmailId,
                EmailAddress = "groups@crossroads.net"
            };

            var replyToContact = new MpContact
            {
                ContactId = requestor.ContactId,
                EmailAddress = requestor.EmailAddress
            };

            var leaders = @group.Participants.
                Where(groupParticipant => groupParticipant.GroupRoleId == _groupRoleLeaderId).
                Select(groupParticipant => new MpContact
                       {
                           ContactId = groupParticipant.ContactId,
                           EmailAddress = groupParticipant.Email,
                           LastName = groupParticipant.LastName,
                           Nickname = groupParticipant.NickName
                       }).ToList();

            var fromString = "<p><i>This email was sent from: " + requestorContact.Nickname + " " + requestorContact.Last_Name + " (" + requestor.EmailAddress + ")</i></p>";

            var toString = "<p><i>This email was sent to: ";

            foreach (var leader in leaders)
            {
                toString += leader.Nickname + " " + leader.LastName + " (" + leader.EmailAddress + "), ";
                RecordConnectInteraction(groupId, requestor.ContactId, leader.ContactId, _connectCommunicationTypeEmailSmallGroupLeader, _connectCommunicationStatusNA);
            }

            char[] trailingChars = { ',', ' ' };
            toString = toString.TrimEnd(trailingChars);
            toString += "</i></p>";

            var email = new MpCommunication
            {
                EmailBody = fromString + "<p>" + message.Body + "</p>" + toString,
                EmailSubject = $"Crossroads Group {@group.GroupName}: {message.Subject}",
                AuthorUserId = _defaultAuthorUserId,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyToContact,
                ToContacts = leaders
            };

     
            _communicationRepository.SendMessage(email);
            var props = new EventProperties();
            props.Add("GroupName", group.GroupName);
            props.Add("GroupLeaderName", leaders[0].Nickname);
            _analyticsService.Track(requestor.ContactId.ToString(), "GroupLeaderContacted", props);

        }

        public void SendAllGroupParticipantsEmail(string token, int groupId, int groupTypeId, string subject, string body)
        {
            var leaderRecord = _participantRepository.GetParticipantRecord(token);
            var groups = _groupService.GetGroupByIdForAuthenticatedUser(token, groupId);

            if (groups == null || !groups.Any())
            {
                throw new GroupNotFoundForParticipantException($"Could not find group {groupId} for groupParticipant {leaderRecord.ParticipantId}");
            }

            if (!ValidateUserAsLeader(token, groupTypeId, groupId, leaderRecord.ParticipantId, groups.First()))
            {
                throw new NotGroupLeaderException($"Group participant ID {leaderRecord.ParticipantId} is not a leader of group {groupId}");
            }

            var fromContact = new MpContact
            {
                ContactId = 1519180,
                EmailAddress = "updates@crossroads.net"
            };

            var replyToContact = new MpContact
            {
                ContactId = leaderRecord.ContactId,
                EmailAddress = leaderRecord.EmailAddress
            };

            var toContacts = groups.First().Participants.Select(groupParticipant => new MpContact
                                                                            {
                                                                                ContactId = groupParticipant.ContactId,
                                                                                EmailAddress = groupParticipant.Email
                                                                            }).DistinctBy(c => c.EmailAddress).ToList();

            var email = new MpCommunication
            {
                EmailBody = body,
                EmailSubject = subject,
                AuthorUserId = 5,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyToContact,
                ToContacts = toContacts
            };

            _communicationRepository.SendMessage(email);
        }

        public bool ValidateUserAsLeader(string token, int groupTypeId, int groupId, int groupParticipantId, GroupDTO group)
        {
            var groupParticipants = group.Participants;
            var me = _participantRepository.GetParticipantRecord(token);

            if (groupParticipants == null || groupParticipants.Find(p => p.ParticipantId == me.ParticipantId) == null)
            {
                throw new NotGroupLeaderException($"Group participant {groupParticipantId} is not a leader of group {groupId}");

            }
            var isLeader = false;
            foreach (var part in groupParticipants)
            {
                if ( (me.ParticipantId == part.ParticipantId) &&
                        (_groupRoleLeaderId == part.GroupRoleId) )
                {
                    isLeader = true;
                    break;

                }
            }
            return isLeader;
        }

        public void EndGroup(int groupId, int reasonEndedId)
        {
            _awsCloudsearchService.DeleteGroupFromAws(groupId);

            //get all participants before we end the group so they are not endDated and still
            //available from this call.
            // ReSharper disable once RedundantArgumentDefaultValue
            var participants = _groupService.GetGroupParticipants(groupId, true);
            _groupService.EndDateGroup(groupId, reasonEndedId);
            var group = _groupService.GetGroupDetails(groupId);
            foreach (var participant in participants)
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Participant_Name", participant.NickName},
                    {"Group_Name", group.GroupName },
                    {"Group_Tool_Url", @"https://" + _baseUrl + "/groups/search"}
                };
                SendSingleGroupParticipantEmail(participant, _groupEndedParticipantEmailTemplate, mergeData);
            }
        }

        /// <summary>
        /// Sends a participant and email based off of a template ID and any merge data you need for that template. 
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="templateId"></param>
        /// <param name="mergeData"></param>
        public int SendSingleGroupParticipantEmail(GroupParticipantDTO participant, int templateId, Dictionary<string, object> mergeData)
        {
            var emailTemplate = _communicationRepository.GetTemplate(templateId);

            var domainId = Convert.ToInt32(_domainId);
            var from = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };

            var to = new List<MpContact>
            {
                new MpContact
                {
                    ContactId = participant.ContactId,
                    EmailAddress = participant.Email
                }
            };

            var message = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = domainId,
                FromContact = from,
                MergeData = mergeData,
                ReplyToContact = from,
                TemplateId = templateId,
                ToContacts = to,
                StartDate = DateTime.Now
            };
            // ReSharper disable once RedundantArgumentDefaultValue
            return _communicationRepository.SendMessage(message, false);
        }

        public List<GroupDTO> GetGroupToolGroups(string token)
        {
            var groups = _groupService.GetGroupsByTypeOrId(token,null, new int[] { _smallGroupTypeId, _onsiteGroupTypeId }, null);

            return _groupService.RemoveOnsiteParticipantsIfNotLeader(groups, token);
        }

        public void SubmitInquiry(string token, int groupId, bool doSendEmail)
        {
            var participant = _participantRepository.GetParticipantRecord(token);
            var contact = _contactRepository.GetContactById(participant.ContactId);

            // check to see if the inquiry is going against a group where a person is already a member or has an outstanding request to join
            var requestsForContact = _groupToolRepository.GetInquiries(groupId).Where(r => r.ContactId == participant.ContactId && r.Placed == null);
            var participants = _groupRepository.GetGroupParticipants(groupId, true).Where(r => r.ContactId == participant.ContactId);

            if (participants.Any())
            {
                throw new ExistingRequestException("User already a member");

            }
            if (requestsForContact.Any())
            {
                throw new ExistingRequestException("User already has request");
            }

            var mpInquiry = new MpInquiry
            {
                ContactId = participant.ContactId,
                GroupId = groupId,
                EmailAddress = participant.EmailAddress,
                PhoneNumber = contact.Home_Phone,
                FirstName = contact.Nickname,
                LastName = contact.Last_Name,
                RequestDate = DateTime.Now
            };

            _groupRepository.CreateGroupInquiry(mpInquiry);

            var group = _groupService.GetGroupDetails(groupId);
            var leaders = group.Participants.
                Where(groupParticipant => groupParticipant.GroupRoleId == _groupRoleLeaderId).ToList();

            // Call Analytics
            var props = new EventProperties
            {
                {"GroupLeaderName", leaders?.FirstOrDefault()?.DisplayName },
                {"GroupName", group.GroupName},
                {"GroupCity", group?.Address?.City},
                {"GroupState", group?.Address?.State},
                {"GroupZip", group?.Address?.PostalCode}
            };
            _analyticsService.Track(contact.Contact_ID.ToString(), "RequestedToJoinGroup", props);

            if (doSendEmail)
            {
                foreach (var leader in leaders)
                {
                    if (group.GroupTypeId == _anywhereGroupType)
                    {
                        var Requestor = "<i>" + contact.Nickname + " " + contact.Last_Name.Substring(0, 1) + "." + "</i> ";
                        var RequestorSub = contact.Nickname + " " + contact.Last_Name.Substring(0, 1) + ".";
                        var mergeData = new Dictionary<string, object> {{"Name", leader.NickName}, {"Requestor", Requestor}, {"RequestorSub", RequestorSub}};
                        SendSingleGroupParticipantEmail(leader, _anywhereGroupRequestToJoinEmailTemplate, mergeData);
                    }
                    else
                    {
                        var Requestor = "<i>" + contact.Nickname + " " + contact.Last_Name + "</i> ";
                        var mergeData = new Dictionary<string, object> {{"Name", leader.NickName}, {"Requestor", Requestor}};
                        SendSingleGroupParticipantEmail(leader, _groupRequestToJoinEmailTemplate, mergeData);
                    }
                }
            }
        }

        public void ArchivePendingGroupInquiriesOlderThan90Days()
        {
            try
            {
                _groupToolRepository.ArchivePendingGroupInquiriesOlderThan90Days();
            }
            catch (Exception e)
            {
                _logger.Error("Error archiving old pending group inquiries", e);
                throw;
            }
        }

        public void SendSmallGroupPendingInquiryReminderEmails()
        {
            try
            {
                var mpRequests = _groupToolRepository.GetInquiries();
                var groupsWithPendingRequests = mpRequests
                    .Where(request => request.RequestDate < DateTime.Now.AddDays(-7))
                    .Select(r => r.GroupId).Distinct().ToList();

                foreach (var groupId in groupsWithPendingRequests)
                {
                    var group = _groupRepository.getGroupDetails(groupId);
                    var requests = mpRequests.FindAll(r => r.GroupId == groupId);
                    SendPendingInquiryReminderEmail(group, requests);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Exception retrieving pending inquiries", e);
                throw;
            }
        }

        public List<AttributeCategoryDTO> GetGroupCategories()
        {
            List<AttributeCategoryDTO> cats = _attributeService.GetAttributeCategory(_attributeTypeGroupCategory);
            
            foreach (AttributeCategoryDTO cat in cats)
            {
                if (cat.RequiresActiveAttribute)
                {
                    cat.Attribute = _attributeService.GetOneAttributeByCategoryId(cat.CategoryId);
                }
            }

            //do not return any categories where it requires an active attribute but there are no active attributes
            cats.RemoveAll(cat => cat.RequiresActiveAttribute && cat.Attribute == null );

            return cats;
        }

        private void SendPendingInquiryReminderEmail(MpGroup group, IReadOnlyCollection<MpInquiry> inquiries)
        {
            var leaders = ((List<MpGroupParticipant>) group.Participants).FindAll(p => p.GroupRoleId == _groupRoleLeaderId).ToList();
            var allLeaders = string.Join(", ", leaders.Select(leader => $"{leader.NickName} {leader.LastName} ({leader.Email})").ToArray());
            var pendingRequests = "<table><tr><th>First Name</th><th>Last Name</th><th>Email Address</th><th>Request Date</th>"
                                  +
                                  string.Join("",
                                      inquiries.Select(
                                              inquiry =>
                                                  $"<tr><td>{inquiry.FirstName}</td><td>{inquiry.LastName}</td><td>{inquiry.EmailAddress}</td><td>{inquiry.RequestDate.ToShortDateString()}</td></tr>")
                                          .ToArray())
                                  + "</table>";
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var leader in leaders)
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Nick_Name", leader.NickName},
                    {"Last_Name", leader.LastName},
                    {"All_Leaders", allLeaders},
                    {"Pending_Requests", pendingRequests},
                    {"Group_Name", group.Name},
                    {"Group_Description", group.GroupDescription},
                    {"Group_ID", group.GroupId},
                    {"Pending_Requests_Count", inquiries.Count}
                };

                var email = new EmailCommunicationDTO
                {
                    groupId = group.GroupId,
                    TemplateId = group.GroupType == _anywhereGroupType ? _gatheringRequestPendingReminderEmailTemplateId : 2037,
                    ToContactId = leader.ContactId,
                    MergeData = mergeData
                };
                _emailCommunicationService.SendEmail(email);
            }
        }
    }
}