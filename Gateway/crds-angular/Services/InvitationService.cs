using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class InvitationService : MinistryPlatformBaseService, IInvitationService
    {

        private readonly IInvitationRepository _invitationRepository;
        private readonly ICommunicationRepository _communicationService;
        private readonly IGroupRepository _groupRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IContactRepository _contactRepository;

        private readonly int _groupInvitationType;
        private readonly int _groupInvitationEmailTemplate;
        private readonly int _groupInvitationEmailTemplateCustom;
        private readonly int _tripInvitationType;
        private readonly int _tripInvitationEmailTemplate;        

        private readonly int _anywhereGatheringInvitationTypeId;
        private readonly int _anywhereGatheringInvitationEmailTemplate;

        private readonly int _defaultInvitationEmailTemplate;
        private readonly int _domainId;

        private readonly int _groupRoleLeader;

        private readonly ILog _logger = LogManager.GetLogger(typeof(GroupToolService));

        private readonly string _baseUrl;

        public InvitationService(
                           IInvitationRepository invitationRepository,
                           ICommunicationRepository communicationService, 
                           IConfigurationWrapper configuration,
                           IGroupRepository groupRepository,
                           IParticipantRepository participantRepository,
                           IContactRepository contactRepository)
        {

            _invitationRepository = invitationRepository;
            _communicationService = communicationService;
            _groupRepository = groupRepository;
            _participantRepository = participantRepository;
            _contactRepository = contactRepository;

            _groupInvitationType = configuration.GetConfigIntValue("GroupInvitationType");
            _groupInvitationEmailTemplate = configuration.GetConfigIntValue("GroupInvitationEmailTemplate");
            _groupInvitationEmailTemplateCustom = configuration.GetConfigIntValue("GroupInvitationEmailTemplateCustom");
            _tripInvitationType = configuration.GetConfigIntValue("TripInvitationType");
            _tripInvitationEmailTemplate = configuration.GetConfigIntValue("PrivateInviteTemplate");
            _anywhereGatheringInvitationEmailTemplate = configuration.GetConfigIntValue("AnywhereGatheringInvitationEmailTemplate");
            _anywhereGatheringInvitationTypeId = configuration.GetConfigIntValue("AnywhereGatheringInvitationType");
            

            _defaultInvitationEmailTemplate = configuration.GetConfigIntValue("DefaultInvitationEmailTemplate");

            _domainId = configuration.GetConfigIntValue("DomainId");

            _groupRoleLeader = configuration.GetConfigIntValue("GroupRoleLeader");

            //URL
            _baseUrl = configuration.GetConfigValue("BaseUrl");
        }

        public Invitation CreateInvitation(Invitation dto, int contactId)
        {
            try
            {
                var mpInvitation = Mapper.Map<MpInvitation>(dto);
                var invitation = _invitationRepository.CreateInvitation(mpInvitation);
                if (dto.InvitationType == _anywhereGatheringInvitationTypeId || dto.InvitationType == _groupInvitationType)
                {
                    var group = _groupRepository.getGroupDetails(dto.SourceId);
                    var leaderParticipantRecord = _participantRepository.GetParticipant(contactId);

                    try
                    {
                        SendEmail(invitation, leaderParticipantRecord, group);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error sending email to {invitation.EmailAddress} for invitation {invitation.InvitationGuid}", e);
                    }
                }

                dto.InvitationGuid = invitation.InvitationGuid;
                dto.InvitationId = invitation.InvitationId;
                return dto;
            }
            catch (Exception e)
            {
                var message = $"Exception creating invitation for {dto.RecipientName}, SourceID = {dto.SourceId}.";
                _logger.Error(message, e);
                throw new ApplicationException(message, e);
            }
        }

        public void ValidateInvitation(Invitation dto, int contactId)
        {
            if (dto.InvitationType == _groupInvitationType)
            {
                ValidateGroupInvitation(dto, contactId);
            } else if (dto.InvitationType == _tripInvitationType)
            {
                ValidateTripInvitation(dto, contactId);
            } else if (dto.InvitationType == _anywhereGatheringInvitationTypeId)
            {
                ValidateGroupInvitation(dto, contactId);
            }

        }

        private void ValidateGroupInvitation(Invitation dto, int contactId)
        {
            var me = _participantRepository.GetParticipant(contactId);
            if (me == null)
            {
                throw new ValidationException("You must be a group leader to invite others to this group (participant not found)");
            }

            var participants = _groupRepository.GetGroupParticipants(dto.SourceId, true);
            if (participants == null || !participants.Any())
            {
                throw new ValidationException("You must be a group leader to invite others to this group (no active participants)");
            }

            var found = participants.Find(p => p.ParticipantId == me.ParticipantId && p.GroupRoleId == _groupRoleLeader);
            if (found == null)
            {
                throw new ValidationException("You must be a group leader to invite others to this group (participant not a leader)");
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ValidateTripInvitation(Invitation dto, int contactId)
        {
            // TODO Implement validation, make sure the token represents someone who is allowed to send this trip invitation
        }

        private void SendEmail(MpInvitation invitation, MpParticipant leader, MpGroup group)
        {
            var leaderContact = _contactRepository.GetContactById(leader.ContactId);

            // basic merge data here
            var mergeData = new Dictionary<string, object>
            {
                { "Invitation_GUID", invitation.InvitationGuid },
                { "Recipient_Name", invitation.RecipientName },
            };

            int emailTemplateId;
            if (invitation.InvitationType == _groupInvitationType)
            {
                if (invitation.CustomMessage != null) {
                    emailTemplateId = _groupInvitationEmailTemplateCustom;
                    mergeData.Add("Leader_Message", invitation.CustomMessage);
                } else {
                    emailTemplateId = _groupInvitationEmailTemplate;
                }
                mergeData.Add("Leader_Name", leaderContact.Nickname + " " + leaderContact.Last_Name);
                mergeData.Add("Group_Name", group.Name);
            } else if (invitation.InvitationType == _tripInvitationType) {
                emailTemplateId = _tripInvitationEmailTemplate;
            } else if (invitation.InvitationType == _anywhereGatheringInvitationTypeId) {
                mergeData["YesURL"] = $"{_baseUrl}/connect/accept-invite/{group.GroupId}/{invitation.InvitationGuid}";
                mergeData["NoURL"]  = $"{_baseUrl}/connect/decline-invite/{group.GroupId}/{invitation.InvitationGuid}";

                mergeData["Recipient_Name"] = invitation.RecipientName.Substring(0, 1).ToUpper() + invitation.RecipientName.Substring(1).ToLower();
                mergeData.Add("Leader_Name", leaderContact.Nickname.Substring(0,1).ToUpper() + leaderContact.Nickname.Substring(1).ToLower() + " " + leaderContact.Last_Name.Substring(0,1).ToUpper() + ".");
                mergeData.Add("City", group.Address.City);
                mergeData.Add("State", group.Address.State);
                mergeData.Add("Description", group.GroupDescription);
                mergeData.Add("Group_ID", group.GroupId);
                emailTemplateId = _anywhereGatheringInvitationEmailTemplate;
            } else {
                emailTemplateId = _defaultInvitationEmailTemplate;
            }
            var emailTemplate = _communicationService.GetTemplate(emailTemplateId);
            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };
            var replyTo = new MpContact
            {
                ContactId = leader.ContactId,
                EmailAddress = leader.EmailAddress
            };

            var to = new List<MpContact>
                {
                    new MpContact
                    {
                        // Just need a contact ID here, doesn't have to be for the recipient
                        ContactId = emailTemplate.FromContactId,
                        EmailAddress = invitation.EmailAddress
                    }
                };



            var confirmation = new MpCommunication
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
            _communicationService.SendMessage(confirmation);
        }

    }
}