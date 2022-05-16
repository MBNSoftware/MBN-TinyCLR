/*
 * OLED-C Click driver for TinyCLR 2.0.
 * 
 * Initial version coded by Stephen Cardinale
 * 
 *  
 * Copyright 2020 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
namespace nanoFramework.Presentation.Media
#else
// ReSharper disable once CheckNamespace
namespace Microsoft.SPOT.Presentation.Media
#endif
{
	/// <summary>
	/// A class providing the RGB 565 equivalent of the RGB 888 KnownColors commonly used in the .Net Framework
	/// </summary>
	public class KnownColors
	{
    	/// <summary>
		/// AliceBlue
		/// </summary>
		public static readonly MikroColor AliceBlue = new MikroColor(0xF7DF);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor AntiqueWhite = new MikroColor(0xFF5A);
		/// <summary>
		/// AntiqueWhite
		/// </summary>
		public static readonly MikroColor Aqua = new MikroColor(0x07FF);
		/// <summary>
		/// Aquamarine
		/// </summary>
		public static readonly MikroColor Aquamarine = new MikroColor(0x7FFA);
		/// <summary>
		/// Azure
		/// </summary>
		public static readonly MikroColor Azure = new MikroColor(0xF7FF);
		/// <summary>
		/// Beige
		/// </summary>
		public static readonly MikroColor Beige = new MikroColor(0xF7BB);
		/// <summary>
		/// Bisque
		/// </summary>
		public static readonly MikroColor Bisque = new MikroColor(0xFF38);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor Black = new MikroColor(0x0000);
		/// <summary>
		/// BlanchedAlmond
		/// </summary>
		public static readonly MikroColor BlanchedAlmond = new MikroColor(0xFF59);
		/// <summary>
		/// Blue
		/// </summary>
		public static readonly MikroColor Blue = new MikroColor(0x001F);
		/// <summary>
		/// Brown
		/// </summary>
		public static readonly MikroColor BlueViolet = new MikroColor(0x895C);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor Brown = new MikroColor(0xA145);
		/// <summary>
		/// BurlyWood
		/// </summary>
		public static readonly MikroColor BurlyWood = new MikroColor(0xDDD0);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor CadetBlue = new MikroColor(0x5CF4);
		/// <summary>
		/// Charcoal
		/// </summary>
		public static readonly MikroColor Charcoal = new MikroColor(0x3186);
		/// <summary>
		/// Charcoal Dust
		/// </summary>
		public static readonly MikroColor CharcoalDust = new MikroColor(0x528A);
		/// <summary>
		/// Chartreuse
		/// </summary>
		public static readonly MikroColor Chartreuse = new MikroColor(0x7FE0);
		/// <summary>
		/// Chocolate
		/// </summary>
		public static readonly MikroColor Chocolate = new MikroColor(0xD343);
		/// <summary>
		/// Coral
		/// </summary>
		public static readonly MikroColor Coral = new MikroColor(0xFBEA);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor CornflowerBlue = new MikroColor(0x64BD);
		/// <summary>
		/// Crimson
		/// </summary>
		public static readonly MikroColor Cornsilk = new MikroColor(0xFFDB);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor Crimson = new MikroColor(0xD8A7);
		/// <summary>
		/// Cyan
		/// </summary>
		public static readonly MikroColor Cyan = new MikroColor(0x07FF);
		/// <summary>
		/// DarkBlue
		/// </summary>
		public static readonly MikroColor DarkBlue = new MikroColor(0x0011);
		/// <summary>
		/// DarkCyan
		/// </summary>
		public static readonly MikroColor DarkCyan = new MikroColor(0x0451);
		/// <summary>
		/// DarkGoldenrod
		/// </summary>
		public static readonly MikroColor DarkGoldenrod = new MikroColor(0xBC21);
		/// <summary>
		/// DarkGray
		/// </summary>
		public static readonly MikroColor DarkGray = new MikroColor(0xAD55);
		/// <summary>
		/// DarkGreen
		/// </summary>
		public static readonly MikroColor DarkGreen = new MikroColor(0x0320);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor DarkKhaki = new MikroColor(0xBDAD);
		/// <summary>
		/// DarkMagenta
		/// </summary>
		public static readonly MikroColor DarkMagenta = new MikroColor(0x8811);
		/// <summary>
		/// DarkOliveGreen
		/// </summary>
		public static readonly MikroColor DarkOliveGreen = new MikroColor(0x5345);
		/// <summary>
		/// DarkOrange
		/// </summary>
		public static readonly MikroColor DarkOrange = new MikroColor(0xFC60);
		/// <summary>
		/// DarkOrchid
		/// </summary>
		public static readonly MikroColor DarkOrchid = new MikroColor(0x9999);
		/// <summary>
		/// DarkRed
		/// </summary>
		public static readonly MikroColor DarkRed = new MikroColor(0x8800);
		/// <summary>
		/// DarkSalmon
		/// </summary>
		public static readonly MikroColor DarkSalmon = new MikroColor(0xECAF);
		/// <summary>
		/// DarkSeaGreen
		/// </summary>
		public static readonly MikroColor DarkSeaGreen = new MikroColor(0x8DF1);
		/// <summary>
		/// DarkSlateBlue
		/// </summary>
		public static readonly MikroColor DarkSlateBlue = new MikroColor(0x49F1);
		/// <summary>
		/// DarkSlateGray
		/// </summary>
		public static readonly MikroColor DarkSlateGray = new MikroColor(0x2A69);
		/// <summary>
		/// DarkTurquoise
		/// </summary>
		public static readonly MikroColor DarkTurquoise = new MikroColor(0x067A);
		/// <summary>
		/// DarkViolet
		/// </summary>
		public static readonly MikroColor DarkViolet = new MikroColor(0x901A);
		/// <summary>
		/// DeepPink
		/// </summary>
		public static readonly MikroColor DeepPink = new MikroColor(0xF8B2);
		/// <summary>
		/// DeepSkyBlue
		/// </summary>
		public static readonly MikroColor DeepSkyBlue = new MikroColor(0x05FF);
		/// <summary>
		/// DimGray
		/// </summary>
		public static readonly MikroColor DimGray = new MikroColor(0x6B4D);
		/// <summary>
		/// DodgerBlue
		/// </summary>
		public static readonly MikroColor DodgerBlue = new MikroColor(0x1C9F);
		/// <summary>
		/// Firebrick
		/// </summary>
		public static readonly MikroColor Firebrick = new MikroColor(0xB104);
		/// <summary>
		/// FloralWhite
		/// </summary>
		public static readonly MikroColor FloralWhite = new MikroColor(0xFFFF);
		/// <summary>
		/// ForestGreen
		/// </summary>
		public static readonly MikroColor ForestGreen = new MikroColor(0x2444);
		/// <summary>
		/// Fuchsia
		/// </summary>
		public static readonly MikroColor Fuchsia = new MikroColor(0xF81F);
		/// <summary>
		/// Gainsboro
		/// </summary>
		public static readonly MikroColor Gainsboro = new MikroColor(0xDEFB);
		/// <summary>
		/// Ghost
		/// </summary>
		public static readonly MikroColor Ghost = new MikroColor(0xF79E);
		/// <summary>
		/// GhostWhite
		/// </summary>
		public static readonly MikroColor GhostWhite = new MikroColor(0xFFDF);
		/// <summary>
		/// Gold
		/// </summary>
		public static readonly MikroColor Gold = new MikroColor(0xFEA0);
		/// <summary>
		/// 
		/// </summary>
		public static readonly MikroColor Goldenrod = new MikroColor(0xDD24);
		/// <summary>
		/// Gray
		/// </summary>
		public static readonly MikroColor Gray = new MikroColor(0x8410);
		/// <summary>
		/// Green
		/// </summary>
		public static readonly MikroColor Green = new MikroColor(0x0400);
		/// <summary>
		/// GreenYellow
		/// </summary>
		public static readonly MikroColor GreenYellow = new MikroColor(0xAFE5);
		/// <summary>
		/// Honeydew
		/// </summary>
		public static readonly MikroColor Honeydew = new MikroColor(0xF7FE);
		/// <summary>
		/// HotPink
		/// </summary>
		public static readonly MikroColor HotPink = new MikroColor(0xFB56);
		/// <summary>
		/// IndianRed
		/// </summary>
		public static readonly MikroColor IndianRed = new MikroColor(0xCAEB);
		/// <summary>
		/// Indigo
		/// </summary>
		public static readonly MikroColor Indigo = new MikroColor(0x4810);
		/// <summary>
		/// Ivory
		/// </summary>
		public static readonly MikroColor Ivory = new MikroColor(0xFFFE);
		/// <summary>
		/// Khaki
		/// </summary>
		public static readonly MikroColor Khaki = new MikroColor(0xF731);
		/// <summary>
		/// Lavender
		/// </summary>
		public static readonly MikroColor Lavender = new MikroColor(0xE73F);
		/// <summary>
		/// LavenderBlush
		/// </summary>
		public static readonly MikroColor LavenderBlush = new MikroColor(0xFF9E);
		/// <summary>
		/// LawnGreen
		/// </summary>
		public static readonly MikroColor LawnGreen = new MikroColor(0x7FE0);
		/// <summary>
		/// LemonChiffon
		/// </summary>
		public static readonly MikroColor LemonChiffon = new MikroColor(0xFFD9);
		/// <summary>
		/// LightBlue
		/// </summary>
		public static readonly MikroColor LightBlue = new MikroColor(0xAEDC);
		/// <summary>
		/// LightCoral
		/// </summary>
		public static readonly MikroColor LightCoral = new MikroColor(0xF410);
		/// <summary>
		/// LightCyan
		/// </summary>
		public static readonly MikroColor LightCyan = new MikroColor(0xE7FF);
		/// <summary>
		/// LightGoldenrodYellow
		/// </summary>
		public static readonly MikroColor LightGoldenrodYellow = new MikroColor(0xFFDA);
		/// <summary>
		/// LightGray
		/// </summary>
		public static readonly MikroColor LightGray = new MikroColor(0xD69A);
		/// <summary>
		/// LightGreen
		/// </summary>
		public static readonly MikroColor LightGreen = new MikroColor(0x9772);
		/// <summary>
		/// LightPink
		/// </summary>
		public static readonly MikroColor LightPink = new MikroColor(0xFDB8);
		/// <summary>
		/// LightSalmon
		/// </summary>
		public static readonly MikroColor LightSalmon = new MikroColor(0xFD0F);
		/// <summary>
		/// LightSeaGreen
		/// </summary>
		public static readonly MikroColor LightSeaGreen = new MikroColor(0x2595);
		/// <summary>
		/// LightSkyBlue
		/// </summary>
		public static readonly MikroColor LightSkyBlue = new MikroColor(0x867F);
		/// <summary>
		/// LightSlateGray
		/// </summary>
		public static readonly MikroColor LightSlateGray = new MikroColor(0x7453);
		/// <summary>
		/// LightSteelBlue
		/// </summary>
		public static readonly MikroColor LightSteelBlue = new MikroColor(0xB63B);
		/// <summary>
		/// LightYellow
		/// </summary>
		public static readonly MikroColor LightYellow = new MikroColor(0xFFFC);
		/// <summary>
		/// Lime
		/// </summary>
		public static readonly MikroColor Lime = new MikroColor(0x07E0);
		/// <summary>
		/// LimeGreen
		/// </summary>
		public static readonly MikroColor LimeGreen = new MikroColor(0x3666);
		/// <summary>
		/// Linen
		/// </summary>
		public static readonly MikroColor Linen = new MikroColor(0xFF9C);
		/// <summary>
		/// Magenta
		/// </summary>
		public static readonly MikroColor Magenta = new MikroColor(0xF81F);
		/// <summary>
		/// Maroon
		/// </summary>
		public static readonly MikroColor Maroon = new MikroColor(0x8000);
		/// <summary>
		/// MediumAquamarine
		/// </summary>
		public static readonly MikroColor MediumAquamarine = new MikroColor(0x6675);
		/// <summary>
		/// MediumBlue
		/// </summary>
		public static readonly MikroColor MediumBlue = new MikroColor(0x0019);
		/// <summary>
		/// MediumOrchid
		/// </summary>
		public static readonly MikroColor MediumOrchid = new MikroColor(0xBABA);
		/// <summary>
		/// MediumPurple
		/// </summary>
		public static readonly MikroColor MediumPurple = new MikroColor(0x939B);
		/// <summary>
		/// MediumSeaGreen
		/// </summary>
		public static readonly MikroColor MediumSeaGreen = new MikroColor(0x3D8E);
		/// <summary>
		/// MediumSlateBlue
		/// </summary>
		public static readonly MikroColor MediumSlateBlue = new MikroColor(0x7B5D);
		/// <summary>
		/// MediumSpringGreen
		/// </summary>
		public static readonly MikroColor MediumSpringGreen = new MikroColor(0x07D3);
		/// <summary>
		/// MediumTurquoise
		/// </summary>
		public static readonly MikroColor MediumTurquoise = new MikroColor(0x4E99);
		/// <summary>
		/// MediumVioletRed
		/// </summary>
		public static readonly MikroColor MediumVioletRed = new MikroColor(0xC0B0);
		/// <summary>
		/// MidnightBlue
		/// </summary>
		public static readonly MikroColor MidnightBlue = new MikroColor(0x18CE);
		/// <summary>
		/// MintCream
		/// </summary>
		public static readonly MikroColor MintCream = new MikroColor(0xF7FF);
		/// <summary>
		/// MistyRose
		/// </summary>
		public static readonly MikroColor MistyRose = new MikroColor(0xFF3C);
		/// <summary>
		/// Moccasin
		/// </summary>
		public static readonly MikroColor Moccasin = new MikroColor(0xFF36);
		/// <summary>
		/// NavajoWhite
		/// </summary>
		public static readonly MikroColor NavajoWhite = new MikroColor(0xFEF5);
		/// <summary>
		/// Navy
		/// </summary>
		public static readonly MikroColor Navy = new MikroColor(0x0010);
		/// <summary>
		/// OldLace
		/// </summary>
		public static readonly MikroColor OldLace = new MikroColor(0xFFBC);
		/// <summary>
		/// Olive
		/// </summary>
		public static readonly MikroColor Olive = new MikroColor(0x8400);
		/// <summary>
		/// OliveDrab
		/// </summary>
		public static readonly MikroColor OliveDrab = new MikroColor(0x6C64);
		/// <summary>
		/// Orange
		/// </summary>
		public static readonly MikroColor Orange = new MikroColor(0xFD20);
		/// <summary>
		/// OrangeRed
		/// </summary>
		public static readonly MikroColor OrangeRed = new MikroColor(0xFA20);
		/// <summary>
		/// Orchid
		/// </summary>
		public static readonly MikroColor Orchid = new MikroColor(0xDB9A);
		/// <summary>
		/// PaleGoldenrod
		/// </summary>
		public static readonly MikroColor PaleGoldenrod = new MikroColor(0xEF55);
		/// <summary>
		/// PaleGreen
		/// </summary>
		public static readonly MikroColor PaleGreen = new MikroColor(0x9FD3);
		/// <summary>
		/// PaleTurquoise
		/// </summary>
		public static readonly MikroColor PaleTurquoise = new MikroColor(0xAF7D);
		/// <summary>
		/// PaleVioletRed
		/// </summary>
		public static readonly MikroColor PaleVioletRed = new MikroColor(0xDB92);
		/// <summary>
		/// PapayaWhip
		/// </summary>
		public static readonly MikroColor PapayaWhip = new MikroColor(0xFF7A);
		/// <summary>
		/// PeachPuff
		/// </summary>
		public static readonly MikroColor PeachPuff = new MikroColor(0xFED7);
		/// <summary>
		/// Peru
		/// </summary>
		public static readonly MikroColor Peru = new MikroColor(0xCC27);
		/// <summary>
		/// Pink
		/// </summary>
		public static readonly MikroColor Pink = new MikroColor(0xFE19);
		/// <summary>
		/// Plum
		/// </summary>
		public static readonly MikroColor Plum = new MikroColor(0xDD1B);
		/// <summary>
		/// PowderBlue
		/// </summary>
		public static readonly MikroColor PowderBlue = new MikroColor(0xB71C);
		/// <summary>
		/// Purple
		/// </summary>
		public static readonly MikroColor Purple = new MikroColor(0x8010);
		/// <summary>
		/// Red
		/// </summary>
		public static readonly MikroColor Red = new MikroColor(0xF800);
		/// <summary>
		/// RosyBrown
		/// </summary>
		public static readonly MikroColor RosyBrown = new MikroColor(0xFDEE);
		/// <summary>
		/// RoyalBlue
		/// </summary>
		public static readonly MikroColor RoyalBlue = new MikroColor(0x435C);
		/// <summary>
		/// SaddleBrown
		/// </summary>
		public static readonly MikroColor SaddleBrown = new MikroColor(0x8A22);
		/// <summary>
		/// Salmon
		/// </summary>
		public static readonly MikroColor Salmon = new MikroColor(0xFC0E);
		/// <summary>
		/// SandyBrown
		/// </summary>
		public static readonly MikroColor SandyBrown = new MikroColor(0xF52C);
		/// <summary>
		/// SeaGreen
		/// </summary>
		public static readonly MikroColor SeaGreen = new MikroColor(0x2C4A);
		/// <summary>
		/// SeaShell
		/// </summary>
		public static readonly MikroColor SeaShell = new MikroColor(0xFFBD);
		/// <summary>
		/// Sienna
		/// </summary>
		public static readonly MikroColor Sienna = new MikroColor(0xA285);
		/// <summary>
		/// Silver
		/// </summary>
		public static readonly MikroColor Silver = new MikroColor(0xC618);
		/// <summary>
		/// SkyBlue
		/// </summary>
		public static readonly MikroColor SkyBlue = new MikroColor(0x867D);
		/// <summary>
		/// SlateBlue
		/// </summary>
		public static readonly MikroColor SlateBlue = new MikroColor(0x6AD9);
		/// <summary>
		/// SlateGray
		/// </summary>
		public static readonly MikroColor SlateGray = new MikroColor(0x7412);
		/// <summary>
		/// Snow
		/// </summary>
		public static readonly MikroColor Snow = new MikroColor(0xFFDF);
		/// <summary>
		/// SpringGreen
		/// </summary>
		public static readonly MikroColor SpringGreen = new MikroColor(0x07EF);
		/// <summary>
		/// SteelBlue
		/// </summary>
		public static readonly MikroColor SteelBlue = new MikroColor(0x4416);
		/// <summary>
		/// Tan
		/// </summary>
		public static readonly MikroColor Tan = new MikroColor(0xD5B1);
		/// <summary>
		/// Teal
		/// </summary>
		public static readonly MikroColor Teal = new MikroColor(0x008080);
		/// <summary>
		/// Thistle
		/// </summary>
		public static readonly MikroColor Thistle = new MikroColor(0xDDFB);
		/// <summary>
		/// Tomato
		/// </summary>
		public static readonly MikroColor Tomato = new MikroColor(0xFB08);
		/// <summary>
		/// Turquoise
		/// </summary>
		public static readonly MikroColor Turquoise = new MikroColor(0x471A);
		/// <summary>
		/// Violet
		/// </summary>
		public static readonly MikroColor Violet = new MikroColor(0xEC1D);
		/// <summary>
		/// Wheat
		/// </summary>
		public static readonly MikroColor Wheat = new MikroColor(0xF6F6);
		/// <summary>
		/// White
		/// </summary>
		public static readonly MikroColor White = new MikroColor(0xFFFF);
		/// <summary>
		/// WhiteSmoke
		/// </summary>
		public static readonly MikroColor WhiteSmoke = new MikroColor(0xF7BE);
		/// <summary>
		/// Yellow
		/// </summary>
		public static readonly MikroColor Yellow = new MikroColor(0xFFE0);
		/// <summary>
		/// YellowGreen
		/// </summary>
		public static readonly MikroColor YellowGreen = new MikroColor(0x9E66);
	}
}