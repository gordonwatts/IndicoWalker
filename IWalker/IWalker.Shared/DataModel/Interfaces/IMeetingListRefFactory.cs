using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Turn a URL into a IMeetingListRef.
    /// </summary>
    public interface IMeetingListRefFactory
    {
        IMeetingListRef GenerateMeetingListRef(string url);
    }
}
