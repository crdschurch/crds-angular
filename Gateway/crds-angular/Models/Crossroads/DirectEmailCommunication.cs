namespace crds_angular.Models.Crossroads
{
    public class DirectEmailCommunication
    {
        public int CommunicationId { get; set; }
        public int CommunicationMessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}