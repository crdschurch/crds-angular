namespace crds_angular.Models.AwsCloudsearch
{
    public class AwsCloudsearchDto
    {
        public string type { get; set; }
        public string id { get; set; }
        public AwsConnectDto fields { get; set; }

        public AwsCloudsearchDto()
        {
        }

        public AwsCloudsearchDto(string type, string id, AwsConnectDto fields)
        {
            this.type = type;
            this.id = id;
            this.fields = fields;
        }
    }
}