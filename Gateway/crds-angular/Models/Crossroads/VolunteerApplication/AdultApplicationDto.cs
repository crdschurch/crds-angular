using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.VolunteerApplication
{
    public class AdultApplicationDto : VolunteerApplicationDto
    {
        [JsonProperty(PropertyName = "firstName")]
        [Required]
        public CustomField FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        [Required]
        public CustomField LastName { get; set; }

        [JsonProperty(PropertyName = "middleInitial")]
        public CustomField MiddleInitial { get; set; }

        [JsonProperty(PropertyName = "previousName")]
        public CustomField PreviousName { get; set; }

        [JsonProperty(PropertyName = "email")]
        [Required]
        public CustomField Email { get; set; }

        [JsonProperty(PropertyName = "nameForNameTag")]
        [Required]
        public CustomField NameForNameTag { get; set; }

        [JsonProperty(PropertyName = "birthDate")]
        [Required]
        public CustomField BirthDate { get; set; }

        [JsonProperty(PropertyName = "gender")]
        [Required]
        public CustomField Gender { get; set; }

        [JsonProperty(PropertyName = "maritalStatus")]
        [Required]
        public CustomField MaritalStatus { get; set; }

        [JsonProperty(PropertyName = "spouseName")]
        public CustomField SpouseName { get; set; }

        [JsonProperty(PropertyName = "spouseGender")]
        public CustomField SpouseGender { get; set; }

        [JsonProperty(PropertyName = "howLongAttending")]
        [Required]
        public CustomField HowLongAttending { get; set; }

        [JsonProperty(PropertyName = "serviceAttend")]
        [Required]
        public CustomField WhatServiceDoYouAttend { get; set; }

        [JsonProperty(PropertyName = "streetAddress")]
        [Required]
        public CustomField StreetAddress { get; set; }

        [JsonProperty(PropertyName = "city")]
        [Required]
        public CustomField City { get; set; }

        [JsonProperty(PropertyName = "state")]
        [Required]
        public CustomField State { get; set; }

        [JsonProperty(PropertyName = "zip")]
        [Required]
        public CustomField Zip { get; set; }

        [JsonProperty(PropertyName = "mobilePhone")]
        [Required]
        public CustomField MobilePhone { get; set; }

        [JsonProperty(PropertyName = "homePhone")]
        [Required]
        public CustomField HomePhone { get; set; }
        
        [JsonProperty(PropertyName = "companyName")]
        [Required]
        public CustomField CompanyName { get; set; }

        [JsonProperty(PropertyName = "position")]
        [Required]
        public CustomField Position { get; set; }

        [JsonProperty(PropertyName = "workPhone")]
        public CustomField WorkPhone { get; set; }

        [JsonProperty(PropertyName = "child1Name")]
        public CustomField Child1Name { get; set; }

        [JsonProperty(PropertyName = "child1Birthdate")]
        public CustomField Child1Birthdate { get; set; }

        [JsonProperty(PropertyName = "child2Name")]
        public CustomField Child2Name { get; set; }

        [JsonProperty(PropertyName = "child2Birthdate")]
        public CustomField Child2Birthdate { get; set; }

        [JsonProperty(PropertyName = "child3Name")]
        public CustomField Child3Name { get; set; }

        [JsonProperty(PropertyName = "child3Birthdate")]
        public CustomField Child3Birthdate { get; set; }

        [JsonProperty(PropertyName = "child4Name")]
        public CustomField Child4Name { get; set; }

        [JsonProperty(PropertyName = "child4Birthdate")]
        public CustomField Child4Birthdate { get; set; }
        
        [JsonProperty(PropertyName = "everBeenArrest")]
        [Required]
        public CustomField EverBeenArrest { get; set; }

        [JsonProperty(PropertyName = "addictionConcern")]
        [Required]
        public CustomField AddictionConcern { get; set; }

        [JsonProperty(PropertyName = "neglectingChild")]
        [Required]
        public CustomField NeglectingChild { get; set; }

        [JsonProperty(PropertyName = "psychiatricDisorder")]
        [Required]
        public CustomField PsychiatricDisorder { get; set; }

        [JsonProperty(PropertyName = "sexuallyActiveOutsideMarriage")]
        [Required]
        public CustomField SexuallyActiveOutsideMarriage { get; set; }

        [JsonProperty(PropertyName = "religionSearchingForAnswers")]
        [Required]
        public CustomField ReligionSearchingForAnswers { get; set; }

        [JsonProperty(PropertyName = "religionReceivedJesus")]
        [Required]
        public CustomField ReligionReceivedJesus { get; set; }

        [JsonProperty(PropertyName = "religionFocusingOnObedience")]
        [Required]
        public CustomField ReligionFocusingOnObedience { get; set; }

        [JsonProperty(PropertyName = "religionReplicating")]
        [Required]
        public CustomField ReligionReplicating { get; set; }

        [JsonProperty(PropertyName = "spiritualOrientationExplain")]
        [Required]
        public CustomField SpiritualOrientationExplain { get; set; }

        [JsonProperty(PropertyName = "whatPromptedApplication")]
        [Required]
        public CustomField WhatPromptedApplication { get; set; }

        [JsonProperty(PropertyName = "specialTalents")]
        [Required]
        public CustomField SpecialTalents { get; set; }

        [JsonProperty(PropertyName = "availabilityWeek")]
        public CustomField AvailabilityWeek { get; set; }

        [JsonProperty(PropertyName = "availabilityWeekend")]
        public CustomField AvailabilityWeekend { get; set; }

        [JsonProperty(PropertyName = "availabilityWeekendSite")]
        public CustomField AvailabilityWeekendSite { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestServingInClassroom")]
        public CustomField AreaOfInterestServingInClassroom { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestWelcomingNewFamilies")]
        public CustomField AreaOfInterestWelcomingNewFamilies { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestHelpSpecialNeeds")]
        public CustomField AreaOfInterestHelpSpecialNeeds { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestTech")]
        public CustomField AreaOfInterestTech { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestRoomPrep")]
        public CustomField AreaOfInterestRoomPrep { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestAdminTasks")]
        public CustomField AreaOfInterestAdminTasks { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestShoppingForSupplies")]
        public CustomField AreaOfInterestShoppingForSupplies { get; set; }

        [JsonProperty(PropertyName = "areaOfInterestCreatingWeekendExperience")]
        public CustomField AreaOfInterestCreatingWeekendExperience { get; set; }

        [JsonProperty(PropertyName = "whatAgeBirthToTwo")]
        public CustomField WhatAgeBirthToTwo { get; set; }
        
        [JsonProperty(PropertyName = "whatAgeThreeToPreK")]
        public CustomField WhatAgeThreeToPreK { get; set; }

        [JsonProperty(PropertyName = "whatAgeKToFifth")]
        public CustomField WhatAgeKToFifth { get; set; }

        [JsonProperty(PropertyName = "reference1Name")]
        [Required]
        public CustomField Reference1Name { get; set; }

        [JsonProperty(PropertyName = "reference1timeKnown")]
        [Required]
        public CustomField Reference1TimeKnown { get; set; }

        [JsonProperty(PropertyName = "reference1homePhone")]
        public CustomField Reference1HomePhone { get; set; }

        [JsonProperty(PropertyName = "reference1mobilePhone")]
        public CustomField Reference1MobilePhone { get; set; }

        [JsonProperty(PropertyName = "reference1workPhone")]
        public CustomField Reference1WorkPhone { get; set; }

        [JsonProperty(PropertyName = "reference1email")]
        [Required]
        public CustomField Reference1Email { get; set; }

        [JsonProperty(PropertyName = "reference1association")]
        [Required]
        public CustomField Reference1Association { get; set; }

        [JsonProperty(PropertyName = "reference1occupation")]
        [Required]
        public CustomField Reference1Occupation { get; set; }


        //ref 2
        [JsonProperty(PropertyName = "reference2Name")]
        [Required]
        public CustomField Reference2Name { get; set; }

        [JsonProperty(PropertyName = "reference2timeKnown")]
        [Required]
        public CustomField Reference2TimeKnown { get; set; }

        [JsonProperty(PropertyName = "reference2homePhone")]
        public CustomField Reference2HomePhone { get; set; }

        [JsonProperty(PropertyName = "reference2mobilePhone")]
        public CustomField Reference2MobilePhone { get; set; }

        [JsonProperty(PropertyName = "reference2workPhone")]
        public CustomField Reference2WorkPhone { get; set; }

        [JsonProperty(PropertyName = "reference2email")]
        [Required]
        public CustomField Reference2Email { get; set; }

        [JsonProperty(PropertyName = "reference2association")]
        [Required]
        public CustomField Reference2Association { get; set; }

        [JsonProperty(PropertyName = "reference2occupation")]
        [Required]
        public CustomField Reference2Occupation { get; set; }

        //ref 3
        [JsonProperty(PropertyName = "reference3Name")]
        [Required]
        public CustomField Reference3Name { get; set; }

        [JsonProperty(PropertyName = "reference3timeKnown")]
        [Required]
        public CustomField Reference3TimeKnown { get; set; }

        [JsonProperty(PropertyName = "reference3homePhone")]
        public CustomField Reference3HomePhone { get; set; }

        [JsonProperty(PropertyName = "reference3mobilePhone")]
        public CustomField Reference3MobilePhone { get; set; }

        [JsonProperty(PropertyName = "reference3workPhone")]
        public CustomField Reference3WorkPhone { get; set; }

        [JsonProperty(PropertyName = "reference3email")]
        [Required]
        public CustomField Reference3Email { get; set; }

        [JsonProperty(PropertyName = "reference3association")]
        [Required]
        public CustomField Reference3Association { get; set; }

        [JsonProperty(PropertyName = "reference3occupation")]
        [Required]
        public CustomField Reference3Occupation { get; set; }

        [JsonProperty(PropertyName = "agree")]
        [Required]
        public CustomField Agree { get; set; }

        [JsonProperty(PropertyName = "agreeDate")]
        [Required]
        public CustomField AgreeDate { get; set; }
    }
}