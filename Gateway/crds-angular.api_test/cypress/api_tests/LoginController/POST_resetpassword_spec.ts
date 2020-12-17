import { setPasswordResetToken } from "shared/mp_api";
import { unzipTests } from "shared/test_scenario_factory";
import { Gatekeeper, KeeperJr } from 'shared/users';
import { getTestPassword, getUUID } from "shared/data_generator";

// Data Setup
const testConfig:TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        data: {
          body: {
            password: getTestPassword(),
            token: getUUID()
          }
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, this.data.body?.token)
        }
      },
      {
        //TODO should set password first to guarantee this scenario
        description: "New Password is Current Password",
        data: {
          body: {
            password: KeeperJr.password,
            token: getUUID()
          }
        },
        setup: function () {
          return setPasswordResetToken(KeeperJr.email, this.data.body?.token)
        }
      }
    ],
    result: { status: 200 }
  },
  {
    setup: [
      {
        description: "Reset Token that Does Not Match any Reset Request",
        data: {
          body: {
            password: getTestPassword(),
            token: getUUID()
          }
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, "")
        },
      },
      {
        description: "Reset Token Value is Substring of Existing Reset Request",
        data: {
          body: {
            password: getTestPassword(),
            token: getUUID()
          }
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, `${this.data.body?.token}9`)
        }
      },
      {
        description: "Request is Missing Reset Token",
        data: {
          body: {
            password: getTestPassword()
          }
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, getUUID())
        }
      },
      {
        description: "Request is Missing Password",
        data: {
          body: { token: getUUID() }
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, this.data.body?.token)
        }
      }
    ],
    result: { status: 500 }
  }
];

// Run Tests
describe('POST /api/resetpassword', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpResetPassword: Partial<Cypress.RequestOptions> = {
          url: "/api/resetpassword",
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpResetPassword))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});

describe('POST /api/v1.0.0/reset-password', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpResetPassword: Partial<Cypress.RequestOptions> = {
          url: "/api/v1.0.0/reset-password",
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpResetPassword))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});