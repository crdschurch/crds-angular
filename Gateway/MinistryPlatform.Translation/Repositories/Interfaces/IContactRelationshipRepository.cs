using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IContactRelationshipRepository
    {
        IEnumerable<MpContactRelationship> GetMyImmediateFamilyRelationships(int contactId, string token);
        IEnumerable<MpRelationship> GetMyCurrentRelationships(int contactId);
        IEnumerable<MpContactRelationship> GetMyCurrentRelationships(int contactId, string token);
        int AddRelationship(MpRelationship relationship, int toContact);
    }
    
}
