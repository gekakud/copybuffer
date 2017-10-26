using System.Windows.Controls;

namespace CopyBuffer.Ui.Wpf
{
    /// <summary>
    /// Interaction logic for ListControl.xaml
    /// </summary>
    public partial class ListControl
    {
        public ListViewModel _lm;
        public ListControl()
        {
            _lm = new ListViewModel();
            InitializeComponent();
        }
    }
}
