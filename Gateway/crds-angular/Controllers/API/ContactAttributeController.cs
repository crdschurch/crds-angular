﻿using System.Web.Http;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Controllers.API
{
    public class ContactAttributeController : MPAuth
    {
        private readonly IObjectAttributeService _objectAttributeService;

        public ContactAttributeController(IPersonService personService, IObjectAttributeService objectAttributeService)
        {
            _objectAttributeService = objectAttributeService;
        }
        
        [Route("api/contact/attribute/{contactId}")]
        public IHttpActionResult Post(int contactId, [FromBody] ObjectAttributeDTO objectAttribute)
        {
            return Authorized(token =>
            {
                var configuration = MpObjectAttributeConfigurationFactory.MyContact();
                _objectAttributeService.SaveObjectMultiAttribute(token, contactId, objectAttribute, configuration);
                return this.Ok();
            });
        }
    }
}