using System;
using System.Diagnostics;
using System.Drawing;
using WK.Libraries.SharpClipboardNS;

namespace CopyBuffer.Service
{
    internal class SharpClipWrapper:IDisposable
    {
        public string MostRecentClipboardText
        {
            get
            {
                lock (lockObject)
                {
                    return _mostRecentClipboardText;
                }
            }
        }

        private SharpClipboard clipboard;
        private string _mostRecentClipboardText;
        private object lockObject = new object();

        public SharpClipWrapper()
        {
            clipboard = new SharpClipboard();

            // Attach your code to the ClipboardChanged event to listen to cuts/copies.
            clipboard.ClipboardChanged += ClipboardChanged;
        }
    
        private void ClipboardChanged(Object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                lock (lockObject)
                {
                    _mostRecentClipboardText = clipboard.ClipboardText;
                }
                // Get the cut/copied text.
                //Debug.WriteLine(clipboard.ClipboardText);
            }

            // Is the content copied of image type?
            else if (e.ContentType == SharpClipboard.ContentTypes.Image)
            {
                // Get the cut/copied image.
                //Image img = clipboard.ClipboardImage;
            }

            // Is the content copied of file type?
            else if (e.ContentType == SharpClipboard.ContentTypes.Files)
            {
                // Get the cut/copied file/files.
                //Debug.WriteLine(clipboard.ClipboardFiles.ToArray());

                // ...or use 'ClipboardFile' to get a single copied file.
                //Debug.WriteLine(clipboard.ClipboardFile);
            }

            // If the cut/copied content is complex, use 'Other'.
            else if (e.ContentType == SharpClipboard.ContentTypes.Other)
            {
                // Do something with 'clipboard.ClipboardObject' or 'e.Content' here...
            }
        }

        public void Dispose()
        {
            clipboard.ClipboardChanged -= ClipboardChanged;
            clipboard?.Dispose();
        }
    }
}
