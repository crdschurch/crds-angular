import { getUUID } from "shared/data_generator";
import { setPasswordResetToken } from "shared/mp_api";
import { Test, TestConfig, unzipTests } from "shared/test_scenario_factory";
import { Ben, KeeperJr, Load } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        data: { body: { email: Ben.email } },
        setup: function () {
          //Remove any existing token
          return setPasswordResetToken(Ben.email, "")
        }
      },
      {
        description: "User Has Pending Password Reset Request",
        data: { body: { email: Ben.email } },
        setup: function () {
          return setPasswordResetToken(Ben.email, getUUID())
        }
      }
    ],
    result: { status: 200 }
  },
  {
    setup: [
      {
        description: "Person Doesn't Exist",
        data: { body: { email: "this_email_should_not_exist@fake_emails.io" } }
      },
      {
        description: "Person has Contact Record but no User Record",
        data: { body: { email: Load.email } }
      },
      {
        description: "Invalid Email",
        data: { body: { email: "{" } }
      },
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
    setup:
    {
      description: "Email is Subset of Another Email (bug)",
      data: { body: { email: KeeperJr.email } }
    },
    result: { status: 200 },
    preferredResult: { status: 200 } //Should still have valid output, but won't error on server side
  },
  {
    setup: { description: "Email is Undefined", data: { body: {} } },
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
    .forEach((t: Test) => {
      it(t.title, () => {
        const requestPasswordReset: Partial<Cypress.RequestOptions> = {
          url: "/api/requestpasswordreset",
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(requestPasswordReset))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});