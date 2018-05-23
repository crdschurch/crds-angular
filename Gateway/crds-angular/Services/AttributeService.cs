using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class AttributeService : IAttributeService
    {
        private readonly MPInterfaces.IAttributeRepository _attributeRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IApiUserRepository _apiUserRepository;

        public AttributeService(MPInterfaces.IAttributeRepository attributeRepository,
                                IMinistryPlatformRestRepository restRepository, 
                                IApiUserRepository apiUserRepository)
        {
            _attributeRepository = attributeRepository;
            _ministryPlatformRestRepository = restRepository;
            _apiUserRepository = apiUserRepository;
        }

        public List<AttributeTypeDTO> GetAttributeTypes(int? attributeTypeId)
        {
            var attributes = _attributeRepository.GetAttributes(attributeTypeId);

            var attributeTypes = new Dictionary<int, AttributeTypeDTO>();

            foreach (var attribute in attributes)
            {
                var attributeDto = ConvertAttributeToAttributeDto(attribute);

                var key = GetOrCreateAttributeTypeDto(attribute, attributeTypes);

                attributeTypes[key].Attributes.Add(attributeDto);
            }

            return attributeTypes.Values.ToList();
        }

        public AttributeDTO ConvertAttributeToAttributeDto(MpAttribute attribute)
        {
            var attributeDto = new AttributeDTO
            {
                AttributeId = attribute.AttributeId,
                Name = attribute.Name,
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
                CategoryId = attribute.CategoryId,
                Category = attribute.Category,
                CategoryDescription = attribute.CategoryDescription,
                EndDate = attribute.EndDate               
            };
            return attributeDto;
        }

        private static int GetOrCreateAttributeTypeDto(MpAttribute attribute, Dictionary<int, AttributeTypeDTO> attributeTypes)
        {
            var attributeTypeDto = new AttributeTypeDTO()
            {
                AttributeTypeId = attribute.AttributeTypeId,
                Name = attribute.AttributeTypeName,
                AllowMultipleSelections = !attribute.PreventMultipleSelection
            };

            var key = attributeTypeDto.AttributeTypeId;

            if (!attributeTypes.ContainsKey(key))
            {
                attributeTypes[key] = attributeTypeDto;
            }
            return key;
        }

        public List<MpAttribute> CreateMissingAttributes(List<MpAttribute> attributes, int attributeType)
        {

            var attributesToSearch = attributes.Select(a => $"(Attribute_Name='{a.Name.Replace("'", "''")}' AND Attribute_Category_Id={a.CategoryId})");

            string searchFilter = $"Attribute_Type_ID={attributeType} AND (" + String.Join(" OR ", attributesToSearch) + ")";

            var foundNames = _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetDefaultApiClientToken()).Search<MpRestAttribute>(searchFilter, "Attribute_ID, Attribute_Name, ATTRIBUTE_CATEGORY_ID");

            foreach (var attribute in attributes)
            {
                if (foundNames.Count(a => String.Equals(a.Name, attribute.Name, StringComparison.CurrentCultureIgnoreCase) && a.CategoryId == attribute.CategoryId) > 0)
                {
                    attribute.AttributeId = foundNames.First(foundAttribute => String.Equals(foundAttribute.Name, attribute.Name, StringComparison.CurrentCultureIgnoreCase) 
                                                            && foundAttribute.CategoryId == attribute.CategoryId).AttributeId;
                }
                else
                {
                    attributes.First(a => a.Name == attribute.Name && a.CategoryId == attribute.CategoryId)
                        .AttributeId = _attributeRepository.CreateAttribute(attribute);
                }
            }

            return attributes;
        }

        public List<AttributeCategoryDTO> GetAttributeCategory(int attributeTypeId)
        {
            List<MpAttributeCategory> mpCats = _attributeRepository.GetAttributeCategory(attributeTypeId);

            return Mapper.Map<List<AttributeCategoryDTO>>(mpCats);
        }

        public AttributeDTO GetOneAttributeByCategoryId(int categoryId)
        {
            MpAttribute mpCat = _attributeRepository.GetOneAttributeByCategoryId(categoryId);

            return Mapper.Map<AttributeDTO>(mpCat);
        }
    }
}