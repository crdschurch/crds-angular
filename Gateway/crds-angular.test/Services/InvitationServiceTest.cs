using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads;
using crds_angular.Services;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.test.Services
{
    public class InvitationServiceTest
    {
        private InvitationService _fixture;
        private Mock<IInvitationRepository> _invitationRepository;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IGroupRepository> _groupRepository;
        private Mock<IParticipantRepository> _participantRepository;
        private Mock<IContactRepository> _contactRespository;

        private const int GroupInvitationType = 11;
        private const int GroupInvitationEmailTemplate = 22;
        private const int TripInvitationType = 33;
        private const int TripInvitationEmailTemplate = 44;
        private const int DefaultInvitationEmailTemplate = 55;
        private const int DomainId = 66;
        private const int GroupRoleLeader = 77;
        private const int AnywhereGatheringEmailTemplateID = 2011;
        private const int AnywhereGatheringInvitationTypeID = 3;


        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _communicationService = new Mock<ICommunicationRepository>(MockBehavior.Strict);
            _invitationRepository = new Mock<IInvitationRepository>(MockBehavior.Strict);
            _groupRepository = new Mock<IGroupRepository>(MockBehavior.Strict);
            _participantRepository = new Mock<IParticipantRepository>(MockBehavior.Strict);
            _contactRespository = new Mock<IContactRepository>(MockBehavior.Strict);
            var config = new Mock<IConfigurationWrapper>();

            config.Setup(mocked => mocked.GetConfigIntValue("GroupInvitationType")).Returns(GroupInvitationType);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupInvitationEmailTemplate")).Returns(GroupInvitationEmailTemplate);
            config.Setup(mocked => mocked.GetConfigIntValue("TripInvitationType")).Returns(TripInvitationType);
            config.Setup(mocked => mocked.GetConfigIntValue("PrivateInviteTemplate")).Returns(TripInvitationEmailTemplate);
            config.Setup(mocked => mocked.GetConfigIntValue("DefaultInvitationEmailTemplate")).Returns(DefaultInvitationEmailTemplate);
            config.Setup(mocked => mocked.GetConfigIntValue("AnywhereGatheringInvitationEmailTemplate")).Returns(AnywhereGatheringEmailTemplateID);
            config.Setup(mocked => mocked.GetConfigIntValue("AnywhereGatheringInvitationType")).Returns(AnywhereGatheringInvitationTypeID);
            config.Setup(mocked => mocked.GetConfigIntValue("DomainId")).Returns(DomainId);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupRoleLeader")).Returns(GroupRoleLeader);
            config.Setup(mocked => mocked.GetConfigValue("BaseUrl")).Returns("www.bob.com");

            _fixture = new InvitationService(_invitationRepository.Object, _communicationService.Object, config.Object, _groupRepository.Object, _participantRepository.Object, _contactRespository.Object);
        }

        [Test]
        public void CanCreateInvitationsForGroups()
        {
            const string token = "dude";

            var invitation = new Invitation()
            {
                EmailAddress = "dudley@doright.com",
                GroupRoleId = GroupRoleLeader,
                InvitationType = GroupInvitationType,
                RecipientName = "Dudley Doright",
                RequestDate = new DateTime(2016, 7, 6),
                SourceId = 33
            };

            var mpInvitation = new MpInvitation
            {
                InvitationType = invitation.InvitationType,
                EmailAddress = invitation.EmailAddress,
                GroupRoleId = invitation.GroupRoleId,
                RecipientName = invitation.RecipientName,
                RequestDate = invitation.RequestDate,
                SourceId = invitation.SourceId,
                InvitationGuid = "guid123",
                InvitationId = 11
            };
            _invitationRepository.Setup(
                m =>
                    m.CreateInvitation(
                        It.Is<MpInvitation>(
                            i =>
                                i.InvitationType == invitation.InvitationType && i.EmailAddress.Equals(invitation.EmailAddress) && i.GroupRoleId == invitation.GroupRoleId &&
                                i.RecipientName.Equals(invitation.RecipientName) && i.RequestDate.Equals(invitation.RequestDate) && i.SourceId == invitation.SourceId))).Returns(mpInvitation);

            var testGroup = new MpGroup
            {
                GroupId = 33,
                Name = "TestGroup"
            };

            _groupRepository.Setup(mocked => mocked.getGroupDetails(invitation.SourceId)).Returns(testGroup);

            var testLeaderParticipant = new MpParticipant
            {
                DisplayName = "TestLeaderName",
                ContactId = 123,
                EmailAddress = "you@there.com"
            };

            var leaderContact = new MpMyContact
            {
                Last_Name = "TestLast",
                Nickname = "TestNick"
            };

            _contactRespository.Setup(mocked => mocked.GetContactById(testLeaderParticipant.ContactId)).Returns(leaderContact);

            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns(testLeaderParticipant);

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 12,
                FromEmailAddress = "me@here.com",
                ReplyToContactId = 34,
                ReplyToEmailAddress = "you@there.com",
                Subject = "subject"
            };
            _communicationService.Setup(mocked => mocked.GetTemplate(GroupInvitationEmailTemplate)).Returns(template);

            //_communicationService.Setup(
            //    mocked =>
            //        mocked.SendMessage(
            //            It.Is<MpCommunication>(
            //                c =>
            //                    c.AuthorUserId == 5 && c.DomainId == DomainId && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
            //                    c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
            //                    c.ReplyToContact.ContactId == template.ReplyToContactId && c.ReplyToContact.EmailAddress.Equals(template.ReplyToEmailAddress) &&
            //                    c.ToContacts.Count == 1 && c.ToContacts[0].EmailAddress.Equals(invitation.EmailAddress) &&
            //                    c.MergeData["Invitation_GUID"].ToString().Equals(mpInvitation.InvitationGuid) &&
            //                    c.MergeData["Recipient_Name"].ToString().Equals(mpInvitation.RecipientName) &&
            //                    c.MergeData["Leader_Name"].ToString().Equals(testLeaderParticipant.DisplayName) &&
            //                    c.MergeData["Group_Name"].ToString().Equals(testGroup.Name)),
            //            false)).Returns(77);

            _communicationService.Setup(
               mocked =>
                   mocked.SendMessage(
                       It.Is<MpCommunication>(
                           c =>
                               c.AuthorUserId == 5 && c.DomainId == DomainId && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                               c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                               c.ReplyToContact.ContactId == testLeaderParticipant.ContactId && c.ReplyToContact.EmailAddress.Equals(template.ReplyToEmailAddress) &&
                               c.ToContacts.Count == 1 && c.ToContacts[0].EmailAddress.Equals(invitation.EmailAddress) &&
                               c.MergeData["Invitation_GUID"].ToString().Equals(mpInvitation.InvitationGuid) &&
                               c.MergeData["Recipient_Name"].ToString().Equals(mpInvitation.RecipientName) &&
                               c.MergeData["Leader_Name"].ToString().Equals(leaderContact.Nickname + " " + leaderContact.Last_Name) &&
                               c.MergeData["Group_Name"].ToString().Equals(testGroup.Name)),
                       false)).Returns(77);

            var created = _fixture.CreateInvitation(invitation, 123);
            _invitationRepository.VerifyAll();
            _communicationService.VerifyAll();
            Assert.AreSame(invitation, created);
            Assert.AreEqual(mpInvitation.InvitationId, created.InvitationId);
            Assert.AreEqual(mpInvitation.InvitationGuid, created.InvitationGuid);
        }

        [Test]
        public void CanCreateInvitationsForAnywhereGroups()
        {
            const string token = "dude";
            
            string groupName = "Dougs Anywhere Gathering";
            string recipientName = "doug";
            string formattedRecipientName = "Doug";
            string leaderFName = "xavier";
            string leaderLName = "johnson";
            string formattedLeaderName = "Xavier J.";
            string acceptURL  = "www.bob.com/connect/accept-invite/33/guid123";
            string declineURL = "www.bob.com/connect/decline-invite/33/guid123";

            string city = "Lima";
            string state = "Ohio";
            string description = "descriptive description";
            int sourceId = 12345;
            int groupRoleId = 16;

            var invitation = new Invitation()
            {
                RecipientName = recipientName,
                EmailAddress = "a@b.com",
                SourceId = sourceId,
                GroupRoleId = groupRoleId,
                InvitationType = AnywhereGatheringInvitationTypeID
            };

            var mpInvitation = new MpInvitation
            {
                InvitationType = invitation.InvitationType,
                EmailAddress = invitation.EmailAddress,
                GroupRoleId = invitation.GroupRoleId,
                RecipientName = invitation.RecipientName,
                RequestDate = invitation.RequestDate,
                SourceId = invitation.SourceId,
                InvitationGuid = "guid123",
                InvitationId = 11
            };
            _invitationRepository.Setup(
                m =>
                    m.CreateInvitation(
                        It.Is<MpInvitation>(
                            i =>
                                i.InvitationType == invitation.InvitationType && i.EmailAddress.Equals(invitation.EmailAddress) && i.GroupRoleId == invitation.GroupRoleId &&
                                i.RecipientName.Equals(invitation.RecipientName) && i.SourceId == invitation.SourceId))).Returns(mpInvitation);

            var testGroup = new MpGroup
            {
                GroupId = 33,
                Name = groupName,
                Address = new MpAddress()
                {
                    City = city,
                    State = state
                }, 
                GroupDescription = description
            };

            _groupRepository.Setup(mocked => mocked.getGroupDetails(invitation.SourceId)).Returns(testGroup);


            MpParticipant leader = new MpParticipant()
            {
                ContactId = 123456789
            };

            MpMessageTemplate template = new MpMessageTemplate()
            {
                Body = "this is the template!",
                FromContactId = 1,
                FromEmailAddress = "us@you.com",
                ReplyToContactId = leader.ContactId,
                ReplyToEmailAddress = "you@you.com",
                Subject = "TheSubject!"
            };
            MpMyContact leaderContact = new MpMyContact()
            {
                Nickname = leaderFName,
                Last_Name = leaderLName
            };

            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns(leader);
            _contactRespository.Setup((c) => c.GetContactById(It.Is<int>((i) => i == leader.ContactId))).Returns(leaderContact);
            _communicationService.Setup((c) => c.GetTemplate(It.Is<int>((i) => i == AnywhereGatheringEmailTemplateID))).Returns(template);
            _communicationService.Setup((c) => c.SendMessage(It.Is<MpCommunication>(
                    (confirmation) => confirmation.EmailBody == template.Body
                    && confirmation.EmailSubject == template.Subject
                    && confirmation.AuthorUserId == 5
                    && confirmation.DomainId == DomainId
                    && confirmation.FromContact.ContactId == template.FromContactId
                    && confirmation.ReplyToContact.ContactId == template.ReplyToContactId
                    && confirmation.TemplateId == AnywhereGatheringEmailTemplateID
                    && confirmation.ToContacts[0].EmailAddress == invitation.EmailAddress
                    && (string)confirmation.MergeData["Invitation_GUID"] == mpInvitation.InvitationGuid
                    && (string)confirmation.MergeData["Recipient_Name"] == formattedRecipientName
                    && (string)confirmation.MergeData["Leader_Name"] == formattedLeaderName
                    && (string)confirmation.MergeData["YesURL"] == acceptURL
                    && (string)confirmation.MergeData["NoURL"] == declineURL
                ),
                false));


            var created = _fixture.CreateInvitation(invitation, 123);
            // _invitationRepository.VerifyAll();
            _communicationService.VerifyAll();
            Assert.AreSame(invitation, created);
            Assert.AreEqual(mpInvitation.InvitationId, created.InvitationId);
            Assert.AreEqual(mpInvitation.InvitationGuid, created.InvitationGuid);
        }

        [Test]
        public void TestValidateGroupInvitation()
        {
            const string token = "dude";
            var invitation = new Invitation()
            {
                EmailAddress = "dudley@doright.com",
                GroupRoleId = GroupRoleLeader,
                InvitationType = GroupInvitationType,
                RecipientName = "Dudley Doright",
                RequestDate = new DateTime(2016, 7, 6),
                SourceId = 33
            };

            var participant = new MpParticipant
            {
                ParticipantId = 654
            };
            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns(participant);

            var participants = new List<MpGroupParticipant>
            {
                new MpGroupParticipant
                {
                    GroupRoleId = GroupRoleLeader + 2,
                    ParticipantId = participant.ParticipantId + 1
                },
                new MpGroupParticipant
                {
                    GroupRoleId = GroupRoleLeader,
                    ParticipantId = participant.ParticipantId
                }
            };
            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(invitation.SourceId, true)).Returns(participants);

            _fixture.ValidateInvitation(invitation, 123);
            _participantRepository.VerifyAll();
            _groupRepository.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void TestValidateGroupInvitationParticipantLookupFails()
        {
            const string token = "dude";
            var invitation = new Invitation()
            {
                EmailAddress = "dudley@doright.com",
                GroupRoleId = GroupRoleLeader,
                InvitationType = GroupInvitationType,
                RecipientName = "Dudley Doright",
                RequestDate = new DateTime(2016, 7, 6),
                SourceId = 33
            };

            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns((MpParticipant)null);

            _fixture.ValidateInvitation(invitation, 123);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void TestValidateGroupInvitationNoParticipantsInGroup()
        {
            const string token = "dude";
            var invitation = new Invitation()
            {
                EmailAddress = "dudley@doright.com",
                GroupRoleId = GroupRoleLeader,
                InvitationType = GroupInvitationType,
                RecipientName = "Dudley Doright",
                RequestDate = new DateTime(2016, 7, 6),
                SourceId = 33
            };

            var participant = new MpParticipant
            {
                ParticipantId = 654
            };
            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns(participant);
            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(invitation.SourceId, true)).Returns((List<MpGroupParticipant>)null);

            _fixture.ValidateInvitation(invitation, 123);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void TestValidateGroupInvitationNotLeader()
        {
            const string token = "dude";
            var invitation = new Invitation()
            {
                EmailAddress = "dudley@doright.com",
                GroupRoleId = GroupRoleLeader,
                InvitationType = GroupInvitationType,
                RecipientName = "Dudley Doright",
                RequestDate = new DateTime(2016, 7, 6),
                SourceId = 33
            };

            var participant = new MpParticipant
            {
                ParticipantId = 654
            };
            _participantRepository.Setup(mocked => mocked.GetParticipant(It.IsAny<int>())).Returns(participant);

            var participants = new List<MpGroupParticipant>
            {
                new MpGroupParticipant
                {
                    GroupRoleId = GroupRoleLeader + 2,
                    ParticipantId = participant.ParticipantId + 1
                },
                new MpGroupParticipant
                {
                    GroupRoleId = GroupRoleLeader + 1,
                    ParticipantId = participant.ParticipantId
                }
            };

            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(invitation.SourceId, true)).Returns(participants);

            _fixture.ValidateInvitation(invitation, 123);
        }
    }
}
