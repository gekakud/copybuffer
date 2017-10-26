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
        void SetItemToClipboard(BufferItem p_item);
    }
}
