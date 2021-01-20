using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Utilities.Interfaces;
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
    public class GroupRepositoryTest
    {
        private GroupRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IContactRepository> _contactService;
        private Mock<IContentBlockService> _contentBlockService;
        private Mock<IAddressRepository> _addressRepository;
        private Mock<IObjectAttributeRepository> _objectAttributeRepository;
        private Mock<IApiUserRepository> _apiUserService;
        private readonly int _groupsParticipantsPageId = 298;
        private readonly int _groupsParticipantsSubPage = 88;
        private readonly int _groupsPageId = 322;
        private readonly int _groupsSubGroupsPageId = 299;
        private readonly int _myGroupParticipationPageId = 563;
        private const string ApiToken = "ABC";
        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestService = new Mock<IMinistryPlatformRestRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _authService = new Mock<IAuthenticationRepository>();
            _communicationService = new Mock<ICommunicationRepository>();
            _contactService = new Mock<IContactRepository>();
            _contentBlockService = new Mock<IContentBlockService>();
            _addressRepository = new Mock<IAddressRepository>();
            _objectAttributeRepository = new Mock<IObjectAttributeRepository>();
            _apiUserService = new Mock<IApiUserRepository>();
        _fixture = new GroupRepository(_ministryPlatformService.Object, _ministryPlatformRestService.Object, _configWrapper.Object, _authService.Object, _communicationService.Object, _contactService.Object, _contentBlockService.Object, _addressRepository.Object, _objectAttributeRepository.Object, _apiUserService.Object
            );

            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(AuthenticateResponse());
        }

        private AuthToken AuthenticateResponse()
        {
            return new AuthToken
            {
                AccessToken = ApiToken,
                ExpiresIn = 123
            };
        }

        [Test]
        public void TestUpdateGroupInquiry()
        {
            const int groupId = 123;
            const int inquiryId = 456;
            const bool approved = true;
            const int groupInquiriesSubPage = 789;

            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("GroupInquiresSubPage")).Returns(groupInquiriesSubPage);
            _ministryPlatformService.Setup(
                mocked =>
                    mocked.UpdateSubRecord(groupInquiriesSubPage,
                                           It.Is<Dictionary<string, object>>(
                                               d => d["Group_Inquiry_ID"].Equals(inquiryId) && d["Placed"].Equals(approved) && d["Group_ID"].Equals(groupId)),
                                           It.IsAny<string>())).Verifiable();
            _fixture.UpdateGroupInquiry(groupId, inquiryId, approved);
            _configWrapper.VerifyAll();
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestAddParticipantToGroup()
        {

            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(1);

            var participant = new MpGroupParticipant()
            {
                ParticipantId = 123,
                GroupParticipantId = 2,
                GroupRoleId = 789,
                StartDate = startDate,
                EndDate = endDate,
                EmployeeRole = false,
                ChildCareRequested = false,
                EnrolledBy = null
            };

            var participants = new List<MpGroupParticipant>{participant};

            _ministryPlatformRestService.Setup(mocked => mocked.PostWithReturn<MpGroupParticipant, MpGroupParticipant>(It.IsAny<List<MpGroupParticipant>>() )).Returns(participants);
            _ministryPlatformRestService.Setup(mocked => mocked.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            var groupParticipantId = _fixture.AddParticipantToGroup(123, 456, 789, false, false, startDate, endDate);
            Assert.AreEqual(2, groupParticipantId);
        }

        [Test]
        public void TestGetGroupsForParticipantByTypeOrID()
        {
            const int participantId = 42;
            const string token = "ABC";
            int[] groupTypeIds = {1, 8};

            List<MpGroup> groups = new List<MpGroup>()
            {
                new MpGroup()
                {
                    AvailableOnline = true,
                    Name = "Group With address",
                    GroupId = 2,
                    GroupRoleId = 22,
                    GroupDescription = "GroupyMcGroupFace",
                    GroupType = 1,
                    GroupTypeName = "Small Group",
                    OffsiteMeetingAddressId = 33
                }, 
                new MpGroup()
                {
                    AvailableOnline = false,
                    Name = "Group without an address",
                    GroupId = 3,
                    GroupDescription = "No address boo",
                    GroupType = 8,
                    GroupRoleId = 16,
                    GroupTypeName = "Onsite Group",
                    Address = new MpAddress()
                    {
                    Address_ID = 2,
                    Address_Line_1 = "123 Street",
                    Address_Line_2 = null,
                    City = "City!",
                    County = "Butler",
                    Foreign_Country = "Merica",
                    Latitude = null,
                    Longitude = null,
                    Postal_Code = "12345",
                    State = "OH"
                    }
                }
            };

            _ministryPlatformRestService.Setup(mocked => mocked.Search<MpGroupParticipant, MpGroup>(It.IsAny<string>(), It.IsAny<string>(), (string) null, false)).Returns(groups);
            _ministryPlatformRestService.Setup(mocked => mocked.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            var result = _fixture.GetGroupsForParticipantByTypeOrID(participantId, token, groupTypeIds);

            Assert.AreEqual(result.Count, 2);

            _ministryPlatformService.VerifyAll();
        }


        [Test]
        public void TestGetAllEventsForGroupNoGroupFound()
        {
            const int groupId = 987654;

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.GetFromStoredProc<MpEvent>("api_crds_Get_Events_For_Group", It.IsAny<Dictionary<string, object>>())).Returns((List<List<MpEvent>>)null);

            var groupEvents = _fixture.getAllEventsForGroup(groupId);
            Assert.IsNull(groupEvents);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void TestGetAllEventsForGroup()
        {
            const int groupId = 987654;

            var mockEvent1 = new MpEvent()
            {
                EventId = 123,
                Congregation = "Katrina's House",
                EventStartDate = new DateTime(2014, 3, 4),
                EventTitle = "Katrina's House Party",
                EventEndDate = new DateTime(2014, 4, 4),
                EventType = "Childcare"
            };
            var mockEvent2 = new MpEvent()
            {
                EventId = 456,
                Congregation = "Andy's House",
                EventStartDate = new DateTime(2014, 4, 4),
                EventTitle = "Andy's House Party",
                EventEndDate = new DateTime(2014, 4, 4),
                EventType = "Childcare"
            };
            var mockResultSet = new List<List<MpEvent>>()
            {
                new List<MpEvent> {mockEvent1, mockEvent2}
            };

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.GetFromStoredProc<MpEvent>("api_crds_Get_Events_For_Group", It.IsAny<Dictionary<string, object>>())).Returns(mockResultSet);

            var events = _fixture.getAllEventsForGroup(groupId);
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(events);
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(123, events[0].EventId);
            Assert.AreEqual(456, events[1].EventId);
        }

        [Test]
        public void TestGetGroupDetails()
        {
            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            var getGroupPageResponse = new Dictionary<string, object>
            {
                {"Group_ID", 456},
                {"Group_Name", "Test Group"},
                {"Target_Size", (short) 5},
                {"Group_Is_Full", true},
                {"Enable_Waiting_List", true},
                {"dp_RecordID", 522},
                {"Start_Date", new DateTime(2014, 3, 4) }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetRecordDict(_groupsPageId, 456, It.IsAny<string>(), false))
                .Returns(getGroupPageResponse);

            var groupParticipantsPageResponse = new List<MpGroupParticipant>();
            for (int i = 42; i <= 46; i++)
            {
                groupParticipantsPageResponse.Add(new MpGroupParticipant()
                {
                    GroupParticipantId = 23434234,
                    ParticipantId =  i,
                    ContactId = i + 10,
                    GroupRoleId = 42,
                    GroupRoleTitle = "Boss",
                    IsApprovedSmallGroupLeader = true,
                    LastName = "Anderson",
                    NickName = "Neo",
                    Email = "Neo@fun.com",
                    StartDate =  new DateTime(2014, 3, 4)
                });
            }

            _ministryPlatformRestService.Setup(mocked => mocked.Search<MpGroupParticipant>(It.IsAny<string>(), It.IsAny<string>(), (string) null, false))
                                                   .Returns(groupParticipantsPageResponse);

            var groupsSubGroupsPageResponse = new List<Dictionary<string, object>>();
            groupsSubGroupsPageResponse.Add(new Dictionary<string, object>()
            {
                {"Group_Name", "Test Wait List"},
                {"Group_Type", "Wait List"},
                {"Group_Type_ID", "20"},
                {"dp_RecordID", 320}
            });

            _ministryPlatformService.Setup(
                mocked => mocked.GetSubPageRecords(_groupsSubGroupsPageId, 456, It.IsAny<string>()))
                .Returns(groupsSubGroupsPageResponse);

            var g = _fixture.getGroupDetails(456);

            _ministryPlatformService.VerifyAll();

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
        public void TestIsUserInGroup()
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
            var result = _fixture.checkIfUserInGroup(participantId, groupParticipants);
            Assert.AreEqual(result, true);
        }

        [Test]
        public void EndDateGroup()
        {
            const int groupId = 1;
            const int reasonEndedId = 1;
            DateTime endDate = DateTime.Now;

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _ministryPlatformRestService.Setup(
                m => m.UpdateRecord("Groups", groupId, It.Is<Dictionary<string, object>>(
                                               d => d["Group_ID"].Equals(groupId) && d["End_Date"].Equals(endDate) && d["Reason_Ended"].Equals(reasonEndedId)))).Verifiable();

            _fixture.EndDateGroup(groupId, endDate, reasonEndedId);

            _ministryPlatformRestService.VerifyAll();
        }

        public void EndDateGroupWithoutDate()
        {
            const int groupId = 1;
            const int reasonEndedId = 1;

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _ministryPlatformRestService.Setup(
                m => m.UpdateRecord("Groups", groupId, It.Is<Dictionary<string, object>>(
                                               d => d["Group_ID"].Equals(groupId) && d["Reason_Ended"].Equals(reasonEndedId)))).Verifiable();

            _fixture.EndDateGroup(groupId, reasonEndedId:reasonEndedId);

            _ministryPlatformRestService.VerifyAll();
        }

        [Test]
        public void EndDateGroupWithoutReason()
        {
            const int groupId = 1;
            DateTime endDate = DateTime.Now;

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _ministryPlatformRestService.Setup(
                m => m.UpdateRecord("Groups", groupId, It.Is<Dictionary<string, object>>(
                                               d => d["Group_ID"].Equals(groupId) && d["End_Date"].Equals(endDate)))).Verifiable();

            _fixture.EndDateGroup(groupId, endDate:endDate);

            _ministryPlatformRestService.VerifyAll();
        }

        [Test]
        public void EndDateGroupWithoutReasonOrDate()
        {
            const int groupId = 1;

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);

            _ministryPlatformRestService.Setup(
                m => m.UpdateRecord("Groups", groupId, It.Is<Dictionary<string, object>>(
                                               d => d["Group_ID"].Equals(groupId)))).Verifiable();

            _fixture.EndDateGroup(groupId);

            _ministryPlatformRestService.VerifyAll();
        }

        [Test]
        public void ParticipantIsGroupMember()
        {
            const int groupId = 1;
            const int participantId = 1000;

            var mockResponse = new List<Dictionary<string, object>> {new Dictionary<string, object>() {{"field1", 7}}};
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 0)).Returns(mockResponse);

            var result = _fixture.ParticipantQualifiedServerGroupMember(groupId, participantId);
            Assert.AreEqual(result, true);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ParticipantIsNotGroupMember()
        {
            const int groupId = 2;
            const int participantId = 2000;

            var mockResponse = new List<Dictionary<string, object>>();
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 0)).Returns(mockResponse);

            var result = _fixture.ParticipantQualifiedServerGroupMember(groupId, participantId);
            Assert.AreEqual(result, false);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void GroupsByEventIdNoRecords()
        {
            //Arrange
            const int eventId = 123456;
            const int pageViewId = 999;
            var searchString = string.Format("\"{0}\",", eventId);

            var mpResponse = new List<Dictionary<string, object>>();

            _configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageViewId);

            _ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(mpResponse);

            //Act
            var groups = _fixture.GetGroupsForEvent(eventId);

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

            _configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageViewId);
            _ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(GroupsByEventId_MpResponse());

            //Act
            var groups = _fixture.GetGroupsForEvent(eventId);

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
            const int pageViewId = 2206;
            const string token = "jenny8675309";
            const int participantId = 9876;
            const int groupTypeId = 19;
            string searchString = ",," + groupTypeId;

            _configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageViewId);

            _ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
               .Returns(MockMyGroups());

            var myGroups = _fixture.GetGroupsByTypeForParticipant(participantId, groupTypeId);

            Assert.IsNotNull(myGroups);
            Assert.AreEqual(2, myGroups.Count);
            Assert.AreEqual("Full Throttle", myGroups[0].Name);
            Assert.AreEqual("Angels Unite", myGroups[1].Name);
        }

        [Test]
        public void GetMyGroupsByType()
        {
            const int pageId = 563;
            const string token = "jenny8675309";
            int[] groupTypeId =  {19};
            string searchString = ",,,,\"" + string.Join("\" or \"", groupTypeId) + "\"";

            _configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageId);

            _ministryPlatformService.Setup(m => m.GetRecordsDict(pageId, It.IsAny<string>(), searchString, It.IsAny<string>()))
                .Returns(MockMyGroups());

            var myGroups = _fixture.GetMyGroupParticipationByType(token, groupTypeId);

            Assert.IsNotNull(myGroups);
            Assert.AreEqual(2, myGroups.Count);
            Assert.AreEqual("Full Throttle", myGroups[0].Name);
            Assert.AreEqual("Angels Unite", myGroups[1].Name);
        }

        [Test]
        public void GetGradeGroupForContact()
        {
            const int contactId = 987654;
            const string apiToken = "letmein";
            const int gradeGroupType = 4;
            var participantList = new List<MpGroupParticipant>
            {
                new MpGroupParticipant()
                {
                    GroupName = "5th Graders",
                    GroupId = 1234
                }
            };

            var searchString = $"Participant_ID_Table_Contact_ID_Table.[Contact_ID]='{contactId}' AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID]='{gradeGroupType}' AND Group_Participants.End_Date IS NULL";
            const string selectColumns = "Group_Participant_ID,Group_ID_Table.[Group_Name],Group_ID_Table.[Group_ID]";

            _configWrapper.Setup(m => m.GetConfigIntValue("AgeorGradeGroupType")).Returns(gradeGroupType);

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.Search<MpGroupParticipant>(searchString, selectColumns, null, true)).Returns(participantList);

            Result<MpGroupParticipant> result = _fixture.GetGradeGroupForContact(contactId, apiToken);

            _ministryPlatformRestService.VerifyAll();
            Assert.IsTrue(result.Status);
            Assert.AreSame(participantList.FirstOrDefault(), result.Value);            
        }

        [Test]
        public void GetGradeGroupForContactEmpty()
        {
            const int contactId = 987654;
            const string apiToken = "letmein";
            const int gradeGroupType = 4;
            var participantList = new List<MpGroupParticipant>
            {
                new MpGroupParticipant()
            };

            var searchString = $"Participant_ID_Table_Contact_ID_Table.[Contact_ID]='{contactId}' AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID]='{gradeGroupType}' AND Group_Participants.End_Date IS NULL";
            const string selectColumns = "Group_Participant_ID,Group_ID_Table.[Group_Name],Group_ID_Table.[Group_ID]";

            _configWrapper.Setup(m => m.GetConfigIntValue("AgeorGradeGroupType")).Returns(gradeGroupType);

            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.Search<MpGroupParticipant>(searchString, selectColumns, null, true)).Returns(participantList);

            Result<MpGroupParticipant> result = _fixture.GetGradeGroupForContact(contactId, apiToken);

            _ministryPlatformRestService.VerifyAll();
            Assert.IsFalse(result.Status);
            Assert.IsNull(result.Value);
            Assert.NotNull(result.ErrorMessage);
        }

        [Test]
        public void GetMyGroupsByTypeAndGroupId()
        {
            const int groupId = 987;
            const int pageId = 563;
            const string token = "jenny8675309";
            int[] groupTypeId = {19};
            string searchString = string.Format(",,,\"{0}\",\"{1}\"", groupId, string.Join("\" or \"", groupTypeId));

            _configWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(pageId);

            _ministryPlatformService.Setup(m => m.GetRecordsDict(pageId, It.IsAny<string>(), searchString, It.IsAny<string>()))
                .Returns(MockMyGroups());

            var myGroups = _fixture.GetMyGroupParticipationByType(token, groupTypeId, groupId);

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

            _configWrapper.Setup(m => m.GetConfigIntValue("CurrentGroupParticipantsByGroupTypePageView")).Returns(pageViewId);

            _ministryPlatformService.Setup(m => m.GetPageViewRecords(pageViewId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(MockMyGroups);

            _fixture.GetGroupsForParticipant(participantId);

            _ministryPlatformService.VerifyAll();

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
                    {"Group_Type_Name", "Small Group" },
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
                    {"Meeting_Time", "06:00:00"},
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
                    {"Group_Type_Name", "Small Group" },
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
                    {"Meeting_Time", "14:00:00"},
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
                MeetingTime = "15:15:00",
                GroupRoleId = 16,
                MinimumAge = 0,
                MinimumParticipants = 8,
                MaximumAge = 99,
                KidsWelcome = false,
                MeetingFrequencyID = null,
                TargetSize = 0,
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
                {"Meeting_Time", "15:15:00" },
                {"Meeting_Day_Id", 2},
                {"Domain_ID", 1 },
                {"Child_Care_Available", false },
                {"Remaining_Capacity", 8 },
                {"Enable_Waiting_List", false },
                {"Online_RSVP_Minimum_Age", 0 },
                {"Maximum_Age", 99 },
                {"Minimum_Participants", 8 },
                {"Kids_Welcome", false },
                {"Meeting_Frequency_ID", null }

            };
           
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(322, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true)).Returns(groupId);

            int resp =  _fixture.CreateGroup(newGroup);

            _ministryPlatformService.Verify(mocked => mocked.CreateRecord(322, values, It.IsAny<string>(), true));
         
            Assert.IsNotNull(resp);  
            Assert.AreEqual(groupId, resp);
        }

        [Test]
        public void GetSmallGroupDetailsByIdTest()
        {
            _ministryPlatformService.Setup(mocked => mocked.GetRecordDict(_groupsPageId, 456, It.IsAny<string>(), false))
                .Returns(MockMyGroups()[0]);

            var resp = _fixture.GetSmallGroupDetailsById(456);
            _ministryPlatformService.VerifyAll();

            Assert.AreEqual("Full Throttle", resp.Name);
        }

        [Test]
        public void UpdateGroupParticipantTest()
        {
            List<MpGroupParticipant> participants = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ParticipantId = 1,
                    GroupParticipantId = 2,
                    GroupRoleId = 22,
                    StartDate = DateTime.Now
                }
            };

            _ministryPlatformService.Setup(
                mocked =>
                    mocked.UpdateSubRecord(_groupsParticipantsPageId,
                                           It.Is<Dictionary<string, object>>(
                                               d => d["Participant_ID"].Equals(participants[0].ParticipantId) && d["Group_Participant_ID"].Equals(participants[0].GroupParticipantId) && d["Group_Role_ID"].Equals(participants[0].GroupRoleId) && d["Start_Date"].Equals(participants[0].StartDate)),
                                           It.IsAny<string>())).Verifiable();

            
            var resp = _fixture.UpdateGroupParticipant(participants);

            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(1, resp);

        }

        [Test]
        public void TestUpdateGroup()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);
            const int groupId = 854725;

            var existingGroup = new MpGroup()
            {
                Name = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupId = groupId,
                GroupType = 1,
                MinistryId = 8,
                ContactId = 74657,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                Full = false,
                TargetSize = 0,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitList = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "15:15:00",
                GroupRoleId = 16,
                MinimumAge = 0,
                MinimumParticipants = 8,
                MaximumAge = 99,
                KidsWelcome = false,
                MeetingFrequencyID = 1,
                Address = new MpAddress()
                {
                    Address_ID = 43567
                }              
            };

            _ministryPlatformService.Setup(mock => mock.UpdateRecord(_groupsPageId,It.IsAny<Dictionary<string, object>>(), It.IsAny<string>())).Verifiable();

            var result = _fixture.UpdateGroup(existingGroup);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetAuthenticatedUserParticipationByGroupIdFindsGroup()
        {
            var values = new Dictionary<string, object>()
            {
                {"Participant_ID", 123},
                {"Group_Participant_ID", 555 },
                {"Group_Role_ID", 978 },
                {"Something_Else", "A string" }
            };

            const int groupId = 43221;

            _ministryPlatformService.Setup(mock => mock.GetRecordsDict(_myGroupParticipationPageId, "abc", $",,,{groupId}", It.IsAny<string>()))
                .Returns(new List<Dictionary<string, object>>() {values});

            var result = _fixture.GetAuthenticatedUserParticipationByGroupID("abc", groupId);

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.ParticipantId, 123);
            
        }

        [Test]
        public void GetAuthenticatedUserParticipationNoGroupsFound()
        {
            const int groupId = 123;

            _ministryPlatformService.Setup(mock => mock.GetRecordsDict(_myGroupParticipationPageId, "abc", $",,,{groupId}", It.IsAny<string>()))
                .Returns(new List<Dictionary<string, object>>());

            var result = _fixture.GetAuthenticatedUserParticipationByGroupID("abc", groupId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNull(result);
        }

        [Test]
        public void SetMeetingFrequencyForNonInvite()
        {
            var record = getTestGroupRecord();
            record.Add("Meeting_Frequency",  "Weekly");
            var group = _fixture.MapRecordToMpGroup(record);
            Assert.AreEqual("Weekly", group.MeetingFrequency);
        }

        [Test]
        public void SetMeetingFrequencyForInvite()
        {
            var record = getTestGroupRecord();
            record.Add("Meeting_Frequency_ID_Text", "Weekly");
            var group = _fixture.MapRecordToMpGroup(record);
            Assert.AreEqual("Weekly", group.MeetingFrequency);
        }

        [Test]
        public void MeetingFrequencyMissingReturnsEmpty()
        {
            var record = getTestGroupRecord();
            var group = _fixture.MapRecordToMpGroup(record);
            Assert.AreEqual(string.Empty, group.MeetingFrequency);
        }

        [Test]
        public void IsMemberOfGradeGroup()
        {
            const string api_token = "asdfasdfasdf";
            const string storedProcName = "storedProcName";
            const int contactId = 12345;
            const int eventId = 54321;

            var storedProcParams = new Dictionary<string, object>
            {
                {"@ContactId", contactId},
                {"@EventID", eventId }
            };

            var result = new List<List<MpStoredProcBool>>
            {
                new List<MpStoredProcBool>
                {
                    new MpStoredProcBool()
                    {
                        isTrue = true
                    }
                }
            };
            _configWrapper.Setup(m => m.GetConfigValue("IsCampEligibleStoredProc")).Returns(storedProcName);
            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.GetFromStoredProc<MpStoredProcBool>(storedProcName, storedProcParams)).Returns(result);
            var actual = _fixture.IsMemberOfEventGroup(contactId, eventId, api_token);
            Assert.IsTrue(actual);
            _ministryPlatformRestService.VerifyAll();           
        }

        [Test]
        public void IsNotMemberOfGradeGroup()
        {
            const string api_token = "asdfasdfasdf";
            const string storedProcName = "storedProcName";
            const int contactId = 12345;
            const int eventId = 543321;

            var storedProcParams = new Dictionary<string, object>
            {
                {"@ContactId", contactId},
                {"@EventID", eventId }
            };

            var result = new List<List<MpStoredProcBool>>
            {
                new List<MpStoredProcBool>
                {
                    new MpStoredProcBool()
                    {
                        isTrue = false
                    }
                }
            };
            _configWrapper.Setup(m => m.GetConfigValue("IsCampEligibleStoredProc")).Returns(storedProcName);
            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.GetFromStoredProc<MpStoredProcBool>(storedProcName, storedProcParams)).Returns(result);
            var actual = _fixture.IsMemberOfEventGroup(contactId, eventId, api_token);
            Assert.IsFalse(actual);
            _ministryPlatformRestService.VerifyAll();            
        }

        [Test]
        public void IsNullMemberOfGradeGroup()
        {
            const string api_token = "asdfasdfasdf";
            const string storedProcName = "storedProcName";
            const int contactId = 12345;
            const int eventId = 5431;

            var storedProcParams = new Dictionary<string, object>
            {
                {"@ContactId", contactId},
                {"@EventID", eventId }
            };

            var result = new List<List<MpStoredProcBool>>();
            _configWrapper.Setup(m => m.GetConfigValue("IsCampEligibleStoredProc")).Returns(storedProcName);
            _ministryPlatformRestService.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestService.Object);
            _ministryPlatformRestService.Setup(m => m.GetFromStoredProc<MpStoredProcBool>(storedProcName, storedProcParams)).Returns(result);
            var actual = _fixture.IsMemberOfEventGroup(contactId, eventId, api_token);
            Assert.IsFalse(actual);
            _ministryPlatformRestService.VerifyAll();
        }

        private Dictionary<string, object> getTestGroupRecord()
        {
            var values = new Dictionary<string, object>
            {
                {"Group_ID", 1 },
                {"Group_Name", "New Testing Group"},
                {"Group_Type_ID", 19 },
                {"Group_Type_Name", "Small Group"},
                {"Ministry_ID", 8 },
                {"Congregation_ID", 1 },
                {"Primary_Contact", 74657},
                {"Description", "The best group ever created for testing stuff and things" },
                {"Start_Date", string.Empty},
                {"End_Date", string.Empty },
                {"Target_Size", 0 },
                {"Offsite_Meeting_Address", 43567 },
                {"Group_Is_Full", false },
                {"Available_Online", true },
                {"Meeting_Time", "15:15:00" },
                {"Meeting_Day_Id", 2},
                {"Domain_ID", 1 },
                {"Child_Care_Available", false },
                {"Remaining_Capacity", 8 },
                {"Enable_Waiting_List", false },
                {"Online_RSVP_Minimum_Age", 0 },
                {"Maximum_Age", 99 },
                {"Minimum_Participants", 8 },
                {"Kids_Welcome", false },
                {"Meeting_Day_ID", 1 },
                {"Primary_Contact_Text", "Text" },
                {"Meeting_Frequency_ID", null }

            };
            return values;
        }
    }
}