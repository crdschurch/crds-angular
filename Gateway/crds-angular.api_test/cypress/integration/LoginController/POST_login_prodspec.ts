import { TestConfig, TestCase, unzipTests } from "cypress/shared/test_scenario_factory";
import { Ben } from "cypress/shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: { description: "Valid User", body: { username: Ben.email, password: Ben.password } },
    result: {
      status: 200,
      body: {
        schemas: [mpLoginSchemaProperties, mpLoginBasicAuthContract],
        properties: [{ name: "userEmail", value: Ben.email }]
      }
    }
  },
  {
    setup: [
      { description: "Incorrect Password", body: { username: Ben.email, password: "bad" } },
      { description: "Missing Password", body: { username: Ben.email } },
      { description: "Missing Username", body: { password: Ben.password } },
      { description: "User Doesn't Exist", body: { username: "fakeUser@fakemail.wxyz", password: Ben.password } },
      { description: "Body Undefined", body: undefined },
      { description: "Body Null", body: null },
      { description: "Body Empty String", body: "" }
    ],
    result: {
      status: 400,
      body: {
        schemas: [badRequestProperties, badRequestContract],
        properties: [{ name: "message", value: "Login Failed" }]
      }
    }
  }
];

// Run Tests
describe('POST /api/login', () => {
  unzipTests(testConfig)
    .forEach((t: TestCase) => {
      it(t.title, () => {
        //Arrange
        const mpLoginRequest = {
          url: "/api/login",
          method: "POST",
          body: t.setup.body,
          headers: t.setup.header,
          failOnStatusCode: false
        };

        //Act & Assert
        cy.request(mpLoginRequest)
          .verifyStatus(t.result.status)
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});