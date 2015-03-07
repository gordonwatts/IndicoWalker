﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Interface for a meeting
    /// </summary>
    public interface IMeetingListRef
    {
        /// <summary>
        /// Return a list of meetings
        /// </summary>
        Task<IEnumerable<IMeetingRefExtended>> GetMeetings(int goingBackDays);
    }
}
