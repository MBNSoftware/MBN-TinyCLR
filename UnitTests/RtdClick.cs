using nanoFramework.TestFramework;
using System;
using MBN.Modules;
using System.Diagnostics;

namespace nanoFramework.ClickDrivers.UnitTests
{
    [TestClass]
    public class RtdClick
    {
        [Setup]
        public void RunSetup()
        {
            Debug.WriteLine("Setup");
            //var click = new RTDClick(socket etc.);
        }

        [TestMethod]
        public void convert_celsius_to_fahrenheit()
        {
            Debug.WriteLine("Convert Celsius to Fahrenheit");
            Assert.True(true);
        }

        [Cleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Cleanup");
        }
    }
}
