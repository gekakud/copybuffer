using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CopyBuffer.Service.Shared;
using NLog.Fluent;

namespace CopyBuffer.Service
{
    public class CopyBufferService:ICopyBufferService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static ICopyBufferService Instance
        {
            get { return instance; }
        }

        //singleton
        private static readonly ICopyBufferService instance = new CopyBufferService();

        private ConcurrentBag<BufferItem> _copyHistory;
        private bool IsServiceRunning;
        private readonly CancellationTokenSource _tokenSource;
        private CancellationToken ct;

        public bool HistoryWasCleared { get; set; }

        private Timer timer;

        private ClipboardWrapper clipboardWrapper;

        private CopyBufferService()
        {
            InitLogger();
            clipboardWrapper = new ClipboardWrapper();
            _copyHistory = new ConcurrentBag<BufferItem>();
            _tokenSource = new CancellationTokenSource();
            ct = _tokenSource.Token;
        }

        private void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        private void MonitorClip(object o)
        {
            var itemStr = string.Empty;

                if (ct.IsCancellationRequested)
                {
                    IsServiceRunning = false;
                    return;
                }

                try
                {
                    itemStr = clipboardWrapper.GetText();

                    if (itemStr == null)
                    {
                        return;
                    }

                    var itemNotYetInhistory = _copyHistory.Count(e =>
                                              e.TextContent != null &&
                                              e.TextContent.Equals(itemStr)) == 0;

                    if (!itemNotYetInhistory)
                    {
                        return;
                    }

                    if (itemNotYetInhistory)
                    {
                        _copyHistory.Add(new BufferItem
                        {
                            TextContent = itemStr,
                            TimeStamp = DateTime.Now,
                            ItemType = BufferItemType.Text
                        });
                        Logger.Info("Item added");
                        return;
                    }
                }
                catch (Exception exception)
                {
                    //TODO need proper handling
                    Logger.Error(exception);
                }
        }

        public void Start()
        {
            if (IsServiceRunning) return;
            IsServiceRunning = true;
            timer = new Timer(MonitorClip, null, 0, 300);
            Logger.Info("Service running");
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
                timer.Dispose();
            }
            catch (Exception exception)
            {
                //TODO need proper handling
                Logger.Error(exception);
            }
        }

        public List<BufferItem> GetBufferedHistory()
        {
            return _copyHistory.ToList();
        }

        public int GetItemsCount()
        {
            return _copyHistory.Count;
        }

        public void ClearBufferHistory()
        {
            foreach (var bufferItem in _copyHistory)
            {
                _copyHistory = new ConcurrentBag<BufferItem>();
            }

            HistoryWasCleared = true;
            Logger.Info("History cleared");
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
            timer?.Dispose();
            _tokenSource?.Dispose();
        }
    }
}