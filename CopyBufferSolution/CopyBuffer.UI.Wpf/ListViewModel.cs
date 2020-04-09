using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using CopyBuffer.Service;
using CopyBuffer.Service.Shared;
using CopyBuffer.Ui.Wpf.Common;

namespace CopyBuffer.Ui.Wpf
{
    public class ListViewModel: BaseViewModel,IDisposable
    {
        public Action CloseAction { get; set; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private SortedSet<BufferItemUi> _sortedSet;
        private ICopyBufferService service;
        private bool _firstSet = true;

        public List<BufferItemUi> CopyList
        {
            get { return _list; }
            set { SetProperty(ref _list, value); }
        }

        public BufferItemUi SelectedItem
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

                service.SetItemToClipboard(new BufferItem
                {
                    TextContent = _selectedItem.TextContent,
                    ItemType = BufferItemType.Text,
                    TimeStamp = _selectedItem.TimeStamp
                });
                
                Application.Current.MainWindow?.Close();
            }
        }

        private List<BufferItemUi> _list;
        private BufferItemUi _selectedItem;

        private Timer timer;

        //how to pass ICopyService to here???
        //how  I know when ListControl is disposed to cancel the task
        public ListViewModel()
        {
            _sortedSet = new SortedSet<BufferItemUi>(new BufferItemComparer());

            service = CopyBufferService.Instance;

            CopyList = new List<BufferItemUi>();
            timer = new Timer(CopyListRefreshTask, null, 0, 200);
            UpdateSortedSet();
        }

        private void CopyListRefreshTask(object o)
        {
            if (service.HistoryWasCleared)
            {
                CopyList.Clear();
                _sortedSet.Clear();
                service.HistoryWasCleared = false;
            }

            if (service.RefreshUiList)
            {
                UpdateSortedSet();
                service.RefreshUiList = false;
            }
        }

        private void UpdateSortedSet()
        {
            CopyList.Clear();
            _sortedSet.Clear();

            foreach (var bufferItem in service.GetBufferedHistory())
            {
                _sortedSet.Add(new BufferItemUi
                {
                    ItemType = bufferItem.ItemType,
                    TextContent = string.Copy(bufferItem.TextContent),
                    TimeStamp = bufferItem.TimeStamp
                });
            }

            CopyList = _sortedSet.Select(r => r).ToList();
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }

    public class BufferItemComparer : IComparer<BufferItemUi>
    {
        public int Compare(BufferItemUi x, BufferItemUi y)
        {
            return y.TimeStamp.CompareTo(x.TimeStamp);
        }
    }
}
