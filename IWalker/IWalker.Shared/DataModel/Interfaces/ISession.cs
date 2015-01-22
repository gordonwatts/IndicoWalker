﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Interfaces
{
    public interface ISession
    {
        /// <summary>
        /// The list of talks associated with this session
        /// </summary>
        ITalk[] Talks { get; }
    }
}