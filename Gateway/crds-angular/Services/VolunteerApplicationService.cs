using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Models.Crossroads.VolunteerApplication;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class VolunteerApplicationService : IVolunteerApplicationService
    {
        private readonly IFormSubmissionRepository _formSubmissionService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private List<MpFormField> _formFields;

        private readonly IServeService _serveService;

        public VolunteerApplicationService(IFormSubmissionRepository formSubmissionService,
            IConfigurationWrapper configurationWrapper, IServeService serveService)
        {
            _formSubmissionService = formSubmissionService;
            _configurationWrapper = configurationWrapper;
            _serveService = serveService;
        }

        public bool SaveAdult(AdultApplicationDto application)
        {
            var formId = _configurationWrapper.GetConfigIntValue("KidsClubAdultApplicant");
            var opportunityResponseId = application.ResponseOpportunityId;
            _formFields = _formSubmissionService.GetFieldsForForm(formId);

            var formResponse = new MpFormResponse();
            formResponse.ContactId = application.ContactId; //contact id of the person the application is for
            formResponse.FormId = formId;
            formResponse.OpportunityId = application.OpportunityId; // we know this from CMS
            formResponse.OpportunityResponseId = opportunityResponseId;

            formResponse.FormAnswers.Add(SetCustomField(application.FirstName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.LastName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.MiddleInitial, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.BirthDate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.PreviousName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.NameForNameTag, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(TranslateGender(application.Gender), opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(TranslateMaritalStatus(application.MaritalStatus),
                opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SpouseName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SpouseGender, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.HowLongAttending, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.WhatServiceDoYouAttend, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.StreetAddress, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.City, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.State, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Zip, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.HomePhone, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.CompanyName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Position, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.WorkPhone, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.Child1Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child1Birthdate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child2Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child2Birthdate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child3Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child3Birthdate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child4Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Child4Birthdate, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.EverBeenArrest, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AddictionConcern, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.NeglectingChild, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.PsychiatricDisorder, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SexuallyActiveOutsideMarriage, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.ReligionSearchingForAnswers, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionReceivedJesus, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionFocusingOnObedience, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionReplicating, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SpiritualOrientationExplain, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.WhatPromptedApplication, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SpecialTalents, opportunityResponseId));


            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityWeek, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityWeekend, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityWeekendSite, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestServingInClassroom,
                opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestWelcomingNewFamilies,
                opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestHelpSpecialNeeds,
                opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestTech, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestRoomPrep, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestAdminTasks, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestShoppingForSupplies,
                opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AreaOfInterestCreatingWeekendExperience,
                opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.WhatAgeBirthToTwo, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.WhatAgeThreeToPreK, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.WhatAgeKToFifth, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1TimeKnown, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1WorkPhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Association, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Occupation, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2TimeKnown, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2WorkPhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Association, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Occupation, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.Reference3Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3TimeKnown, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3WorkPhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3Association, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference3Occupation, opportunityResponseId));

            formResponse.FormAnswers.Add(SetCustomField(application.Agree, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AgreeDate, opportunityResponseId));

            var response = _formSubmissionService.SubmitFormResponse(formResponse);
            return response != 0;
        }

        public bool SaveStudent(StudentApplicationDto application)
        {
            var formId = _configurationWrapper.GetConfigIntValue("KidsClubStudentApplicant");
            var opportunityResponseId = application.ResponseOpportunityId;
            _formFields = _formSubmissionService.GetFieldsForForm(formId);

            var formResponse = new MpFormResponse();
            formResponse.ContactId = application.ContactId; //contact id of the person the application is for
            formResponse.FormId = formId;
            formResponse.OpportunityId = application.OpportunityId; // we know this from CMS
            formResponse.OpportunityResponseId = opportunityResponseId;

            formResponse.FormAnswers.Add(SetCustomField(application.FirstName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.LastName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.MiddleInitial, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.BirthDate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(TranslateGender(application.Gender), opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.NameForNameTag, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.School, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.HowLongAttending, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ServiceAttend, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.StreetAddress, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.City, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.State, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Zip, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Grade, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionSearchingForAnswers, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionReceivedJesus, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionFocusingOnObedience, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ReligionReplicating, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ExplainFaith, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.WhyServe, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.SpecialTalents, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityDuringWeek, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityDuringWeekend, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.AvailabilityWeekendSite, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ServeAgeKids1To2, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ServeAgeKids3ToPreK, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ServeAgeKidsKto5Grade, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1TimeKnown, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1WorkPhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Association, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference1Occupation, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Name, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2TimeKnown, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2HomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2MobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2WorkPhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Email, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Association, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.Reference2Occupation, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentLastName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentFirstName, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentHomePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentMobilePhone, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentEmail, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentSignature, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.ParentSignatureDate, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.StudentSignature, opportunityResponseId));
            formResponse.FormAnswers.Add(SetCustomField(application.StudentSignatureDate, opportunityResponseId));

            var response = _formSubmissionService.SubmitFormResponse(formResponse);
            return response != 0;
        }

        private static CustomField TranslateGender(CustomField gender)
        {
            switch (gender.Value)
            {
                case "1":
                    gender.Value = Gender.Male.ToString();
                    break;
                case "2":
                    gender.Value = Gender.Female.ToString();
                    break;
            }

            return gender;
        }

        private static CustomField TranslateMaritalStatus(CustomField maritalStatus)
        {
            switch (maritalStatus.Value)
            {
                case "1":
                    maritalStatus.Value = MaritalStatus.Single.ToString();
                    break;
                case "2":
                    maritalStatus.Value = MaritalStatus.Married.ToString();
                    break;
                case "3":
                    maritalStatus.Value = MaritalStatus.Divorced.ToString();
                    break;
                case "4":
                    maritalStatus.Value = MaritalStatus.Widowed.ToString();
                    break;
                case "6":
                    maritalStatus.Value = MaritalStatus.Seperated.ToString();
                    break;
            }
            return maritalStatus;
        }


        private MpFormAnswer SetCustomField(CustomField customField, int opportunityResponseId)
        {
            int fieldId;
            try
            {
                fieldId = _formFields.Single(f => f.CrossroadsId == customField.CrossroadsId).FormFieldId;
            }
            catch
            {
                throw new ApplicationException(string.Format("Failed to locate id for crossroads field {0}",
                    customField.CrossroadsId));
            }
            var answer = new MpFormAnswer();
            answer.FieldId = fieldId;
            answer.OpportunityResponseId = opportunityResponseId;
            answer.Response = customField.Value;
            return answer;
        }


        public List<FamilyMember> FamilyThatUserCanSubmitFor(string token)
        {
            var list = _serveService.GetImmediateFamilyParticipants(token);
            var family =
                list.Where(
                    s =>
                        (s.RelationshipId == 0) || (s.RelationshipId == 29) ||
                        (s.RelationshipId == 21 && s.Age >= 10 && s.Age <= 13) ||
                        (s.RelationshipId == 6 && s.Age >= 10 && s.Age <= 13)).ToList();
            //TODO: I don't like these rules hard coded here, Relationship IDs and age limits
            return family;
        }
    }
}