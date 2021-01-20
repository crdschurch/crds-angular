using System;

namespace MinistryPlatform.Translation.Models.Finder
{
    public class MpConnectAws
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SiteName { get; set; }
        public string EmailAddress { get; set; }
        public int? ContactId { get; set; }
        public int? ParticipantId { get; set; }
        public int? AddressId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? HostStatus { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public int? PrimaryContactId { get; set; }
        public string PrimaryContactEmail { get; set; }
        public int? ParticipantCount { get; set; }
        public int? GroupTypeId { get; set; }
        public int? HouseholdId { get; set; }
        public int PinType { get; set; }
        public DateTime? GroupStartDate { get; set; }

        public string GroupCategory { get; set; }
        public string GroupType { get; set; }
        public string GroupAgeRange { get; set; }
        public string GroupMeetingDay { get; set; }
        public string GroupMeetingTime { get; set; }
        public int? GroupVirtual { get; set; }
        public string GroupLocation { get; set; }
        public string GroupMeetingFrequency { get; set; }
        public int GroupKidsWelcome { get; set; }
        public string GroupPrimaryContactFirstName { get; set; }
        public string GroupPrimaryContactLastName { get; set; }
        public string GroupPrimaryContactCongregation { get; set; }
        public int GroupAvailableOnline { get; set; }
    }
}
