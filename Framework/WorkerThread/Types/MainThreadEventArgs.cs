namespace ProgramMain.Framework.WorkerThread.Types
{
    public class MainThreadEventArgs
    {
        // Summary:
        //     Represents an event with no event data.
        public static readonly MainThreadEventArgs Empty = new MainThreadEventArgs();

        public delegate void MainThreadEventHandler<in T>(object sender, T e) where T : MainThreadEventArgs;

        public delegate void DelegateToMainThread<in T>(T eventParams) where T : MainThreadEventArgs;
    }
}