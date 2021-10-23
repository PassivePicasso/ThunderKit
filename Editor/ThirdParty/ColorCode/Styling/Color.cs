using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ColorCode.Styling
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Color
    {
        public Color(byte r, byte g, byte b) : this(255, r, g, b)
        {
        }

        public Color(byte alpha, byte r, byte g, byte b)
        {
            Alpha = alpha;
            R = r;
            G = g;
            B = b;
        }

        public string Name { get; private set; }

        public byte Alpha { get; set; } = 255;
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public static Color AliceBlue => new Color(240, 248, 255) {Name = nameof(AliceBlue)};
        public static Color AntiqueWhite => new Color(250, 235, 215) {Name = nameof(AntiqueWhite)};
        public static Color Aqua => new Color(0, 255, 255) {Name = nameof(Aqua)};
        public static Color Aquamarine => new Color(127, 255, 212) {Name = nameof(Aquamarine)};
        public static Color Azure => new Color(240, 255, 255) {Name = nameof(Azure)};
        public static Color Beige => new Color(245, 245, 220) {Name = nameof(Beige)};
        public static Color Bisque => new Color(255, 228, 196) {Name = nameof(Bisque)};
        public static Color Black => new Color(0, 0, 0) {Name = nameof(Black)};
        public static Color BlanchedAlmond => new Color(255, 235, 205) {Name = nameof(BlanchedAlmond)};
        public static Color Blue => new Color(0, 0, 255) {Name = nameof(Blue)};
        public static Color BlueViolet => new Color(138, 43, 226) {Name = nameof(BlueViolet)};
        public static Color Brown => new Color(165, 42, 42) {Name = nameof(Brown)};
        public static Color BurlyWood => new Color(222, 184, 135) {Name = nameof(BurlyWood)};
        public static Color CadetBlue => new Color(95, 158, 160) {Name = nameof(CadetBlue)};
        public static Color Chartreuse => new Color(127, 255, 0) {Name = nameof(Chartreuse)};
        public static Color Chocolate => new Color(210, 105, 30) {Name = nameof(Chocolate)};
        public static Color Coral => new Color(255, 127, 80) {Name = nameof(Coral)};
        public static Color CornflowerBlue => new Color(100, 149, 237) {Name = nameof(CornflowerBlue)};
        public static Color Cornsilk => new Color(255, 248, 220) {Name = nameof(Cornsilk)};
        public static Color Crimson => new Color(220, 20, 60) {Name = nameof(Crimson)};
        public static Color Cyan => new Color(0, 255, 255) {Name = nameof(Cyan)};
        public static Color DarkBlue => new Color(0, 0, 139) {Name = nameof(DarkBlue)};
        public static Color DarkCyan => new Color(0, 139, 139) {Name = nameof(DarkCyan)};
        public static Color DarkGoldenrod => new Color(184, 134, 11) {Name = nameof(DarkGoldenrod)};
        public static Color DarkGray => new Color(169, 169, 169) {Name = nameof(DarkGray)};
        public static Color DarkGreen => new Color(0, 100, 0) {Name = nameof(DarkGreen)};
        public static Color DarkGrey => new Color(169, 169, 169) {Name = nameof(DarkGrey)};
        public static Color DarkKhaki => new Color(189, 183, 107) {Name = nameof(DarkKhaki)};
        public static Color DarkMagenta => new Color(139, 0, 139) {Name = nameof(DarkMagenta)};
        public static Color DarkOliveGreen => new Color(85, 107, 47) {Name = nameof(DarkOliveGreen)};
        public static Color DarkOrange => new Color(255, 140, 0) {Name = nameof(DarkOrange)};
        public static Color DarkOrchid => new Color(153, 50, 204) {Name = nameof(DarkOrchid)};
        public static Color DarkRed => new Color(139, 0, 0) {Name = nameof(DarkRed)};
        public static Color DarkSalmon => new Color(233, 150, 122) {Name = nameof(DarkSalmon)};
        public static Color DarkSeaGreen => new Color(143, 188, 143) {Name = nameof(DarkSeaGreen)};
        public static Color DarkSlateBlue => new Color(72, 61, 139) {Name = nameof(DarkSlateBlue)};
        public static Color DarkSlateGray => new Color(47, 79, 79) {Name = nameof(DarkSlateGray)};
        public static Color DarkSlateGrey => new Color(47, 79, 79) {Name = nameof(DarkSlateGrey)};
        public static Color DarkTurquoise => new Color(0, 206, 209) {Name = nameof(DarkTurquoise)};
        public static Color DarkViolet => new Color(148, 0, 211) {Name = nameof(DarkViolet)};
        public static Color DeepPink => new Color(255, 20, 147) {Name = nameof(DeepPink)};
        public static Color DeepSkyBlue => new Color(0, 191, 255) {Name = nameof(DeepSkyBlue)};
        public static Color DimGray => new Color(105, 105, 105) {Name = nameof(DimGray)};
        public static Color DimGrey => new Color(105, 105, 105) {Name = nameof(DimGrey)};
        public static Color DodgerBlue => new Color(30, 144, 255) {Name = nameof(DodgerBlue)};
        public static Color Firebrick => new Color(178, 34, 34) {Name = nameof(Firebrick)};
        public static Color FloralWhite => new Color(255, 250, 240) {Name = nameof(FloralWhite)};
        public static Color ForestGreen => new Color(34, 139, 34) {Name = nameof(ForestGreen)};
        public static Color Fuchsia => new Color(255, 0, 255) {Name = nameof(Fuchsia)};
        public static Color Gainsboro => new Color(220, 220, 220) {Name = nameof(Gainsboro)};
        public static Color GhostWhite => new Color(248, 248, 255) {Name = nameof(GhostWhite)};
        public static Color Gold => new Color(255, 215, 0) {Name = nameof(Gold)};
        public static Color Goldenrod => new Color(218, 165, 32) {Name = nameof(Goldenrod)};
        public static Color Gray => new Color(128, 128, 128) {Name = nameof(Gray)};
        public static Color Green => new Color(0, 128, 0) {Name = nameof(Green)};
        public static Color GreenYellow => new Color(173, 255, 47) {Name = nameof(GreenYellow)};
        public static Color Grey => new Color(128, 128, 128) {Name = nameof(Grey)};
        public static Color Honeydew => new Color(240, 255, 240) {Name = nameof(Honeydew)};
        public static Color HotPink => new Color(255, 105, 180) {Name = nameof(HotPink)};
        public static Color IndianRed => new Color(205, 92, 92) {Name = nameof(IndianRed)};
        public static Color Indigo => new Color(75, 0, 130) {Name = nameof(Indigo)};
        public static Color Ivory => new Color(255, 255, 240) {Name = nameof(Ivory)};
        public static Color Khaki => new Color(240, 230, 140) {Name = nameof(Khaki)};
        public static Color Lavender => new Color(230, 230, 250) {Name = nameof(Lavender)};
        public static Color LavenderBlush => new Color(255, 240, 245) {Name = nameof(LavenderBlush)};
        public static Color LawnGreen => new Color(124, 252, 0) {Name = nameof(LawnGreen)};
        public static Color LemonChiffon => new Color(255, 250, 205) {Name = nameof(LemonChiffon)};
        public static Color LightBlue => new Color(173, 216, 230) {Name = nameof(LightBlue)};
        public static Color LightCoral => new Color(240, 128, 128) {Name = nameof(LightCoral)};
        public static Color LightCyan => new Color(224, 255, 255) {Name = nameof(LightCyan)};
        public static Color LightGoldenrodYellow => new Color(250, 250, 210) {Name = nameof(LightGoldenrodYellow)};
        public static Color LightGray => new Color(211, 211, 211) {Name = nameof(LightGray)};
        public static Color LightGreen => new Color(144, 238, 144) {Name = nameof(LightGreen)};
        public static Color LightGrey => new Color(211, 211, 211) {Name = nameof(LightGrey)};
        public static Color LightPink => new Color(255, 182, 193) {Name = nameof(LightPink)};
        public static Color LightSalmon => new Color(255, 160, 122) {Name = nameof(LightSalmon)};
        public static Color LightSeaGreen => new Color(32, 178, 170) {Name = nameof(LightSeaGreen)};
        public static Color LightSkyBlue => new Color(135, 206, 250) {Name = nameof(LightSkyBlue)};
        public static Color LightSlateGray => new Color(119, 136, 153) {Name = nameof(LightSlateGray)};
        public static Color LightSlateGrey => new Color(119, 136, 153) {Name = nameof(LightSlateGrey)};
        public static Color LightSteelBlue => new Color(176, 196, 222) {Name = nameof(LightSteelBlue)};
        public static Color LightYellow => new Color(255, 255, 224) {Name = nameof(LightYellow)};
        public static Color Lime => new Color(0, 255, 0) {Name = nameof(Lime)};
        public static Color LimeGreen => new Color(50, 205, 50) {Name = nameof(LimeGreen)};
        public static Color Linen => new Color(250, 240, 230) {Name = nameof(Linen)};
        public static Color Magenta => new Color(255, 0, 255) {Name = nameof(Magenta)};
        public static Color Maroon => new Color(128, 0, 0) {Name = nameof(Maroon)};
        public static Color MediumAquamarine => new Color(102, 205, 170) {Name = nameof(MediumAquamarine)};
        public static Color MediumBlue => new Color(0, 0, 205) {Name = nameof(MediumBlue)};
        public static Color MediumOrchid => new Color(186, 85, 211) {Name = nameof(MediumOrchid)};
        public static Color MediumPurple => new Color(147, 112, 219) {Name = nameof(MediumPurple)};
        public static Color MediumSeaGreen => new Color(60, 179, 113) {Name = nameof(MediumSeaGreen)};
        public static Color MediumSlateBlue => new Color(123, 104, 238) {Name = nameof(MediumSlateBlue)};
        public static Color MediumSpringGreen => new Color(0, 250, 154) {Name = nameof(MediumSpringGreen)};
        public static Color MediumTurquoise => new Color(72, 209, 204) {Name = nameof(MediumTurquoise)};
        public static Color MediumVioletRed => new Color(199, 21, 133) {Name = nameof(MediumVioletRed)};
        public static Color MidnightBlue => new Color(25, 25, 112) {Name = nameof(MidnightBlue)};
        public static Color MintCream => new Color(245, 255, 250) {Name = nameof(MintCream)};
        public static Color MistyRose => new Color(255, 228, 225) {Name = nameof(MistyRose)};
        public static Color Moccasin => new Color(255, 228, 181) {Name = nameof(Moccasin)};
        public static Color NavajoWhite => new Color(255, 222, 173) {Name = nameof(NavajoWhite)};
        public static Color Navy => new Color(0, 0, 128) {Name = nameof(Navy)};
        public static Color OldLace => new Color(253, 245, 230) {Name = nameof(OldLace)};
        public static Color Olive => new Color(128, 128, 0) {Name = nameof(Olive)};
        public static Color OliveDrab => new Color(107, 142, 35) {Name = nameof(OliveDrab)};
        public static Color Orange => new Color(255, 165, 0) {Name = nameof(Orange)};
        public static Color OrangeRed => new Color(255, 69, 0) {Name = nameof(OrangeRed)};
        public static Color Orchid => new Color(218, 112, 214) {Name = nameof(Orchid)};
        public static Color PaleGoldenrod => new Color(238, 232, 170) {Name = nameof(PaleGoldenrod)};
        public static Color PaleGreen => new Color(152, 251, 152) {Name = nameof(PaleGreen)};
        public static Color PaleTurquoise => new Color(175, 238, 238) {Name = nameof(PaleTurquoise)};
        public static Color PaleVioletRed => new Color(219, 112, 147) {Name = nameof(PaleVioletRed)};
        public static Color PapayaWhip => new Color(255, 239, 213) {Name = nameof(PapayaWhip)};
        public static Color PeachPuff => new Color(255, 218, 185) {Name = nameof(PeachPuff)};
        public static Color Peru => new Color(205, 133, 63) {Name = nameof(Peru)};
        public static Color Pink => new Color(255, 192, 203) {Name = nameof(Pink)};
        public static Color Plum => new Color(221, 160, 221) {Name = nameof(Plum)};
        public static Color PowderBlue => new Color(176, 224, 230) {Name = nameof(PowderBlue)};
        public static Color Purple => new Color(128, 0, 128) {Name = nameof(Purple)};
        public static Color RebeccaPurple => new Color(102, 51, 153) {Name = nameof(RebeccaPurple)};
        public static Color Red => new Color(255, 0, 0) {Name = nameof(Red)};
        public static Color RosyBrown => new Color(188, 143, 143) {Name = nameof(RosyBrown)};
        public static Color RoyalBlue => new Color(65, 105, 225) {Name = nameof(RoyalBlue)};
        public static Color SaddleBrown => new Color(139, 69, 19) {Name = nameof(SaddleBrown)};
        public static Color Salmon => new Color(250, 128, 114) {Name = nameof(Salmon)};
        public static Color SandyBrown => new Color(244, 164, 96) {Name = nameof(SandyBrown)};
        public static Color SeaGreen => new Color(46, 139, 87) {Name = nameof(SeaGreen)};
        public static Color SeaShell => new Color(255, 245, 238) {Name = nameof(SeaShell)};
        public static Color Sienna => new Color(160, 82, 45) {Name = nameof(Sienna)};
        public static Color Silver => new Color(192, 192, 192) {Name = nameof(Silver)};
        public static Color SkyBlue => new Color(135, 206, 235) {Name = nameof(SkyBlue)};
        public static Color SlateBlue => new Color(106, 90, 205) {Name = nameof(SlateBlue)};
        public static Color SlateGray => new Color(112, 128, 144) {Name = nameof(SlateGray)};
        public static Color SlateGrey => new Color(112, 128, 144) {Name = nameof(SlateGrey)};
        public static Color Snow => new Color(255, 250, 250) {Name = nameof(Snow)};
        public static Color SpringGreen => new Color(0, 255, 127) {Name = nameof(SpringGreen)};
        public static Color SteelBlue => new Color(70, 130, 180) {Name = nameof(SteelBlue)};
        public static Color Tan => new Color(210, 180, 140) {Name = nameof(Tan)};
        public static Color Teal => new Color(0, 128, 128) {Name = nameof(Teal)};
        public static Color Thistle => new Color(216, 191, 216) {Name = nameof(Thistle)};
        public static Color Tomato => new Color(255, 99, 71) {Name = nameof(Tomato)};
        public static Color Turquoise => new Color(64, 224, 208) {Name = nameof(Turquoise)};
        public static Color Violet => new Color(238, 130, 238) {Name = nameof(Violet)};
        public static Color Wheat => new Color(245, 222, 179) {Name = nameof(Wheat)};
        public static Color White => new Color(255, 255, 255) {Name = nameof(White)};
        public static Color WhiteSmoke => new Color(245, 245, 245) {Name = nameof(WhiteSmoke)};
        public static Color Yellow => new Color(255, 255, 0) {Name = nameof(Yellow)};
        public static Color YellowGreen => new Color(154, 205, 50) {Name = nameof(YellowGreen)};

        public static IEnumerable<Color> All => new[]
        {
            AliceBlue,
            AntiqueWhite,
            Aqua,
            Aquamarine,
            Azure,
            Beige,
            Bisque,
            Black,
            BlanchedAlmond,
            Blue,
            BlueViolet,
            Brown,
            BurlyWood,
            CadetBlue,
            Chartreuse,
            Chocolate,
            Coral,
            CornflowerBlue,
            Cornsilk,
            Crimson,
            Cyan,
            DarkBlue,
            DarkCyan,
            DarkGoldenrod,
            DarkGray,
            DarkGreen,
            DarkGrey,
            DarkKhaki,
            DarkMagenta,
            DarkOliveGreen,
            DarkOrange,
            DarkOrchid,
            DarkRed,
            DarkSalmon,
            DarkSeaGreen,
            DarkSlateBlue,
            DarkSlateGray,
            DarkSlateGrey,
            DarkTurquoise,
            DarkViolet,
            DeepPink,
            DeepSkyBlue,
            DimGray,
            DimGrey,
            DodgerBlue,
            Firebrick,
            FloralWhite,
            ForestGreen,
            Fuchsia,
            Gainsboro,
            GhostWhite,
            Gold,
            Goldenrod,
            Gray,
            Green,
            GreenYellow,
            Grey,
            Honeydew,
            HotPink,
            IndianRed,
            Indigo,
            Ivory,
            Khaki,
            Lavender,
            LavenderBlush,
            LawnGreen,
            LemonChiffon,
            LightBlue,
            LightCoral,
            LightCyan,
            LightGoldenrodYellow,
            LightGray,
            LightGreen,
            LightGrey,
            LightPink,
            LightSalmon,
            LightSeaGreen,
            LightSkyBlue,
            LightSlateGray,
            LightSlateGrey,
            LightSteelBlue,
            LightYellow,
            Lime,
            LimeGreen,
            Linen,
            Magenta,
            Maroon,
            MediumAquamarine,
            MediumBlue,
            MediumOrchid,
            MediumPurple,
            MediumSeaGreen,
            MediumSlateBlue,
            MediumSpringGreen,
            MediumTurquoise,
            MediumVioletRed,
            MidnightBlue,
            MintCream,
            MistyRose,
            Moccasin,
            NavajoWhite,
            Navy,
            OldLace,
            Olive,
            OliveDrab,
            Orange,
            OrangeRed,
            Orchid,
            PaleGoldenrod,
            PaleGreen,
            PaleTurquoise,
            PaleVioletRed,
            PapayaWhip,
            PeachPuff,
            Peru,
            Pink,
            Plum,
            PowderBlue,
            Purple,
            RebeccaPurple,
            Red,
            RosyBrown,
            RoyalBlue,
            SaddleBrown,
            Salmon,
            SandyBrown,
            SeaGreen,
            SeaShell,
            Sienna,
            Silver,
            SkyBlue,
            SlateBlue,
            SlateGray,
            SlateGrey,
            Snow,
            SpringGreen,
            SteelBlue,
            Tan,
            Teal,
            Thistle,
            Tomato,
            Turquoise,
            Violet,
            Wheat,
            White,
            WhiteSmoke,
            Yellow,
            YellowGreen
        };

        private bool IsEmpty { get; set; }
        public static Color Empty => new Color(0, 0, 0) {IsEmpty = true};

        public override bool Equals(object obj)
        {
            var color = obj as Color;
            return color != null && Equals(color);
        }

        public static bool operator ==(Color left, Color right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left == null && right.IsEmpty) return true;
            if ((object) left != null && left.IsEmpty && (object) right == null) return true;
            if ((object) left != null && left.IsEmpty && (object) right != null && right.IsEmpty) return true;

            if ((object) left == null || (object) right == null) return false;

            return left.IsEmpty && right.IsEmpty && (left.R == right.R) && (left.G == right.G) && (left.B == right.B);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        private bool Equals(Color other)
        {
            return (other.IsEmpty && IsEmpty) || (other.R == R) && (other.G == G) && (other.B == B);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode*397) ^ G.GetHashCode();
                hashCode = (hashCode*397) ^ B.GetHashCode();
                hashCode = (hashCode*397) ^ IsEmpty.GetHashCode();
                return hashCode;
            }
        }
    }
}