const iFrameResizer = require('iframe-resizer/js/iframeResizer.min.js'); // eslint-disable-line no-unused-vars

/**
 * This directive helps you implement a resizeable iframe
 * for any routes within the `crds-embed` repo. The src attribute
 * on the iframe will reflect the current environment (int, demo, etc.)
 * automatically.
 *
 * To use, simply drop the following element in your markup (or content-block)
 * and define the relative path for the route you want passed to the iframe.
 *
 * For example...
 *
 *    <crds-embed path="/?type=donation&theme=dark"></crds-embed>
 *
 */

export default class EmbedController {
  constructor($element, $sce, $attrs) {
    this.element = $element;
    this.sce = $sce;
    this.attrs = $attrs;

    // iframe attributes
    this.frameborder = $attrs.frameborder || 0;
    this.scrolling = $attrs.scrolling || 'no';
    this.width = $attrs.width || '100%';
    this.class = $attrs.class || undefined;
    this.iFrames = undefined;

    // iframe auto-resize attributes
    this.autoResize = $attrs.resize !== undefined ? Boolean($attrs.resize).valueOf() : true; // Default to auto-resizing, make template explicitly turn it off if desired
    this.minHeight = $attrs.minHeight || 350;
    this.resizeInterval = $attrs.resizeInterval || 32;
  }

  $onInit() {
    // TODO Defaulting to /give if the 'href' attribute is not specified - maybe this should be an error instead
    this.defaultPath = '/give/?type=donation';

    switch (__CRDS_ENV__) { // eslint-disable-line no-undef
      case 'int':
        this.baseUrl = 'https://embedint.crossroads.net';
        break;
      case 'demo':
        this.baseUrl = 'https://embeddemo.crossroads.net';
        break;
      case 'local':
        this.baseUrl = 'http://local.crossroads.net:8080/give';
        break;
      default:
        this.baseUrl = 'https://embed.crossroads.net';
        break;
    }

    this.makeResizeable();
  }

  $onDestroy() {
    this.closeIframes();
  }

  buildUrl() {
    const path = this.attrs.href || this.defaultPath;

    return this.sce.trustAsResourceUrl(`${this.baseUrl}${path}`);
  }

  closeIframes() {
    if (this.iFrames) {
      this.iFrames.forEach((frame) => {
        if (frame.iFrameResizer !== undefined) {
          frame.iFrameResizer.close();
        }
      });
    }
  }

  makeResizeable() {
    // Don't auto-resize the iframe if the template didn't ask for it
    if (!this.autoResize) {
      return;
    }

    const el = angular.element(this.element).find('iframe')[0];
    this.iFrames = iFrameResizer({
      heightCalculationMethod: 'taggedElement',
      minHeight: this.minHeight,
      checkOrigin: false,
      interval: this.resizeInterval
    }, el);
  }

}
