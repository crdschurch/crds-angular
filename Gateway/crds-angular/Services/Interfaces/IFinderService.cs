using System;
using System.Collections.Generic;
using System.Device.Location;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Finder;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using System.Threading.Tasks;

namespace crds_angular.Services.Interfaces
{
    public interface IFinderService
    {
        PinDto GetPinDetailsForPerson(int participantId);
        PinDto GetPinDetailsForGroup(int groupId, GeoCoordinate originCoords);
        void EnablePin(int participantId);
        void DisablePin(int participantId);
        void UpdateHouseholdAddress(PinDto pin);
        List<PinDto> GetMyPins(GeoCoordinate originCoords, int contactId, string finderType);
        List<PinDto> GetMyGroupPins(int[] groupTypeIds, int participantId, string finderType);
        int GetParticipantIdFromContact(int contactId);
        List<PinDto> GetPinsInBoundingBox(GeoCoordinate originCoords, string address, AwsBoundingBox boundingBox, string finderType, int contactId, string filterSearchString);
        AddressDTO RandomizeLatLong(AddressDTO address);
        GeoCoordinate GetGeoCoordsFromAddressOrLatLang(string address, GeoCoordinates centerCoords);
        Boolean areAllBoundingBoxParamsPresent(MapBoundingBox boundingBox);
        GeoCoordinate GetGeoCoordsFromLatLong(string lat, string lng);
        Invitation InviteToGroup(int contactId, int gatheringId, User person, string finderFlag);
        List<GroupParticipantDTO> GetParticipantsForGroup(int groupId);
        AddressDTO GetGroupAddress(int groupId);
        AddressDTO GetPersonAddress(int contactId, int participantId = -1, bool shouldGetFullAddress = true);
        PinDto UpdateGathering(PinDto pin);
        void AcceptDenyGroupInvitation(int contactId, int groupId, string invitationGuid, bool accept);
        void SayHi(int fromContactId, int toContactId, string message);
        List<PinDto> RandomizeLatLongForNonSitePins(List<PinDto> pins);
        GeoCoordinate GetMapCenterForResults(string userSearchString, GeoCoordinates frontEndMapCenter, string finderType);
        void AddUserDirectlyToGroup(User user, int groupid, int roleId, int leaderContactId);
        bool DoesActiveContactExists(string email);
        bool DoesUserLeadSomeGroup(int contactId);
        void TryAGroup(int contactId, int groupId);
        void TryAGroupAcceptDeny(int groupId, int participantId, bool accept);
        void ApproveDenyGroupInquiry(bool approve, Inquiry inquiry);
        bool IsUserOnMap(int contactid);
        void SetShowOnMap(int participantId, Boolean showOnMap);
        List<int> GetAddressIdsWithNoGeoCode();
        List<int> GetAddressIdsForMapParticipantWithNoGeoCode();
        // map 2.0
        PersonDTO GetPerson(int participantId);
        MeDTO GetMe(int contactId);
        void SaveMe(int contactId, MeDTO medto);
        void SayHiToParticipant(int fromContactId, int toParticipantId, string message);
        void UpdatePersonPhotoInFirebaseIfOnMap(int contactid);
    }
}