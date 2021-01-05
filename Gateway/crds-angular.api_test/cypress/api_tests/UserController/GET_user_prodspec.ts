import { mpAuthenticatedSchemaProperties } from "api_tests/LoginController/schemas/authenticatedResponse"; //todo this might be shared...
import { badRequestContract, badRequestProperties } from "api_tests/LoginController/schemas/badRequest";//todo this might be shared...
import { authorize as authorizeWithMPClient } from "shared/authorization/mp_client_auth";
import { authorize as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { authorize as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { unzipTests } from "shared/test_scenario_factory";
import { Ben, Sue } from "shared/users";

//TODO make it possible for results to reference in the setup.data. 
//TODO merge/rebase after previous pr is merged and update the emptyResponse references
//TODO fixup Gateway (move Login enum if possible, smarter/safer queries for unique users)
const userBasicAuthContract = {
  title: "MP user response - user info contract",
  type: "object",
  required: ["userToken", "userId", "userEmail", "username", "roles", "age", "userPhone", "canImpersonate"],
};

const emptyResponse = [{ type: "string", maxLength: 0 }]; //TODO use shared response for this once changes merged

// Data Setup
const testConfig: TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request using Okta Auth",
        data: {
          qs: {
            username: Ben.email
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        description: "Valid Request using MP Auth",
        data: {
          qs: {
            username: Ben.email
          }
        },
        setup: function () {
          return authorizeWithMP(Ben.email, Ben.password as string, this.data);
        }
      }
    ],
    result: {
      status: 200,
      body: {
        schemas: [mpAuthenticatedSchemaProperties, userBasicAuthContract],
        properties: [{
          name: "userEmail",
          value: Ben.email
        },
        {
          name: "username",
          value: Ben.firstName
        },
          // {
          //   name: "userToken",
          //   value: //Ben's access token
          // },
        ]
      }
    }
  },
  {
    setup: [{
      //This scenario is problematic because it returns the incorrect userToken (ie. access token) for 
      // the username requested. Depending on the use case it would be better to return:
      // a. 401 Unauthorized - Prevent authorized user A from requesting information for user B
      // b. 200 - But return an empty string for the userToken property
      // c. 200 - But return an object without any properties related to authorization (ex. without the userToken, userTokenExp or refreshToken)
      description: "Request for another user who is not the authorized user",
      data: {
        qs: {
          username: Sue.email
        }
      },
      setup: function () {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
      }
    }],
    result: {
      status: 200,
      body: {
        schemas: [mpAuthenticatedSchemaProperties, userBasicAuthContract],
        properties: [{
          name: "userEmail",
          value: Sue.email
        },
        {
          name: "username",
          value: Sue.firstName
        },
          // {
          //   name: "userToken",
          //   value: //Ben's access token
          // },
        ]
      }
    }
  },
  {
    setup: [
      {
        description: "Unauthorized request using MP Client Credentials token",
        data: {
          qs: {
            username: Ben.email
          }
        },
        setup: function () {
          return authorizeWithMPClient(this.data)
        }
      },
      {
        description: "Request missing authorization",
        data: {
          qs: {
            username: Ben.email
          }
        },
      },
      {
        description: "Request with expired Okta Auth",
        data: {
          qs: {
            username: Ben.email
          },
          headers: {
            Authorization: "eyJraWQiOiJ6YlV5c0E0NnhGWmNDNlNodXVhVE01emZKckRLUXVVTkZUTzJOcDF4TkdjIiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULlZnOWdjbEVMS0x4OU56SlA5a1dQcFpuM0JsSnJzX1ZvWkRWNl92REN0c28iLCJpc3MiOiJodHRwczovL2F1dGhwcmV2aWV3LmNyb3Nzcm9hZHMubmV0L29hdXRoMi9kZWZhdWx0IiwiYXVkIjoiYXBpOi8vZGVmYXVsdCIsImlhdCI6MTYwOTg4MDg3NSwiZXhwIjoxNjA5ODgxNDc1LCJjaWQiOiIwb2FrNzZncjltaUpJRklDSjBoNyIsInVpZCI6IjAwdWkzOG0xd21yeTZVeUNDMGg3Iiwic2NwIjpbIm9wZW5pZCJdLCJzdWIiOiJtcGNyZHMrYXV0bysyQGdtYWlsLmNvbSIsIm1wQ29udGFjdElkIjoiNzc3MjI0OCIsInRlc3RVc2VyIjpbIlRlc3QiXX0.EgYvzBbkmWPY-ltp-Aq7EAw8sSUQXogXCYm-tRYaOZeyntFJFHApqyaDBoOsrewqcStbAfPww3zj0Q6ieFeTJSjQ1lHDeHmtqf-fouZG9bVh_5_bdbmAVh-PLRPE4Iau3udqkCzRB43Q-qxa4DmTNpR4TF-LPmgC_wyHw4nh17Y18esQrA8oS7CG-ponLB-IUZN70jE8ec42K7UHUECkmdZMWxpOPfRuSYAXj4QWuw5IFFYeEeJumL4eTSYkPUESJBgscaukIDdyRJraFgKCCnDx7_ao-JiUcIf3sRXtZbZN0H46msCl6hzQzf-TBx9sUhFXdImLSDg01mQQElT2Ww"
          }
        },
      },
      {
        description: "Request with expired MP Auth",
        data: {
          qs: {
            username: Ben.email
          },
          headers: {
            Authorization: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJodHRwczovL2FkbWluaW50LmNyb3Nzcm9hZHMubmV0L21pbmlzdHJ5cGxhdGZvcm1hcGkvb2F1dGgiLCJhdWQiOiJodHRwczovL2FkbWluaW50LmNyb3Nzcm9hZHMubmV0L21pbmlzdHJ5cGxhdGZvcm1hcGkvb2F1dGgvcmVzb3VyY2VzIiwiZXhwIjoxNjA5ODgyNjc4LCJuYmYiOjE2MDk4ODA4NzgsImNsaWVudF9pZCI6IkNSRFMuVGVzdEF1dG9tYXRpb24iLCJzY29wZSI6WyJodHRwOi8vd3d3LnRoaW5rbWluaXN0cnkuY29tL2RhdGFwbGF0Zm9ybS9zY29wZXMvYWxsIiwib3BlbmlkIl0sInN1YiI6IjhhMjkwMDkyLTM0NTktNGY1YS05NzdlLTVjYzdiMDJlNzM0MiIsImF1dGhfdGltZSI6MTYwOTg4MDg3OCwiaWRwIjoiaWRzcnYiLCJuYW1lIjoibXBjcmRzK2F1dG8rMkBnbWFpbC5jb20iLCJhbXIiOlsicGFzc3dvcmQiXX0.W8Fqw3u6RZdSmXWW7pVauBtyonaK3JPrHeF5F9YlFsjcgqDZDgACdfsUbTJS3bHnFJsmKSBcnRnij4gSk_sSTYESl9AeoN7srb2J4OTWUbsUBAsNOWGIYuMceNWMbMFkqWHbyaDenCVGr2otKSp3Te0yfvxkMXoCnFEzqeEzBImARigp43bW3bd5uN9t08HwnR_2YrTG9T_raeJoGnMYLhqtmxV4GaaUznZRoap39QO1HNmPsJ3ch6fEr7bWmebWqA3SeXoXObSgm62uHfbAmEfNFtdhLAodD6MFh07SRu4p3Dz3Prh1y0dDrbERkHmQYOnIAikak_XiBksxfPck_A"
          }
        },
      },
    ],
    result: {
      status: 401,
      body: {
        schemas: emptyResponse
      }
    }
  },
  {
    setup: [
      {
        description: "Request for a user who does not exist",
        data: {
          qs: {
            username: "thisUsershouldNOTexist@email.com"
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
      {
        //This probably isn't a security issue, and MP's api probably protects against SQL injection,
        //  but we should catch inputs with problematic syntax before they ever hit the database
        description: "Request for a user using SQL search returns multiple users",
        data: {
          qs: {
            username: "%@contractor.crossroads.net"
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
    ],
    result: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "User email did not return exactly one user record" }]
      }
    },
    preferredResult: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "User not found" }]
      }
    }
  },
  {
    setup: [
      {
        //This error scenario could be avoided if Gateway searched by the "username" column, which
        // is guaranteed unique.
        description: "Request for a user whose 'username' is unique but whose 'email' is not",
        data: {
          qs: {
            username: "no-reply@crossroads.net"
          }
        },
        setup: function () {
          return authorizeWithOkta(Ben.email, Ben.password as string, this.data)
        }
      },
    ],
    result: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "User email did not return exactly one user record" }]
      }
    },
    preferredResult: {
      status: 200,
      body: {
        schemas: [mpAuthenticatedSchemaProperties, userBasicAuthContract],
        properties: [
          {
            name: "userEmail",
            value: "no-reply@crossroads.net"
          },
          {
            name: "username",
            value: "Contact"
          }
        ]
      }
    }
  },
];

// Run Tests
describe('GET /api/user', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyCredentials: Partial<Cypress.RequestOptions> = {
          url: `/api/user`,
          method: "GET",
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