namespace MinistryPlatform.Translation.Models.EventReservations
{
    public class MpRoomReservationDto
    {
        public int EventId { get; set; }
        public int EventRoomId { get; set; }
        public int RoomId { get; set; }
        public int RoomLayoutId { get; set; }
        public string Notes { get; set; }
        public bool Hidden { get; set; }
        public bool Cancelled { get; set; }
        /// <summary>
        /// The name of the room, typically a name like "KC103".
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The label of the room, typically like "Nursery A 1-2 months"
        /// </summary>
        public string Label { get; set; }
        public bool CheckinAllowed { get; set; }
        public int Capacity { get; set; }
        public int Volunteers { get; set; }

        public bool Approved { get; set; }
        public bool Rejected { get; set; }
    }
}
