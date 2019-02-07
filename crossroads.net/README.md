#Crossroads.net Clientside Application

The client facing website for crossroads church. 

###Getting Started 
The first thing you'll need to get started is NodeJS. Head over to [http://nodejs.org/](http://nodejs.org) and install based on your operating system. Once you pull down the code, just run `npm i` to install all dependencies locally.

#### Core Functionality
If you are developing a module that can be considered a core module then the files should be stored under ./core folder

This is the core angular module for the Crossroads.net site. It includes multiple dependencies used throughout the crossroads ecosystem including:
* Angular
* Angular Resource
* Angular Cookies
* Angular Messages
* Angular Sanitize
* Angular Stripe
* Ui-Router *may be removed in the future*
* Ui-Bootstrap
* [UI Event](http://htmlpreview.github.io/?https://github.com/angular-ui/ui-event/master/demo/index.html)
* [Angular Growl 2](https://github.com/JanStevens/angular-growl-2)
* [Angular Payments](https://github.com/laurihy/angular-payments) *may be removed in the future*
* [Angular Stripe](https://github.com/bendrucker/angular-stripe)  *may be removed in the future*
* [Angular Toggle Switch](http://cgarvis.github.io/angular-toggle-switch/) *may be removed in the future*
* lodash
* moment

#### Builds

We use gulp scripts to build and run webpack so you will also need to install gulp globally.  This is not required, but makes command-line tasks easier later on.  To install gulp globally, use one of the following commands.  Both will install gulp into the NodeJS path, which is presumably already on your OS's execution PATH.

For Windows users (replace the prefix value below with the path to your NodeJS install): 
Ensure you are running the command prompt as administrator
``` npm set prefix "C:\Program Files\nodejs" ```

``` npm install -g gulp ```

Mac and Linux (replace the prefix value below with the path to your NodeJS install):
``` npm set prefix /usr/local/nodejs ```
``` npm install -g gulp ```

###Configuration
By default webpack inserts `http://localhost:49380` everywhere it finds `__GATEWAY_CLIENT_ENDPOINT__` in the javascript. This can be changed by creating and setting an environment variable called **CRDS_GATEWAY_CLIENT_ENDPOINT**. 
By default webpack inserts `http://content.crossroads.net` everywhere it finds `__CMS_CLIENT_ENDPOINT__` in the javascript. This can be changed by creating and setting an environment variable called **CRDS_CMS_CLIENT_ENDPOINT**. 
By default webpack inserts Crossroads Stripe Publishable Key as `pk_test_TR1GulD113hGh2RgoLhFqO0M` everywhere it find `__STRIPE_PUBKEY__` in the javascript. This can be changed by creating and setting an environment variable called **CRDS_STRIPE_PUBKEY**.
By default webpack defaults to use the API Version of Stripe as configured in the account. This can be overridden by creating an environment variable named CRDS_STRIPE_API_VERSION and setting to a particular verion. (i.e. 2015-04-07) 

For windows users:
```
set CRDS_GATEWAY_CLIENT_ENDPOINT = https://path-to-api-host/
set CRDS_CMS_CLIENT_ENDPOINT = https://path-to-content-host/
set CRDS_STRIPE_PUBKEY = <obtain from Stripe site>
set CROSSROADS_API_TOKEN = <obtain from Ministry Platform Client API Keys>
```

Mac and Linux:
```
export CRDS_GATEWAY_CLIENT_ENDPOINT = https://path-to-api-host/
export CRDS_CMS_CLIENT_ENDPOINT = https://path-to-content-host/
export CRDS_STRIPE_PUBKEY = <obtain from Stripe site>
export CROSSROADS_API_TOKEN = <obtain from Ministry Platform Client API Keys>
export DEPLOY_PRIME_URL=//localhost:8080
```

**Keep in mind that this way of setting environment variables will not be persistent, windows users will have to add this variable in system settings and linux/mac users will have to set it in their .bashrc/.zshrc files for persistence.**

###Install Third Party Dependencies
To install all 3rd party dependencies run the following command in your crds-angular/crossroads.net folder. This will look at the package.json file and install all the dependencies that are configured. It may take several minutes

``` npm install ```

###Build
To just build the project, run one of the following gulp commands
* `gulp` - builds the development configuration, and starts the webpack webserver listening on http port 8080
* `gulp browser-sync-dev` - builds the development configuration, and starts a browser-sync webserver listening on port 3000. This is good for live reloads.
* `gulp build-dev` - builds the development configuration, and outputs files to the assets/ folder
* `gulp build` - builds the production configuration, and outputs files to the assets/folder

###Test
There are two types of tests available, Unit Tests and Functional Tests. 
#### Unit Tests
We use karma as our test runner and Jasmine to write the specs. You will need to install karma-cli globablly to run the tests. 

Unit tests are kept in the (specs)[./specs] folder.

Windows users can run:
``` npm set prefix "C:\Program Files\nodejs" ```

``` npm install -g karma-cli ```

Mac and Linux users can run:
``` npm install -g karma-cli ```


Once karma-cli is installed, just run the commands below which will open chrome and run the tests. Click the debug button to see the results. Refreshing this page will re-run the tests.

* For the main application run `karma start crossroads.conf.js`
* For the core folders run `karma start core.tests.conf.js`
 
###Run
To run the project, run `gulp start` and point your browser to `http://localhost:8080`. If you want live reload, use `http://localhost:8080/webpack-dev-server` but keep in mind that the angular inspector will not work correctly and routes will not show up correctly with live reload. 

##Mac OS with Gateway code running under VirtualBox Windows Guest
Follow these instructions in order to setup the application to call Gateway services that are hosted on IIS Express on a Windows VM running under VirtualBox on a Mac.

###Configuring the corkboard application
~~Clone the crds-corkboard github repo to your local machine~~

##Create the symbolic link (Windows)
1. ~~Open a command prompt with administrator access~~
2. ~~Change to the `PATH_TO_CRDS_ANGULAR_REPO/crossroads.net` folder~~
3. ~~Run the following command to create a symbolic link `mklink /D corkboard PATH_TO_CRDS_CORKBOARD_REPO/website`~~

##Start the webpack processes
1. Run gulp in the `PATH_TO_CRDS_ANGULAR_REPO/crossroads.net` folder
2. ~~Run gulp in the `PATH_TO_CRDS_CORKBOARD_REPO/website` folder~~

###Create a VirtualBox Host-Only Network
1. Open VirtualBox
2. Navigate to **VirtualBox > Preferences...**
3. Click on **Network**
4. Click on **Host-Only Networks**
5. Click on the little network card with a plus sign on the right of the list
6. This will add an entry in the list called `vboxnet0`
7. Click **OK**

###Add a second adapter to VM
1. From VM VirtualBox Manager, with VM powered off, select VM and click **Settings**
2. Click **Network**
3. Click **Adapter 2**
4. Check **Enable Network Adapter**
5. Under **Attached to:** select `Host-only Adapter`
6. Under **Name** select `vboxnet0`
7. Click **OK**

###Start Windows VM and configure IIS Express
1. Open File Explorer and goto C:\Users\ *username*\Documents\IISExpress\config
2. Edit `applicationhost.config`
3. Find a `<site>` entry under `<sites>` that matches the name of the Visual Studio Project (e.g. `crds-angular`)
4. Under the `<bindings>` entry add `<binding protocol="http" bindingInformation="*:`[port number of IIS entry point, most likely `49380` ]`:`[name of VM]`"/>`, e.g. `<binding protocol="http" bindingInformation="*:49380:silbervm"/>`
5. Save file and start Visual Studio
6. Load and run the solution
7. Look in the task bar for the IIS Express icon and right click it
8. Look for an entry under **View Sites** that matches the name of the `<site>` key above and click on it.
9. You should see the binding you added represented as a url, e.g. `http://silbervm:49380/` under **Browse Applications**

###Configure OS X to use network
1. Edit `/etc/hosts` as *root*
2. On the Windows VM, **Open Network and Sharing Center** and click on **Local Area Connection 2**
3. Click **Details...**
4. Note the `IPv4 Address`, most likely `192.168.56.101`
5. Back on OS X, create a entry in `/etc/hosts` using the VM name as the DNS name, e.g. `192.168.56.101  silbervm`
6. As described above, add the **CRDS_GATEWAY_CLIENT_ENDPOINT** environment variable to match the configuration, e.g. `export  CRDS_GATEWAY_CLIENT_ENDPOINT=http://silbervm:49380/`

##Folder Naming Convention
1. Use descriptive folder names
2. Seperate multiple words with underscords (i.e. 'sign_up_to_serve')

##Angular Style Guide
We will follow the [Crossroads Angular Style Guide](https://github.com/crdschurch/angular-styleguide).

##Linting
### JSHint
First install jshint globally with npm `npm install -g jshint`
A .jshintrc file has already been added to the project
* For Atom, install [linter-jshint](https://github.com/AtomLinter/linter-jshint)
* For Sublime Text, install [Sublime-jshint](https://github.com/victorporof/Sublime-JSHint)
* For IntelliJ, supported out of the box

### JSCS
Install jscs with npm `npm install jscs -g`
A .jscsrc file exists to use AirBnB styleguide
* For Atom. [linter-jscs](https://atom.io/packages/linter-jscs)
* For Sublime Text 3, [SublimeLinter](https://github.com/SublimeLinter/SublimeLinter-jscs/)
* For IntelliJ, [jscs-plugin](https://github.com/idok/jscs-plugin)

### ESLint
A .eslintrc file has already been added to the project
* For IntelliJ - Navigate to **File | Settings | Languages and Frameworks | JavaScript | Code Quality Tools | ESLint** and check the Enable box
