import angular from 'angular';
import mocks from 'angular-mocks';
import moment from 'moment-timezone';
import ang from '../core/ang';
import core from '../core/core';

// Other modules needed to instantiate crossroads module not referenced in other tests already
import search from '../app/search/search.module';
import formly from '../app/formlyBuilder/formlyBuilder.module';
import invoices from '../app/invoices/invoices.module';
import formBuilder from '../app/formBuilder/formBuilder.module'
import goVolunteer from '../app/go_volunteer/goVolunteer.module';
import childcareModule from '../app/childcare_dashboard/childcareDashboard.module';
import commonModule from '../app/common/common.module';
import profileModule from '../app/profile/profile.module';
import searchModule from '../app/search/search.module';
import tripsModule from '../app/trips/trips.module';
import waiversModule from '../app/waivers/waivers.module';
import app from '../app/app';

var testsContext = require.context('./', true, /.spec$/);
testsContext.keys().forEach(testsContext);
