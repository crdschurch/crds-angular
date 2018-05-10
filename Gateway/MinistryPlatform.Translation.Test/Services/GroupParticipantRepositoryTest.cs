using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    public class GroupParticipantRepositoryTest
    {
        private GroupParticipantRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IGroupRepository> _groupRepository;
        private Mock<IGroupParticipantRepository> _groupParticipantRepository;

        private const int GroupLeaderRole = 22;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>(MockBehavior.Strict);
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>(MockBehavior.Strict);
            _apiUserRepository = new Mock<IApiUserRepository>(MockBehavior.Strict);
            _groupRepository = new Mock<IGroupRepository>(MockBehavior.Strict);
            _groupParticipantRepository = new Mock<IGroupParticipantRepository>(MockBehavior.Strict);

            var config = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            var auth = new Mock<IAuthenticationRepository>(MockBehavior.Strict);

            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api_user");
            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            auth.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            config.Setup(mocked => mocked.GetConfigIntValue("GroupRoleLeader")).Returns(GroupLeaderRole);

            _fixture = new GroupParticipantRepository(config.Object,
                                                      _ministryPlatformService.Object,
                                                      _apiUserRepository.Object,
                                                      _ministryPlatformRestRepository.Object,
                                                      _groupRepository.Object);
        }

        [Test]
        public void GetAllGroupNamesLeadByParticipantShouldTransfomGroupParticipantIntoGroupWithGroupTypeId()
        {
            var participantId = 3;
            var groupType = 9;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 31,
                    Email = "a1@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = participantId,
                    GroupParticipantId = 2
                },
                new MpGroupParticipant()
                {
                    ContactId = 32,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = participantId,
                    GroupParticipantId = 3
                }
            };

            List<MpGroup> groups = new List<MpGroup>()
            {
                new MpGroup()
                {
                    GroupId = 1,
                    Name = "group one"
                },
                new MpGroup()
                {
                    GroupId = 2,
                    Name = "group two"
                },
                new MpGroup()
                {
                    GroupId = 3,
                    Name = "group three"
                }
            };

            var search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {GroupLeaderRole}" +
                            $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                            $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)" +
                            $" AND Group_ID_Table.Group_Type_ID = {groupType}";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)), 
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groupParticipantReturn);

            var result = _fixture.GetAllGroupNamesLeadByParticipant(participantId, groupType);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 3, "Result count should be three groups");
            Assert.AreEqual(result[0].GroupId, groups[0].GroupId);
            Assert.AreEqual(result[0].Name, groups[0].Name);
            Assert.AreEqual(result[1].GroupId, groups[1].GroupId);
            Assert.AreEqual(result[1].Name, groups[1].Name);
            Assert.AreEqual(result[2].GroupId, groups[2].GroupId);
            Assert.AreEqual(result[2].Name, groups[2].Name);
        }

        private static bool MatchesDate(string input, string pattern)
        {
            var match = new Regex(pattern).Match(input);
            if (!match.Success || match.Groups.Count != 3)
            {
                return false;
            }

            var refDate = DateTime.Now;

            // First group is the entire matched string, so skip it; second and third groups should be the dates we're looking for
            for (int i = 1; i < 2; i++)
            {
                // Consider it ok if the date/time in the string is within 60 seconds of the reference date
                if (Math.Abs(DateTime.Parse(match.Groups[i].Value).Subtract(refDate).TotalSeconds) > 60)
                {
                    return false;
                }
            }

            return true;
        }

        [Test]
        public void GetAllGroupNamesLeadByParticipantShouldTransfomGroupParticipantIntoGroupWithoutGroupTypeId()
        {
            var participantId = 3;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 31,
                    Email = "a1@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = participantId,
                    GroupParticipantId = 2
                },
                new MpGroupParticipant()
                {
                    ContactId = 32,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = participantId,
                    GroupParticipantId = 3
                }
            };

            List<MpGroup> groups = new List<MpGroup>()
            {
                new MpGroup()
                {
                    GroupId = 1,
                    Name = "group one"
                },
                new MpGroup()
                {
                    GroupId = 2,
                    Name = "group two"
                },
                new MpGroup()
                {
                    GroupId = 3,
                    Name = "group three"
                }
            };
            var search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {GroupLeaderRole}" +
                            $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                            $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)), 
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groupParticipantReturn);

            var result = _fixture.GetAllGroupNamesLeadByParticipant(participantId, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 3, "Result count should be three groups");
            Assert.AreEqual(result[0].GroupId, groups[0].GroupId);
            Assert.AreEqual(result[0].Name, groups[0].Name);
            Assert.AreEqual(result[1].GroupId, groups[1].GroupId);
            Assert.AreEqual(result[1].Name, groups[1].Name);
            Assert.AreEqual(result[2].GroupId, groups[2].GroupId);
            Assert.AreEqual(result[2].Name, groups[2].Name);
        }

        [Test]
        public void GetAllParticipantsForLeaderGroupsSuccessWithParticipantIdAndGroupTypeId()
        {
            var participantId = 3;
            var groupType = 9;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = 1,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 33,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = 2,
                    GroupParticipantId = 2
                },
                new MpGroupParticipant()
                {
                    ContactId = 32,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = 4,
                    GroupParticipantId = 4
                }
            };

            List<MpGroupParticipant> groups = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    GroupId = 1
                },
                new MpGroupParticipant()
                {
                    GroupId = 2
                },
                new MpGroupParticipant()
                {
                    GroupId = 3
                }
            };
            string csvGroupIds = "1,2,3";
            string search = $"group_participants.group_id in ({csvGroupIds})" +
                            $" AND (Group_Participants.End_Date > GetDate() OR Group_Participants.End_Date Is Null)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groups);
            _groupParticipantRepository.Setup(mocked => mocked.GetLeadersGroupIds(participantId, groupType)).Returns(groups);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => searchString.Equals(search)),
                //It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                true))
                .Returns(groupParticipantReturn);


            var result = _fixture.GetAllParticipantsForLeaderGroups(participantId, groupType, null);
            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2, "Result count should be two group participants");
            Assert.AreEqual(result[0].Email, groupParticipantReturn[0].Email);
            Assert.AreEqual(result[1].Email, groupParticipantReturn[1].Email);
        }

        [Test]
        public void GetAllParticipantsForLeaderGroupsSuccessWithParticipantId()
        {
            var participantId = 3;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = 1,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 33,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = 2,
                    GroupParticipantId = 2
                },
                new MpGroupParticipant()
                {
                    ContactId = 32,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = 4,
                    GroupParticipantId = 4
                }
            };

            List<MpGroupParticipant> groups = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    GroupId = 1
                },
                new MpGroupParticipant()
                {
                    GroupId = 2
                },
                new MpGroupParticipant()
                {
                    GroupId = 3
                }
            };
            string csvGroupIds = "1,2,3";
            string search = $"group_participants.group_id in ({csvGroupIds})" +
                            $" AND (Group_Participants.End_Date > GetDate() OR Group_Participants.End_Date Is Null)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groups);
            _groupParticipantRepository.Setup(mocked => mocked.GetLeadersGroupIds(participantId, null)).Returns(groups);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => searchString.Equals(search)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                true))
                .Returns(groupParticipantReturn);


            var result = _fixture.GetAllParticipantsForLeaderGroups(participantId, null, null);
            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2, "Result count should be two group participants");
            Assert.AreEqual(result[0].Email, groupParticipantReturn[0].Email);
            Assert.AreEqual(result[1].Email, groupParticipantReturn[1].Email);
        }

        [Test]
        public void GetAllParticipantsForLeaderGroupsSuccessWithParticipantIdAndGroupId()
        {
            var participantId = 3;
            var groupId = 9;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = 1,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 33,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = 2,
                    GroupParticipantId = 2
                },
                new MpGroupParticipant()
                {
                    ContactId = 32,
                    Email = "a3@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = 4,
                    GroupParticipantId = 4
                }
            };

            List<MpGroupParticipant> groups = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    GroupId = 1
                },
                new MpGroupParticipant()
                {
                    GroupId = 2
                },
                new MpGroupParticipant()
                {
                    GroupId = 3
                }
            };
            string search = $"group_participants.group_id in ({groupId})" +
                            $" AND (Group_Participants.End_Date > GetDate() OR Group_Participants.End_Date Is Null)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
        
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => searchString.Equals(search)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                true))
                .Returns(groupParticipantReturn);


            var result = _fixture.GetAllParticipantsForLeaderGroups(participantId, null, groupId);
            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2, "Result count should be two group participants");
            Assert.AreEqual(result[0].Email, groupParticipantReturn[0].Email);
            Assert.AreEqual(result[1].Email, groupParticipantReturn[1].Email);
        }

        [Test]
        public void GetIsLeaderSuccessWithoutGroupTypeId()
        {
            var participantId = 3;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 16,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 16,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
            };

            var search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {GroupLeaderRole}" +
                            $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                            $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)), 
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groupParticipantReturn);

            var result = _fixture.GetIsLeader(participantId);

            Assert.AreEqual(result, true);
        }

        [Test]
        public void GetIsLeaderSuccessWithGroupTypeId()
        {
            var participantId = 3;
            var groupType = 9;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 16,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 16,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
            };

            var search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {GroupLeaderRole}" +
                            $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                            $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)" +
                            $" AND Group_ID_Table.Group_Type_ID = {groupType}";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)),
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groupParticipantReturn);

            var result = _fixture.GetIsLeader(participantId, groupType);

            Assert.AreEqual(result, true);
        }

        [Test]
        public void GetIsLeaderFailure()
        {
            var participantId = 3;

            List<MpGroupParticipant> groupParticipantReturn = new List<MpGroupParticipant>();

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(groupParticipantReturn);

            var result = _fixture.GetIsLeader(participantId);

            Assert.AreEqual(result, false);
        }

        [Test]
        public void GetLeadersGroupIdsWithoutGroupTypeId()
        {
            var participantId = 3;
            var groupType = 9;

            List<MpGroupParticipant> leadersGroupParticipantRecords = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = participantId,
                    GroupParticipantId = 2,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = participantId,
                    GroupParticipantId = 3,
                },
            };

            string search = $"group_participants.participant_id = {participantId}" +
                                   $" AND group_participants.group_role_id = {GroupLeaderRole}" +
                                   $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                                   $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)),
                It.IsAny<string>(),
                (string)null,
                false))
                .Returns(leadersGroupParticipantRecords);

            var result = _fixture.GetLeadersGroupIds(participantId, null);

            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 3, "Result count should be three group participant records");
            Assert.AreEqual(result[0].GroupId, leadersGroupParticipantRecords[0].GroupId);
            Assert.AreEqual(result[1].GroupId, leadersGroupParticipantRecords[1].GroupId);
            Assert.AreEqual(result[2].GroupId, leadersGroupParticipantRecords[2].GroupId);
        }

        [Test]
        public void GetLeadersGroupIdsWithGroupTypeId()
        {
            var participantId = 3;
            var groupType = 9;

            List<MpGroupParticipant> leadersGroupParticipantRecords = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group one",
                    NickName = "name one",
                    LastName = "name one",
                    GroupId = 1,
                    ParticipantId = participantId,
                    GroupParticipantId = 1,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group two",
                    NickName = "name two",
                    LastName = "name two",
                    GroupId = 2,
                    ParticipantId = participantId,
                    GroupParticipantId = 2,
                },
                new MpGroupParticipant()
                {
                    ContactId = 30,
                    Email = "a@b.com",
                    GroupRoleId = 22,
                    GroupName = "group three",
                    NickName = "name three",
                    LastName = "name three",
                    GroupId = 3,
                    ParticipantId = participantId,
                    GroupParticipantId = 3,
                },
            };

            string search = $"group_participants.participant_id = {participantId}" +
                            $" AND group_participants.group_role_id = {GroupLeaderRole}" +
                            $" AND \\(Group_ID_Table.End_Date > '(.*?)' OR Group_ID_Table.End_Date Is Null\\)" +
                            $" AND \\(Group_Participants.End_Date > '(.*?)' OR Group_Participants.End_Date Is Null\\)" +
                            $" AND Group_ID_Table.Group_Type_ID = {groupType}";

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns("yeah!");
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("yeah!")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Search<MpGroupParticipant>(
                It.Is<string>(searchString => MatchesDate(searchString, search)),
                It.IsAny<string>(),
                (string) null,
                false))
                .Returns(leadersGroupParticipantRecords);

            var result = _fixture.GetLeadersGroupIds(participantId, groupType);

            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 3, "Result count should be three group participant records");
            Assert.AreEqual(result[0].GroupId, leadersGroupParticipantRecords[0].GroupId);
            Assert.AreEqual(result[1].GroupId, leadersGroupParticipantRecords[1].GroupId);
            Assert.AreEqual(result[2].GroupId, leadersGroupParticipantRecords[2].GroupId);
        }
    }
}