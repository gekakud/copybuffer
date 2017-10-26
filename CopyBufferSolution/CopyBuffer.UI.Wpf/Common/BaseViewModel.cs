using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CopyBuffer.Ui.Wpf.Common
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected virtual void SetProperty<T>(ref T p_member, T p_val, [CallerMemberName] string p_propertyName = null)
        {
            if (Equals(p_member, p_val))
            {
                return;
            }
            p_member = p_val;
            PropertyChanged(this, new PropertyChangedEventArgs(p_propertyName));
        }

        protected virtual void OnPropertyChanged(string p_propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(p_propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
