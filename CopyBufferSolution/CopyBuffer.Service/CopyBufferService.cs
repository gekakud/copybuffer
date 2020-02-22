using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var itemImg = new KeyValuePair<int, string>?();

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
                    var listOfHashes = _copyHistory.Select(bi => bi.ImageContent.Key).ToList();
                    itemImg = clipboardWrapper.GetImage(listOfHashes);

                    if (itemStr == null && itemImg.Value.Key == 0)
                    {
                        continue;
                    }

                    var item = new BufferItem
                    {
                        TextContent = itemStr,
                        ImageContent = itemImg.Value.Key == 0 ? new KeyValuePair<int, string>() : itemImg.Value,
                        TimeStamp = DateTime.Now,
                        ItemType = itemStr == null ? BufferItemType.Image:BufferItemType.Text
                    };

                    var isTextItemExist = item.TextContent != null && _copyHistory.Count(e =>
                                              e.ItemType == BufferItemType.Text && e.TextContent != null &&
                                              e.TextContent.Equals(item.TextContent)) == 0;

                    if (item.ItemType == BufferItemType.Text && isTextItemExist)
                    {
                        _copyHistory.Add(item);
                        continue;
                    }

                    var isImageItemExist = _copyHistory.Count(
                                               e =>
                                                   e.ItemType == BufferItemType.Image && e.ImageContent.Key != 0 &&
                                                   e.ImageContent.Key == item.ImageContent.Key) == 0;

                    if (item.ItemType == BufferItemType.Image && isImageItemExist)

                    {
                        _copyHistory.Add(item);

                    }
                }
                catch (Exception exe)
                {
                    //TODO need proper handling
                }

                Thread.Sleep(300);
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
                    //clipboardWrapper.SetImage(p_item.ImageContent);
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