import { TestConfig, TestCase, unzipTests } from "../../shared/test_scenario_factory";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";

// Data Setup
const username = "mpcrds+auto+2@gmail.com";
const password = Cypress.env("BEN_KENOBI_PW");

const testConfig: TestConfig[] = [
  {
    setup: { description: "Valid User", body: { username, password } },
    result: {
      status: 200,
      body: {
        schemas: [mpLoginSchemaProperties, mpLoginBasicAuthContract],
        properties: [{ name: "userEmail", value: username }]
      }
    }
  },
  {
    setup: [
      { description: "Incorrect Password", body: { username, password: "bad" } },
      { description: "Missing Password", body: { username } },
      { description: "Missing Username", body: { password } },
      { description: "User Doesn't Exist", body: { username: "fakeUser@fakemail.wxyz", password } },
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