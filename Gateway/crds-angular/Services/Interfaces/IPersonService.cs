using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Profile;
using MinistryPlatform.Translation.Models.DTO;

namespace crds_angular.Services.Interfaces
{
    public interface IPersonService
    {
        void SetProfile(Person person, string userAccessToken);        
        Person GetLoggedInUserProfile(String token);
        Person GetPerson(int contactId);
        List<MpRoleDto> GetLoggedInUserRoles(string token);
    }
}