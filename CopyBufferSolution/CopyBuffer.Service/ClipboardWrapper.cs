using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AsyncWindowsClipboard;

namespace CopyBuffer.Service
{
    internal class ClipboardWrapper
    {
        readonly WindowsClipboardService _clipboardService = new WindowsClipboardService(TimeSpan.FromMilliseconds(50));

        private async Task<string> GetTextInternal()
        {
            return await _clipboardService.GetTextAsync();
        }

        public string GetText()
        {
            return GetTextInternal().Result;
        }

        public void SetText(string p_text)
        {
            _clipboardService.SetTextAsync(p_text);
        }

        public BitmapImage GetImage()
        {
            return ClipboardOldSchool.GetImage();
        }

        public void SetImage(BitmapImage p_image)
        {
            var fff = p_image;
        }
    }
}
