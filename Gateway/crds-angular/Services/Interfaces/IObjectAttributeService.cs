using System.Collections.Generic;
using crds_angular.Models.Crossroads.Attribute;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IObjectAttributeService
    {
        ObjectAllAttributesDTO GetObjectAttributes(int objectId, MpObjectAttributeConfiguration configuration);
        ObjectAllAttributesDTO GetObjectAttributes(int objectId, MpObjectAttributeConfiguration configuration, List<MpAttribute> mpAttributes);

        void SaveObjectAttributes(int objectId,
                                  Dictionary<int, ObjectAttributeTypeDTO> objectAttributes,
                                  Dictionary<int, ObjectSingleAttributeDTO> objectSingleAttributes,
                                  MpObjectAttributeConfiguration configuration);

        void SaveObjectMultiAttribute(int objectId, ObjectAttributeDTO objectAttribute, MpObjectAttributeConfiguration configuration, bool parallel=false);
    }
}