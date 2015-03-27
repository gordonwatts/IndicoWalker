using IWalker.DataModel.Inidco;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Subjects;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The VM to control the add/update
    /// </summary>
    public class AddOrUpdateIndicoApiKeyViewModel : ReactiveObject
    {
        /// <summary>
        /// The add/update command
        /// </summary>
        public ReactiveCommand<object> AddUpdateCommand { get; private set; }

        /// <summary>
        /// Remove the object from the API store
        /// </summary>
        public ReactiveCommand<object> DeleteCommand { get; private set; }

        /// <summary>
        /// The site name
        /// </summary>
        public string SiteName
        {
            get { return _siteName; }
            set { this.RaiseAndSetIfChanged(ref _siteName, value); }
        }
        private string _siteName;

        /// <summary>
        /// The API key
        /// </summary>
        public string ApiKey
        {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }
        private string _apiKey;

        /// <summary>
        /// The Secret key
        /// </summary>
        public string SecretKey
        {
            get { return _secretKey; }
            set { this.RaiseAndSetIfChanged(ref _secretKey, value); }
        }
        private string _secretKey;

        /// <summary>
        /// Get the button text for the add/update button (could be add or update)
        /// </summary>
        public string AddOrUpdateText
        {
            get { return _addOrUpdateText.Value; }
        }
        private ObservableAsPropertyHelper<string> _addOrUpdateText;

        /// <summary>
        /// Create displaying everything. Having a key as null is fine - then everything
        /// starts out blank.
        /// </summary>
        /// <param name="key"></param>
        public AddOrUpdateIndicoApiKeyViewModel(IndicoApiKey key)
        {
            if (key == null)
            {
                SiteName = "";
                ApiKey = "";
                SecretKey = "";
            }
            else
            {
                SiteName = key.Site;
                ApiKey = key.ApiKey;
                SecretKey = key.SecretKey;
            }

            // Setup the add and update commands to work correctly.
            var addCanExe = this.WhenAny(x => x.SiteName, x => x.ApiKey, x => x.SecretKey, (site, apik, seck) => Tuple.Create(site.Value, apik.Value, seck.Value))
                .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2) && !string.IsNullOrWhiteSpace(x.Item3));
            AddUpdateCommand = ReactiveCommand.Create(addCanExe);

            var isKnownSite = new ReplaySubject<bool>(1);
            DeleteCommand = ReactiveCommand.Create(isKnownSite.StartWith(false));

            Observable.Merge(
                    this.WhenAny(x => x.SiteName, x => x.Value),
                    AddUpdateCommand.IsExecuting.Where(isexe => isexe == false).Select(_ => SiteName),
                    DeleteCommand.IsExecuting.Where(isexe => isexe == false).Select(_ => SiteName)
                )
                .Select(sname => IndicoApiKeyAccess.GetKey(sname) != null)
                .Subscribe(gotit => isKnownSite.OnNext(gotit));

            isKnownSite
                .Select(known => known ? "Update" : "Add")
                .ToProperty(this, x => x.AddOrUpdateText, out _addOrUpdateText, "Add");

            // Add, update, or remove
            AddUpdateCommand
                .Subscribe(o => IndicoApiKeyAccess.UpdateKey(new IndicoApiKey() { Site = SiteName, ApiKey = ApiKey, SecretKey = SecretKey }));
            DeleteCommand
                .Subscribe(o => IndicoApiKeyAccess.RemoveKey(SiteName));
        }
    }
}
