using System;

namespace MinistryPlatform.Translation.Models.EventReservations
{
    public class MpRoomReservationRejectionDto
    {
        public int Task_ID { get; set; }
        public int Requestor_Contact_ID { get; set; }
        public string Room_Name { get; set; }
        public DateTime Event_Start_Date { get; set; }
        public string Event_Title { get; set; }
        public string Task_Rejection_Reason { get; set; }
    }
}
