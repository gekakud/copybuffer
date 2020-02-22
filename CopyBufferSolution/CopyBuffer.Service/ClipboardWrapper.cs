using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public KeyValuePair<int, string> GetImage(List<int> existingImagesHashes)
        {
            return ClipboardOldSchool.GetImage(existingImagesHashes);
        }

        //public void SetImage(BitmapImage p_image)
        //{
        //    var fff = p_image;
        //}
    }
}
