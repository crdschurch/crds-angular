using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ContactRelationshipServiceTest
    {
        private ContactRelationshipRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;

        private readonly int CONTACT_RELATIONSHIP_PAGE = 265;
        private readonly int CONTACT_RELATIONSHIP_ID = 110;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
            _fixture = new ContactRelationshipRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _apiUserService.Object);
            Environment.SetEnvironmentVariable("API_USER", "uid");
            Environment.SetEnvironmentVariable("API_PASSWORD", "pwd");

            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("ContactRelationships")).Returns(CONTACT_RELATIONSHIP_PAGE);
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("ContactRelationshipsIds")).Returns(CONTACT_RELATIONSHIP_ID);
        }

        [Test]
        public void AddRelationship()
        {
            const int childId = 4384766;
            const int myId = 2186211;


            MpRelationship r = new MpRelationship
            {
                RelationshipID = 43,
                EndDate = null,
                StartDate = DateTime.Now,
                RelatedContactID = childId
            };

            var dict = new Dictionary<string, object>
            {
                {"Relationship_ID", r.RelationshipID},
                {"Related_Contact_ID", r.RelatedContactID},
                {"Start_Date", r.StartDate},
                {"End_Date", r.EndDate}
            };


            _ministryPlatformService.Setup(mocked =>
                                               mocked.CreateSubRecord(CONTACT_RELATIONSHIP_PAGE,
                                                                      myId,
                                                                      dict,
                                                                      It.IsAny<string>(),
                                                                      true)).Returns(1);

            var id = _fixture.AddRelationship(r, myId);
            Assert.AreEqual(1, id);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void GetSomeonesRelationships()
        {
            const int childId = 4384766;
            const int myId = 2186211;

            var relationships = new List<MpRelationship>
            {
                new MpRelationship
                {
                    RelationshipID = 43,
                    RelatedContactID = childId,
                    StartDate = DateTime.Today,
                    EndDate = null
                }
            };

            var subpageDict = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Relationship_ID", 43},
                    {"Related_Contact_ID", childId},
                    {"End_Date", null},
                    {"Start_Date", DateTime.Today}
                }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetSubpageViewRecords("ContactRelationshipsIds", myId, It.IsAny<string>(), "", "", 0)).Returns(subpageDict);

            var result = _fixture.GetMyCurrentRelationships(myId).ToList();
            Assert.AreEqual(relationships[0].RelatedContactID, result[0].RelatedContactID);
            Assert.AreEqual(relationships[0].RelationshipID, result[0].RelationshipID);
            Assert.AreEqual(relationships[0].StartDate, result[0].StartDate);
            Assert.AreEqual(relationships[0].EndDate, result[0].EndDate);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void GetMyFamily()
        {
            const int contactId = 123456;
            const string token = "some-string";

            //mock GetSubpageViewRecords
            var getSubpageViewRecordsResponse = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    {"Contact_ID", 123456},
                    {"Email_Address", "person-one@email.com"},
                    {"Preferred Name", "person-one"},
                    {"Last Name", "person-one"},
                    {"Participant_ID", 654321},
                    {"Relationship_ID", 1},
                    {"Age", 40},
                    {"HS_Graduation_Year", 0}
                },
                new Dictionary<string, object>()
                {
                    {"Contact_ID", 222222},
                    {"Email_Address", "person-two@email.com"},
                    {"Preferred Name", "person-two"},
                    {"Last Name", "person-two"},
                    {"Participant_ID", 3333333},
                    {"Relationship_ID", 1},
                    {"Age", 40},
                    {"HS_Graduation_Year", 0}
                }
            };
            _ministryPlatformService.Setup(
                mocked =>
                    mocked.GetSubpageViewRecords("MyContactFamilyRelationshipViewId",
                                                 contactId,
                                                 It.IsAny<string>(),
                                                 "",
                                                 "",
                                                 0))
                .Returns(getSubpageViewRecordsResponse);

            var family = _fixture.GetMyImmediateFamilyRelationships(contactId).ToList();

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(family);
            Assert.AreEqual(2, family.Count());

            Assert.AreEqual(123456, family[0].Contact_Id);
            Assert.AreEqual("person-one@email.com", family[0].Email_Address);
            Assert.AreEqual("person-one", family[0].Preferred_Name);
            Assert.AreEqual("person-one", family[0].Last_Name);

            Assert.AreEqual(222222, family[1].Contact_Id);
            Assert.AreEqual("person-two@email.com", family[1].Email_Address);
            Assert.AreEqual("person-two", family[1].Preferred_Name);
            Assert.AreEqual("person-two", family[1].Last_Name);
        }
    }
}