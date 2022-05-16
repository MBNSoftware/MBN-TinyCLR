/*
 * Ambient Click board driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http:///www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using Windows.Devices.Adc;
#else
using GHIElectronics.TinyCLR.Devices.Adc;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the AmbientClick driver
    /// <para><b>This is a generic device.</b></para>
    /// <para><b>Pins used :</b> An</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Exceptions;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    ///
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         private static AmbientClick _ambient;
    ///
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _ambient = new AmbientClick(Hardware.SocketFour);
    ///             }
    ///             catch (PinInUseException ex)
    ///             {
    ///                 Debug.Print(ex.Message);
    ///                 return;
    ///             }
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("Light intensity in mW/cm2 is {_ambient.ReadSensor(10):F0}");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports System.Threading
    /// Imports MBN
    /// Imports MBN.Modules
    /// Imports Microsoft.SPOT
    ///
    /// Namespace Example
    ///
    ///     Public Module Module1
    ///
    ///         Dim _ambient As AmbientClick
    ///
    ///         Sub Main()
    ///             _ambient = New AmbientClick(Hardware.SocketFour)
    ///             While True
    ///                 Debug.WriteLine("Light intensity in mW/cm2 is " <![CDATA[&]]> _ambient.ReadSensor(10).ToString("F0"))
    ///                 Thread.Sleep(1000)
    ///             End While
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public sealed class AmbientClick
    {
        #region .ctor

        public AmbientClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _ambient = AdcController.GetDefault().OpenChannel(socket.AdcChannel);
#else
            _ambient = AdcController.FromName(socket.AdcController).OpenChannel(socket.AdcChannel);
#endif
        }

#endregion

#region Private Fields

        private readonly AdcChannel _ambient;

#endregion

#region Public Methods

        /// <summary>
        /// Reads the light intensity in mW/cm2
        /// </summary>
        /// <param name="numberOfSamples">Read the Ambient click n-times to smooth out stray values.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Light intensity in mW/cm2 is {_ambient.ReadSensor(10):F0}");
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Light intensity in mW/cm2 is " <![CDATA[&]]> _ambient.ReadSensor(10).ToString("F0"))
        /// </code>
        /// </example>
        public Double ReadSensor(UInt16 numberOfSamples = 10)
        {
            if (numberOfSamples == 0) numberOfSamples = 1; // Don't want to divide by Zero.
            var average = 0.00;
            for (var i = 0; i < numberOfSamples - 1; i++) // Read n samples for smoothing.
            {
                average += _ambient.ReadValue();
                Thread.Sleep(1);
            }
            average /= numberOfSamples;
            return ((average * 3300) / 4095) / 7;
        }

#endregion
    }
}
