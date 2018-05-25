using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class OrganizationServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            _mpServiceMock = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _configWrapper.Setup(m => m.GetConfigIntValue("OrganizationsPage")).Returns(OrgPage);
            _configWrapper.Setup(m => m.GetConfigIntValue("LocationsForOrg")).Returns(LocPage);
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new OrganizationRepository(_authService.Object, _configWrapper.Object, _mpServiceMock.Object);
        }

        private OrganizationRepository _fixture;
        private Mock<IMinistryPlatformService> _mpServiceMock;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;

        private const int OrgPage = 1234;
        private const int LocPage = 180;
        private const string FakeToken = "randomString";

        private List<Dictionary<string, object>> ListOfValidOrganizations()
        {
            return new List<Dictionary<string, object>>()
            {
                ValidMpOrganization("Ingage"),
                ValidMpOrganization("Crossroads")
            };
        }

        private static List<Dictionary<string, object>> ListOfOneUniquelyNamedOrganization(bool valid = true)
        {
            if (valid)
            {
                return new List<Dictionary<string, object>>()
                {
                    ValidMpOrganization("Ingage")
                };
            }
            return new List<Dictionary<string, object>>()
            {
                InvalidMpOrganization("Ingage")
            };
        }

        private List<Dictionary<string, object>> ListOfOrganizationsWithSameName()
        {
            return new List<Dictionary<string, object>>()
            {
                ValidMpOrganization("Ingage"),
                ValidMpOrganization("Ingage")
            };
        }

        private static Dictionary<string, object> ValidMpOrganization(string name)
        {
            return new Dictionary<string, object>()
            {
                {"dp_RecordID", It.IsAny<int>()},
                {"Primary_Contact", It.IsAny<int>()},
                {"End_Date", It.IsAny<DateTime>()},
                {"Start_Date", It.IsAny<DateTime>()},
                {"Name", name},
                {"Open_Signup", true},
                {"Image_URL", "www.com.net"}
            };
        }

        private static Dictionary<string, object> InvalidMpOrganization(string name)
        {
            return new Dictionary<string, object>()
            {
                {"dp_RecordID", It.IsAny<int>()},
                {"Primary_Contact", It.IsAny<int>()},
                {"Start_Date", It.IsAny<DateTime>()},
                {"Name", name},
                {"Open_Signup", true},
                {"Image_URL", "www.com.net"}
            };
        }

        private static List<Dictionary<string, object>> LocationList()
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Location ID", It.IsAny<int>()},
                    {"Location Name", "Oakley"},
                    {"Location Type ID", 1},
                    {"Organization ID", 1},
                    {"Name", "Crossroads"},
                    {"Address Line 1", "123 Sesame St."},
                    {"City", "Cincinnati"},
                    {"State/Region", "OH"},
                    {"Postal Code", 45209},
                    {"Image URL", "www.com.net"}
                },
                new Dictionary<string, object>
                {
                    {"Location ID", It.IsAny<int>()},
                    {"Location Name", "Mason"},
                    {"Location Type ID", 1},
                    {"Organization ID", 1},
                    {"Name", "Crossroads"},
                    {"Address Line 1", "221B Baker St."},
                    {"City", "Cincinnati"},
                    {"State/Region", "OH"},
                    {"Postal Code", 45209},
                    {"Image URL", "www.com.net"}
                }
            };
        }

        [Test]
        public void ShouldGetAListOfOrgs()
        {
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                It.IsAny<string>(),
                It.IsAny<string>())
                ).Returns(ListOfValidOrganizations);
            var ret = _fixture.GetOrganizations(FakeToken);
            Assert.IsInstanceOf<List<MPOrganization>>(ret, "The return value is not an instance of an MPOrganization");
        }

        [Test]
        public void ShouldGetLocations()
        {
            _mpServiceMock.Setup(m => m.GetSubpageViewRecords(LocPage, It.IsAny<int>(), FakeToken, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(LocationList());
            var ret = _fixture.GetLocationsForOrganization(1, FakeToken);
            Assert.IsInstanceOf<List<MpOrgLocation>>(ret);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void ShouldGetOrganization()
        {
            var orgs = ListOfOneUniquelyNamedOrganization();
            const string name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                string.Format("{0},", name),
                It.IsAny<string>())
                ).Returns(orgs);
            var ret = _fixture.GetOrganization(name, FakeToken);

            Assert.IsInstanceOf<MPOrganization>(ret, "The return value is not an instance of an MPOrganization");
            Assert.AreEqual(orgs.FirstOrDefault().ToString("Name"), ret.Name);
        }

        [Test]
        public void ShouldHandleAnInvalidOrg()
        {
            var orgs = ListOfOneUniquelyNamedOrganization(false);
            const string name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                string.Format("{0},", name),
                It.IsAny<string>())
                ).Returns(orgs);
            Assert.Throws<KeyNotFoundException>(() => _fixture.GetOrganization(name, FakeToken));
        }

        [Test]
        public void ShouldHandleEmptyListOfOrgs()
        {
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                It.IsAny<string>(),
                It.IsAny<string>())
                ).Returns(new List<Dictionary<string, object>>());
            var ret = _fixture.GetOrganizations(FakeToken);
            Assert.IsInstanceOf<List<MPOrganization>>(ret, "The return value is not an instance of an MPOrganization");
            Assert.IsEmpty(ret);
        }

        [Test]
        public void ShouldHandleNoOrgs()
        {
            var emptyList = new List<Dictionary<string, object>>();
            const string name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                string.Format("{0},", name),
                It.IsAny<string>())
                ).Returns(emptyList);
            var ret = _fixture.GetOrganization(name, FakeToken);
            Assert.IsNull(ret);
        }

        [Test]
        public void ShouldThrowExceptionIfMultipleOrgsReturned()
        {
            const string name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                OrgPage,
                FakeToken,
                string.Format("{0},", name),
                It.IsAny<string>())
                ).Returns(ListOfOrganizationsWithSameName());
            Assert.Throws<InvalidOperationException>(() => _fixture.GetOrganization(name, FakeToken));
        }
    }
}