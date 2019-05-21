/* eslint-disable import/no-extraneous-dependencies */
const webpack = require('webpack');
const path = require('path');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const KarmaWebpackPlugin = require('karma-webpack');
const KarmaJasminePlugin = require('karma-jasmine');
const KarmaMochaReporterPlugin = require('karma-mocha-reporter');
const KarmaPhantomjsLauncherPlugin = require('karma-phantomjs-launcher');
const KarmaEnvPreprocessorPlugin = require('karma-env-preprocessor');
const KarmaSourcemapLoaderPlugin = require('karma-sourcemap-loader');
const KarmaPhantomJSShim = require('karma-phantomjs-shim');
/* eslint-enable */


/**
 * FIXME: why are we using `path.resolve` and `__dirname` here? why not just `./`?
 * --Jonathan Horner
 */
/* eslint-disable import/no-dynamic-require */
const environmentVars = require(path.resolve(__dirname, 'environment.config.js'));
/* eslint-enable */

const definePlugin = new webpack.DefinePlugin(environmentVars.getTest());

module.exports = function crdsTermConf(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: './',

    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine', 'phantomjs-shim'],

    // list of files / patterns to load in the browser
    files: [
      'node_modules/es6-shim/es6-shim.js',
      { pattern: 'node_modules/babel-polyfill/browser.js', instrument: false },
      'https://js.stripe.com/v2/',
      './node_modules/phantomjs-polyfill/bind-polyfill.js',
      { pattern: 'spec/spec_index.js', watched: false },
    ],

    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    preprocessors: {
      'spec/spec_index.js': ['webpack', 'env', 'sourcemap']
    },

    envPreprocessor: [
      'CRDS_GATEWAY_CLIENT_ENDPOINT',
      'CRDS_CMS_CLIENT_ENDPOINT',
    ],

    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    reporters: ['mocha'],


    mochaReporter: {
      ignoreSkipped: true
    },

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
              path.resolve(__dirname, 'core'),
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
          },
          {
            test: /\.json$/,
            loaders: ['json-loader']
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

    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['PhantomJS'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: false,

    // Plugins
    plugins: [
      KarmaWebpackPlugin,
      KarmaJasminePlugin,
      KarmaMochaReporterPlugin,
      KarmaPhantomjsLauncherPlugin,
      KarmaEnvPreprocessorPlugin,
      KarmaSourcemapLoaderPlugin,
      KarmaPhantomJSShim
    ]
  });
};
