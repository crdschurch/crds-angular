﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using crds_angular.Exceptions.Models;
using crds_angular.Security;
using Crossroads.ApiVersioning;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Controllers.API
{
    public class UserController : MPAuth
    {
        private readonly IAccountService _accountService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IUserRepository _userRepository;
        private readonly IContactRepository _contactRepository;
        // Do not change this string without also changing the same in the corejs register_controller
        private const string DUPLICATE_USER_MESSAGE = "Duplicate User";

        public UserController(IAccountService accountService, IUserRepository userRepository, IContactRepository contactRepository)
        {
            _accountService = accountService;
            _userRepository = userRepository;
            _contactRepository = contactRepository;
        }

        [ResponseType(typeof(User))]
        [VersionedRoute(template: "user", minimumVersion: "1.0.0")]
        [Route("user")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] User user)
        {
            try
            {
                var userRecord = _accountService.RegisterPerson(user);
                return Ok(userRecord);
            }
            catch (DuplicateUserException e)
            {
                var apiError = new ApiErrorDto(DUPLICATE_USER_MESSAGE, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
            catch (ContactEmailExistsException contactException)
            {
                var apiError = new ApiErrorDto(string.Format("{0}", contactException.ContactId()), contactException, HttpStatusCode.Conflict);
                throw new HttpResponseException(apiError.HttpResponseMessage);                
            }
        }

        [RequiresAuthorization]
        [ResponseType(typeof(User))]
        [Route("user")]
        [HttpGet]
        public IHttpActionResult Get(string username)
        {
            return Authorized(token =>
            {
                try
                {
                    int userid = _userRepository.GetUserIdByUsername(username);
                    MpUser user = _userRepository.GetUserByRecordId(userid);
                    int contactid = _contactRepository.GetContactIdByEmail(user.UserEmail);
                    MpMyContact contact = _contactRepository.GetContactById(contactid);

                    return Ok(new UserWithContact(user,contact));
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto($"{e.Message}");
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }

    public class UserWithContact {
        public UserWithContact(MpUser thisUser, MpMyContact thisContact)
        {
            user = thisUser;
            contact = thisContact;
        }
        public MpUser user { get; set; }
        public MpMyContact contact { get; set; }
    }
}
