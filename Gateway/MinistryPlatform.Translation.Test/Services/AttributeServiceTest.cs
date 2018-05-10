using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class AttributeServiceTest
    {
        private AttributeRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;

        private readonly string _tokenValue = "ABC";

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestService = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();

            var authenticateResults =
                new AuthToken
                {
                    AccessToken = _tokenValue,
                    ExpiresIn = 123
                };
            _authService.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(authenticateResults);

            _ministryPlatformRestService.Setup(mocked => mocked.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _fixture = new AttributeRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _apiUserService.Object, _ministryPlatformRestService.Object);
        }

        [Test]
        public void Given_An_AttributeTypeId_When_Queried_It_Should_Filter_Records_And_Return_Records()
        {
            int? attributeTypeId = 123456;
            var response = GetMpAttributeResponse();
            _ministryPlatformRestService.Setup(
                mocked =>
                    mocked.Search<MpAttribute>(
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    $"Attributes.Attribute_Type_ID = {attributeTypeId}  AND (Attributes.Start_Date Is Null OR Attributes.Start_Date <= GetDate()) AND (Attributes.End_Date Is Null OR Attributes.End_Date >= GetDate())")),
                        It.Is<string>(m => m.Equals("Attribute_ID, Attribute_Name, Attributes.Description, Attribute_Category_ID_Table.Attribute_Category, Attributes.Attribute_Category_ID, Attribute_Category_ID_Table.Description as Attribute_Category_Description, Attributes.Sort_Order, Attribute_Type_ID_Table.Attribute_Type_ID, Attribute_Type_ID_Table.Attribute_Type, Attribute_Type_ID_Table.Prevent_Multiple_Selection, Start_Date, End_Date")),
                             It.IsAny<string>(),
                             It.IsAny<bool>())).Returns(response);

            var attributes = _fixture.GetAttributes(attributeTypeId);

            _ministryPlatformRestService.VerifyAll();

            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Count());

            var attribute = attributes[0];
            Assert.AreEqual(1, attribute.AttributeId);
            Assert.AreEqual("Attribute #1", attribute.Name);
            Assert.AreEqual(2, attribute.CategoryId);
            Assert.AreEqual("Category #1", attribute.Category);
            Assert.AreEqual(3, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #1", attribute.AttributeTypeName);

            attribute = attributes[1];
            Assert.AreEqual(4, attribute.AttributeId);
            Assert.AreEqual("Attribute #2", attribute.Name);
            Assert.AreEqual(5, attribute.CategoryId);
            Assert.AreEqual("Category #2", attribute.Category);
            Assert.AreEqual(6, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #1", attribute.AttributeTypeName);

            attribute = attributes[2];
            Assert.AreEqual(7, attribute.AttributeId);
            Assert.AreEqual("Attribute #3", attribute.Name);
            Assert.AreEqual(null, attribute.CategoryId);
            Assert.AreEqual(null, attribute.Category);
            Assert.AreEqual(9, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #2", attribute.AttributeTypeName);
        }

        [Test]
        public void Given_An_AttributeTypeId_GetAttributeCategory_Returns_Distinct_Attribute_Categories_In_Sorted_Order()
        {
            int attributeTypeId = 123456;
            var response = GetMpAttributeCategoryResponse();
            string catCols = "Attribute_Category_ID_table.*";
            string catSearch = $"attribute_type_id = {attributeTypeId}";
            var orderBy = "Attribute_Category_ID_table.Sort_Order";
            bool distinct = true;

            _ministryPlatformRestService.Setup(
                mocked =>
                    mocked.Search<MpAttribute, MpAttributeCategory>(
                        It.Is<string>(m => m.Equals(catSearch)),
                        It.Is<string>(m => m.Equals(catCols)),
                             It.Is<string>(m=>m.Equals(orderBy)),
                             It.Is<bool>(m=>m.Equals(distinct)))).Returns(response);

            var attributeCategories = _fixture.GetAttributeCategory(attributeTypeId);

            _ministryPlatformRestService.VerifyAll();

            Assert.IsNotNull(attributeCategories);
            Assert.AreEqual(5, attributeCategories.Count());
        }

        [Test]
        public void Given_An_Null_AttributeTypeId_When_Queried_It_Should_Not_Filter_Records_And_Return_Records()
        {
            var response = GetMpAttributeResponse();
            _ministryPlatformRestService.Setup(
                mocked =>
                    mocked.Search<MpAttribute>(
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    "(Attributes.Start_Date Is Null OR Attributes.Start_Date <= GetDate()) AND (Attributes.End_Date Is Null OR Attributes.End_Date >= GetDate())")),
                        It.Is<string>(m => m.Equals("Attribute_ID, Attribute_Name, Attributes.Description, Attribute_Category_ID_Table.Attribute_Category, Attributes.Attribute_Category_ID, Attribute_Category_ID_Table.Description as Attribute_Category_Description, Attributes.Sort_Order, Attribute_Type_ID_Table.Attribute_Type_ID, Attribute_Type_ID_Table.Attribute_Type, Attribute_Type_ID_Table.Prevent_Multiple_Selection, Start_Date, End_Date")),
                             It.IsAny<string>(),
                             It.IsAny<bool>())).Returns(response);

            var attributes = _fixture.GetAttributes(null);

            _ministryPlatformRestService.VerifyAll();

            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Count());

            var attribute = attributes[0];
            Assert.AreEqual(1, attribute.AttributeId);
            Assert.AreEqual("Attribute #1", attribute.Name);
            Assert.AreEqual(2, attribute.CategoryId);
            Assert.AreEqual("Category #1", attribute.Category);
            Assert.AreEqual(3, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #1", attribute.AttributeTypeName);
            Assert.AreEqual(false, attribute.PreventMultipleSelection);

            attribute = attributes[1];
            Assert.AreEqual(4, attribute.AttributeId);
            Assert.AreEqual("Attribute #2", attribute.Name);
            Assert.AreEqual(5, attribute.CategoryId);
            Assert.AreEqual("Category #2", attribute.Category);
            Assert.AreEqual(6, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #1", attribute.AttributeTypeName);
            Assert.AreEqual(false, attribute.PreventMultipleSelection);

            attribute = attributes[2];
            Assert.AreEqual(7, attribute.AttributeId);
            Assert.AreEqual("Attribute #3", attribute.Name);
            Assert.AreEqual(null, attribute.CategoryId);
            Assert.AreEqual(null, attribute.Category);
            Assert.AreEqual(9, attribute.AttributeTypeId);
            Assert.AreEqual("AttributeType #2", attribute.AttributeTypeName);
            Assert.AreEqual(true, attribute.PreventMultipleSelection);
        }

        private static List<MpAttribute> GetMpAttributeResponse()
        {
            return new List<MpAttribute>
            {
                new MpAttribute()
                {
                    AttributeId = 1,
                    Name ="Attribute #1",
                    Description = "Attribute Description #1",
                    CategoryId = 2,
                    Category = "Category #1",
                    CategoryDescription = "Category Description #1",
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false,
                    SortOrder = 0
                },
                new MpAttribute()
                {
                    AttributeId = 4,
                    Name ="Attribute #2",
                    Description = "Attribute Description #2",
                    CategoryId = 5,
                    Category = "Category #2",
                    CategoryDescription = "Category Description #2",
                    AttributeTypeId = 6,
                    AttributeTypeName = "AttributeType #1",
                    PreventMultipleSelection = false,
                    SortOrder = 0
                },
                new MpAttribute()
                {
                    AttributeId = 7,
                    Name ="Attribute #3",
                    Description = "Attribute Description #3",
                    CategoryId = null,
                    Category = null,
                    CategoryDescription = null,
                    AttributeTypeId = 9,
                    AttributeTypeName = "AttributeType #2",
                    PreventMultipleSelection = true,
                    SortOrder = 0
                }
            };
        }

        private static List<MpAttributeCategory> GetMpAttributeCategoryResponse()
        {
            return new List<MpAttributeCategory>
            {
                new MpAttributeCategory()
                {
                  Attribute_Category_ID = 1,
                  Attribute_Category = "Category 1",
                  Description = "Attribute Category Description #1",
                  Sort_Order = null
                },
                new MpAttributeCategory()
                {
                    Attribute_Category_ID = 2,
                    Attribute_Category = "Category 2",
                    Description = "Attribute Category Description #2",
                    Sort_Order = 0
                },
                new MpAttributeCategory()
                {
                    Attribute_Category_ID = 3,
                    Attribute_Category = "Category 3",
                    Description = "Attribute Category Description #3",
                    Sort_Order = 1
                },
                new MpAttributeCategory()
                {
                    Attribute_Category_ID = 4,
                    Attribute_Category = "Category 4",
                    Description = "Attribute Category Description #4",
                    Sort_Order = 2
                },
                new MpAttributeCategory()
                {
                    Attribute_Category_ID = 5,
                    Attribute_Category = "Category 5",
                    Description = "Attribute Category Description #5",
                    Sort_Order = 3
                }
            };
        }
    }
}