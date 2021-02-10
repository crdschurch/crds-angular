
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Placeholders } from "shared/enums";
import { getContactRecord } from "shared/mp_api";
import { KeeperJr, SkywalkerFamily } from "shared/users";
import { errorResponseContract, errorResponseProperties } from "./schemas/errorResponseSchemas";
import { exceptionResponseContract, exceptionResponseProperties } from "./schemas/exceptionResponseSchemas";

/**
 * WARNING! Do NOT post to the '/api/profile' endpoint and only give a valid contactId and householdId - all other properties
 * that this endpoint can set will be REMOVED from their Contact record, including their email.
 */

let lukeContactId: number;
let lukeHouseholdId: number;
function getLukeProfile(): Cypress.Chainable<any>{
  console.log(`does exist lukeContactId? _${lukeContactId}_`)

  const lukeProfile = (contactId: number, householdId: number) => {
    return {
      contactId,
      householdId,
      emailAddress: SkywalkerFamily.Luke.email,
      firstName: SkywalkerFamily.Luke.firstName,
      nickName: SkywalkerFamily.Luke.firstName,
      lastName: "Skywalker",
      householdName: "Skywalker",
      age: 20,
      dateOfBirth: "01/01/2001",
      congregationId: 6,
      genderId: 1,
      homePhone: "123-867-5309",
      mobilePhone: "321-548-6154",
      maritalStatusId: 1,
      participantStartDate: "2020-12-03T15:04:00",
      addressLine1: "1234 Tatooine Lane",
      city: "CITY",
      postalCode: "45209",
      state: "OH",
      county: "County!",
      foreignCountry: "United States",
    }
  }

  // Use ids if they've been fetched already
  if(lukeContactId && lukeHouseholdId){
    return cy.wrap(lukeProfile(lukeContactId, lukeHouseholdId))
  }

  //Record IDs will change after a DB refresh, so fetch them dynamically
  return getContactRecord(SkywalkerFamily.Luke.email)
  .then((contact) => {
    lukeContactId = contact.Contact_ID;
    lukeHouseholdId = contact.Household_ID;
    return lukeProfile(lukeContactId, lukeHouseholdId);
  });
}


const sharedRequest: CAT.SharedRequest = {
  urls: ['/api/profile'],
  options: {
    method: 'POST',
    failOnStatusCode: false
  }
}

const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Person can update their own profile",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => this.request.body = lukeProfile)
        .then(() => authorizeWithOkta(SkywalkerFamily.Luke.email, SkywalkerFamily.Luke.password as string, this.request))
        .then(() => this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "Parent can update their child's profile",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => this.request.body = lukeProfile)
        .then(() => authorizeWithOkta(SkywalkerFamily.Padme.email, SkywalkerFamily.Padme.password as string, this.request))
        .then(() => this);
      },
      response: {
        status: 200
      }
    },
  ]
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Body missing household id",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => {
          lukeProfile.householdId = undefined; 
          this.request.body = lukeProfile
        })
        .then(() => authorizeWithOkta(SkywalkerFamily.Luke.email, SkywalkerFamily.Luke.password as string, this.request))
        .then(() => this);
      },
      response: {
        status: 400,
        schemas: [errorResponseProperties, errorResponseContract],
        properties: [ //TODO support comparing properties - add callback comparator (name+value = equals, name+comparator callback = comparator, name+value+comparator = use comparator and warn)
          { name: "message", value: "Profile update Failed" },
          // { name: "errors", value: "dbo.Households", }
        ]
      }
    }
  ]
}

const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Missing authorization header",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => this.request.body = lukeProfile)
        .then(() => this)
      },
      response: {
        status: 401
      }
    },
    {
      description: "Request has empty body",
      request: {
        body: {}
      },
      setup() {
        return authorizeWithOkta(SkywalkerFamily.Padme.email, SkywalkerFamily.Padme.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 401
      },
      preferredResponse: {
        status: 400,
        schemas: [errorResponseProperties, errorResponseContract],
        properties: [
          {
            name: "message",
            value: "Save Profile Data Invalid"
          }
        ]
      }
    },
    {
      description: "Contact record does not exist",
      request: {
        body: {
          contactId: 1111111
        }
      },
      setup() {
        return authorizeWithOkta(SkywalkerFamily.Padme.email, SkywalkerFamily.Padme.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 401
      }
    },
    {
      description: "Body missing contact id",
      request: {
        body: {
          emailAddress: SkywalkerFamily.Luke.email
        }
      },
      setup() {
        return authorizeWithOkta(SkywalkerFamily.Luke.email, SkywalkerFamily.Luke.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 401
      },
      preferredResponse: {
        status: 400,
        schemas: [errorResponseProperties, errorResponseContract],
        properties: [
          {
            name: "message",
            value: "Save Profile Data Invalid"
          }
        ]
      }
    },
    {
      description: "Person can not update their sibling's profile",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => this.request.body = lukeProfile)
        .then(() => authorizeWithOkta(SkywalkerFamily.Leia.email, SkywalkerFamily.Leia.password as string, this.request))
        .then(() => this)
      },
      response: {
        status: 401
      }
    },
    {
      description: "Person can not update a stranger's profile",
      request: {
        body: Placeholders.assignedInSetup
      },
      setup() {
        return getLukeProfile()
        .then((lukeProfile) => this.request.body = lukeProfile)
        .then(() => authorizeWithOkta(KeeperJr.email, KeeperJr.password as string, this.request))
        .then(() => this);
      },
      response: {
        status: 401
      }
    },
  ]
}

const serverErrorScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Request missing body",
      request: { },
      setup() {
        return authorizeWithOkta(SkywalkerFamily.Padme.email, SkywalkerFamily.Padme.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 500,
        schemas: [exceptionResponseProperties, exceptionResponseContract] //Note that errors are more detailed when running locally w/ debug on
      }
    },
  ]
}

describe('/profile/Post()', () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(unauthorizedScenarios).forEach(runTest);
  unzipScenarios(badRequestScenarios).forEach(runTest);
  unzipScenarios(serverErrorScenarios).forEach(runTest);
});