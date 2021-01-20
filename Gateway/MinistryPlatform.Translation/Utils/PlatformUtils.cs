using System;
using MinistryPlatform.Translation.PlatformService;

namespace MinistryPlatform.Translation.Utils
{
    class PlatformUtils
    {
        public static T Call<T>(String token, Func<PlatformServiceClient, T> ministryPlatformFunc)
        {
            
            T result;
            var platformServiceClient = new PlatformService.PlatformServiceClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)platformServiceClient.InnerChannel))
            {
                if (System.ServiceModel.Web.WebOperationContext.Current != null)
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                result = ministryPlatformFunc(platformServiceClient);                
            }
            return result;
        }

        public static void VoidCall(String token, Action<PlatformServiceClient> ministryPlatformFunc)
        {
            var platformServiceClient = new PlatformService.PlatformServiceClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)platformServiceClient.InnerChannel))
            {
                if (System.ServiceModel.Web.WebOperationContext.Current != null)
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                ministryPlatformFunc(platformServiceClient);
            }
        }

    }
}
