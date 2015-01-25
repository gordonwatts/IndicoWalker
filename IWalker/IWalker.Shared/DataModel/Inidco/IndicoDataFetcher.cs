using IndicoInterface.NET;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Fetch the indico data for us
    /// </summary>
    class IndicoDataFetcher : IUrlFetcher
    {
        /// <summary>
        /// Fetch the reader to read everything back from the website
        /// for a given URL.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Fix this up so it is fairly efficient.
        /// </remarks>
        public async Task<StreamReader> GetDataFromURL(Uri uri)
        {
            var r = await CERNSSO.WebAccess.GetWebResponse(uri);
            var s = await r.Content.ReadAsInputStreamAsync();
            return new StreamReader(s.AsStreamForRead());
        }
    }
}
