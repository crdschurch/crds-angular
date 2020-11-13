// Access to MP through their API
import { authorize } from "./authorization/mp_client_auth";

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
  console.debug(`reset token is set to ${resetToken}`);
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