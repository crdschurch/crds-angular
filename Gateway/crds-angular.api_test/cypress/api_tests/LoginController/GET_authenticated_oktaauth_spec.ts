import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Ben } from "shared/users";
import { mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties } from "./schemas/authenticatedResponse";

/** Test users cannot be authenticated in Okta Prod at this time, but
 * post-cutover, this test should be updated and made runnable in Prod.
*/

//Test Data
const sharedRequest = {
  urls: ["/api/authenticated", "/api/v1.0.0/authenticated"],
  options: {
    method: "GET",
    failOnStatusCode: false
  }
}
const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Okta Authorization token",
      request: {},
      setup() {
        return authorizeWithOkta(Ben.email, Ben.password as string, this.request).then(() => this);
      },
      response: {
        status: 401,
        bodyIsEmpty: true
      },
      preferredResponse:
      {
        status: 200,
        schemas: [mpAuthenticatedBasicAuthContract, mpAuthenticatedSchemaProperties]
      },
    }
  ]
}

describe("/Login/isAuthenticated()", () => {
  unzipScenarios(unauthorizedScenarios).forEach(runTest);
});