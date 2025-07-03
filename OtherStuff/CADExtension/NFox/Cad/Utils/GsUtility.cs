using System;
using System.Drawing;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;



namespace NFox.Cad
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class GsUtility
    {
        public const string strActive = "*Active";
        public const string strActiveSettings = "ACAD_RENDER_ACTIVE_SETTINGS";

        public static void CustomUpdate(System.IntPtr parmeter, int left, int right, int bottom, int top)
        {
            MessageBox.Show("Left:" + left + "Right" + right + "Bottom" + bottom + "Top" + top);
        }

        public static System.Drawing.Color[] MyAcadColorPs =
            {
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 0 - lets make it red for an example
              //{255, 255, 255, 255},//----- 0 - ByBlock - White
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 1 - Red
              System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 2 - Yellow
              System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 3 - Green
              System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 4 - Cyan
              System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 5 - Blue
              System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 6 - Magenta
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 7 - More red Red
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 8 - More red Red
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 9 - More red Red
              /*System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 7 - White
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 8
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 9*/
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 10
              System.Drawing.Color.FromArgb(255, 127, 127, 255),//----- 11
              System.Drawing.Color.FromArgb(165, 0, 0, 255),    //----- 12
              System.Drawing.Color.FromArgb(165, 82, 82, 255),    //----- 13
              System.Drawing.Color.FromArgb(127, 0, 0, 255),    //----- 14
              System.Drawing.Color.FromArgb(127, 63, 63, 255),    //----- 15
              System.Drawing.Color.FromArgb(76, 0, 0, 255),        //----- 16
              System.Drawing.Color.FromArgb(76, 38, 38, 255),    //----- 17
              System.Drawing.Color.FromArgb(38, 0, 0, 255),        //----- 18
              System.Drawing.Color.FromArgb(38, 19, 19, 255),    //----- 19
              System.Drawing.Color.FromArgb(255, 63, 0, 255),    //----- 20
              System.Drawing.Color.FromArgb(255, 159, 127, 255),//----- 21
              System.Drawing.Color.FromArgb(165, 41, 0, 255),    //----- 22
              System.Drawing.Color.FromArgb(165, 103, 82, 255),    //----- 23
              System.Drawing.Color.FromArgb(127, 31, 0, 255),    //----- 24
              System.Drawing.Color.FromArgb(127, 79, 63, 255),    //----- 25
              System.Drawing.Color.FromArgb(76, 19, 0, 255),    //----- 26
              System.Drawing.Color.FromArgb(76, 47, 38, 255),    //----- 27
              System.Drawing.Color.FromArgb(38, 9, 0, 255),        //----- 28
              System.Drawing.Color.FromArgb(38, 23, 19, 255),    //----- 29
              System.Drawing.Color.FromArgb(255, 127, 0, 255),    //----- 30
              System.Drawing.Color.FromArgb(255, 191, 127, 255),//----- 31
              System.Drawing.Color.FromArgb(165, 82, 0, 255),    //----- 32
              System.Drawing.Color.FromArgb(165, 124, 82, 255),    //----- 33
              System.Drawing.Color.FromArgb(127, 63, 0, 255),    //----- 34
              System.Drawing.Color.FromArgb(127, 95, 63, 255),    //----- 35
              System.Drawing.Color.FromArgb(76, 38, 0, 255),    //----- 36
              System.Drawing.Color.FromArgb(76, 57, 38, 255),    //----- 37
              System.Drawing.Color.FromArgb(38, 19, 0, 255),    //----- 38
              System.Drawing.Color.FromArgb(38, 28, 19, 255),    //----- 39
              System.Drawing.Color.FromArgb(255, 191, 0, 255),    //----- 40
              System.Drawing.Color.FromArgb(255, 223, 127, 255),//----- 41
              System.Drawing.Color.FromArgb(165, 124, 0, 255),    //----- 42
              System.Drawing.Color.FromArgb(165, 145, 82, 255),    //----- 43
              System.Drawing.Color.FromArgb(127, 95, 0, 255),    //----- 44
              System.Drawing.Color.FromArgb(127, 111, 63, 255),    //----- 45
              System.Drawing.Color.FromArgb(76, 57, 0, 255),    //----- 46
              System.Drawing.Color.FromArgb(76, 66, 38, 255),    //----- 47
              System.Drawing.Color.FromArgb(38, 28, 0, 255),    //----- 48
              System.Drawing.Color.FromArgb(38, 33, 19, 255),    //----- 49
              System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 50
              System.Drawing.Color.FromArgb(255, 255, 127, 255),//----- 51
              System.Drawing.Color.FromArgb(165, 165, 0, 255),    //----- 52
              System.Drawing.Color.FromArgb(165, 165, 82, 255),    //----- 53
              System.Drawing.Color.FromArgb(127, 127, 0, 255),    //----- 54
              System.Drawing.Color.FromArgb(127, 127, 63, 255),    //----- 55
              System.Drawing.Color.FromArgb(76, 76, 0, 255),    //----- 56
              System.Drawing.Color.FromArgb(76, 76, 38, 255),    //----- 57
              System.Drawing.Color.FromArgb(38, 38, 0, 255),    //----- 58
              System.Drawing.Color.FromArgb(38, 38, 19, 255),    //----- 59
              System.Drawing.Color.FromArgb(191, 255, 0, 255),    //----- 60
              System.Drawing.Color.FromArgb(223, 255, 127, 255),//----- 61
              System.Drawing.Color.FromArgb(124, 165, 0, 255),    //----- 62
              System.Drawing.Color.FromArgb(145, 165, 82, 255),    //----- 63
              System.Drawing.Color.FromArgb(95, 127, 0, 255),    //----- 64
              System.Drawing.Color.FromArgb(111, 127, 63, 255),    //----- 65
              System.Drawing.Color.FromArgb(57, 76, 0, 255),    //----- 66
              System.Drawing.Color.FromArgb(66, 76, 38, 255),    //----- 67
              System.Drawing.Color.FromArgb(28, 38, 0, 255),    //----- 68
              System.Drawing.Color.FromArgb(33, 38, 19, 255),    //----- 69
              System.Drawing.Color.FromArgb(127, 255, 0, 255),    //----- 70
              System.Drawing.Color.FromArgb(191, 255, 127, 255),//----- 71
              System.Drawing.Color.FromArgb(82, 165, 0, 255),    //----- 72
              System.Drawing.Color.FromArgb(124, 165, 82, 255),    //----- 73
              System.Drawing.Color.FromArgb(63, 127, 0, 255),    //----- 74
              System.Drawing.Color.FromArgb(95, 127, 63, 255),    //----- 75
              System.Drawing.Color.FromArgb(38, 76, 0, 255),    //----- 76
              System.Drawing.Color.FromArgb(57, 76, 38, 255),    //----- 77
              System.Drawing.Color.FromArgb(19, 38, 0, 255),    //----- 78
              System.Drawing.Color.FromArgb(28, 38, 19, 255),    //----- 79
              System.Drawing.Color.FromArgb(63, 255, 0, 255),    //----- 80
              System.Drawing.Color.FromArgb(159, 255, 127, 255),//----- 81
              System.Drawing.Color.FromArgb(41, 165, 0, 255),    //----- 82
              System.Drawing.Color.FromArgb(103, 165, 82, 255),    //----- 83
              System.Drawing.Color.FromArgb(31, 127, 0, 255),    //----- 84
              System.Drawing.Color.FromArgb(79, 127, 63, 255),    //----- 85
              System.Drawing.Color.FromArgb(19, 76, 0, 255),    //----- 86
              System.Drawing.Color.FromArgb(47, 76, 38, 255),    //----- 87
              System.Drawing.Color.FromArgb(9, 38, 0, 255),        //----- 88
              System.Drawing.Color.FromArgb(23, 38, 19, 255),    //----- 89
              System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 90
              System.Drawing.Color.FromArgb(127, 255, 127, 255),//----- 91
              System.Drawing.Color.FromArgb(0, 165, 0, 255),    //----- 92
              System.Drawing.Color.FromArgb(82, 165, 82, 255),    //----- 93
              System.Drawing.Color.FromArgb(0, 127, 0, 255),    //----- 94
              System.Drawing.Color.FromArgb(63, 127, 63, 255),    //----- 95
              System.Drawing.Color.FromArgb(0, 76, 0, 255),        //----- 96
              System.Drawing.Color.FromArgb(38, 76, 38, 255),    //----- 97
              System.Drawing.Color.FromArgb(0, 38, 0, 255),        //----- 98
              System.Drawing.Color.FromArgb(19, 38, 19, 255),    //----- 99
              System.Drawing.Color.FromArgb(0, 255, 63, 255),    //----- 100
              System.Drawing.Color.FromArgb(127, 255, 159, 255),//----- 101
              System.Drawing.Color.FromArgb(0, 165, 41, 255),    //----- 102
              System.Drawing.Color.FromArgb(82, 165, 103, 255),    //----- 103
              System.Drawing.Color.FromArgb(0, 127, 31, 255),    //----- 104
              System.Drawing.Color.FromArgb(63, 127, 79, 255),    //----- 105
              System.Drawing.Color.FromArgb(0, 76, 19, 255),    //----- 106
              System.Drawing.Color.FromArgb(38, 76, 47, 255),    //----- 107
              System.Drawing.Color.FromArgb(0, 38, 9, 255),        //----- 108
              System.Drawing.Color.FromArgb(19, 38, 23, 255),    //----- 109
              System.Drawing.Color.FromArgb(0, 255, 127, 255),    //----- 110
              System.Drawing.Color.FromArgb(127, 255, 191, 255),//----- 111
              System.Drawing.Color.FromArgb(0, 165, 82, 255),    //----- 112
              System.Drawing.Color.FromArgb(82, 165, 124, 255),    //----- 113
              System.Drawing.Color.FromArgb(0, 127, 63, 255),    //----- 114
              System.Drawing.Color.FromArgb(63, 127, 95, 255),    //----- 115
              System.Drawing.Color.FromArgb(0, 76, 38, 255),    //----- 116
              System.Drawing.Color.FromArgb(38, 76, 57, 255),    //----- 117
              System.Drawing.Color.FromArgb(0, 38, 19, 255),    //----- 118
              System.Drawing.Color.FromArgb(19, 38, 28, 255),    //----- 119
              System.Drawing.Color.FromArgb(0, 255, 191, 255),    //----- 120
              System.Drawing.Color.FromArgb(127, 255, 223, 255),//----- 121
              System.Drawing.Color.FromArgb(0, 165, 124, 255),    //----- 122
              System.Drawing.Color.FromArgb(82, 165, 145, 255),    //----- 123
              System.Drawing.Color.FromArgb(0, 127, 95, 255),    //----- 124
              System.Drawing.Color.FromArgb(63, 127, 111, 255),    //----- 125
              System.Drawing.Color.FromArgb(0, 76, 57, 255),    //----- 126
              System.Drawing.Color.FromArgb(38, 76, 66, 255),    //----- 127
              System.Drawing.Color.FromArgb(0, 38, 28, 255),    //----- 128
              System.Drawing.Color.FromArgb(19, 38, 33, 255),    //----- 129
              System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 130
              System.Drawing.Color.FromArgb(127, 255, 255, 255),//----- 131
              System.Drawing.Color.FromArgb(0, 165, 165, 255),    //----- 132
              System.Drawing.Color.FromArgb(82, 165, 165, 255),    //----- 133
              System.Drawing.Color.FromArgb(0, 127, 127, 255),    //----- 134
              System.Drawing.Color.FromArgb(63, 127, 127, 255),    //----- 135
              System.Drawing.Color.FromArgb(0, 76, 76, 255),    //----- 136
              System.Drawing.Color.FromArgb(38, 76, 76, 255),    //----- 137
              System.Drawing.Color.FromArgb(0, 38, 38, 255),    //----- 138
              System.Drawing.Color.FromArgb(19, 38, 38, 255),    //----- 139
              System.Drawing.Color.FromArgb(0, 191, 255, 255),    //----- 140
              System.Drawing.Color.FromArgb(127, 223, 255, 255),//----- 141
              System.Drawing.Color.FromArgb(0, 124, 165, 255),    //----- 142
              System.Drawing.Color.FromArgb(82, 145, 165, 255),    //----- 143
              System.Drawing.Color.FromArgb(0, 95, 127, 255),    //----- 144
              System.Drawing.Color.FromArgb(63, 111, 127, 255),    //----- 145
              System.Drawing.Color.FromArgb(0, 57, 76, 255),    //----- 146
              System.Drawing.Color.FromArgb(38, 66, 76, 255),    //----- 147
              System.Drawing.Color.FromArgb(0, 28, 38, 255),    //----- 148
              System.Drawing.Color.FromArgb(19, 33, 38, 255),    //----- 149
              System.Drawing.Color.FromArgb(0, 127, 255, 255),    //----- 150
              System.Drawing.Color.FromArgb(127, 191, 255, 255),//----- 151
              System.Drawing.Color.FromArgb(0, 82, 165, 255),    //----- 152
              System.Drawing.Color.FromArgb(82, 124, 165, 255),    //----- 153
              System.Drawing.Color.FromArgb(0, 63, 127, 255),    //----- 154
              System.Drawing.Color.FromArgb(63, 95, 127, 255),    //----- 155
              System.Drawing.Color.FromArgb(0, 38, 76, 255),    //----- 156
              System.Drawing.Color.FromArgb(38, 57, 76, 255),    //----- 157
              System.Drawing.Color.FromArgb(0, 19, 38, 255),    //----- 158
              System.Drawing.Color.FromArgb(19, 28, 38, 255),    //----- 159
              System.Drawing.Color.FromArgb(0, 63, 255, 255),    //----- 160
              System.Drawing.Color.FromArgb(127, 159, 255, 255),//----- 161
              System.Drawing.Color.FromArgb(0, 41, 165, 255),    //----- 162
              System.Drawing.Color.FromArgb(82, 103, 165, 255),    //----- 163
              System.Drawing.Color.FromArgb(0, 31, 127, 255),    //----- 164
              System.Drawing.Color.FromArgb(63, 79, 127, 255),    //----- 165
              System.Drawing.Color.FromArgb(0, 19, 76, 255),    //----- 166
              System.Drawing.Color.FromArgb(38, 47, 76, 255),    //----- 167
              System.Drawing.Color.FromArgb(0, 9, 38, 255),        //----- 168
              System.Drawing.Color.FromArgb(19, 23, 38, 255),    //----- 169
              System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 170
              System.Drawing.Color.FromArgb(127, 127, 255, 255),//----- 171
              System.Drawing.Color.FromArgb(0, 0, 165, 255),    //----- 172
              System.Drawing.Color.FromArgb(82, 82, 165, 255),    //----- 173
              System.Drawing.Color.FromArgb(0, 0, 127, 255),    //----- 174
              System.Drawing.Color.FromArgb(63, 63, 127, 255),    //----- 175
              System.Drawing.Color.FromArgb(0, 0, 76, 255),        //----- 176
              System.Drawing.Color.FromArgb(38, 38, 76, 255),    //----- 177
              System.Drawing.Color.FromArgb(0, 0, 38, 255),        //----- 178
              System.Drawing.Color.FromArgb(19, 19, 38, 255),    //----- 179
              System.Drawing.Color.FromArgb(63, 0, 255, 255),    //----- 180
              System.Drawing.Color.FromArgb(159, 127, 255, 255),//----- 181
              System.Drawing.Color.FromArgb(41, 0, 165, 255),    //----- 182
              System.Drawing.Color.FromArgb(103, 82, 165, 255),    //----- 183
              System.Drawing.Color.FromArgb(31, 0, 127, 255),    //----- 184
              System.Drawing.Color.FromArgb(79, 63, 127, 255),    //----- 185
              System.Drawing.Color.FromArgb(19, 0, 76, 255),    //----- 186
              System.Drawing.Color.FromArgb(47, 38, 76, 255),    //----- 187
              System.Drawing.Color.FromArgb(9, 0, 38, 255),        //----- 188
              System.Drawing.Color.FromArgb(23, 19, 38, 255),    //----- 189
              System.Drawing.Color.FromArgb(127, 0, 255, 255),    //----- 190
              System.Drawing.Color.FromArgb(191, 127, 255, 255),//----- 191
              System.Drawing.Color.FromArgb(82, 0, 165, 255),    //----- 192
              System.Drawing.Color.FromArgb(124, 82, 165, 255),    //----- 193
              System.Drawing.Color.FromArgb(63, 0, 127, 255),    //----- 194
              System.Drawing.Color.FromArgb(95, 63, 127, 255),    //----- 195
              System.Drawing.Color.FromArgb(38, 0, 76, 255),    //----- 196
              System.Drawing.Color.FromArgb(57, 38, 76, 255),    //----- 197
              System.Drawing.Color.FromArgb(19, 0, 38, 255),    //----- 198
              System.Drawing.Color.FromArgb(28, 19, 38, 255),    //----- 199
              System.Drawing.Color.FromArgb(191, 0, 255, 255),    //----- 200
              System.Drawing.Color.FromArgb(223, 127, 255, 255),//----- 201
              System.Drawing.Color.FromArgb(124, 0, 165, 255),    //----- 202
              System.Drawing.Color.FromArgb(145, 82, 165, 255),    //----- 203
              System.Drawing.Color.FromArgb(95, 0, 127, 255),    //----- 204
              System.Drawing.Color.FromArgb(111, 63, 127, 255),    //----- 205
              System.Drawing.Color.FromArgb(57, 0, 76, 255),    //----- 206
              System.Drawing.Color.FromArgb(66, 38, 76, 255),    //----- 207
              System.Drawing.Color.FromArgb(28, 0, 38, 255),    //----- 208
              System.Drawing.Color.FromArgb(33, 19, 38, 255),    //----- 209
              System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 210
              System.Drawing.Color.FromArgb(255, 127, 255, 255),//----- 211
              System.Drawing.Color.FromArgb(165, 0, 165, 255),    //----- 212
              System.Drawing.Color.FromArgb(165, 82, 165, 255),    //----- 213
              System.Drawing.Color.FromArgb(127, 0, 127, 255),    //----- 214
              System.Drawing.Color.FromArgb(127, 63, 127, 255),    //----- 215
              System.Drawing.Color.FromArgb(76, 0, 76, 255),    //----- 216
              System.Drawing.Color.FromArgb(76, 38, 76, 255),    //----- 217
              System.Drawing.Color.FromArgb(38, 0, 38, 255),    //----- 218
              System.Drawing.Color.FromArgb(38, 19, 38, 255),    //----- 219
              System.Drawing.Color.FromArgb(255, 0, 191, 255),    //----- 220
              System.Drawing.Color.FromArgb(255, 127, 223, 255),//----- 221
              System.Drawing.Color.FromArgb(165, 0, 124, 255),    //----- 222
              System.Drawing.Color.FromArgb(165, 82, 145, 255),    //----- 223
              System.Drawing.Color.FromArgb(127, 0, 95, 255),    //----- 224
              System.Drawing.Color.FromArgb(127, 63, 111, 255),    //----- 225
              System.Drawing.Color.FromArgb(76, 0, 57, 255),    //----- 226
              System.Drawing.Color.FromArgb(76, 38, 66, 255),    //----- 227
              System.Drawing.Color.FromArgb(38, 0, 28, 255),    //----- 228
              System.Drawing.Color.FromArgb(38, 19, 33, 255),    //----- 229
              System.Drawing.Color.FromArgb(255, 0, 127, 255),    //----- 230
              System.Drawing.Color.FromArgb(255, 127, 191, 255),//----- 231
              System.Drawing.Color.FromArgb(165, 0, 82, 255),    //----- 232
              System.Drawing.Color.FromArgb(165, 82, 124, 255),    //----- 233
              System.Drawing.Color.FromArgb(127, 0, 63, 255),    //----- 234
              System.Drawing.Color.FromArgb(127, 63, 95, 255),    //----- 235
              System.Drawing.Color.FromArgb(76, 0, 38, 255),    //----- 236
              System.Drawing.Color.FromArgb(76, 38, 57, 255),    //----- 237
              System.Drawing.Color.FromArgb(38, 0, 19, 255),    //----- 238
              System.Drawing.Color.FromArgb(38, 19, 28, 255),    //----- 239
              System.Drawing.Color.FromArgb(255, 0, 63, 255),    //----- 240
              System.Drawing.Color.FromArgb(255, 127, 159, 255),//----- 241
              System.Drawing.Color.FromArgb(165, 0, 41, 255),    //----- 242
              System.Drawing.Color.FromArgb(165, 82, 103, 255),    //----- 243
              System.Drawing.Color.FromArgb(127, 0, 31, 255),    //----- 244
              System.Drawing.Color.FromArgb(127, 63, 79, 255),    //----- 245
              System.Drawing.Color.FromArgb(76, 0, 19, 255),    //----- 246
              System.Drawing.Color.FromArgb(76, 38, 47, 255),    //----- 247
              System.Drawing.Color.FromArgb(38, 0, 9, 255),        //----- 248
              System.Drawing.Color.FromArgb(38, 19, 23, 255),    //----- 249
              System.Drawing.Color.FromArgb(84, 84, 84, 255),    //----- 250
              System.Drawing.Color.FromArgb(118, 118, 118, 255),//----- 251
              System.Drawing.Color.FromArgb(152, 152, 152, 255),//----- 252
              System.Drawing.Color.FromArgb(186, 186, 186, 255),//----- 253
              System.Drawing.Color.FromArgb(220, 220, 220, 255),//----- 254
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 255
            };

        //////////////////////////////////////////////////////////////////////////////
        // standard autocad colours
        public static System.Drawing.Color[] MyAcadColorMs =
            {
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 0 - ByBlock - White
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 1 - Red
              System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 2 - Yellow
              System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 3 - Green
              System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 4 - Cyan
              System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 5 - Blue
              System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 6 - Magenta
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 7 - White
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 8
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 9
              System.Drawing.Color.FromArgb(255, 0, 0, 255),    //----- 10
              System.Drawing.Color.FromArgb(255, 127, 127, 255),//----- 11
              System.Drawing.Color.FromArgb(165, 0, 0, 255),    //----- 12
              System.Drawing.Color.FromArgb(165, 82, 82, 255),    //----- 13
              System.Drawing.Color.FromArgb(127, 0, 0, 255),    //----- 14
              System.Drawing.Color.FromArgb(127, 63, 63, 255),    //----- 15
              System.Drawing.Color.FromArgb(76, 0, 0, 255),        //----- 16
              System.Drawing.Color.FromArgb(76, 38, 38, 255),    //----- 17
              System.Drawing.Color.FromArgb(38, 0, 0, 255),        //----- 18
              System.Drawing.Color.FromArgb(38, 19, 19, 255),    //----- 19
              System.Drawing.Color.FromArgb(255, 63, 0, 255),    //----- 20
              System.Drawing.Color.FromArgb(255, 159, 127, 255),//----- 21
              System.Drawing.Color.FromArgb(165, 41, 0, 255),    //----- 22
              System.Drawing.Color.FromArgb(165, 103, 82, 255),    //----- 23
              System.Drawing.Color.FromArgb(127, 31, 0, 255),    //----- 24
              System.Drawing.Color.FromArgb(127, 79, 63, 255),    //----- 25
              System.Drawing.Color.FromArgb(76, 19, 0, 255),    //----- 26
              System.Drawing.Color.FromArgb(76, 47, 38, 255),    //----- 27
              System.Drawing.Color.FromArgb(38, 9, 0, 255),        //----- 28
              System.Drawing.Color.FromArgb(38, 23, 19, 255),    //----- 29
              System.Drawing.Color.FromArgb(255, 127, 0, 255),    //----- 30
              System.Drawing.Color.FromArgb(255, 191, 127, 255),//----- 31
              System.Drawing.Color.FromArgb(165, 82, 0, 255),    //----- 32
              System.Drawing.Color.FromArgb(165, 124, 82, 255),    //----- 33
              System.Drawing.Color.FromArgb(127, 63, 0, 255),    //----- 34
              System.Drawing.Color.FromArgb(127, 95, 63, 255),    //----- 35
              System.Drawing.Color.FromArgb(76, 38, 0, 255),    //----- 36
              System.Drawing.Color.FromArgb(76, 57, 38, 255),    //----- 37
              System.Drawing.Color.FromArgb(38, 19, 0, 255),    //----- 38
              System.Drawing.Color.FromArgb(38, 28, 19, 255),    //----- 39
              System.Drawing.Color.FromArgb(255, 191, 0, 255),    //----- 40
              System.Drawing.Color.FromArgb(255, 223, 127, 255),//----- 41
              System.Drawing.Color.FromArgb(165, 124, 0, 255),    //----- 42
              System.Drawing.Color.FromArgb(165, 145, 82, 255),    //----- 43
              System.Drawing.Color.FromArgb(127, 95, 0, 255),    //----- 44
              System.Drawing.Color.FromArgb(127, 111, 63, 255),    //----- 45
              System.Drawing.Color.FromArgb(76, 57, 0, 255),    //----- 46
              System.Drawing.Color.FromArgb(76, 66, 38, 255),    //----- 47
              System.Drawing.Color.FromArgb(38, 28, 0, 255),    //----- 48
              System.Drawing.Color.FromArgb(38, 33, 19, 255),    //----- 49
              System.Drawing.Color.FromArgb(255, 255, 0, 255),    //----- 50
              System.Drawing.Color.FromArgb(255, 255, 127, 255),//----- 51
              System.Drawing.Color.FromArgb(165, 165, 0, 255),    //----- 52
              System.Drawing.Color.FromArgb(165, 165, 82, 255),    //----- 53
              System.Drawing.Color.FromArgb(127, 127, 0, 255),    //----- 54
              System.Drawing.Color.FromArgb(127, 127, 63, 255),    //----- 55
              System.Drawing.Color.FromArgb(76, 76, 0, 255),    //----- 56
              System.Drawing.Color.FromArgb(76, 76, 38, 255),    //----- 57
              System.Drawing.Color.FromArgb(38, 38, 0, 255),    //----- 58
              System.Drawing.Color.FromArgb(38, 38, 19, 255),    //----- 59
              System.Drawing.Color.FromArgb(191, 255, 0, 255),    //----- 60
              System.Drawing.Color.FromArgb(223, 255, 127, 255),//----- 61
              System.Drawing.Color.FromArgb(124, 165, 0, 255),    //----- 62
              System.Drawing.Color.FromArgb(145, 165, 82, 255),    //----- 63
              System.Drawing.Color.FromArgb(95, 127, 0, 255),    //----- 64
              System.Drawing.Color.FromArgb(111, 127, 63, 255),    //----- 65
              System.Drawing.Color.FromArgb(57, 76, 0, 255),    //----- 66
              System.Drawing.Color.FromArgb(66, 76, 38, 255),    //----- 67
              System.Drawing.Color.FromArgb(28, 38, 0, 255),    //----- 68
              System.Drawing.Color.FromArgb(33, 38, 19, 255),    //----- 69
              System.Drawing.Color.FromArgb(127, 255, 0, 255),    //----- 70
              System.Drawing.Color.FromArgb(191, 255, 127, 255),//----- 71
              System.Drawing.Color.FromArgb(82, 165, 0, 255),    //----- 72
              System.Drawing.Color.FromArgb(124, 165, 82, 255),    //----- 73
              System.Drawing.Color.FromArgb(63, 127, 0, 255),    //----- 74
              System.Drawing.Color.FromArgb(95, 127, 63, 255),    //----- 75
              System.Drawing.Color.FromArgb(38, 76, 0, 255),    //----- 76
              System.Drawing.Color.FromArgb(57, 76, 38, 255),    //----- 77
              System.Drawing.Color.FromArgb(19, 38, 0, 255),    //----- 78
              System.Drawing.Color.FromArgb(28, 38, 19, 255),    //----- 79
              System.Drawing.Color.FromArgb(63, 255, 0, 255),    //----- 80
              System.Drawing.Color.FromArgb(159, 255, 127, 255),//----- 81
              System.Drawing.Color.FromArgb(41, 165, 0, 255),    //----- 82
              System.Drawing.Color.FromArgb(103, 165, 82, 255),    //----- 83
              System.Drawing.Color.FromArgb(31, 127, 0, 255),    //----- 84
              System.Drawing.Color.FromArgb(79, 127, 63, 255),    //----- 85
              System.Drawing.Color.FromArgb(19, 76, 0, 255),    //----- 86
              System.Drawing.Color.FromArgb(47, 76, 38, 255),    //----- 87
              System.Drawing.Color.FromArgb(9, 38, 0, 255),        //----- 88
              System.Drawing.Color.FromArgb(23, 38, 19, 255),    //----- 89
              System.Drawing.Color.FromArgb(0, 255, 0, 255),    //----- 90
              System.Drawing.Color.FromArgb(127, 255, 127, 255),//----- 91
              System.Drawing.Color.FromArgb(0, 165, 0, 255),    //----- 92
              System.Drawing.Color.FromArgb(82, 165, 82, 255),    //----- 93
              System.Drawing.Color.FromArgb(0, 127, 0, 255),    //----- 94
              System.Drawing.Color.FromArgb(63, 127, 63, 255),    //----- 95
              System.Drawing.Color.FromArgb(0, 76, 0, 255),        //----- 96
              System.Drawing.Color.FromArgb(38, 76, 38, 255),    //----- 97
              System.Drawing.Color.FromArgb(0, 38, 0, 255),        //----- 98
              System.Drawing.Color.FromArgb(19, 38, 19, 255),    //----- 99
              System.Drawing.Color.FromArgb(0, 255, 63, 255),    //----- 100
              System.Drawing.Color.FromArgb(127, 255, 159, 255),//----- 101
              System.Drawing.Color.FromArgb(0, 165, 41, 255),    //----- 102
              System.Drawing.Color.FromArgb(82, 165, 103, 255),    //----- 103
              System.Drawing.Color.FromArgb(0, 127, 31, 255),    //----- 104
              System.Drawing.Color.FromArgb(63, 127, 79, 255),    //----- 105
              System.Drawing.Color.FromArgb(0, 76, 19, 255),    //----- 106
              System.Drawing.Color.FromArgb(38, 76, 47, 255),    //----- 107
              System.Drawing.Color.FromArgb(0, 38, 9, 255),        //----- 108
              System.Drawing.Color.FromArgb(19, 38, 23, 255),    //----- 109
              System.Drawing.Color.FromArgb(0, 255, 127, 255),    //----- 110
              System.Drawing.Color.FromArgb(127, 255, 191, 255),//----- 111
              System.Drawing.Color.FromArgb(0, 165, 82, 255),    //----- 112
              System.Drawing.Color.FromArgb(82, 165, 124, 255),    //----- 113
              System.Drawing.Color.FromArgb(0, 127, 63, 255),    //----- 114
              System.Drawing.Color.FromArgb(63, 127, 95, 255),    //----- 115
              System.Drawing.Color.FromArgb(0, 76, 38, 255),    //----- 116
              System.Drawing.Color.FromArgb(38, 76, 57, 255),    //----- 117
              System.Drawing.Color.FromArgb(0, 38, 19, 255),    //----- 118
              System.Drawing.Color.FromArgb(19, 38, 28, 255),    //----- 119
              System.Drawing.Color.FromArgb(0, 255, 191, 255),    //----- 120
              System.Drawing.Color.FromArgb(127, 255, 223, 255),//----- 121
              System.Drawing.Color.FromArgb(0, 165, 124, 255),    //----- 122
              System.Drawing.Color.FromArgb(82, 165, 145, 255),    //----- 123
              System.Drawing.Color.FromArgb(0, 127, 95, 255),    //----- 124
              System.Drawing.Color.FromArgb(63, 127, 111, 255),    //----- 125
              System.Drawing.Color.FromArgb(0, 76, 57, 255),    //----- 126
              System.Drawing.Color.FromArgb(38, 76, 66, 255),    //----- 127
              System.Drawing.Color.FromArgb(0, 38, 28, 255),    //----- 128
              System.Drawing.Color.FromArgb(19, 38, 33, 255),    //----- 129
              System.Drawing.Color.FromArgb(0, 255, 255, 255),    //----- 130
              System.Drawing.Color.FromArgb(127, 255, 255, 255),//----- 131
              System.Drawing.Color.FromArgb(0, 165, 165, 255),    //----- 132
              System.Drawing.Color.FromArgb(82, 165, 165, 255),    //----- 133
              System.Drawing.Color.FromArgb(0, 127, 127, 255),    //----- 134
              System.Drawing.Color.FromArgb(63, 127, 127, 255),    //----- 135
              System.Drawing.Color.FromArgb(0, 76, 76, 255),    //----- 136
              System.Drawing.Color.FromArgb(38, 76, 76, 255),    //----- 137
              System.Drawing.Color.FromArgb(0, 38, 38, 255),    //----- 138
              System.Drawing.Color.FromArgb(19, 38, 38, 255),    //----- 139
              System.Drawing.Color.FromArgb(0, 191, 255, 255),    //----- 140
              System.Drawing.Color.FromArgb(127, 223, 255, 255),//----- 141
              System.Drawing.Color.FromArgb(0, 124, 165, 255),    //----- 142
              System.Drawing.Color.FromArgb(82, 145, 165, 255),    //----- 143
              System.Drawing.Color.FromArgb(0, 95, 127, 255),    //----- 144
              System.Drawing.Color.FromArgb(63, 111, 127, 255),    //----- 145
              System.Drawing.Color.FromArgb(0, 57, 76, 255),    //----- 146
              System.Drawing.Color.FromArgb(38, 66, 76, 255),    //----- 147
              System.Drawing.Color.FromArgb(0, 28, 38, 255),    //----- 148
              System.Drawing.Color.FromArgb(19, 33, 38, 255),    //----- 149
              System.Drawing.Color.FromArgb(0, 127, 255, 255),    //----- 150
              System.Drawing.Color.FromArgb(127, 191, 255, 255),//----- 151
              System.Drawing.Color.FromArgb(0, 82, 165, 255),    //----- 152
              System.Drawing.Color.FromArgb(82, 124, 165, 255),    //----- 153
              System.Drawing.Color.FromArgb(0, 63, 127, 255),    //----- 154
              System.Drawing.Color.FromArgb(63, 95, 127, 255),    //----- 155
              System.Drawing.Color.FromArgb(0, 38, 76, 255),    //----- 156
              System.Drawing.Color.FromArgb(38, 57, 76, 255),    //----- 157
              System.Drawing.Color.FromArgb(0, 19, 38, 255),    //----- 158
              System.Drawing.Color.FromArgb(19, 28, 38, 255),    //----- 159
              System.Drawing.Color.FromArgb(0, 63, 255, 255),    //----- 160
              System.Drawing.Color.FromArgb(127, 159, 255, 255),//----- 161
              System.Drawing.Color.FromArgb(0, 41, 165, 255),    //----- 162
              System.Drawing.Color.FromArgb(82, 103, 165, 255),    //----- 163
              System.Drawing.Color.FromArgb(0, 31, 127, 255),    //----- 164
              System.Drawing.Color.FromArgb(63, 79, 127, 255),    //----- 165
              System.Drawing.Color.FromArgb(0, 19, 76, 255),    //----- 166
              System.Drawing.Color.FromArgb(38, 47, 76, 255),    //----- 167
              System.Drawing.Color.FromArgb(0, 9, 38, 255),        //----- 168
              System.Drawing.Color.FromArgb(19, 23, 38, 255),    //----- 169
              System.Drawing.Color.FromArgb(0, 0, 255, 255),    //----- 170
              System.Drawing.Color.FromArgb(127, 127, 255, 255),//----- 171
              System.Drawing.Color.FromArgb(0, 0, 165, 255),    //----- 172
              System.Drawing.Color.FromArgb(82, 82, 165, 255),    //----- 173
              System.Drawing.Color.FromArgb(0, 0, 127, 255),    //----- 174
              System.Drawing.Color.FromArgb(63, 63, 127, 255),    //----- 175
              System.Drawing.Color.FromArgb(0, 0, 76, 255),        //----- 176
              System.Drawing.Color.FromArgb(38, 38, 76, 255),    //----- 177
              System.Drawing.Color.FromArgb(0, 0, 38, 255),        //----- 178
              System.Drawing.Color.FromArgb(19, 19, 38, 255),    //----- 179
              System.Drawing.Color.FromArgb(63, 0, 255, 255),    //----- 180
              System.Drawing.Color.FromArgb(159, 127, 255, 255),//----- 181
              System.Drawing.Color.FromArgb(41, 0, 165, 255),    //----- 182
              System.Drawing.Color.FromArgb(103, 82, 165, 255),    //----- 183
              System.Drawing.Color.FromArgb(31, 0, 127, 255),    //----- 184
              System.Drawing.Color.FromArgb(79, 63, 127, 255),    //----- 185
              System.Drawing.Color.FromArgb(19, 0, 76, 255),    //----- 186
              System.Drawing.Color.FromArgb(47, 38, 76, 255),    //----- 187
              System.Drawing.Color.FromArgb(9, 0, 38, 255),        //----- 188
              System.Drawing.Color.FromArgb(23, 19, 38, 255),    //----- 189
              System.Drawing.Color.FromArgb(127, 0, 255, 255),    //----- 190
              System.Drawing.Color.FromArgb(191, 127, 255, 255),//----- 191
              System.Drawing.Color.FromArgb(82, 0, 165, 255),    //----- 192
              System.Drawing.Color.FromArgb(124, 82, 165, 255),    //----- 193
              System.Drawing.Color.FromArgb(63, 0, 127, 255),    //----- 194
              System.Drawing.Color.FromArgb(95, 63, 127, 255),    //----- 195
              System.Drawing.Color.FromArgb(38, 0, 76, 255),    //----- 196
              System.Drawing.Color.FromArgb(57, 38, 76, 255),    //----- 197
              System.Drawing.Color.FromArgb(19, 0, 38, 255),    //----- 198
              System.Drawing.Color.FromArgb(28, 19, 38, 255),    //----- 199
              System.Drawing.Color.FromArgb(191, 0, 255, 255),    //----- 200
              System.Drawing.Color.FromArgb(223, 127, 255, 255),//----- 201
              System.Drawing.Color.FromArgb(124, 0, 165, 255),    //----- 202
              System.Drawing.Color.FromArgb(145, 82, 165, 255),    //----- 203
              System.Drawing.Color.FromArgb(95, 0, 127, 255),    //----- 204
              System.Drawing.Color.FromArgb(111, 63, 127, 255),    //----- 205
              System.Drawing.Color.FromArgb(57, 0, 76, 255),    //----- 206
              System.Drawing.Color.FromArgb(66, 38, 76, 255),    //----- 207
              System.Drawing.Color.FromArgb(28, 0, 38, 255),    //----- 208
              System.Drawing.Color.FromArgb(33, 19, 38, 255),    //----- 209
              System.Drawing.Color.FromArgb(255, 0, 255, 255),    //----- 210
              System.Drawing.Color.FromArgb(255, 127, 255, 255),//----- 211
              System.Drawing.Color.FromArgb(165, 0, 165, 255),    //----- 212
              System.Drawing.Color.FromArgb(165, 82, 165, 255),    //----- 213
              System.Drawing.Color.FromArgb(127, 0, 127, 255),    //----- 214
              System.Drawing.Color.FromArgb(127, 63, 127, 255),    //----- 215
              System.Drawing.Color.FromArgb(76, 0, 76, 255),    //----- 216
              System.Drawing.Color.FromArgb(76, 38, 76, 255),    //----- 217
              System.Drawing.Color.FromArgb(38, 0, 38, 255),    //----- 218
              System.Drawing.Color.FromArgb(38, 19, 38, 255),    //----- 219
              System.Drawing.Color.FromArgb(255, 0, 191, 255),    //----- 220
              System.Drawing.Color.FromArgb(255, 127, 223, 255),//----- 221
              System.Drawing.Color.FromArgb(165, 0, 124, 255),    //----- 222
              System.Drawing.Color.FromArgb(165, 82, 145, 255),    //----- 223
              System.Drawing.Color.FromArgb(127, 0, 95, 255),    //----- 224
              System.Drawing.Color.FromArgb(127, 63, 111, 255),    //----- 225
              System.Drawing.Color.FromArgb(76, 0, 57, 255),    //----- 226
              System.Drawing.Color.FromArgb(76, 38, 66, 255),    //----- 227
              System.Drawing.Color.FromArgb(38, 0, 28, 255),    //----- 228
              System.Drawing.Color.FromArgb(38, 19, 33, 255),    //----- 229
              System.Drawing.Color.FromArgb(255, 0, 127, 255),    //----- 230
              System.Drawing.Color.FromArgb(255, 127, 191, 255),//----- 231
              System.Drawing.Color.FromArgb(165, 0, 82, 255),    //----- 232
              System.Drawing.Color.FromArgb(165, 82, 124, 255),    //----- 233
              System.Drawing.Color.FromArgb(127, 0, 63, 255),    //----- 234
              System.Drawing.Color.FromArgb(127, 63, 95, 255),    //----- 235
              System.Drawing.Color.FromArgb(76, 0, 38, 255),    //----- 236
              System.Drawing.Color.FromArgb(76, 38, 57, 255),    //----- 237
              System.Drawing.Color.FromArgb(38, 0, 19, 255),    //----- 238
              System.Drawing.Color.FromArgb(38, 19, 28, 255),    //----- 239
              System.Drawing.Color.FromArgb(255, 0, 63, 255),    //----- 240
              System.Drawing.Color.FromArgb(255, 127, 159, 255),//----- 241
              System.Drawing.Color.FromArgb(165, 0, 41, 255),    //----- 242
              System.Drawing.Color.FromArgb(165, 82, 103, 255),    //----- 243
              System.Drawing.Color.FromArgb(127, 0, 31, 255),    //----- 244
              System.Drawing.Color.FromArgb(127, 63, 79, 255),    //----- 245
              System.Drawing.Color.FromArgb(76, 0, 19, 255),    //----- 246
              System.Drawing.Color.FromArgb(76, 38, 47, 255),    //----- 247
              System.Drawing.Color.FromArgb(38, 0, 9, 255),        //----- 248
              System.Drawing.Color.FromArgb(38, 19, 23, 255),    //----- 249
              System.Drawing.Color.FromArgb(84, 84, 84, 255),    //----- 250
              System.Drawing.Color.FromArgb(118, 118, 118, 255),//----- 251
              System.Drawing.Color.FromArgb(152, 152, 152, 255),//----- 252
              System.Drawing.Color.FromArgb(186, 186, 186, 255),//----- 253
              System.Drawing.Color.FromArgb(220, 220, 220, 255),//----- 254
              System.Drawing.Color.FromArgb(255, 255, 255, 255),//----- 255
            };

        public static bool GetActiveViewPortInfo(ref double height, ref double width, ref Point3d target, ref Vector3d viewDir, ref double viewTwist, bool getViewCenter)
        {
            // get the editor object

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.UpdateTiledViewportsInDatabase();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                ViewportTable vt = (ViewportTable)t.GetObject(db.ViewportTableId, OpenMode.ForRead);
                ViewportTableRecord btr = (ViewportTableRecord)t.GetObject(vt[GsUtility.strActive], OpenMode.ForRead);
                height = btr.Height;
                width = btr.Width;
                target = btr.Target;
                viewDir = btr.ViewDirection;
                viewTwist = btr.ViewTwist;

                t.Commit();
            }
            return true;
        }

        public class RubberbandRectangle
        {
            public enum PenStyles
            {
                PS_SOLID = 0,
                PS_DASH = 1,
                PS_DOT = 2,
                PS_DASHDOT = 3,
                PS_DASHDOTDOT = 4
            }

            // These values come from the larger set of defines in WinGDI.h,
            // but are all that are needed for this application.  If this class
            // is expanded for more generic rectangle drawing, they should be
            // replaced by enums from those sets of defones.
            private int NULL_BRUSH = 5;

            private int R2_XORPEN = 7;
            private PenStyles penStyle;
            private int BLACK_PEN = 0;

            // Default contructor - sets member fields
            public RubberbandRectangle()
            {
                penStyle = PenStyles.PS_DOT;
            }

            // penStyles property get/set.
            public PenStyles PenStyle
            {
                get { return penStyle; }
                set { penStyle = value; }
            }

            public void DrawXORRectangle(Graphics grp, System.Drawing.Point startPt, System.Drawing.Point endPt)
            {
                int X1 = startPt.X;
                int Y1 = startPt.Y;
                int X2 = endPt.X;
                int Y2 = endPt.Y;
                // Extract the Win32 HDC from the Graphics object supplied.
                IntPtr hdc = grp.GetHdc();

                // Create a pen with a dotted style to draw the border of the
                // rectangle.
                IntPtr gdiPen = CreatePen(penStyle,
                              1, BLACK_PEN);

                // Set the ROP cdrawint mode to XOR.
                SetROP2(hdc, R2_XORPEN);

                // Select the pen into the device context.
                IntPtr oldPen = SelectObject(hdc, gdiPen);

                // Create a stock NULL_BRUSH brush and select it into the device
                // context so that the rectangle isn't filled.
                IntPtr oldBrush = SelectObject(hdc,
                                     GetStockObject(NULL_BRUSH));

                // Now XOR the hollow rectangle on the Graphics object with
                // a dotted outline.
                Rectangle(hdc, X1, Y1, X2, Y2);

                // Put the old stuff back where it was.
                SelectObject(hdc, oldBrush); // no need to delete a stock object
                SelectObject(hdc, oldPen);
                DeleteObject(gdiPen);		// but we do need to delete the pen

                // Return the device context to Windows.
                grp.ReleaseHdc(hdc);
            }

            // Use Interop to call the corresponding Win32 GDI functions
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern int SetROP2(
                    IntPtr hdc,		// Handle to a Win32 device context
                    int enDrawMode	// Drawing mode
                    );

            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern IntPtr CreatePen(
                    PenStyles enPenStyle,	// Pen style from enum PenStyles
                    int nWidth,				// Width of pen
                    int crColor				// Color of pen
                    );

            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern bool DeleteObject(
                    IntPtr hObject	// Win32 GDI handle to object to delete
                    );

            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern IntPtr SelectObject(
                    IntPtr hdc,		// Win32 GDI device context
                    IntPtr hObject	// Win32 GDI handle to object to select
                    );

            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern void Rectangle(
                    IntPtr hdc,			// Handle to a Win32 device context
                    int X1,				// x-coordinate of top left corner
                    int Y1,				// y-cordinate of top left corner
                    int X2,				// x-coordinate of bottom right corner
                    int Y2				// y-coordinate of bottm right corner
                    );

            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern IntPtr GetStockObject(
                    int brStyle	// Selected from the WinGDI.h BrushStyles enum
                    );

            // C# version of Win32 RGB macro
            private static int RGB(int R, int G, int B)
            {
                return (R | (G << 8) | (B << 16));
            }
        }
    }
}