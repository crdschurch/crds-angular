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

const loadConfig = require('crds-cypress-config');

function setTestFilesConfig(config){
  const cypressDefaultTestFiles = "**/*.*";
  config.testFiles = config.testFiles === cypressDefaultTestFiles ? "**/*spec.ts" : config.testFiles;
  console.log(`Loading testFiles matching ${config.testFiles}`); //Sanity check
}

/**
 * @type {Cypress.PluginConfig}
 */
module.exports = (on, config) => {
  // Set our own default for testFile extension unless configured in config file
  setTestFilesConfig(config);

  return loadConfig.loadConfigFromVault(config);
}
