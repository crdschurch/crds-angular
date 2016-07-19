﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Security;

namespace crds_angular.Controllers.API
{
    public class GroupToolController : MPAuth
    {
        private readonly Services.Interfaces.IGroupToolService _groupToolService;        

        public GroupToolController(Services.Interfaces.IGroupToolService groupToolService)
        {
            this._groupToolService = groupToolService;
        }

        [AcceptVerbs("POST")]
        [Route("api/grouptool/join-request")]
        public IHttpActionResult CreateRequestToJoin([FromBody] Invitation dto)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Return all pending invitations
        /// </summary>
        /// <param name="sourceId">An integer identifying a group or a trip campaign or some entity to be named later</param>
        /// <param name="invitationTypeId">An integer indicating which invitations are to be returned. For example, Groups or Trips or a source to be identified later.</param>
        /// <returns>A list of Invitation DTOs</returns>
        [AcceptVerbs("GET")]
        [RequiresAuthorization]
        [ResponseType(typeof(List<Invitation>))]
        [Route("api/grouptool/invitations/{sourceId}/{invitationTypeId}")]
        public IHttpActionResult GetInvitations(int sourceId, int invitationTypeId)
        {
            return Authorized(token =>
            {
                try
                {
                    var invitess = _groupToolService.GetInvitations(sourceId, invitationTypeId, token);
                    return Ok(invitess);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetInvitations Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Return all pending inquiries
        /// </summary>
        /// <param name="groupId">An integer identifying the group that we want the inquires for.</param>
        /// <returns>A list of Invitation DTOs</returns>
        [AcceptVerbs("GET")]
        [RequiresAuthorization]
        [ResponseType(typeof(List<Inquiry>))]
        [Route("api/grouptool/inquiries/{groupId}")]
        public IHttpActionResult GetInquiries(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var requestors = _groupToolService.GetInquiries(groupId, token);
                    return Ok(requestors);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetInquires Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [Route("api/grouptool/grouptype/{groupTypeId:int}/group/{groupId:int}/participant/{groupParticipantId:int}")]
        [HttpDelete]
        public IHttpActionResult RemoveParticipantFromMyGroup([FromUri]int groupTypeId, [FromUri]int groupId, [FromUri]int groupParticipantId, [FromUri(Name = "removalMessage")]string removalMessage = null)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.RemoveParticipantFromMyGroup(token, groupTypeId, groupId, groupParticipantId, removalMessage);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error removing group participant {0} from group {1}", groupParticipantId, groupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Allows a group leader to accept or deny a group inquirier.
        /// </summary>
        /// <param name="groupTypeId">An integer identifying the type of group.</param>
        /// <param name="groupId">An integer identifying the group that the inquiry is associated to.</param>
        /// <param name="approve">A boolean showing if the inquiry is being approved or denied. It defaults to approved</param>
        /// <param name="inquiry">An Inquiry JSON Object.</param>
        /// <param name="message">A custom message to send to the inquiry.</param>
        [AcceptVerbs("POST")]
        [RequiresAuthorization]
        [Route("api/grouptool/grouptype/{groupTypeId:int}/group/{groupId:int}/inquiry/approve/:approve")]
        [HttpPost]
        public IHttpActionResult ApproveDenyInquiryFromMyGroup([FromUri]int groupTypeId, [FromUri]int groupId, [FromUri]bool approve, [FromUri(Name = "inquiry")]Inquiry inquiry, [FromUri(Name = "message")]string message = null)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.ApproveDenyInquiryFromMyGroup(token, groupTypeId, groupId, approve,inquiry, message);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error {0} group inquiry {1} from group {2}", approve, inquiry.InquiryId, groupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}