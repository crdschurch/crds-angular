﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Segment.Model;

namespace crds_angular.Services.Analytics
{
    public class AnalyticsService: IAnalyticsService
    {
        public void Track(string userId, string eventName)
        {
            EventProperties props = new EventProperties();
            props.Add("Source", "CrossroadsNet");
            AnalyticsAstronomer.Track(userId, eventName, props);
        }

        public void Track(string userId, string eventName, EventProperties props)
        {
            props.Add("Source", "CrossroadsNet");
            Task.Run(()=> AnalyticsAstronomer.Track(userId, eventName, props));
        }

        public void IdentifyLoggedInUser(string userId, EventProperties props)
        {
            AnalyticsAstronomer.Identify(userId, props);
        }
    }
}