using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace IFoxCAD.WPF;

/// <summary>
/// 颜色索引到画刷转换器
/// 由于不想引用AutoCAD的包，于是用这种ugly的方式来实现
/// </summary>
public class ColorIndex2SolidColorBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || !int.TryParse(value.ToString(), out var index))
            return Binding.DoNothing;
        index = Math.Abs(index) % 257;
        if (index is 0 or 256)
            index = 7;
        if (colorDict.TryGetValue(index, out var color))
            return color;
        return Binding.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private static readonly Dictionary<int, SolidColorBrush> colorDict = new()
    {
        { 1, new(MediaColor.FromRgb(255, 0, 0)) }, { 2, new(MediaColor.FromRgb(255, 255, 0)) },
        { 3, new(MediaColor.FromRgb(0, 255, 0)) }, { 4, new(MediaColor.FromRgb(0, 255, 255)) },
        { 5, new(MediaColor.FromRgb(0, 0, 255)) }, { 6, new(MediaColor.FromRgb(255, 0, 255)) },
        { 7, new(MediaColor.FromRgb(255, 255, 255)) }, { 8, new(MediaColor.FromRgb(128, 128, 128)) },
        { 9, new(MediaColor.FromRgb(192, 192, 192)) }, { 10, new(MediaColor.FromRgb(255, 0, 0)) },
        { 11, new(MediaColor.FromRgb(255, 127, 127)) }, { 12, new(MediaColor.FromRgb(204, 0, 0)) },
        { 13, new(MediaColor.FromRgb(204, 102, 102)) }, { 14, new(MediaColor.FromRgb(153, 0, 0)) },
        { 15, new(MediaColor.FromRgb(153, 76, 76)) }, { 16, new(MediaColor.FromRgb(127, 0, 0)) },
        { 17, new(MediaColor.FromRgb(127, 63, 63)) }, { 18, new(MediaColor.FromRgb(76, 0, 0)) },
        { 19, new(MediaColor.FromRgb(76, 38, 38)) }, { 20, new(MediaColor.FromRgb(255, 63, 0)) },
        { 21, new(MediaColor.FromRgb(255, 159, 127)) }, { 22, new(MediaColor.FromRgb(204, 51, 0)) },
        { 23, new(MediaColor.FromRgb(204, 127, 102)) }, { 24, new(MediaColor.FromRgb(153, 38, 0)) },
        { 25, new(MediaColor.FromRgb(153, 95, 76)) }, { 26, new(MediaColor.FromRgb(127, 31, 0)) },
        { 27, new(MediaColor.FromRgb(127, 79, 63)) }, { 28, new(MediaColor.FromRgb(76, 19, 0)) },
        { 29, new(MediaColor.FromRgb(76, 47, 38)) }, { 30, new(MediaColor.FromRgb(255, 127, 0)) },
        { 31, new(MediaColor.FromRgb(255, 191, 127)) }, { 32, new(MediaColor.FromRgb(204, 102, 0)) },
        { 33, new(MediaColor.FromRgb(204, 153, 102)) }, { 34, new(MediaColor.FromRgb(153, 76, 0)) },
        { 35, new(MediaColor.FromRgb(153, 114, 76)) }, { 36, new(MediaColor.FromRgb(127, 63, 0)) },
        { 37, new(MediaColor.FromRgb(127, 95, 63)) }, { 38, new(MediaColor.FromRgb(76, 38, 0)) },
        { 39, new(MediaColor.FromRgb(76, 57, 38)) }, { 40, new(MediaColor.FromRgb(255, 191, 0)) },
        { 41, new(MediaColor.FromRgb(255, 223, 127)) }, { 42, new(MediaColor.FromRgb(204, 153, 0)) },
        { 43, new(MediaColor.FromRgb(204, 178, 102)) }, { 44, new(MediaColor.FromRgb(153, 114, 0)) },
        { 45, new(MediaColor.FromRgb(153, 133, 76)) }, { 46, new(MediaColor.FromRgb(127, 95, 0)) },
        { 47, new(MediaColor.FromRgb(127, 111, 63)) }, { 48, new(MediaColor.FromRgb(76, 57, 0)) },
        { 49, new(MediaColor.FromRgb(76, 66, 38)) }, { 50, new(MediaColor.FromRgb(255, 255, 0)) },
        { 51, new(MediaColor.FromRgb(255, 255, 127)) }, { 52, new(MediaColor.FromRgb(204, 204, 0)) },
        { 53, new(MediaColor.FromRgb(204, 204, 102)) }, { 54, new(MediaColor.FromRgb(153, 153, 0)) },
        { 55, new(MediaColor.FromRgb(153, 153, 76)) }, { 56, new(MediaColor.FromRgb(127, 127, 0)) },
        { 57, new(MediaColor.FromRgb(127, 127, 63)) }, { 58, new(MediaColor.FromRgb(76, 76, 0)) },
        { 59, new(MediaColor.FromRgb(76, 76, 38)) }, { 60, new(MediaColor.FromRgb(191, 255, 0)) },
        { 61, new(MediaColor.FromRgb(223, 255, 127)) }, { 62, new(MediaColor.FromRgb(153, 204, 0)) },
        { 63, new(MediaColor.FromRgb(178, 204, 102)) }, { 64, new(MediaColor.FromRgb(114, 153, 0)) },
        { 65, new(MediaColor.FromRgb(133, 153, 76)) }, { 66, new(MediaColor.FromRgb(95, 127, 0)) },
        { 67, new(MediaColor.FromRgb(111, 127, 63)) }, { 68, new(MediaColor.FromRgb(57, 76, 0)) },
        { 69, new(MediaColor.FromRgb(66, 76, 38)) }, { 70, new(MediaColor.FromRgb(127, 255, 0)) },
        { 71, new(MediaColor.FromRgb(191, 255, 127)) }, { 72, new(MediaColor.FromRgb(102, 204, 0)) },
        { 73, new(MediaColor.FromRgb(153, 204, 102)) }, { 74, new(MediaColor.FromRgb(76, 153, 0)) },
        { 75, new(MediaColor.FromRgb(114, 153, 76)) }, { 76, new(MediaColor.FromRgb(63, 127, 0)) },
        { 77, new(MediaColor.FromRgb(95, 127, 63)) }, { 78, new(MediaColor.FromRgb(38, 76, 0)) },
        { 79, new(MediaColor.FromRgb(57, 76, 38)) }, { 80, new(MediaColor.FromRgb(63, 255, 0)) },
        { 81, new(MediaColor.FromRgb(159, 255, 127)) }, { 82, new(MediaColor.FromRgb(51, 204, 0)) },
        { 83, new(MediaColor.FromRgb(127, 204, 102)) }, { 84, new(MediaColor.FromRgb(38, 153, 0)) },
        { 85, new(MediaColor.FromRgb(95, 153, 76)) }, { 86, new(MediaColor.FromRgb(31, 127, 0)) },
        { 87, new(MediaColor.FromRgb(79, 127, 63)) }, { 88, new(MediaColor.FromRgb(19, 76, 0)) },
        { 89, new(MediaColor.FromRgb(47, 76, 38)) }, { 90, new(MediaColor.FromRgb(0, 255, 0)) },
        { 91, new(MediaColor.FromRgb(127, 255, 127)) }, { 92, new(MediaColor.FromRgb(0, 204, 0)) },
        { 93, new(MediaColor.FromRgb(102, 204, 102)) }, { 94, new(MediaColor.FromRgb(0, 153, 0)) },
        { 95, new(MediaColor.FromRgb(76, 153, 76)) }, { 96, new(MediaColor.FromRgb(0, 127, 0)) },
        { 97, new(MediaColor.FromRgb(63, 127, 63)) }, { 98, new(MediaColor.FromRgb(0, 76, 0)) },
        { 99, new(MediaColor.FromRgb(38, 76, 38)) }, { 100, new(MediaColor.FromRgb(0, 255, 63)) },
        { 101, new(MediaColor.FromRgb(127, 255, 159)) }, { 102, new(MediaColor.FromRgb(0, 204, 51)) },
        { 103, new(MediaColor.FromRgb(102, 204, 127)) }, { 104, new(MediaColor.FromRgb(0, 153, 38)) },
        { 105, new(MediaColor.FromRgb(76, 153, 95)) }, { 106, new(MediaColor.FromRgb(0, 127, 31)) },
        { 107, new(MediaColor.FromRgb(63, 127, 79)) }, { 108, new(MediaColor.FromRgb(0, 76, 19)) },
        { 109, new(MediaColor.FromRgb(38, 76, 47)) }, { 110, new(MediaColor.FromRgb(0, 255, 127)) },
        { 111, new(MediaColor.FromRgb(127, 255, 191)) }, { 112, new(MediaColor.FromRgb(0, 204, 102)) },
        { 113, new(MediaColor.FromRgb(102, 204, 153)) }, { 114, new(MediaColor.FromRgb(0, 153, 76)) },
        { 115, new(MediaColor.FromRgb(76, 153, 114)) }, { 116, new(MediaColor.FromRgb(0, 127, 63)) },
        { 117, new(MediaColor.FromRgb(63, 127, 95)) }, { 118, new(MediaColor.FromRgb(0, 76, 38)) },
        { 119, new(MediaColor.FromRgb(38, 76, 57)) }, { 120, new(MediaColor.FromRgb(0, 255, 191)) },
        { 121, new(MediaColor.FromRgb(127, 255, 223)) }, { 122, new(MediaColor.FromRgb(0, 204, 153)) },
        { 123, new(MediaColor.FromRgb(102, 204, 178)) }, { 124, new(MediaColor.FromRgb(0, 153, 114)) },
        { 125, new(MediaColor.FromRgb(76, 153, 133)) }, { 126, new(MediaColor.FromRgb(0, 127, 95)) },
        { 127, new(MediaColor.FromRgb(63, 127, 111)) }, { 128, new(MediaColor.FromRgb(0, 76, 57)) },
        { 129, new(MediaColor.FromRgb(38, 76, 66)) }, { 130, new(MediaColor.FromRgb(0, 255, 255)) },
        { 131, new(MediaColor.FromRgb(127, 255, 255)) }, { 132, new(MediaColor.FromRgb(0, 204, 204)) },
        { 133, new(MediaColor.FromRgb(102, 204, 204)) }, { 134, new(MediaColor.FromRgb(0, 153, 153)) },
        { 135, new(MediaColor.FromRgb(76, 153, 153)) }, { 136, new(MediaColor.FromRgb(0, 127, 127)) },
        { 137, new(MediaColor.FromRgb(63, 127, 127)) }, { 138, new(MediaColor.FromRgb(0, 76, 76)) },
        { 139, new(MediaColor.FromRgb(38, 76, 76)) }, { 140, new(MediaColor.FromRgb(0, 191, 255)) },
        { 141, new(MediaColor.FromRgb(127, 223, 255)) }, { 142, new(MediaColor.FromRgb(0, 153, 204)) },
        { 143, new(MediaColor.FromRgb(102, 178, 204)) }, { 144, new(MediaColor.FromRgb(0, 114, 153)) },
        { 145, new(MediaColor.FromRgb(76, 133, 153)) }, { 146, new(MediaColor.FromRgb(0, 95, 127)) },
        { 147, new(MediaColor.FromRgb(63, 111, 127)) }, { 148, new(MediaColor.FromRgb(0, 57, 76)) },
        { 149, new(MediaColor.FromRgb(38, 66, 76)) }, { 150, new(MediaColor.FromRgb(0, 127, 255)) },
        { 151, new(MediaColor.FromRgb(127, 191, 255)) }, { 152, new(MediaColor.FromRgb(0, 102, 204)) },
        { 153, new(MediaColor.FromRgb(102, 153, 204)) }, { 154, new(MediaColor.FromRgb(0, 76, 153)) },
        { 155, new(MediaColor.FromRgb(76, 114, 153)) }, { 156, new(MediaColor.FromRgb(0, 63, 127)) },
        { 157, new(MediaColor.FromRgb(63, 95, 127)) }, { 158, new(MediaColor.FromRgb(0, 38, 76)) },
        { 159, new(MediaColor.FromRgb(38, 57, 76)) }, { 160, new(MediaColor.FromRgb(0, 63, 255)) },
        { 161, new(MediaColor.FromRgb(127, 159, 255)) }, { 162, new(MediaColor.FromRgb(0, 51, 204)) },
        { 163, new(MediaColor.FromRgb(102, 127, 204)) }, { 164, new(MediaColor.FromRgb(0, 38, 153)) },
        { 165, new(MediaColor.FromRgb(76, 95, 153)) }, { 166, new(MediaColor.FromRgb(0, 31, 127)) },
        { 167, new(MediaColor.FromRgb(63, 79, 127)) }, { 168, new(MediaColor.FromRgb(0, 19, 76)) },
        { 169, new(MediaColor.FromRgb(38, 47, 76)) }, { 170, new(MediaColor.FromRgb(0, 0, 255)) },
        { 171, new(MediaColor.FromRgb(127, 127, 255)) }, { 172, new(MediaColor.FromRgb(0, 0, 204)) },
        { 173, new(MediaColor.FromRgb(102, 102, 204)) }, { 174, new(MediaColor.FromRgb(0, 0, 153)) },
        { 175, new(MediaColor.FromRgb(76, 76, 153)) }, { 176, new(MediaColor.FromRgb(0, 0, 127)) },
        { 177, new(MediaColor.FromRgb(63, 63, 127)) }, { 178, new(MediaColor.FromRgb(0, 0, 76)) },
        { 179, new(MediaColor.FromRgb(38, 38, 76)) }, { 180, new(MediaColor.FromRgb(63, 0, 255)) },
        { 181, new(MediaColor.FromRgb(159, 127, 255)) }, { 182, new(MediaColor.FromRgb(51, 0, 204)) },
        { 183, new(MediaColor.FromRgb(127, 102, 204)) }, { 184, new(MediaColor.FromRgb(38, 0, 153)) },
        { 185, new(MediaColor.FromRgb(95, 76, 153)) }, { 186, new(MediaColor.FromRgb(31, 0, 127)) },
        { 187, new(MediaColor.FromRgb(79, 63, 127)) }, { 188, new(MediaColor.FromRgb(19, 0, 76)) },
        { 189, new(MediaColor.FromRgb(47, 38, 76)) }, { 190, new(MediaColor.FromRgb(127, 0, 255)) },
        { 191, new(MediaColor.FromRgb(191, 127, 255)) }, { 192, new(MediaColor.FromRgb(102, 0, 204)) },
        { 193, new(MediaColor.FromRgb(153, 102, 204)) }, { 194, new(MediaColor.FromRgb(76, 0, 153)) },
        { 195, new(MediaColor.FromRgb(114, 76, 153)) }, { 196, new(MediaColor.FromRgb(63, 0, 127)) },
        { 197, new(MediaColor.FromRgb(95, 63, 127)) }, { 198, new(MediaColor.FromRgb(38, 0, 76)) },
        { 199, new(MediaColor.FromRgb(57, 38, 76)) }, { 200, new(MediaColor.FromRgb(191, 0, 255)) },
        { 201, new(MediaColor.FromRgb(223, 127, 255)) }, { 202, new(MediaColor.FromRgb(153, 0, 204)) },
        { 203, new(MediaColor.FromRgb(178, 102, 204)) }, { 204, new(MediaColor.FromRgb(114, 0, 153)) },
        { 205, new(MediaColor.FromRgb(133, 76, 153)) }, { 206, new(MediaColor.FromRgb(95, 0, 127)) },
        { 207, new(MediaColor.FromRgb(111, 63, 127)) }, { 208, new(MediaColor.FromRgb(57, 0, 76)) },
        { 209, new(MediaColor.FromRgb(66, 38, 76)) }, { 210, new(MediaColor.FromRgb(255, 0, 255)) },
        { 211, new(MediaColor.FromRgb(255, 127, 255)) }, { 212, new(MediaColor.FromRgb(204, 0, 204)) },
        { 213, new(MediaColor.FromRgb(204, 102, 204)) }, { 214, new(MediaColor.FromRgb(153, 0, 153)) },
        { 215, new(MediaColor.FromRgb(153, 76, 153)) }, { 216, new(MediaColor.FromRgb(127, 0, 127)) },
        { 217, new(MediaColor.FromRgb(127, 63, 127)) }, { 218, new(MediaColor.FromRgb(76, 0, 76)) },
        { 219, new(MediaColor.FromRgb(76, 38, 76)) }, { 220, new(MediaColor.FromRgb(255, 0, 191)) },
        { 221, new(MediaColor.FromRgb(255, 127, 223)) }, { 222, new(MediaColor.FromRgb(204, 0, 153)) },
        { 223, new(MediaColor.FromRgb(204, 102, 178)) }, { 224, new(MediaColor.FromRgb(153, 0, 114)) },
        { 225, new(MediaColor.FromRgb(153, 76, 133)) }, { 226, new(MediaColor.FromRgb(127, 0, 95)) },
        { 227, new(MediaColor.FromRgb(127, 63, 111)) }, { 228, new(MediaColor.FromRgb(76, 0, 57)) },
        { 229, new(MediaColor.FromRgb(76, 38, 66)) }, { 230, new(MediaColor.FromRgb(255, 0, 127)) },
        { 231, new(MediaColor.FromRgb(255, 127, 191)) }, { 232, new(MediaColor.FromRgb(204, 0, 102)) },
        { 233, new(MediaColor.FromRgb(204, 102, 153)) }, { 234, new(MediaColor.FromRgb(153, 0, 76)) },
        { 235, new(MediaColor.FromRgb(153, 76, 114)) }, { 236, new(MediaColor.FromRgb(127, 0, 63)) },
        { 237, new(MediaColor.FromRgb(127, 63, 95)) }, { 238, new(MediaColor.FromRgb(76, 0, 38)) },
        { 239, new(MediaColor.FromRgb(76, 38, 57)) }, { 240, new(MediaColor.FromRgb(255, 0, 63)) },
        { 241, new(MediaColor.FromRgb(255, 127, 159)) }, { 242, new(MediaColor.FromRgb(204, 0, 51)) },
        { 243, new(MediaColor.FromRgb(204, 102, 127)) }, { 244, new(MediaColor.FromRgb(153, 0, 38)) },
        { 245, new(MediaColor.FromRgb(153, 76, 95)) }, { 246, new(MediaColor.FromRgb(127, 0, 31)) },
        { 247, new(MediaColor.FromRgb(127, 63, 79)) }, { 248, new(MediaColor.FromRgb(76, 0, 19)) },
        { 249, new(MediaColor.FromRgb(76, 38, 47)) }, { 250, new(MediaColor.FromRgb(51, 51, 51)) },
        { 251, new(MediaColor.FromRgb(91, 91, 91)) }, { 252, new(MediaColor.FromRgb(132, 132, 132)) },
        { 253, new(MediaColor.FromRgb(173, 173, 173)) }, { 254, new(MediaColor.FromRgb(214, 214, 214)) },
        { 255, new(MediaColor.FromRgb(255, 255, 255)) }
    };
}