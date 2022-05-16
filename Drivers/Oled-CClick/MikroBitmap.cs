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

using MBN.Modules;

#if (NANOFRAMEWORK_1_0)
using nanoFramework.Presentation.Media;
using nanoFramework.UI;
#else
using System.Drawing;
using Microsoft.SPOT.Presentation.Media;
#endif

using System;

#if (NANOFRAMEWORK_1_0)
namespace nanoFramework.Presentation
#else
namespace Microsoft.SPOT
#endif
{
    /// <summary>
    /// An abstract class with support for bitmaps of the BGR 565 Color format.
    /// </summary>
    public class MikroBitmap
	{
#region Fields

        private Rect _clippingRegion;
		private Byte[] _pixels;

#endregion

#region Constructors

		/// <summary>
		/// Creates a new empty MikroBitmap object which you can use as a drawing surface.
		/// </summary>
        /// <param name="width">The width of the MikroBitmap.</param>
        /// <param name="height">The height of the MikroBitmap.</param>
		/// <remarks>Use this Constructor to instantiate an empty MikroBitmap object with the size of the supplied dimensions.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap bmp = new MikroBitmap(96, 96);
		/// </code>
		/// </example>
        public MikroBitmap(Int32 width, Int32 height)
        {
            if (width <= 0 || width > 96 || height <= 0 || height > 96) throw new ArgumentOutOfRangeException(null, @"The width or height parameters cannot be equal to or less than zero (0) or greater than 96");
            _pixels = new Byte[width * height * 2];
            Width = width;
            Height = height;
            _clippingRegion = new Rect(0, 0, Width, Height);
        }

        /// <summary>
		/// Creates a new MikroBitmap object from raw pixel data which you can use as a drawing surface.
		/// </summary>
		/// <param name="pixelData">Array of pixel data</param>
		/// <param name="width">The Width of the MikroBitmap object.</param>
		/// <param name="height">The Height of MikroBitmap object.</param>
 		/// <remarks>Use this Constructor to instantiate a Bitmap object from an array of 565 BGR pixel data without embedded Width and Height information.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	byte[] tinyBMP =
		///	{
		///		0xA1, 0x24, 0x58, 0x00, 0x58, 0x00, 0xA1, 0x24
		///	};
		///	_oled.Canvas.Clear(KnownColors.White);
		///	_oled.Canvas.DrawImage(new MikroBitmap(tinyBMP, 2, 2), (96 - 2) / 2, (96 - 2) / 2);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public MikroBitmap(Byte[] pixelData, Int32 width, Int32 height)
        {
			if (pixelData.Length != width * height * 2) throw new Exception("Invalid byte length for supplied dimensions");

            _pixels = pixelData;
            Width = width;
            Height = height;
            _clippingRegion = new Rect(0, 0, Width, Height);
        }

        /// <summary>
		/// Creates a new MikroBitmap object from raw pixel data which you can use as a drawing surface.
		/// </summary>
		/// <param name="pixelData">Array of pixel data</param>
		/// <remarks>Use this Constructor to instantiate a Bitmap object from an array of 565 BGR pixel data with embedded Width and Height information. For example, from the output of the MBN BitmapConverter.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	byte[] tinyBmp =
		///	{
		///		0x02, 0x02, 0xA1, 0x24, 0x58, 0x00, 0x58, 0x00, 0xA1, 0x24
		///	};
		///	_oled.Canvas.Clear(KnownColors.White);
		///	_oled.Canvas.DrawImage(new MikroBitmap(tinyBmp), (96 - 2) / 2, (96 - 2) / 2);
		///	_oled.Flush();
		/// Or
		/// MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	_oled.Canvas.Clear(KnownColors.White);
		///	_oled.Canvas.DrawImage(logo, (_oled.CanvasWidth - logo.Width) / 2, (_oled.CanvasHeight - logo.Height) / 2);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public MikroBitmap(Byte[] pixelData)
        {
			Width = pixelData[0];
			Height = pixelData[1];
			_pixels = new Byte[pixelData.Length - 2];
			Array.Copy(pixelData, 2, _pixels, 0, _pixels.Length);
            _clippingRegion = new Rect(0, 0, Width, Height);
        }

#endregion

#region Properties

		/// <summary>
		/// Gets or Sets the clipping region of the drawing surface of the MikroBitmap object.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	logo.ClippingRegion = new Rect(10, 10, 76, 79);
		/// </code>
		/// </example>
		public Rect ClippingRegion
		{
			get => _clippingRegion;
            set
			{
				if (value.X < 0 || value.X > Width || value.Y < 0 || value.Y > Height) throw new ArgumentOutOfRangeException(nameof(value), @"ClippingRegion cannot contain negative values or be larger than the Display Size.");
				if (value.X + value.Width > Width) value.Width = Width - value.X;
				if (value.Y + value.Height > Height) value.Height = Height - value.Y;
				_clippingRegion = value;
			}
		}

		/// <summary>
		/// Gets the Height of the MikroBitmap object
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Dim logo As New MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin))
		/// Debug.Print("Logo Height? " + logo.Height)
		/// </code>
		/// </example>
		public Int32 Height { get; }

        /// <summary>
		/// Gets or Sets the Pixel data of the MikroBitmap object.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap logo1 = new MikroBitmap(96, 96);
		///	MikroBitmap logo2 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	logo1.Pixels = logo2.Pixels;
		///	_oled.Flush(logo);
		/// </code>
		/// </example>
		public Byte[] Pixels
		{
			get => _pixels;
            set
			{
				if (_pixels.Length != value.Length) throw new InvalidOperationException("The supplied Pixels size does not match the existing size. You cannot change the size of the Pixel data Byte Array.");
				_pixels = value;
			}
		}

		/// <summary>
		/// Gets the Width of the MikroBitmap object
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Dim logo As New MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin))
		/// Debug.Print("Logo Width? " + logo.Width)
		/// </code>
		/// </example>
		public Int32 Width { get; }

#endregion

#region Public Methods

		/// <summary>
		/// Clears the MikroBitmap object turning all pixels to Black.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Clear();
		/// </code>
		/// </example>
		public void Clear()
		{
			for (Int32 i = 0; i < _pixels.Length; i++)
			{
				_pixels[i] = 0;
			}
		}

		/// <summary>
		/// Clears the MikroBitmap object turning all pixels to the specified <see cref="MikroColor"/>.
		/// </summary>
		/// <param name="color">The new <see cref="MikroColor"/> for the MikroBitmap object.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Clear(KnownColors.Orange);
		/// </code>
		/// </example>
		public void Clear(MikroColor color)
		{
			Byte c1 = (Byte)(color.Value >> 8);
			Byte c2 = (Byte)(color.Value & 0xff);

			for (Int32 i = 0; i < _pixels.Length; i += 2)
			{
				_pixels[i] = c1;
				_pixels[i + 1] = c2;
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
    using nanoFramework.Presentation;
#if (NANOFRAMEWORK_1_0)
    using nanoFramework.Presentation.Media;
#else
	using Presentation.Media;
#endif

    /// <summary>
    /// A class containing extension methods for the MikroBitmap object.
    /// </summary>
    public static class BitmapExtensions
	{
#region RGB 888 Conversion Extensions

		/// <summary>
		/// Converts a RGB 888 Bitmap to a 565 MikroBitmap object.
		/// </summary>
		/// <param name="bitmap">The <see cref="Bitmap"/> to convert.</param>
		/// <returns>A properly formatted MikroBitmap object to use as a parameter to the <see cref="M:MikroBitmap(byte[])"/> Constructor.</returns>
		/// <exception cref="ArgumentOutOfRangeException">A ArgumentOutOfRangeException will be thrown if the supplied RGB 888 Bitmap is larger than 96 x 96 pixels.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		///	using (var bmp = new Bitmap(Resources.GetBytes(Resources.BinaryResources.mbn_logo), Bitmap.BitmapImageType.Bmp))
		///	{
		///		_oled.Canvas.Clear(KnownColors.White);
		///		var img = bmp.ToMikroBitmap();
		///		_oled.Canvas.DrawImage(img, (_oled.CanvasWidth - bmp.Width) / 2, (_oled.CanvasHeight - bmp.Height) / 2);
		///		_oled.Flush();
		///	}
		/// </code>
		/// </example>
		public static Byte[] ToMikroBitmap(this Bitmap bitmap)
		{
			try
			{
				if (bitmap.Width * bitmap.Height == 0 || bitmap.Width * bitmap.Height > 96 * 96) throw new ArgumentOutOfRangeException(nameof(bitmap), @"Bitmap object passed to ConvertTo565Bitmap() method cannot be equal to 0 x 0 pixels or larger than 96 x 96 pixels.");
				Byte[] bOut = new Byte[bitmap.Width * bitmap.Height * 2 + 2];
				Int32 byteCounter = 0;

				bOut[byteCounter] = (Byte)bitmap.Width;
				bOut[++byteCounter] = (Byte)bitmap.Height;

				for (Int32 y = 0; y <= bitmap.Height - 1; y++)
				{
					for (Int32 x = 0; x <= bitmap.Width - 1; x++)
					{
#if (NANOFRAMEWORK_1_0)
						Color pixel = bitmap.GetPixel(x, y);
						UInt16 pixelOut = (UInt16)((ColorUtility.GetBValue(pixel) >> 3) | ((ColorUtility.GetGValue(pixel) & 0xFC) << 3) | ((ColorUtility.GetRValue(pixel) & 0xF8) << 8));
						bOut[++byteCounter] = (Byte)(pixelOut >> 8);
						bOut[++byteCounter] = (Byte)(pixelOut & 0xff);
#else
						Color pixel = bitmap.GetPixel(x, y);
                        UInt16 pixelOut = (UInt16) ((pixel.B >> 3) | ((pixel.G & 0xFC) << 3) | ((pixel.R & 0xF8) << 8));
						bOut[++byteCounter] = (Byte)(pixelOut >> 8);
						bOut[++byteCounter] = (Byte)(pixelOut & 0xff);
#endif
					}
				}
				return bOut;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

#endregion

#region Image Extensions

		/// <summary>
		/// Renders a MikroBitmap object on the drawing surface of the canvas at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="bitmap">The MikroBitmap object to draw.</param>
		/// <param name="x">The X position to draw at.</param>
		/// <param name="y">The Y position to draw at.</param>
 		/// <remarks>Use this method when drawing an image from a MikroBitmap generated by the MikroBus.Net BitmapGenerator program. For example, as an embedded Bin file.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	_oled.Canvas.Clear(KnownColors.White);
		///	_oled.Canvas.DrawImage(logo, (_oled.CanvasWidth - logo.Width) / 2, (_oled.CanvasHeight - logo.Height) / 2);
		///	_oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// Dim logo As New MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin))
		///_oled.Canvas.Clear(KnownColors.White)
		///_oled.Canvas.DrawImage(logo, (_oled.CanvasWidth - logo.Width) \ 2, (_oled.CanvasHeight - logo.Height) \ 2)
		///_oled.Flush()
		/// </code>
		/// </example>
		public static void DrawImage(this MikroBitmap canvas, MikroBitmap bitmap, Int32 x, Int32 y)
		{
			Int32 bytesPerLine = bitmap.Width * 2;
			Int32 lineCount = bitmap.Height;
			Int32 xOffset = 0;
			Int32 lineStart = 0;

			// Check bounds
			if (x >= canvas.ClippingRegion.X + canvas.ClippingRegion.Width || y >= canvas.ClippingRegion.Y + canvas.ClippingRegion.Height) return;
			if (bitmap.Width + x > canvas.ClippingRegion.Width || bitmap.Height + y > canvas.ClippingRegion.Height) return;

			// Adjust X
			if (x < 0)
			{
				xOffset = -x * 2;
				x = 0;
			}

			if (x < canvas.ClippingRegion.X)
			{
				xOffset += (canvas.ClippingRegion.X - x) * 2;
				x = canvas.ClippingRegion.X;
			}

			// Adjust Y
			if (y < 0)
			{
				lineStart = -y;
				y = 0;
			}

			if (y < canvas.ClippingRegion.Y)
			{
				lineStart += canvas.ClippingRegion.Y - y;
				y = canvas.ClippingRegion.Y;
			}

			// Adjust width
			if (x + bitmap.Width > canvas.ClippingRegion.X + canvas.ClippingRegion.Width) bytesPerLine = (canvas.ClippingRegion.Width - (x - canvas.ClippingRegion.X)) * 2;

			// Adjust height
			if (y + bitmap.Height > canvas.ClippingRegion.X + canvas.ClippingRegion.Width) lineCount = canvas.ClippingRegion.Height - (y - canvas.ClippingRegion.Y);

			// Copy Lines
			for (Int32 i = lineStart; i < lineCount + lineStart; i++)
			{
				Array.Copy(bitmap.Pixels, i * bitmap.Width * 2 + xOffset, canvas.Pixels, (y + i - lineStart) * canvas.Width * 2 + x * 2, bytesPerLine);
			}
		}

		/// <summary>
		/// Renders a MikroBitmap object on the drawing surface of the canvas at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="bitmapBytes">The raw Bitmap bytes to draw.</param>
		/// <param name="x">The X coordinate to start rendering the MikroBitmap at.</param>
		/// <param name="y">The Y coordinate to start rendering the MikroBitmap at.</param>
		/// <remarks>Use this method when drawing an image from a Byte Array containing the Width and Height information of the bitmap as the first two bytes as generated by the MikroBus.Net BitmapGenerator program as Hex output.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	byte[] tinyBmp =
		///	{
		///		0x02, 0x02, 0xA1, 0x24, 0x58, 0x00, 0x58, 0x00, 0xA1, 0x24
		///	};
		///	_oled.Canvas.Clear(KnownColors.White);
		///	_oled.Canvas.DrawImage(new MikroBitmap(tinyBmp), (96 - 2) / 2, (96 - 2) / 2);
		///	_oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// Dim tinyBmp As Byte() = {<![CDATA[&]]>H2, <![CDATA[&]]>H2, <![CDATA[&]]>HA1, <![CDATA[&]]>H24, <![CDATA[&]]>H58, <![CDATA[&]]>H0, <![CDATA[&]]>H58, <![CDATA[&]]>H0, <![CDATA[&]]>HA1, <![CDATA[&]]>H24}
		/// _oled.Canvas.Clear(KnownColors.White)
		/// _oled.Canvas.DrawImage(New MikroBitmap(tinyBmp), (96 - 2) \ 2, (96 - 2) \ 2)
		/// _oled.Flush()
		/// </code>
		/// </example>
		public static void DrawImage(this MikroBitmap canvas, Byte[] bitmapBytes, Int32 x, Int32 y)
		{
			Int32 w = bitmapBytes[0];
			Int32 h = bitmapBytes[1];
			if (bitmapBytes.Length != w * h * 2 + 2) return;
			MikroBitmap tmpBitmap = new MikroBitmap(bitmapBytes);
			DrawImage(canvas, tmpBitmap, x, y);
		}

#endregion

#region Pixel Extensions

		/// <summary>
		/// Returns the <see cref="MikroColor"/> of the pixel on the drawing surface of the canvas at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="x">The X location of the pixel to get.</param>
		/// <param name="y">The Y location of the Pixel to get.</param>
		/// <returns>The MikroColor of the pixel at the specified location. Or <see cref="KnownColors.Black"/> if an invalid pixel location is used a a parameter.</returns>
		/// <exception cref="ArgumentOutOfRangeException">An ArgumentOutOfRangeException will be thrown if the X or Y parameter of the Pixel location is outside the drawing surface of the canvas.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	var pixelColor = logo.GetPixel(10, 10);
		///	if (pixelColor == KnownColors.Black) pixelColor = KnownColors.White;
		///	logo.SetPixel(pixelColor, 10, 10);
		/// </code>
		/// </example>
		public static MikroColor GetPixel(this MikroBitmap canvas, Int32 x, Int32 y)
		{
			try
			{
				if (x < 0 || x > canvas.Width - 1) throw new ArgumentOutOfRangeException(nameof(x), @"Invalid pixel X location.");
				if (y < 0 || y > canvas.Height - 1) throw new ArgumentOutOfRangeException(nameof(y), @"Invalid pixel Y location");

				Int32 l = y * canvas.Width * 2 + x * 2;

				Int32 val = canvas.Pixels[l] << 8;
				val += canvas.Pixels[l + 1];
				return new MikroColor((UInt16)val);

			}
			catch (Exception)
			{
				return KnownColors.Black;
			}
		}

		/// <summary>
		/// Sets the pixel at the specified location on the drawing surface of the canvas at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="color">The <see cref="MikroColor"/> to set the pixel to.</param>
		/// <param name="x">The X location of the pixel to set.</param>
		/// <param name="y">The Y location of the Pixel to set.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));
		///	var pixelColor = logo.GetPixel(10, 10);
		///	if (pixelColor == KnownColors.Black) pixelColor = KnownColors.White;
		///	logo.SetPixel(pixelColor, 10, 10);
		/// </code>
		/// </example>
		public static void SetPixel(this MikroBitmap canvas, MikroColor color, Int32 x, Int32 y)
		{
			if (x >= canvas.Width || y >= canvas.Height) return;
			if (x < canvas.ClippingRegion.X || y < canvas.ClippingRegion.Y || x > canvas.ClippingRegion.X + canvas.ClippingRegion.Width || y > canvas.ClippingRegion.Y + canvas.ClippingRegion.Height) return;
			Int32 l = y * canvas.Width * 2 + x * 2;
			canvas.Pixels[l] = (Byte)(color.Value >> 8);
			canvas.Pixels[l + 1] = (Byte)(color.Value & 0xff);
		}

#endregion

#region Text Extensions

		/// <summary>
		/// Draws text with an transparent background on the drawing surface of the canvas at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor">The <see cref="Color"/> to use for rendering the text.</param>
		/// <param name="x">The X location of the MikroBitmap to render the text.</param>
		/// <param name="y">The Y location of the MikroBitmap to render the text.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, 0, 0);
		///	_oled.Flush(_oled.Canvas);
		/// </code>
		/// </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, Int32 x, Int32 y)
		{
			if (!CheckTextBounds(canvas, text, x, y)) return;
			for (Int32 i = 0; i < text.Length; i++)
			{
				if (x >= canvas.Width) return;
				x += DrawChar(canvas, text[i], font, x, y, textColor, false) + 1;
			}
		}

		///  <summary>
		///  Draws text with an opaque background on the drawing surface of the canvas at the specified location.
		///  </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor"></param>
		/// <param name="backColor">The background <see cref="Color"/> for the rendered text.</param>
		/// <param name="x">The X location of the MikroBitmap to render the text.</param>
		/// <param name="y">The Y location of the MikroBitmap to render the text.</param>
		/// <example>Example usage:
		///  <code language = "C#">
		/// _oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, KnownColors.Red, 0, 0);
		///	_oled.Flush();
		/// </code>
		///  </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, MikroColor backColor, Int32 x, Int32 y)
		{
			if (!CheckTextBounds(canvas, text, x, y)) return;
			for (Int32 i = 0; i < text.Length; i++)
			{
				if (x >= canvas.Width) return;
				x += DrawChar(canvas, text[i], font, x, y, textColor, true, backColor) + 1;
			}
		}

		/// <summary>
		/// Draws text with an transparent background on the drawing surface of the canvas within the confines of the supplied dimensions.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor">The <see cref="Color"/> to use for rendering the text.</param>
		/// <param name="x">The X location of the MikroBitmap to render the text.</param>
		/// <param name="y">The Y location of the MikroBitmap to render the text.</param>
		/// <param name="width">The Width dimension of the MikroBitmap to render the text.</param>
		/// <param name="height">The Height dimension of the MikroBitmap to render the text.</param>
		/// <param name="center">Optional - If True, the text will be horizontally centered within the confines of the supplied dimension, otherwise the text will be left justified.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, 0, 0, false);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, Int32 x, Int32 y, Int32 width, Int32 height, Boolean center = false)
		{
			if (!CheckTextBounds(canvas, text, x, y)) return;
			DrawText(canvas, text, font, textColor, new Rect(x, y, width, height), center);
		}

		/// <summary>
		/// Draws text with an transparent background on the drawing surface of the canvas within the confines of the supplied <see cref="Rect"/> structure.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor">The <see cref="Color"/> to use for rendering the text.</param>
		/// <param name="rect">The <see cref="Rect"/> area of the MikroBitmap in which to render the text.</param>
		/// <param name="center">Optional - Horizontally centers the text within the confines of the supplied dimensions if true, otherwise the text will be left justified.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		///	Rect myRect = new Rect(0, 0, 96, 48);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, myRect);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, Rect rect, Boolean center = false)
		{
			if (!CheckTextBounds(canvas, text, rect.X, rect.Y)) return;

			// Check if the string that fits in the existing rectangle
			if (font.MeasureString(text).Width <= rect.Width)
			{
				DrawText(canvas, text, font, textColor, rect.X + (center ? rect.Width / 2 - font.MeasureString(text).Width / 2 : 0), rect.Y);
				return;
			}

			rect.Height += rect.Y;

			String str = String.Empty;
			Int32 curW = 0;
			Int32 spcW = font.MeasureCharacter(' ').Width;

			// Create string that fits
			while (text.Length > 0)
			{
				// Get next instance of space
				Int32 iPos = text.IndexOf(' ');

				String tmp = iPos > -1 ? text.Substring(0, iPos + 1) : text;
				Int32 tmpW = font.MeasureString(tmp).Width - spcW;

				// See if we can manage to append it
				if (curW + tmpW < rect.Width)
				{
					curW += tmpW;
					str += tmp;
					if (iPos == -1)
					{
						DrawText(canvas, str, font, textColor, rect.X + (center ? rect.Width / 2 - curW / 2 : 0), rect.Y);
						text = String.Empty;
						str = String.Empty;
					}
					else
					{
						text = text.Substring(iPos + 1);
					}
				}
				else
				{
					// Do we already have string data?
					if (str != String.Empty)
					{
						// Flush what we have
						DrawText(canvas, str, font, textColor, rect.X, rect.Y);

						// Linefeed
						rect.Y += font.FontHeight;
						if (rect.Y + font.FontHeight > rect.Height) return;

						// Can we copy the new temp?
						if (tmpW < rect.Width)
						{
							str = tmp;
							curW = tmpW;
							text = text.Substring(iPos + 1);
						}
						else
						{
							// Can't fit, leave for next loop
							curW = 0;
							str = String.Empty;
						}
					}
					else
					{
						// Grab as much of the string as we can
						while (font.MeasureString(tmp).Width > rect.Width)
						{
							tmp = tmp.Substring(0, tmp.Length - 1);
							iPos -= 1;
						}

						// Ensure we have at least 1 character
						if (tmp.Length == 0)
						{
							iPos = 1;
							tmp = text.Substring(0, 1);
						}

						// Paint out
						DrawText(canvas, tmp, font, textColor, rect.X, rect.Y + (center ? rect.Width / 2 - curW / 2 : 0));

						// Linefeed
						rect.Y += font.FontHeight;
						if (rect.Y + font.FontHeight > rect.Height) return;

						// Update
						str = String.Empty;
						curW = 0;
						text = text.Substring(iPos);
					}
				}
			}

			if (str != String.Empty) DrawText(canvas, str, font, textColor, rect.X, rect.Y + (center ? rect.Width / 2 - curW / 2 : 0));
		}

		///  <summary>
		///  Draws text with an opaque background on the drawing surface of the canvas within the confines of the supplied dimensions.
		///  </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor">The <see cref="Color"/> to use for rendering the text.</param>
		/// <param name="backColor">The background <see cref="Color"/> for the rendered text.</param>
		/// <param name="x">The X dimension of the of the MikroBitmap to which to render the text.</param>
		/// <param name="y">The Y> dimension of the MikroBitmap to which to render the text.</param>
		/// <param name="width">The Width dimension of the MikroBitmap to render the text.</param>
		/// <param name="height">The Height dimension of the MikroBitmap to render the text.</param>
		/// <param name="center">Optional - If True, the text will be horizontally centered within the confines of the supplied <see cref="Rect"/>, otherwise the text will be left justified.</param>
		/// <example>Example usage:
		///  <code language = "C#">
		///	_oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, KnownColors.Red, 0, 0, 96, 48);
		///	_oled.Flush();
		///  </code>
		///  </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, MikroColor backColor, Int32 x, Int32 y, Int32 width, Int32 height, Boolean center = false)
		{
			if (!CheckTextBounds(canvas, text, x, y)) return;
			DrawText(canvas, text, font, textColor, backColor, new Rect(x, y, width, height), center);
		}

		///  <summary>
		///  Draws text with an opaque background on the drawing surface of the canvas within the confines of the supplied dimensions.
		///  </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="text">The Text to render to the OLED Display.</param>
		/// <param name="font">The <see cref="MikroFont"/> to render the text with.</param>
		/// <param name="textColor">The <see cref="Color"/> to use for rendering the text.</param>
		/// <param name="backColor">The background <see cref="Color"/> for the rendered text.</param>
		/// <param name="rect">The <see cref="Rect"/> in which to render the text.</param>
		/// <param name="center">Optional - Horizontally centers the text within the confines of the supplied dimensions if true, otherwise the text will be left justified.</param>
		/// <example>Example usage:
		///  <code language = "C#">
		///	_oled.Canvas.Clear(KnownColors.White);
		///	MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		/// Rect myRect = new Rect(0, 0, 96, 48);
		///	_oled.Canvas.DrawText("0123AaBb", _font1, KnownColors.Blue, KnownColors.Red, myRect);
		///	_oled.Flush();
		///  </code>
		///  </example>
		public static void DrawText(this MikroBitmap canvas, String text, MikroFont font, MikroColor textColor, MikroColor backColor, Rect rect, Boolean center = false)
		{
			if (!CheckTextBounds(canvas, text, rect.X, rect.Y)) return;

			// Check for string that fits
			if (font.MeasureString(text).Width <= rect.Width)
			{
				DrawText(canvas, text, font, textColor, backColor, rect.X + (center ? rect.Width / 2 - font.MeasureString(text).Width / 2 : 0), rect.Y);
				return;
			}

			rect.Height += rect.Y;

			String str = String.Empty;
			Int32 curW = 0;
			Int32 spcW = font.MeasureCharacter(' ').Width;


			// Create string that fits
			while (text.Length > 0)
			{
				// Get next instance of space
				Int32 iPos = text.IndexOf(' ');

				String tmp = iPos > -1 ? text.Substring(0, iPos + 1) : text;
				Int32 tmpW = font.MeasureString(tmp).Width - spcW;

				// See if we can manage to append it
				if (curW + tmpW < rect.Width)
				{
					curW += tmpW;
					str += tmp;
					if (iPos == -1)
					{
						DrawText(canvas, str, font, textColor, backColor, rect.X + (center ? rect.Width / 2 - curW / 2 : 0), rect.Y);
						text = String.Empty;
						str = String.Empty;
					}
					else
                    {
                        text = text.Substring(iPos + 1);
                    }
                }
				else
				{
					// Do we already have string data?
					if (str != String.Empty)
					{
						// Flush what we have
						DrawText(canvas, str, font, textColor, backColor, rect.X, rect.Y);

						// Linefeed
						rect.Y += font.FontHeight;
						if (rect.Y + font.FontHeight > rect.Height) return;

						// Can we copy the new temp?
						if (tmpW < rect.Width)
						{
							str = tmp;
							curW = tmpW;
							text = text.Substring(iPos + 1);
						}
						else
						{
							// Can't fit, leave for next loop
							curW = 0;
							str = String.Empty;
						}
					}
					else
					{
						// Grab as much of the string as we can
						while (font.MeasureString(tmp).Width > rect.Width)
						{
							tmp = tmp.Substring(0, tmp.Length - 1);
							iPos -= 1;
						}

						// Ensure we have at least 1 character
						if (tmp.Length == 0)
						{
							iPos = 1;
							tmp = text.Substring(0, 1);
						}

						// Paint out
						DrawText(canvas, tmp, font, textColor, backColor, rect.X, rect.Y + (center ? rect.Width / 2 - curW / 2 : 0));

						// Linefeed
						rect.Y += font.FontHeight;
						if (rect.Y + font.FontHeight > rect.Height) return;

						// Update
						str = String.Empty;
						curW = 0;
						text = text.Substring(iPos);
					}
				}
			}
			if (str != String.Empty) DrawText(canvas, str, font, textColor, rect.X, rect.Y + (center ? rect.Width / 2 - curW / 2 : 0));
		}

#endregion

#region Lines Extensions

		/// <summary>
		/// Draws a line to the drawing surface of the canvas at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="color">The <see cref="Color"/> to use for rendering the line.</param>
		/// <param name="x1">The starting X coordinate to start drawing the line.</param>
		/// <param name="y1">The starting Y coordinate to start drawing the line.</param>
		/// <param name="x2">The ending X coordinate to end drawing the line.</param>
		/// <param name="y2">The ending Y coordinate to end drawing the line.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear( );
		/// _oled.Canvas.DrawLine(KnownColors.White, 0, 10, 96, 10);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawLine(this MikroBitmap canvas, MikroColor color, Int32 x1, Int32 y1, Int32 x2, Int32 y2)
		{
			if (x1 == x2 || y1 == y2) SetPixel(canvas, color, x1, y1);
			if (x1 < canvas.ClippingRegion.Y || x1 > canvas.ClippingRegion.Height || x2 < canvas.ClippingRegion.Y || x2 > canvas.ClippingRegion.Width) return;
			if (y1 < canvas.ClippingRegion.Y || y1 > canvas.ClippingRegion.Height || y2 < canvas.ClippingRegion.Y || y2 > canvas.ClippingRegion.Width) return;

			Int32 dy = y2 - y1;
			Int32 dx = x2 - x1;

			Single m = 0;
			Int32 b = 0;

			if (dx != 0)
			{
				m = (Single)dy / dx;
				b = y1 - (Int32)(m * x1);
			}

			if (Math.Abs(dx) >= Math.Abs(dy))
			{
				if (x1 > x2)
				{
					Swap(ref x1, ref x2);
					Swap(ref y1, ref y2);
				}

				while (x1 <= x2)
				{
					SetPixel(canvas, color, x1, y1);
					x1++;
					if (x1 <= x2) y1 = (Int32)(m * x1) + b;
				}
			}
			else
			{
				if (y1 > y2)
				{
					Swap(ref x1, ref x2);
					Swap(ref y1, ref y2);
				}

				while (y1 <= y2)
				{
					SetPixel(canvas, color, x1, y1);
					y1++;
					if (y1 <= y2 && dx != 0) x1 = (Int32)((y1 - b) / m);
				}
			}
		}

#endregion

#region Rectangles Extensions

		/// <summary>
		/// Draws a hollow Rectangle to the drawing surface of the canvas at the specified rectangle.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="MikroColor"/> of the outline for the rectangle.</param>
		/// <param name="rect">The <see cref="Rect"/> location of the MikroBitmap to render the Rectangle.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear();
		///	Rect myRect = new Rect(0, 0, 96, 48);
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, myRect);
		///	_oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// _oled.Canvas.Clear()
		///	Dim myRect As Rect = new Rect(0, 0, 96, 48)
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, myRect)
		///	_oled.Flush()
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, Rect rect)
		{
			DrawRectangle(canvas, outlineColor, rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>
		/// Draws a hollow Rectangle to the drawing surface of the canvas at the specified at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline for the rectangle.</param>
		/// <param name="x">The X dimension of the of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="y">The Y> dimension of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="width">The Width dimension of the MikroBitmap to render the rectangle.</param>
		/// <param name="height">The Height dimension of the MikroBitmap to render the rectangle.</param>
		/// <example>Example usage:
		/// <code language = "C#">
 		/// _oled.Canvas.Clear();
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, 0, 0, 96, 16);
		/// _oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// _oled.Canvas.Clear()
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, 0, 0, 96, 16)
		/// _oled.Flush();
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, Int32 x, Int32 y, Int32 width, Int32 height)
		{
			FillRectangle(canvas, outlineColor, x, y, width, 1);
			FillRectangle(canvas, outlineColor, x, y, 1, height);
			FillRectangle(canvas, outlineColor, x + width - 1, y, 1, height);
			FillRectangle(canvas, outlineColor, x, y + height - 1, width, 1);
		}

		/// <summary>
		/// Draws a filled Rectangle to the drawing surface of the canvas at the specified rectangle.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline for the rectangle.</param>
		/// <param name="fillColor">The <see cref="Color"/> to fill the rectangle with.</param>
		/// <param name="rect">The <see cref="Rect"/> location of the MikroBitmap to render the Rectangle.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear();
		/// Rect myRect = new Rect(0, 0, 96, 48);
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, myRect);
		/// _oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// _oled.Canvas.Clear()
		/// Dim myRect As Rect = new Rect(0, 0, 96, 48)
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, myRect);
		/// _oled.Flush()
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, MikroColor fillColor, Rect rect)
		{
			DrawRectangle(canvas, outlineColor, fillColor, rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>
		/// Draws a filled Rectangle to the drawing surface of the canvas at the specified at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline for the rectangle.</param>
		/// <param name="fillColor">The <see cref="Color"/> to fill the rectangle with.</param>
		/// <param name="x">The X dimension of the of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="y">The Y> dimension of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="width">The Width dimension of the MikroBitmap to render the rectangle.</param>
		/// <param name="height">The Height dimension of the MikroBitmap to render the rectangle.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear();
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, 0, 0, 96, 16);
		/// _oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// _oled.Canvas.Clear()
		///	_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, 0, 0, 96, 16)
		/// _oled.Flush();
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, MikroColor fillColor, Int32 x, Int32 y, Int32 width, Int32 height)
		{
			FillRectangle(canvas, outlineColor, x, y, width, 1);
			FillRectangle(canvas, outlineColor, x, y, 1, height);
			FillRectangle(canvas, outlineColor, x + width - 1, y, 1, height);
			FillRectangle(canvas, outlineColor, x, y + height - 1, width, 1);
			FillRectangle(canvas, fillColor, x + 1, y + 1, width - 2, height - 2);
		}

		/// <summary>
		/// Draws a filled Rectangle with a gradient and outline to the drawing surface of the canvas at the specified rectangle.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> for the outline of the rectangle.</param>
		/// <param name="outlineWidth">The thickness in pixels of the outline of the rectangle.</param>
		/// <param name="gradientStartColor">The starting <see cref="Color"/> of the gradient to fill the rectangle with.</param>
		/// <param name="gradientEndColor">The ending <see cref="Color"/> of the gradient to fill the rectangle with.</param>
		/// <param name="rect">The <see cref="Rect"/> location of the MikroBitmap to render the Rectangle.</param>
		/// <param name="horizontal">If true, the gradient will be drawn horizontally, if false, it will be drawn vertically.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear();
		///	Rect myRect = new Rect(0, 0, _oled.CanvasWidth, _oled.CanvasHeight);
		///	_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, myRect, true);
		///	_oled.Flush();
		/// </code>
		/// <code language = "VB">
		///	_oled.Canvas.Clear()
		///	Dim myRect As Rect = new Rect(0, 0, _oled.CanvasWidth, _oled.CanvasHeight)
		///	_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, myRect, true)
		///	_oled.Flush()
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, Int32 outlineWidth, MikroColor gradientStartColor, MikroColor gradientEndColor, Rect rect, Boolean horizontal = false)
		{
			DrawRectangle(canvas, outlineColor, outlineWidth, gradientStartColor, gradientEndColor, rect.X, rect.Y, rect.Width, rect.Height, horizontal);
		}

		/// <summary>
		/// Draws a filled Rectangle with a gradient and outline to the drawing surface of the canvas at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> for the outline of the rectangle.</param>
		/// <param name="outlineWidth">The thickness in pixels of the outline of the rectangle.</param>
		/// <param name="gradientStartColor">The starting <see cref="Color"/> of the gradient to fill the rectangle with.</param>
		/// <param name="gradientEndColor">The ending <see cref="Color"/> of the gradient to fill the rectangle with.</param>
		/// <param name="x">The X dimension of the of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="y">The Y> dimension of the MikroBitmap to which to render the rectangle.</param>
		/// <param name="width">The Width dimension of the MikroBitmap to render the rectangle.</param>
		/// <param name="height">The Height dimension of the MikroBitmap to render the rectangle.</param>
		/// <param name="horizontal">If true, the gradient will be drawn horizontally, if false, it will be drawn vertically.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.Clear();
		///	_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, 0, 0, _oled.CanvasWidth, _oled.CanvasHeight, true);
		/// _oled.Flush();
		/// </code>
		/// <code language = "VB">
		/// _oled.Canvas.Clear()
		///	_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, 0, 0, _oled.CanvasWidth, _oled.CanvasHeight, True)
		/// _oled.Flush()
		/// </code>
		/// </example>
		public static void DrawRectangle(this MikroBitmap canvas, MikroColor outlineColor, Int32 outlineWidth, MikroColor gradientStartColor, MikroColor gradientEndColor, Int32 x, Int32 y, Int32 width, Int32 height, Boolean horizontal = false)
		{
			Int32 ow2 = outlineWidth * 2;

			// Draw Outline
			if (outlineWidth > 0)
			{
				FillRectangle(canvas, outlineColor, x, y, width, outlineWidth);
				FillRectangle(canvas, outlineColor, x, y + height - outlineWidth, width, outlineWidth);
				FillRectangle(canvas, outlineColor, x, y + outlineWidth, outlineWidth, height - ow2);
				FillRectangle(canvas, outlineColor, x + width - outlineWidth, y + outlineWidth, outlineWidth, height - ow2);
			}

			// Update Vars
			x += outlineWidth;
			y += outlineWidth;
			width -= ow2;
			height -= ow2;

			if (width == 0 || height == 0) return;

			// Check for same values
			if (gradientStartColor.Value == gradientEndColor.Value)
			{
				FillRectangle(canvas, gradientStartColor, x, y, width, height);
				return;
			}

			// Get Values
			Double r = gradientStartColor.R;
			Double g = gradientStartColor.G;
			Double b = gradientStartColor.B;
			Double rS;
			Double gS;
			Double bS;
			MikroColor c;


			if (horizontal)
			{
				rS = (Double)(gradientEndColor.R - gradientStartColor.R) / width;
				gS = (Double)(gradientEndColor.G - gradientStartColor.G) / width;
				bS = (Double)(gradientEndColor.B - gradientStartColor.B) / width;

				for (Int32 i = x; i < x + width; i++)
				{
					c = new MikroColor((Byte)r, (Byte)g, (Byte)b);
					FillRectangle(canvas, c, i, y, 1, height);
					r += rS;
					g += gS;
					b += bS;
				}
			}
			else
			{
				rS = (Double)(gradientEndColor.R - gradientStartColor.R) / height;
				gS = (Double)(gradientEndColor.G - gradientStartColor.G) / height;
				bS = (Double)(gradientEndColor.B - gradientStartColor.B) / height;

				for (Int32 i = y; i < y + height; i++)
				{
					c = new MikroColor((Byte)r, (Byte)g, (Byte)b);
					FillRectangle(canvas, c, x, i, width, 1);
					r += rS;
					g += gS;
					b += bS;
				}
			}
		}

		/// <summary>
		/// Fills a rectangular area with the specified MikroColor.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="fillcolor">The <see cref="MikroColor"/> used to fill with.</param>
		/// <param name="rect">The rectangular area to fill.</param>
		/// <returns>True if successful, otherwise false.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear();
		///	Rect myRect = new Rect(0, 0, 96, 48);
		///	_oled.Canvas.FillRectangle(KnownColors.Red, myRect);
		///	_oled.Flush();
		/// </code>
		/// <code language = "VB">
		///	_oled.Canvas.Clear()
		///	Dim myRect As Rect = new Rect(0, 0, 96, 48)
		///	_oled.Canvas.FillRectangle(KnownColors.Red, myRect)
		///	_oled.Flush()
		/// </code>
		/// </example>
		public static void FillRectangle(this MikroBitmap canvas, MikroColor fillcolor, Rect rect)
		{
			FillRectangle(canvas, fillcolor, rect.X, rect.Y, rect.Width, rect.Height);
		}

#endregion

#region Ellipse Extensions

		/// <summary>
		/// Renders a hollow ellipse on the MikroBitmap object at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline of the ellipse.</param>
		/// <param name="centerX">The center X dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="centerY">The center Y dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="xRadius">The X radius of the ellipse.</param>
		/// <param name="yRadius">The Y radius of the ellipse.</param>
		/// <remarks>To draw a perfect circle, set the xRadius and yRadius equal.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear();
		///	_oled.Canvas.DrawEllipse(KnownColors.White, 48, 48, 20, 20);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawEllipse(this MikroBitmap canvas, MikroColor outlineColor, Int32 centerX, Int32 centerY, Int32 xRadius, Int32 yRadius)
		{
			Int32 rx2 = xRadius * xRadius;
			Int32 ry2 = yRadius * yRadius;
			Int32 twoRx2 = 2 * rx2;
			Int32 twoRy2 = 2 * ry2;
			Int32 x = 0;
			Int32 y = yRadius;
			Int32 px = 0;
			Int32 py = twoRx2 * y;

			EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);

			// Region 1
			Int32 p = Round(ry2 - rx2 * yRadius + 0.25 * rx2);

			while (px < py)
			{
				x++;
				px += twoRy2;
				if (p < 0)
                {
                    p += ry2 + px;
                }
                else
				{
					y--;
					py -= twoRx2;
					p += ry2 + px - py;
				}

				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);
			}

			// Region 2
			p = Round(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

			while (y > 0)
			{
				y--;
				py -= twoRx2;
				if (p > 0)
                {
                    p += rx2 - py;
                }
                else
				{
					x++;
					px += twoRy2;
					p += rx2 - py + px;
				}
				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);
			}
		}

		/// <summary>
		/// Renders a filled ellipse on the MikroBitmap object at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline of the ellipse.</param>
		/// <param name="fillColor">The <see cref="Color"/> of the fill textColor of the ellipse.</param>
		/// <param name="centerX">The center X dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="centerY">The center Y dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="xRadius">The X radius of the ellipse.</param>
		/// <param name="yRadius">The Y radius of the ellipse.</param>
		/// <remarks>To draw a perfect circle, set the xRadius and yRadius equal.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear();
		///	_oled.Canvas.DrawEllipse(KnownColors.White, KnownColors.Blue, 48, 48, 20, 20);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawEllipse(this MikroBitmap canvas, MikroColor outlineColor, MikroColor fillColor, Int32 centerX, Int32 centerY, Int32 xRadius, Int32 yRadius)
		{
			Int32 rx2 = xRadius * xRadius;
			Int32 ry2 = yRadius * yRadius;
			Int32 twoRx2 = 2 * rx2;
			Int32 twoRy2 = 2 * ry2;
			Int32 x = 0;
			Int32 y = yRadius;
			Int32 px = 0;
			Int32 py = twoRx2 * y;

			EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);

			// Region 1
			Int32 p = Round(ry2 - rx2 * yRadius + 0.25 * rx2);

			Boolean bStart = false;
			Boolean bDrawn = false;

			while (px < py)
			{
				x++;
				px += twoRy2;
				if (p < 0)
                {
                    p += ry2 + px;
                }
                else
				{
					y--;
					py -= twoRx2;
					p += ry2 + px - py;
					bStart = true;
					bDrawn = false;
				}

				if (bStart && !bDrawn)
				{
					FillRectangle(canvas, fillColor, centerX - x, centerY - y, centerX + x - (centerX - x), 1);
					FillRectangle(canvas, fillColor, centerX - x, centerY + y, centerX + x - (centerX - x), 1);

					bDrawn = true;
				}

				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);
			}

			FillRectangle(canvas, fillColor, centerX - x, centerY - y + 1, centerX + x - (centerX - x) + 1, centerY + y - (centerY - y) - 1);

			// Region 2
			p = Round(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

			bDrawn = false;
			while (y > 0)
			{
				y--;
				py -= twoRx2;
				if (p > 0)
                {
                    p += rx2 - py;
                }
                else
				{
					x++;
					px += twoRy2;
					p += rx2 - py + px;
					bDrawn = false;
				}

				if (!bDrawn)
				{
					FillRectangle(canvas, fillColor, centerX - x, centerY - y, 1, centerY + y - (centerY - y));
					FillRectangle(canvas, fillColor, centerX + x, centerY - y, 1, centerY + y - (centerY - y));
					bDrawn = true;
				}

				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);
			}
		}

		/// <summary>
		/// Renders a  hollow ellipse on the MikroBitmap object at the specified location.
		/// </summary>
		/// <param name="canvas">The canvas surface to draw on.</param>
		/// <param name="outlineColor">The <see cref="Color"/> of the outline of the ellipse.</param>
		/// <param name="gradientStartColor">The starting <see cref="Color"/> of the gradient to fill the ellipse with.</param>
		/// <param name="gradientEndColor">The ending <see cref="Color"/> of the gradient to fill the ellipse with.</param>
		/// <param name="centerX">The center X dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="centerY">The center Y dimension to render the ellipse on the MikroBitmap.</param>
		/// <param name="xRadius">The X radius of the ellipse.</param>
		/// <param name="yRadius">The Y radius of the ellipse.</param>
		/// <remarks>To draw a perfect circle, set the xRadius and yRadius equal.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.Canvas.Clear();
		///	_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, 0, 0, _oled.CanvasWidth, _oled.CanvasHeight, true);
		///	_oled.Flush();
		/// </code>
		/// </example>
		public static void DrawEllipse(this MikroBitmap canvas, MikroColor outlineColor, MikroColor gradientStartColor, MikroColor gradientEndColor, Int32 centerX, Int32 centerY, Int32 xRadius, Int32 yRadius)
		{
			if (xRadius == 0 || yRadius == 0) return;
			if (xRadius == yRadius)
			{
				DrawEllipse(canvas, outlineColor, gradientStartColor, centerX, centerY, xRadius, yRadius);
				return;
			}

			Int32 rx2 = xRadius * xRadius;
			Int32 ry2 = yRadius * yRadius;
			Int32 twoRx2 = 2 * rx2;
			Int32 twoRy2 = 2 * ry2;
			Int32 x = 0;
			Int32 y = yRadius;
			Int32 px = 0;
			Int32 py = twoRx2 * y;

			// Get Values
			Double r = gradientStartColor.R;
			Double g = gradientStartColor.G;
			Double b = gradientStartColor.B;

			Double rb = gradientEndColor.R;
			Double gb = gradientEndColor.G;
			Double bb = gradientEndColor.B;

			Double rS = (Double)(gradientEndColor.R - gradientStartColor.R) / (yRadius * 2);
			Double gS = (Double)(gradientEndColor.G - gradientStartColor.G) / (yRadius * 2);
			Double bS = (Double)(gradientEndColor.B - gradientStartColor.B) / (yRadius * 2);
			MikroColor c;

			EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);

			// Region 1
			Int32 p = Round(ry2 - rx2 * yRadius + 0.25 * rx2);

			Boolean bStart = false;
			Boolean bDrawn = false;

			while (px < py)
			{
				x++;
				px += twoRy2;
				if (p < 0)
                {
                    p += ry2 + px;
                }
                else
				{
					y--;
					py -= twoRx2;
					p += ry2 + px - py;
					bStart = true;
					bDrawn = false;

					r += rS;
					b += bS;
					g += gS;

					rb -= rS;
					bb -= bS;
					gb -= gS;
				}

				if (bStart && !bDrawn)
				{
					c = new MikroColor((Byte)r, (Byte)g, (Byte)b);
					FillRectangle(canvas, c, centerX - x, centerY - y, centerX + x - (centerX - x), 1);

					c = new MikroColor((Byte)rb, (Byte)gb, (Byte)bb);
					FillRectangle(canvas, c, centerX - x, centerY + y, centerX + x - (centerX - x), 1);
					bDrawn = true;
				}
				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);
			}

			// Region 2
			p = Round(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

			while (y > 0)
			{
				y--;
				py -= twoRx2;
				if (p > 0)
                {
                    p += rx2 - py;
                }
                else
				{
					x++;
					px += twoRy2;
					p += rx2 - py + px;
				}

				c = new MikroColor((Byte)r, (Byte)g, (Byte)b);
				FillRectangle(canvas, c, centerX - x, centerY - y, centerX + x - (centerX - x) + 1, 1);
				c = new MikroColor((Byte)rb, (Byte)gb, (Byte)bb);
				FillRectangle(canvas, c, centerX - x, centerY + y, centerX + x - (centerX - x) + 1, 1);

				EllipsePlotPoints(canvas, centerX, centerY, x, y, outlineColor);

				r += rS;
				b += bS;
				g += gS;

				rb -= rS;
				bb -= bS;
				gb -= gS;
			}
		}

#endregion

#region Private Methods

		private static void Swap(ref Int32 a1, ref Int32 a2)
		{
			Int32 temp = a1;
			a1 = a2;
			a2 = temp;
		}

		private static Int32 Round(Double a)
		{
			return (Int32)(a + 0.5);
		}

		private static Boolean CheckTextBounds(MikroBitmap canvas, String text, Int32 x, Int32 y)
		{
			if (String.IsNullOrEmpty(text)) return false;
			return y <= canvas.ClippingRegion.Y + canvas.ClippingRegion.Height && x <= canvas.ClippingRegion.X + canvas.ClippingRegion.Width;
		}

		private static Int32 DrawChar(this MikroBitmap canvas, Char character, MikroFont font, Int32 x, Int32 y, MikroColor color, Boolean drawOpaque, MikroColor backgroundColor = null)
		{
			if (character < 32 || character > (font.ExtendedCharacterSet ? 255 : 126)) return font.Sizes[23] * (8 / font.FontHeight) - 1;

			// Get Lookup
			Int32 iIndex = font.Lookups[character - 32];
			Byte[] b = new Byte[font.Sizes[character - 32]];
			Array.Copy(font.FontData, iIndex, b, 0, b.Length);

			// Calculate width
			Int32 w = b.Length * 8 / font.FontHeight;

			// Fill background
			if (drawOpaque) FillRectangle(canvas, backgroundColor, x, y, w, font.FontHeight);

			Int32 cw = 0;
			for (Int32 i = 0; i < b.Length; i++)
			{
				if ((b[i] & 0x80) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x40) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x20) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x10) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x8) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x4) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x2) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw == w)
				{
					x -= cw;
					cw = 0;
					y += 1;
				}

				if ((b[i] & 0x1) != 0) SetPixel(canvas, color, x, y);
				x += 1;
				cw += 1;
				if (cw != w) continue;
				x -= cw;
				cw = 0;
				y += 1;
			}

			return w;
		}

		private static void FillRectangle(this MikroBitmap canvas, MikroColor color, Int32 x, Int32 y, Int32 width, Int32 height)
		{
			// Check bounds
			if (x >= canvas.ClippingRegion.X + canvas.ClippingRegion.Width || y >= canvas.ClippingRegion.Y + canvas.ClippingRegion.Height) return;

			// Adjust X
			if (x < 0)
			{
				width += x;
				x = 0;
			}
			if (x < canvas.ClippingRegion.X)
			{
				width -= canvas.ClippingRegion.X - x;
				x = canvas.ClippingRegion.X;
			}

			// Adjust Y
			if (y < 0)
			{
				height += y;
				y = 0;
			}

			if (y < canvas.ClippingRegion.Y)
			{
				height -= canvas.ClippingRegion.Y - y;
				y = canvas.ClippingRegion.Y;
			}

			// Adjust width
			if (x + width > canvas.ClippingRegion.X + canvas.ClippingRegion.Width) width = canvas.ClippingRegion.Width - canvas.ClippingRegion.X - x;

			// Adjust height
			if (y + height > canvas.ClippingRegion.Y + canvas.ClippingRegion.Height) height = canvas.ClippingRegion.Height - canvas.ClippingRegion.Y - y;

			Byte c1 = (Byte)(color.Value >> 8);
			Byte c2 = (Byte)(color.Value & 0xff);
			Int32 i;

			for (i = 0; i < height; i++)
			{
				Int32 l = (y + i) * canvas.Width * 2 + x * 2;
				Int32 ii;
				for (ii = 0; ii < width; ii++)
				{
					canvas.Pixels[l++] = c1;
					canvas.Pixels[l++] = c2;
				}
			}
		}

		private static void EllipsePlotPoints(this MikroBitmap canvas, Int32 centerX, Int32 centerY, Int32 radiusX, Int32 radiusY, MikroColor color)
		{
			SetPixel(canvas, color, centerX + radiusX, centerY + radiusY);
			SetPixel(canvas, color, centerX - radiusX, centerY + radiusY);
			SetPixel(canvas, color, centerX + radiusX, centerY - radiusY);
			SetPixel(canvas, color, centerX - radiusX, centerY - radiusY);
		}

#endregion

	}

}
