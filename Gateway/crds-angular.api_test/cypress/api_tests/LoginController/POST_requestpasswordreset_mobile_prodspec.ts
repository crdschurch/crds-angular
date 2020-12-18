import { unzipTests } from "shared/test_scenario_factory";
import { Ben } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { emptyStringResponseProperties } from "./schemas/emptyStringResponse";
/**
 * This endpoint is an alternate name for the POST /api/requestpasswordreset/mobile endpoint.
 * Tests here are lighter tests runnable in Prod - More robust tests are run in the
 * POST /api/requestpasswordreset/mobile test file.
*/

// Data Setup
const testConfig:TestFactory.TestConfig[] = [
  {
    setup: { description: "Valid Request", data: { body: { email: Ben.email } } },
    result: { status: 200 }
  },
  {
    setup: { description: "Missing Body", data:{} },
    result: {
      status: 500,
      body: {
        schemas: [emptyStringResponseProperties]
      }
    }, //Not technically a bug but error could be more descriptive
    preferredResult: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Missing Email" }]
      }
    }
  }
];

// Run Tests
describe('POST /api/v1.0.0/request-password-reset-mobile', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const requestPWResetMobile: Partial<Cypress.RequestOptions> = {
          url: "/api/v1.0.0/request-password-reset-mobile",
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(requestPWResetMobile))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});
