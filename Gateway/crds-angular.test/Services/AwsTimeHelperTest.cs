using crds_angular.Services;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class AwsTimeHelperTest
    {
        private AwsTimeHelper fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new AwsTimeHelper();
        }

        [Test]
        public void ShouldAddLeadingZeroIfMissing()
        {
            string testTime1 = "5:25AM";
            string testTime2 = "11:25AM";
            string testTime3 = "06:25PM";

            string expectedTimeWithLeadingZero1 = "05:25AM";
            string expectedTimeWithLeadingZero2 = "11:25AM";
            string expectedTimeWithLeadingZero3 = "06:25PM";

            string actualTimeWithLeadingZero1 = fixture.AddLeadingZeroIfMissing(testTime1);
            string actualTimeWithLeadingZero2 = fixture.AddLeadingZeroIfMissing(testTime2);
            string actualTimeWithLeadingZero3 = fixture.AddLeadingZeroIfMissing(testTime3);

            Assert.AreEqual(expectedTimeWithLeadingZero1, actualTimeWithLeadingZero1);
            Assert.AreEqual(expectedTimeWithLeadingZero2, actualTimeWithLeadingZero2);
            Assert.AreEqual(expectedTimeWithLeadingZero3, actualTimeWithLeadingZero3);
        }

        [Test]
        public void ShouldConvertToMilitaryTimeHoursCorrectly()
        {
            string testTime1 = "05:25PM";
            string testTime2 = "11:25PM";
            string testTime3 = "06:25PM";

            string expectedMilitaryTimeHours1 = "17";
            string expectedMilitaryTimeHours2 = "23";
            string expectedMilitaryTimeHours3 = "18";

            string actualMilitaryTimeHours1 = fixture.GetPmTimeHours(testTime1);
            string actualMilitaryTimeHours2 = fixture.GetPmTimeHours(testTime2);
            string actualMilitaryTimeHours3 = fixture.GetPmTimeHours(testTime3);

            Assert.AreEqual(expectedMilitaryTimeHours1, actualMilitaryTimeHours1);
            Assert.AreEqual(expectedMilitaryTimeHours2, actualMilitaryTimeHours2);
            Assert.AreEqual(expectedMilitaryTimeHours3, actualMilitaryTimeHours3);
        }

        [Test]
        public void ShouldConvert24To00Correctly()
        {
            string testHours1 = "24";
            string testHours2 = "05";

            string expectedHours1 = "00";
            string expectedHours2 = "05";

            string actualHours1 = fixture.Convert24To00(testHours1);
            string actualHours2 = fixture.Convert24To00(testHours2);

            Assert.AreEqual(expectedHours1, actualHours1);
            Assert.AreEqual(expectedHours2, actualHours2);
        }

        [Test]
        public void ShouldNotAdd12to12pm()
        {
            string testHours1 = "12:15:00.0000000";
            string testHours2 = "12:30 PM";
            string testHours3 = "12:45 AM";

            string actualHours1 = fixture.ConvertTimeToUtcString(testHours1);
            string actualHours2 = fixture.ConvertTimeToUtcString(testHours2);
            string actualHours3 = fixture.ConvertTimeToUtcString(testHours3);

            Assert.AreEqual("0001-01-01T12:15:00.000Z", actualHours1);
            Assert.AreEqual("0001-01-01T12:30:00.000Z", actualHours2);
            Assert.AreEqual("0001-01-01T00:45:00.000Z", actualHours3);
        }
        
    }
}