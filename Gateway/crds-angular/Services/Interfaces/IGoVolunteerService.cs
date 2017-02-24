using System.Collections.Generic;
using crds_angular.Models.Crossroads.GoVolunteer;

namespace crds_angular.Services.Interfaces
{
    public interface IGoVolunteerService
    {
        Registration CreateRegistration(Registration registration, string token);

        List<ProjectType> GetProjectTypes();
        List<ChildrenOptions> ChildrenOptions();
        bool SendMail(Registration registration);
        List<ProjectCity> GetParticipatingCities(int initiativeId);
    }
}