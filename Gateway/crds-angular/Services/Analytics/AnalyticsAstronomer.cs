using System;
using System.Collections.Generic;
using Segment;
using Segment.Model;

namespace crds_angular.Services.Analytics
{
    public static class AnalyticsAstronomer
    {
        private static string applicationId = Environment.GetEnvironmentVariable("ASTRONOMER_APPLICATION_ID");

        static AnalyticsAstronomer()
        {
            Segment.Analytics.Initialize(applicationId, new Config().SetAsync(true));
        }

        public static void Track(string userId, string eventName, EventProperties props)
        {
            Properties segProps = mapEventProps(props);
            var opts = new Options()
                .SetContext(new Segment.Model.Context()
                {
                    {"ip", "0.0.0.0"}
                });

            Segment.Analytics.Client.Track(userId, eventName, segProps, opts);
            Segment.Analytics.Client.Identify(userId, mapProps(segProps));
            Segment.Analytics.Client.Flush();
        }

        public static void Identify(string userId, EventProperties props)
        {
            Properties segProps = mapEventProps(props);
            Segment.Analytics.Client.Identify(userId, mapProps(segProps));
        }

        private static Traits mapProps(Properties props)
        {
            Traits t = new Traits();
            foreach (KeyValuePair<string, object> p in props)
            {
                t.Add(p.Key, p.Value);
            }
            return t;
        }

        private  static Properties mapEventProps(EventProperties eventProps)
        {
            var props = new Properties();
            foreach (KeyValuePair<string, object> p in eventProps)
            {
                props.Add(p.Key, p.Value);
            }
            return props;
        }
    }
}