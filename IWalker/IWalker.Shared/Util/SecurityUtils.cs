using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;

namespace IWalker.Util
{
    class SecurityUtils
    {
        /// <summary>
        /// Get the name under which we are storing the cern cert in the App store's
        /// lock box.
        /// </summary>
        public static string CERNCertName
        {
            get { return "CERNCert";}
        }

        /// <summary>
        /// Returns a certificate with a given name
        /// </summary>
        /// <param name="certName">Name of cert we are going to look for</param>
        /// <returns>null if the cert isn't there, otherwise the cert that was found.</returns>
        public static async Task<Certificate> FindCert(string certName)
        {
            // Work around for the TplEventListener not working correctly.
            // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/3e505e04-7f30-4313-aa47-275eaef333dd/systemargumentexception-use-of-undefined-keyword-value-1-for-event-taskscheduled-in-async?forum=wpdevelop
            await Task.Delay(1);

            // Do the CERT query

            var query = new CertificateQuery();
            query.FriendlyName = certName;
            var certificates = await CertificateStores.FindAllAsync(query);

            if (certificates.Count != 1)
            {
                return null;
            }
            return certificates[0];
        }
    }
}
