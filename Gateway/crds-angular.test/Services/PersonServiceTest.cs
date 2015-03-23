﻿using System;
using crds_angular.App_Start;
using crds_angular.Services;
using MinistryPlatform.Translation.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    class PersonServiceTest
    {
        private PersonService _personService = new PersonService();

        //private const string USERNAME = "testme";
        //private const string PASSWORD = "changeme";
        private const string USERNAME = "tmaddox@aol.com";
        private const string PASSWORD = "crds1234";

        // TODO figure out why testme can not get their profile
        [Test]
        public void ATestMyFamily()
        {
            //force AutoMapper to register
            AutoMapperConfig.RegisterMappings();

            var token = TranslationService.Login(USERNAME, PASSWORD);
            var contactId = AuthenticationService.GetContactId(token);

            var personService = new PersonService();
            var fam = personService.GetMyFamily( contactId,token);

            Assert.IsNotNull(fam);
        }
    }
}
