using MinistryPlatform.Translation.Helpers;
using MinistryPlatform.Translation.PlatformService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web.Security;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Services;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MinistryPlatform.Translation.Repositories
{
    public class MinistryPlatformServiceImpl : IMinistryPlatformService
    {
        private PlatformServiceClient _platformServiceClient;
        private IConfigurationWrapper _configurationWrapper;

        /// <summary>
        /// This is the cookie name MinistryPlatform looks for when impersonating a user.
        /// </summary>
        private readonly string _impersonateCookieName;

        public MinistryPlatformServiceImpl(PlatformServiceClient platformServiceClient, IConfigurationWrapper configurationWrapper)
        {
            _platformServiceClient = platformServiceClient;
            _configurationWrapper = configurationWrapper;

            _impersonateCookieName = string.Format("{0}.1", FormsAuthentication.FormsCookieName);
        }

       public List<Dictionary<string, object>> GetLookupRecords(int pageId, String token)
        {
            return MPFormatConversion.MPFormatToList(GetPageLookupRecords(token, pageId, string.Empty, string.Empty, 0));
        }

       public List<Dictionary<string, object>> GetLookupRecords(String token, int pageId, string search, string sort, int maxNumberOfRecordsToReturn = 100)
       {
           return MPFormatConversion.MPFormatToList(GetPageLookupRecords(token, pageId, search, sort, maxNumberOfRecordsToReturn));
       }

        public Dictionary<string, object> GetLookupRecord(int pageId, string search, String token, int maxNumberOfRecordsToReturn = 100)
        {
            return GetLookupRecord(token, pageId, search, string.Empty, maxNumberOfRecordsToReturn );
        }

        public Dictionary<string, object> GetLookupRecord(String token, int pageId, string search, string sort, int maxNumberOfRecordsToReturn = 100)
        {
            return MPFormatConversion.MPFormatToDictionary(GetPageLookupRecords(token, pageId, search, sort, maxNumberOfRecordsToReturn));
        }

        private SelectQueryResult GetPageLookupRecords(String token, int pageId, string search, string sort, int maxNumberOfRecordsToReturn = 100)
        {
            return Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageLookupRecords(pageId, search, sort, maxNumberOfRecordsToReturn));
        }

        public SelectQueryResult GetRecords(int pageId, String token, String search = "", String sort = "")
        {
            return Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageRecords(pageId, search, sort, 0));
        }

        public JArray GetRecordsArr(int pageId, String token, String search = "", String sort = "")
        {
            return MPFormatConversion.MPFormatToJson(GetRecords(pageId, token, search, sort));
        }

        public List<Dictionary<string, object>> GetRecordsDict(int pageId, String token, String search = "", String sort = "")
        {
            return MPFormatConversion.MPFormatToList(GetRecords(pageId, token, search, sort));
        }

        public List<Dictionary<string, object>> GetRecordsDict(string pageKey, String token, String search = "", String sort = "")
        {
            return MPFormatConversion.MPFormatToList(GetRecords(GetMinistryPlatformId(pageKey), token, search, sort));
        }

        public List<Dictionary<string, object>> GetSelectionsForPageDict(int pageId, int selectionId, String token)
        {
            var selections = GetSelectionsDict(selectionId, token);
            var selectionPageRecords = new List<Dictionary<string, object>>();

            foreach (var selection in selections)
            {
                object recordId;
                selection.TryGetValue("dp_RecordID", out recordId);
                selectionPageRecords.Add(GetRecordDict(pageId, Convert.ToInt32(recordId), token));
            }

            return selectionPageRecords;
        }

        public List<Dictionary<string, object>> GetSelectionsDict(int selectionId, String token, String search = "", String sort = "")
        {
            return MPFormatConversion.MPFormatToList(GetSelectionRecords(selectionId, token, search, sort));
        }

        public SelectQueryResult GetSelectionRecords(int selectionId, String token, String search = "", String sort = "")
        {
            return Call(token, platformClient => platformClient.GetSelectionRecords(selectionId, search, sort, 0));
        }

        public SelectQueryResult GetRecord(int pageId, int recordId, String token, bool quickadd = false)
        {
            return Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageRecord(pageId, recordId, quickadd));
        }

        public JArray GetRecordArr(int pageId, int recordId, String token, bool quickadd = false)
        {
            return MPFormatConversion.MPFormatToJson(GetRecord(pageId, recordId, token, quickadd));
        }

        public Dictionary<string, object> GetRecordDict(int pageId, int recordId, String token, bool quickadd = false)
        {
            return MPFormatConversion.MPFormatToDictionary(GetRecord(pageId, recordId, token, quickadd));
        }

        public Dictionary<string, object> GetSubPageRecord(string subPageKey, int recordId, String token)
        {
            var subPageId = GetMinistryPlatformId(subPageKey);
            var result = Call<SelectQueryResult>(token,
                                                 platformClient => platformClient.GetSubpageRecord(subPageId, recordId, false));
            return MPFormatConversion.MPFormatToDictionary(result);
        }

        public List<Dictionary<string, object>> GetSubPageRecords(int subPageId, int recordId, String token)
        {
            SelectQueryResult result = Call<SelectQueryResult>(token,
                platformClient => platformClient.GetSubpageRecords(subPageId, recordId, string.Empty, string.Empty, 0));
            return MPFormatConversion.MPFormatToList(result);
        }

        public List<Dictionary<string, object>> GetSubPageRecords(string subPageKey, int recordId, String token)
        {
            var subPageId = GetMinistryPlatformId(subPageKey);
            return GetSubPageRecords(subPageId, recordId, token);
        }

        public List<Dictionary<string, object>> GetSubpageViewRecords(int viewId, int recordId,
           string token, string searchString = "", string sort = "", int top = 0)
        {
            var result = Call<SelectQueryResult>(token,
                platformClient => platformClient.GetSubpageViewRecords(viewId, recordId, searchString, sort, top));
            return MPFormatConversion.MPFormatToList(result);
        }

        public List<Dictionary<string, object>> GetSubpageViewRecords(string viewKey, int recordId,
           string token, string searchString = "", string sort = "", int top = 0)
        {
            var result = Call<SelectQueryResult>(token,
                platformClient => platformClient.GetSubpageViewRecords(GetMinistryPlatformId(viewKey), recordId, searchString, sort, top));
            return MPFormatConversion.MPFormatToList(result);
        }

        public List<Dictionary<string, object>> GetPageViewRecords(int viewId, string token, string searchString = "", string sort = "",
            int top = 0)
        {
            var result = Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageViewRecords(viewId, searchString, sort, top));
            return MPFormatConversion.MPFormatToList(result);
        }

        public List<Dictionary<string, object>> GetPageViewRecords(string viewKey, string token, string searchString = "", string sort = "",
            int top = 0)
        {
            var result = Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageViewRecords(GetMinistryPlatformId(viewKey), searchString, sort, top));
            return MPFormatConversion.MPFormatToList(result);
        }

        public int CreateRecord(int pageId, Dictionary<string, object> dictionary, String token,
            bool quickadd = false)
        {
            return Call<int>(token,
                platformClient => platformClient.CreatePageRecord(pageId, dictionary, quickadd));
        }

        public int CreateRecord(string pageKey, Dictionary<string, object> dictionary, String token,
            bool quickadd = false)
        {
            return Call<int>(token,
                platformClient => platformClient.CreatePageRecord(GetMinistryPlatformId(pageKey), dictionary, quickadd));
        }

        public int CreateSubRecord(int subPageId, int parentRecordId, Dictionary<string, object> dictionary,
            String token, bool quickadd = false)
        {
            return Call<int>(token,
                platformClient => platformClient.CreateSubpageRecord(subPageId, parentRecordId, dictionary, quickadd));
        }

        public Task<int> CreateSubRecordAsync(int subPageId,
                                              int parentRecordId,
                                              Dictionary<string, object> dictionary,
                                              String token,
                                              bool quickadd = false)
        {
            return Call<Task<int>>(token,
                platformClient => platformClient.CreateSubpageRecordAsync(subPageId, parentRecordId, dictionary, quickadd));
        }

        public int CreateSubRecord(string subPageKey, int parentRecordId, Dictionary<string, object> dictionary,
            String token, bool quickadd = false)
        {
            var subPageId = GetMinistryPlatformId(subPageKey);
            return Call<int>(token,
                platformClient => platformClient.CreateSubpageRecord(subPageId, parentRecordId, dictionary, quickadd));
        }

        public void UpdateFile(Int32 fileId, String fileName, String description, Boolean isDefaultImage, Int32 longestDimension, Byte[] file, String token)
        {
            VoidCall(token, platformClient => 
                platformClient.UpdateFile(fileId, fileName, description, isDefaultImage, longestDimension, file));
        }

        public FileDescription CreateFile(String pageName, Int32 recordId, String fileName, String description, Boolean isDefaultImage, Int32 longestDimension, Byte[] file, String token)
        {
            var result = Call<FileDescription>(token, platformClient =>
                platformClient.CreateFile(GetMinistryPlatformId(pageName), recordId, fileName, description, isDefaultImage, longestDimension, file));
            return result;
        }

        public Stream GetFile(Int32 fileId, String token)
        {
            var result = Call<Stream>(token, platformClient => platformClient.GetFile(fileId, false));
            return result;
        }

        public FileDescription GetFileDescription(Int32 fileId, String token)
        {
            var result = Call<FileDescription>(token, platformClient => platformClient.GetFileDescription(fileId));
            return result;
        }

        public FileDescription[] GetFileDescriptions(String pageName, Int32 recordId, String token)
        {
            var result = Call<FileDescription[]>(token,
                platformClient => platformClient.GetFileDescriptions(GetMinistryPlatformId(pageName), recordId));
            return result;
        }

        /*
         * Think carefully before using, in most cases we don't want to delete records, but end date them, or mark them as inactive
         */
        public int DeleteRecord(int pageId, int recordId, DeleteOption[] deleteOptions, String token)
        {
            VoidCall(token,
                platformClient => platformClient.DeletePageRecord(pageId, recordId, deleteOptions));
            return recordId;
        }

        public void UpdateRecord(int pageId, Dictionary<string, object> dictionary, String token)
        {
            VoidCall(token, platformClient => platformClient.UpdatePageRecord(pageId, dictionary, false));
        }

        public void UpdateSubRecord(int subpageId, Dictionary<string, object> dictionary, String token)
        {            
            VoidCall(token, platformClient => platformClient.UpdateSubpageRecord(subpageId, dictionary, false));
        }

        public void UpdateSubRecord(string subPageKey, Dictionary<string, object> subscription, string token)
        {
            var subPageId = GetMinistryPlatformId(subPageKey);
            UpdateSubRecord(subPageId, subscription, token);
        }

        //public void CopyPageRecord(int pageId, int recordId, int[] subpages, System.DateTime[] startDateTimes, bool updateOriginalRecord)
        //{
        //    base.Channel.CopyPageRecord(pageId, recordId, subpages, startDateTimes, updateOriginalRecord);
        //}

        public void CopyPageRecord(int pageId, int recordId, int[] subpages, System.DateTime[] startDateTimes, bool updateOriginalRecord, string token)
        {
            VoidCall(token, platformClient => platformClient.CopyPageRecord(pageId, recordId, subpages, startDateTimes, updateOriginalRecord));
        }

        public void CopyPageRecordAsync(int pageId, int recordId, int[] subpages, System.DateTime[] startDateTimes, bool updateOriginalRecord, string token)
        {
            VoidCall(token, platformClient => platformClient.CopyPageRecord(pageId, recordId, subpages, startDateTimes, updateOriginalRecord));
        }

        public UserInfo GetContactInfo(string token)
        {
            return Call<UserInfo>(token, platformClient => platformClient .GetCurrentUserInfo());
        }

        public void CompleteTask(string token, int taskId, bool rejected, string comments)
        {
            VoidCall(token, platformClient => platformClient.CompleteTask(taskId, rejected, comments));
        }

        public void UpdateSubRecordAsync(int subPageId, Dictionary<string, object> attributeDictionary, string token)
        {
            Call<Task>(token, platformClient => platformClient.UpdateSubpageRecordAsync(subPageId, attributeDictionary, false));
        }

        public int CreateSelection(SelectionDescription selectionDescription, string token)
        {
            return Call<int>(token, platformClient => platformClient.CreateSelection(selectionDescription));
        }

        public void AddToSelection(int selectionId, int[] recordIds, string token)
        {
            VoidCall(token, platformClient => platformClient.AddToSelection(selectionId, false, recordIds));
        }

        public void DeleteSelection(int selectionId, string token)
        {
            VoidCall(token, platformClient => platformClient.DeleteSelection(selectionId));
        }

        public void DeleteSelectionRecords(int selectionId, string token)
        {
            DeleteOption option = new DeleteOption();
            option.Action = DeleteAction.Delete;
            DeleteOption[] options = new DeleteOption[]
            {
                option
            };

            VoidCall(token, platformClient => platformClient.DeleteSelectionRecords(selectionId, options));
        }

        public void RemoveSelection(int selectionId, int[] records, String token)
        {
            VoidCall(token, platformClient => platformClient.RemoveFromSelection(selectionId, records));
        }

        private int GetMinistryPlatformId(string mpKey)
        {
            return _configurationWrapper.GetConfigIntValue(mpKey);
        }

        private T Call<T>(string token, Func<PlatformServiceClient, T> ministryPlatformFunc)
        {
            T result;
            using (new OperationContextScope(_platformServiceClient.InnerChannel))
            {
                if (System.ServiceModel.Web.WebOperationContext.Current != null)
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                    Impersonate(token);
                }

                result = ministryPlatformFunc(_platformServiceClient);
            }
            return result;
        }

        private void VoidCall(string token, Action<PlatformServiceClient> ministryPlatformFunc)
        {
            using (new OperationContextScope(_platformServiceClient.InnerChannel))
            {
                if (System.ServiceModel.Web.WebOperationContext.Current != null)
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                    Impersonate(token);
                }
                ministryPlatformFunc(_platformServiceClient);
            }
        }

        /// <summary>
        /// This method sets an impersonation cookie on the OutgoingMessageProperties.HttpRequest.  MinistryPlatform looks for this to be set
        /// to a GUID of a User, and if set, all requests to MP will act as though that user is executing them, rather than the actual
        /// authenticated user.  This looks at the <see cref="ImpersonatedUserGuid"/> ThreadLocal to see if there is a user to impersonate.
        /// </summary>
        private void Impersonate(string currentToken)
        {
            if (!ImpersonatedUserGuid.HasValue())
            {
                return;
            }

            bool userTokenIsSameAsImpersonateToken = ImpersonatedUserGuid.GetToken() == currentToken;
            bool impersonateTokenEqualsNewAuthToken = ImpersonatedUserGuid.GetToken() == ImpersonatedUserGuid.NewAuthImpersonateToken;
            if (!userTokenIsSameAsImpersonateToken && !impersonateTokenEqualsNewAuthToken)
            {
                return;
            }

            var httpRequest = OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] as
                HttpRequestMessageProperty;
            if (httpRequest == null)
            {
                httpRequest = new HttpRequestMessageProperty();
                OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, httpRequest);
            }

            var cookies = new CookieContainer();
            cookies.Add(_platformServiceClient.Endpoint.Address.Uri, new Cookie(_impersonateCookieName, ImpersonatedUserGuid.Get()));
            httpRequest.Headers.Add(HttpRequestHeader.Cookie, cookies.GetCookieHeader(_platformServiceClient.Endpoint.Address.Uri));
        }
    }
}
