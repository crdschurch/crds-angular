﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace crds_angular.Exceptions
{
    public class MoreThanOneRecordException : Exception
    {
        public MoreThanOneRecordException(string recordType, string search) :
            base(string.Format("More than one {0} found for {1}", recordType, search))
        {
        }
    }
}