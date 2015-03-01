﻿
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
namespace IWalker.ViewModels
{
    /// <summary>
    /// This ViewModel renders the slides as a list of thumb nails (each thumb is managed by SlideThumbViewModel).
    /// It is usually viewed in the on the main app's meeting page, listing all the slide images one after the other.
    /// </summary>
    public class FileSlideListViewModel : ReactiveObject
    {
        /// <summary>
        /// Cache the file we are responsible for.
        /// </summary>
        private IFile _file;

        /// <summary>
        /// The list of thumbnails
        /// </summary>
        public ReactiveList<SlideThumbViewModel> SlideThumbnails { get; private set; }

        /// <summary>
        /// An observable that will fire each time we update the images.
        /// </summary>
        public IObservable<Unit> DoneBuilding { get; private set; }

        /// <summary>
        /// Setup the VM for the file.
        /// </summary>
        /// <param name="f">The file we are to display</param>
        /// <param name="talkTime">The time that this guy is relevant</param>
        public FileSlideListViewModel(IFile f, TimePeriod talkTime)
        {
            Debug.Assert(f != null);

            // Get the object consistent.
            _file = f;
            SlideThumbnails = new ReactiveList<SlideThumbViewModel>();

            if (_file.IsValid && _file.FileType == "pdf")
            {
                // We will want to refresh the view of this file depending on how close we are to the actual
                // meeting time.

                var innerBuffer = new TimePeriod(talkTime);
                innerBuffer.StartTime -= TimeSpan.FromMinutes(30);
                innerBuffer.EndTime += TimeSpan.FromHours(2);

                var updateTalkFile = Observable.Empty<Unit>();
                if (innerBuffer.Contains(DateTime.Now))
                {
                    // Fire every 15 minutes, but only while in the proper time.
                    // Because we will check right after we start up, no need to look right away (skip).
                    updateTalkFile = Observable.Interval(TimeSpan.FromMinutes(15))
                        .Skip(1)
                        .Where(_ => Settings.AutoDownloadNewMeeting)
                        .Where(_ => innerBuffer.Contains(DateTime.Now))
                        .Select(_ => default(Unit));

                }

                // If we are doing auto-download, then we need to re-download the file no matter
                // what.
                if (Settings.AutoDownloadNewMeeting)
                {
                    updateTalkFile = Observable.Return(default(Unit)).Concat(updateTalkFile);
                }

                // Run a rendering and populate the render pdf control with all the
                // thumbnails we can.
                // TODO: Replace the catch below to notify bad PDF format.
                var renderPDF = ReactiveCommand.CreateAsyncObservable<IRandomAccessStream>(_ => f.GetAndUpdateFileOnce(updateTalkFile));

                // Change them into files
                var files = renderPDF
                    .SelectMany(async sf =>
                    {
                        try
                        {
                            var d = await PdfDocument.LoadFromStreamAsync(sf);
                            var key = string.Format("{0}-{1}", f.UniqueKey, (await f.GetCacheCreateTime()).ToString());
                            return Tuple.Create(key, d);
                        }
                        catch (Exception e)
                        {
                            //TODO surface these errors?
                            Debug.WriteLine(string.Format("Error rendering PDF document: '{0}'", e.Message));
                            return Tuple.Create((string)null, (PdfDocument)null);
                        }
                    }).Multicast(new ReplaySubject<Tuple<string, PdfDocument>>()).RefCount();

                // The files are used to go after the items we display
                var fullVM = new Lazy<FullTalkAsStripViewModel>(() => new FullTalkAsStripViewModel(Locator.Current.GetService<IScreen>(), files));

                // The pages now must be changed into thumb-nails for display.
                var pages = files
                    .Select(sf => Enumerable.Range(0, (int)sf.Item2.PageCount)
                                    .Select(index => Tuple.Create(index, sf.Item2.GetPage((uint)index)))
                                    .Select(p => new SlideThumbViewModel(p.Item2, fullVM, p.Item1, sf.Item1)))
                    .Publish();

                Exception userBomb;
                pages
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(pgs =>
                    {
                        SlideThumbnails.Clear();
                        SlideThumbnails.AddRange(pgs);
                    },
                               ex => userBomb = ex);

                // And we should let any testing stuff going know we are done.
                DoneBuilding = pages.Select(_ => Unit.Default);

                // Wire it up!
                pages.Connect();

                // TODO: Normally this would not kick off a download, but in this case
                // we will until we get a real background store in there. Then we can kick this
                // off only if the file is actually downloaded.

                renderPDF.ExecuteAsync().Subscribe();
            }
        }
    }
}
