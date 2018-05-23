using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Services;
using Crossroads.Utilities.Interfaces;
using FsCheck;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using MvcContrib.TestHelper.Ui;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class GoSkillsServiceTest
    {
        private GoSkillsService _fixture;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<ISkillsRepository> _skillsService;
        private Mock<crds_angular.Services.Interfaces.IObjectAttributeService> _objectAttributeService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IContactRepository> _contactService;
        private const int SKILLSATTRIBUTETYPEID = 123;

        [SetUp]
        public void Setup()
        {
            _apiUserService = new Mock<IApiUserRepository>();
            _skillsService = new Mock<ISkillsRepository>();
            _objectAttributeService = new Mock<crds_angular.Services.Interfaces.IObjectAttributeService>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _contactService = new Mock<IContactRepository>();
            _fixture = new GoSkillsService(_apiUserService.Object, _skillsService.Object, _objectAttributeService.Object, _contactService.Object, _configurationWrapper.Object);
            
        }

        [Test]
        public void ShouldRetrieveGoSkills()
        {

            Prop.ForAll<string>( token =>
            {
                var skills = TestHelpers.MPSkills();
                _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
                _skillsService.Setup(m => m.GetGoVolunteerSkills(token)).Returns(skills);
                var returned = _fixture.RetrieveGoSkills(string.Empty);
                Assert.IsInstanceOf<List<GoSkills>>(returned);
                Assert.AreEqual(skills.Count, returned.Count);
                _skillsService.VerifyAll();
            }).QuickCheckThrowOnFailure();           
        }

        [Test]
        public void ShouldUpdateSkills()
        {
            var skills = TestHelpers.ListOfGoSkills(3);
            var currentSkills = CurrentSkills(skills);           
            var contact = TestHelpers.MyContact();
            const string token = "token";
            var participantId = TestHelpers.RandomInt();
            _configurationWrapper.Setup(m => m.GetConfigIntValue("AttributeTypeIdSkills")).Returns(SKILLSATTRIBUTETYPEID);
            _contactService.Setup(m => m.GetContactByParticipantId(participantId)).Returns(contact);
            _objectAttributeService.Setup(m => m.GetObjectAttributes(token, contact.Contact_ID, It.IsAny<MpObjectAttributeConfiguration>())).Returns(currentSkills);

            var toEndDate = _fixture.SkillsToEndDate(skills, currentSkills.MultiSelect[1].Attributes).Select(sk =>
            {
                sk.EndDate = DateTime.Now;
                return sk;
            });
            var toAdd = _fixture.SkillsToAdd(skills, currentSkills.MultiSelect[1].Attributes);
            var total = toAdd.Concat(toEndDate);
            total.ForEach(skill =>
            {
                if (skill.EndDate != null)
                {
                    skill.EndDate = It.IsAny<DateTime>();
                }
                _objectAttributeService.Setup(m => m.SaveObjectMultiAttribute(token, contact.Contact_ID, skill, It.IsAny<MpObjectAttributeConfiguration>(), false));
                _objectAttributeService.Verify();
            });
            _fixture.UpdateSkills(participantId, skills, token);            
            _contactService.Verify();
            
        }

        [Test]
        public void ShouldGetSkillsToEndDate()
        {
            var skills = TestHelpers.ListOfGoSkills(3);
            var currentSkills = CurrentSkills(skills);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("AttributeTypeIdSkills")).Returns(SKILLSATTRIBUTETYPEID);           
            var skillsToEndDate = _fixture.SkillsToEndDate(skills, currentSkills.MultiSelect[1].Attributes);
            Assert.IsInstanceOf<List<ObjectAttributeDTO>>(skillsToEndDate);
            Assert.AreEqual(2, skillsToEndDate.Count);            
        }

        [Test]
        public void ShouldGetSkillsToAdd()
        {
            var skills = TestHelpers.ListOfGoSkills(3);
            var currentSkills = CurrentSkills(skills);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("AttributeTypeIdSkills")).Returns(SKILLSATTRIBUTETYPEID);
            var skillsToAdd = _fixture.SkillsToAdd(skills, currentSkills.MultiSelect[1].Attributes);
            Assert.AreEqual(2, skillsToAdd.Count);
        }

        public ObjectAllAttributesDTO CurrentSkills(List<GoSkills> skills)
        {
            return new ObjectAllAttributesDTO()
            {
                MultiSelect = new Dictionary<int, ObjectAttributeTypeDTO>()
                {
                    {1, new ObjectAttributeTypeDTO()
                        {
                            AttributeTypeId = SKILLSATTRIBUTETYPEID,
                            Attributes = new List<ObjectAttributeDTO>()
                            {
                                new ObjectAttributeDTO()
                                {
                                    AttributeId = skills.First().AttributeId,
                                    Name = skills.First().Name,
                                    EndDate = null,
                                    Selected = true
                                },
                                new ObjectAttributeDTO()
                                {
                                    AttributeId = 1290848210,
                                    Name = "not a duplicate",
                                    EndDate = null,
                                    Selected = true
                                },
                                new ObjectAttributeDTO()
                                {
                                    AttributeId = 323284821,
                                    Name = "not a duplicate",
                                    EndDate = null,
                                    Selected = true
                                },

                            }
                        }
                    },
                    {2, new ObjectAttributeTypeDTO() }
                }
            };
        }

    }
}
