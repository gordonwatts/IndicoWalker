using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// The info for an indico api key
    /// </summary>
    public class IndicoApiKey
    {
        /// <summary>
        /// The site for which this api key is valid for
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// The api key itself
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The api key
        /// </summary>
        public string SecretKey { get; set; }
    }
}
