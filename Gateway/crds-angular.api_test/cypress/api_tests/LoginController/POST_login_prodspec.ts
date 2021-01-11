import { Ben } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";

// Data Setup
const sharedRequest = {
  urls: ["/api/login", "/api/v1.0.0/login"],
  options: {
    method: "POST",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Valid User",
      request: { body: { username: Ben.email, password: Ben.password } },
      response: {
        status: 200,
        schemas: [mpLoginSchemaProperties, mpLoginBasicAuthContract],
        properties: [{ name: "userEmail", value: Ben.email }]
      }
    }
  ]
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [badRequestProperties, badRequestContract],
    properties: [{ name: "message", value: "Login Failed" }]
  },
  scenarios: [
    {
      description: "Incorrect Password",
      request: { body: { username: Ben.email, password: "bad" } },
      response: { status: 400 }
    },
    {
      description: "Missing Password",
      request: { body: { username: Ben.email } },
      response: { status: 400 }
    },
    {
      description: "Missing Username",
      request: { body: { password: Ben.password } },
      response: { status: 400 }
    },
    {
      description: "User Doesn't Exist",
      request: { body: { username: "fakeUser@fakemail.wxyz", password: Ben.password } },
      response: { status: 400 }
    },
    {
      description: "Body Undefined",
      request: { body: undefined },
      response: { 
        status: 400, 
        parseAsJSON: true 
      }
    },
    {
      description: "Body Empty String",
      request: { body: "" },
      response: { 
        status: 400, 
        parseAsJSON: true 
      }
    },
  ]
}

describe("/Login/Post()", () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(badRequestScenarios).forEach(runTest);
});