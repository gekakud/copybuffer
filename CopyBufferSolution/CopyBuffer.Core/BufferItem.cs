using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace CopyBuffer.Core
{
    public struct BufferItem
    {
        public DateTime TimeStamp { get; set; }
        public string TextContent { get; set; }
        public BitmapImage ImageContent { get; set; }
        public BufferItemType ItemType { get; set; }
    }

    public enum BufferItemType
    {
        Text,
        Image
    }

    public static class BitmapImageExtensions
    {
        public static bool IsEqual(this BitmapImage image1, BitmapImage image2)
        {
            if (image1 == null || image2 == null)
            {
                return false;
            }
            return image1.ToBytes().SequenceEqual(image2.ToBytes());
        }

        public static byte[] ToBytes(this BitmapImage image)
        {
            byte[] data = new byte[] { };
            if (image != null)
            {
                try
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        data = ms.ToArray();
                        return data;
                    }
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return data;
        }
    }
}
