﻿using System.Windows;
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

        public MainWindow(ICopyBufferService pBufferService)
        {
            InitializeComponent();
            BufferService = pBufferService;
            DataContext = new ListViewModel(BufferService);

            StateChanged -= MainWindowStateChanged;
            Deactivated -= MainWindow_Deactivated;
            Closed -= MainWindow_Closed;

            StateChanged += MainWindowStateChanged;
            Deactivated += MainWindow_Deactivated;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
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
