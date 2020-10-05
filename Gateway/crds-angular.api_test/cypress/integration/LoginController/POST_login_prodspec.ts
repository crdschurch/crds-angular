import { scenarioFactory, TestScenario, TestScenarioRequest, TestScenarioResponse } from "../../shared/test_scenario_factory";
import { badRequestContract, badRequestProperties } from "./schemas/badRequest";
import { mpLoginBasicAuthContract, mpLoginSchemaProperties } from "./schemas/loginResponse";
chai.use(require('chai-json-schema-ajv')
  .create({
    verbose: true
  }));

// Data Setup
const username = "mpcrds+auto+2@gmail.com";
const password = Cypress.env("BEN_KENOBI_PW");

const validRequest: TestScenarioRequest[] = [
  { description: "Valid User", body: { username, password } }
];

const validResponse: TestScenarioResponse = {
  status: 200,
  schemas: [mpLoginSchemaProperties, mpLoginBasicAuthContract],
  properties: [{ name: "userEmail", value: username }]
};

const badRequest: TestScenarioRequest[] = [
  { description: "Incorrect Password", body: { username, password: "bad" } },
  { description: "Missing Password", body: { username } },
  { description: "Missing Username", body: { password } },
  { description: "User Doesn't Exist", body: { username: "fakeUser@fakemail.wxyz", password } },
  { description: "Body Undefined", body: undefined },
  { description: "Body Null", body: null },
  { description: "Body Empty String", body: "" }
];

const badResponse: TestScenarioResponse = {
  status: 400,
  schemas: [badRequestProperties, badRequestContract],
  properties: [{ name: "message", value: "Login Failed" }]
};

// Run Tests
describe('POST /api/login', () => {
  const okScenarios = scenarioFactory(validRequest, validResponse);
  const badRequestScenarios = scenarioFactory(badRequest, badResponse);
  const allScenarios = okScenarios.concat(badRequestScenarios);

  allScenarios.forEach((t: TestScenario) => {
    it(t.title, () => {
      //Arrange
      const mpLoginRequest = {
        url: "/api/login",
        method: "POST",
        body: t.request.body,
        failOnStatusCode: false
      };

      //Act
      cy.request(mpLoginRequest).as('response');

      //Assert
      //Verify response status
      cy.get('@response').its('status').should('eq', t.response.status);

      //Verify response body
      cy.get('@response').its('body').toJSON().as('body');
      t.response.schemas?.forEach((schema) => cy.get('@body').should('have.jsonSchema', schema));
      t.response.properties?.forEach((prop) => cy.get('@body').should('have.property', prop.name, prop.value));
    });
  });
});