export interface TestUser {
  email: string;
  password?: string;
}

/**
 * Ben is a test user with the same MP contact ID in every environment.
 * It is safe to authenticate with either MP or Okta, and will be safe post-cutover.
 * DO NOT change his email, password or anything that may prevent him from logging in,
 *   even temporarily. His account is used by many automated tests.
 */
export const Ben: TestUser = {
  email: "mpcrds+auto+2@gmail.com",
  password: Cypress.env("BEN_KENOBI_PW")
};


/**
 * Gate is a test user in non-prod environments.
 * They cannot be authenticated through Okta in Demo.
 */
export const Gatekeeper: TestUser = {
  email: "mpcrds+auto+gatekeeper@testmail.com",
  password: Cypress.env("TEST_GATEKEEPER_PW")
};


/**
 * Jr is a test user in non-prod environments.
 * They cannot be authenticated through Okta in Demo.
 * They have an email address that is a subset of another user's email address.
 */
export const KeeperJr: TestUser = {
  email: "auto+gatekeeper@testmail.com",
  password: Cypress.env("TEST_GATEKEEPERJR_PW")
};


/**
 * Load has a MP Contact record only in all environments.
 * They cannot be authenticated.
 */
export const Load: TestUser = {
  email: "mpcrds+LoadTest_98@gmail.com",
  password: undefined
};