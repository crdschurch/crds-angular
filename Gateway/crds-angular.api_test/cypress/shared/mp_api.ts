// Access to MP through their API

let mpToken: string;

function getNewToken(): Cypress.Chainable<string> {
  const tokenRequest: Partial<Cypress.RequestOptions> = {
    url: `${Cypress.env('MP_REST_API_ENDPOINT')}/oauth/connect/token`,
    method: "POST",
    form: true,
    body: {
      client_id: Cypress.env('CRDS_MP_COMMON_CLIENT_ID'),
      client_secret: Cypress.env('CRDS_MP_COMMON_CLIENT_SECRET'),
      grant_type: 'client_credentials',
      scope: 'http://www.thinkministry.com/dataplatform/scopes/all'
    }
  };

  return cy.request(tokenRequest).its('body.access_token')
}

//Gets existing or new token. 
// Tokens not stored globally, so will be shared within a test file but not across multiple test files.
function getToken(): Cypress.Chainable<string> {
  if(mpToken === undefined){
    console.debug('fetching new mp token');
    return getNewToken().then(token => mpToken = token)
  }
  
  console.debug('fetching stored mp token');
  return cy.wrap(mpToken);
}

// Adds MP Bearer token to request
function authorize(request: Partial<Cypress.RequestOptions>){
  return getToken()
  .then(token => {
    request.auth = {bearer: token};
    return request;
  });
}

//The properties returned depend on the filter
export interface MPUser {
  User_Name: string,
  User_ID: number,
  Password: string,
  PasswordResetToken: string
}

export function getMPUser(email: string): Cypress.Chainable<MPUser> {
  const userIdRequest: Partial<Cypress.RequestOptions> = {
    url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/dp_Users`,
    method: "GET",
    qs: {
      '$filter': `User_Name='${email}'`
    }
  };

  return authorize(userIdRequest) 
  .then(cy.request)
  .its('body').then(body => body[0]);
}

// Returns user after update
export function setPasswordResetToken(email: string, resetToken: string): Cypress.Chainable<MPUser>{
  return getMPUser(email)
  .then(mpUser => {
    const updateResetTokenRequest: Partial<Cypress.RequestOptions> = {
      url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/dp_Users`,
      method: "PUT",
      body: [{User_ID: mpUser.User_ID, PasswordResetToken: resetToken}]
    };

    return authorize(updateResetTokenRequest)
    .then(cy.request)
    .its('body').then(body => body[0])
  });  
}