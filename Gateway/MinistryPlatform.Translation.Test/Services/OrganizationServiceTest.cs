﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Services;
using MinistryPlatform.Translation.Services.Interfaces;
using Moq;
using MvcContrib.Filters;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class OrganizationServiceTest
    {
        private OrganizationService _fixture;
        private Mock<IMinistryPlatformService> _mpServiceMock;
        private Mock<IAuthenticationService> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;

        private const int ORGPAGE = 1234;

        [SetUp]
        public void SetUp()
        {
            _mpServiceMock = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationService>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _configWrapper.Setup(m => m.GetConfigIntValue("OrganizationsPage")).Returns(ORGPAGE);
            _authService.Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<string, object> { { "token", "ABC" }, { "exp", "123" } });

            _fixture = new OrganizationService(_authService.Object, _configWrapper.Object, _mpServiceMock.Object);
        }

        [Test]
        public void ShouldGetOrganization()
        {
            var orgs = ListOfOneUniquelyNamedOrganization();
            var fakeToken = "randomString";
            var name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                ORGPAGE, fakeToken, string.Format("{0},", name), It.IsAny<string>())
             ).Returns(orgs);
            var ret = _fixture.GetOrganization(name, fakeToken);

            Assert.IsInstanceOf<MPOrganization>(ret, "The return value is not an instance of an MPOrganization");
            Assert.AreEqual(orgs.FirstOrDefault().ToString("Name"), ret.Name);
        }

        [Test]
        public void ShouldThrowExceptionIfMultipleOrgsReturned()
        {
            var fakeToken = "randomString";
            var name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                ORGPAGE, fakeToken, string.Format("{0},", name), It.IsAny<string>())
             ).Returns(ListOfOrganizationsWithSameName());
            Assert.Throws<InvalidOperationException>(() => _fixture.GetOrganization(name, fakeToken));
        }

        [Test]
        public void ShouldHandleAnInvalidOrg()
        {
            var orgs = ListOfOneUniquelyNamedOrganization(false);
            var fakeToken = "randomString";
            var name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                ORGPAGE, fakeToken, string.Format("{0},", name), It.IsAny<string>())
             ).Returns(orgs);            
            Assert.Throws<KeyNotFoundException>(() => _fixture.GetOrganization(name, fakeToken));
        }

        [Test]
        public void ShouldHandleNoOrgs()
        {
            var emptyList = new List<Dictionary<string, object>>();
            var fakeToken = "randomString";
            var name = "Ingage";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
                ORGPAGE, fakeToken, string.Format("{0},", name), It.IsAny<string>())
             ).Returns(emptyList);
            var ret = _fixture.GetOrganization(name, fakeToken);
            Assert.IsNull(ret);
        }

        [Test]
        public void ShouldGetAListOfOrgs()
        {
            var fakeToken = "randomString";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
               ORGPAGE, fakeToken, It.IsAny<string>(), It.IsAny<string>())
            ).Returns(ListOfValidOrganizations);
            var ret = _fixture.GetOrganizations(fakeToken);
            Assert.IsInstanceOf<List<MPOrganization>>(ret, "The return value is not an instance of an MPOrganization");
        }

        [Test]
        public void ShouldHandleEmptyListOfOrgs()
        {
            var fakeToken = "randomString";
            _mpServiceMock.Setup(m => m.GetRecordsDict(
               ORGPAGE, fakeToken, It.IsAny<string>(), It.IsAny<string>())
            ).Returns(new List<Dictionary<string, object>>());
            var ret = _fixture.GetOrganizations(fakeToken);
            Assert.IsInstanceOf<List<MPOrganization>>(ret, "The return value is not an instance of an MPOrganization");
            Assert.IsEmpty(ret);
        }

        private List<Dictionary<string, object>> ListOfValidOrganizations()
        {
            return new List<Dictionary<string, object>>()
            {
                ValidMPOrganization("Ingage"),
                ValidMPOrganization("Crossroads")
            };
        }

        private static List<Dictionary<string, object>> ListOfOneUniquelyNamedOrganization(bool valid = true)
        {
            if (valid)
            {
                return new List<Dictionary<string, object>>()
                {
                    ValidMPOrganization("Ingage")
                };
            }
            return new List<Dictionary<string, object>>()
            {
                InvalidMPOrganization("Ingage")
            };
        }

        private List<Dictionary<string, object>> ListOfOrganizationsWithSameName()
        {
            return new List<Dictionary<string, object>>()
            {
                ValidMPOrganization("Ingage"),
                ValidMPOrganization("Ingage")
            };
        }

        private static Dictionary<string, object> ValidMPOrganization(string name)
        {
            return new Dictionary<string, object>()
            {
                {"dp_RecordID", It.IsAny<int>()},
                {"Primary_Contact", It.IsAny<int>()},
                {"End_Date", It.IsAny<DateTime>()},
                {"Start_Date", It.IsAny<DateTime>()},
                {"Name", name},
                {"Open_Signup", true}
            };
        }

        private static Dictionary<string, object> InvalidMPOrganization(string name)
        {
            return new Dictionary<string, object>()
            {
                {"dp_RecordID", It.IsAny<int>()},
                {"Primary_Contact", It.IsAny<int>()},
                {"Start_Date", It.IsAny<DateTime>()},
                {"Name", name},
                {"Open_Signup", true}
            };
        }

    }
}
