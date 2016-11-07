﻿using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
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

        private readonly string _tokenValue = "ABC";

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestService = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            var authenticateResults =
                new Dictionary<string, object>()
                {
                    {"token", _tokenValue},
                    {"exp", "123"}
                };
            _authService.Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(authenticateResults);
            _ministryPlatformRestService.Setup(mocked => mocked.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _fixture = new AttributeRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _ministryPlatformRestService.Object);
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
                        It.Is<string>(m => m.Equals("Attribute_ID, Attribute_Name, Attribute_Category_ID_Table.Attribute_Category, Attributes.Attribute_Category_ID, Attribute_Category_ID_Table.Description as Attribute_Category_Description, Attributes.Sort_Order, Attribute_Type_ID_Table.Prevent_Multiple_Selection")),
                             (string)null,
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
                        It.Is<string>(m => m.Equals("Attribute_ID, Attribute_Name, Attribute_Category_ID_Table.Attribute_Category, Attributes.Attribute_Category_ID, Attribute_Category_ID_Table.Description as Attribute_Category_Description, Attributes.Sort_Order, Attribute_Type_ID_Table.Prevent_Multiple_Selection")),
                             (string)null,
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
    }
}