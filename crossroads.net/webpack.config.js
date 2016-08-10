var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var path = require('path');
var AssetsPlugin = require('assets-webpack-plugin');
var assetsPluginInstance = new AssetsPlugin();

var endpoint = {
  url: 'http://localhost:49380'
};

var environmentVars = require(path.resolve(__dirname, 'environment.config.js'));
var definePlugin = new webpack.DefinePlugin(environmentVars.get());

module.exports = {
  entry: {
    childcare: './app/childcare_dashboard/childcareDashboard.module.js',
    trips: './app/trips/trips.module.js',
    search: './app/search/search.module.js',
    media: './app/media/media.module.js',
    give: './app/give/give.module.js',
    profile: './app/profile/profile.module.js',
    main: './app/app.js',
    ang: './core/ang.js',
    govolunteer: './app/go_volunteer/goVolunteer.module.js',
    core: ['./core/core.js'],
    common: ['./app/common/common.module.js'],
    formbuilder: ['./app/formBuilder/formBuilder.module.js'],
    formlybuilder: ['./app/formlyBuilder/formlyBuilder.module.js'],
    boot: ['./app/boot.ts']
  },
  resolve: {
    extensions: ['', '.ts', '.tsx', '.js', '.jsx']
  },
  watchPattern: ['app/**/**','core/**/**'],
  externals: {
    stripe: 'Stripe',
    moment: 'moment'
  },
  noParse: [
      /node_modules[\/]video\.js[\/]/,
      /node_modules[\/]videojs-contrib-hls[\/]/,
      /node_modules[\/]moment[\/]/
  ],
  context: __dirname,
  output: {
    path: './assets',
    publicPath: '/assets/',
    filename: '[name].[hash].js',
  },
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
                path.resolve(__dirname, 'node_modules/angular-stripe')
              ],
              exclude: [/streamspotAnalytics\.js$/],
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
              loader: 'ng-cache?prefix=[dir]',
              exclude: [/\.ng2component\.html$/]
            },
            {
              test: /\.ts$/,
              loaders: ['awesome-typescript-loader', 'angular2-template-loader'],
              exclude: [/\.(spec|e2e)\.ts$/]
            },
            {
              test: /\.json$/,
              loaders: ["json-loader"]
            },
            {
              test: /\.ng2component\.html$/,
              loader: 'raw-loader'
            }
    ]
  },
  plugins: [
    new ExtractTextPlugin('[name].[hash].css'),
    definePlugin,
    assetsPluginInstance,
    new webpack.optimize.UglifyJsPlugin({
      sourceMap: false,
      mangle: false,
      output: { ascii_only: true }
    })
  ]
};
