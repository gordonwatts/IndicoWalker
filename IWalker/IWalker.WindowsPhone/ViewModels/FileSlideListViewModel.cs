using ReactiveUI;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Dummy VM for use building x-platform.
    /// </summary>
    public class FileSlideListViewModel : ReactiveObject
    {
        public FileSlideListViewModel(DataModel.Interfaces.IFile file, Util.TimePeriod timeSpan, System.IObservable<System.Reactive.Unit> observable)
        {
        }
    }
}
