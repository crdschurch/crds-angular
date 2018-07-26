using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.GroupLeader;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;
using System.Reactive.Disposables;
using System.Web.Http;
using MinistryPlatform.Translation.Models;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class GroupLeaderControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IGroupLeaderService> _groupLeaderService;
        private Mock<IUserImpersonationService> _userImpersonation;
        private Mock<IAuthenticationRepository> _authenticationRepo;
        private GroupLeaderController _fixture;

        private const string authToken = "authtoken";
        private const string authType = "authtype";

        [SetUp]
        public void Setup()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _groupLeaderService = new Mock<IGroupLeaderService>(MockBehavior.Strict);
            _userImpersonation = new Mock<IUserImpersonationService>();
            _authenticationRepo = new Mock<IAuthenticationRepository>();   
            _fixture = new GroupLeaderController(_authTokenExpiryService.Object, 
                                                 _groupLeaderService.Object, 
                                                 _userImpersonation.Object, 
                                                 _authenticationRepo.Object)
            {
                Request = new HttpRequestMessage(),
                RequestContext = new HttpRequestContext()
            };
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
        }

        [TearDown]
        public void Teardown()
        {
            _groupLeaderService.VerifyAll();
            _authenticationRepo.VerifyAll();
            _userImpersonation.VerifyAll();   
        }

        [Test]
        public void ShouldThrowExceptionWhenProfileIsNotSaved()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockProfile = GroupLeaderMock();
            _groupLeaderService.Setup(m => m.SaveReferences(It.IsAny<GroupLeaderProfileDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SaveProfile(It.IsAny<string>(), mockProfile)).Throws(new Exception());            
            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveProfile(mockProfile);                
            });           
        }

        [Test]
        public void ShouldOnlyAllowAuthenticatedUsers()
        {            
            var mockProfile = GroupLeaderMock();
            _groupLeaderService.Setup(m => m.SaveReferences(It.IsAny<GroupLeaderProfileDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SaveProfile(It.IsAny<string>(), mockProfile)).Throws(new Exception());
            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveProfile(mockProfile);                
            });

        }

        [Test]
        public async void ShouldSaveInterestedStaus()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            _groupLeaderService.Setup(m => m.SetInterested($"{authType} {authToken}"));

            var response = await _fixture.InterestedInGroupLeadership();
            Assert.IsNotNull(response);
        }

        [Test]
        public async void ShouldSaveSpiritualGrowthAnswers()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockSpiritualGrowth = SpiritualGrowthMock();
            const int referenceContactId = 987654;
            const string studentLeaderRequest = "true";
            var participant = ParticipantMock();
            var contact = ContactMock(mockSpiritualGrowth.ContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", referenceContactId.ToString() },
                { "studentLeaderRequest", studentLeaderRequest  }
            };

            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SetApplied(It.IsAny<string>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.GetApplicationData(mockSpiritualGrowth.ContactId)).Returns(Observable.Start(() => referenceData));            

            var response = await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public async void ShouldSendNoReferenceEmail()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockSpiritualGrowth = SpiritualGrowthMock();
            var participant = ParticipantMock();
            var contact = ContactMock(mockSpiritualGrowth.ContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", "0" }
            };

            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SetApplied(It.IsAny<string>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.GetApplicationData(mockSpiritualGrowth.ContactId)).Returns(Observable.Start(() => referenceData));

            var response = await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public async void ShouldSendStudentLeaderRequestEmail()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockSpiritualGrowth = SpiritualGrowthMock();
            const string studentLeaderRequest = "true";
            var participant = ParticipantMock();
            var contact = ContactMock(mockSpiritualGrowth.ContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", "0" },
                { "studentLeaderRequest", studentLeaderRequest  }
            };

            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SetApplied(It.IsAny<string>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.GetApplicationData(mockSpiritualGrowth.ContactId)).Returns(Observable.Start(() => referenceData));
            _groupLeaderService.Setup(m => m.SendStudentMinistryRequestEmail(It.IsAny<Dictionary<string, object>>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SendNoReferenceEmail(It.IsAny<Dictionary<string, object>>())).Returns(Observable.Start(() => 1));

            var response = await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public async void ShouldNotSendStudentLeaderRequestEmail()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockSpiritualGrowth = SpiritualGrowthMock();
            const string studentLeaderRequest = "true";
            var participant = ParticipantMock();
            var contact = ContactMock(mockSpiritualGrowth.ContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", "0" },
                { "studentLeaderRequest", studentLeaderRequest  }
            };

            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SetApplied(It.IsAny<string>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.GetApplicationData(mockSpiritualGrowth.ContactId)).Returns(Observable.Start(() => referenceData));
            _groupLeaderService.Setup(m => m.SendNoReferenceEmail(It.IsAny<Dictionary<string, object>>())).Returns(Observable.Start(() => 1));

            var response = await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void ShouldNotSendEmailIfSpiritualGrowthFails()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            var mockSpiritualGrowth = SpiritualGrowthMock();
            const int referenceContactId = 987654;
            var participant = ParticipantMock();
            var contact = ContactMock(mockSpiritualGrowth.ContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", referenceContactId.ToString() }
            };
            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Create<int>((observer) =>
            {
                observer.OnError(new Exception(""));
                return Disposable.Empty;
            }));
            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            });
        }

        [Test]
        public void ShouldThrowWhenSavingInterestedStaus()
        {
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            _groupLeaderService.Setup(m => m.SetInterested($"{authType} {authToken}")).Throws(new Exception());

            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.InterestedInGroupLeadership();
            });
        }

        [Test]
        public void ShouldThrowExceptionWhenSpiritualGrowthAnswersArentSaved()
        {
            var mockSpiritualGrowth = SpiritualGrowthMock();
            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SetApplied(It.IsAny<string>())).Returns(Observable.Create<int>((observer) =>
            {
                observer.OnError(new Exception(""));
                return Disposable.Empty;
            }));

            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            });
        }

        [Test]
        public void ShouldThrowExceptionWhenSpiritualGrowthAnswersAreSavedButNotApplied()
        {
            var mockSpiritualGrowth = SpiritualGrowthMock();

            _groupLeaderService.Setup(m => m.SaveSpiritualGrowth(It.IsAny<SpiritualGrowthDTO>())).Throws(new Exception());

            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveSpiritualGrowth(mockSpiritualGrowth);
            });
        }

        [Test]
        public void ShouldThrowExceptionWhenSaveProfileThrowsException()
        {
            var mockProfile = GroupLeaderMock();

            _groupLeaderService.Setup(m => m.SaveReferences(It.IsAny<GroupLeaderProfileDTO>())).Returns(Observable.Start(() => 1));
            _groupLeaderService.Setup(m => m.SaveProfile(It.IsAny<string>(), It.IsAny<GroupLeaderProfileDTO>())).Throws<ApplicationException>();

            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.SaveProfile(mockProfile);
            });
        }

        [Test]
        public async void ShouldGetLeaderStatus()
        {
            _groupLeaderService.Setup(m => m.GetGroupLeaderStatus(It.IsAny<string>())).Returns(Observable.Start(() => 0));

            var response = await _fixture.GetLeaderStatus();
            Assert.IsInstanceOf<OkNegotiatedContentResult<GroupLeaderStatusDTO>>(response);
        }

        [Test]
        public void ShouldThrowExceptionWhenGettingStatusFails()
        {
            _groupLeaderService.Setup(m => m.GetGroupLeaderStatus(It.IsAny<string>())).Throws<ApplicationException>();

            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.GetLeaderStatus();
            });
        }

        private static GroupLeaderProfileDTO GroupLeaderMock()
        {
            return new GroupLeaderProfileDTO()
            {
                ContactId = 12345,
                BirthDate = new DateTime(1980, 02, 21),
                Email = "silbermm@gmail.com",
                LastName = "Silbernagel",
                NickName = "Matt",
                Site = 1,
                OldEmail = "matt.silbernagel@ingagepartners.com",
                HomePhone = "123-456-7890"
            };
        }

        private static SpiritualGrowthDTO SpiritualGrowthMock()
        {
            return new SpiritualGrowthDTO()
            {
                ContactId = 654321,
                EmailAddress = "hornerjn@gmail.com",
                OpenResponse1 = "open response 1",
                OpenResponse2 = "open response 2",
                OpenResponse3 = "open response 3",
                OpenResponse4 = "open response 4"
            };
        }

        private static MpMyContact ContactMock(int contactId)
        {
            return new MpMyContact
            {
                First_Name = "Blah",
                Last_Name = "Halb",
                Contact_ID = contactId,
                Nickname = "B",
                Email_Address = "sumemail@mock.com"
            };
        }

        private static MpParticipant ParticipantMock()
        {
            return new MpParticipant
            {
                ContactId = 12345,
                ParticipantId = 67890,
                GroupLeaderStatus = 1,
                DisplayName = "Fakerson, Fakey"
            };
        }
    }
}
