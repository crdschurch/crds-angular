import angular from 'angular';
import mocks from 'angular-mocks';
import moment from 'moment';
import ang from '../core/ang';
import core from '../core/core';

// Other modules needed to instantiate crossroads module not referenced in other tests already
import search from '../app/search/search.module';
import media from '../app/media/media.module';

var testsContext = require.context('./', true, /.spec$/);
testsContext.keys().forEach(testsContext);
