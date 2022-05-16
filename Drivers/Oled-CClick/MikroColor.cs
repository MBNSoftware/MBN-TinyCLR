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
namespace nanoFramework.Presentation.Media
#else
// ReSharper disable once CheckNamespace
namespace Microsoft.SPOT.Presentation.Media
#endif
{
	/// <summary>
	/// An abstract class with support for the RGB 565 Color format.
	/// </summary>
	public class MikroColor
	{

#region Fields

		/// <summary>
		/// The Blue component value
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value);
		///	Debug.Print("newColor B Component Value - " + newColor.B);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor As MikroColor = New MikroColor(KnownColors.Gray.Value)
		///	Debug.Print("newColor B Component Value - " <![CDATA[&]]> newColor.B)
		/// </code>
		/// </example>
		public readonly Byte B;

		/// <summary>
		/// The Green component value
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value);
		///	Debug.Print("newColor G Component Value - " + newColor.G);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor As MikroColor = New MikroColor(KnownColors.Gray.Value)
		///	Debug.Print("newColor G Component Value - " <![CDATA[&]]> newColor.G)
		/// </code>
		/// </example>
		public readonly Byte G;

		/// <summary>
		/// The Red component value
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value);
		///	Debug.Print("newColor R Component Value - " + newColor.R);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor As MikroColor = New MikroColor(KnownColors.Gray.Value)
		///	Debug.Print("newColor R Component Value - " <![CDATA[&]]> newColor.R)
		/// </code>
		/// </example>
		public readonly Byte R;

		/// <summary>
		/// The 565 Color (BBBBB, GGGGGG, RRRRR) representation of a color
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value);
		///	Debug.Print("newColor Value - " + newColor.Value);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor As MikroColor = New MikroColor(KnownColors.Gray.Value)
		///	Debug.Print("newColor Value - " <![CDATA[&]]> newColor.Value)
		/// </code>
		/// </example>
		public readonly UInt16 Value;

#endregion

#region Constructors

		/// <summary>
		/// Creates a new <see cref="MikroColor"/> by RGB (8-8-8) component values
		/// </summary>
		/// <param name="red">The Red component of a RGB color.</param>
		/// <param name="green">The Blue component of a RGB color.</param>
		/// <param name="blue">The Green component of a RGB color.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(128, 128, 128);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor As MikroColor = New MikroColor(128, 128, 128)
		/// </code>
		/// </example>
		public MikroColor(Byte red, Byte green, Byte blue)
		{
			R = red;
			G = green;
			B = blue;
			Value = (UInt16)((blue >> 3) | ((green & 0xFC) << 3) | ((red & 0xF8) << 8));
		}

		/// <summary>
		/// Creates a new <see cref="MikroColor"/> by RGB (5-6-5) value.
		/// </summary>
		/// <param name="value">The unsigned short (UInt16) of the RGB color value.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// MikroColor newColor1 = new MikroColor(KnownColors.Gray.Value);
		/// MikroColor newcolr2 = new MikroColor(0x2233);
		/// </code>
		/// <code language = "VB">
		///	Dim newColor1 As MikroColor = New MikroColor(KnownColors.Gray.Value)
		///	Dim newColor2 As MikroColor = New MikroColor(<![CDATA[&]]>H2233)
		/// </code>
		/// </example>
		public MikroColor(UInt16 value)
		{
			B = (Byte)(value << 3);
			G = (Byte)((value >> 5) << 2);
			R = (Byte)((value >> 11) << 3);
			Value = value;
		}

#endregion

#region Public Methods

		/// <summary>
		/// Returns the string representation of the <see cref="MikroColor"/>.
		/// </summary>
		/// <returns>{R, G, B}</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value);
		///	Debug.Print(newColor.ToString());
		/// </code>
		/// <code language = "VB">
		///	MikroColor newColor = new MikroColor(KnownColors.Gray.Value)
		///	Debug.Print(newColor.ToString())
		/// </code>
		/// </example>
		public override String ToString()
		{
			return "{" + R + ", " + G + ", " + B + "}";
		}

#endregion

	}
}