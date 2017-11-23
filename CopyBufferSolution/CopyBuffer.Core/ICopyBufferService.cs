using System;
using System.Collections.Generic;
using CopyBuffer.Core;

namespace CopyBuffer.Service.Shared
{
    public interface ICopyBufferService:IDisposable
    {
        event EventHandler ClibordNewMessage;

        void Start();
        void Stop();
        void ClearBufferHistory();

        List<BufferItem> GetBufferedHistory();
        void SetItemToClipboard(BufferItem p_item);
    }
}
