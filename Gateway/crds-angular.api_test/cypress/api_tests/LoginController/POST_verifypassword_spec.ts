import { unzipTests } from "shared/test_scenario_factory";
import { Ben, Sue } from 'shared/users';
import { authorize as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { authorize as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";

// Data Setup
const testConfig: TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request", //TODO why isn't this working? try running locally?
        data: {
          body: {
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithMP(Ben.email, Ben.password as string, this.data);
        }
      },
    ],
    result: {
      status: 200
    }
  },
  {
    setup: [
          {
        description: "Invalid user password",
        data: {
          body: {
            password: "NotMyAPassword"
          }
        },
        setup: function () {
          return authorizeWithMP(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        description: "Request authorized by a different user",
        data: {
          body: {
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithMP(Sue.email, Sue.password as string, this.data)
        }
      },
      {
        description: "Request missing authorization",
        data: {
          body: {
            password: Ben.password
          }
        }
      }],
    result:
    {
      status: 401
    }
  },
  {
    setup: [
      {
        description: "Request authorized using Okta Auth",
        data: {
          body: {
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      }
    ],
    result: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{
          name: "message",
          value: "Verify Password Failed"
        }]
      }
    }
  }
];

// Run Tests
describe('POST /api/verifypassword', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyPassword: Partial<Cypress.RequestOptions> = {
          url: `/api/verifypassword`,
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpVerifyPassword))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});

// describe('POST /api/v1.0.0/verify-password', () => {
//   unzipTests(testConfig)
//     .forEach((t: TestFactory.Test) => {
//       it(t.title, () => {
//         const mpVerifyPassword: Partial<Cypress.RequestOptions> = {
//           url: `/api/v1.0.0/verify-password`,
//           method: "POST",
//           failOnStatusCode: false
//         };

//         t.setup() //Arrange
//           .then(() => cy.request(t.buildRequest(mpVerifyPassword))) //Act
//           .verifyStatus(t.result.status) //Assert
//           .itsBody(t.result.body)
//           .verifySchema(t.result.body)
//           .verifyProperties(t.result.body);
//       });
//     });
// });