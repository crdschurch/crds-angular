using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Camp;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class CampController : ImpersonateAuthBaseController
    {
        private readonly ICampService _campService;

        public CampController(IAuthTokenExpiryService authTokenExpiryService, 
                              ICampService campService, 
                              IUserImpersonationService userImpersonationService, 
                              IAuthenticationRepository authenticationRepository) 
                              : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _campService = campService;
        }

        [VersionedRoute(template: "camps/{eventId}/family", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/family")]
        [HttpGet]
        public IHttpActionResult GetCampFamily(int eventId)
        {
            return Authorized(token =>
            {
                try
                {
                    var members = _campService.GetEligibleFamilyMembers(eventId, token);
                    return Ok(members);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Camp Family", e
                        );
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(List<MyCampDTO>))]
        [VersionedRoute(template: "camps/my-camp", minimumVersion: "1.0.0")]
        [Route("camps/my-camp")]
        [HttpGet]
        public IHttpActionResult GetMyCampsInfo()
        {
            return Authorized(token =>
            {
                try
                {
                    var myCampsInfo = _campService.GetMyCampInfo(token);
                    return Ok(myCampsInfo);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("My Camp Info", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof (CampDTO))]
        [VersionedRoute(template: "camps/{eventId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}")]
        [HttpGet]
        public IHttpActionResult GetCampEventDetails(int eventId)
        {
            return Authorized(token =>
            {
                try
                {
                    var campEventInfo = _campService.GetCampEventDetails(eventId);
                    return Ok(campEventInfo);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("EventInfo", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(ProductDTO))]
        [Route("camps/{eventid}/product/{contactid}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetCampProductDetails(int eventId, int contactId)
        {
            return Authorized(token =>
            {
                try
                {
                    var campProductInfo = _campService.GetCampProductDetails(eventId, contactId, token);
                    return Ok(campProductInfo);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("EventInfo", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(CampReservationDTO))]
        [VersionedRoute(template: "camps/{eventId}/campers/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/campers/{contactId}")]
        [HttpGet]
        public IHttpActionResult GetCamperInfo(int eventId, int contactId)
        {
            return Authorized(token =>
            {
                try
                {
                    var camperInfo = _campService.GetCamperInfo(token, eventId, contactId);
                    return Ok(camperInfo);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("CamperInfo", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/product", minimumVersion: "1.0.0")]
        [Route("camps/product")]
        [AcceptVerbs("POST")]
        public IHttpActionResult SaveProductDetails([FromBody] CampProductDTO campProductDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Product data Invalid", new InvalidOperationException("Product Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _campService.SaveInvoice(campProductDto, token);
                    return Ok();
                }

                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Product Invoicing failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/{eventId}/campers", minimumVersion: "1.0.0")]
        [Route("camps/{eventid}/campers")]
        [HttpPost]
        public IHttpActionResult SaveCampReservation([FromBody] CampReservationDTO campReservation, int eventId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Camper Application data Invalid", new InvalidOperationException("Invalid Camper Application Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    var newCamperInfo = _campService.SaveCampReservation(campReservation, eventId, token);
                    return Ok(newCamperInfo);
                }
                catch (ApplicationException e)
                {
                    throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Camp Reservation failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/{eventId}/waivers/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/waivers/{contactId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetCampWaivers(int eventId, int contactId)
        {
            return Authorized(token =>
            {
                try
                {
                    var waivers = _campService.GetCampWaivers(eventId, contactId);
                    return Ok(waivers);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to get waiver data", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/{eventId}/medical/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/medical/{contactId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetCampMedicalInfo(int eventId, int contactId)
        {
            return Authorized(token =>
            {
                try
                {
                    var medicalInfo = _campService.GetCampMedicalInfo(eventId, contactId, token);
                    return Ok(medicalInfo);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to get medical info data", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/medical/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/medical/{contactId}")]
        [AcceptVerbs("POST")]
        public IHttpActionResult SaveMedicalInformation([FromBody] MedicalInfoDTO medicalInfo, int contactId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Camper Medical Info Invalid", new InvalidOperationException("Invalid Camper Medical Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _campService.SaveCamperMedicalInfo(medicalInfo, contactId, token);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Camp Medical Info failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/{eventId}/waivers/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/waivers/{contactId}")]
        [AcceptVerbs("POST")]
        public IHttpActionResult SaveWaivers([FromBody] List<CampWaiverResponseDTO> waivers, int eventId, int contactId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Waiver data Invalid", new InvalidOperationException("Invalid Waiver Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _campService.SaveWaivers(token, eventId, contactId, waivers);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to save waiver data", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }


            });
        }

        [VersionedRoute(template:"camps/{eventId}/emergencycontact/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/emergencycontact/{contactId}")]
        [AcceptVerbs("POST")]
        public IHttpActionResult SaveCamperEmergencyContact([FromBody] List<CampEmergencyContactDTO> emergencyContacts, int eventId, int contactId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Camper Emergency Contact data Invalid", new InvalidOperationException("Invalid Camper emergency contact Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _campService.SaveCamperEmergencyContactInfo(emergencyContacts, eventId, contactId, token);

                    return Ok();
                }

                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Save Camp Emergency Contact Info failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "camps/{eventId}/confirmation/{contactId}", minimumVersion: "1.0.0")]
        [Route("camps/{eventId}/confirmation/{contactId}")]
        [AcceptVerbs("POST")]
        public IHttpActionResult CamperConfirmation(int eventId, int contactId, int invoiceId, int paymentId)
        {
            return Authorized(token =>
            {
                try
                {
                    _campService.SendCampConfirmationEmail(eventId, invoiceId, paymentId, token);
                    _campService.SetCamperAsRegistered(eventId, contactId);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Camp Confirmation failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        

        [VersionedRoute(template: "camps/{eventId}/emergencycontact/{contactId}", minimumVersion: "1.0.0")]
        [ResponseType(typeof(List<CampEmergencyContactDTO>))]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetCamperEmergencyContact(int eventId, int contactId)
        {
            return Authorized(token =>
            {
                try
                {
                    var emergencyContacts = _campService.GetCamperEmergencyContactInfo(eventId, contactId, token);
                    return Ok(emergencyContacts);
                }

                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Camp Emergency Contact Info failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}
