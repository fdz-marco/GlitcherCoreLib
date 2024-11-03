using System.Text;
using System.Text.RegularExpressions;
using TwinCAT.TypeSystem;

namespace glitcher.core.PLC.Beckhoff
{
    /// <summary>
    /// (Class/Object Definition) TwinCAT Structure: Tag Subscribed
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.04.26 - April 04, 2024
    /// </remarks>
    public class TagSubscribed
    {
        #region Properties

        public uint handler { get; set; }
        public string tagPath { get; set; }
        public string datatype { get; set; }
        public Type type { get { return GetTypeFromString(datatype, size); } }
        public int size { get; set; }
        public int cycleTime { get; set; }
        public int maxDelay { get; set; }
        public dynamic? value { get; set; } = null;
        public string? valueString { get { return GetStringFromValue(value, datatype, size); } }
        public DateTime timestamp { get; set; }
        public string time { get { return timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffff"); } }
        public bool logging { get; set; } = false;
        public ADSLoggerSQLite? log { get; set; } = null;

        #endregion

        #region Constructor

        public TagSubscribed(uint handler, string tagPath, string datatype, int size, int cycleTime, int maxDelay, dynamic? value)
        {
            this.handler = handler;
            this.tagPath = tagPath;
            this.datatype = datatype;
            this.size = size;
            this.cycleTime = cycleTime;
            this.maxDelay = maxDelay;
            this.value = value;
            this.timestamp = DateTime.Now;
        }

        #endregion

        #region Methods

        public async Task<bool> UpdateValue(dynamic? value, DateTime? timestamp = null)
        {
            // Variable doesnt changed
            if (value == this.value)
                return false;
            // Variable changed
            this.value = value;
            this.timestamp = (DateTime)((timestamp == null) ? DateTime.Now : timestamp);
            // Log if enabled
            if ((this.logging) && (this.log != null))
                await this.log.InsertValue(this.valueString, this.time);
            return true;
        }

        public static Type GetTypeFromString(string datatype, int size = 0)
        {
            Type? type = null;
            switch (datatype)
            {
                case "BOOL": type = typeof(System.Boolean); break;
                case "BOOLEAN": type = typeof(System.Boolean); break;

                case "SINT": type = typeof(System.SByte); break;     // 08 bit:: -128 > 127
                case "USINT": type = typeof(System.Byte); break;      // 08 bit:: 0 > 255
                case "INT": type = typeof(System.Int16); break;     // 16 bit:: -32768 > 32767
                case "UINT": type = typeof(System.UInt16); break;    // 16 bit:: 0 > 65535
                case "DINT": type = typeof(System.Int32); break;     // 32 bit:: -2147483648 > 2147483647
                case "UDINT": type = typeof(System.UInt32); break;    // 32 bit:: 0 > 4294967295
                case "LINT": type = typeof(System.Int64); break;     // 64 bit:: -2E63 > 2E63-1
                case "ULINT": type = typeof(System.UInt64); break;    // 64 bit:: 0 > 2E64-1

                case "REAL": type = typeof(System.Single); break;    // 32 bit:: -3.402823e+38 > 3.402823e+38
                case "LREAL": type = typeof(System.Double); break;    // 64 bit:: -1.7976931348623158e+308 > 1.7976931348623158e+308

                case "TIME": type = typeof(System.UInt32); break;    // 32 bit:: Milliseconds
                case "LTIME": type = typeof(System.UInt64); break;    // 64 bit:: Nanoseconds

                case "BYTE": type = typeof(System.Byte); break;      // 08 bit
                case "WORD": type = typeof(System.UInt16); break;    // 16 bit
                case "DWORD": type = typeof(System.UInt32); break;    // 32 bit
                case "LWORD": type = typeof(System.UInt64); break;    // 64 bit

                case "DATE": type = typeof(System.UInt32); break;    // 32 bit:: Seconds, although only the day is displayed.
                case "DATE_AND_TIME": type = typeof(System.UInt32); break;    // 32 bit:: Seconds
                case "DT": type = typeof(System.UInt32); break;    // 32 bit:: Seconds
                case "TIME_OF_DAY": type = typeof(System.UInt32); break;    // 32 bit:: Milliseconds
                case "TOD": type = typeof(System.UInt32); break;    // 32 bit:: Milliseconds

                //case "STRING":        type = typeof(System.String); break;
                //case "WSTRING":       type = typeof(System.String); break;

                case string s when s.StartsWith("STRING"):
                    type = typeof(System.String); break;

                case string s when s.StartsWith("WSTRING"):
                    int byteSize = new PrimitiveTypeMarshaler(Encoding.Unicode).MarshalSize(size);
                    type = new byte[byteSize].GetType(); break;

                default: type = typeof(System.String); break;
            }
            return type;
        }

        public static string GetStringFromValue(dynamic? value, string datatype, int size = 0)
        {
            Type type = GetTypeFromString(datatype);
            string valueString = "";
            switch (datatype)
            {
                case "BOOL": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "BOOLEAN": valueString = Convert.ChangeType(value, typeof(String)); break;

                case "SINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "USINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "INT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "UINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "DINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "UDINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "LINT": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "ULINT": valueString = Convert.ChangeType(value, typeof(String)); break;

                case "REAL": valueString = Convert.ChangeType(value, typeof(String)); break;
                case "LREAL": valueString = Convert.ChangeType(value, typeof(String)); break;

                case "TIME": valueString = IntegerToTimeFormat(value); break;
                case "LTIME": valueString = IntegerToTimeFormat(value); break;

                case "BYTE": valueString = IntegerToDecHexBinFormat(value, 8); break;
                case "WORD": valueString = IntegerToDecHexBinFormat(value, 16); break;
                case "DWORD": valueString = IntegerToDecHexBinFormat(value, 32); break;
                case "LWORD": valueString = IntegerToDecHexBinFormat(value, 64); break;

                case "DATE": valueString = UnixIntegerToDateFormat(value); break;
                case "DATE_AND_TIME": valueString = UnixIntegerToDateTimeFormat(value); break;
                case "DT": valueString = UnixIntegerToDateTimeFormat(value); break;

                case "TIME_OF_DAY": valueString = UnixIntegerToTimeFormat(value); break;
                case "TOD": valueString = UnixIntegerToTimeFormat(value); break;

                //case "STRING":        valueString = Convert.ChangeType(value, typeof(String)); break;
                //case "WSTRING":       valueString = Convert.ChangeType(value, typeof(String)); break;

                case string s when s.StartsWith("STRING"):
                    valueString = Convert.ChangeType(value, typeof(String)); break;

                case string s when s.StartsWith("WSTRING"):
                    valueString = WStringToStringFormat(value, size); break;

                default: valueString = Convert.ChangeType(value, typeof(String)); break;
            }
            return valueString;
        }

        #endregion

        #region Format Methods

        private static String IntegerToTimeFormat(dynamic? value)
        {
            if (value == null)
                return "<Null>";
            string valueString = Convert.ChangeType(value, typeof(String));
            int timeMiliseconds = Int32.Parse(valueString);
            int timeSeconds = (int)Math.Truncate((decimal)timeMiliseconds / 1000);
            int timeMilisecondsRemaining = (timeMiliseconds >= 1000) ? (timeMiliseconds % 1000) : timeMiliseconds;
            int timeMinutes = (int)Math.Truncate((decimal)timeSeconds / 60);
            int timeSecondsRemaining = (timeMiliseconds >= 60 * 1000) ? (timeSeconds % 60) : timeSeconds;
            int timeHours = (int)Math.Truncate((decimal)timeMinutes / 60);
            int timeMinutesRemaining = (timeMiliseconds >= 60 * 60 * 1000) ? (timeMinutes % 60) : timeMinutes;
            int timeDays = (int)Math.Truncate((decimal)timeHours / 24);
            int timeHoursRemaining = (timeMiliseconds >= 24 * 60 * 60 * 1000) ? (timeHours % 24) : timeHours;

            if (timeMiliseconds < 1000) { return String.Format("T#{0}MS", timeMilisecondsRemaining); }
            else if (timeMiliseconds < 60 * 1000) { return String.Format("T#{0}S {1}MS", timeSecondsRemaining, timeMilisecondsRemaining); }
            else if (timeMiliseconds < 60 * 60 * 1000) { return String.Format("T#{0}M {1}S {2}MS", timeMinutesRemaining, timeSecondsRemaining, timeMilisecondsRemaining); }
            else if (timeMiliseconds < 24 * 60 * 60 * 1000) { return String.Format("T#{0}H {1}M {2}S {3}MS", timeHoursRemaining, timeMinutesRemaining, timeSecondsRemaining, timeMilisecondsRemaining); }
            else { return String.Format("T#{0}D {1}H {2}M {3}S {4}MS", timeDays, timeHoursRemaining, timeMinutesRemaining, timeSecondsRemaining, timeMilisecondsRemaining); }
        }

        private static String IntegerToDecHexBinFormat(dynamic? value, int bitSize = 8)
        {
            if (value == null)
                return "<Null>";
            Type? type = null;

            int padHex = 0;
            int padBin = 0;

            if (bitSize <= 8) { type = typeof(System.Byte); padBin = 08; padHex = 2; }   // 08 bit
            else if (bitSize <= 16) { type = typeof(System.UInt16); padBin = 16; padHex = 4; }   // 16 bit
            else if (bitSize <= 32) { type = typeof(System.UInt32); padBin = 32; padHex = 8; }   // 32 bit
            else { type = typeof(System.UInt64); padBin = 64; padHex = 16; }   // 64 bit

            var integerNumber = Convert.ChangeType(value, type);

            string dec = Convert.ToString(value, 10);
            string bin = Regex.Replace(Convert.ToString(value, 2).PadLeft(padBin, '0'), ".{4}", "$0_").TrimEnd('_');
            string hex = Regex.Replace(Convert.ToString(value, 16).PadLeft(padHex, '0'), ".{2}", "$0_").TrimEnd('_');

            return String.Format("({0}) Hex: {1} | Dec: {2}", dec, hex, bin);
        }

        private static String UnixIntegerToDateFormat(dynamic? value)
        {
            if (value == null)
                return "<Null>";
            string valueString = Convert.ChangeType(value, typeof(String));
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(valueString));
            return offset.ToString("D#yyyy-MM-dd");
        }

        private static String UnixIntegerToDateTimeFormat(dynamic? value)
        {
            if (value == null)
                return "<Null>";
            string valueString = Convert.ChangeType(value, typeof(String));
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(valueString));
            return offset.ToString("DT#yyyy-MM-dd HH:mm:ss");
        }

        private static String UnixIntegerToTimeFormat(dynamic? value)
        {
            if (value == null)
                return "<Null>";
            string valueString = Convert.ChangeType(value, typeof(String));
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(valueString) / 1000);
            return offset.ToString("TOD#HH:mm:ss");
        }

        private static String WStringToStringFormat(dynamic? value, int size)
        {
            if (value == null)
                return "<Null>";
            string valueString = "";
            PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(Encoding.Unicode);
            int byteSize = converter.MarshalSize(size);
            Type type = new byte[byteSize].GetType();
            byte[] readBuffer = new byte[byteSize];
            readBuffer = Convert.ChangeType(value, type);
            converter.Unmarshal(readBuffer.AsSpan(), out valueString);
            return valueString;
        }

        #endregion

        #region Log Methods

        public async void LogEnable()
        {
            if (this.log == null)
            {
                this.log = new ADSLoggerSQLite(this.tagPath);
                this.logging = true;
                if ((this.logging) && (this.log != null))
                    await this.log.InsertValue(this.valueString, this.time);
            }
        }

        public void LogDisable()
        {
            if (this.log == null) return;
            this.log.Dispose();
            this.log = null;
            this.logging = false;
        }

        public string LogToggle()
        {
            if ((this.logging) && (this.log != null))
            {
                LogDisable();
            }
            else
            {
                LogEnable();
            }
            return this.logging.ToString();
        }

        #endregion

    }
}