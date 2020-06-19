using GHIElectronics.TinyCLR.Devices.I2c;

using System;

namespace MBN.Modules
{
    public sealed class RTC3Click
    {
        #region .ctor

        public RTC3Click(Hardware.Socket socket)
        {
            _rtc3 = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(_slaveAddress, 400000));
        }

        #endregion

        #region Private Fields

        private static I2cDevice _rtc3;
        private UInt32 _century = 2000;

        #endregion

        #region Private Constants

        // Register address
        private const Byte _slaveAddress = 0x68;

        // Register addresses
        private const Byte RTC3_REG_TIME_SEC = 0x00;
        private const Byte RTC3_REG_TIME_HOUR = 0x02;
        private const Byte RTC3_REG_CONFIG1 = 0x07;
        private const Byte RTC3_REG_TRICKLE_CHARGE2 = 0x08;
        private const Byte RTC3_REG_CONFIG2 = 0x09;
        private const Byte RTC3_REG_SF_KEY1 = 0x20;
        private const Byte RTC3_REG_SF_KEY2 = 0x21;
        private const Byte RTC3_REG_SFR = 0x22;

        #endregion

        #region Internal DateTme Structure

        private struct DateTimeStruct
        {
            internal Int32 seconds;
            internal Int32 minutes;
            internal Int32 hours;
            internal Int32 dayOfWeek;
            internal Int32 day;
            internal Int32 month;
            internal Int32 years;
        }

        #endregion

        #region Public ENUMS

        public enum CalibrationSign
        {
            Slowdown = 0,
            Speedup = 1
        }

        public enum CalibrationFrequency
        {
            Hertz_512 = 0,
            Hertz_1 = 1
        }

        #endregion

        #region Public Properties

        public Boolean RTCEnabled
        {
            get
            {
                Byte[] registerData = ReadRegister(RTC3_REG_TIME_SEC, 1);
                return (registerData[0] & 0x80) == 0x00;
            }
            set
            {
                Byte[] registerData = ReadRegister(RTC3_REG_TIME_SEC, 1);
                Bits.Set(ref registerData[0], 7, !value);
                WriteRegister(RTC3_REG_TIME_SEC, registerData);
            }
        }

        public UInt32 Century
        {
            get => _century;
            set
            {
                if (value % 100 != 0) throw new ArgumentException("The century valye must be evenly divisible by 100.");
                _century = value;
            }
        }

        #endregion

        #region Public Methods

        public DateTime GetDateTime()
        {
            Byte[] registerData = ReadRegister(RTC3_REG_TIME_SEC, 7);

            DateTimeStruct clockData = new DateTimeStruct();

            Int32 ones = registerData[0] & 0x0F;
            Int32 tens = (registerData[0] & 0x70) >> 4;
            clockData.seconds = 10 * tens + ones;

            ones = registerData[1] & 0x0F;
            tens = (registerData[1] & 0x70) >> 4;
            clockData.minutes = 10 * tens + ones;

            ones = registerData[2] & 0x0F;
            tens = (registerData[2] & 0x30) >> 4;
            clockData.hours = 10 * tens + ones;

            ones = registerData[4] & 0x0F;
            tens = (registerData[4] & 0x30) >> 4;
            clockData.day = 10 * tens + ones;

            ones = registerData[5] & 0x0F;
            tens = (registerData[5] & 0x100) >> 4;
            clockData.month = 10 * tens + ones;

            ones = registerData[6] & 0x0F;
            tens = (registerData[6] & 0xF0) >> 4;
            clockData.years = 10 * tens + ones;
            clockData.years += (Int32)_century;

            return new DateTime(clockData.years,
                clockData.month,
                clockData.day,
                clockData.hours,
                clockData.minutes,
                clockData.seconds
                );
        }

        public void SetDateTime(DateTime dateTme)
        {
            DateTimeStruct clockData = new DateTimeStruct();

            Int32 seconds = dateTme.Second;
            Int32 ones = seconds % 10;
            Int32 tens = (seconds / 10) << 4;
            clockData.seconds = tens | ones;

            Int32 minutes = dateTme.Minute;
            ones = minutes % 10;
            tens = (minutes / 10) << 4;
            clockData.minutes = tens | ones;

            Int32 hours = dateTme.Hour;
            ones = hours % 10;
            tens = (Byte)((hours / 10) << 4);
            clockData.hours = 0xC0 | tens | ones;

            Byte day = (Byte) dateTme.DayOfWeek;
            clockData.dayOfWeek = day + 1;

            Int32 dateDay = dateTme.Day;
            ones = dateDay % 10;
            tens = (dateDay / 10) << 4;
            clockData.day = tens | ones;

            Int32 month = dateTme.Month;
            ones = month % 10;
            tens = (month / 10) << 4;
            clockData.month = tens | ones;

            Int32 year = dateTme.Year % 100;
            ones = year % 10;
            tens = (year / 10) << 4;
            clockData.years = tens | ones;

            Byte centuryEnabledBits = (Byte)(ReadRegister(RTC3_REG_TIME_HOUR, 1)[0] & 0xC0);

            WriteRegister(RTC3_REG_TIME_SEC,
                new[]
                {
                    (Byte) ((RTCEnabled ? 0x00 : 0x80) | clockData.seconds),
                    (Byte) clockData.minutes,
                    (Byte) (centuryEnabledBits | clockData.hours),
                    (Byte) clockData.dayOfWeek,
                    (Byte) clockData.day,
                    (Byte) clockData.month,
                    (Byte) clockData.years
                });
        }

        public void EnableTrickleCharger(Boolean enable, Boolean bypassFET = false)
        {
            WriteRegister(RTC3_REG_TRICKLE_CHARGE2, new[] { (Byte)(enable ? 0x20 : 0x00) });
            Byte[] registerData = new Byte[1];
            registerData[0] = (Byte)(enable ? 0x05 : 0x00);
            registerData[0] |= (Byte)(bypassFET ? 0x40 : 0x00);
            WriteRegister(RTC3_REG_CONFIG2, registerData);
        }

        public void SetCalibration(CalibrationSign calibrationSign, Byte calibrationFactor)
        {
            if (calibrationFactor > 31) calibrationFactor = 31;
            Byte[] registerData = ReadRegister(RTC3_REG_CONFIG1, 1);
            registerData[0] |= (Byte)((Byte)calibrationSign << 5);
            registerData[0] |= calibrationFactor;
            WriteRegister(RTC3_REG_CONFIG1, registerData);
        }

        public void SetCalibrationFrequency(CalibrationFrequency frequency)
        {
            WriteRegister(RTC3_REG_SF_KEY1, new Byte[] { 0x5E });
            WriteRegister(RTC3_REG_SF_KEY2, new Byte[] { 0xC7 });
            WriteRegister(RTC3_REG_SFR, new[] { (Byte)(frequency == CalibrationFrequency.Hertz_512 ? 0x00 : 0x01) });
        }

        public void DoFrequencyTest(Boolean enable)
        {
            Byte[] registerData = ReadRegister(RTC3_REG_CONFIG1, 1);
            registerData[0] &= 0xBF;
            registerData[0] |= (Byte)(enable ? 0x40 : 0x00);
            WriteRegister(RTC3_REG_CONFIG1, registerData);
        }

        #endregion

        #region Private Methods

        private static Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] registerData = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _rtc3.WriteRead(new[] { registerAddress }, registerData);
            }
            return registerData;
        }

        private static void WriteRegister(Byte registerAddress, Byte[] registerData)
        {
            Byte[] writeBuffer = new Byte[registerData.Length + sizeof(Byte)];
            writeBuffer[0] = registerAddress;

            for (Byte x = 0; x < registerData.Length; x++)
            {
                writeBuffer[x + 1] = registerData[x];
            }

            lock (Hardware.LockI2C)
            {
                _rtc3.Write(writeBuffer);

            }
        }

        #endregion

    }
}