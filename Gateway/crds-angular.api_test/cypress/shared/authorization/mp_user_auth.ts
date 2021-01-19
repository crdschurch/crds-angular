/**
 * Gets an MP Authorization token for a user.
 * This should match the request used by AuthenticateUser in Crossroads.Web.Common.Security
 * @param email User's email
 * @param password User's password
 */
export function getToken(email: string, password: string): Cypress.Chainable<string> {
  const tokenRequest: Partial<Cypress.RequestOptions> = {
    url: `${Cypress.env('MP_REST_API_ENDPOINT')}/oauth/connect/token`,
    method: "POST",
    form: true,
    body: {
      client_id: Cypress.env('CRDS_MP_TESTAUTOMATION_CLIENT_ID'),
      client_secret: Cypress.env('CRDS_MP_TESTAUTOMATION_CLIENT_SECRET'),
      username: email,
      password,
      grant_type: 'password',
      scope: 'http://www.thinkministry.com/dataplatform/scopes/all openid' //matches Resource Owner (no refresh) scope
    }
  };

  return cy.request(tokenRequest).its('body.access_token');
}

/**
 * Adds MP Authorization token header authorized by the given user to Cypress request 
 * @param request 
 */
export function addAuthorizationHeader(email: string, password: string, request: Partial<Cypress.RequestOptions>): Cypress.Chainable<string>{
  return getToken(email, password)
  .then(token => {
    request.headers = {
      ...request.headers,
      Authorization: token
    }
  });
}