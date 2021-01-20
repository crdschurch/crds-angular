using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Group_Participants")]
    public class MpGroupParticipant
    {
        [JsonProperty("Group_Participant_ID")]
        public int GroupParticipantId { get; set; }
        [JsonProperty("Participant_ID")]
        public int ParticipantId { get; set; }
        [JsonProperty("Group_ID")]
        public int GroupId { get; set; }
        [JsonProperty("Group_Role_ID")]
        public int GroupRoleId { get; set; }
        [JsonProperty("Group_Name")]
        public string GroupName { get; set; }
        [JsonProperty("Contact_ID")]
        public int ContactId { get; set; }
        [JsonProperty("Nickname")]
        public string NickName { get; set; }
        [JsonProperty("Display_Name")]
        public string DisplayName { get; set; }
        [JsonProperty("Last_name")]
        public string LastName { get; set; }
        [JsonProperty("Role_Title")]
        public string GroupRoleTitle { get; set; }
        [JsonProperty("Email_Address")]
        public string Email { get; set; }
        [JsonProperty("Child_Care_Requested")]
        public bool ChildCareRequested { get; set; }
        [JsonProperty("Start_Date")]
        public DateTime? StartDate { get; set; }
        [JsonProperty("End_Date")]
        public DateTime? EndDate { get; set; }
        [JsonProperty("Employee_Role")]
        public bool EmployeeRole { get; set; }
        [JsonProperty("Enrolled_By")]
        public int? EnrolledBy { get; set; }
        public string Congregation { get; set; }
        public bool IsApprovedSmallGroupLeader { get; set; }
    }
}
