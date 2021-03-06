﻿
using System;
namespace IWalker.DataModel.Interfaces
{
    public interface ISession
    {
        /// <summary>
        /// The list of talks associated with this session
        /// </summary>
        ITalk[] Talks { get; }

        /// <summary>
        /// Return the start time for the session
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// The name of the session.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// A unique ID for a sesson
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Return true if this is a place holder session - in short, its title, etc., should
        /// not be displayed to the user
        /// </summary>
        bool IsPlaceHolderSession { get; }
    }
}
