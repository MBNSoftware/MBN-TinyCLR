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
using MBN.Modules;

namespace MBN.Modules
{
	/// <summary>
	/// An abstract Font class used for drawing text to the OLED-C Display.
	/// </summary>
	public class MikroFont
	{

		#region Fields

		internal Byte[] FontData;

		internal Int32[] Lookups;

		internal Byte[] Sizes;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new MikroFont from raw data. for example, from an embedded resource.
		/// </summary>
		/// <param name="data">The MikroFont data</param>
		public MikroFont(Byte[] data)
		{
			LoadFont(data);
		}

		#endregion

		#region Properties

		/// <summary>
		/// If the MikroFont contains the Extended ASCII Character Set this property will be true, otherwise false.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// private static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaExt7);
		/// Debug.Print("Has Extended ASCII Characters? " + _font1.ExtendedCharacterSet);
		/// </code>
		/// <code language = "VB">
		/// Private Shared ReadOnly _font1 As MikroFont = FontManager.GetFont(FontManager.FontName.TahomaExt7)
		/// Debug.Print("Has Extended ASCII Characters? " <![CDATA[&]]> _font1.ExtendedCharacterSet.ToString())
		/// </code>
		/// </example>
		public Boolean ExtendedCharacterSet { get; private set; }

		/// <summary>
		/// Gets the Height of the MikroFont
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// private static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaExt7);
		/// Debug.Print("Font Height - " + _font1.FontHeight);
		/// </code>
		/// <code language = "VB">
		/// Private Shared ReadOnly _font1 As MikroFont = FontManager.GetFont(FontManager.FontName.TahomaExt7)
		/// Debug.Print("Font Height - " <![CDATA[&]]> _font1.FontHeight.ToString())
		/// </code>
		/// </example>
		public Int32 FontHeight { get; private set; }

		#endregion

		#region Private Methods

		private void LoadFont(Byte[] data)
		{
			Int32 dStart = 5;
			Int32 fdStart = 0;
			Int32 totalOffset = 0;

			if (data[0] != 109 || data[1] != 70 || data[2] != 110 || data[3] != 116) throw new InvalidOperationException("Invalid MikroFont File format");

			ExtendedCharacterSet = data[4] == 1;

			// MikroFont Height
			FontHeight = data[dStart++];

			// Create Lookups (character count - 1)
			Sizes = new Byte[ExtendedCharacterSet ? 222 : 94];
			Lookups = new Int32[ExtendedCharacterSet ? 222 : 94];


			// Create MikroFont Data
			FontData = new Byte[data.Length - dStart - (ExtendedCharacterSet ? 220 : 92)];

			for (Int32 i = 0; i < (ExtendedCharacterSet ? 220 : 92); i++)
			{
				// Get MikroFont Length
				Int32 len = data[dStart++];
				Sizes[i] = (Byte) len;
				Lookups[i] = totalOffset;
				totalOffset += len;

				// Copy Data
				Array.Copy(data, dStart, FontData, fdStart, len);
				dStart += len;
				fdStart += len;
			}
		}

		#endregion
	}
}

#if (NANOFRAMEWORK_1_0)
namespace nanoFramework.Presentation
#else
namespace Microsoft.SPOT
#endif
{

	/// <summary>
	/// Extension methods for the MikroFont object.
	/// </summary>
	public static class FontExtensions
	{
		#region Public Methods

		/// <summary>
		/// Computes the width and height of a specified line of text
		/// </summary>
		/// <param name="font">The <see cref="MikroFont"/> used to ComputeExtent.</param>
		/// <param name="text">The text you want to measure.</param>
		/// <param name="width">[OutAttribute] The width of the specified text.</param>
		/// <param name="height">[OutAttribute] The height of the specified text.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	int height, width;
		///	_font1.ComputeExtent("MikroBus.Net", out width, out height);
		///	Debug.Print("The string size is " + width + "pixels by " + height + " pixels");
		/// </code>
		/// <code language = "VB">
		///	Dim height As Integer, width As Integer
		///	_font1.ComputeExtent("MikroBus.Net", width, height)
		///	Debug.Print("The string size is " <![CDATA[&]]> width <![CDATA[&]]> "pixels by " <![CDATA[&]]> height <![CDATA[&]]> " pixels")
		/// </code>
		/// </example>
		public static void ComputeExtent(this MikroFont font, String text, out Int32 width, out Int32 height)
			{
				Size sz = MeasureString(font, text);
				width = sz.Width;
				height = sz.Height;
			}

		/// <summary>
		/// Computes the width and height of a specified line of text
		/// </summary>
		/// <remarks>This method takes into account multi-line strings with lines separated by an ASCII linefeed character (hex 0A).</remarks>
		/// <param name="font">The <see cref="MikroFont"/> used to ComputeExtentEx.</param>
		/// <param name="text">The text you want to measure.</param>
		/// <returns>The Size structure of the measured text.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	var sz = _font1.ComputeExtentEx("MikroBus.Net");
		///	Debug.Print("The string size is " + sz.Width + "pixels by " + sz.Height + " pixels");
		/// </code>
		/// <code language = "VB">
		///	Dim sz = _font1.ComputeExtentEx("MikroBus.Net")
		///	Debug.Print("The string size is " <![CDATA[&]]> sz.Width <![CDATA[&]]> "pixels by " <![CDATA[&]]> sz.Height <![CDATA[&]]> " pixels")
		/// </code>
		/// </example>
		public static Size ComputeExtentEx(this MikroFont font,  String text)
		{
			Size sz = new Size();

			String[] lines = text.Split('\n');

			for (Int32 i = 0; i < lines.Length; i++)
			{
				Int32 w = MeasureString(font, lines[i]).Width;
				if (w > sz.Width) sz.Width = w;
			}

			sz.Height = font.FontHeight * lines.Length;

			return sz;
		}

		/// <summary>
		/// Computes the width and height of a specified line of text to fit inside a Rectangle
		/// </summary>
		/// <param name="font">The <see cref="MikroFont"/> used to ComputeExtentInRect.</param>
		/// <param name="text">The text you want to measure.</param>
		/// <param name="width">[OutAttribute] The width of the specified text.</param>
		/// <param name="height">[OutAttribute] The height of the specified text.</param>
		/// <param name="availableWidth">The Available Width of the Rectangle</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// int height = 0, width = 0;
		///	_font1.ComputeExtentInRect("MikroBus.Net", out width, out height, 96);
		///	Debug.Print("The string size is " + width + "pixels by " + height + " pixels");
		/// </code>
		/// <code language = "VB">
		/// Dim height As Integer = 0, width As Integer = 0
		///	_font1.ComputeExtentInRect("MikroBus.Net", width, height, 96)
		///	Debug.Print("The string size is " <![CDATA[&]]> width <![CDATA[&]]> "pixels by " <![CDATA[&]]> height <![CDATA[&]]> " pixels")
		/// </code>
		/// </example>
		public static void ComputeExtentInRect(this MikroFont font, String text, out Int32 width, out Int32 height, Int32 availableWidth)
		{
			Int32 addLines = 0;
			Size sz = new Size();

			String[] lines = text.Split('\n');

			for (Int32 i = 0; i < lines.Length; i++)
			{
				Int32 w = MeasureString(font, lines[i]).Width;

				if (w > availableWidth)
				{
					addLines += w / availableWidth;
					w = availableWidth;
				}

				if (w > sz.Width) sz.Width = w;
			}

			width = sz.Width;
			height = font.FontHeight * (lines.Length + addLines);
		}

		/// <summary>
		///  Returns the height and width the specified character will occupy when rendered with this MikroFont.
		/// </summary>
		/// <param name="font">The <see cref="MikroFont"/> used to measure a single character with.</param>
		/// <param name="character">The character to measure.</param>
		/// <returns>The <see cref="Size"/> that the character will occupy when rendered with this MikroFont.</returns>
 		/// <example>Example usage:
		/// <code language = "C#">
		///	var sz = _font1.MeasureCharacter('M');
		///	Debug.Print("The size of the character is " + sz.Width + "pixels by " + sz.Height + " pixels");
		/// </code>
		/// <code language = "VB">
		///	Dim sz = _font1.MeasureCharacter("M"c)
		///	Debug.Print("The size of the character is " <![CDATA[&]]> sz.Width <![CDATA[&]]> "pixels by " <![CDATA[&]]> sz.Height <![CDATA[&]]> " pixels")
		/// </code>
		/// </example>
		public static Size MeasureCharacter(this MikroFont font, Char character)
		{
			if (character < 32 || character > (font.ExtendedCharacterSet ? 255 : 126)) return new Size(font.Sizes[23] * (8 / font.FontHeight), font.FontHeight);
			return new Size(font.Sizes[character - 32] * (8 / font.FontHeight), font.FontHeight);
		}

		/// <summary>
		///  Returns the height and width the specified string will occupy when rendered with this MikroFont.
		/// </summary>
		/// <param name="font">The <see cref="MikroFont"/> used to measure a string with..</param>
		/// <param name="text">The textural string of characters to measure.</param>
		/// <returns>The <see cref="Size"/> that the string will occupy when rendered with this MikroFont.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	var sz = _font1.MeasureString("MikroBus.Net");
		///	Debug.Print("The size of the string is " + sz.Width + "pixels by " + sz.Height + " pixels");
		/// </code>
		/// <code language = "VB">
		///	Dim sz = _font1.MeasureString("MikroBus.Net")
		///	Debug.Print("The size of the string is " <![CDATA[&]]> sz.Width <![CDATA[&]]> "pixels by " <![CDATA[&]]> sz.Height <![CDATA[&]]> " pixels")
		/// </code>
		/// </example>
		public static Size MeasureString(this MikroFont font, String text)
		{
			Int32 w = 0;

			for (Int32 i = 0; i < text.Length; i++)
			{
				if (text[i] < 32 || text[i] > (font.ExtendedCharacterSet ? 255 : 126)) w += font.Sizes[23] * 8 / font.FontHeight;
				else w += font.Sizes[text[i] - 32] * 8 / font.FontHeight + 1;
			}
			return new Size(w - 1, font.FontHeight);
		}

		#endregion
	}
}