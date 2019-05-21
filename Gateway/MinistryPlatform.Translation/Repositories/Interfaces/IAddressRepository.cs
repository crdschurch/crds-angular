using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        int Create(MpAddress address);
        int Update(MpAddress address);
        List<MpAddress> FindMatches(MpAddress address);
        MpAddress GetAddressById(string token, int id);
        MpAddress GetAddressById(int id);
        List<int> FindAddressIdsWithoutGeocode();
        List<int> FindMapParticipantsAddressIdsWithoutGeocode();
    }
}