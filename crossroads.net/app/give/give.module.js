'use strict';

var constants = require('../constants');

require('./templates/give.html');
require('./templates/amount.html');
require('./templates/register.html');
require('./templates/mockup-give-commitment.html');

var app = angular.module(constants.MODULES.GIVE, [
    constants.MODULES.CORE,
    constants.MODULES.COMMON
])
.constant('GIVE_PROGRAM_TYPES', { Fuel: 1, Events: 2, Trips: 3, NonFinancial: 4 })
.constant('GIVE_ROLES', { StewardshipDonationProcessor: 7 })
;

app.controller('GiveController', require('./give.controller'));

require('./one_time');
require('./recurring');
