using System.Collections.Generic;
using crds_angular.Services;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class StaffContactServiceTest
    {
        private Mock<IContactRepository> _contactRepository;
        private StaffContactService _fixture;

        [SetUp]
        public void SetUp()
        {
            _contactRepository = new Mock<IContactRepository>();
            _fixture = new StaffContactService(_contactRepository.Object);
        }

        [Test]
        public void ShouldMapProperlyFromDictionary()
        {
            var returnData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Contact_ID", 1},
                    {"Display_Name", "Nukem, Duke"},
                    {"Email_Address", "Duke.Nukem@compuserv.net"},
                    {"First_Name", "Duke"},
                    {"Last_Name", "Nukem" },
                    {"Extra_Data", 3 }
                } ,
                new Dictionary<string, object> {
                    {"Contact_ID", 2 },
                    {"Display_Name", "Croft, Lara"},
                    {"First_Name", "Lara"},
                    {"Last_Name", "Croft" },
                    {"Email_Address", "Lara.Croft@gmail.com"}
                }
            };

            _contactRepository.Setup(m => m.PrimaryContacts(true)).Returns(returnData);

            var result = _fixture.GetStaffContacts();
            _contactRepository.VerifyAll();
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0].ContactId, 1);
            Assert.AreEqual(result[1].DisplayName, "Croft, Lara");

        }
    }
}
