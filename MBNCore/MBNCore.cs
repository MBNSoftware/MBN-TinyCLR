

using System;
using System.Collections;

namespace MBN
{

    #region Enums
    /// <summary>
    /// Units used by the ITemperature interface
    /// </summary>
    public enum TemperatureUnits
    {
        /// <summary>
        /// Celsius unit
        /// </summary>
        Celsius,
        /// <summary>
        /// Fahrenheit unit
        /// </summary>
        Fahrenheit,
        /// <summary>
        /// Kelvin unit
        /// </summary>
        Kelvin
    }

    /// <summary>
    /// Temperature sources used by the ITemperature interface.
    /// </summary>
    public enum TemperatureSources
    {
        /// <summary>
        /// Measures the ambient (room) temperature.
        /// </summary>
        Ambient,
        /// <summary>
        /// Measures an object temperature, either via external sensor or IR sensor, for example.
        /// </summary>
        Object
    }

    /// <summary>
    /// Measurement modes used by the IHumidity interface.
    /// </summary>
    public enum HumidityMeasurementModes
    {
        /// <summary>
        /// Relative humidity measurement mode
        /// </summary>
        Relative,
        /// <summary>
        /// Absolute humidity measurement mode
        /// </summary>
        Absolute
    }

    /// <summary>
    /// Compensation modes for pressure sensors
    /// </summary>
    public enum PressureCompensationModes
    {
        /// <summary>
        /// Sea level compensated
        /// </summary>
        SeaLevelCompensated,
        /// <summary>
        /// Raw uncompensated
        /// </summary>
        Uncompensated
    }

    /// <summary>
    /// Power modes that may be applicable to a module
    /// </summary>
    public enum PowerModes : Byte
    {
        /// <summary>
        /// Module is turned off, meaning it generally can't perform measures or operate
        /// </summary>
        Off,
        /// <summary>
        /// Module is either in hibernate mode or low power mode (depending on the module)
        /// </summary>
        Low,
        /// <summary>
        /// Module is turned on, at full power, meaning it is fully functionnal
        /// </summary>
        On
    }

    /// <summary>
    /// Reset modes that may be applicable to a module
    /// </summary>
    public enum ResetModes : Byte
    {
        /// <summary>
        /// Software reset, which usually consists in a command sent to the device.
        /// </summary>
        Soft,
        /// <summary>
        /// Hardware reset, which usually consists in toggling a IO pin connected to the device.
        /// </summary>
        Hard
    }

    /// <summary>
    /// Interface used for drivers using humidity sensors
    /// </summary>
    public interface IHumidity
    {
        /// <summary>
        /// Reads the relative or absolute humidity value from the sensor.
        /// </summary>
        /// <returns>A single representing the relative/absolute humidity as read from the sensor, in percentage (%) for relative reading or value in case of absolute reading.</returns>
        Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative);

        /// <summary>
        /// Gets the raw data of the humidity value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Interface used for drivers using pressure sensors
    /// </summary>
    public interface IPressure
    {
        /// <summary>
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">Indicates if the pressure reading returned by the sensor is see-level compensated or not.</param>
        /// <returns>A single representing the pressure read from the source, in hPa (hectoPascal)</returns>
        Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated);

        /// <summary>
        /// Gets the raw data of the pressure value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }

    /// <summary>
    /// Interface used for drivers using temperature sensors
    /// </summary>
    public interface ITemperature
    {
        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient);

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }
    #endregion



    #region Bits manipulation class
    /// <summary>
    /// Bits manipulation class
    /// </summary>
    public class Bits
    {
        /// <summary>
        /// Determines whether a bit is set at a given position in a byte.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static Boolean IsBitSet(Byte value, Byte pos) => (value & (1 << pos)) != 0;

        /// <summary>
        /// Determines whether a bit is set at a given position in an Int16.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static Boolean IsBitSet(Int16 value, Byte pos) => (value & (1 << pos)) != 0;

        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static void Set(ref Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }
        }

        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static Byte Set(Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }

            return value;
        }

        /// <summary>
        /// Toggles a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be toggled.</param>
        /// <param name="index">The index of the bit.</param>
        public static void Toggle(ref Byte value, Byte index) => value ^= (Byte)(1 << index);

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static void Set(ref Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length - 1; i >= 0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7 - i), mask[i] == '1'); }
            }
            value = valTmp;
        }

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static Byte Set(Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length - 1; i >= 0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7 - i), mask[i] == '1'); }
            }
            return valTmp;
        }

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a binary mask.
        /// <para>Example : SetRegister(Registers.BW_RATE, 0x20, (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, Byte mask, Byte value) => (Byte)((originalValue & ~mask) | (value & mask));

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a string mask. Bits marked "1" in the String mask will be replaced by value.
        /// <para>Example : SetRegister(Registers.BW_RATE, "00001111", (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, String mask, Byte value)
        {
            var i = mask.Length - 1;
            var binMask = ParseBinary(mask);
            while (mask[i] != '1') { i--; }
            return (Byte)((originalValue & ~binMask) | ((value << (7 - i)) & binMask));
        }

        /// <summary>
        /// Parses a number given in a binary format string.
        /// </summary>
        /// <param name="input">The input string, representing bits positions, e.g. "01110011".</param>
        /// <returns>The Int32 representation of the binary number.</returns>
        public static Int32 ParseBinary(String input)
        {
            // Thanks to Jon Skeet for this one
            var output = 0;
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '1') { output |= 1 << (input.Length - i - 1); }
            }
            return output;
        }
    }
    #endregion

    #region DeviceInitialisationException
    /// <summary>
    /// Exception thrown when a new instance of a driver can't be created. It may be because of too short delays or bad commands sent to the device.
    /// </summary>
    [Serializable]
    public class DeviceInitialisationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        public DeviceInitialisationException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceInitialisationException(String message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DeviceInitialisationException(String message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        /// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
        public new Exception InnerException { get { return base.InnerException; } }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" /></PermissionSet>
        public override String ToString() => "DeviceInitialisationException : " + base.Message;
    }
    #endregion

    #region String extension
    /// <summary>
    /// Extensions methods for Strings
    /// </summary>
    public static class MBNStrings
    {
        /// <summary>
        /// Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded left : " + str.PadLeft(5, '0')); // Should display "Padded left : 00012"
        /// </code>
        /// </example>
        public static String PadLeft(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source = paddingChar + source;
            }
            while (source.Length < totalWidth);

            return source;
        }

        /// <summary>
        /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded right : " + str.PadRight(5, '0')); // Should display "Padded right : 12000"
        /// </code>
        /// </example>
        public static String PadRight(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source += paddingChar;
            }
            while (source.Length < totalWidth);

            return source;
        }

        /// <summary>
        /// Get substring of specified number of characters on the right.
        /// </summary>
        /// <param name="length">A Unicode padding character.</param>
        /// <returns>A new string that is a substring of specified number of characters on the right.
        public static String Right(this String value, Int32 length) => value.Substring(value.Length - length);
    }
    #endregion

    #region Enum extension
    /// <summary>
    ///     An Enumeration extension class providing additional functionality to Microsoft Net Framework (4.3).
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Extension method to determine is a specific flag is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.Contains(Alarms.TemperatureHigh)) Debug.Print("High Temperature Alarm Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.Contains(Alarms.TemperatureHigh) Then
        ///          Debug.Print("High Temperature Alarm Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean ContainsFlag(this Enum source, Enum flag)
        {
            var sourceValue = ToUInt64(source);
            var flagValue = ToUInt64(flag);

            return (sourceValue & flagValue) == flagValue;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flags">The parameter array of flags to test for.</param>
        /// <returns>True if the referenced array contains any one of the flags passed in the parameter array, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh)) Debug.Print("High Temperature and High Humidity Alarms Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh) Then
        ///          Debug.Print("High Temperature and High Humidity Alarms Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean ContainsAnyFlag(this Enum source, params Enum[] flags)
        {
            var sourceValue = ToUInt64(source);

            foreach (Enum flag in flags)
            {
                var flagValue = ToUInt64(flag);

                if ((sourceValue & flagValue) == flagValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.IsSet(Alarms.NoAlarm)) Debug.Print("No alarms present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.IsSet(Alarms.NoAlarm) Then
        ///         Debug.Print("No alarms present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean IsSet(this Enum source, Enum flag) => (Convert.ToUInt32(source.ToString()) & Convert.ToUInt32(flag.ToString())) != 0;

        private static UInt64 ToUInt64(Object value) => Convert.ToUInt64(value.ToString());
    }
    #endregion

    #region Bytes manipulation extension
    /// <summary>
    /// An utility class for 2's complements
    /// </summary>
    public static class MBNBytes
    {
        /// <summary>
        /// Gets the 2's complement of a Byte value
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>An Int32 representing the 2's complement</returns>
        public static Int32 TwoComplement(this Byte value)
        {
            if ((value & 0x80) != 0x80) return value;
            Int32 valtmp = value;
            return -1 * ((Byte)~valtmp) - 1;
        }

        /// <summary>
        /// Gets the Byte's 2's complement of a Integer value
        /// </summary>
        /// <param name="value">The Int32 value to convert.</param>
        /// <returns>An Byte representing the 2's complement</returns>
        public static Byte TwoComplement(this Int32 value) => (value & 0x80) != 0x80 ? (Byte)value : (Byte)(0xFF + value + 1);

        /// <summary>
        /// <para>This Extension method reverses the bytes in a Byte Array.</para>
        /// </summary>
        /// <param name="byteArray">The Byte[] that needs to be reversed.</param>
        /// <returns>
        /// <para>the reverse order of the byte array passed.</para>
        /// </returns>
        public static Byte[] Reverse(this Byte[] byteArray)
        {
            var buffer = new Stack();

            foreach (var b in byteArray)
            {
                buffer.Push(b);
            }

            var reversedBuffer = buffer.ToArray();

            for (Byte i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = (Byte)reversedBuffer[i];
            }

            return byteArray;
        }

    }
    #endregion

    #region Time extensions
    /// <summary>
    /// Extensions for Time methods
    /// </summary>
    public static class MBNTimeExtensions
    {
        /// <summary>
        /// Convert an 8-byte array from NTP format to .NET DateTime.
        /// </summary>
        /// <param name="ntpTime">NTP format 8-byte array containing date and time</param>
        /// <returns>A Standard .NET DateTime</returns>
        public static DateTime ToDateTime(this Byte[] ntpTime)
        {
            UInt64 intpart = 0;
            UInt64 fractpart = 0;

            for (var i = 0; i <= 3; i++)
                intpart = (intpart << 8) | ntpTime[i];

            for (var i = 4; i <= 7; i++)
                fractpart = (fractpart << 8) | ntpTime[i];

            var milliseconds = intpart * 1000 + (fractpart * 1000) / 0x100000000L;

            var timeSince1900 = TimeSpan.FromTicks((Int64)milliseconds * TimeSpan.TicksPerMillisecond);
            return new DateTime(1900, 1, 1).Add(timeSince1900);
        }
    }
    #endregion

    #region Storage class
    /// <summary>
    /// Storage class
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _storage1, _storage2;
    ///
    ///     public static void Main()
    ///     {
    ///        _storage1 = new EEpromClick(Hardware.SocketOne, memorySize: 256);  // Here, the original 8KB chip has been replace by a 256KB one ;)
    ///        _storage2 = new OnboardFlash();
    ///
    ///        Debug.Print("Address 231 before : " + _storage1.ReadByte(231));
    ///        _storage1.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _storage1.ReadByte(231));
    ///
    ///        _storage2.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _storage2.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public abstract class Storage
    {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in bytes.
        /// </value>
        public abstract Int32 Capacity { get; }
        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public abstract Int32 PageSize { get; }
        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public abstract Int32 SectorSize { get; }
        /// <summary>
        /// Gets the size of a block.
        /// </summary>
        /// <value>
        /// The size of a block in bytes.
        /// </value>
        public abstract Int32 BlockSize { get; }
        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public virtual Int32 PagesPerCluster { get { return 4; } }

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.</remarks>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseChip();
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseChip();

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseSector(10,1);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseSector(Int32 sector, Int32 count);

        /// <summary>
        /// Erases "count" blocks starting at "sector".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseBlock(10,2);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseBlock(Int32 block, Int32 count);

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new EEpromClick(Hardware.Socket2);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void WriteData(Int32 address, Byte[] data, Int32 index, Int32 count);
        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new FlashClick(Hardware.Socket1);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count);

        /// <summary>
        /// Reads a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>A byte value</returns>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public Byte ReadByte(Int32 address)
        {
            var tmp = new Byte[1];
            ReadData(address, tmp, 0, 1);
            return tmp[0];
        }

        /// <summary>
        /// Writes a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write at the specified address.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public void WriteByte(Int32 address, Byte value) => WriteData(address, new[] { value }, 0, 1);
    }
    #endregion


}

