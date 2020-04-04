﻿using System;
using System.Collections.Generic;

namespace CopyBuffer.Service.Shared
{
    public struct BufferItem
    {
        public DateTime TimeStamp { get; set; }
        public string TextContent { get; set; }
        public KeyValuePair<int, string> ImageContent { get; set; }
        public BufferItemType ItemType { get; set; }
    }

    public enum BufferItemType
    {
        Text,
        Image
    }
}