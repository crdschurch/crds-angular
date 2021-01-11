import { Ben, Sue } from 'shared/users';
import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";

// Data Setup
const sharedRequest = {
  urls: ["/api/verifycredentials", "/api/v1.0.0/verify-credentials"],
  options: {
    method: "POST",
    failOnStatusCode: false
  }
}
const successScenario: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Valid Request using MP Auth",
      request: {
        body: {
          username: Ben.email,
          password: Ben.password
        }
      },
      setup() {
        return authorizeWithMP(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: { status: 200 }
    },
    {
      description: "Valid Request using Okta Auth",
      request: {
        body: {
          username: Ben.email,
          password: Ben.password
        }
      },
      setup() {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: { status: 200 }
    },
    {
      description: "Request authorized by a different user",
      request: {
        body: {
          username: Ben.email,
          password: Ben.password
        }
      },
      setup() {
        return authorizeWithOkta(Sue.email, Sue.password as string, this.request).then(() => this);
      },
      response: { status: 200 }
    },
  ]
}
const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Invalid user password",
      request: {
        body: {
          username: Ben.email,
          password: "NotAPassword"
        },
      },
      setup() {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: { status: 401 }
    },
    {
      description: "Incorrect email",
      request: {
        body: {
          username: `123${Ben.email}`,
          password: Ben.password
        },
      },
      setup() {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: { status: 401 }
    },
    {
      description: "Request missing authorization",
      request: {
        body: {
          username: Ben.email,
          password: Ben.password
        }
      },
      response: { status: 401 }
    }
  ]
}

describe("/Login/VerifyCredentials()", () => {
  unzipScenarios(successScenario).forEach(runTest)
  unzipScenarios(unauthorizedScenarios).forEach(runTest)
})