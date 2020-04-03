using System;
using System.Threading.Tasks;
using AsyncWindowsClipboard;

namespace CopyBuffer.Service
{
    /// <summary>
    /// Wrapping actual clipboard service!
    /// </summary>
    internal class ClipboardWrapper
    {
        readonly WindowsClipboardService _clipboardService = new WindowsClipboardService(TimeSpan.FromMilliseconds(50));
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private async Task<string> GetTextInternal()
        {
            try
            {
                return await _clipboardService.GetTextAsync();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return await Task.Run(() => "");
        }

        public string GetText()
        {
            return GetTextInternal().Result;
        }

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
