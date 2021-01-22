import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { getTempTesterEmail, getTestPassword } from "shared/data_generator";
import { Placeholders } from "shared/enums";
import { Gatekeeper } from "shared/users";
import { badRequestProperties, badRequestContract } from "./schemas/badRequestSchemas";
import { createUserSchemaProperties, createUserContract, duplicateUserRegistrationPageContract } from "./schemas/createUserSchemas";
import { genericServerErrorContract } from "./schemas/serverErrorSchemas";

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
      description: "Email is substring of existing email",
      request: {
        body: {
          firstname: "Substring email",
          lastname: "Test",
          email: Placeholders.assignedInSetup,
          password: Placeholders.assignedInSetup
        }
      },
      setup() {
        // Register a test user so we have an email to be a substring of
        const superEmail = getTempTesterEmail();
        const registerSuperEmail = {
          url: "/api/user",
          method: "POST",
          body: {
            firstname: "Super",
            lastname: "Email",
            email: superEmail,
            password: getTestPassword()
          }
        }

        return cy.request(registerSuperEmail)
        .then(() => {
          //TODO optimize this - use createAndSetEmail, then create the super email from the gnerated one
          const subsetEmail = superEmail.slice(0, superEmail.length-1);
          (this.request.body as { email: string }).email = subsetEmail;
          (this.response.properties?.find(r => r.name === "email") as CAT.PropertyCompare).value = subsetEmail;

          createAndSetPassword(this);
          return this;
        })
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

//Note that this bug does not exist when registering a user through Okta.
// const bug_emailUniqueButSubsetOfContactEmailScenario: CAT.CompactTestScenario = {
//   sharedRequest,
//   scenarios: [
//   {
//       description: "email is subset of existing email",
//       request: {
//         body: {
//           firstname: "Subset email",
//           lastname: "Test",
//           email: Placeholders.assignedInSetup,
//           password: Placeholders.assignedInSetup
//         }
//       },
//       setup() {
//         // Register a test user so we have an email to subset
//         const supersetEmail = getTempTesterEmail();
//         const registerSuperset = {
//           url: "/api/user",
//           method: "POST",
//           body: {
//             firstname: "Super",
//             lastname: "Email",
//             email: supersetEmail,
//             password: getTestPassword()
//           }
//         }

//         return cy.request(registerSuperset)
//         .then(() => {
//           const subsetEmail = supersetEmail.slice(0, supersetEmail.length-1);
//           (this.request.body as { email: string }).email = subsetEmail;

//           createAndSetPassword(this, false);
//           return this;
//         })
//       },
//       response: {
//         //Note that THE USER WAS CREATED! but the step to retrieve their contact ID incorrectly 
//         //  found two users (test uer and the superset user registered in setup) so failed, 
//         //  triggering a 500 response
//         status: 500,
//         schemas: [genericServerErrorContract],
//         properties: [{ name: "Message", value: "An error has occurred." }]
//       },
//       preferredResponse: {
//         status: 200,
//         properties: [
//           {
//             name: "email",
//             value: Placeholders.assignedInSetup
//           },
//           {
//             name: "password",
//             value: Placeholders.assignedInSetup
//           }
//         ]
//       }
//     }
//   ]
// };

//Run Tests
describe('/User/Post()', () => {
  unzipScenarios(successScenarios).forEach(runTest)
  // unzipScenarios(badRequestScenarios).forEach(runTest)
  // unzipScenarios(serverErrorScenarios).forEach(runTest)
  // unzipScenarios(bug_emailUniqueButSubsetOfContactEmailScenario).forEach(runTest)
});

//TODO need to fix Gateway for GetUserIdByUsername and update those tests
//TODO search through Gateway for other potential problems with emails