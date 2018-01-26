using Amazon.CloudSearchDomain.Model;
using AutoMapper;
using crds_angular.Exceptions;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using log4net;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;

namespace crds_angular.Services
{

    public class RemoteAddress
    {
        public string Ip { get; set; }
        public string region_code { get; set; }
        public string city { get; set; }
        public string zip_code { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

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
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IInvitationService _invitationService;
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IAccountService _accountService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILookupService _lookupService;
        private readonly int _approvedHost;
        private readonly int _pendingHost;
        private readonly int _anywhereGroupType;
        private readonly int _trialMemberRoleId;
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

        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private const double MinutesInDegree = 60;
        private const double StatuteMilesInNauticalMile = 1.1515;

        public FinderService(
            IAddressGeocodingService addressGeocodingService,
            IFinderRepository finderRepository,
            IContactRepository contactRepository,
            IAddressService addressService,
            IParticipantRepository participantRepository,
            IGroupRepository groupRepository,
            IGroupService groupService,
            IGroupToolService groupToolService,
            IApiUserRepository apiUserRepository,
            IConfigurationWrapper configurationWrapper,
            IInvitationService invitationService,
            IAwsCloudsearchService awsCloudsearchService,
            IAuthenticationRepository authenticationRepository,
            ICommunicationRepository communicationRepository,
            IAccountService accountService,
            ILookupService lookupService,
            IAnalyticsService analyticsService
        )
        {
            // services
            _addressGeocodingService = addressGeocodingService;
            _finderRepository = finderRepository;
            _contactRepository = contactRepository;
            _addressService = addressService;
            _participantRepository = participantRepository;
            _groupService = groupService;
            _groupRepository = groupRepository;
            _apiUserRepository = apiUserRepository;
            _authenticationRepository = authenticationRepository;
            _groupToolService = groupToolService;
            _configurationWrapper = configurationWrapper;
            _invitationService = invitationService;
            _awsCloudsearchService = awsCloudsearchService;
            _communicationRepository = communicationRepository;
            _accountService = accountService;
            _lookupService = lookupService;
            _analyticsService = analyticsService;
            // constants
            _anywhereCongregationId = _configurationWrapper.GetConfigIntValue("AnywhereCongregationId");
            _approvedHost = configurationWrapper.GetConfigIntValue("ApprovedHostStatus");
            _pendingHost = configurationWrapper.GetConfigIntValue("PendingHostStatus");
            _anywhereGroupType = configurationWrapper.GetConfigIntValue("AnywhereGroupTypeId");
            _trialMemberRoleId = configurationWrapper.GetConfigIntValue("GroupsTrialMemberRoleId");
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
        }

        public PinDto GetPinDetailsForGroup(int groupId, GeoCoordinate originCoords)
        {
            List<PinDto> pins = null;
            var cloudReturn = _awsCloudsearchService.SearchByGroupId(groupId.ToString());

            pins = ConvertFromAwsSearchResponse(cloudReturn);
            this.AddPinMetaData(pins, originCoords);

            return pins.First();

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
        }

        public void DisablePin(int participantId)
        {
            _finderRepository.DisablePin(participantId);
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

            var householdDictionary = (pin.Address.AddressID == null)
                ? new Dictionary<string, object> {{"Household_ID", pin.Household_ID}}
                : null;
            var address = Mapper.Map<MpAddress>(pin.Address);

            var addressDictionary = getDictionary(address);
            addressDictionary.Add("State/Region", addressDictionary["State"]);
            _contactRepository.UpdateHouseholdAddress((int) pin.Contact_ID, householdDictionary, addressDictionary);
        }

        public List<GroupParticipantDTO> GetParticipantsForGroup(int groupId)
        {
            return _groupService.GetGroupParticipantsWithoutAttributes(groupId);
        }

        private void GatheringValidityCheck(int contactId, AddressDTO address)
        {
            //get the list of anywhere groups

            var groups = _groupRepository.GetGroupsByGroupType(_anywhereGroupType);

            //get groups that this user is the primary contact for at this address
            var matchingGroupsCount = groups
                .Where(x => x.PrimaryContact == contactId.ToString())
                .Where(x => x.Address.Address_Line_1 == address.AddressLine1)
                .Where(x => x.Address.City == address.City).Count(x => x.Address.State == address.State);

            if (matchingGroupsCount > 0)
            {
                throw new GatheringException(contactId);
            }
        }

        public void RequestToBeHost(string token, HostRequestDto hostRequest)
        {
            //check if they are already a host at this address. If they are then throw
            GatheringValidityCheck(hostRequest.ContactId, hostRequest.Address);

            // get contact data
            var contact = _contactRepository.GetContactById(hostRequest.ContactId);
            var participant = _participantRepository.GetParticipant(hostRequest.ContactId);

            if (participant.HostStatus != _approvedHost)
            {
                participant.HostStatus = _pendingHost;
                _participantRepository.UpdateParticipantHostStatus(participant);
            }

            //update mobile phone number on contact record
            contact.Mobile_Phone = hostRequest.ContactNumber;
            var updateToDictionary = new Dictionary<string, object>
            {
                {"Contact_ID", hostRequest.ContactId},
                {"Mobile_Phone", hostRequest.ContactNumber},
                {"First_Name", contact.First_Name}
            };
            _contactRepository.UpdateContact(hostRequest.ContactId, updateToDictionary);

            // create the address for the group
            var hostAddressId = _addressService.CreateAddress(hostRequest.Address);
            hostRequest.Address.AddressID = hostAddressId;

            // create the group
            var group = new GroupDTO();
            group.GroupName = contact.Nickname + " " + contact.Last_Name[0];
            group.GroupDescription = hostRequest.GroupDescription;
            group.ContactId = hostRequest.ContactId;
            group.PrimaryContactEmail = contact.Email_Address;
            group.PrimaryContactName = contact.Nickname + " " + contact.Last_Name;
            group.Address = hostRequest.Address;
            group.StartDate = DateTime.Now;
            group.AvailableOnline = false;
            group.GroupFullInd = false;
            group.ChildCareAvailable = false;
            group.CongregationId = _anywhereCongregationId;
            group.GroupTypeId = _anywhereGroupType;
            group.MinistryId = _spritualGrowthMinistryId;
            group.MeetingTime = null;

            _groupService.CreateGroup(group);

            //add our contact to the group as a leader
            var participantSignup = new ParticipantSignup
            {
                particpantId = participant.ParticipantId,
                groupRoleId = _configurationWrapper.GetConfigIntValue("GroupRoleLeader")
            };
            _groupService.addParticipantToGroupNoEvents(group.GroupId, participantSignup);

            //check if we also need to update the home address
            if (!hostRequest.IsHomeAddress) return;
            var addressId = _addressService.CreateAddress(hostRequest.Address);
            // assign new id to users household
            _contactRepository.SetHouseholdAddress(hostRequest.ContactId, contact.Household_ID, addressId);
        }

        public void GatheringJoinRequest(string token, int gatheringId)
        {
            var group = _groupService.GetGroupDetails(gatheringId);

            var commType = group.GroupTypeId == _smallGroupType ? _connectCommunicationTypeRequestToJoinSmallGroup : _connectCommunicationTypeRequestToJoinGathering;

            var connection = new ConnectCommunicationDto
            {
                CommunicationTypeId = commType,
                ToContactId = group.ContactId,
                FromContactId = _contactRepository.GetContactId(token),
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusUnanswered"),
                GroupId = gatheringId
            };
            RecordCommunication(connection);

            _groupToolService.SubmitInquiry(token, gatheringId, true);
        }

        public void TryAGroup(string token, int groupId)
        {
            var group = _groupService.GetGroupDetails(groupId);

            var commType = group.GroupTypeId == _smallGroupType ? _connectCommunicationTypeRequestToJoinSmallGroup : _connectCommunicationTypeRequestToJoinGathering;

            var connection = new ConnectCommunicationDto
            {
                CommunicationTypeId = commType,
                ToContactId = group.ContactId,
                FromContactId = _contactRepository.GetContactId(token),
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusUnanswered"),
                GroupId = groupId
            };
            
            _groupToolService.SubmitInquiry(token, groupId, false);
            RecordCommunication(connection);
            SendTryAGroupEmailToLeader(token, group);
        }

        public AddressDTO GetAddressForIp(string ip)
        {
            var address = new AddressDTO();
            var request = WebRequest.Create("http://freegeoip.net/json/" + ip);
            using (var response = request.GetResponse())
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                var responseString = stream.ReadToEnd();
                var s = JsonConvert.DeserializeObject<RemoteAddress>(responseString);
                address.City = s.city;
                address.State = s.region_code;
                address.PostalCode = s.zip_code;
                address.Latitude = s.latitude;
                address.Longitude = s.longitude;
            }
            return address;
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

            var cloudReturn = _awsCloudsearchService.SearchConnectAwsCloudsearch(queryString, "_all_fields",returnSize, originCoords);
            var pins = ConvertFromAwsSearchResponse(cloudReturn);

            AddPinMetaData(pins, originCoords, contactId);

            return pins;
        }

        public void AddUserDirectlyToGroup(string token, User user, int groupid, int roleId)
        {
            
            //check to see if user exists in MP. Exclude Guest Giver and Deceased status
            var contactId = _contactRepository.GetActiveContactIdByEmail(user.email);
            if (contactId == 0)
            {
                user.password = System.Web.Security.Membership.GeneratePassword(25, 10);
                contactId = _accountService.RegisterPersonWithoutUserAccount(user);
            }
            
            var groupParticipant = _groupService.GetGroupParticipants(groupid, false).FirstOrDefault(p => p.ContactId == contactId);

            // groupParticipant == null then participant not in group
            if (groupParticipant == null)
            {
                SendEmailToAddedUser(token, user, groupid);
                _groupService.addContactToGroup(groupid, contactId, roleId);
                //send leader email
                SendAddEmailToGroupLeaders(user, groupid);
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
                    .GroupBy(u => new {u.Address.Latitude, u.Address.Longitude})
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
                        newLat  = g.Address.Latitude;
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

        private bool isMyPin(PinDto pin, int contactId)
        {
            var isMyPin = pin.Contact_ID == contactId && contactId != 0; 
            return isMyPin; 
        }

        private string isMyPinAsString(PinDto pin, int contactId)
        {
            string isMyPinString = isMyPin(pin, contactId).ToString().ToLower();
            return isMyPinString; 
        }

        private string GetPinTitle(PinDto pin, int contactId = 0)
        {
            string jsonData = "";
            var lastname = string.IsNullOrEmpty(pin.LastName) ? " " : pin.LastName[0].ToString();
            switch (pin.PinType)
            {
                case PinType.SITE:
                    jsonData = $"{{ 'siteName': '{RemoveSpecialCharacters(pin.SiteName)}','isHost':  false,'isMe': false,'pinType': {(int) pin.PinType}}}";
                    break;
                case PinType.GATHERING:
                    jsonData = $"{{ 'firstName': '{RemoveSpecialCharacters(pin.FirstName)}', 'lastInitial': '{RemoveSpecialCharacters(lastname)}','isHost':  true,'isMe': false,'pinType': {(int) pin.PinType}}}";
                    break;
                case PinType.PERSON:
                    jsonData = $"{{ 'firstName': '{RemoveSpecialCharacters(pin.FirstName)}', 'lastInitial': '{RemoveSpecialCharacters(lastname)}','isHost':  false,'isMe': {isMyPinAsString(pin, contactId)},'pinType': {(int) pin.PinType}}}";
                    break;
                case PinType.SMALL_GROUP:
                    var groupName = RemoveSpecialCharacters(pin.Gathering.GroupName).Trim();
                    if (groupName.Length > 22)
                    {
                        groupName = RemoveSpecialCharacters(pin.Gathering.GroupName).Trim().Substring(0, 22);
                    }
                    jsonData = $"{{ 'firstName': '{groupName}', 'lastInitial': '','isHost':  false,'isMe': false,'pinType': {(int)pin.PinType}}}";
                    break;
            }

            return jsonData.Replace("'", "\"");
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

        private List<PinDto> ConvertFromAwsSearchResponse(SearchResponse response)
        {
            var pins = new List<PinDto>();

            foreach (var hit in response.Hits.Hit)
            {
                var pin = new PinDto
                {
                    Proximity = hit.Fields.ContainsKey("proximity") ? Convert.ToDecimal(hit.Fields["proximity"].FirstOrDefault()) : (decimal?) null,
                    PinType = hit.Fields.ContainsKey("pintype") ? (PinType) Convert.ToInt32(hit.Fields["pintype"].FirstOrDefault()) : PinType.PERSON,
                    FirstName = hit.Fields.ContainsKey("firstname") ? hit.Fields["firstname"].FirstOrDefault() : null,
                    LastName = hit.Fields.ContainsKey("lastname") ? hit.Fields["lastname"].FirstOrDefault() : null,
                    SiteName = hit.Fields.ContainsKey("sitename") ? hit.Fields["sitename"].FirstOrDefault() : null,
                    EmailAddress = hit.Fields.ContainsKey("emailaddress") ? hit.Fields["emailaddress"].FirstOrDefault() : null,
                    Contact_ID = hit.Fields.ContainsKey("contactid") ? Convert.ToInt32(hit.Fields["contactid"].FirstOrDefault()) : (int?) null,
                    Participant_ID = hit.Fields.ContainsKey("participantid") ? Convert.ToInt32(hit.Fields["participantid"].FirstOrDefault()) : (int?) null,
                    Host_Status_ID = hit.Fields.ContainsKey("hoststatus") ? Convert.ToInt32(hit.Fields["hoststatus"].FirstOrDefault()) : (int?) null,
                    Household_ID = hit.Fields.ContainsKey("householdid") ? Convert.ToInt32(hit.Fields["householdid"].FirstOrDefault()) : (int?) null,
                    Address = new AddressDTO
                    {
                        AddressID = hit.Fields.ContainsKey("addressid") ? Convert.ToInt32(hit.Fields["addressid"].FirstOrDefault()) : (int?) null,
                        City = hit.Fields.ContainsKey("city") ? hit.Fields["city"].FirstOrDefault() : null,
                        State = hit.Fields.ContainsKey("state") ? hit.Fields["state"].FirstOrDefault() : null,
                        PostalCode = hit.Fields.ContainsKey("zip") ? hit.Fields["zip"].FirstOrDefault() : null,
                        Latitude = hit.Fields.ContainsKey("latitude") ? Convert.ToDouble(hit.Fields["latitude"].FirstOrDefault()) : (double?) null,
                        Longitude = hit.Fields.ContainsKey("longitude") ? Convert.ToDouble(hit.Fields["longitude"].FirstOrDefault()) : (double?) null,
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
                        VirtualGroup = hit.Fields.ContainsKey("groupvirtual") && hit.Fields["groupvirtual"].FirstOrDefault()== "1",
                        PrimaryContactFirstName = hit.Fields.ContainsKey("groupprimarycontactfirstname") ? hit.Fields["groupprimarycontactfirstname"].FirstOrDefault() : null,
                        PrimaryContactLastName = hit.Fields.ContainsKey("groupprimarycontactlastname") ? hit.Fields["groupprimarycontactlastname"].FirstOrDefault() : null,
                        PrimaryContactCongregation = hit.Fields.ContainsKey("groupprimarycontactcongregation") ? hit.Fields["groupprimarycontactcongregation"].FirstOrDefault() : null,
                        GroupAgesRangeList = hit.Fields.ContainsKey("groupagerange") ? hit.Fields["groupagerange"].ToList() : null,
                        GroupCategoriesList = hit.Fields.ContainsKey("groupcategory") ? hit.Fields["groupcategory"].ToList() : null,
                        AvailableOnline = hit.Fields.ContainsKey("groupavailableonline") && hit.Fields["groupavailableonline"].FirstOrDefault() == "1",                            
                    };

                    if (hit.Fields.ContainsKey("groupstartdate") && !String.IsNullOrWhiteSpace(hit.Fields["groupstartdate"].First()))
                    {
                        DateTime? startDate = null;
                        startDate = Convert.ToDateTime(hit.Fields["groupstartdate"].First());
                        pin.Gathering.StartDate = (DateTime) startDate;
                    }

                }
                pins.Add(pin);
            }

            return pins;
        }

        private static decimal GetProximity(GeoCoordinate originCoords, GeoCoordinate pinCoords)
        {
            return (decimal) Proximity(originCoords.Latitude, originCoords.Longitude, pinCoords.Latitude, pinCoords.Longitude);
        }

        private static double Proximity(double lat1, double lon1, double lat2, double lon2)
        {
            var theta = lon1 - lon2;
            var dist = Math.Sin(Deg2Rad(lat1))*Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1))*Math.Cos(Deg2Rad(lat2))*Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2Deg(dist);
            dist = dist* MinutesInDegree * StatuteMilesInNauticalMile;

            return (dist);
        }

        private static double Deg2Rad(double deg)
        {
            return (deg*Math.PI/180.0);
        }

        private static double Rad2Deg(double rad)
        {
            return (rad/Math.PI*180.0);
        }

        public List<PinDto> GetMyPins(string token, GeoCoordinate originCoords, int contactId, string finderType)
        {
            var pins = new List<PinDto>();
            var participantId = GetParticipantIdFromContact(contactId);

            int[] groupTypesToFetch = finderType == _finderConnect ? new int[] { _anywhereGroupType } : new int[] { _smallGroupType };

            var groupPins = GetMyGroupPins(token, groupTypesToFetch, participantId, finderType);
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

        public List<PinDto> GetMyGroupPins(string token, int[] groupTypeIds, int participantId, string finderType)
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

            var cloudsearchQueryString = groupsByType.Aggregate("(or ", (current, @group) => current + ("groupid:" + @group.GroupId + " ")) +")";
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

            var distanceNorth = Math.Sin(angle)*distance;
            var distanceEast = Math.Cos(angle)*distance;

            var newLat = (double) (address.Latitude + (distanceNorth/earthRadius)*180/Math.PI);
            var newLong = (double) (address.Longitude + (distanceEast/(earthRadius*Math.Cos(newLat*180/Math.PI)))*180/Math.PI);
            address.Latitude = newLat;
            address.Longitude = newLong;

            return address;
        }


        public Invitation InviteToGroup(string token, int gatheringId, User person, string finderFlag)
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

            _invitationService.ValidateInvitation(invitation, token);
            invitation = _invitationService.CreateInvitation(invitation, token);

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
                FromContactId = _contactRepository.GetContactId(token),
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

        public AddressDTO GetPersonAddress(string token, int participantId, bool shouldGetFullAddress = true)
        {
            var user = _participantRepository.GetParticipantRecord(token);

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

        public void SayHi(int fromContactId, int toContactId)
        {
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

        public void AcceptDenyGroupInvitation(string token, int groupId, string invitationGuid, bool accept)
        {
            try
            {
                _groupToolService.AcceptDenyGroupInvitation(token, groupId, invitationGuid, accept);

                var host = GetPinDetailsForPerson(GetLeaderParticipantIdFromGroup(groupId));
                var cm = _contactRepository.GetContactById(_authenticationRepository.GetContactId(token));

                var connection = new ConnectCommunicationDto
                {
                    FromContactId = cm.Contact_ID,
                    ToContactId = (int) host.Contact_ID,
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
                    var props = new EventProperties {{"InvitationTo", cm.Contact_ID}, {"InvitationToEmail", cm.Email_Address}};
                    _analyticsService.Track(host.Contact_ID.ToString(), "HostInvitationAccepted", props);

                    props = new EventProperties {{"InvitationFrom", host.Contact_ID}, {"InvitationFromEmail", host.EmailAddress}};
                    _analyticsService.Track(cm.Contact_ID.ToString(), "InviteeAcceptedInvitation", props);
                }

            }
            catch (Exception e)
            {
                throw e;
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
                        ContactId = host.Contact_ID.Value,
                        EmailAddress = host.EmailAddress
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

        private Dictionary<string, object> GetEmailMergeData(int newUserContactId , GroupDTO group)
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
                {"YesURL", $"{baseUrl}{groupToolPath}/small-group/{group.GroupId}/true/{participant.ParticipantId}" },
                {"NoURL" , $"{baseUrl}{groupToolPath}/small-group/{group.GroupId}/false/{participant.ParticipantId}" },
                {"StartURL",   $"{baseUrl}{groupToolPath}/create-group" },
                {"SearchURL",   $"{baseUrl}{groupToolPath}" },
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

        private void SendTryAGroupEmailToLeader(string token, GroupDTO group)
        {
            try
            {
                var newMemberId = _contactRepository.GetContactId(token);
                var mergeData = GetEmailMergeData(newMemberId, group);

                var emailTemplateId = _configurationWrapper.GetConfigIntValue("GroupsTryAGroupLeaderNotificationTemplateId");

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

                var primary = _contactRepository.GetContactById(_contactRepository.GetContactIdByParticipantId(_groupService.GetPrimaryContactParticipantId(group.GroupId)));

                var to = new List<MpContact>
                {
                    new MpContact
                    {
                        // Just need a contact ID here, doesn't have to be for the recipient
                        ContactId = primary.Contact_ID,
                        EmailAddress = primary.Email_Address
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

        private void SendEmailToAddedUser(string token, User user, int groupid)
        {
            var emailTemplateId = _configurationWrapper.GetConfigIntValue("GroupsAddParticipantEmailNotificationTemplateId");
            var emailTemplate = _communicationRepository.GetTemplate(emailTemplateId);
            var leaderContactId = _contactRepository.GetContactId(token);
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
                {"Leader_Phone", $"{leaderContact.Home_Phone}\n{leaderContact.Mobile_Phone}" }
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
                FromContactId = _contactRepository.GetContactId(token),
                CommunicationStatusId = _configurationWrapper.GetConfigIntValue("ConnectCommunicationStatusNA"),
                GroupId = groupid,
              
            };

            RecordCommunication(connection);
        }
       
        public List<PinDto> AddPinMetaData(List<PinDto> pins, GeoCoordinate originCoords, int contactId = 0)
        {
            foreach (var pin in pins)
            {
                pin.Title = GetPinTitle(pin, contactId);
                pin.IconUrl = GetPinUrl(pin.PinType);

                if ((pin.PinType == PinType.GATHERING || pin.PinType == PinType.SMALL_GROUP) && pin.Address.PostalCode != null && pin.Address.Longitude == 0)
                {
                    // Everything will go to a state level with bad address - because state is required select control
                    _addressService.SetGroupPinGeoCoordinates(pin);
                }

                //calculate proximity for all pins to origin
                if (pin.Address.Latitude == null) continue;
                if (pin.Address.Longitude != null && originCoords != null)
                {
                    pin.Proximity = GetProximity(originCoords, new GeoCoordinate(pin.Address.Latitude.Value, pin.Address.Longitude.Value));
                }

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
                    resultMapCenterCoords  = new GeoCoordinate(frontEndMapCenter.Lat.Value, frontEndMapCenter.Lng.Value);
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

        public void ApproveDenyGroupInquiry(string token, bool approve, Inquiry inquiry)
        {
            try
            {
                _groupToolService.VerifyCurrentUserIsGroupLeader(token, inquiry.GroupId);
                var group = _groupService.GetGroupDetails(inquiry.GroupId);
                var participant = _participantRepository.GetParticipant(inquiry.ContactId);
                ApproveOrDenyGroupInquiry(inquiry, group, participant, approve);

                // Record Analytics
                var leader = ((List<GroupParticipantDTO>)group.Participants).FirstOrDefault(p => p.GroupRoleId == _groupRoleLeaderId);
                var props = new EventProperties
                    {
                        {"GroupLeaderName", leader?.DisplayName },
                        {"GroupName", group.GroupName},
                        {"GroupCity", group?.Address?.City},
                        {"GroupState", group?.Address?.State},
                        {"GroupZip", group?.Address?.PostalCode}
                    };
                var eventName = approve ? "AcceptedIntoGroup" : "DeniedIntoGroup";
                _analyticsService.Track(inquiry.ContactId.ToString(), eventName, props);
            }
            catch (GroupParticipantRemovalException e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            catch (Exception e)
            {
                throw new GroupParticipantRemovalException($"Could not add Inquirer {inquiry.InquiryId} from group {inquiry.GroupId}", e);
            }
        }

        private void ApproveOrDenyGroupInquiry(Inquiry inquiry, GroupDTO group, MpParticipant participant, bool approve)
        {
            // Add member to group if it is approved
            var roleId = group.GroupTypeId == _smallGroupType ? _trialMemberRoleId : _memberRoleId;
            if (approve)
            {
                _groupService.addContactToGroup(group.GroupId, inquiry.ContactId, roleId);
            }
            // Update the inquiry
            _groupRepository.UpdateGroupInquiry(group.GroupId, inquiry.InquiryId, approve);

            // Record in connections table if this is an anywhere gathering.
            var commType = group.GroupTypeId == _smallGroupType ? _connectCommunicationTypeRequestToJoinSmallGroup : _connectGatheringRequestToJoin;
            RecordConnectInteraction(group.GroupId, participant.ContactId, inquiry.ContactId, commType, (approve) ? 1 : 0);

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

        public void TryAGroupAcceptDeny(string token, int groupId, int participantId, bool accept)
        {
            var contactId = _contactRepository.GetContactIdByParticipantId(participantId);
            var inquiry = _groupToolService.GetGroupInquiryForContactId(groupId, contactId);

            if(_groupRepository.GetGroupParticipants(groupId, true).Exists(p =>p.ParticipantId == participantId) || inquiry.Placed != null)
            {
               var e = new Exception("User is already a group member");
               throw e;
            }

            //accept or deny the inquiry
            ApproveDenyGroupInquiry(token, accept, inquiry);
        }
    }
}

