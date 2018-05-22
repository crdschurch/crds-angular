using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Childcare;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using Newtonsoft.Json;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class ChildcareController : MPAuth
    {
        private readonly IChildcareService _childcareService;
        private readonly IEventService _eventService;
        private readonly IPersonService _personService;

        public ChildcareController(IAuthTokenExpiryService authTokenExpiryService, 
                                   IChildcareService childcareService, 
                                   IEventService eventService, 
                                   IPersonService personService, 
                                   IUserImpersonationService userImpersonationService, 
                                   IAuthenticationRepository authenticationRepository) 
                                  : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _childcareService = childcareService;
            _eventService = eventService;
            _personService = personService;
        }

        [VersionedRoute(template: "childcare/rsvp", minimumVersion: "1.0.0")]
        [Route("childcare/rsvp")]
        [HttpPost]
        public IHttpActionResult SaveRsvp([FromBody] ChildcareRsvpDto saveRsvp)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("SaveRsvp Data Invalid", new InvalidOperationException("Invalid SaveRsvp Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    if (saveRsvp.Registered)
                    {
                        _childcareService.SaveRsvp(saveRsvp);
                    }
                    else
                    {
                        _childcareService.CancelRsvp(saveRsvp);
                    }
                    return Ok();
                }
                catch (GroupFullException e)
                {
                    var json = JsonConvert.SerializeObject(e.Message, Formatting.None);
                    var message = new HttpResponseMessage(HttpStatusCode.PreconditionFailed);
                    message.Content = new StringContent(json);
                    throw new HttpResponseException(message);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Childcare-SaveRsvp failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(Event))]
        [VersionedRoute(template: "childcare/event/{eventId}", minimumVersion: "1.0.0")]
        [Route("childcare/event/{eventId}")]
        [HttpGet]
        public IHttpActionResult ChildcareEventById(int eventId)
        {
            return Authorized(token =>
            {
                try
                {
                    return Ok(_eventService.GetMyChildcareEvent(eventId, token));
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("ChildcareEventById failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }



        [ResponseType(typeof(List<FamilyMember>))]
        [VersionedRoute(template: "childcare/eligible-children", minimumVersion: "1.0.0")]
        [Route("childcare/eligible-children")]
        [HttpGet]
        public IHttpActionResult ChildrenEligibleForChildcare()
        {
            return Authorized(token =>
            {
                try
                {
                    return Ok(_childcareService.MyChildren(token));
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("ChildrenEligibleForChildcare failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "childcare/request", minimumVersion: "1.0.0")]
        [Route("childcare/request")]
        [HttpPost]
        public IHttpActionResult CreateChildcareRequest([FromBody] ChildcareRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Save Request Data Invalid", new InvalidOperationException("Invalid Save request Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _childcareService.CreateChildcareRequest(request, token);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Create Childcare Request Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [VersionedRoute(template: "childcare/update-request", minimumVersion: "1.0.0")]
        [Route("childcare/updaterequest")]
        [HttpPost]
        public IHttpActionResult UpdateChildcareRequest([FromBody] ChildcareRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Update Request Data Invalid", new InvalidOperationException("Invalid Save request Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _childcareService.UpdateChildcareRequest(request, token);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Update Childcare Request Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [VersionedRoute(template: "childcare/request/approve/{requestId}", minimumVersion: "1.0.0")]
        [Route("childcare/request/approve/{requestId}")]
        [HttpPost]
        public IHttpActionResult ApproveChildcareRequest(int requestId, ChildcareRequestDto childcareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Save Request Data Invalid", new InvalidOperationException("Invalid Save request Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _childcareService.ApproveChildcareRequest(requestId, childcareRequest, token);
                    return Ok();
                }
                catch (EventMissingException e)
                {
                    var errors = new DateError() {Errors = e.MissingDateTimes, Message = e.Message};
                    var json = JsonConvert.SerializeObject(errors, Formatting.Indented);
                    var message = new HttpResponseMessage(HttpStatusCode.RequestedRangeNotSatisfiable);
                    message.Content = new StringContent(json);
                    throw new HttpResponseException(message);
                }
                catch (ChildcareDatesMissingException e)
                {
                    var json = JsonConvert.SerializeObject(e.Message, Formatting.None);
                    var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                    message.Content = new StringContent(json);
                    throw new HttpResponseException(message);
                }
                catch (DuplicateChildcareEventsException e)
                {
                    var error = new DateError() { Error = e.RequestedDate, Message = e.Message };
                    var json = JsonConvert.SerializeObject(error, Formatting.Indented);
                    var message = new HttpResponseMessage(HttpStatusCode.MultipleChoices);
                    message.Content = new StringContent(json);
                                        throw new HttpResponseException(message);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Approve Childcare Request Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [VersionedRoute(template: "childcare/request/reject/{requestId}", minimumVersion: "1.0.0")]
        [Route("childcare/request/reject/{requestId}")]
        [HttpPost]
        public IHttpActionResult RejectChildcareRequest(int requestId, ChildcareRequestDto childcareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Save Request Data Invalid", new InvalidOperationException("Invalid Save request Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _childcareService.RejectChildcareRequest(requestId, childcareRequest, token);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Reject Childcare Request Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [VersionedRoute(template: "childcare/get-request/{requestId}", minimumVersion: "1.0.0")]
        [Route("childcare/getrequest/{requestId}")]
        [HttpGet]
        public IHttpActionResult GetChildcareRequest(int requestId)
        {

            return Authorized(token =>
            {
                try
                {
                    return Ok(_childcareService.GetChildcareRequestForReview(requestId, token));
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Get Childcare Request Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [VersionedRoute(template: "childcare/dashboard/{contactId}", minimumVersion: "1.0.0")]
        [Route("childcare/dashboard/{contactId}")]
        [HttpGet]
        public IHttpActionResult ChildcareDashboard(int contactId)
        {
            return Authorized(token =>
            {
                // Get contactId of token
                var person = _personService.GetLoggedInUserProfile(token);
                try
                {
                    var householdInfo = _childcareService.GetHeadsOfHousehold(person.ContactId, person.HouseholdId);
                    return Ok(_childcareService.GetChildcareDashboard(person, householdInfo));
                }
                catch (NotHeadOfHouseholdException notHead)
                {
                    var json = JsonConvert.SerializeObject(notHead.Message, Formatting.Indented);
                    var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable) {Content = new StringContent(json)};
                    throw new HttpResponseException(message);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Get Childcare Dashboard Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });

        }

        private class DateError
        {
            public List<DateTime> Errors { get; set;}
            public string Message { get; set;}
            public DateTime Error { get; set; }
        }
    }
}
