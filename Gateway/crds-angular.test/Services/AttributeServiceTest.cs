using System.Collections.Generic;
using System.Linq;
using crds_angular.Services;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.test.Services
{
    public class AttributeServiceTest
    {
        private AttributeService _fixture;
        private Mock<MPServices.IAttributeRepository> _mpAttributeRepository;
        private Mock<IMinistryPlatformRestRepository> _mpRestRepository;
        private Mock<IApiUserRepository> _mpApiUserRepository;

        [SetUp]
        public void SetUp()
        {
            _mpAttributeRepository = new Mock<MPServices.IAttributeRepository>(MockBehavior.Strict);
            _mpRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _mpApiUserRepository = new Mock<IApiUserRepository>();


            _fixture = new AttributeService(_mpAttributeRepository.Object, _mpRestRepository.Object, _mpApiUserRepository.Object);
        }

        [Test]
        public void Given_An_Attribute_List_When_Translated_To_AttributeTypes_Should_Create_Hierarchy_By_AttributeType()
        {
            var attriubuteResults = GetAttributesResults();
            _mpAttributeRepository.Setup(mocked => mocked.GetAttributes(null)).Returns(attriubuteResults);

            var result = _fixture.GetAttributeTypes(null);
            _mpAttributeRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2, "Records are not grouped by attributeType");
            Assert.AreEqual(result[0].Name, "AttributeType #1", "attributeType name not correct");
            Assert.AreEqual(result[0].AllowMultipleSelections, true);
            Assert.AreEqual(result[0].Attributes.Count, 2, "Number of attributes for attributeType not correct");


            Assert.AreEqual(result[1].Name, "AttributeType #2", "attributeType name not correct");
            Assert.AreEqual(result[1].AllowMultipleSelections, false);
            Assert.AreEqual(result[1].Attributes.Count, 1, "Number of attributes for attributeType not correct");
        }

        [Test]
        public void ShouldCreateAttributes()
        {
            List<MpAttribute> inputList = new List<MpAttribute>()
            {
                new MpAttribute()
                {
                    AttributeId = 0,
                    Name = "Existing Attribute 1",
                    CategoryId = 2,
                    Category = "Category #1",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false
                },
                new MpAttribute()
                {
                    AttributeId = 0,
                    Name = "New Attribute 1",
                    CategoryId = 2,
                    Category = "Category #1",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false
                },
                new MpAttribute()
                {
                    AttributeId = 0,
                    Name = "Existing Attribute 2",
                    CategoryId = 3,
                    Category = "Category #2",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false
                }
            };

            _mpApiUserRepository.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns("yeah!");
            _mpRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_mpRestRepository.Object);
            _mpRestRepository.Setup(mocked => mocked.Search<MpRestAttribute>(It.IsAny<string>(), It.IsAny<string>(), (string)null, false)).Returns(
                new List<MpRestAttribute>()
                {
                    new MpRestAttribute()
                    {
                        AttributeId = 1,
                        Name = "Existing Attribute 1",
                        CategoryId = 2,
                        AttributeTypeId = 3,
                    }, 
                    new MpRestAttribute()
                    {
                        AttributeId = 2,
                        Name = "Existing Attribute 2",
                        CategoryId = 3,
                        AttributeTypeId = 3
                    }
                });
            _mpAttributeRepository.Setup(mocked => mocked.CreateAttribute(It.IsAny<MpAttribute>())).Returns(3);

            var result = _fixture.CreateMissingAttributes(inputList, 3);
            _mpAttributeRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 3, "Result count should match what was passed in");
            Assert.AreEqual(result.Count(attribute => attribute.AttributeId == 0), 0, "All attributes should have an id that does not equal zero");

        }

        private List<MpAttribute> GetAttributesResults()
        {
            return new List<MpAttribute>
            {
                new MpAttribute()
                {
                    AttributeId = 1,
                    Name=  "Attribute #1",
                    CategoryId = 2,
                    Category = "Category #1",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false
                },
                new MpAttribute()
                {
                    AttributeId = 4,
                    Name=  "Attribute #2",
                    CategoryId = 5,
                    Category = "Category #2",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false
                },
                new MpAttribute()
                {
                    AttributeId = 7,
                    Name=  "Attribute #3",
                    CategoryId = null,
                    Category = null,
                    AttributeTypeId = 9,
                    AttributeTypeName = "AttributeType #2",
                    PreventMultipleSelection = true
                }
            };
        }
    }
}