using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Models.Crossroads.Lookups;
using crds_angular.Services.Interfaces;
using FsCheck;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;
using crds_angular.Util;

namespace crds_angular.test.controllers
{
    public class GoVolunteerControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IAttributeService> _attributeService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private GoVolunteerController _fixture;
        private Mock<IGatewayLookupService> _gatewayLookupService;
        private Mock<IGoVolunteerService> _goVolunteerService;
        private Mock<IGroupConnectorService> _groupConnectorService;
        private Mock<IOrganizationService> _organizationService;
        private Mock<IGoSkillsService> _skillsService;
        private string authToken;
        private string authType;


        [SetUp]
        public void Setup()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _organizationService = new Mock<IOrganizationService>();
            _gatewayLookupService = new Mock<IGatewayLookupService>();
            _goVolunteerService = new Mock<IGoVolunteerService>();
            _skillsService = new Mock<IGoSkillsService>();
            _groupConnectorService = new Mock<IGroupConnectorService>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _attributeService = new Mock<IAttributeService>();

            authType = "authType";
            authToken = "authToken";

            _fixture = new GoVolunteerController(_authTokenExpiryService.Object, 
                                                 _organizationService.Object,
                                                 _groupConnectorService.Object,
                                                 _gatewayLookupService.Object,
                                                 _skillsService.Object,
                                                 _goVolunteerService.Object,
                                                 _attributeService.Object,
                                                 _configurationWrapper.Object,
                                                 new Mock<IUserImpersonationService>().Object,
                                                 new Mock<IAuthenticationRepository>().Object)

            {
                Request = new HttpRequestMessage(),
                RequestContext = new HttpRequestContext()
            };

            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
        }

        [Test]
        public void ShouldGetSkillsList()
        {
            string token = $"{authType} {authToken}";
            const int listSize = 20;
            var skills = TestHelpers.ListOfGoSkills(listSize);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _skillsService.Setup(m => m.RetrieveGoSkills(token)).Returns(skills);
            var response = _fixture.GetGoSkills();
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<GoSkills>>>(response);
            var r = (OkNegotiatedContentResult<List<GoSkills>>) response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual(r.Content.Count, listSize);
            Assert.AreSame(skills, r.Content);
        }

        [Test]
        public void ShouldGetRequestedProject()
        {
            const int projectId = 564;

            var project = new Project
            {
                AddressId = 1,
                InitiativeId = 2,
                LocationId = 3,
                OrganizationId = 4,
                ProjectId = projectId,
                ProjectName = "Make Cleveland Great (Again?)",
                ProjectStatusId = 5,
                ProjectTypeId = 6
            };

            _goVolunteerService.Setup(m => m.GetProject(564)).Returns(project);
            var response = _fixture.GetProject(projectId);
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<Project>>(response);
            var r = (OkNegotiatedContentResult<Project>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual(r.Content.ProjectId, project.ProjectId);
            Assert.AreSame(project, r.Content);
        }

        [Test]
        public void ShouldThrowExceptionIfProjectNotFound()
        {
            const int projectId = 564;

            var project = new Project
            {
                AddressId = 1,
                InitiativeId = 2,
                LocationId = 3,
                OrganizationId = 4,
                ProjectId = projectId,
                ProjectName = "Make Cleveland Great (Again?)",
                ProjectStatusId = 5,
                ProjectTypeId = 6
            };

            _goVolunteerService.Setup(m => m.GetProject(564)).Throws<ApplicationException>();
            Assert.Throws<HttpResponseException>(() => {
                _fixture.GetProject(projectId);
                _goVolunteerService.VerifyAll();
            });
             
        }

        [Test]
        public void ShouldGetOrganizationByName()
        {
            Prop.ForAll<int, int, string>((contactId, orgId, name) =>
            {
                var returnValue = ValidOrganization(contactId, orgId, name);
                _organizationService.Setup(m => m.GetOrganizationByName(name)).Returns(returnValue);
                var response = _fixture.GetOrganization(name);
                Assert.IsNotNull(response);
                Assert.IsInstanceOf<OkNegotiatedContentResult<Organization>>(response);
                // ReSharper disable once SuspiciousTypeConversion.Global
                var r = (OkNegotiatedContentResult<Organization>) response;
                Assert.IsNotNull(r.Content);
                Assert.AreSame(returnValue, r.Content);
            }).VerboseCheckThrowOnFailure();
        }

        [Test]
        public void ShouldHandleNullOrganization()
        {
            _organizationService.Setup(m => m.GetOrganizationByName(It.IsAny<string>())).Returns((Organization) null);
            Prop.ForAll<string>(st =>
            {
                var response = _fixture.GetOrganization(st);
                Assert.IsNotNull(response);
                Assert.IsInstanceOf<NotFoundResult>(response);
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void ShouldGetListOfOtherOrganizations()
        {
            var orgs = otherOrganizations();
            _gatewayLookupService.Setup(m => m.GetOtherOrgs(null)).Returns(orgs);


            var response = _fixture.GetOtherOrganizations();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<OtherOrganization>>>(response);
            // ReSharper disable once SuspiciousTypeConversion.Global
            var r = (OkNegotiatedContentResult<List<OtherOrganization>>) response;
            Assert.IsNotNull(r.Content);
            Assert.AreSame(orgs, r.Content);
        }

        [Test]
        [ExpectedException(typeof (HttpResponseException))]
        public void ShouldThrowAnException()
        {
            _gatewayLookupService.Setup(m => m.GetOtherOrgs(null)).Throws(new Exception());
            _fixture.GetOtherOrganizations();
        }

        [Test]
        public void ShouldGetPrepTimes()
        {
            Prop.ForAll<int>(config =>
            {
                _configurationWrapper.Setup(m => m.GetConfigIntValue("PrepWorkAttributeTypeId")).Returns(config);
                var attributeTypes = TestHelpers.ListOfAttributeTypeDtos(1);
                _attributeService.Setup(m => m.GetAttributeTypes(config)).Returns(attributeTypes);
                var response = _fixture.GetPrepTimes();
                Assert.IsNotNull(response);
                Assert.IsInstanceOf<OkNegotiatedContentResult<List<AttributeDTO>>>(response);
                // ReSharper disable once SuspiciousTypeConversion.Global
                var r = (OkNegotiatedContentResult<List<AttributeDTO>>) response;
                Assert.IsNotNull(r.Content);
                Assert.AreSame(attributeTypes.Single().Attributes, r.Content);
                _attributeService.VerifyAll();
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void ShouldThrowExceptionIfThereAreMoreThanOneAttributeType()
        {
            Prop.ForAll<int>(config =>
            {
                _configurationWrapper.Setup(m => m.GetConfigIntValue("PrepWorkAttributeTypeId")).Returns(config);
                var attributeTypes = TestHelpers.ListOfAttributeTypeDtos(10);
                _attributeService.Setup(m => m.GetAttributeTypes(config)).Returns(attributeTypes);
                Assert.Throws<HttpResponseException>(() => { _fixture.GetPrepTimes(); });
                _attributeService.VerifyAll();
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void ShouldGetTheGroupLeaderExportFile()
        {
            var projectId = 123;
            var stream = new MemoryStream();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _goVolunteerService.Setup(m => m.CreateGroupLeaderExport(projectId)).Returns(stream);

            var response = _fixture.GetGroupLeaderExportFile(projectId);

            _goVolunteerService.VerifyAll();
            Assert.IsInstanceOf<FileResult>(response);
        }

        [Test]
        public void ShouldThrowExceptionOnGroupLeaderExportFile()
        {
            var projectId = 123;
            var stream = new MemoryStream();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _goVolunteerService.Setup(m => m.CreateGroupLeaderExport(projectId)).Throws<Exception>();
            Assert.Throws<HttpResponseException>(() =>
            {
                var response = _fixture.GetGroupLeaderExportFile(projectId);
                _goVolunteerService.VerifyAll();
            });
        }

        [Test]
        public void ShouldThrowHttpResponseExceptionConflictDueToDuplicateUser()
        {
            const int projectId = 123;
            string token = $"{authType} {authToken}";
            var registration = BuildAnywhereRegistration();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _goVolunteerService.Setup(m => m.CreateAnywhereRegistration(registration, projectId, token))
                .Throws(new DuplicateUserException(registration.Self.EmailAddress));

            try
            {
                _fixture.Post(registration, projectId);
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(e.Response.StatusCode, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

            _goVolunteerService.VerifyAll();
        }

        private AnywhereRegistration BuildAnywhereRegistration()
        {
            return new AnywhereRegistration()
            {
                InitiativeId = 12,
                SpouseParticipation = false,
                Self = new Registrant()
                {
                    EmailAddress = "abomb@thebomb.com"
                }
            };
        }

        private List<OtherOrganization> otherOrganizations()
        {
            return new List<OtherOrganization>()
            {
                new OtherOrganization(12, "sdfrtrtg"),
                new OtherOrganization(15, "dghjhnjmh"),
                new OtherOrganization(13, "gfhnnhmjm")
            };
        }

        private static Organization ValidOrganization(int contactId, int orgId, string name)
        {
            return new Organization()
            {
                ContactId = contactId,
                EndDate = new DateTime(),
                StartDate = new DateTime(),
                Name = name,
                OpenSignup = true,
                OrganizationId = orgId
            };
        }
    }
}
