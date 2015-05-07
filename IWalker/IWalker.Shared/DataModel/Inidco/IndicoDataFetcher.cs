using IndicoInterface.NET;
using IWalker.Util;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Fetch the Indico data for us
    /// </summary>
    class IndicoDataFetcher : IUrlFetcher
    {
        /// <summary>
        /// Hold onto the fetcher singleton.
        /// </summary>
        static Lazy<IndicoDataFetcher> _fetcher = new Lazy<IndicoDataFetcher>(() => new IndicoDataFetcher());

        /// <summary>
        /// Get the singlton instance of the fetcher
        /// </summary>
        public static IndicoDataFetcher Fetcher { get { return _fetcher.Value; } }

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
            var r = await FetchURIResponse(uri);
            var s = await r.Content.ReadAsInputStreamAsync();
            return new StreamReader(s.AsStreamForRead());
        }

        /// <summary>
        /// Fetch the response message for a URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method">What method should be used</param>
        /// <returns></returns>
        private async Task<Windows.Web.Http.HttpResponseMessage> FetchURIResponse(Uri uri, HttpMethod method = null)
        {
            // Only insert a cert once, and make sure that no one gets
            // by here without first doing the insertion. And try to keep it fast
            // if there is no lock needed!
            if (!_loadedCERNCert)
            {
                var c = await SecurityUtils.FindCert(SecurityUtils.CERNCertName);
                if (c != null)
                {
                    lock (this)
                    {
                        if (!_loadedCERNCert)
                        {
                            _loadedCERNCert = true;
                            CERNSSO.WebAccess.LoadCertificate(c);
                        }
                    }
                }
            }

            // Do the actual loading. Hopefully with the CERN cert already in there!

            var r = await CERNSSO.WebAccess.GetWebResponse(uri, method);
            return r;
        }

        /// <summary>
        /// Fetch the header information for this URL, but not the data.
        /// </summary>
        /// <returns></returns>
        internal async Task<HttpContentHeaderCollection> GetContentHeadersFromUrl(Uri uri)
        {
            var r = await FetchURIResponse(uri, HttpMethod.Head);
            return r.Content.Headers;
        }
    }
}
