using System;
using CopyBuffer.Service;
using CopyBuffer.Service.Shared;
using CopyBuffer.Ui.Wpf.Common;
using Application = System.Windows.Application;

namespace CopyBuffer.Ui.Wpf
{
    public class NotifyIconViewModel : BaseViewModel,IDisposable
    {
        public Command ShowWindowCommand { get; private set; }
        public Command HideWindowCommand { get; private set; }
        public Command ClearHistoryCommand { get; private set; }
        public Command ExitApplicationCommand { get; private set; }

        private readonly KeyEventHandler _keyEventHandler;
        private readonly ICopyBufferService _bufferService;

        public NotifyIconViewModel()
        {
            _bufferService = CopyBufferService.Instance;
            _bufferService.Start();
            
            ShowWindowCommand = new Command(ShowWindowInternal);
            HideWindowCommand = new Command(HideWindowInternal);
            ClearHistoryCommand = new Command(ClearHistoryInternal);
            ExitApplicationCommand = new Command(ExitApplicationInternal);
            
            _keyEventHandler = new KeyEventHandler(ShowWindowInternal);
        }

        private void ShowWindowInternal()
        {
            if (Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = new MainWindow(_bufferService);
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
                Application.Current.MainWindow.Focusable = true;
                Application.Current.MainWindow.Focus();
            }
            else
            {
                Application.Current.MainWindow.Show();
            }
        }

        private void ClearHistoryInternal()
        {
            _bufferService.ClearBufferHistory();
        }

        private void HideWindowInternal()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
            }
        }

        private void ExitApplicationInternal()
        {
            _keyEventHandler.UnsubscribeFromEvent();
            _bufferService.Stop();
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _bufferService?.Dispose();
            _keyEventHandler.UnsubscribeFromEvent();
        }
    }
}