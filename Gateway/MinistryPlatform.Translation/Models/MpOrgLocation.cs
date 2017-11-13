namespace MinistryPlatform.Translation.Models
{ 
    public class MpOrgLocation
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int LocationTypeId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ImageUrl { get; set; }
    }
}
