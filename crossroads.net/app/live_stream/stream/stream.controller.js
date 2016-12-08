let WOW = require('wow.js/dist/wow.min.js');
let iFrameResizer = require('iframe-resizer/js/iframeResizer.min.js');
var $ = require('jquery');

export default class StreamingController {
  /*@ngInject*/
  constructor(CMSService, StreamspotService, GeolocationService, $rootScope, $modal, $location, $timeout, $sce) {
    this.cmsService         = CMSService;
    this.streamspotService  = StreamspotService;
    this.geolocationService = GeolocationService;
    this.rootScope          = $rootScope;
    this.timeout            = $timeout;
    this.modal              = $modal;
    this.inProgress     = false;
    this.numberOfPeople = 2;
    this.displayCounter = true;
    this.countSubmit    = false;
    this.dontMiss       = [];
    this.beTheChurch    = [];
    this.sce = $sce;
    let debug = false;

    if ( $location != undefined ) {
      let params = $location.search();
      debug = params.debug;
    }

    if ( debug === "true" ) {
      this.inProgress = true;
    } else {
      this.rootScope.$on('isBroadcasting', (e, inProgress) => {
        this.inProgress = inProgress;
        if (this.inProgress === false) {
          window.location.href = '/live';
        }
      });
    }

    this.cmsService
        .getDigitalProgram()
        .then((data) => {
          this.sortDigitalProgram(data);
        });

    new WOW({
      mobile: false
    }).init();

    this.openGeolocationModal();

    switch (__CRDS_ENV__) {
      case 'int':
        this.baseUrl = 'https://embedint.crossroads.net';
        break;
      case 'demo':
        this.baseUrl = 'https://embeddemo.crossroads.net';
        break;
      default:
        this.baseUrl = 'https://embed.crossroads.net';
        break;
    }

    this.timeout(this.resizeIframe.bind(this), 500);
  }

  resizeIframe() {
    var el = document.querySelector('.digital-program__giving iframe');
        el.removeAttribute('height');

    iFrameResizer({
      heightCalculationMethod: 'taggedElement',
      minHeight: 350,
      checkOrigin: false,
      interval: -16
    }, el);
  }

  buildUrl() {
    var params = this.queryStringParams || 'type=donation&theme=dark';
    return this.sce.trustAsResourceUrl(`${this.baseUrl}?${params}`);
  }

  sortDigitalProgram(data) {
    data.forEach((feature, i, data) => {
      // null status indicates a published feature
      if (feature.status === null || feature.status.toLowerCase() !== 'draft') {
        feature.delay = i * 100
        feature.url = 'javascript:;';

        if (feature.link !== null) {
          feature.url = feature.link;
        }

        feature.target = '_blank';

        if (typeof feature.image !== 'undefined' && typeof feature.image.filename !== 'undefined') {
          feature.image = feature.image.filename;
        } else {
          feature.image = 'https://crds-cms-uploads.imgix.net/content/images/register-bg.jpg'
        }
        if (feature.section === 1 ) {
          this.dontMiss.push(feature)
        } else if (feature.section === 2 ) {
          this.beTheChurch.push(feature);
        }
      }
    })
  }

  showGeolocationBanner() {
    return this.geolocationService.showBanner();
  }

  openGeolocationModal() {
    if (this.geolocationService.showModal()) {
      this.modalInstance = this.modal.open({
        templateUrl: 'geolocation_modal/geolocationModal.html',
        controller: 'GeolocationModalController',
        controllerAs: 'geolocationModal',
        openedClass: 'geolocation-modal',
        backdrop: 'static',
        size: 'lg'
      });
    }
  }
}