import { getToken as getOktaToken} from "shared/authorization/okta_user_auth";
import { Test, TestConfig, unzipTests } from "shared/test_scenario_factory";
import { Ben } from "shared/users";
import { mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties } from "./schemas/authenticatedResponse";

/** Test users cannot be authenticated in Okta Prod at this time. */

//Test Data
const testConfig: TestConfig[] = [
  {
    setup: [
      {
        description: "Okta Authorization token",
        setup: function () {
          return getOktaToken(Ben.email, Ben.password as string)
            .then((token) => {
              this.data.header = { Authorization: token }
            });
        }
      }  
    ],
    result: {
      status: 401,
      body: {
        schemas: [{ type: "string", maxLength: 0 }]
      }
    },
    preferredResult: {
      status: 200,
      body: {
        schemas: [mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties]
      }
    }
  }
];

describe('GET /api/authenticated', () => {
  unzipTests(testConfig)
  .forEach((t: Test) => {
    it(t.title, () => {
      const authenticatedRequest: Partial<Cypress.RequestOptions> = {
        url: "/api/authenticated",
        method: "GET",
        failOnStatusCode: false
      };

      t.setup() //Arrange
        .then(() => cy.request(t.buildRequest(authenticatedRequest))) //Act
        .verifyStatus(t.result.status) //Assert
        .itsBody(t.result.body)
        .verifySchema(t.result.body)
        .verifyProperties(t.result.body);
    });
  });
});

describe('GET /api/v1.0.0/authenticated', () => {
  unzipTests(testConfig)
  .forEach((t: Test) => {
    it(t.title, () => {
      const authenticatedRequest: Partial<Cypress.RequestOptions> = {
        url: "/api/v1.0.0/authenticated",
        method: "GET",
        failOnStatusCode: false
      };

      t.setup() //Arrange
        .then(() => cy.request(t.buildRequest(authenticatedRequest))) //Act
        .verifyStatus(t.result.status) //Assert
        .itsBody(t.result.body)
        .verifySchema(t.result.body)
        .verifyProperties(t.result.body);
    });
  });
});