namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using System.Runtime.InteropServices;

    public class Utility
    {
        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);

        public static void ChangeSystemDateTime(DateTime value)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var st = new SYSTEMTIME();
            st.wYear = (short)value.Year;
            st.wMonth = (short)value.Month;
            st.wDay = (short)value.Day;
            st.wHour = (short)value.Hour;
            st.wMinute = (short)value.Minute;
            st.wSecond = (short)value.Second;
            SetSystemTime(ref st);
        }
    }
}
