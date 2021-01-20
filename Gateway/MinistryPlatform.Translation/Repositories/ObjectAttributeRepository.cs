using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ObjectAttributeRepository : BaseRepository, IObjectAttributeRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ObjectAttributeRepository(IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IMinistryPlatformRestRepository ministryPlatformRest,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
        }

        public List<MpObjectAttribute> GetCurrentObjectAttributes(string token, int objectId, MpObjectAttributeConfiguration configuration, int? attributeIdFilter = null)
        {
            var table = configuration.TableName;
            string columns = $"{table}_Attribute_ID AS ObjectAttributeId, {table}_Attributes.Attribute_ID, {table}_Attributes.Start_Date, {table}_Attributes.End_Date, Attribute_ID_Table.Attribute_Type_ID, Notes, Attribute_ID_Table_Attribute_Category_ID_Table.Attribute_Category, Attribute_ID_Table_Attribute_Type_ID_Table.Attribute_Type, Attribute_ID_Table.Description";
            string filter = $"{table}_ID = {objectId} AND ({table}_Attributes.End_Date Is Null OR {table}_Attributes.End_Date >= GetDate())";

            if (attributeIdFilter != null)
            {
                filter += $" AND Attribute_ID_Table.Attribute_ID = {attributeIdFilter}";
            }

            var objectAttributes = _ministryPlatformRest.UsingAuthenticationToken(token).SearchTable<MpObjectAttribute>($"{table}_Attributes", filter, columns);
            return objectAttributes;
        }

        public int CreateAttribute(string token, int objectId, MpObjectAttribute attribute, MpObjectAttributeConfiguration configuration)
        {
            var attributeDictionary = TranslateAttributeToDictionary(attribute, configuration);
            var subPageId = configuration.SubPage;

            try
            {
                return _ministryPlatformService.CreateSubRecord(subPageId, objectId, attributeDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating object attribute, objectId: {0} attributeId: {1}",
                                        objectId,
                                        attribute.AttributeId);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public IObservable<int> CreateAttributeAsync(string token, int objectId, MpObjectAttribute attribute, MpObjectAttributeConfiguration configuration)
        {

            return Observable.Create<int>(o =>
            {
                var attributeDictionary = TranslateAttributeToDictionary(attribute, configuration);
                var subPageId = configuration.SubPage;
                var task = _ministryPlatformService.CreateSubRecordAsync(subPageId, objectId, attributeDictionary, token).ToObservable<int>();
                task.Subscribe();
                task.Catch<int, Exception>(tx =>
                {
                    var msg = string.Format("Error creating object attribute, objectId: {0} attributeId: {1}",
                                        objectId,
                                        attribute.AttributeId);
                    _logger.Error(msg, tx);
                    o.OnError(new ApplicationException());
                    return Observable.Return(-1);
                });
                o.OnNext(task.Wait());
                return Disposable.Empty;
            });
        }

        public void UpdateAttribute(string token, MpObjectAttribute attribute, MpObjectAttributeConfiguration configuration)
        {
            var attributeDictionary = TranslateAttributeToDictionary(attribute, configuration);
            var subPageId = configuration.SubPage;

            try
            {
                _ministryPlatformService.UpdateSubRecord(subPageId, attributeDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating object attribute, objectAttributeId: {0} attributeId: {1}",
                                        attribute.ObjectAttributeId, attribute.AttributeId);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void UpdateAttributeAsync(string token, MpObjectAttribute attribute, MpObjectAttributeConfiguration configuration)
        {
            var attributeDictionary = TranslateAttributeToDictionary(attribute, configuration);
            var subPageId = configuration.SubPage;

            try
            {
                _ministryPlatformService.UpdateSubRecordAsync(subPageId, attributeDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating object attribute, objectAttributeId: {0} attributeId: {1}",
                                        attribute.ObjectAttributeId, attribute.AttributeId);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }

        }

        private Dictionary<string, object> TranslateAttributeToDictionary(MpObjectAttribute attribute, MpObjectAttributeConfiguration configuration)
        {
            var keyColumn = string.Format("{0}_Attribute_ID", configuration.TableName);

            var attributeDictionary = new Dictionary<string, object>
            {
                {"Attribute_Type_ID", attribute.AttributeTypeId},
                {"Attribute_ID", attribute.AttributeId},
                {keyColumn, attribute.ObjectAttributeId},
                {"Start_Date", attribute.StartDate},
                {"End_Date", attribute.EndDate},
                {"Notes", attribute.Notes}
            };
            return attributeDictionary;
        }
    }
}
