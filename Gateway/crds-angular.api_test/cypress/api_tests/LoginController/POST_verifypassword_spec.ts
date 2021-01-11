import { Ben, Sue } from 'shared/users';
import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { unzipScenarios, runTest } from "shared/CAT/cypress_api_tests";

// Data Setup
const sharedRequest = {
  urls: ["/api/verifypassword", "/api/v1.0.0/verify-password"],
  options: {
    method: "POST",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Valid Request",
      request: {
        body: JSON.stringify(Ben.password),
        headers: {
          'Content-Type': 'application/json'
        }
      },
      setup() {
        return authorizeWithMP(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: {
        status: 200,
        bodyIsEmpty: true
      }
    },
  ]
}

const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    bodyIsEmpty: true,
  },
  scenarios: [
    {
      description: "Invalid user password",
      request: {
        body: JSON.stringify("NotMyAPassword"),
        headers: {
          'Content-Type': 'application/json'
        }
      },
      setup() {
        return authorizeWithMP(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: {
        status: 401
      }
    },
    {
      description: "Request authorized by a different user",
      request: {
        body: JSON.stringify(Ben.password),
        headers: {
          'Content-Type': 'application/json'
        }
      },
      setup() {
        return authorizeWithMP(Sue.email, Sue.password as string, this.request).then(() => this);
      },
      response: {
        status: 401
      }
    },
    {
      description: "Request missing authorization",
      request: {
        body: JSON.stringify(Ben.password),
        headers: {
          'Content-Type': 'application/json'
        }
      },
      response: { status: 401 }
    }
  ]
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Request authorized using Okta Auth",
      request: {
        body: JSON.stringify(Ben.password),
        headers: {
          'Content-Type': 'application/json'
        }
      },
      setup() {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: {
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{
          name: "message",
          value: "Verify Password Failed"
        }],
        parseAsJSON: true
      }
    }
  ]
}

describe("/Login/VerifyPassword()", () => {
  unzipScenarios(successScenarios).forEach(runTest)
  unzipScenarios(unauthorizedScenarios).forEach(runTest)
  unzipScenarios(badRequestScenarios).forEach(runTest)
});