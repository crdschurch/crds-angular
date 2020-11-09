import { getUUID } from "shared/data_generator";
import { setPasswordResetToken } from "shared/mp_api";
import { TestCase, TestConfig, unzipTests } from "shared/test_scenario_factory";
import { Ben, KeeperJr, Load } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        body: { email: Ben.email },
        dataSetup: function () {
          //Remove any existing token
          setPasswordResetToken(Ben.email, "")
        }
      },
      {
        description: "User Has Pending Password Reset Request",
        body: { email: Ben.email },
        dataSetup: function () {
          setPasswordResetToken(Ben.email, getUUID())
        }
      }
    ],
    result: { status: 200 }
  },
  {
    setup: [
      { description: "Person Doesn't Exist", body: { email: "this_email_should_not_exist@fake_emails.io" } },
      { description: "Person has Contact Record but no User Record", body: { email: Load.email } },
      { description: "Invalid Email", body: { email: "{" } },
    ],
    result: { status: 200 }, //Not technically a bug but misleading to user
    preferredResult: {
      status: 404,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "User Not Found" }]
      }
    }
  },
  {
    setup: [
      { description: "Email is Subset of Another Email (bug)", body: { email: KeeperJr.email } }
    ],
    result: { status: 200 },
    preferredResult: { status: 200 } //Should still have valid output, but won't error on server side
  },
  {
    setup: { description: "Email is Undefined", body: {} },
    result: { status: 200 }, //Not technically a bug but misleading to user
    preferredResult: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Missing Email" }]
      }
    }
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
describe('POST /api/requestpasswordreset', () => {
  unzipTests(testConfig)
    .forEach((t: TestCase) => {
      it(t.title, () => {
        if (t.setup.dataSetup) t.setup.dataSetup();

        const mpRequestPasswordReset: Partial<Cypress.RequestOptions> = {
          url: "/api/requestpasswordreset",
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