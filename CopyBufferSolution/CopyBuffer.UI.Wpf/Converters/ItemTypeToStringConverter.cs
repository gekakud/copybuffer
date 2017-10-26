using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using CopyBuffer.Service.Shared;
using MaterialDesignThemes.Wpf;

namespace CopyBuffer.Ui.Wpf.Converters
{
    public class ItemTypeTostringConverer : IValueConverter
    {
        private readonly Dictionary<BufferItemType, PackIconKind> _itemTypeMapper;

        public ItemTypeTostringConverer() : base()
        {
            _itemTypeMapper = new Dictionary<BufferItemType, PackIconKind>
            {
                {BufferItemType.Image, PackIconKind.Image},
                {BufferItemType.Text, PackIconKind.FormatText}
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (BufferItemType)value;

            return _itemTypeMapper.ContainsKey(type) ? _itemTypeMapper[type] : PackIconKind.Biohazard;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
