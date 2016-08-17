using System;
using System.Collections.Generic;
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

namespace crds_angular.Services
{
    public class GroupToolService : MinistryPlatformBaseService, IGroupToolService
    {

        private readonly IGroupToolRepository _groupToolRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupService _groupService;
        private readonly IParticipantRepository _participantRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IContentBlockService _contentBlockService;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IAddressProximityService _addressProximityService;
        private readonly IContactRepository _contactRepository;
        private readonly IConfigurationWrapper _configurationWrapper;

        private readonly int _defaultGroupContactEmailId;
        private readonly int _defaultAuthorUserId;
        private readonly int _defaultGroupRoleId;
        private readonly int _defaultGroupTypeId;
        private readonly int _groupRoleLeaderId;
        private readonly int _removeParticipantFromGroupEmailTemplateId;
        private readonly int _domainId;
        private readonly int _groupEndedParticipantEmailTemplate;
        private readonly int _defaultEmailContactId;
        private readonly string _baseUrl;

        private const string GroupToolRemoveParticipantEmailTemplateTextTitle = "groupToolRemoveParticipantEmailTemplateText";
        private const string GroupToolRemoveParticipantSubjectTemplateText = "groupToolRemoveParticipantSubjectTemplateText";
        private const string GroupToolApproveInquiryEmailTemplateText = "groupToolApproveInquiryEmailTemplateText";
        private const string GroupToolApproveInquirySubjectTemplateText = "groupToolApproveInquirySubjectTemplateText";
        private const string GroupToolDenyInquiryEmailTemplateText = "groupToolDenyInquiryEmailTemplateText";
        private const string GroupToolDenyInquirySubjectTemplateText = "groupToolDenyInquirySubjectTemplateText";

        private readonly ILog _logger = LogManager.GetLogger(typeof(GroupToolService));

        public GroupToolService(
                           IGroupToolRepository groupToolRepository,
                           IGroupRepository groupRepository,
                           IGroupService groupService,
                           IParticipantRepository participantRepository,
                           ICommunicationRepository communicationRepository,
                           IContentBlockService contentBlockService,
                           IConfigurationWrapper configurationWrapper, 
                           IInvitationRepository invitationRepository,
                           IAddressProximityService addressProximityService,
                           IContactRepository contactRepository)
        {

            _groupToolRepository = groupToolRepository;
            _groupRepository = groupRepository;
            _groupService = groupService;
            _participantRepository = participantRepository;
            _communicationRepository = communicationRepository;
            _contentBlockService = contentBlockService;
            _invitationRepository = invitationRepository;
            _addressProximityService = addressProximityService;
            _contactRepository = contactRepository;

            _defaultGroupContactEmailId = configurationWrapper.GetConfigIntValue("DefaultGroupContactEmailId");
            _defaultAuthorUserId = configurationWrapper.GetConfigIntValue("DefaultAuthorUser");
            _groupRoleLeaderId = configurationWrapper.GetConfigIntValue("GroupRoleLeader");
            _defaultGroupRoleId = configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
            _defaultGroupTypeId = configurationWrapper.GetConfigIntValue("GroupTypeSmallId");

               _removeParticipantFromGroupEmailTemplateId = configurationWrapper.GetConfigIntValue("RemoveParticipantFromGroupEmailTemplateId");

            _domainId = configurationWrapper.GetConfigIntValue("DomainId");
            _groupEndedParticipantEmailTemplate = Convert.ToInt32(configurationWrapper.GetConfigIntValue("GroupEndedParticipantEmailTemplate"));
            _defaultEmailContactId = Convert.ToInt32(configurationWrapper.GetConfigIntValue("DefaultContactEmailId"));
            _baseUrl = configurationWrapper.GetConfigValue("BaseURL");
        }

        public List<Invitation> GetInvitations(int sourceId, int invitationTypeId, string token)
        {
            var invitations = new List<Invitation>();
            try
            {
                VerifyCurrentUserIsGroupLeader(token, _defaultGroupTypeId, sourceId);

                var mpInvitations = _groupToolRepository.GetInvitations(sourceId, invitationTypeId);
                mpInvitations.ForEach(x => invitations.Add(Mapper.Map<Invitation>(x)));
            }
            catch (Exception e)
            {
                var message = string.Format("Exception retrieving invitations for SourceID = {0}, InvitationTypeID = {1}.", sourceId, invitationTypeId);
                _logger.Error(message, e);
                throw;
            }
            return invitations;
        }

        public List<Inquiry> GetInquiries(int groupId, string token)
        {
            var requests = new List<Inquiry>();
            try
            {
                VerifyCurrentUserIsGroupLeader(token, _defaultGroupTypeId, groupId);

                var mpRequests = _groupToolRepository.GetInquiries(groupId);
                mpRequests.ForEach(x => requests.Add(Mapper.Map<Inquiry>(x)));
            }
            catch (Exception e)
            {
                var message = string.Format("Exception retrieving inquiries for group id = {0}.", groupId);
                _logger.Error(message, e);
                throw;
            }
            return requests;
        }

        public void RemoveParticipantFromMyGroup(string token, int groupTypeId, int groupId, int groupParticipantId, string message = null)
        {
            try
            {
                var myGroup = VerifyCurrentUserIsGroupLeader(token, groupTypeId, groupId);

                _groupService.endDateGroupParticipant(groupId, groupParticipantId);

                try
                {
                    SendGroupParticipantEmail(groupId,
                                              groupParticipantId,
                                              true,
                                              myGroup.Group,
                                              _removeParticipantFromGroupEmailTemplateId,
                                              GroupToolRemoveParticipantSubjectTemplateText,
                                              GroupToolRemoveParticipantEmailTemplateTextTitle,
                                              message,
                                              myGroup.Me);
                }
                catch (Exception e)
                {
                    _logger.Warn(string.Format("Could not send email to group participant {0} notifying of removal from group {1}", groupParticipantId, groupId), e);
                }
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException(string.Format("Could not remove group participant {0} from group {1}", groupParticipantId, groupId), e);
            }

        }

        public void SendGroupParticipantEmail(int groupId, int participantId, bool groupParticpantId, GroupDTO group, int emailTemplateId, string subjectTemplateContentBlockTitle = null, string emailTemplateContentBlockTitle = null, string message = null, Participant fromParticipant = null)
        {
            var participant = groupParticpantId ? group.Participants.Find(p => p.GroupParticipantId == participantId) : group.Participants.Find(p => p.ParticipantId == participantId);

            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };
            var replyTo = new MpContact
            {
                ContactId = fromParticipant == null ? emailTemplate.ReplyToContactId : fromParticipant.ContactId,
                EmailAddress = fromParticipant == null ? emailTemplate.ReplyToEmailAddress : fromParticipant.EmailAddress
            };

            var to = new List<MpContact>
                {
                    new MpContact
                    {
                        ContactId = participant.ContactId,
                        EmailAddress = participant.Email
                    }
                };

            var subjectTemplateText = string.IsNullOrWhiteSpace(subjectTemplateContentBlockTitle) ? string.Empty : _contentBlockService[subjectTemplateContentBlockTitle].Content;
            var emailTemplateText = string.IsNullOrWhiteSpace(emailTemplateContentBlockTitle) ? string.Empty : _contentBlockService[emailTemplateContentBlockTitle].Content;
            var mergeData = getDictionary(participant);
            mergeData["Email_Template_Text"] = emailTemplateText;
            mergeData["Email_Custom_Message"] = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            mergeData["Subject_Template_Text"] = subjectTemplateText;
            mergeData["Group_Name"] = group.GroupName;
            mergeData["Group_Description"] = group.GroupDescription;
            if (fromParticipant != null)
            {
                mergeData["From_Display_Name"] = fromParticipant.DisplayName;
                mergeData["From_Preferred_Name"] = fromParticipant.PreferredName;
            }
            var email = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyTo,
                TemplateId = emailTemplateId,
                ToContacts = to,
                MergeData = mergeData
            };
            _communicationRepository.SendMessage(email);
        }
        
        public MyGroup VerifyCurrentUserIsGroupLeader(string token, int groupTypeId, int groupId)
        {
            var groups = _groupService.GetGroupsByTypeForAuthenticatedUser(token, groupTypeId, groupId);
            var group = groups == null || !groups.Any() ? null : groups.FirstOrDefault();

            if (group == null)
            {
                throw new GroupNotFoundForParticipantException(string.Format("Could not find group {0} for user", groupId));
            }

            var groupParticipants = group.Participants;
            var me = _participantRepository.GetParticipantRecord(token);

            if (groupParticipants?.Find(p => p.ParticipantId == me.ParticipantId) == null ||
                groupParticipants.Find(p => p.ParticipantId == me.ParticipantId).GroupRoleId != _groupRoleLeaderId)
            {
                throw new NotGroupLeaderException(string.Format("User is not a leader of group {0}", groupId));
            }

            return new MyGroup
            {
                Group = group,
                Me = me
            };
        }

        public void ApproveDenyInquiryFromMyGroup(string token, int groupTypeId, int groupId, bool approve, Inquiry inquiry, string message = null)
        {
            try
            {
                var myGroup = VerifyCurrentUserIsGroupLeader(token, groupTypeId, groupId);

                if (approve)
                {
                    ApproveInquiry(groupId, myGroup.Group, inquiry, myGroup.Me, message);
                }
                else
                {
                    DenyInquiry(groupId, myGroup.Group, inquiry, myGroup.Me, message);
                }
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException(string.Format("Could not add Inquirier {0} from group {1}", inquiry.InquiryId, groupId), e);
            }
        }

        private void ApproveInquiry(int groupId, GroupDTO group, Inquiry inquiry, Participant me, string message)
        {
            _groupService.addContactToGroup(groupId, inquiry.ContactId);
            _groupRepository.UpdateGroupInquiry(groupId, inquiry.InquiryId, true);

            try
            {
                //TODO:: Update template id
                SendApproveDenyInquiryEmail(
                    true,
                    groupId,
                    group,
                    inquiry,
                    me,
                    _removeParticipantFromGroupEmailTemplateId,
                    GroupToolApproveInquiryEmailTemplateText,
                    message);
            }
            catch (Exception e)
            {
                _logger.Warn(string.Format("Could not send email to Inquirier {0} notifying of being approved to group {1}", inquiry.InquiryId, groupId), e);
            }
        }

        private void DenyInquiry(int groupId, GroupDTO group, Inquiry inquiry, Participant me, string message)
        {
            _groupRepository.UpdateGroupInquiry(groupId, inquiry.InquiryId, false);

            try
            {
                //TODO:: Update template id
                SendApproveDenyInquiryEmail(
                    false,
                    groupId,
                    group,
                    inquiry,
                    me,
                    _removeParticipantFromGroupEmailTemplateId,
                    GroupToolDenyInquiryEmailTemplateText,
                    message);
            }
            catch (Exception e)
            {
                _logger.Warn(string.Format("Could not send email to Inquirier {0} notifying of being approved to group {1}", inquiry.InquiryId, groupId), e);
            }
        }

        private void SendApproveDenyInquiryEmail(bool approve, int groupId, GroupDTO group, Inquiry inquiry, Participant me, int emailTemplateId, string emailTemplateContentBlockTitle, string message)
        {
            try
            {
                var subject = approve ? GroupToolApproveInquirySubjectTemplateText : GroupToolDenyInquirySubjectTemplateText;
                var participant = _participantRepository.GetParticipant(inquiry.ContactId);

                SendGroupParticipantEmail(groupId,
                                          participant.ParticipantId,
                                          false,
                                          group,
                                          emailTemplateId,
                                          subject,
                                          emailTemplateContentBlockTitle,
                                          message,
                                          me);
            }
            catch (Exception e)
            {
                _logger.Warn(string.Format("Could not send email to Inquirer {0} notifying for group {1}", inquiry.InquiryId, groupId), e);
            }
        }

        public void AcceptDenyGroupInvitation(string token, int groupId, string invitationGuid, bool accept)
        {
            try
            {
                //If they accept the invite get their participant record and them to the group as a member.
                if (accept)
                {
                    var participant = _participantRepository.GetParticipantRecord(token);
                    _groupRepository.addParticipantToGroup(participant.ParticipantId, groupId, _defaultGroupRoleId, false, DateTime.Now);
                }

                _invitationRepository.MarkInvitationAsUsed(invitationGuid);
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException(string.Format("Could not accept = {0} from group {1}", accept, groupId), e);
            }
        }

        public void SendAllGroupLeadersEmail(string token, int groupId, GroupMessageDTO message)
        {
            var requestor = _participantRepository.GetParticipantRecord(token);
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
                    EmailAddress = groupParticipant.Email
                }).ToList();

            var email = new MpCommunication
            {
                EmailBody = message.Body,
                EmailSubject = string.Format("Crossroads Group {0}: {1}", group.GroupName, message.Subject),
                AuthorUserId = _defaultAuthorUserId,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyToContact,
                ToContacts = leaders
            };

            _communicationRepository.SendMessage(email);
        }

        public void SendAllGroupParticipantsEmail(string token, int groupId, int groupTypeId, string subject, string body)
        {
            var leaderRecord = _participantRepository.GetParticipantRecord(token);
            var groups = _groupService.GetGroupsByTypeForAuthenticatedUser(token, groupTypeId, groupId);

            ValidateUserAsLeader(token, groupId, groupTypeId, leaderRecord.ParticipantId, groups);

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

            List<MpContact> toContacts = groups.First().Participants.Select(groupParticipant => new MpContact
            {
                ContactId = groupParticipant.ContactId, EmailAddress = groupParticipant.Email
            }).ToList();

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

        public void ValidateUserAsLeader(string token, int groupTypeId, int groupId, int groupParticipantId, List<GroupDTO> groups)
        {
            if (groups == null || !groups.Any())
            {
                throw new GroupNotFoundForParticipantException(string.Format("Could not find group {0} for groupParticipant {1}", groupId, groupParticipantId));
            }

            var groupParticipants = groups.FirstOrDefault().Participants;
            var me = _participantRepository.GetParticipantRecord(token);

            if (groupParticipants == null || groupParticipants.Find(p => p.ParticipantId == me.ParticipantId) == null ||
                groupParticipants.Find(p => p.ParticipantId == me.ParticipantId).GroupRoleId != _groupRoleLeaderId)
            {
                throw new NotGroupLeaderException(string.Format("Group participant {0} is not a leader of group {1}", groupParticipantId, groupId));
            }
        }

        public void EndGroup(int groupId, int reasonEndedId)
        {
            //get all participants before we end the group so they are not endDated and still
            //available from this call.
            var participants = _groupService.GetGroupParticipants(groupId, true);
            _groupService.EndDateGroup(groupId, reasonEndedId);
            foreach (var participant in participants)
            {
                SendGroupEndedParticipantEmail(participant);
            }
        }

        public void SendGroupEndedParticipantEmail(GroupParticipantDTO participant)
        {
            var emailTemplate = _communicationRepository.GetTemplate(_groupEndedParticipantEmailTemplate);
            var mergeData = new Dictionary<string, object>
            {
                {"Participant_Name", participant.NickName},
                {"Group_Tool_Url",  @"https://" + _baseUrl + "/groups/search"}
            };

            var domainId = Convert.ToInt32(_domainId);
            var from = new MpContact()
            {
                ContactId = _defaultEmailContactId,
                EmailAddress = _communicationRepository.GetEmailFromContactId(_defaultEmailContactId)
            };

            var to = new List<MpContact>();
            to.Add(new MpContact()
            {
                ContactId = participant.ContactId,
                EmailAddress = participant.Email
            });

            var groupEnded = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = domainId,
                FromContact = from,
                MergeData = mergeData,
                ReplyToContact = from,
                TemplateId = _groupEndedParticipantEmailTemplate,
                ToContacts = to
            };
            _communicationRepository.SendMessage(groupEnded, false);
        }

        public List<GroupDTO> SearchGroups(int groupTypeId, string keywords = null, string location = null)
        {
            // Split single search term into multiple words, broken on whitespace
            // TODO Should remove stopwords from search - possibly use a configurable list of words (http://www.link-assistant.com/seo-stop-words.html)
            var search = string.IsNullOrWhiteSpace(keywords) ? null : keywords.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);

            var results = _groupToolRepository.SearchGroups(groupTypeId, search);
            if (results == null || !results.Any())
            {
                return null;
            }

            var groups = results.Select(Mapper.Map<GroupDTO>).ToList();

            if (string.IsNullOrWhiteSpace(location))
            {
                return groups;
            }

            try
            {
                var proximities = _addressProximityService.GetProximity(location, groups.Select(g => g.Address).ToList());
                for (var i = 0; i < groups.Count; i++)
                {
                    groups[i].Proximity = proximities[i];
                }
            }
            catch (InvalidAddressException e)
            {
                _logger.Info($"Can't validate origin address {location}", e);
            }
            catch (Exception e)
            {
                _logger.Error($"Can't search by proximity for address {location}", e);
            }

            return groups;
        }
        
        public void SubmitInquiry(string token, int groupId)
        {
            var active = true;
            var participant = _participantRepository.GetParticipantRecord(token);
            var contact = _contactRepository.GetContactById(participant.ContactId);

            // check to see if the inquiry is going against a group where a person is already a member or has an outstanding request to join
            var requestsForContact = _groupToolRepository.GetInquiries(groupId).Where(r => r.ContactId == participant.ContactId && (r.Placed == null || r.Placed == true));
            var participants = _groupRepository.GetGroupParticipants(groupId, active).Where(r => r.ContactId == participant.ContactId);

            if (requestsForContact.Any() || participants.Any())
            {
                throw new ExistingRequestException("User is already member or has request");
            }

            MpInquiry mpInquiry = new MpInquiry
            {
                ContactId = participant.ContactId,
                GroupId = groupId,
                EmailAddress = participant.EmailAddress,
                PhoneNumber = contact.Home_Phone,
                FirstName = contact.Nickname,
                LastName = contact.Last_Name,
                RequestDate = System.DateTime.Now
            };

            _groupRepository.CreateGroupInquiry(mpInquiry);
        }
    }
}
