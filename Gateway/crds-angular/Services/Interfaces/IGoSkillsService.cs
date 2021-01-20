using System.Collections.Generic;
using crds_angular.Models.Crossroads.GoVolunteer;

namespace crds_angular.Services.Interfaces
{
    public interface IGoSkillsService
    {
        List<GoSkills> RetrieveGoSkills(string token);
        void UpdateSkills(int participantId, List<GoSkills> skills, string token);
    }

}
