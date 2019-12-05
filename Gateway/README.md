#Crossroads.net Serverside Application

The web services supporting the website for crossroads connecting to Ministry Platform.

<a width="150" height="50" href="https://auth0.com/?utm_source=oss&utm_medium=gp&utm_campaign=oss" target="_blank" alt="Single Sign On & Token Based Authentication - Auth0"><img width="150" height="50" alt="JWT Auth for open source projects" src="https://cdn.auth0.com/oss/badges/a0-badge-light.png"/></a>
###Getting Started

Prerequisites: A Windows machine, or Windows VM

1. You will need to acquire a copy of Visual Studio and license. This will vary depending on your affiliation to the project. You can download a 90 day trial of Visual Studio to begin with. A copy of Visual Studio 2013 Professional is available via Flash Drive.
2. Install Visual Studio. Instructions on how to install Visual Studio.
3. Install Node.js: http://nodejs.org/ .
4. Setup local environment variables.  For staff see the Developer Setup document in the Project Onboarding folder.
  * Environment Variables on Windows
    * Control Panel -> System and Security -> System
	* Advanced System Settings
	* Environment Variables
    -IDENTITY_SERVICE_URL
    -AMAZON_API_KEY
    -AMAZON_API_SECRET 
    -AMAZON_DOC_ENDPOINT 
    -APP_LOG_ROOT 
    -ASTRONOMER_APPLICATION_ID
    -AUTH_SERVICE_BASE_URL 
    -BULK_EMAIL_API_KEY 
    -CRDS_CMS_SERVER_ENDPOINT 
    -CRDS_MP_COMMON_CLIENT_ID 
    -CRDS_MP_COMMON_CLIENT_SECRET 
    -EZSCAN_DB_CONN_STRING 
    -EZSCAN_DB_SECRET_KEY 
    -GOOGLE_API_SECRET_KEY 
    -MP_OAUTH_BASE_URL 
    -MP_REST_API_ENDPOINT 
    -OKTA_MIGRATION_AZURE_API_KEY 
    -OKTA_MIGRATION_BASE_URL 
    -STRIPE_AUTH_TOKEN


##.NET Style
###Resharper Configuration (Provided by https://www.jetbrains.com)
The team level configuration file is checked into git alongside the .NET solution file, and is named **Crossroads.Resharper.Team.DotSettings**
The initial version of this file is concentrated on providing a default code formatting and cleanup template.
To format and cleanup code for the current file using the following steps
* Press Ctrl+E,F for both VS and InteliJ keyboard layouts to run the silent cleanup which will utilize the **Crossroads - Reformat and Remove References** cleanup options
* Alternatively you can press Ctrl+E,C for VS keyboard layout or Ctrl+Alt+F for InteliJ keyboard layout, and then select a specific cleanup option by name

These settings will probably be tweaked over time. When modifying the settings for code styles and formatting please set the values on the team configuration file **Crossroads.Resharper.Team.DotSettings**
and check into source control. The easiest way to modify the team configuration file is to go to the **Resharper -> Manage Options** menu item and click on the Edit Layer Wrench icon on the **Crossroads.Resharper.Team** entry.
