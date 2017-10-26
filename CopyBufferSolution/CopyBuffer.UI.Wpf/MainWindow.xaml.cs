using System.Windows;
using CopyBuffer.Service.Shared;

namespace CopyBuffer.Ui.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ICopyBufferService BufferService { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ICopyBufferService p_bufferService)
        {
            InitializeComponent();
            BufferService = p_bufferService;

            StateChanged -= MainWindowStateChanged;
            Deactivated -= MainWindow_Deactivated;
            Closed -= MainWindow_Closed;

            StateChanged += MainWindowStateChanged;
            Deactivated += MainWindow_Deactivated;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            StateChanged -= MainWindowStateChanged;
            Deactivated -= MainWindow_Deactivated;
            Closed -= MainWindow_Closed;

            Close();
        }

        private void MainWindow_Deactivated(object sender, System.EventArgs e)
        {
            //looses focus
            Hide();
        }

        private void MainWindowStateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Close();
            }
        }
    }
}
