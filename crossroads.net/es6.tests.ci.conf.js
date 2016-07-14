var webpack = require('webpack');
var path = require('path');
var ExtractTextPlugin = require('extract-text-webpack-plugin');

var definePlugin = new webpack.DefinePlugin({
  __API_ENDPOINT__: JSON.stringify(process.env.CRDS_API_ENDPOINT || 'https://gatewayint.crossroads.net/gateway/'),
  __CMS_ENDPOINT__: JSON.stringify(process.env.CRDS_CMS_ENDPOINT || 'https://contentint.crossroads.net/'),
  __GOOGLE_API_KEY__: JSON.stringify(process.env.CRDS_GOOGLE_API_KEY || 'AIzaSyArKsBK97N0Wi-69x10OL7Sx57Fwlmu6Cs'),
  __STRIPE_PUBKEY__: JSON.stringify(process.env.CRDS_STRIPE_PUBKEY || 'pk_test_TR1GulD113hGh2RgoLhFqO0M'),
  __STRIPE_API_VERSION__: JSON.stringify(process.env.CRDS_STRIPE_API_VERSION),
  __SOUNDCLOUD_API_KEY__: JSON.stringify(process.env.CRDS_SOUNDCLOUD_KEY || '67723f3ff9ea6bda29331ac06ce2960c'),
  __AWS_SEARCH_ENDPOINT__:
    JSON.stringify(process.env.CRDS_AWS_SEARCH_ENDPOINT ||
      'https://vs9gac5tz7.execute-api.us-east-1.amazonaws.com/prod/')
});

module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: './',

    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine', 'es6-shim'],

    // list of files / patterns to load in the browser
    files: [
      'https://js.stripe.com/v2/',
      './node_modules/phantomjs-polyfill/bind-polyfill.js',
      { pattern: 'spec-es6/spec_index.js', watched: false },
    ],

    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    preprocessors: {
      'spec-es6/spec_index.js': ['webpack','env', 'sourcemap']
    },

    envPreprocessor: [
      'CRDS_API_ENDPOINT',
      'CRDS_CMS_ENDPOINT',
    ],

    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    reporters: ['teamcity'],

    // web server port
    port: 9876,

    webpack: {
      devtool: 'inline-source-map',
      module: {
        loaders: [
          {
            test: /\.css$/,
            loader: 'style-loader!css-loader'
          },
          {
            test: /\.js$/,
            include: [
              path.resolve(__dirname, 'app'),
              path.resolve(__dirname, './node_modules/angular-stripe'),
              path.resolve(__dirname, 'spec')
            ],
            loader: 'ng-annotate!babel-loader'
          },
          {
            test: /\.scss$/,
            loader: ExtractTextPlugin.extract('style-loader', 'css-loader!autoprefixer-loader!sass-loader')
          },
          {
            test: /\.(jpe?g|png|gif|svg)$/i,
            loaders: ['image?bypassOnDebug&optimizationLevel=7&interlaced=false']
          },
          {
            test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
            loader: 'url-loader?limit=10000&minetype=application/font-woff'
          },
          {
            test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
            loader: 'file-loader'
          },
          {
            test: /\.html$/,
            loader: 'ng-cache?prefix=[dir]'
          }
        ]
      },
      plugins: [new ExtractTextPlugin('[name].css'), definePlugin]
    },

    webpackServer: {
      noInfo: true // prevent console spamming when running in Karma!
    },

    webpackMiddleware: {
      stats: {
        colors: true
      }
    },

    // enable / disable colors in the output (reporters and logs)
    colors: true,

    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,

    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: true,

    browsers: ['PhantomJS'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true,

    // Plugins
    plugins: [
      require('karma-webpack'),
      require('karma-jasmine'),
      require('karma-teamcity-reporter'),
      require('karma-phantomjs-launcher'),
      require('karma-env-preprocessor'),
      require('karma-sourcemap-loader'),
      require('karma-es6-shim'),
    ]
  });
};
