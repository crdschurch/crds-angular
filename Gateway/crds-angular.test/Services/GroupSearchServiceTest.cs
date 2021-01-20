using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MpAttribute = MinistryPlatform.Translation.Models.MpAttribute;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;

namespace crds_angular.test.Services
{
    public class GroupSearchServiceTest
    {
        private GroupSearchService _fixture;
        private Mock<IGroupRepository> _groupService;
        private Mock<IConfigurationWrapper> _config;
        private Mock<MPServices.IAttributeRepository> _attributeService;
        private const int GroupGoalConnectWithCommunity = 6999;
        private const int GroupGoalMakeFriends = 7002;
        private const int GroupGoalLearnAndGrow = 7000;
        private const int GroupGoalMentorOthers = 7001;

        private const int ParticipantGoalNotSure = 7003;
        private const int ParticipantGoalGrowSpiritually = 7004;
        private const int ParticipantGoalLearnFromSomeone = 7005;
        private const int ParticipantGoalMakeFriends = 7006;

        private const int MaxGroupSearchResults = 32;
        private const string InMarketZipCodes = "40355, 41001, 41005, 41006, 41007, 41011";
        private const int WeekendTimes = 7030;
        private const int WeekdayTimes = 7023;
        private const int ParticipantGoal = 7003;
        private const int Gender = 7018;
        private const int MaritalStatus = 7020;


   
        [SetUp]
        public void SetUp()
        {
            _groupService = new Mock<IGroupRepository>();
            _attributeService = new Mock<MPServices.IAttributeRepository>();
            
            _config = new Mock<IConfigurationWrapper>();

            _config.Setup(mocked => mocked.GetConfigIntValue("MaxGroupSearchResults")).Returns(MaxGroupSearchResults);
            _config.Setup(mocked => mocked.GetConfigValue("InMarketZipCodes")).Returns(InMarketZipCodes);
            _config.Setup(mocked => mocked.GetConfigIntValue("WeekendTimesAttributeTypeId")).Returns(WeekendTimes);
            _config.Setup(mocked => mocked.GetConfigIntValue("WeekdayTimesAttributeTypeId")).Returns(WeekdayTimes);
            _config.Setup(mocked => mocked.GetConfigIntValue("ParticipantGoalAttributeTypeId")).Returns(ParticipantGoal);
            _config.Setup(mocked => mocked.GetConfigIntValue("GenderTypeAttributeTypeId")).Returns(Gender);
            _config.Setup(mocked => mocked.GetConfigIntValue("MaritalStatusTypeAttributeTypeId")).Returns(MaritalStatus);

            _config.Setup(mocked => mocked.GetConfigIntValue("GroupGoalConnectWithCommunity")).Returns(GroupGoalConnectWithCommunity);
            _config.Setup(mocked => mocked.GetConfigIntValue("GroupGoalMakeFriends")).Returns(GroupGoalMakeFriends);
            _config.Setup(mocked => mocked.GetConfigIntValue("GroupGoalLearnAndGrow")).Returns(GroupGoalLearnAndGrow);
            _config.Setup(mocked => mocked.GetConfigIntValue("GroupGoalMentorOthers")).Returns(GroupGoalMentorOthers);

            _config.Setup(mocked => mocked.GetConfigIntValue("ParticipantGoalNotSure")).Returns(ParticipantGoalNotSure);
            _config.Setup(mocked => mocked.GetConfigIntValue("ParticipantGoalGrowSpiritually")).Returns(ParticipantGoalGrowSpiritually);
            _config.Setup(mocked => mocked.GetConfigIntValue("ParticipantGoalLearnFromSomeone")).Returns(ParticipantGoalLearnFromSomeone);
            _config.Setup(mocked => mocked.GetConfigIntValue("ParticipantGoalMakeFriends")).Returns(ParticipantGoalMakeFriends);

            _fixture = new GroupSearchService(_groupService.Object, _attributeService.Object,_config.Object);
        }

        [Test]
        public void FindMatchesTest()
        {
            const int groupTypeId = 19;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now.AddMonths(4);

            var participant = new GroupParticipantDTO();
            
            _groupService.Setup(mocked => mocked.GetSearchResults(groupTypeId)).Returns(It.IsAny<List<MpGroupSearchResult>>());
            _attributeService.Setup(mocked => mocked.GetAttributes(null)).Returns(It.IsAny<List<MpAttribute>>());
            
            //var result =  _fixture.FindMatches(groupTypeId, participant);
            
        }
    }
}