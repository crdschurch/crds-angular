using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class AttributeRepository : BaseRepository, IAttributeRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly int _attributesPageId = Convert.ToInt32(AppSettings("Attributes"));
        private readonly IApiUserRepository _apiUserService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        public AttributeRepository(IMinistryPlatformService ministryPlatformService,
                                   IAuthenticationRepository authenticationService,
                                   IConfigurationWrapper configurationWrapper,
                                   IApiUserRepository apiUserService,
                                   IMinistryPlatformRestRepository ministryPlatformRest)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserService = apiUserService;
        }
        
        public List<MpAttribute> GetAttributes(int? attributeTypeId)
        {
            var token = base.ApiLogin();
            const string COLUMNS =
                "Attribute_ID, Attribute_Name, Attributes.Description, Attribute_Category_ID_Table.Attribute_Category, Attributes.Attribute_Category_ID, Attribute_Category_ID_Table.Description as Attribute_Category_Description, Attributes.Sort_Order, Attribute_Type_ID_Table.Attribute_Type_ID, Attribute_Type_ID_Table.Attribute_Type, Attribute_Type_ID_Table.Prevent_Multiple_Selection, Start_Date, End_Date";

            var search = attributeTypeId.HasValue ? $"Attributes.Attribute_Type_ID = { attributeTypeId}  AND " : string.Empty;
            search += "(Attributes.Start_Date Is Null OR Attributes.Start_Date <= GetDate()) ";
            search += "AND (Attributes.End_Date Is Null OR Attributes.End_Date >= GetDate())";
            var orderBy = "Attribute_Type_ID_Table.[Attribute_Type_ID], Attributes.[Sort_Order], Attributes.[Attribute_Name]";
            var records = _ministryPlatformRest.UsingAuthenticationToken(token).Search<MpAttribute>(search, COLUMNS, orderBy, false);
            return records;
        }

        public int CreateAttribute(MpAttribute attribute)
        {
            var token = base.ApiLogin();
            var values = new Dictionary<string, object>
                    {
                        {"Attribute_Name", attribute.Name.ToLower()},
                        {"Attribute_Category_ID", attribute.CategoryId},
                        {"Attribute_Type_ID", attribute.AttributeTypeId},
                        {"PreventMultipleSelection", attribute.PreventMultipleSelection},
                        {"Sort_Order", attribute.SortOrder}
                    };

            return _ministryPlatformService.CreateRecord(_attributesPageId, values, token, true);
        }
        
        public List<MpAttributeCategory> GetAttributeCategory(int attributeTypeId)
        {
            //cat cols haha
            const string catCols = "Attribute_Category_ID_table.*";
            string catSearch = $"attribute_type_id = {attributeTypeId}";
            const string orderBy = "Attribute_Category_ID_table.Sort_Order";
            const bool distinct = true;

            return _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpAttribute, MpAttributeCategory>(catSearch, catCols, orderBy, distinct);
        }

        public MpAttribute GetOneAttributeByCategoryId(int categoryId)
        {
            string atSearch = $"attribute_category_id = {categoryId}";
            atSearch += " AND (Attributes.Start_Date Is Null OR Attributes.Start_Date <= GetDate())";
            atSearch += " AND (Attributes.End_Date Is Null OR Attributes.End_Date >= GetDate())";

            var ret = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpAttribute>(atSearch, (string)null, (string)null, true);
            return ret.FirstOrDefault();

        }
    }
}