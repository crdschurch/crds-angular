using System.Collections.Generic;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ILookupRepository
    {
        Dictionary<string, object> EmailSearch(string email);

        List<Dictionary<string, object>> EventTypes();

        List<Dictionary<string, object>> EventTypesForEventTool();

        List<Dictionary<string, object>> Genders();

        List<Dictionary<string, object>> MaritalStatus();

        List<Dictionary<string, object>> ServiceProviders();

        List<Dictionary<string, object>> States();

        List<Dictionary<string, object>> Countries();

        List<Dictionary<string, object>> CrossroadsLocations();

        List<Dictionary<string, object>> ReminderDays();

        List<Dictionary<string, object>> WorkTeams();

        List<Dictionary<string, object>> GroupReasonEnded();

        List<Dictionary<string, object>> MeetingDays();

        List<Dictionary<string, object>> MeetingFrequencies();

        IEnumerable<T> GetList<T>();
        
    }
}
