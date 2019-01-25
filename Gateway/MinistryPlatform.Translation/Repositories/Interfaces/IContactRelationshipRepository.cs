using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IContactRelationshipRepository
    {
        IEnumerable<MpContactRelationship> GetMyImmediateFamilyRelationships(int contactId);
        IEnumerable<MpRelationship> GetMyCurrentRelationships(int contactId);
        IEnumerable<MpContactRelationship> GetMyCurrentRelationships(int contactId, string param2);
        int AddRelationship(MpRelationship relationship, int toContact);
    }
    
}
