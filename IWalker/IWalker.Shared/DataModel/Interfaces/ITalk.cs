
using System;
namespace IWalker.DataModel.Interfaces
{
    public interface ITalk : IEquatable<ITalk>
    {
        /// <summary>
        /// Get the title of this talk
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Get the main file that holds the contents of this talk.
        /// </summary>
        IFile TalkFile { get; }
        
        /// <summary>
        /// Returns a list of all talk files
        /// </summary>
        IFile[] AllTalkFiles { get; }

        /// <summary>
        /// TIme when the talk should start.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Time when the talk should finish.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Return list of sub-talks
        /// </summary>
        ITalk[] SubTalks { get; }
    }
}
