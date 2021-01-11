import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Ben } from "shared/users";
import { mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties, errorHasOccurredProperties, errorHasOccurredContract } from './schemas/authenticatedResponse';

//Test Data
const sharedRequest = {
  urls: ["/api/authenticated", "/api/v1.0.0/authenticated"],
  options: {
    method: "GET",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Valid Request",
      request: {},
      setup() {
        return authorizeWithMP(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: {
        status: 200,
        schemas: [mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties]
      }
    }
  ]
}

const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    bodyIsEmpty: true
  },
  scenarios: [
    {
      description: "Expired Authorization token",
      request: {
        headers: {
          Authorization: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTYwNDQzNDA2MSwibmJmIjoxNjA0NDMyMjYxLCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbImh0dHA6Ly93d3cudGhpbmttaW5pc3RyeS5jb20vZGF0YXBsYXRmb3JtL3Njb3Blcy9hbGwiLCJvZmZsaW5lX2FjY2VzcyIsIm9wZW5pZCJdLCJzdWIiOiJkOTNjNDMxYi02OWQ2LTRiZDYtYTdiZC0zNmRhOWIyOGY3NjIiLCJhdXRoX3RpbWUiOjE2MDQ0MzIyNjEsImlkcCI6Imlkc3J2IiwibmFtZSI6Im1wY3JkcythbGljaWFrZXlzQGdtYWlsLmNvbSIsImFtciI6WyJwYXNzd29yZCJdfQ.knkFeMdE5D6sJRpAyvTMcvqScKl9G0y0o8nXHKtabpai_2Z9FlW9bQrOjrjM6oA8Y3M1Fh7lF2dAVurxG_278fn8lFWThMpDAlPFrB8m3IO4RkBDd6EQduelFHKXuVsae9zAav63KuSdW5L8hTZVWNldEPwxq3TAeG9Mz3Ci61GwKE5AqXqVtopd0dO4w5AS9tr4XDlgj4D1Fk-ypExq4FYYbi_Q6HeuKF_Jn0JT5vivZtl3hW13ZQgTCtCdVjUhkdXc0oNpcCaXFXTAvhpqG2Efz_bZp4ixMBBxpBuNZ3ugB2BjYUx398v1H01Xiq92Ue2sjWe39WLtdqZblP61vQ"
        }
      },
      response: {
        status: 401
      }
    },
    {
      description: "Missing Authorization header",
      request: {},
      response: {
        status: 401
      }
    }
  ]
}

const serverErrorScenario: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Invalid Authorization token",
      request: {
        headers: {
          Authorization: "123"
        }
      },
      response: {
        status: 500,
        schemas: [errorHasOccurredProperties, errorHasOccurredContract]
      }
    }
  ]
}

describe("/Login/isAuthenticated()", () => {
  unzipScenarios(successScenarios).forEach(runTest)
  unzipScenarios(unauthorizedScenarios).forEach(runTest)
  unzipScenarios(serverErrorScenario).forEach(runTest)
})