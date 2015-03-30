using IWalker.DataModel.Interfaces;
using Newtonsoft.Json;
using Splat;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage;
using System.Reactive;

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
            try
            {
                var cached = JsonConvert.DeserializeObject<CategoryConfigInfo[]>(json, GetSettings())
                    .Select(c => {
                        if (c.CategoryTitle == null)
                            c.CategoryTitle = "<blank>";
                        return c;
                    });
                var ll = new List<CategoryConfigInfo>();
                ll.AddRange(cached);
                return ll;
            } catch (Exception)
            {
                // This is brutal, but not sure how else to deal with corrupt settings data!
                return new List<CategoryConfigInfo>();
            }
        }

        /// <summary>
        /// Save the list of categories to local storage.
        /// </summary>
        /// <param name="cats"></param>
        public static void SaveCategories(List<CategoryConfigInfo> cats)
        {
            var sb = JsonConvert.SerializeObject(cats.ToArray(), GetSettings());
            ApplicationData.Current.RoamingSettings.Values[CategoryDBSettingName] = sb;
        }

        /// <summary>
        /// Internal helper to get the JSON settings easier...
        /// </summary>
        /// <returns></returns>
        private static JsonSerializerSettings GetSettings()
        {
            return Locator.Current.GetService<JsonSerializerSettings>();
        }

        /// <summary>
        /// Update or, if not there, insert the category.
        /// </summary>
        /// <param name="cat"></param>
        public static void UpdateOrInsert (CategoryConfigInfo cat)
        {
            bool found = false;
            var items = LoadCategories();
            for (int i = 0; i < items.Count; i++ )
            {
                if (items[i].MeetingList.UniqueString == cat.MeetingList.UniqueString)
                {
                    items[i] = cat;
                    found = true;
                }
            }
            if (!found)
            {
                items.Add(cat);
            }
            SaveCategories(items);
        }

        /// <summary>
        /// Remove a category from the list.
        /// </summary>
        /// <param name="cat"></param>
        public static void Remove (CategoryConfigInfo cat)
        {
            var items = LoadCategories();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].MeetingList.UniqueString == cat.MeetingList.UniqueString)
                {
                    items.RemoveAt(i);
                    SaveCategories(items);
                    return;
                }
            }
        }

        /// <summary>
        /// Find a meeting in the db. If it isn't there, return null.
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        public static CategoryConfigInfo Find (IMeetingListRef meeting)
        {
            return LoadCategories()
                .Where(m => m.MeetingList.UniqueString == meeting.UniqueString)
                .FirstOrDefault();
        }
    }
}
