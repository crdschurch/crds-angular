using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.GoVolunteer;
using FsCheck;
using MinistryPlatform.Translation.Models;

namespace crds_angular.test
{
    public static class TestHelpers
    {
        public static int RandomInt()
        {
            return Gen.Sample(10000, 10000, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault;
        }

        public static List<MpGoVolunteerSkill> MPSkills(int size = 10)
        {
            return Enumerable.Repeat<MpGoVolunteerSkill>(new MpGoVolunteerSkill(
                Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault,
                Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<string>())).HeadOrDefault,
                Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<string>())).HeadOrDefault,
                Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault), size).ToList();        
        }

        public static List<GoSkills> ListOfGoSkills(int size = 10)
        {
            return Enumerable.Range(0, size).Select((curr) =>
                new GoSkills(
                    curr + 1,
                    curr + 1,
                    $"{curr +1}",
                    $"{curr + 1}",
                    true
                    )
            ).ToList();                                          
        }

        public static MpMyContact MyContact(int contactId = 0)
        {
            var contact = new MpMyContact()
            {
                Address_ID = 12,
                Address_Line_1 = "123 Sesme Street",
                Age = 23,
                City = "Cincinnati",
                Contact_ID = 123445,
                County = "USA"
            };
            if (contactId != 0)
            {
                contact.Contact_ID = contactId;
            }
            return contact;            
        }
    }
}
