using System;
using System.Collections.Generic;
using AutoMapper;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Models.People;
using MinistryPlatform.Translation.Repositories;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using Participant = MinistryPlatform.Translation.Models.Participant;


namespace crds_angular.Services
{
    public class PersonService : MinistryPlatformBaseService, IPersonService
    {
        private readonly MPServices.IContactRepository _contactService;
        private readonly IObjectAttributeService _objectAttributeService;
        private readonly MPServices.IApiUserRepository _apiUserService;
        private readonly MPServices.IParticipantRepository _participantService;
        private readonly MPServices.IUserRepository _userService;
        private readonly MPServices.IAuthenticationRepository _authenticationService;

        public PersonService(MPServices.IContactRepository contactService, 
            IObjectAttributeService objectAttributeService, 
            MPServices.IApiUserRepository apiUserService,
            MPServices.IParticipantRepository participantService,
            MPServices.IUserRepository userService,
            MPServices.IAuthenticationRepository authenticationService)
        {
            _contactService = contactService;
            _objectAttributeService = objectAttributeService;
            _apiUserService = apiUserService;
            _participantService = participantService;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public void SetProfile(String token, Person person)
        {
            var contactDictionary = getDictionary(person.GetContact());
            var householdDictionary = getDictionary(person.GetHousehold());
            var addressDictionary = getDictionary(person.GetAddress());
            addressDictionary.Add("State/Region", addressDictionary["State"]);
            _contactService.UpdateContact(person.ContactId, contactDictionary, householdDictionary, addressDictionary);

            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            _objectAttributeService.SaveObjectAttributes(person.ContactId, person.AttributeTypes, person.SingleAttributes, configuration);

            Participant participant = _participantService.GetParticipant(person.ContactId);
            if (participant.AttendanceStart != person.AttendanceStartDate)
            {                
                participant.AttendanceStart = person.AttendanceStartDate;
                // convert to the object with underscores
                var p = Mapper.Map <MpParticipant>(participant);
                _participantService.UpdateParticipant(getDictionary(p));
            }

            // TODO: It appears we are updating the contact records email address above if the email address is changed
            // TODO: If the password is invalid we would not run the update on user, and therefore create a data integrity problem
            // TODO: See About moving the check for new password above or moving the update for user / person into an atomic operation
            //
            // update the user values if the email and/or password has changed
            //var user = _userService.
            if (!(String.IsNullOrEmpty(person.NewPassword)) || (person.EmailAddress != person.OldEmail && person.OldEmail != null))
            {
                var authData = TranslationService.Login(person.OldEmail, person.OldPassword);

                if (authData == null)
                {
                    throw new Exception("Old password did not match profile");
                }
                else
                {
                    var userUpdateValues = person.GetUserUpdateValues();
                    userUpdateValues["User_ID"] = _userService.GetUserIdByUsername(person.OldEmail);
                    _userService.UpdateUser(userUpdateValues);
                }
            }
        }

        public Person GetPerson(int contactId)
        {
            var contact = _contactService.GetContactById(contactId);
            var person = Mapper.Map<Person>(contact);

            var family = _contactService.GetHouseholdFamilyMembers(person.HouseholdId);
            person.HouseholdMembers = family;

            // TODO: Should this move to _contactService or should update move it's call out to this service?
            var apiUser = _apiUserService.GetToken();
            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            var attributesTypes = _objectAttributeService.GetObjectAttributes(apiUser, contactId, configuration);
            person.AttributeTypes = attributesTypes.MultiSelect;
            person.SingleAttributes = attributesTypes.SingleSelect;

            return person;
        }

        public List<MpRoleDto> GetLoggedInUserRoles(string token)
        {
            return GetMyRecordsRepository.GetMyRoles(token);
        }

        public Person GetLoggedInUserProfile(String token)
        {
            var contact = _contactService.GetMyProfile(token);
            var person = Mapper.Map<Person>(contact);

            var family = _contactService.GetHouseholdFamilyMembers(person.HouseholdId);
            person.HouseholdMembers = family;

            return person;
        }
    }
}