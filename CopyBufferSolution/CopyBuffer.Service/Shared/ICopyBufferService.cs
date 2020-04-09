using System;
using System.Collections.Generic;

namespace CopyBuffer.Service.Shared
{
    public interface ICopyBufferService:IDisposable
    {
        void Start();
        void Stop();
        void ClearBufferHistory();

        List<BufferItem> GetBufferedHistory();
        int GetItemsCount();
        bool HistoryWasCleared { get; set; }
        bool RefreshUiList { get; set; }
        void SetItemToClipboard(BufferItem p_item);
    }
}
