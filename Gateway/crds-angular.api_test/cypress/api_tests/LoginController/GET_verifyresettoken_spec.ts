import { setPasswordResetToken } from "shared/mp_api";
import { unzipTests } from "shared/test_scenario_factory";
import { Gatekeeper, } from 'shared/users';
import { getUUID } from "shared/data_generator";
import { mpVerifyResetTokenContract, mpVerifyResetTokenSchemaProperties } from "./schemas/verifyresettokenResponse";
import { ResponseTypes } from "shared/enums";

// Data Setup
const testConfig:TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        data: {
          token: getUUID()
        },
        setup: function () {
          return setPasswordResetToken(Gatekeeper.email, this.data?.token)
        }
      }
    ],
    result: { 
      status: 200,
      body: {
        schemas: [mpVerifyResetTokenSchemaProperties, mpVerifyResetTokenContract],
        properties: [{
          name: "TokenValid",
          value: true
        }] 
      }
     }
  },
  {
    setup: [
      {
        description: "Reset Token that Does Not Match any Reset Request",
        data: {
          token: '123FakeToken99876'
        }
      },
    ],
    result: { 
      status: 200,
      body:{
        schemas: [mpVerifyResetTokenSchemaProperties, mpVerifyResetTokenContract],
        properties: [{
          name: "TokenValid",
          value: false
        }] 
      }
     }
  },
  {
    setup: [      
      {
        description: "Reset Token is empty string",
        data: {
          token: ''
        }
      },
    ],
    result:{
      status: 404,
      body: { 
        contentType: ResponseTypes.text
      }
    }
  }
];

// Run Tests
describe('GET /api/verifyresettoken/{token}', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyResetToken: Partial<Cypress.RequestOptions> = {
          url: `/api/verifyresettoken/${t.data.token}`,
          method: "GET",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpVerifyResetToken))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});

describe('GET /api/v1.0.0/verify-reset-token/{token}', () => {
  unzipTests(testConfig)
    .forEach((t: TestFactory.Test) => {
      it(t.title, () => {
        const mpVerifyResetToken: Partial<Cypress.RequestOptions> = {
          url: `/api/v1.0.0/verify-reset-token/${t.data.token}`,
          method: "GET",
          failOnStatusCode: false
        };

        t.setup() //Arrange
          .then(() => cy.request(t.buildRequest(mpVerifyResetToken))) //Act
          .verifyStatus(t.result.status) //Assert
          .itsBody(t.result.body)
          .verifySchema(t.result.body)
          .verifyProperties(t.result.body);
      });
    });
});