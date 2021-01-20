using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ILocationRepository
    {
        List<MpLocation> GetLocations(string search = null);
    }
}
