import { TestCase, TestConfig, unzipTests } from "cypress/shared/test_scenario_factory";
import { Ben } from "cypress/shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
/**
 * This endpoint is an alternate name for the POST /api/requestpasswordreset endpoint.
 * Tests here are lighter tests runnable in Prod - More robust tests are run in the
 * POST /api/requestpasswordreset test file.
*/

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: { description: "Valid Request", body: { email: Ben.email } },
    result: { status: 200 }
  },
  {
    setup: { description: "Missing Body" },
    result: {
      status: 500,
      body: {
        schemas: [{ type: "string", maxLength: 0 }]
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
describe('POST /api/v1.0.0/request-password-reset', () => {
  unzipTests(testConfig)
    .forEach((t: TestCase) => {
      it(t.title, () => {
        const mpRequestPasswordReset: Partial<Cypress.RequestOptions> = {
          url: "/api/v1.0.0/request-password-reset",
          method: "POST",
          body: t.setup.body,
          headers: t.setup.header,
          failOnStatusCode: false
        };

        //Act & Assert
        cy.request(mpRequestPasswordReset)
          .verifyStatus(t.result.status)
          .itsBody(t.result.body)
          .verifySchema(t.result.body?.schemas)
          .verifyProperties(t.result.body?.properties);
      });
    });
});


