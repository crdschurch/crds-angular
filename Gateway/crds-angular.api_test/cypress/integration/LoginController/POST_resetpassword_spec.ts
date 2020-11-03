import { setPasswordResetToken } from "shared/mp_api";
import { TestCase, TestConfig, unzipTests } from "shared/test_scenario_factory";
import { Gatekeeper, KeeperJr } from 'shared/users';
import { getTestPassword, getUUID } from "shared/data_generator";

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        body: { password: getTestPassword(), token: getUUID() },
        dataSetup: function () {
          setPasswordResetToken(Gatekeeper.email, this.body.token)
        }
      },
      {
        //TODO should set password first to guarantee this scenario
        description: "New Password is Current Password",
        body: { password: KeeperJr.password, token: getUUID() },
        dataSetup: function () {
          setPasswordResetToken(KeeperJr.email, this.body.token)
        }
      }
    ],
    result: { status: 200 }
  },
  {
    setup: [
      {
        description: "Reset Token that Does Not Match any Reset Request",
        body: { password: getTestPassword(), token: getUUID() },
        dataSetup: function () {
          setPasswordResetToken(Gatekeeper.email, "")
        }
      },
      {
        description: "Reset Token Value is Substring of Existing Reset Request",
        body: { password: getTestPassword(), token: getUUID() },
        dataSetup: function () {
          setPasswordResetToken(Gatekeeper.email, `${this.body.token}9`)
        }
      },
      {
        description: "Request is Missing Reset Token",
        body: { password: getTestPassword() },
        dataSetup: function () {
          setPasswordResetToken(Gatekeeper.email, getUUID())
        }
      },
      {
        description: "Request is Missing Password",
        body: { token: getUUID() },
        dataSetup: function () {
          setPasswordResetToken(Gatekeeper.email, this.body.token)
        }
      }
    ],
    result: { status: 500 }
  }
];

// Run Tests
describe('POST /api/resetpassword', () => {
  unzipTests(testConfig)
    .forEach((t: TestCase) => {
      it(t.title, () => {
        if (t.setup.dataSetup) t.setup.dataSetup();

        const mpResetPassword: Partial<Cypress.RequestOptions> = {
          url: "/api/resetpassword",
          method: "POST",
          body: t.setup.body,
          headers: t.setup.header,
          failOnStatusCode: false
        };

        //Act & Assert
        cy.request(mpResetPassword)
          .verifyStatus(t.result.status)
          .itsBody(t.result.body)
          .verifySchema(t.result.body?.schemas)
          .verifyProperties(t.result.body?.properties);
      });
    });
});

describe('POST /api/v1.0.0/reset-password', () => {
  unzipTests(testConfig)
    .forEach((t: TestCase) => {
      it(t.title, () => {
        if (t.setup.dataSetup) t.setup.dataSetup();

        const mpResetPassword: Partial<Cypress.RequestOptions> = {
          url: "/api/v1.0.0/reset-password",
          method: "POST",
          body: t.setup.body,
          headers: t.setup.header,
          failOnStatusCode: false
        };

        //Act & Assert
        cy.request(mpResetPassword)
          .verifyStatus(t.result.status)
          .itsBody(t.result.body)
          .verifySchema(t.result.body?.schemas)
          .verifyProperties(t.result.body?.properties);
      });
    });
});