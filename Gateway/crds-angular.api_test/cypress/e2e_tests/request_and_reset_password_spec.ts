import { getTestPassword } from "shared/data_generator";
import { getMPUser } from "shared/mp_api";
import { Gatekeeper } from "shared/users";

describe('Password reset workflow', () => {
  it('Requests a password reset and resets password', () => {
    const testerEmail = Gatekeeper.email;

    //Request password reset
    const requestPasswordReset: Partial<Cypress.RequestOptions> = {
      url: "/api/requestpasswordreset",
      method: "POST",
      body: { email: testerEmail }
    };

    cy.request(requestPasswordReset)
      .then(() => getMPUser(testerEmail))
      .then((user) => {
        const oldPw = user.Password;
        const resetToken = user.PasswordResetToken;
        expect(resetToken).to.be.a('string').and.not.be.null;

        //Reset password
        const resetPassword: Partial<Cypress.RequestOptions> = {
          url: "/api/resetpassword",
          method: "POST",
          body: { password: getTestPassword(), token: resetToken }
        };

        cy.request(resetPassword)
          .then(() => getMPUser(testerEmail))
          .then((updatedUser) => {
            expect(updatedUser.PasswordResetToken).to.be.null;
            expect(updatedUser.Password).to.not.eq(oldPw);
          })
      });
  });
});