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

        public static ICopyBufferService Instance => instance;

        //singleton
        private static readonly ICopyBufferService instance = new CopyBufferService();

        private ConcurrentBag<BufferItem> _copyHistory;
        private bool IsServiceRunning;
        private readonly CancellationTokenSource _tokenSource;
        private CancellationToken cancellationToken;
        private string LastItemAdded = string.Empty;

        public bool HistoryWasCleared { get; set; }
        public bool RefreshUiList { get; set; }

        private Timer timer;

        private NativeClipboardSetter _nativeClipboardSetter;
        private SharpClipWrapper sharpClipWrapper;

        private CopyBufferService()
        {
            InitLogger();
            _nativeClipboardSetter = new NativeClipboardSetter();
            sharpClipWrapper = new SharpClipWrapper();

            _copyHistory = new ConcurrentBag<BufferItem>();
            _tokenSource = new CancellationTokenSource();
            cancellationToken = _tokenSource.Token;
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

        object locker = new object();

        private void MonitorClip(object o)
        {
            //check possible race condition
            lock (locker)
            {
                var itemStr = string.Empty;

                if (cancellationToken.IsCancellationRequested)
                {
                    IsServiceRunning = false;
                    return;
                }

                try
                {
                    itemStr = sharpClipWrapper.MostRecentClipboardText;

                    if (LastItemAdded == itemStr || string.IsNullOrEmpty(itemStr))
                    {
                        return;
                    }

                    BufferItem itemInHistory = _copyHistory.FirstOrDefault(e => e.TextContent == itemStr);

                    //if item already exist - move it in top of list
                    if (itemInHistory != null)
                    {
                        itemInHistory.TimeStamp = DateTime.Now;
                        LastItemAdded = itemStr;
                        RefreshUiList = true;
                        return;
                    }

                    _copyHistory.Add(new BufferItem
                    {
                        TextContent = itemStr,
                        TimeStamp = DateTime.Now,
                        ItemType = BufferItemType.Text
                    });

                    LastItemAdded = itemStr;
                    RefreshUiList = true;
                    Logger.Info("Item added");
                }
                catch (Exception exception)
                {
                    //TODO need proper handling
                    Logger.Error(exception);
                }
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
            RefreshUiList = true;
            Logger.Info("History cleared");
        }

        public void SetItemToClipboard(BufferItem p_item)
        {
            switch (p_item.ItemType)
            {
                case BufferItemType.Text:
                    _nativeClipboardSetter.SetText(p_item.TextContent);
                    break;

                case BufferItemType.Image:
                    
                    break;
            }
        }

        public void Dispose()
        {
            sharpClipWrapper?.Dispose();
            timer?.Dispose();
            _tokenSource?.Dispose();
        }
    }
}