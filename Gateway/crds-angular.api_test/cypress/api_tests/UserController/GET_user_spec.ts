import { getUserSchemaProperties, getUserAuthContract } from "./schemas/getUserSchemas";
import { badRequestContract, badRequestProperties } from "./schemas/badRequestSchemas";
import { addAuthorizationHeader as authorizeWithMPClient } from "shared/authorization/mp_client_auth";
import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Placeholders } from "shared/enums";
import { Ben, KeeperJr, Sue } from "shared/users";

// Data Setup
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest: {
    urls: ["/api/user"],
    options: {
      method: "GET"
    }
  },
  sharedResponse: {
    schemas: [getUserSchemaProperties, getUserAuthContract],
    properties: [
      {
        name: "userToken",
        value: Placeholders.assignedInSetup
      }
    ]
  },
  scenarios: [
    {
      description: "Valid Request using Okta Auth",
      request: {
        qs: {
          username: Ben.email
        }
      },
      setup(){
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request)
          .then((token) => {
            // Add Authorization token in response check
            (this.response.properties?.find(p => p.name === 'userToken') as CAT.PropertyCompare) //Make Typescript happy <3
              .value = token;

            return this;
          });
      },
      response: {
        status: 200,
          properties: [
            {
              name: "userEmail",
              value: Ben.email
            },
            {
              name: "username",
              value: Ben.firstName
            }
          ]
      },
    },
    {
      description: "Valid Request using MP Auth",
      request: {
        qs: {
          username: Ben.email
        }
      },
      setup(){
        return authorizeWithMP(Ben.email, Ben.password as string, this.request)
          .then((token) => {
            // Add Authorization token in response check
            (this.response.properties?.find(p => p.name === 'userToken') as CAT.PropertyCompare) //Make Typescript happy <3
              .value = token;

            return this;
          });
      },
      response: {
        status: 200,
          properties: [
            {
              name: "userEmail",
              value: Ben.email
            },
            {
              name: "username",
              value: Ben.firstName
            }
          ]
      },
    },
    {
      //This scenario is problematic because it returns the incorrect userToken (ie. access token) for 
      // the username requested. Depending on the use case it would be better to return:
      // a. 401 Unauthorized - Prevent authorized user A from requesting information for user B
      // b. 200 - But return an empty string for the userToken property
      // c. 200 - But return an object without any properties related to authorization (ex. without the userToken, userTokenExp or refreshToken)
      description: "Request for another user who is not the authorized user",
      request: {
        qs: {
          username: Sue.email
        }
      },
      setup(){
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request)
          .then((token) => {
            // Add Authorization token in response check
            (this.response.properties?.find(p => p.name === 'userToken') as CAT.PropertyCompare) //Make Typescript happy <3
              .value = token;

            return this;
          });
      },
      response: {
        status: 200,
          properties: [
            {
              name: "userEmail",
              value: Sue.email
            },
            {
              name: "username",
              value: Sue.firstName
            }
          ]
      },
    },
  ]
};

const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest: {
    urls: ["/api/user"],
    options: {
      method: "GET",
      failOnStatusCode: false
    }
  },
  sharedResponse: {
    bodyIsEmpty: true
  },
  scenarios: [
    {
      description: "Request missing authorization",
      request: {
        qs: {
          username: Ben.email
        }
      },
      response: {
        status: 401
      }
    },
    {
      description: "Request with expired Okta Auth",
      request: {
        qs: {
          username: Ben.email
        },
        headers: {
          Authorization: "eyJraWQiOiJ6YlV5c0E0NnhGWmNDNlNodXVhVE01emZKckRLUXVVTkZUTzJOcDF4TkdjIiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULlZnOWdjbEVMS0x4OU56SlA5a1dQcFpuM0JsSnJzX1ZvWkRWNl92REN0c28iLCJpc3MiOiJodHRwczovL2F1dGhwcmV2aWV3LmNyb3Nzcm9hZHMubmV0L29hdXRoMi9kZWZhdWx0IiwiYXVkIjoiYXBpOi8vZGVmYXVsdCIsImlhdCI6MTYwOTg4MDg3NSwiZXhwIjoxNjA5ODgxNDc1LCJjaWQiOiIwb2FrNzZncjltaUpJRklDSjBoNyIsInVpZCI6IjAwdWkzOG0xd21yeTZVeUNDMGg3Iiwic2NwIjpbIm9wZW5pZCJdLCJzdWIiOiJtcGNyZHMrYXV0bysyQGdtYWlsLmNvbSIsIm1wQ29udGFjdElkIjoiNzc3MjI0OCIsInRlc3RVc2VyIjpbIlRlc3QiXX0.EgYvzBbkmWPY-ltp-Aq7EAw8sSUQXogXCYm-tRYaOZeyntFJFHApqyaDBoOsrewqcStbAfPww3zj0Q6ieFeTJSjQ1lHDeHmtqf-fouZG9bVh_5_bdbmAVh-PLRPE4Iau3udqkCzRB43Q-qxa4DmTNpR4TF-LPmgC_wyHw4nh17Y18esQrA8oS7CG-ponLB-IUZN70jE8ec42K7UHUECkmdZMWxpOPfRuSYAXj4QWuw5IFFYeEeJumL4eTSYkPUESJBgscaukIDdyRJraFgKCCnDx7_ao-JiUcIf3sRXtZbZN0H46msCl6hzQzf-TBx9sUhFXdImLSDg01mQQElT2Ww"
        }
      },
      response: {
        status: 401
      }
    },
    {
      description: "Request with expired MP Auth",
      request: {
        qs: {
          username: Ben.email
        },
        headers: {
          Authorization: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJodHRwczovL2FkbWluaW50LmNyb3Nzcm9hZHMubmV0L21pbmlzdHJ5cGxhdGZvcm1hcGkvb2F1dGgiLCJhdWQiOiJodHRwczovL2FkbWluaW50LmNyb3Nzcm9hZHMubmV0L21pbmlzdHJ5cGxhdGZvcm1hcGkvb2F1dGgvcmVzb3VyY2VzIiwiZXhwIjoxNjA5ODgyNjc4LCJuYmYiOjE2MDk4ODA4NzgsImNsaWVudF9pZCI6IkNSRFMuVGVzdEF1dG9tYXRpb24iLCJzY29wZSI6WyJodHRwOi8vd3d3LnRoaW5rbWluaXN0cnkuY29tL2RhdGFwbGF0Zm9ybS9zY29wZXMvYWxsIiwib3BlbmlkIl0sInN1YiI6IjhhMjkwMDkyLTM0NTktNGY1YS05NzdlLTVjYzdiMDJlNzM0MiIsImF1dGhfdGltZSI6MTYwOTg4MDg3OCwiaWRwIjoiaWRzcnYiLCJuYW1lIjoibXBjcmRzK2F1dG8rMkBnbWFpbC5jb20iLCJhbXIiOlsicGFzc3dvcmQiXX0.W8Fqw3u6RZdSmXWW7pVauBtyonaK3JPrHeF5F9YlFsjcgqDZDgACdfsUbTJS3bHnFJsmKSBcnRnij4gSk_sSTYESl9AeoN7srb2J4OTWUbsUBAsNOWGIYuMceNWMbMFkqWHbyaDenCVGr2otKSp3Te0yfvxkMXoCnFEzqeEzBImARigp43bW3bd5uN9t08HwnR_2YrTG9T_raeJoGnMYLhqtmxV4GaaUznZRoap39QO1HNmPsJ3ch6fEr7bWmebWqA3SeXoXObSgm62uHfbAmEfNFtdhLAodD6MFh07SRu4p3Dz3Prh1y0dDrbERkHmQYOnIAikak_XiBksxfPck_A"
        }
      },
      response: {
        status: 401
      }
    },
  ],
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest: {
    urls: ["/api/user"],
    options: {
      method: "GET",
      failOnStatusCode: false
    }
  },
  sharedResponse: {
    schemas: [badRequestProperties, badRequestContract],
    properties: [
      {
        name: "message",
        value: "User email did not return exactly one user record"
      }
    ],    
    parseAsJSON: true
  },
  scenarios: [
    {
      description: "Request for a user who does not exist",
      request: {
        qs: {
          username: "thisUsershouldNOTexist@email.com"
        },
      },
      setup(){
        return authorizeWithMP(Ben.email, Ben.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 400
      },
      preferredResponse: {
        properties: [{ name: "message", value: "User not found" }]
      }
    },
    {
      //This probably isn't a security issue, and MP's api probably protects against SQL injection,
      //  but we should catch inputs with problematic syntax before they ever hit the database
      description: "Request for a user using SQL search returns multiple users",
      request: {
        qs: {
          username: "%@contractor.crossroads.net"
        },
      },
      setup(){
        return authorizeWithMP(Ben.email, Ben.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 400
      },
      preferredResponse: {
          properties: [{ name: "message", value: "User not found" }]
      }
    },
    {
      description: "Request for a user whose email is a subset of another user's email (bug)",
      request: {
        qs: {
          username: KeeperJr.email
        },
      },
      setup(){
        return authorizeWithMP(KeeperJr.email, KeeperJr.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 400
      },
      preferredResponse: {
        status: 200
      }
    },
    {
      //This error scenario could be avoided if Gateway searched by the "username" column, which
      // is guaranteed unique.
      description: "Request for a user whose 'username' is unique but whose 'email' is not",
      request: {
        qs: {
          username: "no-reply@crossroads.net"
        },
      },
      setup(){
        return authorizeWithMP(Ben.email, Ben.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 400
      },
      preferredResponse: {
        status: 200,
          schemas: [getUserSchemaProperties, getUserAuthContract],
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
    },
  ],
}

const serverErrorScenario: CAT.TestScenario = {
  description: "Unauthorized request using MP Client Credentials token",
  request: {
    url: "/api/user",
    method: "GET", 
    qs: {
      username: Ben.email
    },
    failOnStatusCode: false
  },
  setup() {
    return authorizeWithMPClient(this.request).then(() => this);
  },
  response: {
    status: 500,
      properties: [
        {
          name: "Message",
          value: "An error has occurred."
        }
      ]
  }
}

// Run Tests
describe('/User/Get()', () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(unauthorizedScenarios).forEach(runTest);
  unzipScenarios(badRequestScenarios).forEach(runTest);
  runTest(serverErrorScenario);
});