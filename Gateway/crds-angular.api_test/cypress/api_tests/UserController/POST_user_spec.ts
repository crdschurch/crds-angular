import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { getTempTesterEmail, getTestPassword } from "shared/data_generator";
import { Placeholders } from "shared/enums";
import { Gatekeeper } from "shared/users";
import { badRequestProperties, badRequestContract } from "./schemas/badRequestSchemas";
import { createUserSchemaProperties, createUserContract, duplicateUserRegistrationPageContract } from "./schemas/createUserSchemas";
import { genericServerErrorContract } from "./schemas/serverErrorSchemas";

//TODO fixup Gateway source: (iff these tests behave as expected running locally)
// ContactEmailExistsException is never thrown - 
//   1.move Login enum if possible, 
//   2. smarter/safer queries for unique users. There might be lots of different code doing this - POST uses Transactions but GET might use something else
//TODO try to fix the source - why is it doing this? /r/n?

/**
 * Generates an email and assigns it the request body. Assigns to response property check if indicated.
 * @param scenario 
 * @param setResponseProperty Default true
 */
function createAndSetEmail(scenario: CAT.TestScenario, setResponseProperty = true): void {
  const email = getTempTesterEmail();
  (scenario.request.body as { email: string }).email = email;
  if (setResponseProperty) {
    (scenario.response.properties?.find(r => r.name === "email") as CAT.PropertyCompare).value = email;
  }
}

/**
 * Generates a password and assigns it the request body. Assigns to response property check if indicated.
 * @param scenario 
 * @param setResponseProperty Default true
 */
function createAndSetPassword(scenario: CAT.TestScenario, setResponseProperty = true): void {
  const password = getTestPassword();
  (scenario.request.body as { password: string }).password = password;
  if (setResponseProperty) {
    (scenario.response.properties?.find(r => r.name === "password") as CAT.PropertyCompare).value = password;
  }
}

//Setup Data
const sharedRequest = {
  urls: ["/api/user", "/api/v1.0.0/user"],
  options: {
    method: "POST",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [createUserSchemaProperties, createUserContract],
    properties: [
      {
        name: "email",
        value: Placeholders.assignedInSetup
      },
      {
        name: "password",
        value: Placeholders.assignedInSetup
      }
    ]
  },
  scenarios: [
    {
      description: "Valid new user info",
      request: {
        body: {
          firstname: "Testing",
          lastname: "Gateway",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetEmail(this);
        createAndSetPassword(this);
        return cy.wrap(this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "Valid new user info and household source",
      request: {
        qs: {
          householdSourceId: 38
        },
        body: {
          firstname: "Testing",
          lastname: "Gateway",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetEmail(this);
        createAndSetPassword(this);
        return cy.wrap(this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "User info missing first name",
      request: {
        body: {
          lastname: "Gateway",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetEmail(this);
        createAndSetPassword(this);
        return cy.wrap(this)
      },
      response: {
        status: 200
      }
    },
    {
      description: "User info missing password",
      request: {
        body: {
          firstname: "Testing",
          lastname: "Gateway",
          email: Placeholders.assignedInSetup,
        }
      },
      setup() {
        createAndSetEmail(this);
        (this.response.properties?.find(r => r.name === "password") as CAT.PropertyCompare).value = null;
        return cy.wrap(this)
      },
      response: {
        status: 200
      },
      preferredResponse: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Missing password" }]
      }
    }
  ]
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Existing user info",
      request: {
        body: {
          firstname: Gatekeeper.firstName,
          lastname: "Keeper",
          email: Gatekeeper.email,
          password: Gatekeeper.password
        }
      },
      response: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract, duplicateUserRegistrationPageContract],
      }
    },
  ]
}

const serverErrorScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [genericServerErrorContract],
    properties: [{ name: "Message", value: "An error has occurred." }]
  },
  scenarios: [
    {
      description: "Invalid household source",
      request: {
        qs: {
          householdSourceId: -55
        },
        body: {
          firstname: "Testing",
          lastname: "Gateway",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetEmail(this, false);
        createAndSetPassword(this, false);
        return cy.wrap(this)
      },
      response: {
        status: 500
      },
      preferredResponse: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Invalid Household Source Id" }]
      }
    },
    {
      description: "Missing last name",
      request: {
        body: {
          firstname: "Testing",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetEmail(this, false);
        createAndSetPassword(this, false);
        return cy.wrap(this)
      },
      response: {
        status: 500,
      },
      preferredResponse: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Missing last name" }]
      }
    },
    {
      description: "Missing email",
      request: {
        body: {
          firstname: "Testing",
          lastname: "Gateway",
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        createAndSetPassword(this, false);
        return cy.wrap(this)
      },
      response: {
        status: 500,
      },
      preferredResponse: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Missing email" }]
      }
    }
  ]
}

/*
// I think this is caused because an unhandled exception is thrown when searching for the contact record
//  by email - pretty sure it uses a %email% search so finds more than it should. If multiple records are found
//  an error is thrown which is not handled by the controller. The account is created though.
// To reproduce, the email must be unique but a subset of another email, so will be a bit tricky to setup
// IMO this will be a non-issue once we move to Okta (confirm this) but maybe add a note to the code?
const emailUniqueButSubsetOfContactEmail: CAT.TestScenario = {
  description: "email is subset of existing email",
  request: {
    url: sharedRequest.urls[0],
    method: sharedRequest.options.method,
    failOnStatusCode: sharedRequest.options.failOnStatusCode,
    body: {
      firstname: "experimental",
      lastname: "Test",
      email: "gatekeeper@testmail.com", //TODO this needs to be dynamically assigned - create a user then change email extension
      password: Placeholders.assignedInSetup
    }
  },
  setup(){
    const password = getTestPassword();
    (this.request.body as {password: string}).password = password;
    return cy.wrap(this)
  },
  response: {
    status: 500
  },
  preferredResponse: {
    status: 200
  }
};
*/

//Run Tests
describe('/User/Post()', () => {
  unzipScenarios(successScenarios).forEach(runTest)
  unzipScenarios(badRequestScenarios).forEach(runTest)
  unzipScenarios(serverErrorScenarios).forEach(runTest)
});