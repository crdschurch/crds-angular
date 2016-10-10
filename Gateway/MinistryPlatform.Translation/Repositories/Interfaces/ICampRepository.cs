﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ICampRepository
    {
        List<MpCampEvent> GetCampEventDetails(int eventId);
        int CreateMinorContact(MpMinorContact minorContact);
    }
}
