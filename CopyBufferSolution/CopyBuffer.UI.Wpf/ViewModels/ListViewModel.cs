using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CopyBuffer.Core;
using CopyBuffer.Service;
using CopyBuffer.Service.Shared;
using CopyBuffer.Ui.Wpf.Common;


namespace CopyBuffer.Ui.Wpf
{
    public class ListViewModel: BaseViewModel,IDisposable
    {
        public Action CloseAction { get; set; }

        #region Data Members

        private SortedSet<BufferItem> _sortedSet;
        private ICopyBufferService _service;
        private bool _firstSet = true;
        private List<BufferItem> _list;
        private BufferItem _selectedItem;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        private BufferItem _firstItem;

        #endregion

        #region Properties

        public BufferItem FirstItem
        {
            get { return _firstItem; }
            set { SetProperty(ref _firstItem, value);}
        }

        public List<BufferItem> CopyList
        {
            get { return _list; }
            set { SetProperty(ref _list, value); }
        }

        public BufferItem SelectedItem
        {
            get
            {
                if (_firstSet)
                {
                    _firstSet = false;
                }
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                _service.SetItemToClipboard(_selectedItem);

                Application.Current.MainWindow.Close();
            }
        }

        #endregion

        #region Ctor

        //how to pass ICopyService to here???
        //how  I know when ListControl is disposed to cancel the task
        public ListViewModel(ICopyBufferService service)
        {
            _sortedSet = new SortedSet<BufferItem>(new BufferItemComparer());
            cts = new CancellationTokenSource();
            ct = cts.Token;

            _service = service;
            _service.ClibordNewMessage += _service_ClibordNewMessage;   
            CopyList = new List<BufferItem>();
            UpdateSortedSet();
        }

        private void _service_ClibordNewMessage(object sender, EventArgs e)
        {
            UpdateSortedSet();
        }

        #endregion

        #region Private Methods


        private void UpdateSortedSet()
        {
            foreach (var bufferItem in _service.GetBufferedHistory())
            {
                _sortedSet.Add(bufferItem);
            }
            CopyList = _sortedSet.Select(r => r).ToList();
            FirstItem = CopyList.Count > 0 ? CopyList.First(): new BufferItem();

            if (CopyList.Contains(_sortedSet.First()))
            {
                CopyList.Remove(_sortedSet.First());
            }
          
        
        }

        #endregion

        public void Dispose()
        {
            cts.Cancel();
        }
    }

    public class BufferItemComparer : IComparer<BufferItem>
    {
        public int Compare(BufferItem x, BufferItem y)
        {
            return y.TimeStamp.CompareTo(x.TimeStamp);
        }
    }
}
