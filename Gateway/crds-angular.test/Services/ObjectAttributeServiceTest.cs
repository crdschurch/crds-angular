using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using GateWayInterfaces = crds_angular.Services.Interfaces;
using Moq;
using NUnit.Framework;
using Rhino.Mocks;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using ObjectAttributeService = crds_angular.Services.ObjectAttributeService;

namespace crds_angular.test.Services
{
    public class ContactAttributeServiceTest
    {

        private ObjectAttributeService _fixture;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<MPInterfaces.IObjectAttributeRepository> _contactAttributeService;
        private Mock<GateWayInterfaces.IAttributeService> _attributeService;
        private Mock<MPInterfaces.IAttributeRepository> _mpAttributeService;
        private List<ObjectSingleAttributeDTO> _updatedAttributes = new List<ObjectSingleAttributeDTO>();
        private List<MpObjectAttribute> _currentAttributes = new List<MpObjectAttribute>();

        private int _fakeContactId = 2186211;
        private string _fakeToken = "afaketoken";
        private string _updatedNote = "New and Updated Notes";
        private DateTime _startDate = new DateTime(2015, 2, 21);

        [SetUp]
        public void Setup()
        {
            _mpAttributeService = new Mock<MPInterfaces.IAttributeRepository>(MockBehavior.Strict);
            _contactAttributeService = new Mock<MPInterfaces.IObjectAttributeRepository>();
            _attributeService = new Mock<GateWayInterfaces.IAttributeService>();
            _apiUserService = new Mock<IApiUserRepository>();
            _mpAttributeService = new Mock<MPInterfaces.IAttributeRepository>();

            _fixture = new ObjectAttributeService(_contactAttributeService.Object, _attributeService.Object, _apiUserService.Object, _mpAttributeService.Object);
            _updatedAttributes.Add(new ObjectSingleAttributeDTO
            {
                Value = new AttributeDTO
                {
                    AttributeId = 23,
                    Category = "Allergies",
                    Name = "All Allergies"
                },
                Notes = "New and Updated Notes"
 
            });  
            _currentAttributes.Add(new MpObjectAttribute
            {
                AttributeTypeId = 2,
                AttributeId = 23,
                AttributeTypeName = "Allergies",
                ObjectAttributeId = 123456,
                StartDate = _startDate,
                Notes = "original notes"
            });
        }

        [Test]
        public void ShouldUpdatePreviouslySavedAttributeNotes()
        {
            var contactAttributes = new Dictionary<int, ObjectAttributeTypeDTO>();

            var contactSingleAttributes = new Dictionary<int, ObjectSingleAttributeDTO>
            {
                {1, new ObjectSingleAttributeDTO
                    {
                        Value = new AttributeDTO
                        {
                            AttributeId = 23,
                            Category = "Allergies",
                            Name = "All Allergies"
                        },
                        Notes = _updatedNote
 
                    } 
                }
            };

            var configuration = MpObjectAttributeConfigurationFactory.Contact();

            _contactAttributeService.Setup(x => x.GetCurrentObjectAttributes(_fakeToken, _fakeContactId, configuration, null)).Returns(_currentAttributes);
            _apiUserService.Setup(x => x.GetDefaultApiClientToken()).Returns(_fakeToken);
            _contactAttributeService.Setup(x => x.UpdateAttribute(_fakeToken, It.IsAny<MpObjectAttribute>(), configuration)).Callback<string, MpObjectAttribute, MpObjectAttributeConfiguration>((id, actual, objectConfiguration) =>
            {
                Assert.AreEqual(actual.Notes, _updatedNote);  
                Assert.AreEqual(actual.ObjectAttributeId, 123456);
                Assert.AreEqual(configuration, objectConfiguration);
            });
            _fixture.SaveObjectAttributes(_fakeContactId, contactAttributes, contactSingleAttributes, configuration);
            _apiUserService.VerifyAll();
            _attributeService.VerifyAll();
            _mpAttributeService.VerifyAll();
            _contactAttributeService.Verify(update => update.UpdateAttribute(_fakeToken, It.IsAny<MpObjectAttribute>(), It.IsAny<MpObjectAttributeConfiguration>()), Times.Once);
            

        }

        
    }
}
