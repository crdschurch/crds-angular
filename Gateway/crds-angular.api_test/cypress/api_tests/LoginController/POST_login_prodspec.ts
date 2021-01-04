import { Ben } from "shared/users";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";
import { unzipTests } from "shared/test_scenario_factory";

// Data Setup
const testConfig:TestFactory.TestConfig[] = [
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
    .forEach((t: TestFactory.Test) => {
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

describe('POST /api/v1.0.0/login', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpLoginRequest: Partial<Cypress.RequestOptions> = {
          url: "/api/v1.0.0/login",
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