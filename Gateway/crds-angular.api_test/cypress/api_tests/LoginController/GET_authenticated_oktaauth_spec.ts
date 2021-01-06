import { authorize as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { unzipTests } from "shared/test_scenario_factory";
import { Ben } from "shared/users";
import { mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties } from "./schemas/authenticatedResponse";
import { emptyStringResponseProperties } from "./schemas/emptyStringResponse";

/** Test users cannot be authenticated in Okta Prod at this time, but
 * post-cutover, this test should be updated and made runnable in Prod.
*/

//Test Data
const testConfig: TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Okta Authorization token",
        data: {},
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data);
        }
      }
    ],
    result: {
      status: 401,
      body: {
        schemas: [emptyStringResponseProperties]
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
    .forEach((t: TestFactory.Test) => {
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
    .forEach((t: TestFactory.Test) => {
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