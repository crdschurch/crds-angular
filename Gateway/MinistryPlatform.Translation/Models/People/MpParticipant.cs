﻿using System;

namespace MinistryPlatform.Translation.Models.People
{
    // ReSharper disable InconsistentNaming
    public class MpParticipant
    {
        public int Contact_ID { get; set; }
        public int Participant_ID { get; set; }
        public string Display_Name { get; set; }
        public string Email_Address { get; set; }
        public DateTime? Attendance_Start_Date { get; set; }
        public bool Approved_Small_Group_Leader { get; set; }
    }
}
