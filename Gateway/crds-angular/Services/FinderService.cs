﻿using Amazon.CloudSearchDomain.Model;
using AutoMapper;
using crds_angular.Exceptions;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using log4net;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;

namespace crds_angular.Services
{
    public class FinderService : MinistryPlatformBaseService, IFinderService
    {
        private readonly IAddressGeocodingService _addressGeocodingService;
        private readonly IContactRepository _contactRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(FinderService));
        private readonly IFinderRepository _finderRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IAddressService _addressService;
        private readonly IGroupToolService _groupToolService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IGroupService _groupService;
        private readonly IInvitationService _invitationService;
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IAccountService _accountService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILookupService _lookupService;
        private readonly ILocationService _locationService;
        private readonly IAddressRepository _addressRepository;
        private readonly IFirestoreUpdateService _firestoreUpdateService;

        private readonly int _anywhereGroupType;
        private readonly int _memberRoleId;
        private readonly int _groupRoleLeaderId;
        private readonly int _anywhereGatheringInvitationTypeId;
        private readonly int _groupInvitationTypeId;
        private readonly int _domainId;
        private readonly string _finderConnect;
        private readonly string _finderGroupTool;
        private readonly int _inviteAcceptedTemplateId;
        private readonly int _inviteDeclinedTemplateId;
        private readonly int _anywhereCongregationId;
        private readonly int _spritualGrowthMinistryId;
        private readonly string _connectPersonPinUrl;
        private readonly string _connectSitePinUrl;
        private readonly string _connectGatheringPinUrl;
        private readonly string _connectSmallGroupPinUrl;
        private readonly int _smallGroupType;
        private readonly int _connectCommunicationTypeInviteToGathering;
        private readonly int _connectCommunicationTypeInviteToSmallGroup;
        private readonly int _connectCommunicationTypeRequestToJoinSmallGroup;
        private readonly int _connectCommunicationTypeRequestToJoinGathering;
        private readonly int _GroupsTryAGroupParticipantAcceptedNotificationTemplateId;
        private readonly int _GroupsTryAGroupParticipantDeclinedNotificationTemplateId;
        private readonly int _gatheringHostAcceptTemplate;
        private readonly int _gatheringHostDenyTemplate;
        private readonly int _connectGatheringRequestToJoin;
        private readonly int _sayHiWithMessageTemplateId;
        private readonly int _sayHiWithoutMessageTemplateId;

        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private const double MinutesInDegree = 60;
        private const double StatuteMilesInNauticalMile = 1.1515;

        private const int ADD_TO_MAP = 1;
        private const int REMOVE_FROM_MAP = 2;

        public FinderService(IAddressGeocodingService addressGeocodingService,
            IFinderRepository finderRepository,
            IContactRepository contactRepository,
            IAddressService addressService,
            IParticipantRepository participantRepository,
            IGroupRepository groupRepository,
            IGroupService groupService,
            IGroupToolService groupToolService,
            IConfigurationWrapper configurationWrapper,
            IInvitationService invitationService,
            IAwsCloudsearchService awsCloudsearchService,
            ICommunicationRepository communicationRepository,
            IAccountService accountService,
            ILookupService lookupService,
            IAnalyticsService analyticsService,
            ILocationService locationService,
            IAddressRepository addressRepository,
            IFirestoreUpdateService firestoreUpdateService)
        {
            // services
            _addressGeocodingService = addressGeocodingService;
            _finderRepository = finderRepository;
            _contactRepository = contactRepository;
            _addressService = addressService;
            _participantRepository = participantRepository;
            _groupService = groupService;
            _groupRepository = groupRepository;
            _groupToolService = groupToolService;
            _configurationWrapper = configurationWrapper;
            _invitationService = invitationService;
            _awsCloudsearchService = awsCloudsearchService;
            _communicationRepository = communicationRepository;
            _accountService = accountService;
            _lookupService = lookupService;
            _analyticsService = analyticsService;
            _locationService = locationService;
            _addressRepository = addressRepository;
            _firestoreUpdateService = firestoreUpdateService;
            // constants
            _anywhereCongregationId = _configurationWrapper.GetConfigIntValue("AnywhereCongregationId");
            configurationWrapper.GetConfigIntValue("ApprovedHostStatus");
            configurationWrapper.GetConfigIntValue("PendingHostStatus");
            _anywhereGroupType = configurationWrapper.GetConfigIntValue("AnywhereGroupTypeId");
            configurationWrapper.GetConfigIntValue("GroupsTrialMemberRoleId");
            _memberRoleId = configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
            _groupRoleLeaderId = configurationWrapper.GetConfigIntValue("GroupRoleLeader");
            _anywhereGatheringInvitationTypeId = configurationWrapper.GetConfigIntValue("AnywhereGatheringInvitationType");
            _groupInvitationTypeId = configurationWrapper.GetConfigIntValue("GroupInvitationType");
            _domainId = configurationWrapper.GetConfigIntValue("DomainId");
            _finderConnect = configurationWrapper.GetConfigValue("FinderConnectFlag");
            _finderGroupTool = configurationWrapper.GetConfigValue("FinderGroupToolFlag");
            _inviteAcceptedTemplateId = configurationWrapper.GetConfigIntValue("AnywhereGatheringInvitationAcceptedTemplateId");
            _inviteDeclinedTemplateId = configurationWrapper.GetConfigIntValue("AnywhereGatheringInvitationDeclinedTemplateId");
            _domainId = configurationWrapper.GetConfigIntValue("DomainId");
            _spritualGrowthMinistryId = _configurationWrapper.GetConfigIntValue("SpiritualGrowthMinistryId");
            _connectPersonPinUrl = _configurationWrapper.GetConfigValue("ConnectPersonPinUrl");
            _connectSitePinUrl = _configurationWrapper.GetConfigValue("ConnectSitePinUrl");
            _connectGatheringPinUrl = _configurationWrapper.GetConfigValue("ConnectGatheringPinUrl");
            _connectSmallGroupPinUrl = _configurationWrapper.GetConfigValue("ConnectSmallGroupPinUrl");
            _smallGroupType = _configurationWrapper.GetConfigIntValue("SmallGroupTypeId");
            _connectCommunicationTypeInviteToGathering = _configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeInviteToGathering");
            _connectCommunicationTypeInviteToSmallGroup = _configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeInviteToSmallGroup");
            _connectCommunicationTypeRequestToJoinSmallGroup = _configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeRequestToJoinSmallGroup");
            _connectCommunicationTypeRequestToJoinGathering = _configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeRequestToJoinGathering");
            _GroupsTryAGroupParticipantAcceptedNotificationTemplateId =
                _configurationWrapper.GetConfigIntValue("GroupsTryAGroupParticipantAcceptedNotificationTemplateId");
            _GroupsTryAGroupParticipantDeclinedNotificationTemplateId =
                _configurationWrapper.GetConfigIntValue("GroupsTryAGroupParticipantDeclinedNotificationTemplateId");
            _gatheringHostAcceptTemplate = configurationWrapper.GetConfigIntValue("GatheringHostAcceptTemplate");
            _gatheringHostDenyTemplate = configurationWrapper.GetConfigIntValue("GatheringHostDenyTemplate");
            _connectGatheringRequestToJoin = configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeRequestToJoinGathering");
            _sayHiWithMessageTemplateId = configurationWrapper.GetConfigIntValue("sayHiWithMessageTemplateId");
            _sayHiWithoutMessageTemplateId = configurationWrapper.GetConfigIntValue("sayHiWithoutMessageTemplateId");
        }

        public void UpdatePersonPhotoInFirebaseIfOnMap(int contactid)
        {
            if (IsUserOnMap(contactid))
            {
                try
                {
                    int participantid = GetParticipantIdFromContact(contactid);
                    _logger.Info($"FIRESTORE: UpdatePersonPhotoInFirebaseIfOnMap - Calling Delete");
                    _firestoreUpdateService.DeleteProfilePhotoFromFirestore(participantid);
                    _logger.Info($"FIRESTORE: UpdatePersonPhotoInFirebaseIfOnMap - Calling Send");
                    _firestoreUpdateService.SendProfilePhotoToFirestore(participantid);
                    _logger.Info($"FIRESTORE: UpdatePersonPhotoInFirebaseIfOnMap - Completing successfully");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _logger.Info($"FIRESTORE: UpdatePersonPhotoInFirebaseIfOnMap - {ex.Message}");
                }
            }
        }

        public MeDTO GetMe(int contactId)
        {
            var addr = GetPersonAddress(contactId);
            MpParticipant participant = _participantRepository.GetParticipant(contactId); // showon map comes from here
            var contact = _contactRepository.GetContactById(contactId);
            var medto = new MeDTO
            {
                Address = addr,
                CongregationId = contact.Congregation_ID,
                ShowOnMap = participant.ShowOnMap,
                ParticipantId = participant.ParticipantId
            };
            return medto;
        }

        public void SaveMe(int contactId, MeDTO medto)
        {
            try
            {
                // address
                var addressDTO = GetPersonAddress(contactId);
                addressDTO.AddressLine1 = medto.Address.AddressLine1;
                addressDTO.AddressLine2 = medto.Address.AddressLine2;
                addressDTO.City = medto.Address.City;
                addressDTO.State = medto.Address.State;
                addressDTO.PostalCode = medto.Address.PostalCode;
                //update the lat/lon
                GeoCoordinate originCoordsFromGoogle = _addressGeocodingService.GetGeoCoordinates(addressDTO);
                addressDTO.Latitude = originCoordsFromGoogle.Latitude;
                addressDTO.Longitude = originCoordsFromGoogle.Longitude;

                if (addressDTO.AddressID == null)
                {
                    var addressid = _addressRepository.Create(Mapper.Map<MpAddress>(addressDTO));
                    addressDTO.AddressID = addressid;
                }
                else
                {
                    _addressRepository.Update(Mapper.Map<MpAddress>(addressDTO));
                }

                // congregation
                var contact = _contactRepository.GetContactById(contactId);
                var household = new MpHousehold
                {
                    Address_ID = contact.Address_ID ?? addressDTO.AddressID,
                    Household_ID = contact.Household_ID,
                    Congregation_ID = medto.CongregationId,
                    Home_Phone = contact.Home_Phone
                };
                _contactRepository.UpdateHousehold(household);

                //show on map
                MpParticipant participant = _participantRepository.GetParticipant(contact.Contact_ID);

                if (medto.ShowOnMap == true)
                {
                    EnablePin(participant.ParticipantId);
                }
                else
                {
                    DisablePin(participant.ParticipantId);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public void SayHiToParticipant(int fromContactId, int toParticipantId, string message)
        {
            var to = _contactRepository.GetContactIdByParticipantId(toParticipantId);
            this.SayHi(fromContactId, to, message);
        }

        public List<int> GetAddressIdsWithNoGeoCode()
        {
            return _addressRepository.FindAddressIdsWithoutGeocode().Take(100).ToList();
        }

        public List<int> GetAddressIdsForMapParticipantWithNoGeoCode()
        {
            return _addressRepository.FindMapParticipantsAddressIdsWithoutGeocode().Take(100).ToList();
        }

        public PinDto GetPinDetailsForGroup(int groupId, GeoCoordinate originCoords)
        {
            var group = _groupService.GetGroupDetailsWithAttributes(groupId);
            return (ConvertGroupDTOToPinDTO(group));
        }

        public PersonDTO GetPerson(int participantId)
        {
            var contact = _contactRepository.GetContactByParticipantId(participantId);
            var person = new PersonDTO
            {
                Name = $"{contact.Nickname.ToUpper()} {contact.Last_Name[0].ToString().ToUpper()}",
                Location = $"{contact.City}, {contact.State}"
            };
            return person;
        }

        public PinDto GetPinDetailsForPerson(int participantId)
        {
            var pinDetails = Mapper.Map<PinDto>(_finderRepository.GetPinDetails(participantId));

            //make sure we have a lat/long
            if (pinDetails?.Address.Latitude != null && pinDetails.Address.Longitude != null)
            {
                _addressService.SetGeoCoordinates(pinDetails.Address);
                pinDetails.Address.AddressLine1 = "";
                pinDetails.Address.AddressLine2 = "";
            }

            pinDetails.PinType = PinType.PERSON;
            return pinDetails;
        }

        public void EnablePin(int participantId)
        {
            _finderRepository.EnablePin(participantId);
            _finderRepository.RecordPinHistory(participantId, ADD_TO_MAP);
        }

        public void DisablePin(int participantId)
        {
            _finderRepository.DisablePin(participantId);
            _finderRepository.RecordPinHistory(participantId, REMOVE_FROM_MAP);
        }

        public void SetShowOnMap(int participantId, Boolean showOnMap)
        {
            if (showOnMap)
            {
                _finderRepository.RecordPinHistory(participantId, ADD_TO_MAP);
            }
            else
            {
                _finderRepository.RecordPinHistory(participantId, REMOVE_FROM_MAP);
            }

            var dictionary = new Dictionary<string, object>()
            {
                {"Participant_ID", participantId },
                {"Show_On_Map", showOnMap }
            };

            _participantRepository.UpdateParticipant(dictionary);
        }

        public PinDto UpdateGathering(PinDto pin)
        {
            // Update coordinates
            var coordinates = _addressService.GetGeoLocationCascading(pin.Gathering.Address);

            pin.Gathering.Address.Latitude = coordinates.Latitude;
            pin.Gathering.Address.Longitude = coordinates.Longitude;
            pin.Gathering.GroupTypeId = _anywhereGroupType;
            // When staff manually updates the gathering group in MP to be public/available online (or any other edit),
            // the meeting_time in MP console is forced to a value (midnight)
            // then on gathering group edits in the app, the meeting_time pre-populated and getting a sql datetime overflow
            // Gatherings do not have meeting time, so set back to null
            pin.Gathering.MeetingTime = null;

            if (pin.ShouldUpdateHomeAddress)
            {
                var pinAddressId = pin.Address.AddressID;
                pin.Address = new AddressDTO
                {
                    AddressID = pinAddressId,
                    AddressLine1 = pin.Gathering.Address.AddressLine1,
                    AddressLine2 = pin.Gathering.Address.AddressLine2,
                    City = pin.Gathering.Address.City,
                    County = pin.Gathering.Address.County,
                    ForeignCountry = pin.Gathering.Address.ForeignCountry,
                    Latitude = pin.Gathering.Address.Latitude,
                    Longitude = pin.Gathering.Address.Longitude,
                    PostalCode = pin.Gathering.Address.PostalCode,
                    State = pin.Gathering.Address.State
                };
                this.UpdateHouseholdAddress(pin);
                pin.PinType = PinType.PERSON;
                _awsCloudsearchService.UploadNewPinToAws(pin);
                pin.PinType = PinType.GATHERING;
            }

            var gathering = Mapper.Map<FinderGatheringDto>(pin.Gathering);

            _finderRepository.UpdateGathering(gathering);

            return pin;
        }

        public void UpdateHouseholdAddress(PinDto pin)
        {
            var coords = _addressGeocodingService.GetGeoCoordinates(pin.Address);
            pin.Address.Longitude = coords.Longitude;
            pin.Address.Latitude = coords.Latitude;

            var contact = _contactRepository.GetContactById((int)pin.Contact_ID);

            var household = new MpHousehold();
            household.Household_ID = contact.Household_ID;
            household.Address_ID = pin.Address.AddressID;
            household.Congregation_ID = pin.congregationId;
            household.Home_Phone = contact.Home_Phone;

            _contactRepository.UpdateHousehold(household);

            var householdDictionary = (pin.Address.AddressID == null)
                ? new Dictionary<string, object> { { "Household_ID", pin.Household_ID } }
                : null;
            var address = Mapper.Map<MpAddress>(pin.Address);

            var addressDictionary = getDictionary(address);
            addressDictionary.Add("State/Region", addressDictionary["State"]);
            _contactRepository.UpdateHouseholdAddress((int)pin.Contact_ID, householdDictionary, addressDictionary);
        }

        public List<GroupParticipantDTO> GetParticipantsForGroup(int groupId)
        {
            return _groupService.GetGroupParticipantsWithoutAttributes(groupId);
        }

        public void TryAGroup(int contactId, int groupId)
        {
            var group = _groupService.GetGroupDetails(groupId);

            var commType = group.GroupTypeId == _smallGroupType ? _connectCommunicationTypeRequestToJoinSmallGroup : _connectCommunicationTypeRequestToJoinGathering;

            var connection = new ConnectCommunicationDto
            {
                CommunicationTypeId = commType,
                ToContactId = group.ContactId,
                FromContactId = contactId,
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusUnanswered"),
                GroupId = groupId
            };

            _groupToolService.SubmitInquiry(contactId, groupId, false);
            RecordCommunication(connection);
            SendTryAGroupEmailToLeader(contactId, group);
        }

        public int GetParticipantIdFromContact(int contactId)
        {
            var participant = _participantRepository.GetParticipant(contactId);
            return participant.ParticipantId;
        }

        public int GetLeaderParticipantIdFromGroup(int groupId)
        {
            var participantId = _groupService.GetPrimaryContactParticipantId(groupId);
            return participantId;
        }

        public bool DoesUserLeadSomeGroup(int contactId)
        {
            int participantId = _participantRepository.GetParticipant(contactId).ParticipantId;
            bool doesUserLeadSomeGroup = _groupRepository.GetDoesUserLeadSomeGroup(participantId);

            return doesUserLeadSomeGroup;
        }

        public bool IsUserOnMap(int contactid)
        {
            return _participantRepository.GetParticipant(contactid).ShowOnMap;
        }

        public List<PinDto> GetPinsInBoundingBox(GeoCoordinate originCoords, string userKeywordSearchString, AwsBoundingBox boundingBox, string finderType, int contactId, string filterSearchString)
        {
            userKeywordSearchString = userKeywordSearchString?.Replace("%27", "\\'");
            var queryString = "";
            var returnSize = _configurationWrapper.GetConfigIntValue("ConnectDefaultNumberOfPins");

            // new search string for AWS call based on the findertype, use pintype

            if (finderType.Equals(_finderConnect))
            {
                queryString = (String.IsNullOrEmpty(filterSearchString)) ? "(or pintype:3 pintype:2 pintype:1)" : filterSearchString;
            }
            else if (finderType.Equals(_finderGroupTool))
            {
                queryString = $"(and pintype:4 groupavailableonline:1 (or (prefix field=groupdescription '{userKeywordSearchString}') (prefix field=groupname '{userKeywordSearchString}') (prefix field=groupprimarycontactfirstname '{userKeywordSearchString}') (prefix field=groupprimarycontactlastname '{userKeywordSearchString}') groupname:'{userKeywordSearchString}' groupdescription:'{userKeywordSearchString}' groupprimarycontactfirstname:'{userKeywordSearchString}' groupprimarycontactlastname:'{userKeywordSearchString}') {filterSearchString})";
            }
            else
            {
                throw new Exception("No pin search performed - finder type not found");
            }

            var cloudReturn = _awsCloudsearchService.SearchConnectAwsCloudsearch(queryString, "_all_fields", returnSize, originCoords);
            var pins = ConvertFromAwsSearchResponse(cloudReturn);

            AddPinMetaData(pins, originCoords, contactId);
            AddAddressToSites(pins.Where(a => a.PinType == PinType.SITE).ToList());

            return pins;
        }

        public void AddUserDirectlyToGroup(User userBeingAdded, int groupid, int roleId, int leaderContactId)
        {

            //check to see if user exists in MP. Exclude Guest Giver and Deceased status
            var contactId = _contactRepository.GetActiveContactIdByEmail(userBeingAdded.email);
            if (contactId == 0)
            {
                userBeingAdded.password = System.Web.Security.Membership.GeneratePassword(25, 10);
                contactId = _accountService.RegisterPersonWithoutUserAccount(userBeingAdded);
            }

            var groupParticipant = _groupService.GetGroupParticipants(groupid, false).FirstOrDefault(p => p.ContactId == contactId);

            // groupParticipant == null then participant not in group
            if (groupParticipant == null)
            {
                SendEmailToAddedUser(leaderContactId, userBeingAdded, groupid);
                _groupService.addContactToGroup(groupid, contactId, roleId);
                //send leader email
                SendAddEmailToGroupLeaders(userBeingAdded, groupid);
            }
            else
            {
                throw new DuplicateGroupParticipantException($"Participant {groupParticipant.ParticipantId} already in group.");
            }
        }

        private void SendAddEmailToGroupLeaders(User user, int groupId)
        {
            var leaders = GetParticipantsForGroup(groupId).Where(w => w.GroupRoleId == _configurationWrapper.GetConfigIntValue("GroupRoleLeader"));

            var emailTemplateId = _configurationWrapper.GetConfigIntValue("GroupsAddParticipantLeaderEmailNotificationTemplateId");
            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
            var group = _groupService.GetGroupDetails(groupId);
            var meetingDay = _lookupService.GetMeetingDayFromId(group.MeetingDayId);
            var groupLocation = GetGroupAddress(groupId);
            var formatedMeetingTime = group.MeetingTime == null ? "Flexible time" : $"{DateTimeOffset.Parse(@group.MeetingTime).LocalDateTime:t}";
            var formatedMeetingDay = meetingDay ?? "Flexible day";
            var formatedMeetingFrequency = group.MeetingFrequencyID == null ? "Flexible frequency" : getMeetingFrequency((int)group.MeetingFrequencyID);
            var mergeData = new Dictionary<string, object>
            {
                {"Nickname", user.firstName},
                {"Lastname", user.lastName},
                {"Participant_Email", user.email},
                {"Group_Name", group.GroupName},
                {"Group_Meeting_Day",  formatedMeetingDay},
                {"Group_Meeting_Time", formatedMeetingTime},
                {"Group_Meeting_Frequency", formatedMeetingFrequency},
                {"Group_Meeting_Location", groupLocation == null || groupLocation.AddressLine1 == null ? "Online" : $"{groupLocation.AddressLine1}\n{groupLocation.AddressLine2}\n{groupLocation.City}\n{groupLocation.State}\n{groupLocation.PostalCode}" },
                {"Recipient_First_Name", " " }
            };

            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };

            var replyTo = new MpContact
            {
                ContactId = emailTemplate.ReplyToContactId,
                EmailAddress = emailTemplate.ReplyToEmailAddress
            };

            foreach (var leader in leaders)
            {
                mergeData["Recipient_First_Name"] = leader.NickName;
                var to = new List<MpContact>
                {
                    new MpContact
                    {
                        ContactId = leader.ContactId,
                        EmailAddress = leader.Email
                    }
                };

                var confirmation = new MpCommunication
                {
                    EmailBody = emailTemplate.Body,
                    EmailSubject = emailTemplate.Subject,
                    AuthorUserId = 5,
                    DomainId = _domainId,
                    FromContact = fromContact,
                    ReplyToContact = replyTo,
                    TemplateId = emailTemplateId,
                    ToContacts = to,
                    MergeData = mergeData
                };
                _communicationRepository.SendMessage(confirmation);
            }
        }

        private void MakeAllLatLongsUnique(List<PinDto> thePins)
        {

            var groupedMatchingLatitude = thePins
            .Where(w => w.Address.Latitude != null && w.Address.Longitude != null)
                .GroupBy(u => new { u.Address.Latitude, u.Address.Longitude })
                .Select(grp => grp.ToList())
                .ToList();

            foreach (var grouping in groupedMatchingLatitude.Where(x => x.Count > 1))
            {
                // each of these groups matches latitude, so we need to create slight differences
                double? newLat = 0.0;
                double? newLong = 0.0;
                foreach (var g in grouping)
                {
                    if (newLat.Equals(0.0))
                    {
                        newLat = g.Address.Latitude;
                        newLong = g.Address.Longitude;
                    }
                    else
                    {
                        newLat += .0001;
                        newLong -= .0001;

                        g.Address.Latitude = newLat;
                        g.Address.Longitude = newLong;
                    }
                }
            }
        }

        private string GetPinTitle(PinDto pin, int contactId = 0)
        {
            string titleString = "";
            var lastname = string.IsNullOrEmpty(pin.LastName) ? " " : pin.LastName[0].ToString();
            switch (pin.PinType)
            {
                case PinType.SITE:
                    titleString = $"Crossroads {RemoveSpecialCharacters(pin.SiteName ?? "")}";
                    break;
                case PinType.GATHERING:
                    titleString = $"{RemoveSpecialCharacters(pin.FirstName ?? "")} {RemoveSpecialCharacters(lastname ?? "")}";
                    break;
                case PinType.PERSON:
                    titleString = $"{RemoveSpecialCharacters(pin.FirstName ?? "")} {RemoveSpecialCharacters(lastname ?? "")}";
                    break;
                case PinType.SMALL_GROUP:
                    var groupName = RemoveSpecialCharacters(pin.Gathering.GroupName ?? "").Trim();
                    if (groupName.Length > 22)
                    {
                        groupName = RemoveSpecialCharacters(pin.Gathering.GroupName ?? "").Trim().Substring(0, 22);
                    }
                    titleString = $"{groupName}";
                    break;
            }

            return titleString;
        }

        private static string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string GetPinUrl(PinType pintype)
        {
            switch (pintype)
            {
                case PinType.GATHERING:
                    return _connectGatheringPinUrl;
                case PinType.SITE:
                    return _connectSitePinUrl;
                case PinType.PERSON:
                    return _connectPersonPinUrl;
                case PinType.SMALL_GROUP:
                    return _connectSmallGroupPinUrl;
                default:
                    return _connectPersonPinUrl;
            }
        }

        private PinDto ConvertGroupDTOToPinDTO(GroupDTO mpGroup)
        {

            var contact = _contactRepository.GetContactById(mpGroup.ContactId);

            var pin = new PinDto
            {
                Proximity = null,
                PinType = PinType.SMALL_GROUP,
                FirstName = contact.First_Name != null ? contact.First_Name : null,
                LastName = contact.Last_Name != null ? contact.Last_Name : null,
                SiteName = mpGroup.Congregation,
                Contact_ID = contact.Contact_ID,
                Household_ID = contact.Household_ID
            };

            if (mpGroup.Address != null)
            {
                pin.Address = new AddressDTO
                {
                    AddressID = mpGroup.Address.AddressID != null ? mpGroup.Address.AddressID : (int?)null,
                    City = mpGroup.Address.City != null ? mpGroup.Address.City : null,
                    State = mpGroup.Address.State != null ? mpGroup.Address.State : null,
                    PostalCode = mpGroup.Address.PostalCode != null ? mpGroup.Address.PostalCode : null,
                    Latitude = mpGroup.Address.Latitude != null ? mpGroup.Address.Latitude : (double?)null,
                    Longitude = mpGroup.Address.Longitude != null ? mpGroup.Address.Longitude : (double?)null,
                };
            }

            pin.Gathering = new FinderGroupDto
            {
                GroupId = mpGroup.GroupId,
                GroupName = mpGroup.GroupName ?? null,
                GroupDescription = mpGroup.GroupDescription ?? null,
                PrimaryContactEmail = mpGroup.PrimaryContactEmail ?? null,
                Address = pin.Address,
                ContactId = pin.Contact_ID.Value,
                MinistryId = _spritualGrowthMinistryId,
                KidsWelcome = mpGroup.KidsWelcome,
                MeetingDay = _lookupService.GetMeetingDayFromId(mpGroup.MeetingDayId),
                MeetingDayId = mpGroup.MeetingDayId,
                MeetingTime = mpGroup.MeetingTime == null ? "Flexible time" : String.Format("{0:t}", DateTimeOffset.Parse(mpGroup.MeetingTime).LocalDateTime),
                MeetingFrequency = mpGroup.MeetingFrequencyID == null ? "Flexible frequency" : getMeetingFrequency((int)mpGroup.MeetingFrequencyID),
                MeetingFrequencyID = mpGroup.MeetingFrequencyID,
                GroupType = GetGroupTypeFromAttribute(mpGroup.SingleAttributes),
                VirtualGroup = (bool)mpGroup.AvailableOnline && mpGroup.Address == null,
                PrimaryContactFirstName = contact.First_Name != null ? contact.First_Name : null,
                PrimaryContactLastName = contact.Last_Name != null ? contact.Last_Name : null,
                PrimaryContactCongregation = null,
                GroupAgesRangeList = GetStringListFromAttribute(mpGroup.AttributeTypes, 91),
                GroupCategoriesList = GetStringListFromAttribute(mpGroup.AttributeTypes, 90),
                AvailableOnline = mpGroup.AvailableOnline,
                StartDate = mpGroup.StartDate
            };
            var subCatList = GetStringListFromAttribute(mpGroup.AttributeTypes, 92);
            foreach (var subCat in subCatList)
            {
                pin.Gathering.GroupCategoriesList.Add(subCat);
            }
            pin.Gathering.GroupCategoriesList.Remove("Theme");
            return pin;
        }

        private string GetGroupTypeFromAttribute(Dictionary<int, ObjectSingleAttributeDTO> s)
        {
            var returnString = "";
            ObjectSingleAttributeDTO grouptype;
            if (s.TryGetValue(73, out grouptype) && grouptype.Value != null)
            {
                // grouptype is now equal to the value
                var x = grouptype.Value;
                returnString = x.Name;
            }
            return returnString;
        }

        private List<string> GetStringListFromAttribute(Dictionary<int, ObjectAttributeTypeDTO> t, int attributeId)
        {
            var itemList = new List<string>();
            ObjectAttributeTypeDTO attributes;
            if (t.TryGetValue(attributeId, out attributes))
            {
                foreach (var a in attributes.Attributes)
                {
                    if (a.Selected)
                    {
                        itemList.Add(a.Name);
                    }
                }
            }
            return itemList;
        }

        private List<PinDto> ConvertFromAwsSearchResponse(SearchResponse response)
        {
            var pins = new List<PinDto>();

            foreach (var hit in response.Hits.Hit)
            {
                var pin = new PinDto
                {
                    Proximity = hit.Fields.ContainsKey("proximity") ? Convert.ToDecimal(hit.Fields["proximity"].FirstOrDefault()) : (decimal?)null,
                    PinType = hit.Fields.ContainsKey("pintype") ? (PinType)Convert.ToInt32(hit.Fields["pintype"].FirstOrDefault()) : PinType.PERSON,
                    FirstName = hit.Fields.ContainsKey("firstname") ? hit.Fields["firstname"].FirstOrDefault() : null,
                    LastName = hit.Fields.ContainsKey("lastname") ? hit.Fields["lastname"].FirstOrDefault() : null,
                    SiteName = hit.Fields.ContainsKey("sitename") ? hit.Fields["sitename"].FirstOrDefault() : null,
                    Contact_ID = hit.Fields.ContainsKey("contactid") ? Convert.ToInt32(hit.Fields["contactid"].FirstOrDefault()) : (int?)null,
                    Participant_ID = hit.Fields.ContainsKey("participantid") ? Convert.ToInt32(hit.Fields["participantid"].FirstOrDefault()) : (int?)null,
                    Host_Status_ID = hit.Fields.ContainsKey("hoststatus") ? Convert.ToInt32(hit.Fields["hoststatus"].FirstOrDefault()) : (int?)null,
                    Household_ID = hit.Fields.ContainsKey("householdid") ? Convert.ToInt32(hit.Fields["householdid"].FirstOrDefault()) : (int?)null,
                    Address = new AddressDTO
                    {
                        AddressID = hit.Fields.ContainsKey("addressid") ? Convert.ToInt32(hit.Fields["addressid"].FirstOrDefault()) : (int?)null,
                        City = hit.Fields.ContainsKey("city") ? hit.Fields["city"].FirstOrDefault() : null,
                        State = hit.Fields.ContainsKey("state") ? hit.Fields["state"].FirstOrDefault() : null,
                        PostalCode = hit.Fields.ContainsKey("zip") ? hit.Fields["zip"].FirstOrDefault() : null,
                        Latitude = hit.Fields.ContainsKey("latitude") ? Convert.ToDouble(hit.Fields["latitude"].FirstOrDefault()) : (double?)null,
                        Longitude = hit.Fields.ContainsKey("longitude") ? Convert.ToDouble(hit.Fields["longitude"].FirstOrDefault()) : (double?)null,
                    }
                };
                if (hit.Fields.ContainsKey("latlong"))
                {
                    var locationstring = hit.Fields["latlong"].FirstOrDefault() ?? "";
                    var coordinates = locationstring.Split(',');
                    pin.Address.Latitude = Convert.ToDouble(coordinates[0]);
                    pin.Address.Longitude = Convert.ToDouble(coordinates[1]);
                }
                if (pin.PinType == PinType.GATHERING || pin.PinType == PinType.SMALL_GROUP)
                {
                    pin.Gathering = new FinderGroupDto
                    {
                        GroupId = hit.Fields.ContainsKey("groupid") ? Convert.ToInt32(hit.Fields["groupid"].FirstOrDefault()) : 0,
                        GroupName = hit.Fields.ContainsKey("groupname") ? hit.Fields["groupname"].FirstOrDefault() : null,
                        GroupDescription = hit.Fields.ContainsKey("groupdescription") ? hit.Fields["groupdescription"].FirstOrDefault() : null,
                        PrimaryContactEmail = hit.Fields.ContainsKey("primarycontactemail") ? hit.Fields["primarycontactemail"].FirstOrDefault() : null,
                        Address = pin.Address,
                        ContactId = pin.Contact_ID.Value,
                        GroupTypeId = _anywhereGroupType,
                        CongregationId = _anywhereCongregationId,
                        MinistryId = _spritualGrowthMinistryId,
                        KidsWelcome = hit.Fields.ContainsKey("groupkidswelcome") && hit.Fields["groupkidswelcome"].FirstOrDefault() == "1",
                        MeetingDay = hit.Fields.ContainsKey("groupmeetingday") ? hit.Fields["groupmeetingday"].FirstOrDefault() : null,
                        MeetingTime = hit.Fields.ContainsKey("groupmeetingtime") ? hit.Fields["groupmeetingtime"].FirstOrDefault() : null,
                        MeetingFrequency = hit.Fields.ContainsKey("groupmeetingfrequency") ? hit.Fields["groupmeetingfrequency"].FirstOrDefault() : null,
                        GroupType = hit.Fields.ContainsKey("grouptype") ? hit.Fields["grouptype"].FirstOrDefault() : null,
                        VirtualGroup = hit.Fields.ContainsKey("groupvirtual") && hit.Fields["groupvirtual"].FirstOrDefault() == "1",
                        PrimaryContactFirstName = hit.Fields.ContainsKey("groupprimarycontactfirstname") ? hit.Fields["groupprimarycontactfirstname"].FirstOrDefault() : null,
                        PrimaryContactLastName = hit.Fields.ContainsKey("groupprimarycontactlastname") ? hit.Fields["groupprimarycontactlastname"].FirstOrDefault() : null,
                        PrimaryContactCongregation = hit.Fields.ContainsKey("groupprimarycontactcongregation") ? hit.Fields["groupprimarycontactcongregation"].FirstOrDefault() : null,
                        GroupAgesRangeList = hit.Fields.ContainsKey("groupagerange") ? hit.Fields["groupagerange"].Where(s => !String.IsNullOrEmpty(s)).Select(x => x.Trim()).ToList() : null,
                        GroupCategoriesList = hit.Fields.ContainsKey("groupcategory") ? hit.Fields["groupcategory"].Where(s => !String.IsNullOrEmpty(s)).Select(x => x.Trim()).ToList() : null,
                        AvailableOnline = hit.Fields.ContainsKey("groupavailableonline") && hit.Fields["groupavailableonline"].FirstOrDefault() == "1",
                    };

                    if (hit.Fields.ContainsKey("groupstartdate") && !String.IsNullOrWhiteSpace(hit.Fields["groupstartdate"].First()))
                    {
                        DateTime? startDate = null;
                        startDate = Convert.ToDateTime(hit.Fields["groupstartdate"].First());
                        pin.Gathering.StartDate = (DateTime)startDate;
                    }

                }
                pins.Add(pin);
            }

            return pins;
        }

        private static decimal GetProximity(GeoCoordinate originCoords, GeoCoordinate pinCoords)
        {
            var proxval = Proximity(originCoords.Latitude, originCoords.Longitude, pinCoords.Latitude, pinCoords.Longitude);
            decimal retval = 0;

            try
            {
                retval = Convert.ToDecimal(proxval);
            }
            catch
            {
                retval = 0;
            }

            return retval;
        }

        private static double Proximity(double lat1, double lon1, double lat2, double lon2)
        {
            var theta = lon1 - lon2;
            var dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2Deg(dist);
            dist = dist * MinutesInDegree * StatuteMilesInNauticalMile;

            return (dist);
        }

        private static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public List<PinDto> GetMyPins(GeoCoordinate originCoords, int contactId, string finderType)
        {
            var pins = new List<PinDto>();
            var participantId = GetParticipantIdFromContact(contactId);

            int[] groupTypesToFetch = finderType == _finderConnect ? new int[] { _anywhereGroupType } : new int[] { _smallGroupType };

            var groupPins = GetMyGroupPins(groupTypesToFetch, participantId, finderType);
            var personPin = GetPinDetailsForPerson(participantId);

            pins.AddRange(groupPins);

            if (personPin != null && personPin.ShowOnMap && finderType == _finderConnect)
            {
                pins.Add(personPin);
            }

            foreach (var pin in pins)
            {
                //calculate proximity for all pins to origin
                if (pin.Address.Latitude == null) continue;
                if (pin.Address.Longitude != null) pin.Proximity = GetProximity(originCoords, new GeoCoordinate(pin.Address.Latitude.Value, pin.Address.Longitude.Value));
            }

            pins = this.AddPinMetaData(pins, originCoords, contactId);

            MakeAllLatLongsUnique(pins);

            return pins;
        }

        public List<PinDto> GetMyGroupPins(int[] groupTypeIds, int participantId, string finderType)
        {
            var groupsByType = _groupRepository.GetGroupsForParticipantByTypeOrID(participantId, null, groupTypeIds);

            if (groupsByType == null)
            {
                return null;
            }

            if (groupsByType.Count == 0)
            {
                return new List<PinDto>();
            }

            var cloudsearchQueryString = groupsByType.Aggregate("(or ", (current, @group) => current + ("groupid:" + @group.GroupId + " ")) + ")";
            // use the groups found to get full dataset from AWS
            var cloudReturn = _awsCloudsearchService.SearchConnectAwsCloudsearch(cloudsearchQueryString, "_all_fields");

            var pins = ConvertFromAwsSearchResponse(cloudReturn);

            return pins;
        }

        public GeoCoordinate GetGeoCoordsFromAddressOrLatLang(string address, GeoCoordinates centerCoords)
        {

            double latitude = centerCoords.Lat.HasValue ? centerCoords.Lat.Value : 0;
            double longitude = centerCoords.Lng.HasValue ? centerCoords.Lng.Value : 0;

            var geoCoordsPassedIn = latitude != 0 && longitude != 0;

            GeoCoordinate originCoordsFromGoogle = geoCoordsPassedIn ? null : _addressGeocodingService.GetGeoCoordinates(address);

            GeoCoordinate originCoordsFromClient = new GeoCoordinate(latitude, longitude);

            GeoCoordinate originCoordinates = geoCoordsPassedIn ? originCoordsFromClient : originCoordsFromGoogle;

            return originCoordinates;
        }

        public GeoCoordinate GetGeoCoordsFromLatLong(string lat, string lng)
        {
            var latitude = Convert.ToDouble(lat.Replace("$", "."));
            var longitude = Convert.ToDouble(lng.Replace("$", "."));

            return new GeoCoordinate(latitude, longitude);
        }

        public AddressDTO RandomizeLatLong(AddressDTO address)
        {
            if (!address.HasGeoCoordinates()) return address;
            var distance = _random.Next(75, 300); // up to a quarter mile
            var angle = _random.Next(0, 359);
            const int earthRadius = 6371000; // in meters

            var distanceNorth = Math.Sin(angle) * distance;
            var distanceEast = Math.Cos(angle) * distance;

            var newLat = (double)(address.Latitude + (distanceNorth / earthRadius) * 180 / Math.PI);
            var newLong = (double)(address.Longitude + (distanceEast / (earthRadius * Math.Cos(newLat * 180 / Math.PI))) * 180 / Math.PI);
            address.Latitude = newLat;
            address.Longitude = newLong;

            return address;
        }


        public Invitation InviteToGroup(int contactId, int gatheringId, User person, string finderFlag)
        {
            var inviteType = finderFlag.Equals(_finderConnect) ? _anywhereGatheringInvitationTypeId : _groupInvitationTypeId;

            var invitation = new Invitation
            {
                RecipientName = person.firstName,
                EmailAddress = person.email,
                SourceId = gatheringId,
                GroupRoleId = _memberRoleId,
                InvitationType = inviteType,
                RequestDate = DateTime.Now
            };

            _invitationService.ValidateInvitation(invitation, contactId);
            invitation = _invitationService.CreateInvitation(invitation, contactId);

            // TODO US8247 - Guest giver stuff - see story for info

            //if the invitee does not have a contact then create one
            int toContactId;

            try
            {
                toContactId = _contactRepository.GetContactIdByEmail(person.email);
            }
            catch (Exception e) //go ahead and create additional contact, becuase we don't know which contactId to use
            {
                _logger.Info($"Can't get specific contact_id,  '{person.email}', already has multiple contact records, create another, becuase don't know which one to pick", e);
                toContactId = _contactRepository.CreateContactForGuestGiver(person.email, $"{person.lastName}, {person.firstName}", person.firstName, person.lastName);
            }

            if (toContactId == 0)
            {
                toContactId = _contactRepository.CreateContactForGuestGiver(person.email, $"{person.lastName}, {person.firstName}", person.firstName, person.lastName);
            }

            var communicationType = finderFlag.Equals(_finderConnect) ? _connectCommunicationTypeInviteToGathering : _connectCommunicationTypeInviteToSmallGroup;

            var connection = new ConnectCommunicationDto
            {
                CommunicationTypeId = communicationType,
                ToContactId = toContactId,
                FromContactId = contactId,
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusUnanswered"),
                GroupId = gatheringId
            };

            RecordCommunication(connection);
            return invitation;
        }

        public AddressDTO GetGroupAddress(int groupId)
        {
            return _groupService.GetGroupDetails(groupId).Address;
        }

        public AddressDTO GetPersonAddress(int contactId, int participantId = -1, bool shouldGetFullAddress = true)
        {
            var user = _participantRepository.GetParticipant(contactId);

            if (participantId == -1)
            {
                participantId = user.ParticipantId;
            }

            if ((user.ParticipantId == participantId) || !shouldGetFullAddress)
            {
                var address = _finderRepository.GetPinAddress(participantId);

                if (address != null)
                {
                    if (!shouldGetFullAddress)
                    {
                        address.Address_Line_1 = null;
                        address.Address_Line_2 = null;
                    }
                    return Mapper.Map<AddressDTO>(address);
                }
                else
                {
                    throw new Exception("User address not found");
                }
            }
            else
            {
                throw new Exception("User does not have access to requested address");
            }
        }

        private void RecordCommunication(ConnectCommunicationDto connection)
        {
            _finderRepository.RecordConnection(Mapper.Map<MpConnectCommunication>(connection));
        }

        public void SayHi(int fromContactId, int toContactId, string message)
        {

            var from = _contactRepository.GetContactById(fromContactId);
            var to = _contactRepository.GetContactById(toContactId);

            SendSayHiEmail(from, to, message);

            var connection = new ConnectCommunicationDto
            {
                FromContactId = fromContactId,
                ToContactId = toContactId,
                CommunicationTypeId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationTypeSayHi"),
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusNA"),
                GroupId = null
            };
            RecordCommunication(connection);
        }

        public void AcceptDenyGroupInvitation(int contactId, int groupId, string invitationGuid, bool accept)
        {
            try
            {
                _groupToolService.AcceptDenyGroupInvitation(contactId, groupId, invitationGuid, accept);

                var host = GetPinDetailsForPerson(GetLeaderParticipantIdFromGroup(groupId));
                var cm = _contactRepository.GetContactById(contactId);

                var connection = new ConnectCommunicationDto
                {
                    FromContactId = cm.Contact_ID,
                    ToContactId = (int)host.Contact_ID,
                    CommunicationTypeId = _connectCommunicationTypeInviteToGathering,
                    CommunicationStatusId =
                        accept
                            ? _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusAccepted")
                            : _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusDeclined"),
                    GroupId = groupId
                };
                RecordCommunication(connection);

                SendGatheringInviteResponseEmail(accept, host, cm);

                if (accept)
                {
                    // Call Analytics
                    var props = new EventProperties { { "InvitationTo", cm.Contact_ID } };
                    _analyticsService.Track(host.Contact_ID.ToString(), "HostInvitationAccepted", props);

                    props = new EventProperties { { "InvitationFrom", host.Contact_ID } };
                    _analyticsService.Track(cm.Contact_ID.ToString(), "InviteeAcceptedInvitation", props);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SendSayHiEmail(MpMyContact from, MpMyContact to, String message)
        {
            try
            {
                // basic merge data here
                var mergeData = new Dictionary<string, object>
        {
          { "Community_Member_Name", from.Nickname + " " + from.Last_Name[0] + "." },
          { "Pin_First_Name", to.Nickname },
          { "Community_Member_Email", from.Email_Address},
          { "Community_Member_City", from.City },
          { "Community_Member_State", from.State },
          { "User_Message", message }
      };
                var emailTemplateId = (message != null && message != "") ? _sayHiWithMessageTemplateId : _sayHiWithoutMessageTemplateId;
                var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);

                var fromContact = new MpContact
                {
                    ContactId = emailTemplate.FromContactId,
                    EmailAddress = emailTemplate.FromEmailAddress
                };

                var replyTo = new MpContact
                {
                    ContactId = from.Contact_ID,
                    EmailAddress = from.Email_Address
                };

                var toContact = new List<MpContact>
                    {
                        new MpContact
                        {
                            // Just need a contact ID here, doesn't have to be for the recipient
                            ContactId = to.Contact_ID,
                            EmailAddress = to.Email_Address
                        }
                    };

                var sayHi = new MpCommunication
                {
                    EmailBody = emailTemplate.Body,
                    EmailSubject = emailTemplate.Subject,
                    AuthorUserId = 5,
                    DomainId = _domainId,
                    FromContact = fromContact,
                    ReplyToContact = replyTo,
                    TemplateId = emailTemplateId,
                    ToContacts = toContact,
                    MergeData = mergeData
                };
                _communicationRepository.SendMessage(sayHi);
            }
            catch (Exception e)
            {
                return;
            }
        }

        private void SendGatheringInviteResponseEmail(bool inviteAccepted, PinDto host, MpMyContact communityMember)
        {
            try
            {
                // basic merge data here
                var mergeData = new Dictionary<string, object>
                {
                    {"Community_Member", communityMember.Nickname + " " + communityMember.Last_Name},
                    {"Host", host.FirstName},
                };

                int emailTemplateId = inviteAccepted ? _inviteAcceptedTemplateId : _inviteDeclinedTemplateId;

                var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
                var fromContact = new MpContact
                {
                    ContactId = emailTemplate.FromContactId,
                    EmailAddress = emailTemplate.FromEmailAddress
                };
                var replyTo = new MpContact
                {
                    ContactId = emailTemplate.ReplyToContactId,
                    EmailAddress = emailTemplate.ReplyToEmailAddress
                };

                var to = new List<MpContact>
                {
                    new MpContact
                    {
                        // Just need a contact ID here, doesn't have to be for the recipient
                        ContactId = host.Contact_ID.Value
                    }
                };

                var confirmation = new MpCommunication
                {
                    EmailBody = emailTemplate.Body,
                    EmailSubject = emailTemplate.Subject,
                    AuthorUserId = 5,
                    DomainId = _domainId,
                    FromContact = fromContact,
                    ReplyToContact = replyTo,
                    TemplateId = emailTemplateId,
                    ToContacts = to,
                    MergeData = mergeData
                };
                _communicationRepository.SendMessage(confirmation);
            }
            catch (Exception e)
            {
                return;
            }
        }

        private string getMeetingFrequency(int meetingFrequencyId)
        {

            switch (meetingFrequencyId)
            {
                case 1:
                    return "Weekly";
                case 2:
                    return "Bi-Weekly";
                default:
                    return "Monthly";
            }
        }

        private void RecordConnectInteraction(int groupId, int fromContactId, int toContactId, int connectionType, int connectionStatus)
        {
            //only record anywhere group type interactions
            var group = _groupService.GetGroupDetails(groupId);
            if (group.GroupTypeId != _anywhereGroupType && group.GroupTypeId != _smallGroupType)
            {
                return;
            }

            var connection = new MpConnectCommunication
            {
                GroupId = groupId,
                FromContactId = fromContactId,
                ToContactId = toContactId,
                CommunicationTypeId = connectionType,
                CommunicationStatusId = connectionStatus
            };
            _finderRepository.RecordConnection(connection);
        }

        private Dictionary<string, object> GetEmailMergeData(int newUserContactId, GroupDTO group)
        {
            // group
            var groupLocation = GetGroupAddress(group.GroupId);
            var meetingDay = _lookupService.GetMeetingDayFromId(group.MeetingDayId);
            var formatedMeetingTime = group.MeetingTime == null ? "Flexible time" : $"{DateTimeOffset.Parse(@group.MeetingTime).LocalDateTime:t}";
            var formatedMeetingDay = meetingDay ?? "Flexible day";
            var formatedMeetingFrequency = group.MeetingFrequencyID == null ? "Flexible frequency" : getMeetingFrequency((int)group.MeetingFrequencyID);

            //leader
            var leaderContact = _contactRepository.GetContactByParticipantId(GetLeaderParticipantIdFromGroup(group.GroupId));

            //participant
            var newMember = _contactRepository.GetContactById(newUserContactId);
            var participant = _participantRepository.GetParticipant(newUserContactId);

            //URL
            var baseUrl = _configurationWrapper.GetConfigValue("BaseUrl");
            var groupToolPath = _configurationWrapper.GetConfigValue("GroupsTryAGroupPathFragment");

            var mergeData = new Dictionary<string, object>
            {
                {"YesURL", $"https://{baseUrl}{groupToolPath}/small-group/{group.GroupId}/true/{participant.ParticipantId}" },
                {"NoURL" , $"https://{baseUrl}{groupToolPath}/small-group/{group.GroupId}/false/{participant.ParticipantId}" },
                {"StartURL",   $"{baseUrl}{groupToolPath}/create-group" },
                {"SearchURL",   $"{baseUrl}{groupToolPath}" },
                {"Base_URL", baseUrl },
                {"Group_Tool_Path", groupToolPath },
                {"Participant_Name",  newMember.Nickname},
                {"Nickname", newMember.Nickname },
                {"Last_Name", newMember.Last_Name },
                {"Email_Address", newMember.Email_Address },
                {"Phone_Number", newMember.Mobile_Phone },
                {"Leader_Name", leaderContact.First_Name},
                {"Primary_First_Name", leaderContact.First_Name},
                {"Primary_Last_Name", leaderContact.Last_Name},
                {"Primary_Email", leaderContact.Email_Address},
                {"Primary_Phone", leaderContact.Mobile_Phone},
                {"Leader_Full_Name", $"{leaderContact.First_Name} {leaderContact.Last_Name}" },
                {"Leader_Email", leaderContact.Email_Address},
                {"Group_Name", group.GroupName},
                {"Group_ID", group.GroupId },
                {"Group_Meeting_Day",  formatedMeetingDay},
                {"Group_Meeting_Time", formatedMeetingTime},
                {"Group_Meeting_Frequency", formatedMeetingFrequency},
                {"Group_Meeting_Location", groupLocation == null || groupLocation.AddressLine1 == null ? "Online" : $"{groupLocation.AddressLine1}\n{groupLocation.AddressLine2}\n{groupLocation.City}\n{groupLocation.State}\n{groupLocation.PostalCode}" },
                {"Leader_Phone", $"{leaderContact.Home_Phone}\n{leaderContact.Mobile_Phone}" },
                {"State", (groupLocation != null) ? groupLocation.State : "" },
                {"City", (groupLocation != null) ? groupLocation.City : "" }
            };
            return mergeData;
        }

        private void SendTryAGroupEmailToLeader(int contactId, GroupDTO group)
        {
            try
            {
                var newMemberId = contactId;
                var mergeData = GetEmailMergeData(newMemberId, group);

                var emailTemplateId = _configurationWrapper.GetConfigIntValue("GroupRequestPendingReminderEmailTemplateId");

                var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
                var fromContact = new MpContact
                {
                    ContactId = emailTemplate.FromContactId,
                    EmailAddress = emailTemplate.FromEmailAddress
                };
                var replyTo = new MpContact
                {
                    ContactId = emailTemplate.ReplyToContactId,
                    EmailAddress = emailTemplate.ReplyToEmailAddress
                };

                var leaders = GetParticipantsForGroup(group.GroupId).Where(w => w.GroupRoleId == _configurationWrapper.GetConfigIntValue("GroupRoleLeader"));
                foreach (var leader in leaders)
                {
                    mergeData["Recipient_First_Name"] = leader.NickName;
                    var to = new List<MpContact>
                        {
                            new MpContact
                            {
                                ContactId = leader.ContactId,
                                EmailAddress = leader.Email
                            }
                        };

                    var confirmation = new MpCommunication
                    {
                        EmailBody = emailTemplate.Body,
                        EmailSubject = emailTemplate.Subject,
                        AuthorUserId = 5,
                        DomainId = _domainId,
                        FromContact = fromContact,
                        ReplyToContact = replyTo,
                        TemplateId = emailTemplateId,
                        ToContacts = to,
                        MergeData = mergeData
                    };
                    _communicationRepository.SendMessage(confirmation);
                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        private void SendEmailToAddedUser(int contactIdOfLeader, User user, int groupid)
        {
            var emailTemplateId = _configurationWrapper.GetConfigIntValue("GroupsAddParticipantEmailNotificationTemplateId");
            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
            var leaderContactId = contactIdOfLeader;
            var leaderContact = _contactRepository.GetContactById(leaderContactId);
            var leaderEmail = leaderContact.Email_Address;
            var userEmail = user.email;
            GroupDTO group = _groupService.GetGroupDetails(groupid);
            var meetingDay = _lookupService.GetMeetingDayFromId(group.MeetingDayId);
            var newMemberContactId = _contactRepository.GetActiveContactIdByEmail(user.email);
            var groupLocation = GetGroupAddress(groupid);
            var formatedMeetingTime = group.MeetingTime == null ? "Flexible time" : String.Format("{0:t}", DateTimeOffset.Parse(group.MeetingTime).LocalDateTime);
            var formatedMeetingDay = meetingDay == null ? "Flexible day" : meetingDay;
            var formatedMeetingFrequency = group.MeetingFrequencyID == null ? "Flexible frequency" : getMeetingFrequency((int)group.MeetingFrequencyID);
            var mergeData = new Dictionary<string, object>
            {
                {"Participant_Name", user.firstName},
                {"Leader_Name", leaderContact.First_Name},
                {"Leader_Full_Name", $"{leaderContact.First_Name} {leaderContact.Last_Name}" },
                {"Leader_Email", leaderEmail},
                {"Group_Name", group.GroupName},
                {"Group_Meeting_Day",  formatedMeetingDay},
                {"Group_Meeting_Time", formatedMeetingTime},
                {"Group_Meeting_Frequency", formatedMeetingFrequency},
                {"Group_Meeting_Location", groupLocation == null || groupLocation.AddressLine1 == null ? "Online" : $"{groupLocation.AddressLine1}\n{groupLocation.AddressLine2}\n{groupLocation.City}\n{groupLocation.State}\n{groupLocation.PostalCode}" },
                {"Leader_Phone", $"{leaderContact.Mobile_Phone}" }
            };

            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };

            var replyTo = new MpContact
            {
                ContactId = emailTemplate.ReplyToContactId,
                EmailAddress = emailTemplate.ReplyToEmailAddress
            };

            var to = new List<MpContact>
                {
                    new MpContact
                    {
                      ContactId = newMemberContactId,
                      EmailAddress = userEmail
                    }
                };

            var confirmation = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyTo,
                TemplateId = emailTemplateId,
                ToContacts = to,
                MergeData = mergeData
            };
            _communicationRepository.SendMessage(confirmation);
            var connection = new ConnectCommunicationDto
            {
                CommunicationTypeId = _connectCommunicationTypeInviteToSmallGroup,
                ToContactId = newMemberContactId,
                FromContactId = contactIdOfLeader,
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusNA"),
                GroupId = groupid,

            };

            RecordCommunication(connection);
        }

        public List<PinDto> AddAddressToSites(List<PinDto> pins)
        {
            // get locations
            var locationList = _locationService.GetAllCrossroadsLocations();
            foreach (var pin in pins)
            {
                pin.Address.AddressLine1 = locationList.Find(a => a.LocationName == pin.SiteName)?.Address.AddressLine1 ?? "";
            }

            return pins;
        }

        public List<PinDto> AddPinMetaData(List<PinDto> pins, GeoCoordinate originCoords, int contactId = 0)
        {
            try
            {
                foreach (var pin in pins)
                {
                    pin.Title = GetPinTitle(pin, contactId);
                    pin.IconUrl = GetPinUrl(pin.PinType);

                    //calculate proximity for all pins to origin
                    if (pin.Address.Latitude == null) continue;
                    if (pin.Address.Longitude != null && originCoords != null)
                    {
                        pin.Proximity = GetProximity(originCoords, new GeoCoordinate(pin.Address.Latitude.Value, pin.Address.Longitude.Value));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failure in AddPinMetaData", e);
            }

            return pins;
        }

        public bool areAllBoundingBoxParamsPresent(MapBoundingBox boundingBox)
        {
            var isUpperLeftLatNull = boundingBox.UpperLeftLat == null;
            var isUpperLeftLngNull = boundingBox.UpperLeftLng == null;
            var isBottomRightLatNull = boundingBox.BottomRightLat == null;
            var isBottomRightLngNull = boundingBox.BottomRightLng == null;
            var areAllBoundingBoxParamsPresent = !isUpperLeftLatNull && !isUpperLeftLngNull && !isBottomRightLatNull && !isBottomRightLngNull;

            return areAllBoundingBoxParamsPresent;
        }

        public List<PinDto> RandomizeLatLongForNonSitePins(List<PinDto> pins)
        {
            foreach (var pin in pins)
            {
                if (pin.PinType != PinType.SITE)
                {
                    pin.Address = RandomizeLatLong(pin.Address);
                }
            }

            return pins;
        }

        public GeoCoordinate GetMapCenterForResults(string userSearchString, GeoCoordinates frontEndMapCenter, string finderType)
        {
            GeoCoordinate resultMapCenterCoords = new GeoCoordinate();

            if (finderType == _finderConnect)
            {
                resultMapCenterCoords = GetGeoCoordsFromAddressOrLatLang(userSearchString, frontEndMapCenter);
            }
            else
            {
                if (frontEndMapCenter.Lat.HasValue && frontEndMapCenter.Lng.HasValue)
                {
                    resultMapCenterCoords = new GeoCoordinate(frontEndMapCenter.Lat.Value, frontEndMapCenter.Lng.Value);
                }
                else
                {
                    resultMapCenterCoords = GetGeoCoordsFromAddressOrLatLang(userSearchString, frontEndMapCenter);
                }
            }

            return resultMapCenterCoords;
        }

        public bool DoesActiveContactExists(string email)
        {
            var contactId = _contactRepository.GetActiveContactIdByEmail(email);
            return contactId != 0;
        }

        public void ApproveDenyGroupInquiry(bool approve, Inquiry inquiry)
        {
            try
            {
                if (
                _groupRepository.GetGroupParticipants(inquiry.GroupId, true)
                        .Exists(p => p.ContactId == inquiry.ContactId) || inquiry.Placed != null)
                {
                    // Update the inquiry
                    _groupRepository.UpdateGroupInquiry(inquiry.GroupId, inquiry.InquiryId, approve);
                    var e = new DuplicateGroupParticipantException("User is already a group member");
                    throw e;
                }
                var group = _groupService.GetGroupDetails(inquiry.GroupId);
                var participant = _participantRepository.GetParticipant(inquiry.ContactId);
                ApproveOrDenyGroupInquiry(inquiry, group, participant, approve);

                // Record Analytics
                var leader =
                    ((List<GroupParticipantDTO>)group.Participants).FirstOrDefault(
                        p => p.GroupRoleId == _groupRoleLeaderId);
                var props = new EventProperties
                {
                    {"GroupLeaderName", leader?.DisplayName},
                    {"GroupName", group.GroupName},
                    {"GroupCity", group?.Address?.City},
                    {"GroupState", group?.Address?.State},
                    {"GroupZip", group?.Address?.PostalCode}
                };
                var eventName = approve ? "AcceptedIntoGroup" : "DeniedIntoGroup";
                _analyticsService.Track(inquiry.ContactId.ToString(), eventName, props);
            }
            catch (GroupParticipantRemovalException)
            {
                throw;
            }
            catch (DuplicateGroupParticipantException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException($"Could not add Inquirer {inquiry.InquiryId} from group {inquiry.GroupId}", e);
            }
        }

        private void ApproveOrDenyGroupInquiry(Inquiry inquiry, GroupDTO group, MpParticipant participant, bool approve)
        {
            if (approve)
            {
                _groupService.addContactToGroup(group.GroupId, inquiry.ContactId, _memberRoleId);
            }
            // Update the inquiry
            _groupRepository.UpdateGroupInquiry(group.GroupId, inquiry.InquiryId, approve);

            // Record in connections table if this is an anywhere gathering.
            var commType = group.GroupTypeId == _smallGroupType ? _connectCommunicationTypeRequestToJoinSmallGroup : _connectGatheringRequestToJoin;
            RecordConnectInteraction(group.GroupId, participant.ContactId, inquiry.ContactId, commType, (approve) ? 1 : 2);

            // Send the email
            SendInquiryAcceptDenyEmail(group, approve, participant);

        }

        private void SendInquiryAcceptDenyEmail(GroupDTO group, bool approve, MpParticipant participant)
        {
            var emailTemplateId = 0;

            if (group.GroupTypeId == _smallGroupType)
            {
                emailTemplateId = approve
                    ? _GroupsTryAGroupParticipantAcceptedNotificationTemplateId
                    : _GroupsTryAGroupParticipantDeclinedNotificationTemplateId;
            }
            else if (group.GroupTypeId == _anywhereGroupType)
            {
                emailTemplateId = approve ? _gatheringHostAcceptTemplate : _gatheringHostDenyTemplate;
            }
            else
            {
                throw new Exception($"No email template defined for {group.GroupTypeId}");
            }

            var mergeData = GetEmailMergeData(participant.ContactId, group);

            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
            var fromContact = new MpContact
            {
                ContactId = emailTemplate.FromContactId,
                EmailAddress = emailTemplate.FromEmailAddress
            };
            var replyTo = new MpContact
            {
                ContactId = emailTemplate.ReplyToContactId,
                EmailAddress = emailTemplate.ReplyToEmailAddress
            };

            var toContact = _contactRepository.GetContactById(participant.ContactId);
            var to = new List<MpContact>
                {
                    new MpContact
                    {
                        ContactId = toContact.Contact_ID,
                        EmailAddress = toContact.Email_Address
                    }
                };

            var confirmation = new MpCommunication
            {
                EmailBody = emailTemplate.Body,
                EmailSubject = emailTemplate.Subject,
                AuthorUserId = 5,
                DomainId = _domainId,
                FromContact = fromContact,
                ReplyToContact = replyTo,
                TemplateId = emailTemplateId,
                ToContacts = to,
                MergeData = mergeData
            };
            _communicationRepository.SendMessage(confirmation);

        }

        public void TryAGroupAcceptDeny(int groupId, int participantId, bool accept)
        {
            var contactId = _contactRepository.GetContactIdByParticipantId(participantId);
            var inquiry = _groupToolService.GetGroupInquiryForContactId(groupId, contactId);
            //accept or deny the inquiry
            ApproveDenyGroupInquiry(accept, inquiry);
        }

        public List<MyDTO> GetMyListForPinType(int contactId, int pintypeId)
        {
            var myList = new List<MyDTO>();
            var participantId = GetParticipantIdFromContact(contactId);
            switch (pintypeId)
            {
                case PinTypeConstants.PIN_PERSON:
                    // return the participant
                    myList.Add(new MyDTO { InternalId = participantId, PinTypeId = pintypeId });
                    break;
                case PinTypeConstants.PIN_GROUP:
                    myList = GetMyListForGroup(participantId, pintypeId);
                    break;
                case PinTypeConstants.PIN_SITE:
                    // what do we return here
                    break;
                case PinTypeConstants.PIN_ONLINEGROUP:
                    myList = GetMyListForGroup(participantId, pintypeId);
                    break;
            }
            return myList;
        }

        private List<MyDTO> GetMyListForGroup(int participantId, int pintypeId)
        {
            var myDtoList = new List<MyDTO>();

            var groupsByType = _groupRepository.GetGroupsForParticipantByTypeOrID(participantId, null, new int[] { 1 });

            if (groupsByType == null)
            {
                return myDtoList;
            }

            if (groupsByType.Count == 0)
            {
                return myDtoList;
            }

            switch (pintypeId)
            {
                case PinTypeConstants.PIN_GROUP:
                    // in home groups
                    groupsByType = groupsByType.Where(c => (c.Address != null && c.Address.Address_ID != 0)).ToList();
                    break;

                case PinTypeConstants.PIN_ONLINEGROUP:
                    // online groups
                    groupsByType = groupsByType.Where(c => (c.Address == null || c.Address.Address_ID == 0)).ToList();
                    break;
            }

            foreach (MpGroup g in groupsByType)
            {
                myDtoList.Add(new MyDTO { InternalId = g.GroupId, PinTypeId = pintypeId });
            }
            return myDtoList;
        }
    }
}
