import { runTest } from "shared/CAT/cypress_api_tests";
import { Ben } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";

/**
 * This endpoint is an alternate name for the POST /api/requestpasswordreset endpoint.
 * Tests here are lighter tests runnable in Prod - More robust tests are run in the
 * POST /api/requestpasswordreset test file.
*/

// Data Setup
const successScenario: CAT.TestScenario = {
  description: "Valid Request",
  request: {
    url: "/api/v1.0.0/request-password-reset",
    method: "POST",
    body: { email: Ben.email }
  },
  response: {
    status: 200
  }
}

const serverErrorScenario: CAT.TestScenario = {
  description: "Missing Body",
  request: {
    url: "/api/v1.0.0/request-password-reset",
    method: "POST",
    failOnStatusCode: false
  },
  response: {
    status: 500,
    bodyIsEmpty: true
  },
  preferredResponse: {
    //Not technically a bug but error could be more descriptive
    status: 400,
    schemas: [badRequestProperties, badRequestContract],
    properties: [{ name: "message", value: "Missing Email" }]
  }
}

describe("/Login/RequestPasswordReset()", () => {
  runTest(successScenario);
  runTest(serverErrorScenario)
})
