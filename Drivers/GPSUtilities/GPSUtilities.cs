using System;
using System.Text;

namespace MBN.Modules
{
    /// <summary>Main class for centralized GPS methods</summary>
    public static class GPSUtilities
    {
        /// <summary>Event handler for a decoded NMEA GSA sentence</summary>
        /// <param name="sender">GPS for NMEA sentences</param>
        /// <param name="e">The <see cref="GSAFrameEventArgs" /> instance containing the event data.</param>
        public delegate void GSAFrameEventHandler(Object sender, GSAFrameEventArgs e);
        /// <summary>Event handler for a decoded NMEA RMC sentence</summary>
        /// <param name="sender">GPS for NMEA sentences</param>
        /// <param name="e">The <see cref="RMCFrameEventArgs" /> instance containing the event data.</param>
        public delegate void RMCFrameEventHandler(Object sender, RMCFrameEventArgs e);
        /// <summary>Event handler for a decoded NMEA GGA sentence</summary>
        /// <param name="sender">GPS for NMEA sentences</param>
        /// <param name="e">The <see cref="GGAFrameEventArgs" /> instance containing the event data.</param>
        public delegate void GGAFrameEventHandler(Object sender, GGAFrameEventArgs e);
        /// <summary>Event handler for a decoded NMEA GSV sentence</summary>
        /// <param name="sender">GPS for NMEA sentences</param>
        /// <param name="e">The <see cref="GSVFrameEventArgs" /> instance containing the event data.</param>
        public delegate void GSVFrameEventHandler(Object sender, GSVFrameEventArgs e);
        /// <summary>Event handler for a decoded L86 proprietary sentence</summary>
        /// <param name="sender">L86 for Quectel L86 proprietary sentences</param>
        /// <param name="e">The <see cref="L86PMTKFrameEventArgs" /> instance containing the event data.</param>
        public delegate void L86PMTKFrameEventHandler(Object sender, L86PMTKFrameEventArgs e);
        /// <summary>Event handler for a decoded NMEA GSA sentence</summary>
        /// <param name="sender">L86 for Quectel L30 proprietary sentences</param>
        /// <param name="e">The <see cref="L30PSRFFrameEventArgs" /> instance containing the event data.</param>
        public delegate void L30PSRFFrameEventHandler(Object sender, L30PSRFFrameEventArgs e);

        /// <summary>Occurs when a NMEA RMC frame is received.</summary>
        public static event RMCFrameEventHandler RMCFrameReceived = delegate { };
        /// <summary>Occurs when a NMEA GGA frame is received.</summary>
        public static event GGAFrameEventHandler GGAFrameReceived = delegate { };
        /// <summary>Occurs when a NMEA GSA frame is received.</summary>
        public static event GSAFrameEventHandler GSAFrameReceived = delegate { };
        /// <summary>Occurs when a NMEA GSV frame is received.</summary>
        public static event GSVFrameEventHandler GSVFrameReceived = delegate { };
        /// <summary>Occurs when a NMEA PMTK frame is received.</summary>
        public static event L86PMTKFrameEventHandler L86PMTKFrameReceived = delegate { };
        /// <summary>Occurs when a NMEA PSRF frame is received.</summary>
        public static event L30PSRFFrameEventHandler L30PSRFFrameReceived = delegate { };

        /// <summary>  Returns the number of received frames from GPS.</summary>
        public static Int32 FrameCount
        {
            get;
            private set;
        }

        /// <summary>  Gets the complete name of the satellite system that sent the sentence</summary>
        /// <param name="shortName">The short name, two chars long.</param>
        /// <returns>A meaningfull string of the satellite system.</returns>
        public static String ResolveSignalOrigin(String shortName)
        {
            switch (shortName)
            {
                case "BD":
                case "GB":
                    return "BEIDOU";
                case "GA":
                    return "GALILEO";
                case "GP":
                    return "GPS";
                case "GL":
                    return "GLONASS";
                case "GN":
                    return "GPS+GLONASS";
                case "LC":
                    return "LORAN-C";
                case "OM":
                    return "OMEGA";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>Verifies the checksum of a given NMEA sentence</summary>
        /// <param name="frame">  The complete sentence, including $ and * chars.</param>
        /// <param name="checksum">The checksum to check</param>
        /// <returns>
        /// True if both checksums match, False otherwise.
        /// </returns>
        public static Boolean VerifyChecksum(String frame, Byte checksum)
        {
            var b = Encoding.UTF8.GetBytes(frame);
            Int32 calculatedChecksum = b[1];
            for (var i = 2; i < b.Length - 3; i++)
            {
                calculatedChecksum ^= b[i];
            }
            return checksum == calculatedChecksum;
        }

        /// <summary>Calculates the checksum of an NMEA sentence.</summary>
        /// <param name="sentence">The string representing the NMEA sentence, excluding the starting $ and the ending * chars.</param>
        /// <returns>The calculated checksum</returns>
        public static Byte CalculateChecksum(String sentence)
        {
            var b = Encoding.UTF8.GetBytes(sentence);
            Int32 checksum = b[0];
            for (var i = 1; i < b.Length; i++)
            {
                checksum ^= b[i];
            }
            return (Byte)checksum;
        }

        /// <summary>  Decodes an NMEA sentence.</summary>
        /// <param name="str">The NMEA sentence to decode.</param>
        public static void Parse(String str)
        {
            FrameCount++;
            // Internal GNSS Click (Quectel L86) commands/responses
            if (str.Substring(1, 4) == "PMTK")
            {
                L86PMTKFrameEventHandler L86PMTKEvent = L86PMTKFrameReceived;
                L86PMTKEvent("L86", new L86PMTKFrameEventArgs(str));
            }
            // Internal GPS2 Click (Quectel L30) commands/responses
            else
            if (str.Substring(1, 4) == "PSRF")
            {
                L30PSRFFrameEventHandler L30PSRFEvent = L30PSRFFrameReceived;
                L30PSRFEvent("L30", new L30PSRFFrameEventArgs(str));
            }
            else
            // NMEA messages
            {
                var frameKind = str.Substring(3, 3);
                switch (frameKind)
                {
                    case "GGA":
                        GGAFrameEventHandler GGAEvent = GGAFrameReceived;
                        GGAEvent("NMEA", new GGAFrameEventArgs(str));
                        break;
                    case "GSA":
                        GSAFrameEventHandler GSAEvent = GSAFrameReceived;
                        GSAEvent("NMEA", new GSAFrameEventArgs(str));
                        break;
                    case "RMC":
                        RMCFrameEventHandler RMCEvent = RMCFrameReceived;
                        RMCEvent("NMEA", new RMCFrameEventArgs(str));
                        break;
                    case "GSV":
                        GSVFrameEventHandler GSVEvent = GSVFrameReceived;
                        GSVEvent("NMEA", new GSVFrameEventArgs(str));
                        break;
                    default:
                        break;
                }
            }
        }

        #region GSA frame
        /// <summary>NMEA GSA sentence fields</summary>
        public class GSAFrameEventArgs
        {
            #region Fields
            /// <summary>Gets the signal origin.</summary>
            /// <value>The long name of the satellite system that sent the sentence</value>
            public String SignalOrigin
            {
                get;
            }
            /// <summary>  2D/3D mode</summary>
            /// <value>M=Manual, forced to operate in 2D or 3D<br />A=Automatic, 3D/2D</value>
            public Char Auto2D3D
            {
                get;
            }
            /// <summary>Gets the fix mode.</summary>
            /// <value>1=Fix not available<br />2=2D<br />3=3D</value>
            public Byte FixMode
            {
                get;
            }
            /// <summary>Gets the satellites IDs</summary>
            /// <value>IDs of the satellites used in position fix</value>
            public Byte[] PRN
            {
                get;
            }
            /// <summary>Gets the dilution of precision</summary>
            /// <value>Dilution of precision</value>
            public Single PDOP
            {
                get;
            }
            /// <summary>Gets the horizontal dilution of precision</summary>
            /// <value>Horizontal dilution of precision</value>
            public Single HDOP
            {
                get;
            }
            /// <summary>Gets the vertical dilution of precision</summary>
            /// <value>Vertical dilution of precision</value>
            public Single VDOP
            {
                get;
            }
            /// <summary>NMEA sentence checksum</summary>
            /// <value>The checksum appended at the end of the sentence.</value>
            public Byte Checksum
            {
                get;
            }
            #endregion
            /// <summary>Initializes a new instance of the <see cref="GSAFrameEventArgs" /> class.</summary>
            /// <param name="str">The NMEA GSA sentence from the GPS module.</param>
            public GSAFrameEventArgs(String str)
            {
                var tab = str.Split(',');
                SignalOrigin = ResolveSignalOrigin(str.Substring(1, 2));
                FixMode = Byte.Parse(tab[2]);
                Auto2D3D = String.IsNullOrEmpty(tab[1]) ? Char.MinValue : tab[1][0];
                Checksum = (Byte)Convert.ToInt32(str.Right(2), 16);
                PDOP = (Single)Double.Parse(tab[15]);
                HDOP = (Single)Double.Parse(tab[16]);
                VDOP = (Single)Double.Parse(tab[17].Split('*')[0]);
                PRN = new Byte[12];
                for (var i = 0; i < 12; i++)
                    PRN[i] = tab[3 + i] == "" ? (Byte)0 : Convert.ToByte(tab[3 + i]);
            }
        }
        #endregion

        #region RMC frame event
        /// <summary>NMEA RMC sentence fields</summary>
        public class RMCFrameEventArgs
        {
            #region Fields
            /// <summary>Gets the signal origin.</summary>
            /// <value>The long name of the satellite system that sent the sentence</value>
            public String SignalOrigin
            {
                get;
            }
            /// <summary>
            ///   Gets the fix time.
            /// </summary>
            /// <value>
            ///   The fix time HH:MM:SS.sss (depending on the module firmware)
            /// </value>
            public TimeSpan FixTime
            {
                get;
            }
            /// <summary>Gets a value indicating whether the frame contains valid data.</summary>
            /// <value>
            ///   <c>True</c> if frame has valid data, false otherwise</value>
            public Boolean ValidFrame
            {
                get;
            }
            /// <summary>Gets the latitude of the fix.</summary>
            /// <value>The latitude in degrees,minutes.seconds</value>
            public Single Latitude
            {
                get;
            }
            /// <summary>Gets the latitude hemisphere.</summary>
            /// <value>N for North hemisphere, S for South hemisphere</value>
            public Char LatitudeHemisphere
            {
                get;
            }
            /// <summary>Gets the longitude of the fix.</summary>
            /// <value>The longitude in degrees,minutes.seconds</value>
            public Single Longitude
            {
                get;
            }
            /// <summary>Gets the longitude direction.</summary>
            /// <value>W for West, E for East</value>
            public Char LongitudePosition
            {
                get;
            }
            /// <summary>Ground speed in knots.</summary>
            /// <value>The ground speed in knots.</value>
            public Single SpeedKnots
            {
                get;
            }
            /// <summary>Gournd speed in km/h.</summary>
            /// <value>The ground speed in km/h.</value>
            public Single SpeedKm
            {
                get;
            }
            /// <summary>True course</summary>
            /// <value>The true course.</value>
            public Single TrackAngle
            {
                get;
            }
            /// <summary>Gets the magnetic variation.</summary>
            /// <value>The magnetic variation, in degrees.</value>
            public Single MagneticVariation
            {
                get;
            }
            /// <summary>Gets the magnetic variation direction.</summary>
            /// <value>W for West, E for East</value>
            public Char MagneticVariationDirection
            {
                get;
            }

            /// <summary>NMEA sentence checksum</summary>
            /// <value>The checksum appended at the end of the sentence.</value>
            public Byte Checksum
            {
                get;
            }
            #endregion
            /// <summary>Initializes a new instance of the <see cref="RMCFrameEventArgs" /> class.</summary>
            /// <param name="str">The NMEA RMC sentence from the GPS module.</param>
            public RMCFrameEventArgs(String str)
            {
                var tab = str.Split(',');
                SignalOrigin = ResolveSignalOrigin(str.Substring(1, 2));
                FixTime = tab[1] != String.Empty
                    ? new TimeSpan(Convert.ToInt32(tab[1].Substring(0, 2)), Convert.ToInt32(tab[1].Substring(2, 2)), Convert.ToInt32(tab[1].Substring(4, 2)))
                    : new TimeSpan(0);
                ValidFrame = String.IsNullOrEmpty(tab[2]) ? false : tab[2][0] == 'A';
                Latitude = (Single)Double.Parse(tab[3]) / 100;
                LatitudeHemisphere = String.IsNullOrEmpty(tab[4]) ? Char.MinValue : tab[4][0];
                Longitude = (Single)Double.Parse(tab[5]) / 100;
                LongitudePosition = String.IsNullOrEmpty(tab[6]) ? Char.MinValue : tab[6][0];
                SpeedKnots = (Single)Double.Parse(tab[7]);
                SpeedKm = SpeedKnots * 1.852f;
                TrackAngle = (Single)Double.Parse(tab[8]);
                MagneticVariation = (Single)Double.Parse(tab[10]);
                MagneticVariationDirection = tab[12].Split('*')[0][0];
                Checksum = (Byte)Convert.ToInt32(str.Right(2), 16);
            }
        }
        #endregion

        #region GSV frame
        /// <summary>
        /// A structure containing necessary information to identify a Satellite.
        /// </summary>
        public struct Satellite
        {
            /// <summary>
            /// The heading of the horizon measured from true north and clockwise from the point where a vertical circle through a satellite intersects the horizon. 
            /// </summary>
            public UInt16 Azimuth;
            /// <summary>
            /// The height in heading above the horizon of the satellite position. 
            /// </summary>
            public UInt16 Elevation;
            /// <summary>
            /// The satellite's PRN Number (Pseudo-Random-Noise) used to identify a satellite.
            /// </summary>
            public UInt16 PRNNumber;
            /// <summary>
            /// Signal to Noise Ratio.
            /// </summary>
            public UInt16 SignalNoiseRatio;

            /// <summary>
            /// Instantiates a new Satellite structure.
            /// </summary>
            /// <param name="PRNNumber">The satellite's PRN Number (Pseudo-Random-Noise)</param>
            /// <param name="Elevation">The height in heading above the horizon of the satellite position.</param>
            /// <param name="Azimuth">The heading of the horizon measured from true north and clockwise from the point where a vertical circle through a satellite intersects the horizon. </param>
            /// <param name="SignalNoiseRatio">Signal to Noise Ratio.</param>
            public Satellite(UInt16 PRNNumber, UInt16 Elevation, UInt16 Azimuth, UInt16 SignalNoiseRatio)
            {
                this.PRNNumber = PRNNumber;
                this.Elevation = Elevation;
                this.Azimuth = Azimuth;
                this.SignalNoiseRatio = SignalNoiseRatio;
            }

            /// <summary>
            /// Returns a string that represents the current Satellite object.
            /// </summary>
            /// <returns>
            /// A string that represents the current Satellite object.
            /// </returns>
            public override String ToString() => "Satellite PRN: " + PRNNumber + ", Elevation: " + Elevation + ", Azimuth: " + Azimuth + ", Signal To Noise Ratio: " + SignalNoiseRatio;
        }

        public class GSVFrameEventArgs : EventArgs
        {
            #region Fields
            /// <summary>Gets the signal origin.</summary>
            /// <value>The long name of the satellite system that sent the sentence</value>
            public String SignalOrigin
            {
                get;
            }
            /// <summary>
            ///     Number of messages, total number of GPGSV messages being output (1-3)
            /// </summary>
            public Int32 NumberOfMessages
            {
                get; internal set;
            }

            /// <summary>
            ///     Sequence number of this entry (1~3)
            /// </summary>
            public Int32 SequenceNumber
            {
                get; internal set;
            }

            /// <summary>
            ///     Total satellites in view
            /// </summary>
            public Int32 SatellitesInView
            {
                get; internal set;
            }

            /// <summary>
            ///     Satellite ID1
            /// </summary>
            public Satellite SatelliteID1
            {
                get; internal set;
            }

            /// <summary>
            ///     Satellite ID2
            /// </summary>
            public Satellite SatelliteID2
            {
                get; internal set;
            }

            /// <summary>
            ///     Satellite ID3
            /// </summary>
            public Satellite SatelliteID3
            {
                get; internal set;
            }

            /// <summary>
            ///     Satellite ID4
            /// </summary>
            public Satellite SatelliteID4
            {
                get; internal set;
            }

            /// <summary>NMEA sentence checksum</summary>
            /// <value>The checksum appended at the end of the sentence.</value>
            public Byte Checksum
            {
                get;
            }

            /// <summary>
            ///     Provides a formatted string for GSV Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString() => "Number of Messages: " + NumberOfMessages + ", Sequence Number: " + SequenceNumber +
                       ", Satellites In View: " + SatellitesInView +
                       ", Satellite ID 1: " + SatelliteID1 + ", Satellite ID 2: " + SatelliteID2 +
                       ", Satellite ID 3: " + SatelliteID3 + ", Satellite ID 4: " + SatelliteID4;

            #endregion
            /// <summary>Initializes a new instance of the <see cref="GSVFrameEventArgs" /> class.</summary>
            /// <param name="str">The NMEA GSV sentence from the GPS module.</param>
            public GSVFrameEventArgs(String str)
            {
                var tokens = str.Split(',');
                SignalOrigin = ResolveSignalOrigin(str.Substring(1, 2));

                Satellite satId1 = (tokens.Length > 5)
                    ? new Satellite((UInt16)(tokens[4] != String.Empty ? UInt16.Parse(tokens[4]) : 0),
                        (UInt16)(tokens[5] != String.Empty ? UInt16.Parse(tokens[5]) : 0),
                        (UInt16)(tokens[6] != String.Empty ? UInt16.Parse(tokens[6]) : 0),
                        (UInt16)(tokens[7].Split('*')[0] == String.Empty ? 0 : UInt16.Parse(tokens[7].Split('*')[0])))
                    : new Satellite();

                Satellite satId2 = (tokens.Length > 9)
                    ? new Satellite((UInt16)(tokens[8] != String.Empty ? UInt16.Parse(tokens[8]) : 0),
                        (UInt16)(tokens[9] != String.Empty ? UInt16.Parse(tokens[9]) : 0),
                        (UInt16)(tokens[10] != String.Empty ? UInt16.Parse(tokens[10]) : 0),
                        (UInt16)(tokens[11].Split('*')[0] == String.Empty ? 0 : UInt16.Parse(tokens[11].Split('*')[0])))
                    : new Satellite();

                Satellite satId3 = (tokens.Length > 13)
                    ? new Satellite((UInt16)(tokens[12] != String.Empty ? UInt16.Parse(tokens[12]) : 0),
                        (UInt16)(tokens[13] != String.Empty ? UInt16.Parse(tokens[13]) : 0),
                        (UInt16)(tokens[14] != String.Empty ? UInt16.Parse(tokens[14]) : 0),
                        (UInt16)(tokens[15].Split('*')[0] == String.Empty ? 0 : UInt16.Parse(tokens[15].Split('*')[0])))
                    : new Satellite();

                Satellite satId4 = (tokens.Length > 17)
                    ? new Satellite((UInt16)(tokens[16] != String.Empty ? UInt16.Parse(tokens[16]) : 0),
                        (UInt16)(tokens[17] != String.Empty ? UInt16.Parse(tokens[17]) : 0),
                        (UInt16)(tokens[18] != String.Empty ? UInt16.Parse(tokens[18]) : 0),
                        (UInt16)(tokens[19].Split('*')[0] == String.Empty ? 0 : UInt16.Parse(tokens[19].Split('*')[0])))
                    : new Satellite();

                SatellitesInView = UInt16.Parse(tokens[3].Split('*')[0]);
                NumberOfMessages = Int32.Parse(tokens[1]);
                SequenceNumber = Int32.Parse(tokens[2]);
                SatelliteID1 = satId1;
                SatelliteID2 = satId2;
                SatelliteID3 = satId3;
                SatelliteID4 = satId4;
            }
        }
        #endregion

        #region GGA frame event
        /// <summary>NMEA GGA sentence fields</summary>
        public class GGAFrameEventArgs
        {
            #region Fields
            /// <summary>Gets the signal origin.</summary>
            /// <value>The long name of the satellite system that sent the sentence</value>
            public String SignalOrigin
            {
                get;
            }
            /// <summary>
            ///   Gets the fix time.
            /// </summary>
            /// <value>
            ///   The fix time HH:MM:SS.sss (depending on the module firmware)
            /// </value>
            public TimeSpan FixTime
            {
                get;
            }
            /// <summary>Gets the latitude of the fix.</summary>
            /// <value>The latitude in degrees,minutes.seconds</value>
            public Single Latitude
            {
                get;
            }
            /// <summary>Gets the latitude hemisphere.</summary>
            /// <value>N for North hemisphere, S for South hemisphere</value>
            public Char LatitudeHemisphere
            {
                get;
            }
            /// <summary>Gets the longitude of the fix.</summary>
            /// <value>The longitude in degrees,minutes.seconds</value>
            public Single Longitude
            {
                get;
            }
            /// <summary>Gets the longitude direction.</summary>
            /// <value>W for West, E for East</value>
            public Char LongitudePosition
            {
                get;
            }
            /// <summary>Gets the fix quality.</summary>
            /// <value>0 = Invalid<br />1 = GPS fix<br />2 = DGPS fix</value>
            public Byte FixQuality
            {
                get;
            }
            /// <summary>Gets the number of satellites tracked.</summary>
            /// <value>The number of satellites tracked.</value>
            public Byte SatellitesTracked
            {
                get;
            }
            /// <summary>Gets the horizontal dilution.</summary>
            /// <value>Relative accuracy of horizontal position</value>
            public Single HorizontalDilution
            {
                get;
            }
            /// <summary>Gets the altitude.</summary>
            /// <value>The altitude in <see cref="AltitudeUnit" /> property.
            /// </value>
            public Single Altitude
            {
                get;
            }
            /// <summary>Gets the altitude unit.</summary>
            /// <value>The altitude unit.</value>
            public Char AltitudeUnit
            {
                get;
            }
            /// <summary>Gets the height of the geoide.</summary>
            /// <value>Height of geoid above WGS84 ellipsoid</value>
            public Single GeoideHeight
            {
                get;
            }
            /// <summary>Gets the geoide height unit.</summary>
            /// <value>The geoide height unit.</value>
            public Char GeoideHeightUnit
            {
                get;
            }
            /// <summary>NMEA sentence checksum</summary>
            /// <value>The checksum appended at the end of the sentence.</value>
            public Byte Checksum
            {
                get;
            }
            #endregion
            /// <summary>Initializes a new instance of the <see cref="GGAFrameEventArgs" /> class.</summary>
            /// <param name="str">The NMEA GGA sentence from the GPS module.</param>
            public GGAFrameEventArgs(String str)
            {
                var tab = str.Split(',');
                SignalOrigin = ResolveSignalOrigin(str.Substring(1, 2));
                FixTime = tab[1] != String.Empty
                    ? new TimeSpan(Convert.ToInt32(tab[1].Substring(0, 2)), Convert.ToInt32(tab[1].Substring(2, 2)), Convert.ToInt32(tab[1].Substring(4, 2)))
                    : new TimeSpan(0);
                FixQuality = Byte.Parse(tab[6]);
                SatellitesTracked = Byte.Parse(tab[7]);
                AltitudeUnit = String.IsNullOrEmpty(tab[10]) ? Char.MinValue : tab[10][0];
                GeoideHeightUnit = String.IsNullOrEmpty(tab[12]) ? Char.MinValue : tab[12][0];
                Latitude = (Single)Double.Parse(tab[2]) / 100;
                LatitudeHemisphere = String.IsNullOrEmpty(tab[3]) ? Char.MinValue : tab[3][0];
                Longitude = (Single)Double.Parse(tab[4]) / 100;
                LongitudePosition = String.IsNullOrEmpty(tab[5]) ? Char.MinValue : tab[5][0];
                HorizontalDilution = (Single)Double.Parse(tab[8]);
                Altitude = (Single)Double.Parse(tab[9]);
                GeoideHeight = (Single)Double.Parse(tab[11]);
                Checksum = (Byte)Convert.ToInt32(str.Right(2), 16);
            }
        }
        #endregion

        #region PMTK frame
        /// <summary>Quectel L86 proprietary sentence fields</summary>
        public class L86PMTKFrameEventArgs : EventArgs
        {
            #region Fields
            /// <summary>Gets the splitted sentence.</summary>
            /// <value>The splitted sentence.</value>
            public String[] SplittedSentence
            {
                get;
            }

            /// <summary>Gets the type of the packet.</summary>
            /// <value>A 3-digits number indicating the type of the packet.</value>
            public Int32 PacketType
            {
                get;
            }
            #endregion
            /// <summary>Initializes a new instance of the <see cref="L86PMTKFrameEventArgs" /> class.</summary>
            /// <param name="str">The L86 proprietary sentence from the GPS module.</param>
            public L86PMTKFrameEventArgs(String str)
            {
                SplittedSentence = str.Split(',');
                PacketType = Convert.ToInt32(str.Substring(5, 3));
            }
        }
        #endregion

        #region PSRF frame
        /// <summary>Quectel L30 proprietary sentence fields</summary>
        public class L30PSRFFrameEventArgs : EventArgs
        {
            #region Fields
            /// <summary>Gets the splitted sentence.</summary>
            /// <value>The splitted sentence.</value>
            public String[] SplittedSentence
            {
                get;
            }
            /// <summary>Gets the type of the packet.</summary>
            /// <value>A 3-digits number indicating the type of the packet.</value>
            public Int32 PacketType
            {
                get;
            }
            #endregion
            /// <summary>Initializes a new instance of the <see cref="L30PSRFFrameEventArgs" /> class.</summary>
            /// <param name="str">The L30 proprietary sentence from the GPS module.</param>
            public L30PSRFFrameEventArgs(String str)
            {
                SplittedSentence = str.Split(',');
                PacketType = Convert.ToInt32(str.Substring(5, 3));
            }
        }
        #endregion
    }
}


