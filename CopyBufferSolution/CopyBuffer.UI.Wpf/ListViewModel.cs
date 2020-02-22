using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CopyBuffer.Service;
using CopyBuffer.Service.Shared;
using CopyBuffer.Ui.Wpf.Common;

namespace CopyBuffer.Ui.Wpf
{
    public class ListViewModel: BaseViewModel,IDisposable
    {
        public Action CloseAction { get; set; }
        private SortedSet<BufferItem> _sortedSet;
        private ICopyBufferService service;
        private bool _firstSet = true;

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

                service.SetItemToClipboard(_selectedItem);
                
                Application.Current.MainWindow?.Close();
            }
        }

        private List<BufferItem> _list;
        private BufferItem _selectedItem;
        private CancellationTokenSource cts;
        private CancellationToken ct;

        //how to pass ICopyService to here???
        //how  I know when ListControl is disposed to cancel the task
        public ListViewModel()
        {
            _sortedSet = new SortedSet<BufferItem>(new BufferItemComparer());
            cts = new CancellationTokenSource();
            ct = cts.Token;

            service = CopyBufferService.Instance;

            CopyList = new List<BufferItem>();
            UpdateSortedSet();
            Task.Run(CopyListRefreshTask, cts.Token);
        }

        private void CopyListRefreshTask()
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(200);
                if (CopyList.Count == service.GetBufferedHistory().Count)
                    continue;

                UpdateSortedSet();              
            }
        }

        private void UpdateSortedSet()
        {
            foreach (var bufferItem in service.GetBufferedHistory())
            {
                _sortedSet.Add(bufferItem);
            }
            CopyList = _sortedSet.Select(r => r).ToList();
        }

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
