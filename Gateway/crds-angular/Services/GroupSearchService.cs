using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MpAttribute = MinistryPlatform.Translation.Models.MpAttribute;
using IAttributeRepository = MinistryPlatform.Translation.Repositories.Interfaces.IAttributeRepository;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;

namespace crds_angular.Services
{
    public class GroupSearchService : IGroupSearchService
    {
        private readonly IGroupRepository _mpGroupService;
        private readonly IAttributeRepository _attributeService;
        private readonly int GroupGoalAttributeTypeId;
        private readonly int GroupTypeAttributeTypeId;
        private readonly int ParticipantGoalAttributeTypeId;
        private readonly int GenderTypeAttributeTypeId;
        private readonly int MaritalStatusTypeAttributeTypeId;
        private readonly int WeekdayTimesAttributeTypeId;
        private readonly int WeekendTimesAttributeTypeId;

        private readonly int ParticipantJourneyTogetherAttributeId;
        private readonly int ParticipantGenderManAttributeId;
        private readonly int ParticipantGenderWomanAttributeId;

        private readonly int GroupTypeMixedAttributeId;
        private readonly int GroupTypeMenAttributeId;
        private readonly int GroupTypeWomenAttributeId;
        private readonly int GroupTypeMarriedCouplesAttributeId;

        private readonly int GroupPetsAttributeTypeId;
        private readonly int GroupPetsDogAttributeId;
        private readonly int GroupPetsCatAttributeId;

        private readonly int GroupGoalConnectWithCommunity;
        private readonly int GroupGoalMakeFriends;
        private readonly int GroupGoalLearnAndGrow;
        private readonly int GroupGoalMentorOthers;

        private readonly int ParticipantGoalNotSure;
        private readonly int ParticipantGoalGrowSpiritually;
        private readonly int ParticipantGoalLearnFromSomeone;
        private readonly int ParticipantGoalMakeFriends;
        private readonly int MaxGroupSearchResults;
        private List<string> _inMarketZipCodes;
        private Dictionary<int, int> _goalMatches;

        public GroupSearchService(IGroupRepository mpGroupService,
                                  IAttributeRepository attributeService, 
                                  IConfigurationWrapper configurationWrapper)
        {
            _mpGroupService = mpGroupService;
            _attributeService = attributeService;

            GroupGoalAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupGoalAttributeTypeId");
            GroupTypeAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupTypeAttributeTypeId");
            ParticipantGoalAttributeTypeId = configurationWrapper.GetConfigIntValue("ParticipantGoalAttributeTypeId");
            GenderTypeAttributeTypeId = configurationWrapper.GetConfigIntValue("GenderTypeAttributeTypeId");
            MaritalStatusTypeAttributeTypeId = configurationWrapper.GetConfigIntValue("MaritalStatusTypeAttributeTypeId");
            WeekdayTimesAttributeTypeId = configurationWrapper.GetConfigIntValue("WeekdayTimesAttributeTypeId");
            WeekendTimesAttributeTypeId = configurationWrapper.GetConfigIntValue("WeekendTimesAttributeTypeId");

            ParticipantJourneyTogetherAttributeId = configurationWrapper.GetConfigIntValue("ParticipantJourneyTogetherAttributeId");
            ParticipantGenderManAttributeId = configurationWrapper.GetConfigIntValue("ParticipantGenderManAttributeId");
            ParticipantGenderWomanAttributeId = configurationWrapper.GetConfigIntValue("ParticipantGenderWomanAttributeId");

            GroupTypeMixedAttributeId = configurationWrapper.GetConfigIntValue("GroupTypeMixedAttributeId");
            GroupTypeMenAttributeId = configurationWrapper.GetConfigIntValue("GroupTypeMenAttributeId");
            GroupTypeWomenAttributeId = configurationWrapper.GetConfigIntValue("GroupTypeWomenAttributeId");
            GroupTypeMarriedCouplesAttributeId = configurationWrapper.GetConfigIntValue("GroupTypeMarriedCouplesAttributeId");

            GroupPetsAttributeTypeId = configurationWrapper.GetConfigIntValue("GroupPetsAttributeTypeId");
            GroupPetsCatAttributeId = configurationWrapper.GetConfigIntValue("GroupPetsCatAttributeId");
            GroupPetsDogAttributeId = configurationWrapper.GetConfigIntValue("GroupPetsDogAttributeId");

            GroupGoalConnectWithCommunity = configurationWrapper.GetConfigIntValue("GroupGoalConnectWithCommunity");
            GroupGoalMakeFriends = configurationWrapper.GetConfigIntValue("GroupGoalMakeFriends");
            GroupGoalLearnAndGrow = configurationWrapper.GetConfigIntValue("GroupGoalLearnAndGrow");
            GroupGoalMentorOthers = configurationWrapper.GetConfigIntValue("GroupGoalMentorOthers");

            ParticipantGoalNotSure = configurationWrapper.GetConfigIntValue("ParticipantGoalNotSure");
            ParticipantGoalGrowSpiritually = configurationWrapper.GetConfigIntValue("ParticipantGoalGrowSpiritually");
            ParticipantGoalLearnFromSomeone = configurationWrapper.GetConfigIntValue("ParticipantGoalLearnFromSomeone");
            ParticipantGoalMakeFriends = configurationWrapper.GetConfigIntValue("ParticipantGoalMakeFriends");

            _inMarketZipCodes = ParseZipCodes(configurationWrapper.GetConfigValue("InMarketZipCodes"));
            MaxGroupSearchResults = configurationWrapper.GetConfigIntValue("MaxGroupSearchResults");

            _goalMatches = new Dictionary<int, int>()
            {
                {GroupGoalConnectWithCommunity, ParticipantGoalNotSure},
                {GroupGoalMakeFriends, ParticipantGoalMakeFriends},
                {GroupGoalLearnAndGrow, ParticipantGoalGrowSpiritually},
                {GroupGoalMentorOthers, ParticipantGoalLearnFromSomeone},
            };
        }

        private List<string> ParseZipCodes(string zipCodes)
        {
            zipCodes = zipCodes.Replace(" ", "");
            return zipCodes.Split(',').ToList();
        }

        public IEnumerable<GroupDTO> FindMatches(int groupTypeId, GroupParticipantDTO participant)
        {
            // Load all groups for potential match
            var mpGroups = _mpGroupService.GetSearchResults(groupTypeId);
            var mpAttributes = _attributeService.GetAttributes(null);

            var mpFilteredGroups = FilterSearchResults(participant, mpGroups);
            mpFilteredGroups = mpFilteredGroups.Take(MaxGroupSearchResults);

            var groups = ConvertToGroupDto(mpFilteredGroups, mpAttributes);
         
            return groups;
        }

        private IEnumerable<MpGroupSearchResult> FilterSearchResults(GroupParticipantDTO participant, IEnumerable<MpGroupSearchResult> mpGroups)
        {
            ObjectSingleAttributeDTO participantGoal;
            ObjectSingleAttributeDTO gender;
            ObjectSingleAttributeDTO maritalStatus;
            ObjectAttributeTypeDTO weekdayTimes;
            ObjectAttributeTypeDTO weekendTimes;

            // Single-Attributes
            participant.SingleAttributes.TryGetValue(ParticipantGoalAttributeTypeId, out participantGoal);
            participant.SingleAttributes.TryGetValue(GenderTypeAttributeTypeId, out gender);
            participant.SingleAttributes.TryGetValue(MaritalStatusTypeAttributeTypeId, out maritalStatus);

            // Attributes
            participant.AttributeTypes.TryGetValue(WeekdayTimesAttributeTypeId, out weekdayTimes);
            participant.AttributeTypes.TryGetValue(WeekendTimesAttributeTypeId, out weekendTimes);

            return mpGroups.Where(mpGroup =>
                                      CheckCapacity(maritalStatus, mpGroup.RemainingCapacity) &&
                                      InMarket(mpGroup.Address.Postal_Code) &&
                                      MatchDayTime(weekdayTimes, weekendTimes, mpGroup.SearchAttributes.MeetingRangeId) &&
                                      MatchGroupType(gender, maritalStatus, mpGroup.SearchAttributes.TypeId))
                .OrderByDescending(mpGroup => SortGoal(participantGoal, mpGroup.SearchAttributes.GoalId));
        }

        private Boolean InMarket(String postalCode)
        {
            if (string.IsNullOrEmpty(postalCode))
            {
                return false;
            }

            var length = postalCode.Length <= 5 ? postalCode.Length : 5;
            postalCode = postalCode.Substring(0, length);
            return _inMarketZipCodes.Contains(postalCode);
        }

        private Boolean CheckCapacity(ObjectSingleAttributeDTO maritalStatus, int capacity)
        {
            return capacity > (maritalStatus.Value.AttributeId == ParticipantJourneyTogetherAttributeId ? 1 : 0);
        }

        private Boolean MatchGroupType(ObjectSingleAttributeDTO gender, ObjectSingleAttributeDTO maritalStatus, int? groupTypeId)
        {
            return (groupTypeId == GroupTypeMixedAttributeId && maritalStatus.Value.AttributeId != ParticipantJourneyTogetherAttributeId) ||
                   (groupTypeId == GroupTypeMarriedCouplesAttributeId && maritalStatus.Value.AttributeId == ParticipantJourneyTogetherAttributeId) ||
                   (groupTypeId == GroupTypeMixedAttributeId && maritalStatus.Value.AttributeId == ParticipantJourneyTogetherAttributeId) ||
                   (groupTypeId == GroupTypeMenAttributeId && (gender.Value.AttributeId == ParticipantGenderManAttributeId) && (maritalStatus.Value.AttributeId != ParticipantJourneyTogetherAttributeId)) ||
                   (groupTypeId == GroupTypeWomenAttributeId && (gender.Value.AttributeId == ParticipantGenderWomanAttributeId) && (maritalStatus.Value.AttributeId != ParticipantJourneyTogetherAttributeId));
        }

        private Boolean MatchDayTime(ObjectAttributeTypeDTO weekdayTime, ObjectAttributeTypeDTO weekendTime, int? attributeId)
        {
            var weekdayMatch = weekdayTime.Attributes.Exists(x => x.AttributeId == attributeId);
            var weekendMatch = weekendTime.Attributes.Exists(x => x.AttributeId == attributeId);
            return weekdayMatch || weekendMatch;
        }

        private Boolean SortGoal(ObjectSingleAttributeDTO participantGoal, int? groupGoalId)
        {            

            if (!groupGoalId.HasValue)
            {
                return false;
            }

            if (!_goalMatches.ContainsKey(groupGoalId.Value))
            {
                return false;
            }

            return participantGoal.Value.AttributeId == _goalMatches[groupGoalId.Value];
        }

        private List<GroupDTO> ConvertToGroupDto(IEnumerable<MpGroupSearchResult> mpGroups, List<MpAttribute> mpAttributes)
        {
            var groups = new List<GroupDTO>();

            foreach (var mpGroup in mpGroups)
            {
                var group = Mapper.Map<GroupDTO>(mpGroup);

                var searchAttributes = mpGroup.SearchAttributes;

                // Multi-Attributes
                var groupPets = GetPetAttributes(mpAttributes, searchAttributes);
                group.AttributeTypes.Add(groupPets.AttributeTypeId, groupPets);

                // Single-Attributes
                if (searchAttributes.GoalId.HasValue)
                {
                    var goal = ConvertToSingleAttribute(mpAttributes, searchAttributes.GoalId.Value);
                    group.SingleAttributes.Add(goal.Key, goal.Value);
                }               

                if (searchAttributes.KidsId.HasValue)
                {
                    var kids = ConvertToSingleAttribute(mpAttributes, searchAttributes.KidsId.Value);
                    group.SingleAttributes.Add(kids.Key, kids.Value);
                }

                if (searchAttributes.TypeId.HasValue)
                {
                    var groupType = ConvertToSingleAttribute(mpAttributes, searchAttributes.TypeId.Value);
                    group.SingleAttributes.Add(groupType.Key, groupType.Value);
                }

                groups.Add(group);
            }

            return groups;
        }

        private ObjectAttributeTypeDTO GetPetAttributes(List<MpAttribute> mpAttributes, MpGroupSearchAttributes searchAttributes)
        {
            var petsAttributeType = mpAttributes.First(x => x.AttributeTypeId == GroupPetsAttributeTypeId);

            var groupPets = new ObjectAttributeTypeDTO()
            {
                AttributeTypeId = petsAttributeType.AttributeTypeId,
                Name = petsAttributeType.AttributeTypeName,
                Attributes = new List<ObjectAttributeDTO>()
            };

            // TODO: Determine if we can loop over the attributes and lookup if attribute type is a searchAttrbitues.CatId or DogId           
            var cat = ConvertToMultiAttribute(mpAttributes, GroupPetsCatAttributeId, searchAttributes.CatId.HasValue);
            groupPets.Attributes.Add(cat);

            var dog = ConvertToMultiAttribute(mpAttributes, GroupPetsDogAttributeId, searchAttributes.DogId.HasValue);
            groupPets.Attributes.Add(dog);

            return groupPets;
        }

        private KeyValuePair<int, ObjectSingleAttributeDTO> ConvertToSingleAttribute(List<MpAttribute> mpAttributes, int attributeId)
        {
            var mpAttribute = mpAttributes.First(x => x.AttributeId == attributeId);
            var groupSingleAttribute = new ObjectSingleAttributeDTO()
            {
                Notes = string.Empty,
                Value = new AttributeDTO()
                {
                    AttributeId = mpAttribute.AttributeId,
                    Category = mpAttribute.Category,
                    CategoryDescription = mpAttribute.CategoryDescription,
                    CategoryId = mpAttribute.CategoryId,
                    Description = mpAttribute.Description,
                    Name = mpAttribute.Name,
                    SortOrder = mpAttribute.SortOrder
                }
            };

            return new KeyValuePair<int, ObjectSingleAttributeDTO>(mpAttribute.AttributeTypeId, groupSingleAttribute);            
        }

        private ObjectAttributeDTO ConvertToMultiAttribute(List<MpAttribute> mpAttributes, int attributeId, bool selected)
        {
            var mpAttribute = mpAttributes.First(x => x.AttributeId == attributeId);

            var groupAttribute = new ObjectAttributeDTO()
            {
                AttributeId = mpAttribute.AttributeId,
                Category = mpAttribute.Category,
                CategoryDescription = mpAttribute.CategoryDescription,
                Description = mpAttribute.Description,
                Selected = selected,
                StartDate = DateTime.Today,
                EndDate = null,
                Notes = string.Empty,
                Name = mpAttribute.Name
            };

            return groupAttribute;
        }
    }
}