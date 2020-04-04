using System;
using AsyncWindowsClipboard;

namespace CopyBuffer.Service
{
    /// <summary>
    /// Use Windows native clipboard to set(paste) text to clipboard
    /// </summary>
    internal class NativeClipboardSetter
    {
        readonly WindowsClipboardService _clipboardService = new WindowsClipboardService(TimeSpan.FromMilliseconds(50));
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void SetText(string p_text)
        {
            try
            {
                _clipboardService.SetTextAsync(p_text);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }
    }
}
