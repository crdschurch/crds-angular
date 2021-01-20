using System;
using crds_angular.Models.Crossroads.GoVolunteer;
using MinistryPlatform.Translation.Models;
using NUnit.Framework;

namespace crds_angular.test.Models.Crossroads.GoVolunteer
{
    [TestFixture]
    public class OrganizationTest
    {
        private const int CONTACTID = 123456789;
        private readonly DateTime STARTDATE = new DateTime(2008, 02, 21);
        private const string NAME = "CROSSROADS";
        private const int ORGID = 987654321;

        [Test]
        public void ShouldConvertFromMPOrg()
        {
            var mp = new MPOrganization()
            {
                ContactId = CONTACTID,
                EndDate = new DateTime(),
                StartDate = STARTDATE,
                Name = NAME,
                OpenSignup = false,
                OrganizationId = ORGID
            };
            
            var org = new Organization();
            org = org.FromMpOrganization(mp);
            Assert.AreEqual(mp.OrganizationId, org.OrganizationId);
            Assert.AreEqual(mp.ContactId, org.ContactId);
            Assert.AreEqual(mp.EndDate, org.EndDate);
            Assert.AreEqual(mp.StartDate, org.StartDate);
            Assert.AreEqual(mp.Name, org.Name);
            Assert.AreEqual(mp.OpenSignup, org.OpenSignup);

        }

        [Test]
        public void ShouldConvertToMPOrg()
        {
            var org = new Organization()
            {
                ContactId = CONTACTID,
                EndDate = new DateTime(),
                StartDate = STARTDATE,
                Name = NAME,
                OpenSignup = false,
                OrganizationId = ORGID
            };
            
            var mp = org.ToMpOrganization(org);
            Assert.AreEqual(mp.OrganizationId, org.OrganizationId);
            Assert.AreEqual(mp.ContactId, org.ContactId);
            Assert.AreEqual(mp.EndDate, org.EndDate);
            Assert.AreEqual(mp.StartDate, org.StartDate);
            Assert.AreEqual(mp.Name, org.Name);
            Assert.AreEqual(mp.OpenSignup, org.OpenSignup);

        }
    }
}
