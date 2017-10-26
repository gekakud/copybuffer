using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace CopyBuffer.Ui.Wpf
{
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            notifyIcon.Visibility = Visibility.Visible;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
