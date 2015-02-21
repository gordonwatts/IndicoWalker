
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
    }
}
