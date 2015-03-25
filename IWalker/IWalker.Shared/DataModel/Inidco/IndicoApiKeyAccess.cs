using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using System.Linq;
using Newtonsoft.Json;

namespace IWalker.DataModel.Inidco
{
    /// <summary>
    /// Provide access to the API keys for an indico site
    /// </summary>
    public static class IndicoApiKeyAccess
    {
        /// <summary>
        /// Fetch the api key for a give site from the encrypted store.
        /// Returns null if it is not known.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static IndicoApiKey GetKey (string site)
        {
            var key = AsKey(site);
            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(key))
            {
                return null;
            }
            return ApplicationData.Current.RoamingSettings.Values[key].Deserialize();
        }

        /// <summary>
        /// The key prefix for our store.
        /// </summary>
        const string KeyPrefix = "IndicoApiKey_";

        /// <summary>
        /// Create a setting name given a site.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private static string AsKey(string site)
        {
            return KeyPrefix + site;
        }

        /// <summary>
        /// Update or store a key in the encrypted store.
        /// </summary>
        /// <param name="apikey"></param>
        public static void UpdateKey(IndicoApiKey apikey)
        {
            ApplicationData.Current.RoamingSettings.Values[AsKey(apikey.Site)] = apikey.Serialize();
        }

        /// <summary>
        /// Remove a key from the store. Ignore if key does not exist.
        /// </summary>
        /// <param name="site"></param>
        public static void RemoveKey(string site)
        {
            var key = AsKey(site);
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.RoamingSettings.Values.Remove(key);
            }
        }

        /// <summary>
        /// Make sure the store is totally empty of keys.
        /// </summary>
        public static void RemoveAllKeys()
        {
            var siteKeys = ApplicationData.Current.RoamingSettings.Values.Keys
                .Where(k => k.StartsWith(KeyPrefix))
                .ToArray();
            foreach (var k in siteKeys)
            {
                ApplicationData.Current.RoamingSettings.Values.Remove(k);
            }
        }
    }

    /// <summary>
    /// Internal helper classes to do the serialization and deserialization.
    /// </summary>
    static class IndicoApiKeyAccessHelpers
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Serialize(this IndicoApiKey key)
        {
            return JsonConvert.SerializeObject(key);
        }

        /// <summary>
        /// Deserialize an object.
        /// </summary>
        /// <param name="serialized"></param>
        /// <returns></returns>
        public static IndicoApiKey Deserialize(this object serialized)
        {
            return JsonConvert.DeserializeObject<IndicoApiKey>(serialized as string);
        }
    }
}
