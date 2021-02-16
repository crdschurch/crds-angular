import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { getUUID } from "shared/data_generator";
import { setPasswordResetToken } from "shared/mp_api";
import { Ben, KeeperJr, Load } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";

// Data Setup
const sharedRequest = {
  urls: ["/api/requestpasswordreset", "/api/v1.0.0/request-password-reset"],
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
        body: { email: Ben.email }
      },
      setup() {
        //Remove any existing token
        return setPasswordResetToken(Ben.email, "").then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "User Has Pending Password Reset Request",
      request: {
        body: { email: Ben.email },
      },
        setup() {
          return setPasswordResetToken(Ben.email, getUUID()).then(() => this)
        },
        response: {
          status: 200
        }
    },
    {
      description: "Email is Substring of Another Email",
      request: { body: { email: KeeperJr.email } },
      response: { status: 200 },
      preferredResponse: { 
        status: 200 
      }
    },
    {
      description: "Person Doesn't Exist",
      request: { 
        body: { 
          email: "this_email_should_not_exist@fake_emails.io" 
        } },
        response: {
          status: 200
        },
        preferredResponse: {
          //Not technically a bug but misleading to user
          status: 404,
          schemas: [badRequestProperties, badRequestContract],
          properties: [{ name: "message", exactValue: "User Not Found" }]
        }
    },
    {
      description: "Person has Contact Record but no User Record",
      request: { body: { email: Load.email } },
      response: {
        status: 200
      },
      preferredResponse: {
        //Not technically a bug but misleading to user
        status: 404,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", exactValue: "User Not Found" }]
      }
    },
    {
      description: "Invalid Email",
      request: { body: { email: "{" } },
      response: {
        status: 200
      },
      preferredResponse: {
        //Not technically a bug but misleading to user
        status: 404,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", exactValue: "User Not Found" }]
      }
    },
    {
      description: "Email is Undefined", 
      request: { body: {} },
      response: {
        status: 200
      },
      preferredResponse: {
        //Not technically a bug but misleading to user
        status: 400,
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", exactValue: "Missing Email" }]
      }
    }
  ]
}

const serverErrorScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [{
    description: "Missing Body",
    request: { },
    response: {
      status: 500,
      bodyIsEmpty: true
    },
    preferredResponse: {
      status: 400,
      schemas: [badRequestProperties, badRequestContract],
      properties: [{ name: "message", exactValue: "Missing Email" }]
    }
  }]
}

describe("/Login/RequestPasswordReset()", () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(serverErrorScenarios).forEach(runTest);
})