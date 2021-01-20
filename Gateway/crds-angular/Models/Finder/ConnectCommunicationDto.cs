namespace crds_angular.Models.Finder
{
    public class ConnectCommunicationDto
    {
        public int ConnectCommunicationsId { get; set; }
        public int FromContactId { get; set; }
        public int ToContactId { get; set; }
        public int CommunicationTypeId { get; set; }
        public int CommunicationStatusId { get; set; }
        public int? GroupId { get; set; }
    }
}