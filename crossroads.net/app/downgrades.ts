import { upgradeAdapter } from './upgrade-adapter';
import { Ng2TestComponent } from './ng2test/ng2test.component';
import { Ng2TestCMSDataComponent } from './ng2test/ng2testcmsdata.component';
import { StreamingComponent } from './streaming/streaming.component';
import { VideoJSComponent } from './streaming/videojs.component';
import { DynamicContentNg2Component } from '../core/dynamic_content/dynamic-content-ng2.component';
import { LinkedContentNg2Component } from '../core/linked_content/linked-content-ng2.component';
import { ContentMessageService } from '../core/services/contentMessage.service';
import { VideoComponent } from './streaming/video.component';
import { PageScroll }  from './ng2-page-scroll/ng2-page-scroll.component';
import { VideoJSLanding } from './streaming/videojslanding.component';

declare let angular:any;

angular.module('crossroads')
    .directive('ng2Test', upgradeAdapter.downgradeNg2Component(Ng2TestComponent))
    .directive('ng2TestCmsData', upgradeAdapter.downgradeNg2Component(Ng2TestCMSDataComponent))
    .directive('streaming', upgradeAdapter.downgradeNg2Component(StreamingComponent))
    .directive('videojs', upgradeAdapter.downgradeNg2Component(VideoJSComponent))
    .directive('dynamic-content-ng2', upgradeAdapter.downgradeNg2Component(DynamicContentNg2Component))
    .directive('linked-content-ng2', upgradeAdapter.downgradeNg2Component(LinkedContentNg2Component))
    .directive('streamingVideo', upgradeAdapter.downgradeNg2Component(VideoComponent))
    .directive('pageScroll', upgradeAdapter.downgradeNg2Component(PageScroll))
    .directive('videojsLanding', upgradeAdapter.downgradeNg2Component(VideoJSLanding));

upgradeAdapter.addProvider(ContentMessageService);
angular.module('crossroads')
  .factory('contentMessageService', upgradeAdapter.downgradeNg2Provider(ContentMessageService));
