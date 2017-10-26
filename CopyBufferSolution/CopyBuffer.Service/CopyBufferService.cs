using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CopyBuffer.Service.Shared;

namespace CopyBuffer.Service
{
    public class CopyBufferService:ICopyBufferService
    {
        public static ICopyBufferService Instance
        {
            get { return instance; }
        }

        private readonly ConcurrentBag<BufferItem> _copyHistory;
        private bool IsServiceRunning;
        private readonly Task _clipboardMonitoring;
        private readonly CancellationTokenSource _tokenSource;
        private CancellationToken ct;

        private static readonly ICopyBufferService instance = new CopyBufferService();
        private ClipboardWrapper clipboardWrapper;

        private CopyBufferService()
        {
            clipboardWrapper = new ClipboardWrapper();
            _copyHistory = new ConcurrentBag<BufferItem>();
            _tokenSource = new CancellationTokenSource();
            ct = _tokenSource.Token;
            _clipboardMonitoring = new Task(MonitorClip, ct, TaskCreationOptions.LongRunning);
        }

        private void MonitorClip()
        {
            var itemStr = string.Empty;
            var itemImg = new BitmapImage();

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    IsServiceRunning = false;
                    break;
                }

                try
                {
                    itemStr = clipboardWrapper.GetText();
                    itemImg = clipboardWrapper.GetImage();

                    if (itemStr == null && itemImg == null)
                    {
                        continue;
                    }

                    var item = new BufferItem
                    {
                        TextContent = itemStr,
                        ImageContent = itemImg,
                        TimeStamp = DateTime.Now,
                        ItemType = itemStr == null ? BufferItemType.Image:BufferItemType.Text
                    };

                    if (item.ItemType == BufferItemType.Text && item.TextContent!=null && _copyHistory.Count(e => e.ItemType == BufferItemType.Text && e.TextContent!=null && e.TextContent.Equals(item.TextContent)) == 0)
                    {
                        _copyHistory.Add(item);
                        continue;
                    }

                    if (item.ItemType == BufferItemType.Image && item.ImageContent!=null && 
                        _copyHistory.Count(
                            e =>
                                e.ItemType == BufferItemType.Image && e.ImageContent != null &&
                                e.ImageContent.IsEqual(item.ImageContent)) == 0)
                    {
                        _copyHistory.Add(item);

                    }
                }
                catch (ExternalException)
                {
                    //TODO need proper handling
                }

                Thread.Sleep(200);
            }
        }

        public void Start()
        {
            if (IsServiceRunning) return;
            IsServiceRunning = true;
            _clipboardMonitoring.Start();
        }

        public void Stop()
        {
            if (!IsServiceRunning)
            {
                return;
            }
            try
            {
                _tokenSource.Cancel();
            }
            catch (Exception)
            {
                //TODO need proper handling
            }
        }

        public List<BufferItem> GetBufferedHistory()
        {
            return _copyHistory.ToList();
        }

        public void ClearBufferHistory()
        {
            foreach (var bufferItem in _copyHistory)
            {
                var item = new BufferItem();
                _copyHistory.TryTake(out item);
            }
        }

        public void SetItemToClipboard(BufferItem p_item)
        {
            switch (p_item.ItemType)
            {
                case BufferItemType.Text:
                    clipboardWrapper.SetText(p_item.TextContent);
                    break;

                case BufferItemType.Image:
                    clipboardWrapper.SetImage(p_item.ImageContent);
                    break;
            }
        }

        public void Dispose()
        {
            _clipboardMonitoring?.Dispose();
            _tokenSource?.Dispose();
        }
    }
}