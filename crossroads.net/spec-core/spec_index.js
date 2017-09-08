require('../core/core');
require('../core/ang');
require('../app/common/common.module');

// require all modules ending in ".spec" from the
// current directory and all subdirectories

var testsContext = require.context('./', true, /.spec$/);
testsContext.keys().forEach(testsContext);