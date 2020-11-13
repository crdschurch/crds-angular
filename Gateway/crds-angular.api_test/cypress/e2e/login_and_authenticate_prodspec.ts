import { Ben } from "shared/users";

describe('Login and check authentication workflow', () => {
  it('Logs a user in and verifies token can be authenticated', () => {
    const username = Ben.email;
    const password = Ben.password as string;
    const mpLoginRequest: Partial<Cypress.RequestOptions> = {
      url: "/api/login",
      method: "POST",
      body: { username, password }
    };

    cy.request(mpLoginRequest)
      .its('body.userToken')
      .then((token) => {
        const authenticatedRequest: Partial<Cypress.RequestOptions> = {
          url: "/api/authenticated",
          method: "GET",
          headers: { Authorization: token }
        };

        cy.request(authenticatedRequest)
          .its('status').should('eq', 200)
      });
  });
});