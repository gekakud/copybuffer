using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

        public static bool IsImageAvailiable()
        {
            if (!IsClipboardFormatAvailable(CF_DIBV5))
                return false;

            return Clipboard.ContainsImage();
        }

        public static KeyValuePair<int, string> GetImage(List<int> existingImagesHashes)
        {
            var newKeyValue = new KeyValuePair<int, string>(0, "");

            if (!IsClipboardFormatAvailable(CF_DIBV5))
                return newKeyValue;
            
            if (!Clipboard.ContainsImage()) return newKeyValue;

            var t = new Thread(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    var source = Clipboard.GetImage();
                    if (source != null)
                    {
                        var encoder = new JpegBitmapEncoder();
                        var encoder2 = new JpegBitmapEncoder();
                        encoder2.Frames.Add(BitmapFrame.Create(source));
                        encoder2.Save(memoryStream);

                        int hashByMemstream = ComputeHash(memoryStream.ToArray());
                        if (!existingImagesHashes.Contains(hashByMemstream))
                        {
                            encoder.Frames.Add(BitmapFrame.Create(source));
                            var path = Environment.CurrentDirectory + DateTime.Now.Ticks + ".png";
                            using (var fileStream = new FileStream(path, System.IO.FileMode.Create))
                            {
                                encoder.Save(fileStream);
                            }

                            newKeyValue = new KeyValuePair<int, string>(hashByMemstream, path);
                        }
                    }
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = false;
            t.Start();
            t.Join();

            return newKeyValue;
        }

        public static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < 500; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}