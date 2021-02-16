import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { getContactRecord } from "shared/mp_api";
import { Ben } from "shared/users";
import { exceptionResponseContract, exceptionResponseProperties } from "./schemas/exceptionResponseSchemas";

/** Helper Functions */
let benContactId: number;
function getBenContactId(){
  if(benContactId){
    return cy.wrap(benContactId);
  }

  return getContactRecord(Ben.email)
  .then((contact) => {
    benContactId = contact.Contact_ID
    return benContactId;
  })
}

function setUrl(contactId: string, request: Partial<Cypress.RequestOptions>){
  request.url = request.url?.replace('{contactId}', contactId);
}

//{contactId} is a placeholder and must be set to a value before calling
const sharedRequest = {
  urls: ['/api/lookup/{contactId}/find', 
  '/api/v1.0.0/lookup/{contactId}/find',
  // TODO endpoints ending in / regression test a previous version of the endpoint definition. 
  //  These tests should be removed once no errors related to this endpoint show up in Prod.
  '/api/lookup/{contactId}/find/', 
  '/api/v1.0.0/lookup/{contactId}/find/'], 
  options: {
    method: 'GET',
    failOnStatusCode: false
  }
}

const successScenario: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    bodyIsEmpty: true
  },
  scenarios: [
    {
      description: "Unauthorized request where user does not exist and contact Id = 0",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        setUrl('0', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "MP Authorized request where user does not exist and contact Id = 0",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        setUrl('0', this.request);
        return authorizeWithMP(Ben.email, Ben.password!, this.request).then(() => this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "Ota Authorized request where user does not exist and contact Id = 0",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        setUrl('0', this.request);
        return authorizeWithOkta(Ben.email, Ben.password!, this.request).then(() => this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "MP Authorized request where user exists and contact Id matches their account",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithMP(Ben.email, Ben.password!, this.request)
        })
        .then(() =>  this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "Okta Authorized request where user exists and contact Id matches their account",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithOkta(Ben.email, Ben.password!, this.request)
        })
        .then(() =>  this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "MP Authorized request where user doesn't exist but contact Id matches a real user",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithMP(Ben.email, Ben.password!, this.request)
        })
        .then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "Okta Authorized request where user doesn't exist but contact Id matches a real user",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithOkta(Ben.email, Ben.password!, this.request)
        })
        .then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "Unauthorized request where user doesn't exist but contact Id matches a real user",
      request:{
        qs: {
          email: 'usershouldnotexist@testmail.com'
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return this;
        })
      },
      response: {
        status: 200
      }
    }
  ]
}

const badRequestScenario: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    bodyIsEmpty: true
  },
  scenarios: [
    {
      description: "Unauthorized request where user exists and contact Id = 0",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        setUrl('0', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 400
      }
    },
    {
      description: "MP Authorized request where user exists and contact Id = 0",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        setUrl('0', this.request);
        return authorizeWithMP(Ben.email, Ben.password!, this.request).then(() => this)
      },
      response: {
        status: 400
      }
    },
    {
      description: "Okta Authorized request where user exists and contact Id = 0",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        setUrl('0', this.request);
        return authorizeWithOkta(Ben.email, Ben.password!, this.request).then(() => this)
      },
      response: {
        status: 400
      }
    },
    {
      description: "Unauthorized request where user exists and contact Id matches their account",
      request:{
        qs: {
          email: Ben.email
        }
      },
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return this;
        })
      },
      response: {
        status: 400
      }
    }
  ]
}

const notFoundScenario: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [exceptionResponseProperties, exceptionResponseContract],
    properties: [
      {
        name: "Message",
        satisfies(value){
          return value.includes("No HTTP resource was found that matches the request URI");
        }
      }
    ]
  },
  scenarios: [
    {
      description: "Unauthorized request where email is missing but contact Id matches a real user",
      request:{},
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return this;
        })
      },
      response: {
        status: 404,
        schemas: [exceptionResponseProperties, exceptionResponseContract],
      }
    },
    {
      description: "MP Authorized request where email is missing but contact Id matches a real user",
      request:{},
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithMP(Ben.email, Ben.password!, this.request)
        })
        .then(() => this);
      },
      response: {
        status: 404
      }
    },
    {
      description: "Okta Authorized request where email is missing but contact Id matches a real user",
      request:{},
      setup(){
        return getBenContactId()
        .then((contactId) => {
          setUrl(`${contactId}`, this.request);
          return authorizeWithOkta(Ben.email, Ben.password!, this.request)
        })
        .then(() => this);
      },
      response: {
        status: 404
      }
    },
    {
      description: "Unauthorized request where email is missing and contact Id = 0",
      request: {},
      setup() {
        setUrl('0', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 404
      }
    },
    {
      description: "MP Authorized request where email is missing and contact Id = 0",
      request: {},
      setup() {
        setUrl('0', this.request);
        return authorizeWithMP(Ben.email, Ben.password!, this.request)
          .then(() => this);
      },
      response: {
        status: 404
      }
    },
    {
      description: "Okta Authorized request where email is missing and contact Id = 0",
      request:{},
      setup() {
        setUrl('0', this.request);
        return authorizeWithOkta(Ben.email, Ben.password!, this.request)
          .then(() => this);
      },
      response: {
        status: 404        
      }
    }
  ]
}

describe('/Lookup/EmailExists()', () => {
  unzipScenarios(successScenario).forEach(runTest);
  unzipScenarios(badRequestScenario).forEach(runTest);
  unzipScenarios(notFoundScenario).forEach(runTest);
})