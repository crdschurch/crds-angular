﻿using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Groups;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Services;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using Communication = MinistryPlatform.Translation.Models.MpCommunication;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class GroupRepositoryTest
    {
        private GroupRepository fixture;
        private Mock<IMinistryPlatformService> ministryPlatformService;
        private Mock<IConfigurationWrapper> configWrapper;
        private Mock<IAuthenticationRepository> authService;
        private Mock<ICommunicationRepository> communicationService;
        private Mock<IContactRepository> contactService;
        private Mock<IContentBlockService> contentBlockService;
        private readonly int GroupsParticipantsPageId = 298;
        private readonly int GroupsParticipantsSubPage = 88;
        private readonly int GroupsPageId = 322;
        private readonly int GroupsEventsPageId = 302;
        private readonly int EventsGroupsPageId = 408;
        private readonly int GroupsSubGroupsPageId = 299;

        [SetUp]
        public void SetUp()
        {
            ministryPlatformService = new Mock<IMinistryPlatformService>();
            configWrapper = new Mock<IConfigurationWrapper>();
            authService = new Mock<IAuthenticationRepository>();
            communicationService = new Mock<ICommunicationRepository>();
            contactService = new Mock<IContactRepository>();
            contentBlockService = new Mock<IContentBlockService>();
            fixture = new GroupRepository(ministryPlatformService.Object, configWrapper.Object, authService.Object, communicationService.Object, contactService.Object, contentBlockService.Object);


            configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            authService.Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(AuthenticateResponse());
        }

        private Dictionary<string, object> AuthenticateResponse()
        {
            return new Dictionary<string, object>
            {
                {"token", "ABC"},
                {"exp", "123"}
            };
        }

        [Test]
        public void testAddParticipantToGroup()
        {
            var getGroupPageResponse = new Dictionary<string, object>
            {
                {"Group_ID", 456},
                {"Group_Name", "Test Group"},
                {"Target_Size", (short) 1},
                {"Group_Is_Full", false},
                {"Child_Care_Available", true}
            };

            ministryPlatformService.Setup(mocked => mocked.GetRecordDict(GroupsPageId, 456, It.IsAny<string>(), false))
                .Returns(getGroupPageResponse);

            ministryPlatformService.Setup(
                mocked => mocked.GetSubPageRecords(GroupsParticipantsPageId, 456, It.IsAny<string>()))
                .Returns((List<Dictionary<string, object>>) null);

            ministryPlatformService.Setup(mocked => mocked.CreateSubRecord(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>(),
                true)).Returns(987);

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddDays(1);

            var expectedValues = new Dictionary<string, object>
            {
                {"Participant_ID", 123},
                {"Group_Role_ID", 789},
                {"Start_Date", startDate},
                {"End_Date", endDate},
                {"Employee_Role", true},
                {"Child_Care_Requested", true}
            };

            int groupParticipantId = fixture.addParticipantToGroup(123, 456, 789, true, startDate, endDate, true);

            ministryPlatformService.Verify(mocked => mocked.CreateSubRecord(
                GroupsParticipantsPageId,
                456,
                expectedValues,
                It.IsAny<string>(),
                true));

            Assert.AreEqual(987, groupParticipantId);
        }


        [Test]
        public void TestGetAllEventsForGroupNoGroupFound()
        {
            const string pageKey = "GroupEventsSubPageView";
            const int groupId = 987654;
            const string token = "ABC";

            ministryPlatformService.Setup(m => m.GetSubpageViewRecords(pageKey, groupId, token, "", "", 0)).Returns((List<Dictionary<string, object>>) null);

            var groupEvents = fixture.getAllEventsForGroup(groupId);
            Assert.IsNull(groupEvents);

            ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestGetAllEventsForGroup()
        {
            const string pageKey = "GroupEventsSubPageView";
            const int groupId = 987654;
            const string token = "ABC";

            var mock1 = new Dictionary<string, object>
            {
                {"Event_ID", 123},
                {"Congregation_Name", "Katrina's House"},
                {"Event_Start_Date", new DateTime(2014, 3, 4)},
                {"Event_Title", "Katrina's House Party"},
                {"Event_End_Date", new DateTime(2014, 4, 4)}
            };
            var mock2 = new Dictionary<string, object>
            {
                {"Event_ID", 456},
                {"Congregation_Name", "Andy's House"},
                {"Event_Start_Date", new DateTime(2014, 4, 4)},
                {"Event_Title", "Andy's House Party"},
                {"Event_End_Date", new DateTime(2014, 4, 4)}
            };
            var mockSubPageView = new List<Dictionary<string, object>> {mock1, mock2};

            ministryPlatformService.Setup(m => m.GetSubpageViewRecords(pageKey, groupId, token, "", "", 0)).Returns(mockSubPageView);

            var events = fixture.getAllEventsForGroup(groupId);
            ministryPlatformService.VerifyAll();

            Assert.IsNotNull(events);
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(123, events[0].EventId);
            Assert.AreEqual(456, events[1].EventId);
        }

        [Test]
        public void TestGetGroupDetails()
        {
            var getGroupPageResponse = new Dictionary<string, object>
            {
                {"Group_ID", 456},
                {"Group_Name", "Test Group"},
                {"Target_Size", (short) 5},
                {"Group_Is_Full", true},
                {"Enable_Waiting_List", true},
                {"dp_RecordID", 522}
            };

            ministryPlatformService.Setup(mocked => mocked.GetRecordDict(GroupsPageId, 456, It.IsAny<string>(), false))
                .Returns(getGroupPageResponse);

            var groupParticipantsPageResponse = new List<Dictionary<string, object>>();
            for (int i = 42; i <= 46; i++)
            {
                groupParticipantsPageResponse.Add(new Dictionary<string, object>()
                {
                    {"dp_RecordID", 23434234 },
                    {"Participant_ID", i},
                    {"Contact_ID", i + 10},
                    {"Group_Role_ID", 42},
                    {"Role_Title", "Boss"},
                    {"Last_Name", "Anderson"},
                    {"Nickname", "Neo"},
                    {"Email", "Neo@fun.com"}

                });
            }
            ministryPlatformService.Setup(
                mocked => mocked.GetSubpageViewRecords(GroupsParticipantsSubPage, 456, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(groupParticipantsPageResponse);

            var GroupsSubGroupsPageResponse = new List<Dictionary<string, object>>();
            GroupsSubGroupsPageResponse.Add(new Dictionary<string, object>()
            {
                {"Group_Name", "Test Wait List"},
                {"Group_Type", "Wait List"},
                {"Group_Type_ID", "20"},
                {"dp_RecordID", 320}
            });

            ministryPlatformService.Setup(
                mocked => mocked.GetSubPageRecords(GroupsSubGroupsPageId, 456, It.IsAny<string>()))
                .Returns(GroupsSubGroupsPageResponse);

            var g = fixture.getGroupDetails(456);

            ministryPlatformService.VerifyAll();

            Assert.NotNull(g);
            Assert.AreEqual(456, g.GroupId);
            Assert.AreEqual(5, g.TargetSize);
            Assert.AreEqual(true, g.Full);
            Assert.AreEqual(true, g.WaitList);
            Assert.AreEqual("Test Group", g.Name);
            Assert.NotNull(g.Participants);
            Assert.AreEqual(5, g.Participants.Count);
            //Assert.AreEqual(42, g.Participants[0]);
            //Assert.AreEqual(43, g.Participants[1]);
            //Assert.AreEqual(44, g.Participants[2]);
            //Assert.AreEqual(45, g.Participants[3]);
            //Assert.AreEqual(46, g.Participants[4]);
            Assert.AreEqual(true, g.WaitList);
            Assert.AreEqual(320, g.WaitListGroupId);
        }

        [Test]
        public void testIsUserInGroup()
        {
            int participantId = 123;
            List<MpGroupParticipant> groupParticipants = new List<MpGroupParticipant>
            {
                new MpGroupParticipant
                {
                    ParticipantId = 1111
                },
                new MpGroupParticipant
                {
                    ParticipantId = 2222
                },
                new MpGroupParticipant
                {
                    ParticipantId = 123
                }
            };
            var result = fixture.checkIfUserInGroup(participantId, groupParticipants);
            Assert.AreEqual(result, true);
        }

        [Test]
        public void ParticipantIsGroupMember()
        {
            const int groupId = 1;
            const int participantId = 1000;

            var mockResponse = new List<Dictionary<string, object>> {new Dictionary<string, object>() {{"field1", 7}}};
            ministryPlatformService.Setup(
                m => m.GetPageViewRecords(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 0)).Returns(mockResponse);

            var result = fixture.ParticipantQualifiedServerGroupMember(groupId, participantId);
            Assert.AreEqual(result, true);

            ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ParticipantIsNotGroupMember()
        {
            const int groupId = 2;
            const int participantId = 2000;

            var mockResponse = new List<Dictionary<string, object>>();
            ministryPlatformService.Setup(
                m => m.GetPageViewRecords(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 0)).Returns(mockResponse);

            var result = fixture.ParticipantQualifiedServerGroupMember(groupId, participantId);
            Assert.AreEqual(result, false);

            ministryPlatformService.VerifyAll();
        }

        [Test]
        public void GroupsByEventIdNoRecords()
        {
            //Arrange
            const int eventId = 123456;
            const int pageViewId = 999;
            var searchString = string.Format("\"{0}\",", eventId);

            var mpResponse = new List<Dictionary<string, object>>();

            configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageViewId);

            ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(mpResponse);

            //Act
            var groups = fixture.GetGroupsForEvent(eventId);

            //Assert
            Assert.IsNotNull(groups);
            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void GroupsByEventId()
        {
            //Arrange
            const int eventId = 123456;
            const int pageViewId = 999;
            var searchString = string.Format("\"{0}\",", eventId);

            configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageViewId);
            ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(GroupsByEventId_MpResponse());

            //Act
            var groups = fixture.GetGroupsForEvent(eventId);

            //Assert
            Assert.IsNotNull(groups);
            Assert.AreEqual(2, groups.Count);

            Assert.AreEqual(1, groups[0].GroupId);
            Assert.AreEqual("group-one", groups[0].Name);

            Assert.AreEqual(2, groups[1].GroupId);
            Assert.AreEqual("group-two", groups[1].Name);
        }

        private List<Dictionary<string, object>> GroupsByEventId_MpResponse()
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    {"Group_ID", 1},
                    {"Group_Name", "group-one"},
                    {"Role_Title", "group-one-role"},
                    {"Primary_Contact", "me@aol.com"}
                },
                new Dictionary<string, object>()
                {
                    {"Group_ID", 2},
                    {"Group_Name", "group-two"},
                    {"Role_Title", "group-two-role"},
                    {"Primary_Contact", "me@aol.com"}
                }
            };
        }

        [Test]
        public void GetGroupsByTypeForParticipant()
        {
            const int pageId = 563;
            const string token = "jenny8675309";
            const int participantId = 9876;
            const int groupTypeId = 19;
            string searchString = ",\"" + participantId + "\",,,\"" + groupTypeId + "\"";

            configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageId);

            ministryPlatformService.Setup(m => m.GetRecordsDict(pageId, It.IsAny<string>(), searchString, It.IsAny<string>()))
                .Returns(MockMyGroups());

            var myGroups = fixture.GetGroupsByTypeForParticipant(token, participantId, groupTypeId);

            Assert.IsNotNull(myGroups);
            Assert.AreEqual(2, myGroups.Count);
            Assert.AreEqual("Full Throttle", myGroups[0].Name);
            Assert.AreEqual("Angels Unite", myGroups[1].Name);
        }

        [Test]
        public void GetGroupsForParticipant()
        {
            const int pageViewId = 2307;
            const string token = "logmein";
            const int participantId = 123456789;        
            string searchString = ",\"" + participantId + "\"";

            configWrapper.Setup(m => m.GetConfigIntValue("CurrentGroupParticipantsByGroupTypePageView")).Returns(pageViewId);

            ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, token, searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(MockMyGroups);

            fixture.GetGroupsForParticipant(token, participantId);

            ministryPlatformService.VerifyAll();

        }

        private List<Dictionary<string, object>> MockMyGroups()
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Group_ID", 2121},
                    {"Congregation_ID", 4},
                    {"Group_Name", "Full Throttle"},
                    {"Group_Role_ID", 16},
                    {"Description", "Not The First"},
                    {"Ministry_ID", 4},
                    {"Primary_Contact", 3213},
                    {"Primary_Contact_Name", "Jim Beam"},
                    {"Primary_Contact_Email", "JimBeam@test.com"},
                    {"Group_Type_ID", 19},
                    {"Start_Date", "2016-02-01"},
                    {"End_Date", "2018-02-11"},
                    {"Meeting_Day_ID", 5},
                    {"Meeting_Day", "Monday" },
                    {"Meeting_Time", "180000"},
                    {"Meeting_Frequency", "Monday's at 6:00 PM, Every Other Week" },
                    {"Available_Online", false},
                    {"Maximum_Age", 10 },
                    {"Remaining_Capacity", 42},
                    {"Address_ID", 42934 },
                    {"Address_Line_1", "98 Center St"},
                    {"Address_Line_2", "Suite 1000"},
                    {"City", "Cincinnati"},
                    {"State", "OH"},
                    {"Zip_Code", "42525"},
                    {"Foreign_Country", "United States"},
                },
                new Dictionary<string, object>
                {
                    {"Group_ID", 54345},
                    {"Congregation_ID", 5},
                    {"Group_Name", "Angels Unite"},
                    {"Group_Role_ID", 15},
                    {"Description", "Girls Rule"},
                    {"Ministry_ID", 6},
                    {"Primary_Contact", 43212},
                    {"Primary_Contact_Name", "Johnny Walker"},
                    {"Primary_Contact_Email", "Johnny Walker@test.com"},
                    {"Group_Type_ID", 19},
                    {"Start_Date", "2016-01-01"},
                    {"End_Date", "2020-01-01"},
                    {"Meeting_Day_ID", 4},
                    {"Meeting_Day", "Wednesday" },
                    {"Meeting_Time", "140000"},
                    {"Meeting_Frequency", "Wednesday's at 2:00 PM, Every Other Week" },
                    {"Available_Online", true},
                    {"Maximum_Age", 10},
                    {"Remaining_Capacity", 42},
                    {"Address_ID", 42934 },
                    {"Address_Line_1", "86 Middle Rd"},
                    {"Address_Line_2", ""},
                    {"City", "Cincinnati"},
                    {"State", "OH"},
                    {"Zip_Code", "45010"},
                    {"Foreign_Country", "United States"},
                }
            };
        }

		[Test]
        public void TestCreateGroup()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);
            const int groupId = 854725;

            var newGroup = new MpGroup()
            {
                Name = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",              
                GroupType = 19,
                MinistryId = 8,
                ContactId = 74657,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                Full = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitList = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16,
                MinimumAge = 0,
                MaximumParticipants = 9,
                MinimumParticipants = 8,
                MaximumAge = 99,
                Address = new MpAddress()
                {
                    Address_ID = 43567
                }
            };
           
            var values = new Dictionary<string, object>
            {
                {"Group_Name", "New Testing Group"},
                {"Group_Type_ID", 19 },
                {"Ministry_ID", 8 },
                {"Congregation_ID", 1 },
                {"Primary_Contact", 74657},
                {"Description", "The best group ever created for testing stuff and things" },
                {"Start_Date", start},
                {"End_Date", end },
                {"Target_Size", 0 },
                {"Offsite_Meeting_Address", 43567 },
                {"Group_Is_Full", false },
                {"Available_Online", true },
                {"Meeting_Time", "18000" },
                {"Meeting_Day_Id", 2},
                {"Domain_ID", 1 },
                {"Child_Care_Available", false },
                {"Remaining_Capacity", 8 },
                {"Enable_Waiting_List", false },
                {"Online_RSVP_Minimum_Age", 0 },
                {"Maximum_Age", 99 },
                {"Minimum_Participants", 8 },
                {"Maximum_Participants", 9 }

            };
           
            ministryPlatformService.Setup(mocked => mocked.CreateRecord(322, It.IsAny<Dictionary<string, object>>(), "ABC", true)).Returns(groupId);

            int resp =  fixture.CreateGroup(newGroup);

            ministryPlatformService.Verify(mocked => mocked.CreateRecord(322, values, "ABC", true));
         
            Assert.IsNotNull(resp);  
            Assert.AreEqual(groupId, resp);
        }        
    }
}