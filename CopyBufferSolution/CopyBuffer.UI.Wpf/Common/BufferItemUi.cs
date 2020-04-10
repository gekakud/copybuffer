using System;
using System.Collections.Generic;
using CopyBuffer.Service.Shared;

namespace CopyBuffer.Ui.Wpf.Common
{
    public class BufferItemUi
    {
        public DateTime TimeStamp { get; set; }
        public string TextContent { get; set; }
        public KeyValuePair<int, string> ImageContent { get; set; }
        public BufferItemType ItemType { get; set; }
    }
}