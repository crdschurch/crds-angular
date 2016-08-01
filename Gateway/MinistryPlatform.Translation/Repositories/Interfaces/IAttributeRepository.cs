using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IAttributeRepository
    {
        List<MpAttribute> GetAttributes(int? attributeTypeId);
        void CreateMissingAttributesReturnAttributeIds(List<MpAttribute> attributes, int attributeType);
    }
}