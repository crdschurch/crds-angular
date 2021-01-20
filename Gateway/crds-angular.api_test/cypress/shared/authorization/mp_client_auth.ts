// Tokens not stored globally, so will be shared within a test file but not across multiple test files.
let storedToken: string;

/**
 * Gets an MP Authorization token for the CRDS.Common client app
 */
function getNewToken(): Cypress.Chainable<string> {
  const tokenRequest: Partial<Cypress.RequestOptions> = {
    url: `${Cypress.env('MP_REST_API_ENDPOINT')}/oauth/connect/token`,
    method: "POST",
    form: true,
    body: {
      client_id: Cypress.env('CRDS_MP_TESTAUTOMATION_CLIENT_ID'),
      client_secret: Cypress.env('CRDS_MP_TESTAUTOMATION_CLIENT_SECRET'),
      grant_type: 'client_credentials',
      scope: 'http://www.thinkministry.com/dataplatform/scopes/all'
    }
  };

  return cy.request(tokenRequest).its('body.access_token')
}

/**
 * Gets an MP Authorization token for the CRDS.Common client app. Will return existing token if one has been fetched already.
 */
function getToken(): Cypress.Chainable<string> {
  if(storedToken === undefined){
    return getNewToken().then(token => storedToken = token)
  }
  
  return cy.wrap(storedToken);
}


/**
 * Adds MP Bearer token authorized by CRDS.Common client app to Cypress request 
 * @param request 
 */
export function addAuthorizationHeader(request: Partial<Cypress.RequestOptions>): Cypress.Chainable<Partial<Cypress.RequestOptions>>{
  return getToken()
  .then(token => {
    request.auth = {bearer: token};
    return request;
  });
}