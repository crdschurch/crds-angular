import { setPasswordResetToken } from "shared/mp_api";
import { Gatekeeper, } from 'shared/users';
import { getUUID } from "shared/data_generator";
import { mpVerifyResetTokenContract, mpVerifyResetTokenSchemaProperties } from "./schemas/verifyresettokenResponse";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";

// Data Setup
function setUrl(token: string, request: Partial<Cypress.RequestOptions>){
  request.url = `${request.url}/${token}`;
}

const sharedRequest = {
  urls: ["/api/verifyresettoken", "/api/v1.0.0/verify-reset-token"],
  options: {
    method: "GET",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse:{
    schemas: [mpVerifyResetTokenSchemaProperties, mpVerifyResetTokenContract]
  },
  scenarios: [
    {
      description: "Valid Request",
      request: {},
      setup() {
        const token = getUUID();
        setUrl(token, this.request);
        return setPasswordResetToken(Gatekeeper.email, token).then(() => this);
      },
      response: {
        status: 200,
        properties: [{
          name: "TokenValid",
          value: true
        }]
      }
    },
    {
      description: "Reset Token that Does Not Match any Reset Request",
      request: {},
      setup() {
        const token = '123FakeToken99876';
        setUrl(token, this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200,
        properties: [{
          name: "TokenValid",
          value: false
        }]
      }
    },
  ]
}
const notFoundScenario: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Reset Token is empty string",
      request: {},
      setup(){
        setUrl("", this.request);
        return cy.wrap(this);
      },
      response:{
        status: 404
      }
    },
  ]
}

describe("/Login/VerifyResetTokenRequest()", () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(notFoundScenario).forEach(runTest);
})