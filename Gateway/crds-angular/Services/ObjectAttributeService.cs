using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MpAttribute = MinistryPlatform.Translation.Models.MpAttribute;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class ObjectAttributeService : IObjectAttributeService
    {
        private readonly MPInterfaces.IObjectAttributeRepository _mpObjectAttributeService;
        private readonly IAttributeService _attributeService;
        private readonly IApiUserRepository _apiUserService;
        private readonly MPInterfaces.IAttributeRepository _mpAttributeService;

        public ObjectAttributeService(
            MPInterfaces.IObjectAttributeRepository mpObjectAttributeService,
            IAttributeService attributeService,
            IApiUserRepository apiUserService,
            MPInterfaces.IAttributeRepository mpAttributeService)
        {
            _mpObjectAttributeService = mpObjectAttributeService;
            _attributeService = attributeService;
            _apiUserService = apiUserService;
            _mpAttributeService = mpAttributeService;
        }

        public ObjectAllAttributesDTO GetObjectAttributes(string token, int objectId, MpObjectAttributeConfiguration configuration)
        {
            var mpAttributes = _mpAttributeService.GetAttributes(null);
            return GetObjectAttributes(token, objectId, configuration, mpAttributes);
        }

        public ObjectAllAttributesDTO GetObjectAttributes(string token, int objectId, MpObjectAttributeConfiguration configuration, List<MpAttribute> mpAttributes)
        {
            var mpObjectAttributes = _mpObjectAttributeService.GetCurrentObjectAttributes(_apiUserService.GetDefaultApiClientToken(), objectId, configuration);

            var allAttributes = new ObjectAllAttributesDTO();

            allAttributes.MultiSelect = TranslateToAttributeTypeDtos(mpObjectAttributes, mpAttributes);
            allAttributes.SingleSelect = TranslateToSingleAttributeTypeDtos(mpObjectAttributes, mpAttributes);

            return allAttributes;
        }


        private Dictionary<int, ObjectAttributeTypeDTO> TranslateToAttributeTypeDtos(List<MpObjectAttribute> mpObjectAttributes, List<MpAttribute> mpAttributes)
        {
            var mpFilteredAttributes = mpAttributes.Where(x => x.PreventMultipleSelection == false).ToList();

            var attributeTypesDictionary = mpFilteredAttributes
                .Select(x => new {x.AttributeTypeId, x.AttributeTypeName})
                .Distinct()
                .ToDictionary(mpAttributeType => mpAttributeType.AttributeTypeId,
                              mpAttributeType => new ObjectAttributeTypeDTO()
                              {
                                  AttributeTypeId = mpAttributeType.AttributeTypeId,
                                  Name = mpAttributeType.AttributeTypeName
                              });


            foreach (var mpAttribute in mpFilteredAttributes)
            {
                var objectAttribute = new ObjectAttributeDTO()
                {
                    AttributeId = mpAttribute.AttributeId,
                    Name = mpAttribute.Name,
                    Description = mpAttribute.Description,
                    SortOrder = mpAttribute.SortOrder,
                    Selected = false,
                    Category = mpAttribute.Category,
                    CategoryDescription = mpAttribute.CategoryDescription,
                    EndDate = mpAttribute.EndDate
                };

                attributeTypesDictionary[mpAttribute.AttributeTypeId].Attributes.Add(objectAttribute);
            }

            foreach (var mpObjectAttribute in mpObjectAttributes)
            {
                if (!attributeTypesDictionary.ContainsKey(mpObjectAttribute.AttributeTypeId))
                {
                    continue;
                }

                var objectAttributeType = attributeTypesDictionary[mpObjectAttribute.AttributeTypeId];

                // Is the attribute ID in the collection (if not, was most likely end dated)
                if (!objectAttributeType.Attributes.Exists(x => x.AttributeId == mpObjectAttribute.AttributeId))
                {
                    continue;
                }
                var objectAttribute = objectAttributeType.Attributes.First(x => x.AttributeId == mpObjectAttribute.AttributeId);
                objectAttribute.StartDate = mpObjectAttribute.StartDate;
                objectAttribute.EndDate = mpObjectAttribute.EndDate;
                objectAttribute.Notes = mpObjectAttribute.Notes;
                objectAttribute.Selected = true;
            }

            return attributeTypesDictionary;
        }

        private Dictionary<int, ObjectSingleAttributeDTO> TranslateToSingleAttributeTypeDtos(
            List<MpObjectAttribute> mpObjectAttributes,
            List<MpAttribute> mpAttributes)
        {
            var mpFilteredAttributes = mpAttributes.Where(x => x.PreventMultipleSelection == true).ToList();

            var attributeTypesDictionary = mpFilteredAttributes
                .Select(x => new {x.AttributeTypeId, x.AttributeTypeName})
                .Distinct()
                .ToDictionary(mpAttributeType => mpAttributeType.AttributeTypeId,
                              mpAttributeType => new ObjectSingleAttributeDTO());

            foreach (var mpObjectAttribute in mpObjectAttributes)
            {
                if (!attributeTypesDictionary.ContainsKey(mpObjectAttribute.AttributeTypeId))
                {
                    continue;
                }
                if (!mpAttributes.Exists(x => x.AttributeId == mpObjectAttribute.AttributeId))
                {
                    continue;
                }
                var mpAttribute = mpAttributes.First(x => x.AttributeId == mpObjectAttribute.AttributeId);

                var attribute = _attributeService.ConvertAttributeToAttributeDto(mpAttribute);
                var objectSingleAttribute = attributeTypesDictionary[mpObjectAttribute.AttributeTypeId];

                objectSingleAttribute.Value = attribute;
                objectSingleAttribute.Notes = mpObjectAttribute.Notes;
            }

            return attributeTypesDictionary;
        }

        public void SaveObjectAttributes(int objectId,
                                         Dictionary<int, ObjectAttributeTypeDTO> objectAttributes,
                                         Dictionary<int, ObjectSingleAttributeDTO> objectSingleAttributes, MpObjectAttributeConfiguration configuration)
        {
            var currentAttributes = TranslateMultiToMPAttributes(objectAttributes);
            currentAttributes.AddRange(TranslateSingleToMPAttribute(objectSingleAttributes));

            if (objectAttributes == null)
            {
                return;
            }

            var apiUserToken = _apiUserService.GetDefaultApiClientToken();            

            var persistedAttributes = _mpObjectAttributeService.GetCurrentObjectAttributes(apiUserToken, objectId, configuration);
            var attributesToSave = GetDataToSave(currentAttributes, persistedAttributes);

            foreach (var attribute in attributesToSave)
            {
                SaveAttribute(objectId, attribute, apiUserToken, configuration);
            }
        }

        public void SaveObjectMultiAttribute(int objectId, ObjectAttributeDTO objectAttribute, MpObjectAttributeConfiguration configuration, bool parallel)
        {
            objectAttribute.StartDate = ConvertToServerDate(objectAttribute.StartDate);
            if (objectAttribute.EndDate != null)
            {
                objectAttribute.EndDate = ConvertToServerDate(objectAttribute.EndDate.Value);
            }

            var mpObjectAttribute = TranslateMultiToMPAttribute(objectAttribute, null);            
            var persistedAttributes = _mpObjectAttributeService.GetCurrentObjectAttributes(_apiUserService.GetDefaultApiClientToken(), objectId, configuration, objectAttribute.AttributeId);

            if (persistedAttributes.Count >= 1)
            {
                mpObjectAttribute.ObjectAttributeId = persistedAttributes[0].ObjectAttributeId;
            }
            var token = _apiUserService.GetDefaultApiClientToken();
            SaveAttribute(objectId, mpObjectAttribute, token, configuration, parallel);
        }

        private DateTime ConvertToServerDate(DateTime source)
        {
            if (source.Kind != DateTimeKind.Utc)
            {
                return source.Date;
            }

            // Client side for Skills sends up UTC date/times. 
            // These need to be converted from UTC timestamp to local servers date
            // and then put back in UTC timezone so MP does mess with the time portion          
            var result = source.ToLocalTime().Date;
            result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
            return result;
        }

        private void SaveAttribute(int objectId, MpObjectAttribute attribute, string token, MpObjectAttributeConfiguration configuration, bool parallel = false)
        {
            if (attribute.ObjectAttributeId == 0)
            {
                // These are new so add them
                if (parallel)
                {
                    var obs = _mpObjectAttributeService.CreateAttributeAsync(token, objectId, attribute, configuration);
                    obs.Subscribe();
                }
                else
                {
                    _mpObjectAttributeService.CreateAttribute(token, objectId, attribute, configuration);
                }
            }
            else
            {
                // These are existing so update them
                if (parallel)
                {
                    _mpObjectAttributeService.UpdateAttributeAsync(token, attribute, configuration);
                }
                else
                {
                    _mpObjectAttributeService.UpdateAttribute(token, attribute, configuration);
                }
                
            }
        }

        private void SaveAttributeAsync(int objectId, MpObjectAttribute attribute, string token, MpObjectAttributeConfiguration configuration)
        {
            if (attribute.ObjectAttributeId == 0)
            {
                // These are new so add them
                _mpObjectAttributeService.CreateAttribute(token, objectId, attribute, configuration);
            }
            else
            {
                // These are existing so update them
                _mpObjectAttributeService.UpdateAttribute(token, attribute, configuration);
            }
        }

        private List<MpObjectAttribute> TranslateMultiToMPAttributes(Dictionary<int, ObjectAttributeTypeDTO> objectAttributeTypes)
        {
            var results = new List<MpObjectAttribute>();

            if (objectAttributeTypes == null)
            {
                return results;
            }
            results.AddRange(from objectAttributeType in objectAttributeTypes.Values
                from objectAttribute in objectAttributeType.Attributes
                where objectAttribute.Selected
                select TranslateMultiToMPAttribute(objectAttribute, objectAttributeType));
            return results;
        }

        private static MpObjectAttribute TranslateMultiToMPAttribute(ObjectAttributeDTO objectAttribute, ObjectAttributeTypeDTO objectAttributeType)
        {
            var mpObjectAttribute = new MpObjectAttribute();
            if (objectAttribute == null)
            {
                return mpObjectAttribute;
            }
            mpObjectAttribute.AttributeId = objectAttribute.AttributeId;
            mpObjectAttribute.AttributeTypeId = objectAttributeType != null ? objectAttributeType.AttributeTypeId : 0;
            mpObjectAttribute.AttributeTypeName = objectAttributeType != null ? objectAttributeType.Name : string.Empty;
            mpObjectAttribute.StartDate = objectAttribute.StartDate;
            mpObjectAttribute.EndDate = objectAttribute.EndDate;
            mpObjectAttribute.Notes = objectAttribute.Notes;

            return mpObjectAttribute;
        }

        private List<MpObjectAttribute> TranslateSingleToMPAttribute(Dictionary<int, ObjectSingleAttributeDTO> objectSingleAttributes)
        {
            var results = new List<MpObjectAttribute>();

            if (objectSingleAttributes == null)
            {
                return results;
            }

            foreach (var objectSingleAttribute in objectSingleAttributes)
            {
                var objectAttribute = objectSingleAttribute.Value;

                if (objectAttribute.Value == null)
                {
                    continue;
                }

                var mpObjectAttribute = new MpObjectAttribute()
                {
                    AttributeId = objectAttribute.Value.AttributeId,
                    AttributeTypeId = objectSingleAttribute.Key,
                    Notes = objectAttribute.Notes
                };

                results.Add(mpObjectAttribute);
            }
            return results;
        }

        private List<MpObjectAttribute> GetDataToSave(List<MpObjectAttribute> currentAttributes, List<MpObjectAttribute> persistedAttributes)
        {
            // prevent side effects by cloning lists
            currentAttributes = new List<MpObjectAttribute>(currentAttributes);
            persistedAttributes = new List<MpObjectAttribute>(persistedAttributes);

            for (int index = currentAttributes.Count - 1; index >= 0; index--)
            {
                var attributeToSave = currentAttributes[index];

                bool foundMatch = false;

                for (int currentIndex = persistedAttributes.Count - 1; currentIndex >= 0; currentIndex--)
                {
                    var currentAttribute = persistedAttributes[currentIndex];

                    if (currentAttribute.AttributeId == attributeToSave.AttributeId)
                    {
                        foundMatch = true;
                        if (attributeToSave.Notes == currentAttribute.Notes)
                        {
                            persistedAttributes.RemoveAt(currentIndex);
                            currentAttributes.RemoveAt(index);
                        }
                        else if (attributeToSave.Notes != String.Empty || attributeToSave.Notes == null)
                        {
                            persistedAttributes.RemoveAt(currentIndex);
                            attributeToSave.StartDate = currentAttribute.StartDate;
                            attributeToSave.ObjectAttributeId = currentAttribute.ObjectAttributeId;
                        }
                        else
                        {
                            currentAttributes.RemoveAt(index);
                        }
                        break;
                    }
                }

                if (!foundMatch)
                {
                    // New Entry with no match
                    attributeToSave.StartDate = DateTime.Today;
                }
            }

            foreach (var persisted in persistedAttributes)
            {
                // Existing entry with no match, so effectively remove it by end-dating it
                persisted.EndDate = DateTime.Today;
            }

            var dataToSave = new List<MpObjectAttribute>(currentAttributes);
            dataToSave.AddRange(persistedAttributes);
            return dataToSave;
        }
    }
}