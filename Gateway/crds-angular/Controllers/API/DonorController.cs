using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Security;
using crds_angular.Services.Analytics;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Controllers.API
{
    public class DonorController : ImpersonateAuthBaseController
    {
        private readonly IDonorService _donorService;
        private readonly IPaymentProcessorService _stripePaymentService;
        private readonly IDonationService _donationService;
        private readonly MPInterfaces.IDonorRepository _mpDonorService;
        private readonly IAuthenticationRepository _authenticationService;
        private readonly IUserImpersonationService _impersonationService;
        private readonly IAnalyticsService _analyticsService;

        public DonorController(IAuthTokenExpiryService authTokenExpiryService, 
                                IDonorService donorService,
                                IPaymentProcessorService stripePaymentService, 
                                IDonationService donationService, 
                                MPInterfaces.IDonorRepository mpDonorService, 
                                IAuthenticationRepository authenticationService,
                                IUserImpersonationService impersonationService,
                                IAnalyticsService analyticsService) 
            : base(authTokenExpiryService, impersonationService, authenticationService)
        {
            _donorService = donorService;
            _stripePaymentService = stripePaymentService;
            _donationService = donationService;
            _authenticationService = authenticationService;
            _mpDonorService = mpDonorService;
            _impersonationService = impersonationService;
            _analyticsService = analyticsService;
        }
    
        [ResponseType(typeof(DonorDTO))]
        [VersionedRoute(template: "donor/{email?}", minimumVersion: "1.0.0")]
        [Route("donor/{email?}")]
        public IHttpActionResult Get(string email="")
        {
            return (Authorized(GetDonorForAuthenticatedUser, () => GetDonorForUnauthenticatedUser(email)));
        }

        [ResponseType(typeof (DonorDTO))]
        [VersionedRoute(template: "donor", minimumVersion: "1.0.0")]
        [Route("donor")]
        public IHttpActionResult Post([FromBody] CreateDonorDTO dto)
        {
            return (Authorized(authDto => CreateDonorForAuthenticatedUser(authDto, dto), () => CreateDonorForUnauthenticatedUser(dto)));
        }

        private IHttpActionResult CreateDonorForUnauthenticatedUser(CreateDonorDTO dto)
        {
            MpContactDonor donor;
            try
            {
                donor = _donorService.GetContactDonorForEmail(dto.email_address);
            }
            catch (Exception e)
            {
                var msg = "Error getting donor for email " + dto.email_address;
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
            int existingDonorId = 
                (donor == null) ? 
                    0 : 
                    donor.DonorId;

            try
            {
                donor = _donorService.CreateOrUpdateContactDonor(donor, String.Empty, dto.first_name, dto.last_name, dto.email_address, dto.stripe_token_id, DateTime.Now);
            }
            catch (PaymentProcessorException e)
            {
                return (e.GetStripeResult());
            }
            catch (Exception e)
            {
                var msg = "Error creating donor for email " + dto.email_address;
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }

            var responseBody = new DonorDTO
            {
                Id = donor.DonorId,
                ProcessorId = donor.ProcessorId,
                RegisteredUser = false,
                Email = donor.Email
            };

            // HTTP StatusCode should be 201 (Created) if we created a donor, or 200 (Ok) if returning an existing donor
            var statusCode =
                (existingDonorId == donor.DonorId) ?
                    HttpStatusCode.OK :
                    HttpStatusCode.Created;
            return (ResponseMessage(Request.CreateResponse(statusCode, responseBody)));
        }

        private IHttpActionResult CreateDonorForAuthenticatedUser(AuthDTO authDto, CreateDonorDTO dto)
        {
            try
            {
                var donor = _donorService.GetContactDonorByContactId(authDto.UserInfo.Mp.ContactId);
                donor = _donorService.CreateOrUpdateContactDonor(donor, string.Empty, string.Empty, string.Empty, string.Empty, dto.stripe_token_id, DateTime.Now);

                var response = new DonorDTO
                {
                    Id = donor.DonorId,
                    ProcessorId = donor.ProcessorId,
                    RegisteredUser = true,
                    Email = donor.Email
                };

                return Ok(response);
            }
            catch (PaymentProcessorException e)
            {
                return (e.GetStripeResult());
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Donor Post Failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        private IHttpActionResult GetDonorForAuthenticatedUser(AuthDTO authDto)
        {
            try
            {
                var donor = _donorService.GetContactDonorByContactId(authDto.UserInfo.Mp.ContactId);

                if (donor == null || !donor.HasPaymentProcessorRecord)
                {
                    return (NotFound());
                }
                else
                {
                    var defaultSource = _stripePaymentService.GetDefaultSource(donor.ProcessorId);
                    
                    var response = new DonorDTO()   
                    {
                        Id = donor.DonorId,
                        ProcessorId = donor.ProcessorId,
                        DefaultSource = new DefaultSourceDTO
                        {
                            credit_card = new CreditCardDTO
                            {
                              last4 = defaultSource.last4,
                              brand = defaultSource.brand,
                              address_zip = defaultSource.address_zip,
                              exp_date = defaultSource.exp_month + defaultSource.exp_year
                            },
                            bank_account = new BankAccountDTO
                            {
                              last4 = defaultSource.bank_last4,
                              routing = defaultSource.routing_number,
                              accountHolderName = defaultSource.account_holder_name,
                              accountHolderType = defaultSource.account_holder_type
                            }
                         },
                         RegisteredUser = donor.RegisteredUser,
                         Email = donor.Email
                    };

                    return Ok(response);
                }
            }
            catch (PaymentProcessorException stripeException)
            {
                return (stripeException.GetStripeResult());
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Donor Get Failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        private IHttpActionResult GetDonorForUnauthenticatedUser(string email)
        {
            try
            {
                var donor = _donorService.GetContactDonorForEmail(email);
                if (donor == null || !donor.HasPaymentProcessorRecord)
                {
                    return NotFound();
                }
                else
                {
                    var response = new DonorDTO
                    {
                        Id = donor.DonorId,
                        ProcessorId = donor.ProcessorId,
                        RegisteredUser = donor.RegisteredUser,
                        Email = donor.Email
                    };

                    return Ok(response); 
                }
            }
            catch (PaymentProcessorException stripeException)
            {
                return (stripeException.GetStripeResult());
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Donor Get Failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof (DonorDTO))]
        [VersionedRoute(template: "donor", minimumVersion: "1.0.0")]
        [Route("donor")]
        [HttpPut]
        public IHttpActionResult UpdateDonor([FromBody] UpdateDonorDTO dto)
        {
            return (Authorized(authDto => UpdateDonor(authDto.UserInfo.Mp.ContactId, dto), () => UpdateDonor(null, dto)));
        }

        private IHttpActionResult UpdateDonor(int? contactId, UpdateDonorDTO dto)
        {
            MpContactDonor mpContactDonor;
            SourceData sourceData;

            try
            {
                mpContactDonor = 
                    contactId == null ? 
                    _donorService.GetContactDonorForEmail(dto.EmailAddress) 
                    : 
                    _donorService.GetContactDonorByContactId(contactId.Value);
              
                sourceData = _stripePaymentService.UpdateCustomerSource(mpContactDonor.ProcessorId, dto.StripeTokenId);
            }
            catch (PaymentProcessorException stripeException)
            {
                return (stripeException.GetStripeResult());
            }
            catch (ApplicationException applicationException)
            {
                var apiError = new ApiErrorDto("Error calling Ministry Platform" + applicationException.Message, applicationException);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }

            //return donor
            var donor = new DonorDTO
            {
                Id = mpContactDonor.DonorId,
                ProcessorId = mpContactDonor.ProcessorId,
                DefaultSource = new DefaultSourceDTO
                {
                    credit_card = new CreditCardDTO
                    {
                        brand = sourceData.brand,
                        last4 = sourceData.last4,
                        address_zip = sourceData.address_zip,
                        exp_date = sourceData.exp_month + sourceData.exp_year
                    },
                    bank_account = new BankAccountDTO
                    {
                        last4 = sourceData.bank_last4,
                        routing = sourceData.routing_number,                        
                        accountHolderName = sourceData.account_holder_name,
                        accountHolderType = sourceData.account_holder_type
                    }
                },
                RegisteredUser = mpContactDonor.RegisteredUser,
                Email = mpContactDonor.Email
            };

            return Ok(donor);
        }

        /// <summary>
        /// Create a recurring gift for the authenticated user.
        /// </summary>
        /// <param name="recurringGiftDto">The data required to setup the recurring gift in MinistryPlatform and Stripe.</param>
        /// <param name="impersonateDonorId">An optional donorId of a donor to impersonate</param>
        /// <returns>The input RecurringGiftDto, with donor email address and recurring gift ID from MinistryPlatform populated</returns>
        //[RequiresAuthorization]
        //[ResponseType(typeof(RecurringGiftDto))]
        //[VersionedRoute(template: "donor/recurrence", minimumVersion: "1.0.0")]
        //[Route("donor/recurrence")]
        //[HttpPost]
        //public IHttpActionResult CreateRecurringGift([FromBody] RecurringGiftDto recurringGiftDto, [FromUri(Name = "impersonateDonorId")] int? impersonateDonorId = null)
        //{
        //    return (Authorized(token =>
        //    {
        //        var impersonateUserId = impersonateDonorId == null ? string.Empty : _mpDonorService.GetEmailViaDonorId(impersonateDonorId.Value).Email;

        //        try
        //        {
        //            var contactDonor = (impersonateDonorId != null)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.GetContactDonorForAuthenticatedUser(token))
        //                : _donorService.GetContactDonorForAuthenticatedUser(token);
        //            var donor = _donorService.CreateOrUpdateContactDonor(contactDonor, string.Empty,string.Empty, string.Empty, string.Empty);
        //            var recurringGift = !string.IsNullOrWhiteSpace(impersonateUserId)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.CreateRecurringGift(token, recurringGiftDto, donor, donor.Email, donor.Details?.DisplayName))
        //                : _donorService.CreateRecurringGift(token, recurringGiftDto, donor, donor.Email, donor.Details?.DisplayName);

        //            recurringGiftDto.EmailAddress = donor.Email;
        //            recurringGiftDto.RecurringGiftId = recurringGift;

        //            _analyticsService.Track(donor.ContactId.ToString(), "PaymentSucceededServerSide", new EventProperties() { { "Url", recurringGiftDto.SourceUrl }, { "FundingMethod", recurringGiftDto.Source }, { "Email", "" }, { "CheckoutType", "Registered" }, { "Amount", recurringGiftDto.PlanAmount } });

        //            return Ok(recurringGiftDto);
        //        }
        //        catch (PaymentProcessorException stripeException)
        //        {
        //            return (stripeException.GetStripeResult());
        //        }
        //        catch (ApplicationException applicationException)
        //        {
        //            var apiError = new ApiErrorDto("Error calling Ministry Platform " + applicationException.Message, applicationException);
        //            throw new HttpResponseException(apiError.HttpResponseMessage);
        //        }
                
        //    }));
        //}

        /// <summary>
        /// Edit a recurring gift for the authenticated user.
        /// </summary>
        /// <param name="recurringGiftId">The ID of the recurring gift to edit</param>
        /// <param name="editGift">The data required to edit the recurring gift in MinistryPlatform and/or Stripe.</param>
        /// <param name="impersonateDonorId">An optional donorId of a donor to impersonate</param>
        /// <returns>The input RecurringGiftDto, with donor email address and recurring gift ID from MinistryPlatform populated</returns>
        //[RequiresAuthorization]
        //[ResponseType(typeof(RecurringGiftDto))]
        //[VersionedRoute(template: "donor/recurrence/{recurringGiftId}", minimumVersion: "1.0.0")]
        //[Route("donor/recurrence/{recurringGiftId:int}")]
        //[HttpPut]
        //public IHttpActionResult EditRecurringGift([FromUri]int recurringGiftId, [FromBody] RecurringGiftDto editGift, [FromUri(Name = "impersonateDonorId")] int? impersonateDonorId = null)
        //{
        //    editGift.RecurringGiftId = recurringGiftId;

        //    return (Authorized(token =>
        //    {
        //        var impersonateUserId = impersonateDonorId == null ? string.Empty : _mpDonorService.GetEmailViaDonorId(impersonateDonorId.Value).Email;

        //        try
        //        {
        //            var donor = (impersonateDonorId != null)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.GetContactDonorForAuthenticatedUser(token))
        //                : _donorService.GetContactDonorForAuthenticatedUser(token);

        //            var recurringGift = !string.IsNullOrWhiteSpace(impersonateUserId)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.EditRecurringGift(token, editGift, donor))
        //                : _donorService.EditRecurringGift(token, editGift, donor);

        //            return Ok(recurringGift);
        //        }
        //        catch (PaymentProcessorException stripeException)
        //        {
        //            return (stripeException.GetStripeResult());
        //        }
        //        catch (ApplicationException applicationException)
        //        {
        //            var apiError = new ApiErrorDto("Error calling Ministry Platform " + applicationException.Message, applicationException);
        //            throw new HttpResponseException(apiError.HttpResponseMessage);
        //        }

        //    }));
        //}

        /// <summary>
        /// Cancel a recurring gift for the authenticated user.  This simply end-dates the gift, and cancels the subscription at Stripe.
        /// </summary>
        /// <param name="recurringGiftId">The recurring gift ID to delete in MinistryPlatform and Stripe.</param>
        /// <param name="impersonateDonorId">An optional donorId of a donor to impersonate</param>
        /// <param name="sendEmail">An optional flag to disable or enable sending an email after canceling recurring gift</param>
        /// <returns>The RecurringGiftDto representing the gift that was deleted</returns>
        //[RequiresAuthorization]
        //[VersionedRoute(template: "donor/recurrence/{recurringGiftId}", minimumVersion: "1.0.0")]
        //[Route("donor/recurrence/{recurringGiftId:int}")]
        //[HttpDelete]
        //public IHttpActionResult CancelRecurringGift([FromUri]int recurringGiftId, [FromUri(Name = "impersonateDonorId")] int? impersonateDonorId = null, [FromUri(Name = "sendEmail")] bool sendEmail = true)
        //{
        //    return(Authorized(token =>
        //    {

        //        try
        //        {
        //            var impersonateUserId = impersonateDonorId == null ? string.Empty : _mpDonorService.GetEmailViaDonorId(impersonateDonorId.Value).Email;

        //            var result = (impersonateDonorId != null)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.CancelRecurringGift(token, recurringGiftId, sendEmail))
        //                : _donorService.CancelRecurringGift(token, recurringGiftId, sendEmail);

        //            return (Ok());
        //        }
        //        catch (PaymentProcessorException stripeException)
        //        {
        //            return (stripeException.GetStripeResult());
        //        }
        //        catch (ApplicationException applicationException)
        //        {
        //            var apiError = new ApiErrorDto("Error calling Ministry Platform " + applicationException.Message, applicationException);
        //            throw new HttpResponseException(apiError.HttpResponseMessage);
        //        }
        //    }));
        //}


        /// <summary>
        /// Retrieve list of recurring gifts for the logged-in donor.
        /// </summary>
        /// <param name="impersonateDonorId">An optional donorId of a donor to impersonate</param>
        /// <returns>A list of RecurringGiftDto</returns>
        // [RequiresAuthorization]
        //[ResponseType(typeof(List<RecurringGiftDto>))]
        //[VersionedRoute(template: "donor/recurrence", minimumVersion: "1.0.0")]
        //[Route("donor/recurrence")]
        //[HttpGet]
        //public IHttpActionResult GetRecurringGifts([FromUri(Name = "impersonateDonorId")] int? impersonateDonorId = null)
        //{
        //    return (Authorized(token =>
        //    {
        //        var impersonateUserId = impersonateDonorId == null ? string.Empty : _mpDonorService.GetEmailViaDonorId(impersonateDonorId.Value).Email;
                
        //        try
        //        {
        //            var recurringGifts = (impersonateDonorId != null)
        //                ? _impersonationService.WithImpersonation(token,
        //                                                          impersonateUserId,
        //                                                          () =>
        //                                                              _donorService.GetRecurringGiftsForAuthenticatedUser(token)) 
        //                : _donorService.GetRecurringGiftsForAuthenticatedUser(token);

        //            if (recurringGifts == null || !recurringGifts.Any())
        //            {
        //                return (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.NotFound, new ApiErrorDto("No matching donations found")));
        //            }

        //            return (Ok(recurringGifts));
        //        }
        //        catch (UserImpersonationException e)
        //        {
        //            return (e.GetRestHttpActionResult());
        //        }
        //    }));
        //}

        ///// <summary>
        ///// Retrieve list of capital campaign pledges for the logged-in donor.
        ///// </summary>
        ///// <returns>A list of PledgeDto</returns>
        //[ResponseType(typeof(List<PledgeDto>))]
        //[VersionedRoute(template: "donor/pledge", minimumVersion: "1.0.0")]
        //[Route("donor/pledge")]
        //[HttpGet]
        //public IHttpActionResult GetPledges()
        //{
        //    return (Authorized(authDto =>
        //    {
        //        try
        //        {
        //            var pledges = _donorService.GetCapitalCampaignPledgesForAuthenticatedUser(authDto);

        //            if (pledges == null || !pledges.Any())
        //            {
        //                return (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.NotFound, new ApiErrorDto("No matching commitments found")));
        //            }

        //            return (Ok(pledges));
        //        }
        //        catch (UserImpersonationException e)
        //        {
        //            return (e.GetRestHttpActionResult());
        //        }
        //    }));
        //}
    }
}

