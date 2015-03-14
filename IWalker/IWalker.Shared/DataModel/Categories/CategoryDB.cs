using Newtonsoft.Json;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage;

namespace IWalker.DataModel.Categories
{
    /// <summary>
    /// Implement I/O for the categories.
    /// </summary>
    public static class CategoryDB
    {
        /// <summary>
        /// The name of the setting we use to cache the category db
        /// THis is set in the roaming settings!
        /// </summary>
        private const string CategoryDBSettingName = "CategoryDBSerliazied";

        /// <summary>
        /// Load the categories from local storage.
        /// </summary>
        /// <returns></returns>
        public static List<CategoryConfigInfo> LoadCategories()
        {
            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(CategoryDBSettingName))
                return new List<CategoryConfigInfo>();

            var json = ApplicationData.Current.RoamingSettings.Values[CategoryDBSettingName] as string;
            var cached = JsonConvert.DeserializeObject<List<CategoryConfigInfo>>(json, GetSettings());
            return cached;
        }

        /// <summary>
        /// Save the list of categories to local storage.
        /// </summary>
        /// <param name="cats"></param>
        public static void SaveCategories(List<CategoryConfigInfo> cats)
        {
            var sb = JsonConvert.SerializeObject(cats, GetSettings());
            ApplicationData.Current.RoamingSettings.Values[CategoryDBSettingName] = sb;
        }

        private static JsonSerializerSettings GetSettings()
        {
            return Locator.Current.GetService<JsonSerializerSettings>();
        }
    }
}
