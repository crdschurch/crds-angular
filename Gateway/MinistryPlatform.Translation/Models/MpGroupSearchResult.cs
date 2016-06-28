﻿using System;

namespace MinistryPlatform.Translation.Models
{
    public class MpGroupSearchResult
    {
        public int GroupId { get; set; }
        // TODO: Cleanup
        //public int GroupType { get; set; }
        public string Name { get; set; }
        //public IList<GroupParticipant> Participants { get; set; }
        //public string PrimaryContact { get; set; }
        public string PrimaryContactName { get; set; }
        //public string PrimaryContactEmail { get; set; }
        public string GroupDescription { get; set; }
        public string MeetingTime { get; set; }
        public int? MeetingDayId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RemainingCapacity { get; set; }
        public int ContactId { get; set; }
        public MpAddress Address { get; set; }
        public MpGroupSearchAttributes SearchAttributes { get; set; }
    }
}
