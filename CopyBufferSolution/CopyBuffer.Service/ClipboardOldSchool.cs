using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using CopyBuffer.Service.Shared;

namespace CopyBuffer.Service
{
    internal static class ClipboardOldSchool
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll")]
        static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();
        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        static extern bool GlobalUnlock(IntPtr hMem);

        const uint CF_UNICODETEXT = 13;
        private const uint CF_DIBV5 = 17;//bitmap

        private static Dictionary<uint,bool> formatMap = new Dictionary<uint, bool>();

        static ClipboardOldSchool()
        {
            for (uint i = 0; i < 18; i++)
            {
                formatMap.Add(i, IsClipboardFormatAvailable(i));
            }
        }

        public static string GetText()
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;
            if (!OpenClipboard(IntPtr.Zero))
                return null;

            string data = null;
            var hGlobal = GetClipboardData(CF_UNICODETEXT);
            if (hGlobal != IntPtr.Zero)
            {
                var lpwcstr = GlobalLock(hGlobal);
                if (lpwcstr != IntPtr.Zero)
                {
                    data = Marshal.PtrToStringUni(lpwcstr);
                    GlobalUnlock(lpwcstr);
                }
            }
            CloseClipboard();

            return data;
        }

        public static BitmapImage GetImage()
        {
            if (!IsClipboardFormatAvailable(CF_DIBV5))
                return null;
            
            BitmapImage image = null;
//            if (!Clipboard.ContainsImage()) return image;
//
//            var t = new Thread((ThreadStart)(() => {
//                var encoder = new JpegBitmapEncoder();
//                var memoryStream = new MemoryStream();
//                var bImg = new BitmapImage();
//
//                encoder.Frames.Add(BitmapFrame.Create(Clipboard.GetImage()));
//                encoder.Save(memoryStream);
//
//                memoryStream.Position = 0;
//                bImg.BeginInit();
//                bImg.StreamSource = memoryStream;
//                bImg.EndInit();
//
//                memoryStream.Close();
//                image = bImg;
//            }));
//
//            t.SetApartmentState(ApartmentState.MTA);
//            t.Start();
//            t.Join();

            return image;
        }

        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}