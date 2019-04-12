using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using crds_angular.Models.Crossroads.Waivers;
using crds_angular.Services;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class WaiverServiceTest
    {

        private readonly Mock<IWaiverRepository> _waiverRepository;
        private readonly Mock<IAuthenticationRepository> _authenticationRepository;
        private readonly Mock<IInvitationRepository> _invitationRepository;
        private readonly Mock<IContactRepository> _contactRepository;
        private readonly Mock<ICommunicationRepository> _communicationRepository;
        private readonly Mock<IEventParticipantRepository> _eventParticipantRepository;
        private readonly Mock<IConfigurationWrapper> _configurationWrapper;
        private readonly WaiverService _fixture;

        public WaiverServiceTest()
        {
            _waiverRepository = new Mock<IWaiverRepository>();
            _authenticationRepository = new Mock<IAuthenticationRepository>();
            _invitationRepository = new Mock<IInvitationRepository>();
            _contactRepository = new Mock<IContactRepository>();
            _communicationRepository = new Mock<ICommunicationRepository>();
            _eventParticipantRepository = new Mock<IEventParticipantRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _fixture = new WaiverService(_waiverRepository.Object, _authenticationRepository.Object, _invitationRepository.Object, _contactRepository.Object, _communicationRepository.Object, _eventParticipantRepository.Object, _configurationWrapper.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _waiverRepository.VerifyAll();
            _authenticationRepository.VerifyAll();
            _invitationRepository.VerifyAll();
            _contactRepository.VerifyAll();
            _communicationRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
        }

        [Test]
        public async Task ShouldGetWaiverAndConvertToDTO()
        {
            const int waiverId = 23;
            var startDate = DateTime.Now;
            var mpWaiver = MpWaiver(startDate);

            _waiverRepository.Setup(m => m.GetWaiver(waiverId)).Returns(Observable.Return(mpWaiver));
            var result = await _fixture.GetWaiver(waiverId);

            Assert.IsInstanceOf(typeof(WaiverDTO), result);
            Assert.AreEqual(startDate, result.WaiverStartDate);
            Assert.AreEqual(waiverId, result.WaiverId);
        }

        [Test]
        public async Task ShouldGetEventWaivers()
        {
            const int eventId = 2345;
            const int myContactId = 56778;

            var partWaiver1 = MpEventParticpantWaiver(true, myContactId, 34);               
            var partWaiver2 = MpEventParticpantWaiver(true, myContactId, 35);
            var eventWaiver1 = MpEventWaiver(34);
            var eventWaiver2 = MpEventWaiver(35);
            var eventWaiver3 = MpEventWaiver(33);
            
            _waiverRepository.Setup(m => m.GetEventParticipantWaiversByContact(eventId, myContactId)).Returns(Observable.Create<MpEventParticipantWaiver>(observer =>
            {
                observer.OnNext(partWaiver2);
                observer.OnNext(partWaiver1);
                observer.OnCompleted();
                return Disposable.Empty;
            }));

            _waiverRepository.Setup(m => m.GetEventWaivers(eventId)).Returns(Observable.Create<MpEventWaivers>(observer =>
            {
                observer.OnNext(eventWaiver1);
                observer.OnNext(eventWaiver2);
                observer.OnNext(eventWaiver3);
                observer.OnCompleted();
                return Disposable.Empty;
            }));

            var result = await _fixture.EventWaivers(eventId, myContactId).ToList();

            var waiverResult1 = result.Single(ew => ew.WaiverId == 34);
            Assert.IsNotNull(waiverResult1);
            Assert.IsTrue(waiverResult1.Accepted);

            var waiverResult2 = result.Single(ew => ew.WaiverId == 35);
            Assert.IsNotNull(waiverResult2);
            Assert.IsTrue(waiverResult2.Accepted);

            var waiverResult3 = result.Single(ew => ew.WaiverId == 33);
            Assert.IsNotNull(waiverResult3);
            Assert.IsFalse(waiverResult3.Accepted);
        }

        [Test]
        public async Task ShouldSendAcceptanceEmail()
        {
            var contactInvitation = ContactInvitation();
            var eventParticipant = MpEventParticipant();
            var template = Template();
            var mpCommunication = MpCommunication(template, contactInvitation.Contact);

            _eventParticipantRepository.Setup(m => m.GetEventParticpantByEventParticipantWaiver(contactInvitation.Invitation.SourceId)).Returns(Observable.Return(eventParticipant));

            _configurationWrapper.Setup(m => m.GetConfigIntValue("WaiverEmailTemplateId")).Returns(23);
            _configurationWrapper.Setup(m => m.GetConfigValue("BaseUrl")).Returns("localhost:3000");
            _communicationRepository.Setup(m => m.GetTemplate(23)).Returns(template);
            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(23,
                                                                             template.FromContactId,
                                                                             template.FromEmailAddress,
                                                                             template.ReplyToContactId,
                                                                             template.ReplyToEmailAddress,
                                                                             contactInvitation.Contact.ContactId,
                                                                             contactInvitation.Contact.EmailAddress,
                                                                             It.IsAny<Dictionary<string, object>>())).Returns(mpCommunication);
            _communicationRepository.Setup(m => m.SendMessage(mpCommunication, false)).Returns(2990);

            var result = await _fixture.SendAcceptanceEmail(contactInvitation);
            Assert.AreEqual(2990, result);
        }

        [Test]
        public async Task ShouldCreateWaiverInvitation()
        {
            const int myContactId = 56778;
            const int eventParticipantId = 9908;
            const int eventParticipantWaiverId = 89;
            const int waiverId = 888;
            const int waiverInvitiationTypeId = 4343;
            _configurationWrapper.Setup(m => m.GetConfigIntValue("WaiverInvitationType")).Returns(waiverInvitiationTypeId);
            _contactRepository.Setup(m => m.GetSimpleContact(myContactId)).Returns(Observable.Return(new MpSimpleContact
            {
                ContactId = myContactId, 
                DateOfBirth = DateTime.Now.AddDays(-1000),
                EmailAddress = "andy@pi.com",
                FirstName = "Andy",
                LastName = "Pi",
                Nickname = "Andi"
            }));

            _waiverRepository.Setup(m => m.CreateEventParticipantWaiver(waiverId, eventParticipantId, myContactId)).Returns(Observable.Return(new MpEventParticipantWaiver
            {
                EventParticipantId = eventParticipantId,
                EventParticipantWaiverId = eventParticipantWaiverId,
                SignerId = myContactId,
                Accepted = false,
                WaiverId = waiverId
            }));

            _invitationRepository.Setup(m => m.CreateInvitationAsync(It.IsAny<MpInvitation>())).Returns((MpInvitation mp) =>
            {
                Assert.AreEqual(mp.SourceId, eventParticipantWaiverId);
                Assert.AreEqual(mp.EmailAddress, "andy@pi.com");
                mp.InvitationGuid = new Guid().ToString();
                mp.InvitationId = 999999;
                return Observable.Return(mp);
            });
            
            

            var result = await _fixture.CreateWaiverInvitation(waiverId, eventParticipantId, myContactId);
            Assert.AreEqual(999999, result.Invitation.InvitationId);
        }

        private static MpEventParticipant MpEventParticipant()
        {
            return new MpEventParticipant
            {
                EventTitle = "blah"
            };
        }

        private static MpEventParticipantWaiver MpEventParticpantWaiver(bool accepted, int signeeId, int waiverId )
        {
            return new MpEventParticipantWaiver
            {
                Accepted = accepted,
                EventParticipantId = 12001,
                EventParticipantWaiverId = 13,
                SignerId = signeeId,
                WaiverId = waiverId
            };
        }

        private static MpCommunication MpCommunication(MpMessageTemplate template, MpSimpleContact to)
        {
            return new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact {EmailAddress = template.FromEmailAddress, ContactId = template.FromContactId},
                MergeData = It.IsAny<Dictionary<string, object>>(),
                ReplyToContact = new MpContact {EmailAddress = template.ReplyToEmailAddress, ContactId = template.ReplyToContactId},
                StartDate = DateTime.Now,
                TemplateId = 23,
                ToContacts = new List<MpContact> {new MpContact { ContactId = to.ContactId, EmailAddress = to.EmailAddress }}
            };
        }

        private static MpMessageTemplate Template()
        {
            return new MpMessageTemplate
            {
                Body = "words and things",
                FromContactId = 24,
                FromEmailAddress = "",
                ReplyToEmailAddress = "",
                ReplyToContactId = 1123,
                Subject = "Sub"
            };
        }

        private static ContactInvitation ContactInvitation()
        {
            return new ContactInvitation
            {
                Contact = new MpSimpleContact
                {
                    ContactId = 12234,
                    DateOfBirth = DateTime.Now.AddDays(-1000),
                    EmailAddress = "blah@Halb.com",
                    FirstName = "Blah",
                    LastName = "Halb",
                    Nickname = "b"
                },
                Invitation = new MpInvitation
                {
                    EmailAddress = "blah@Halb.com",
                    InvitationGuid = new Guid().ToString(),
                    InvitationType = 23,
                    RecipientName = "Blah Halb",
                    InvitationId = 34,
                    SourceId = 999
                }
            };
        }

        private static MpEventWaivers MpEventWaiver(int waiverId)
        {
            return new MpEventWaivers
            {
                Required = true,
                WaiverId = waiverId,
                WaiverName = "Waiver 3",
                WaiverText = "Blah Blah Blah"
            };
        }

        private static MpWaivers MpWaiver(DateTime startDate)
        {
            return new MpWaivers
            {
                WaiverEndDate = null,
                WaiverId = 23,
                WaiverName = "Blah",
                WaiverStartDate = startDate,
                WaiverText = "You Agree to all the things"
            };
        }
    }
}