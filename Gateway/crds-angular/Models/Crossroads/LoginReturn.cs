using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;

namespace crds_angular.Models.Crossroads
{
    public class LoginReturn
    {
        public string userToken { get; set; }
        public string userTokenExp { get; set; }
        public string refreshToken { get; set; }
        public int userId { get; set; }
        public string username { get; set; }
        public string userEmail { get; set;  }
        public List<MpRoleDto> roles { get; set; }
        public Boolean canImpersonate { get; set; }
        public int age { get; set; }
        public string userPhone { get; set; }
    }
}