import { Ben } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";
import { unzipTests, Test, TestConfig } from "shared/test_scenario_factory";

// Data Setup
const testConfig: TestConfig[] = [
  {
    setup: {
      description: "Valid User",
      data: { body: { username: Ben.email, password: Ben.password } },
    },
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
      { description: "Incorrect Password", data: { body: { username: Ben.email, password: "bad" } } },
      { description: "Missing Password", data: { body: { username: Ben.email } } },
      { description: "Missing Username", data: { body: { password: Ben.password } } },
      { description: "User Doesn't Exist", data: { body: { username: "fakeUser@fakemail.wxyz", password: Ben.password } } },
      { description: "Body Undefined", data: { body: undefined } },
      { description: "Body Null", data: { body: null } },
      { description: "Body Empty String", data: { body: "" } },
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
    .forEach((t: Test) => {
      it(t.title, () => {
        const mpLoginRequest: Partial<Cypress.RequestOptions> = {
          url: "/api/login",
          method: "POST",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpLoginRequest))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});
