/// <reference types="cypress" />
// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)

import { loadConfigFromVault } from 'crds-cypress-config';

/**
 * Configure custom extension for test files unless one is provided elsewhere (config file, command line argument, etc.)
 * @param config Cypress config
 */
function setTestFilesConfig(config: Cypress.PluginConfigOptions){
  const cypressDefaultTestFiles = "**/*.*";
  config.testFiles = config.testFiles === cypressDefaultTestFiles ? "**/*spec.ts" : config.testFiles;
  console.log(`Loading testFiles matching ${config.testFiles}`); //Sanity check
}

module.exports = (on: Cypress.PluginEvents, config: Cypress.PluginConfigOptions): Cypress.PluginConfig => {
  // Don't record video since this is an API only suite
  config.video = false;

  // Set our own default for testFile extension unless configured in config file
  setTestFilesConfig(config);

  // return loadConfig.loadConfigFromVault(config);
  return loadConfigFromVault(config);
}

//TODO keep configuring eslint - see if 2 custom tslinter extensions are compatible