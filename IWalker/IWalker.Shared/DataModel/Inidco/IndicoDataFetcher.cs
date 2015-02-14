using IndicoInterface.NET;
using IWalker.Util;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Fetch the Indico data for us
    /// </summary>
    class IndicoDataFetcher : IUrlFetcher
    {
        /// <summary>
        /// True if we've loaded the CERN cert.
        /// </summary>
        bool _loadedCERNCert = false;

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
            if (!_loadedCERNCert)
            {
                var c = await SecurityUtils.FindCert(SecurityUtils.CERNCertName);
                if (c != null)
                {
                    _loadedCERNCert = true;
                    CERNSSO.WebAccess.LoadCertificate(c);
                }
            }
            
            // Do the actual loading. Hopefully with the cern cert already in there!

            var r = await CERNSSO.WebAccess.GetWebResponse(uri);
            var s = await r.Content.ReadAsInputStreamAsync();
            return new StreamReader(s.AsStreamForRead());
        }
    }
}
