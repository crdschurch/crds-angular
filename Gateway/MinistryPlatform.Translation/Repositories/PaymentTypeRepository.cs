using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class PaymentTypeRepository : IPaymentTypeRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;

        public PaymentTypeRepository(IMinistryPlatformRestRepository ministryPlatformRest, IApiUserRepository apiUserRepository)
        {
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }
        public MpPaymentType GetPaymentType(int paymentTypeId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpPaymentType>(paymentTypeId);
        }

        public bool PaymentTypeExists(int paymentTypeId)
        {
            return GetPaymentType(paymentTypeId) != null;
        }

    }
}
