///

export function getToken(email: string, password: string): Cypress.Chainable<string> {
  const tokenRequest: Partial<Cypress.RequestOptions> = {
    method: 'POST',
    url: `${Cypress.env('OKTA_OAUTH_BASE_URL')}/v1/token`,
    headers: { authorization: Cypress.env('OKTA_TOKEN_AUTH') },
    form: true,
    body: {
      grant_type: 'password',
      username: email,
      password,
      scope: 'openid'
    }
  };

  return cy.request(tokenRequest).its('body.access_token');
}