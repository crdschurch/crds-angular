using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models;
using System.Collections.Generic;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ICongregationRepository
    {
        MpCongregation GetCongregationById(int id);
        Result<MpCongregation> GetCongregationByName(string congregationName , string token );
        List<MpCongregation> GetCongregations(string searchString = null);
    }
}