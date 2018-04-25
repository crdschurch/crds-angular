/* eslint no-var: 0 */
module.exports = {
  get() {
    return {
      __IN_MAESTRO__: JSON.stringify(process.env.IN_MAESTRO || '1'),
      __CRDS_ENV__: JSON.stringify(process.env.CRDS_ENV || ''),
      __COOKIE_DOMAIN__: JSON.stringify(process.env.CRDS_COOKIE_DOMAIN || ''),
      __CROSSROADS_API_TOKEN__: JSON.stringify(process.env.CROSSROADS_API_TOKEN || ''),
      __APP_SERVER_ENDPOINT__: JSON.stringify(process.env.CRDS_APP_SERVER_ENDPOINT || 'https://int.crossroads.net/'),
      __GATEWAY_CLIENT_ENDPOINT__: JSON.stringify(process.env.CRDS_GATEWAY_CLIENT_ENDPOINT || 'https://gatewayint.crossroads.net/gateway/'),
      __CMS_CLIENT_ENDPOINT__: JSON.stringify(process.env.CRDS_CMS_CLIENT_ENDPOINT || 'https://contentint.crossroads.net/'),
      __SERVICES_CLIENT_ENDPOINT__: JSON.stringify(process.env.CRDS_SERVICES_CLIENT_ENDPOINT || 'https://gatewayint.crossroads.net/'),
      __GOOGLE_API_KEY__: JSON.stringify(process.env.CRDS_GOOGLE_API_KEY || 'AIzaSyArKsBK97N0Wi-69x10OL7Sx57Fwlmu6Cs'),
      __STRIPE_PUBKEY__: JSON.stringify(process.env.CRDS_STRIPE_PUBKEY || 'pk_test_U8U15gSZFM4AQtPDLHYnKWqH'),
      __STRIPE_API_VERSION__: JSON.stringify(process.env.CRDS_STRIPE_API_VERSION),
      __SOUNDCLOUD_API_KEY__: JSON.stringify(process.env.CRDS_SOUNDCLOUD_KEY || '67723f3ff9ea6bda29331ac06ce2960c'),
      __AWS_SEARCH_ENDPOINT__: JSON.stringify(process.env.CRDS_AWS_SEARCH_ENDPOINT || 'https://vs9gac5tz7.execute-api.us-east-1.amazonaws.com/prod/'),
      __STREAMSPOT_ENDPOINT__: JSON.stringify(process.env.CRDS_STREAMSPOT_ENDPOINT || 'https://api.streamspot.com/'),
      __STREAMSPOT_API_KEY__: JSON.stringify(process.env.CRDS_STREAMSPOT_API_KEY || '82437b4d-4e38-42e2-83b6-148fcfaf36fb'),
      __STREAMSPOT_SSID__: JSON.stringify(process.env.CRDS_STREAMSPOT_SSID || 'crossr4915'),
      __STREAMSPOT_PLAYER_ID__: JSON.stringify(process.env.CRDS_STREAMSPOT_PLAYER_ID || '1adb55de'),
      __IMG_ENDPOINT__: JSON.stringify((process.env.CRDS_GATEWAY_CLIENT_ENDPOINT || 'https://gatewayint.crossroads.net/gateway/') + 'api/image/profile/'),
      __HEADER_CONTENTBLOCK_TITLE__: JSON.stringify(process.env.HEADER_CONTENTBLOCK_TITLE || 'sharedGlobalHeader'),
      __APP_CLIENT_ENDPOINT__: JSON.stringify(process.env.CRDS_APP_CLIENT_ENDPOINT || '/')
    };
  },
  getTest() {
    var params = this.get();
    /* eslint-disable no-underscore-dangle */
    params.__CRDS_ENV__ = JSON.stringify('');
    params.__COOKIE_DOMAIN__ = JSON.stringify('');
    params.__CROSSROADS_API_TOKEN__ = JSON.stringify('crds_api_token_value');
    /* eslint-enable */
    return params;
  }
};
