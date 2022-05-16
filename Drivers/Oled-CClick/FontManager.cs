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

using System;
#if (NANOFRAMEWORK_1_0)
using System.Resources;
#else
using MBN.Modules.Properties;
#endif

namespace MBN.Modules
{
	/// <summary>
	/// Manages font-related functionality.
	/// </summary>
	public static class FontManager
	{
#region ENUMS

		/// <summary>
		/// A set of predefined Fonts of varying sizes (8-24 points).
		/// </summary>
		public enum FontName
		{
			/// <summary>
			/// Tahoma 6 point without Extended ASCII Characters
			/// </summary>
			TahomaReg6,

			/// <summary>
			/// Tahoma 7 point without Extended ASCII Characters
			/// </summary>
			TahomaReg7,

			/// <summary>
			/// Tahoma 8 point without Extended ASCII Characters
			/// </summary>
			TahomaReg8,

			/// <summary>
			/// Tahoma 10 point without Extended ASCII Characters
			/// </summary>
			TahomaReg10,

			/// <summary>
			/// Tahoma 14 point without Extended ASCII Characters
			/// </summary>
			TahomaReg14,

			/// <summary>
			/// Tahoma 16 point without Extended ASCII Characters
			/// </summary>
			TahomaReg16,

			/// <summary>
			/// Tahoma 18 point without Extended ASCII Characters
			/// </summary>
			TahomaReg18,

			/// <summary>
			/// Roboto Mono 11x23 without Extended ASCII Characters
			/// </summary>
			RobotoMono11x23,

			/// <summary>
			/// Exo 2 Condensed 10x16 without Extended ASCII Characters
			/// </summary>
			Exo2Condensed10x16,

			/// <summary>
			/// Exo 2 Condensed 15x23 without Extended ASCII Characters
			/// </summary>
			Exo2Condensed15x23,

			/// <summary>
			/// Exo 2 Condensed 21x32 without Extended ASCII Characters
			/// </summary>
			Exo2Condensed21x32

		}

#endregion

#region Public Methods

		/// <summary>
		/// Returns a MikroFont resource specified by a predefined font.
		/// </summary>
		/// <param name="font">The predefined font</param>
		/// <returns>A Font usable by the OLED-C Click driver.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// private static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		/// </code>
		/// <code language = "VB">
		/// Private Shared ReadOnly _font1 As MikroFont = FontManager.GetFont(FontManager.FontName.TahomaReg7)
		/// </code>
		/// </example>
		public static MikroFont GetFont(FontName font)
		{
			switch (font)
			{
				case FontName.TahomaReg6:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_6_Regular);
				case FontName.TahomaReg7:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_7_Regular);
				case FontName.TahomaReg8:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_8_Regular);
				case FontName.TahomaReg10:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_10_Regular);
				case FontName.TahomaReg14:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_14_Regular);
				case FontName.TahomaReg16:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_16_Regular);
				case FontName.TahomaReg18:
					return new MikroFont(MikroFontFiles.guiFont_Tahoma_18_Regular);
				case FontName.RobotoMono11x23:
					return new MikroFont(MikroFontFiles.guiFont_Roboto_Mono11x23_Regular);
				case FontName.Exo2Condensed10x16:
					return new MikroFont(MikroFontFiles.guiFont_Exo_2_Condensed10x16_Regular);
				case FontName.Exo2Condensed15x23:
					return new MikroFont(MikroFontFiles.guiFont_Exo_2_Condensed15x23_Regular);
				case FontName.Exo2Condensed21x32:
					return new MikroFont(MikroFontFiles.guiFont_Exo_2_Condensed21x32_Regular);
				default:
					throw new ArgumentException("No such font exists.");
			}
		}
		// https://github.com/MBNSoftware/NETMF44/tree/f7d3203db1932b853143bc78412529dd18528296/Drivers/OledCClick
		// https://www.mikroe.com/glcd-font-creator
		// https://github.com/MikroElektronika/OLED_C_click/blob/master/example/c/ARM/STM/fonts.h
		// https://github.com/MikroElektronika/OLED_C_click/blob/master/example/basic/PIC32/fonts.mbas

#endregion
	}
}

