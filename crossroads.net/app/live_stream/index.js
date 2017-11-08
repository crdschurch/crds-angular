
import CONSTANTS from 'crds-constants';
import liveStreamRouter from './live_stream.routes';
import StreamspotService from './services/streamspot.service';
import ReminderService from './services/reminder.service';
import StreamStatusService from './services/stream-status.service';
import GeolocationService from './services/geolocation.service';
import CountdownService from './services/countdown.service';
import GoogleMapsService from '../services/google_maps.service';

export default angular
  .module(CONSTANTS.MODULES.LIVE_STREAM, [CONSTANTS.MODULES.CORE, CONSTANTS.MODULES.COMMON, CONSTANTS.MODULES.MEDIA, CONSTANTS.MODULES.PROFILE])
  .config(liveStreamRouter)
  .service('StreamspotService', StreamspotService)
  .service('ReminderService', ReminderService)
  .service('GeolocationService', GeolocationService)
  .service('GoogleMapsService', GoogleMapsService)
  .service('StreamStatusService', StreamStatusService)
  .service('CountdownService', CountdownService)
;

import contentCard from './content_card';
import countdownHeader from './countdown_header';
import countdownHome from './countdown_home';
import countdownIntro from './countdown_intro';
import currentSeries from './current_series';
import currentSeriesModal from './current_series_modal';
import geolocation from './geolocation';
import geolocationModal from './geolocation_modal';
import landing from './landing';
import stream from './stream';
import streamingReminder from './streaming_reminder';
import streamspotPlayer from './streamspot_player'
import socialSharing from '../../core/components/social_sharing';
