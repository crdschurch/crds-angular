import { getToken as getMPToken} from "shared/authorization/mp_user_auth";
import { unzipTests } from "shared/test_scenario_factory";
import { Ben } from "shared/users";
import { mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties, errorHasOccurredProperties, errorHasOccurredContract } from './schemas/authenticatedResponse';

//Test Data
const testConfig: TestFactory.TestConfig[] = [
  {
    setup: [
      {
        description: "Valid Request",
        setup: function () {
          return getMPToken(Ben.email, Ben.password as string)
            .then((token) => {
              this.data.header = { Authorization: token }
            });
        }
      }      
    ],
    result: {
      status: 200,
      body: {
        schemas: [mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties]
      }
    }
  },
  {
    setup: [
      {
        description: "Expired Authorization token",
        data: {
          header: {
            Authorization: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTYwNDQzNDA2MSwibmJmIjoxNjA0NDMyMjYxLCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbImh0dHA6Ly93d3cudGhpbmttaW5pc3RyeS5jb20vZGF0YXBsYXRmb3JtL3Njb3Blcy9hbGwiLCJvZmZsaW5lX2FjY2VzcyIsIm9wZW5pZCJdLCJzdWIiOiJkOTNjNDMxYi02OWQ2LTRiZDYtYTdiZC0zNmRhOWIyOGY3NjIiLCJhdXRoX3RpbWUiOjE2MDQ0MzIyNjEsImlkcCI6Imlkc3J2IiwibmFtZSI6Im1wY3JkcythbGljaWFrZXlzQGdtYWlsLmNvbSIsImFtciI6WyJwYXNzd29yZCJdfQ.knkFeMdE5D6sJRpAyvTMcvqScKl9G0y0o8nXHKtabpai_2Z9FlW9bQrOjrjM6oA8Y3M1Fh7lF2dAVurxG_278fn8lFWThMpDAlPFrB8m3IO4RkBDd6EQduelFHKXuVsae9zAav63KuSdW5L8hTZVWNldEPwxq3TAeG9Mz3Ci61GwKE5AqXqVtopd0dO4w5AS9tr4XDlgj4D1Fk-ypExq4FYYbi_Q6HeuKF_Jn0JT5vivZtl3hW13ZQgTCtCdVjUhkdXc0oNpcCaXFXTAvhpqG2Efz_bZp4ixMBBxpBuNZ3ugB2BjYUx398v1H01Xiq92Ue2sjWe39WLtdqZblP61vQ"
          }
        }
      },
      {
        description: "Missing Authorization header"
      }
    ],
    result: {
      status: 401,
      body: {
        schemas: [{ type: "string", maxLength: 0 }]
      }
    }
  },
  {
    setup: [
      {
        description: "Invalid Authorization token",
        data: {
          header: {
            Authorization: "123"
          }
        }
      }      
    ],
    result: {
      status: 500,
      body: {
        schemas: [ errorHasOccurredProperties, errorHasOccurredContract ]
      }
    }
  }
];

describe('GET /api/authenticated', () => {
  unzipTests(testConfig)
  .forEach((t: TestFactory.Test) => {
    it(t.title, () => {
      const authenticatedRequest: Partial<Cypress.RequestOptions> = {
        url: "/api/authenticated",
        method: "GET",
        failOnStatusCode: false
      };

      t.setup() //Arrange
        .then(() => cy.request(t.buildRequest(authenticatedRequest))) //Act
        .verifyStatus(t.result.status) //Assert
        .itsBody(t.result.body)
        .verifySchema(t.result.body)
        .verifyProperties(t.result.body);
    });
  });
});

describe('GET /api/v1.0.0/authenticated', () => {
  unzipTests(testConfig)
  .forEach((t: TestFactory.Test) => {
    it(t.title, () => {
      const authenticatedRequest: Partial<Cypress.RequestOptions> = {
        url: "/api/v1.0.0/authenticated",
        method: "GET",
        failOnStatusCode: false
      };

      t.setup() //Arrange
        .then(() => cy.request(t.buildRequest(authenticatedRequest))) //Act
        .verifyStatus(t.result.status) //Assert
        .itsBody(t.result.body)
        .verifySchema(t.result.body)
        .verifyProperties(t.result.body);
    });
  });
});