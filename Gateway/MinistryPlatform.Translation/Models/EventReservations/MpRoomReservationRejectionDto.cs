using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinistryPlatform.Translation.Models.EventReservations
{
    public class MpRoomReservationRejectionDto
    {
        public int Requestor_Contact_ID { get; set; }
        public string Room_Name { get; set; }
        public string Event_Start_Date { get; set; }
        public string Event_Title { get; set; }
        public string Task_Rejection_Reason { get; set; }
    }
}
