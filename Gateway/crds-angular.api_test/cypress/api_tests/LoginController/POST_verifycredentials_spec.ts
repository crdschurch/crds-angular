import { unzipTests } from "shared/test_scenario_factory";
import { Ben, Sue } from 'shared/users';
import { authorize as authorizeWithMP} from "shared/authorization/mp_user_auth";
import { authorize as authorizeWithOkta } from "shared/authorization/okta_user_auth";

// Data Setup
const testConfig:TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request using MP Auth",
        data: {
          body: {
            username: Ben.email,
            password: Ben.password
          }
        },
        setup: function() {
          return authorizeWithMP(Ben.email, Ben.password as string, this.data);
        } 
      },
      {
        description: "Valid Request using Okta Auth",
        data: {
          body: {
            username: Ben.email,
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        description: "Request authorized by a different user",
        data: {
          body: {
            username: Ben.email,
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithOkta(Sue.email, Sue.password as string, this.data)
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
            username: Ben.email,
            password: "NotAPassword"
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        description: "Incorrect email",
        data: {
          body: {
            username: `123${Ben.email}`,
            password: Ben.password
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        description: "Request missing authorization",
        data: {
          body: {
            username: Ben.email,
            password: Ben.password
          }
        }
      },
    ],
    result: { 
      status: 401
     }
  }
];

// Run Tests
describe('POST /api/verifycredentials', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyCredentials: Partial<Cypress.RequestOptions> = {
          url: `/api/verifycredentials`,
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpVerifyCredentials))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});

describe('POST /api/v1.0.0/verify-credentials', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyCredentials: Partial<Cypress.RequestOptions> = {
          url: `/api/v1.0.0/verify-credentials`,
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpVerifyCredentials))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});