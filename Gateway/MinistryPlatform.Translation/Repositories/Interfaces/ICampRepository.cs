using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ICampRepository
    {
        MpCamp GetCampEventDetails(int eventId);
    }
}
