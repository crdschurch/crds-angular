using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using MinistryPlatform.Translation.PlatformService;
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
            Properties segProps = mapProps(props);
            var opts = new Options()
                .SetContext(new Segment.Model.Context()
                {
                    {"ip", "0.0.0.0"}
                });

            Segment.Analytics.Client.Track(userId, eventName, segProps, opts);
            Segment.Analytics.Client.Flush();

        }

        private  static Properties mapProps(EventProperties eventProps)
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