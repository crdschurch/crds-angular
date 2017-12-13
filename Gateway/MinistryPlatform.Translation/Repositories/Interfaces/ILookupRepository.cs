using System.Collections.Generic;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ILookupRepository
    {
        Dictionary<string, object> EmailSearch(string email, string token);

        List<Dictionary<string, object>> EventTypes(string token);

        List<Dictionary<string, object>> EventTypesForEventTool(string token);

        List<Dictionary<string, object>> Genders(string token);

        List<Dictionary<string, object>> MaritalStatus(string token);

        List<Dictionary<string, object>> ServiceProviders(string token);

        List<Dictionary<string, object>> States(string token);

        List<Dictionary<string, object>> Countries(string token);

        List<Dictionary<string, object>> CrossroadsLocations(string token);

        List<Dictionary<string, object>> ReminderDays(string token);

        List<Dictionary<string, object>> WorkTeams(string token);

        List<Dictionary<string, object>> GroupReasonEnded(string token);

        List<Dictionary<string, object>> MeetingDays(string token);

        IEnumerable<T> GetList<T>(string token);
        T GetObject<T>(string token);

    }
}
