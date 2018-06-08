using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using exgeneric;

namespace exhelper
{
    public enum LogLevel
    {
        APP_RUN = 0,
        APP_DEBUG,
        FULL_DEBUG,
    }

    public class Logging
    {
        const String LOGFILE_TIMEFMT = "dd-MM-yyyy hh:mm:ss";
        const String LOGFILE_DELIMITER = "|";
        const String LOGFILE_LINEDELIMITER = "\r\n";
        const String LOGFILE_CYCLETIMEFMT = "yyMMdd";

        const Int32 GENERIC_WRITE = 0x0000000;
        const Int32 FILE_SHARE_READ = 0x1;
        const Int32 FILE_SHARE_WRITE = 0x2;
        const Int32 OPEN_ALWAYS = 4;
        const Int32 OPEN_EXISTING = 3;
        const Int32 FILE_ATTRIBUTE_NORMAL = 0x80;

        const Int32 FILE_BEGIN = 0;
        const Int32 FILE_CURRENT = 1;
        const Int32 FILE_END = 2;

        IntPtr mLogFileHandle;
        Boolean mLoggingEnabled;

        String LogFileName { get; set; }
        String LogCyclePath { get; set; }
        String LogCyclePrefix { get; set; }
        String LogCycleKind { get; set; }
        String LogCycleArgs { get; set; }
        DateTime NextLogCycleTime { get; set; }
        Int32 LogFileSizeLimit { get; set; }
        
        Boolean CycleOnTerminate { get; set; }
        public String LogMessagePrefix { private get; set; }
        String LogFileExtention { get; set; }

        private LogLevel mDebugLevel;
        public LogLevel DebugLevel
        {
            private get { return mDebugLevel; }
            set
            {
                if (value > LogLevel.FULL_DEBUG)
                {
                    mDebugLevel = LogLevel.FULL_DEBUG;
                }
                else
                {
                    mDebugLevel = value;
                }
            }
        }

        public Logging()
        {
            mLogFileHandle = IntPtr.Zero;
            LogFileName = String.Empty;
            LogCyclePath = String.Empty;
            LogCyclePrefix = String.Empty;
            LogCycleKind = String.Empty;
            LogCycleArgs = String.Empty;
            NextLogCycleTime = DateTime.Now;
            LogFileSizeLimit = 0;
            mLoggingEnabled = false;
            CycleOnTerminate = false;
            LogMessagePrefix = String.Empty;
        }

        ~Logging()
        {
            TerminateLogging();
        }

        public void TerminateLogging()
        {
            Boolean CycleNow = false;

            if (IntPtr.Zero == mLogFileHandle) return;

            CloseLogFile (mLogFileHandle);

            CycleNow = CycleOnTerminate || (String.Compare(LogCycleKind, "shutdown", true) != 0);

            if (CycleNow)
            {
                CycleFile(LogFileName, LogCyclePath, LogCyclePrefix, LOGFILE_CYCLETIMEFMT);
            }

            mLogFileHandle = IntPtr.Zero;
        }

        Boolean InitialiseLogging(String Filename, String CyclePath, String CycleFilePrefix, String CycleKind, String CycleArgs, Int32 SizeLimitKB, Boolean CycleOnStart, Boolean CycleOnClose, Boolean LoggingIsEnabled)
        {
            //   CycleKind and CycleArgs Values
            //      hourly      None
            //      n-hourly    Number of hours between Cycling
            //      daily       Hour of day 0-23 (default is midday)
            //      weekly      <DayOfWeek>;<HourOfDay> (DayOfWeek : 1-7 where Mon = 1 and HourOfDay : 0-23)
            //      startup     None
            //      shutdown    None

            long CurrentSize = 0;

            KernelImport.WIN32_FILE_ATTRIBUTE_DATA fileData;

            if (mLogFileHandle != IntPtr.Zero)
            {
                return false;
            }

            LogFileName = Filename;
            LogCyclePath = CyclePath;
            LogCyclePrefix = CycleFilePrefix;
            LogCycleKind = CycleKind;
            LogCycleArgs = CycleArgs;
            CycleOnTerminate = CycleOnClose;

            if (SizeLimitKB > 0)
            {
                LogFileSizeLimit = SizeLimitKB * 1024;
            }
            else
            {
                LogFileSizeLimit = -1;
            }

            if (KernelImport.GetFileAttributesEx(Filename, KernelImport.GET_FILEEX_INFO_LEVELS.GetFileExMaxInfoLevel, out fileData))
            {
                CurrentSize = fileData.nFileSizeLow;
            }
            else
            {
                CurrentSize = 0;
            }

            NextLogCycleTime = NextScheduledTime(DateTime.Now, CycleKind, CycleArgs);
            if (NextLogCycleTime == null)
            {
                NextLogCycleTime = NextScheduledTime(DateTime.Now, "shutdown", "");
            }

            CycleOnStart = (CurrentSize > 0 && (CycleOnStart || DateTime.Compare(NextLogCycleTime, DateTime.Now) < 0 || (LogFileSizeLimit > 0 && CurrentSize > LogFileSizeLimit)));
            if (CycleOnStart)
            {
                CycleFile(LogFileName, CyclePath, CycleFilePrefix, LOGFILE_CYCLETIMEFMT);
            }

            mLogFileHandle = OpenLogFile(LogFileName, false);

            mLoggingEnabled = LoggingIsEnabled;
            
            return (mLogFileHandle != IntPtr.Zero);
        }

        void DisableLogging()
        {
            mLoggingEnabled = false;
        }

        void EnableLogging()
        {
            mLoggingEnabled = true;
        }

        Boolean IsLoggingEnabled()
        {
            return mLoggingEnabled;
        }
        
        public Boolean ClearOldCycleLog(String FilePattern, String CycleLogPath, Int16 NoOfDays = 0)
        {
            DateTime limit;
            int result = 0;

            if (NoOfDays == 0)
            {
                return false;
            }

            limit = DateTime.Now.AddDays(-NoOfDays);

            if (!Directory.Exists(CycleLogPath))
            {
                throw new ArgumentException("Cycle log path is missing.");
            }

            if (!FilePattern.Contains("*.*"))
            {
                FilePattern = FilePattern + "*.*";
            }

            foreach (string current_file in Directory.EnumerateFiles(CycleLogPath, FilePattern))
            {
                result = DateTime.Compare(File.GetCreationTime(current_file), limit);
                if (result < 0)
                {
                    File.Delete(current_file);
                }
            }
            return true;
        }

        private void WriteSchedulerLog(String AgentName, params object[] Message)
        {
            FileStream file_stream;
            String path = Environment.CurrentDirectory + "\\" + AgentName + ".log";

            try
            {
                if (File.Exists(path))
                {
                    file_stream = File.Open(path, FileMode.Append);
                }
                else
                {
                    file_stream = File.Create(path);
                }

                StreamWriter stream_writer = new StreamWriter(file_stream);

                for (int i = 0; i < Message.Length; i++)
                {
                    stream_writer.WriteLine(Message[i].ToString() + DateTime.Now.ToString());
                }

                file_stream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void AppendToOpenLogFile(IntPtr hFile, Boolean UseLogPrefix, params object[] Entries)
        {
            StringBuilder sb = null;
            byte[] buffer = null;
            long Index;
            uint uintval = 0;

            if (hFile == IntPtr.Zero)
            {
                return;
            }

            sb = new StringBuilder();

            sb.Append(DateTime.Now.ToString(LOGFILE_TIMEFMT) + LOGFILE_DELIMITER);

            if (UseLogPrefix)
            {
                sb.Append(LogMessagePrefix + LOGFILE_DELIMITER);
            }

            if (Entries.Length != 0 || Entries != null)
            {
                foreach (object val in Entries)
                {
                    if (!val.GetType().IsArray)
                    {
                        if (val is String)
                        {
                            sb.Append(val + LOGFILE_DELIMITER);
                        }
                        else
                        {
                            String converted_val;
                            try
                            {
                                converted_val = (String)Convert.ChangeType(val, typeof(String));
                                sb.Append(converted_val + LOGFILE_DELIMITER);
                            }
                            catch (Exception)
                            {
                                // ignore this value and move on .. 
                            }
                        }
                    }
                }

                sb.Append(LOGFILE_LINEDELIMITER);

                // move to end of file
                Index = KernelImport.SetFilePointer(hFile, 0, out uintval, KernelImport.EMoveMethod.End);
                if (Index == 0xFFFFFFFF) return;
                try
                {
                    buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    // lock the required portion of file
                    if (KernelImport.LockFile(hFile, (uint)Index, 0, (uint)buffer.Length, 0) == false) return;

                    // write and unlock
                    if (!KernelImport.WriteFile(hFile, buffer, (uint)buffer.Length, out uintval, IntPtr.Zero))
                    {
                        Index = KernelImport.GetLastError();
                    }

                    KernelImport.UnlockFile(hFile, (uint)Index, 0, (uint)buffer.Length, 0);
                    KernelImport.FlushFileBuffers(hFile);
                }
                catch (Exception) { }
            }
        }

        Boolean CycleFile(String FileToCycle, String CyclePath, String CycleFilePrefix, String TimeFmtStr)
        {
            Int32 RetryCount = 10;
            String pattern = String.Empty, extrn = String.Empty, place=string.Empty;
            pattern = Path.Combine(CyclePath, CycleFilePrefix + DateTime.Now.ToString(TimeFmtStr));

            if (!File.Exists(FileToCycle)) return false;

            while (RetryCount > 0)
            {
                extrn = NextCycleExtension(extrn);

                if (String.IsNullOrEmpty(extrn)) break;

                place = pattern + "." + extrn;
                if (!File.Exists(place))
                {
                    try
                    {
                        File.Move(FileToCycle, place);
                        break;
                    }
                    catch (Exception)
                    {
                        RetryCount--;
                    }
                }
            }
            return false;
        }

        void ResetLogCycleConfig(String CycleKind, String CycleArgs, Int32 SizeLimitKB, Boolean CycleNow, Boolean LoggingIsEnabled)
        {
            DateTime NextScheduled;
            Boolean needcycle = false;

            NextScheduled = NextScheduledTime(DateTime.Now, CycleKind, CycleArgs);
            if (NextScheduled == null) return;

            if (mLogFileHandle != IntPtr.Zero)
            {
                needcycle = (
                                CycleNow ||
                                DateTime.Compare(DateTime.Now, (DateTime)NextScheduled) > 0 ||
                                IsLogFilePastSizeLimit(mLogFileHandle, SizeLimitKB * 1024)
                            );
            }

            if (needcycle)
            {
                CloseLogFile(mLogFileHandle);

                if (!CycleFile(LogFileName, LogCyclePath, LogCyclePrefix, LOGFILE_CYCLETIMEFMT))
                { 
                    throw new Exception("couldnt cycle the file"); 
                }

                mLogFileHandle = OpenLogFile(LogFileName, true);
            }

            LogCycleKind = CycleKind;
            LogCycleArgs = CycleArgs;

            mLoggingEnabled = LoggingIsEnabled;
                
            if (SizeLimitKB > 0)
            {
                LogFileSizeLimit = SizeLimitKB * 1024;
            }
            else
            {
                LogFileSizeLimit = -1;
            }
        }

        String NextCycleExtension(String CurrentExtn)
        {
            Int32 N = 0;

            String AlphaStr = String.Empty;

            if (String.IsNullOrEmpty(CurrentExtn))
            {
                return "LOG";
            }

            if (CurrentExtn.Equals("LOG"))
            {
                return "001";
            }

            if (CurrentExtn.Equals("ZZZ")) return String.Empty;

            N = Int32.Parse(CurrentExtn);

            if (N > 0 && N <= 998)
            {
                return String.Format("{0:D3}", N + 1);
            }

            if (N == 999)
            {
                return "00A";
            }
    
            N = AlphaMathGetNumber(CurrentExtn) + 1;

            return AlphaMathGetString(N);
        }

        void CalcNextCycleTime_GetArgs(String IntervalArg, out Int32 IntArg1, out Int32 IntArg2)
        {
            Int32 FirstIndex = 0, SecondIndex = 0;
            String arg1, arg2;

            arg1 = String.Empty;
            arg2 = String.Empty;

            IntArg1 = IntArg2 = 0;

            FirstIndex = IntervalArg.IndexOf(";");
            if (FirstIndex > -1)
            {
                arg1 = IntervalArg.Substring(0, FirstIndex - 1);

                SecondIndex = IntervalArg.IndexOf(";", FirstIndex + 1);

                if (SecondIndex > -1)
                {
                    arg2 = IntervalArg.Substring(FirstIndex + 1, SecondIndex - FirstIndex + 1);
                }
                else
                {
                    arg2 = IntervalArg.Substring(FirstIndex + 1);
                }

                if (!String.IsNullOrEmpty(arg1)) arg1 = arg1.Trim();
                if (!String.IsNullOrEmpty(arg2)) arg2 = arg2.Trim();
            }
            else
            {
                arg1 = IntervalArg.Trim();
            }

            if (arg1.Length > 0)
            {
                IntArg1 = Convert.ToInt32(arg1);
            }

            if (arg2.Length > 0)
            {
                IntArg2 = Convert.ToInt32(arg2);
            }
        }

        DateTime NextScheduledTime(DateTime RefTime, String IntervalKind, String IntervalArg)
        {
            DateTime NewValue;
            Int32 arg1, arg2;
            int Spot = 0;

            switch (IntervalKind.Trim().ToLower())
            {
                case "hourly":
                    // set it to the next full hour
                    NewValue = RefTime.AddHours(1);

                    //Remove Minutes and seconds
                    NewValue = ((DateTime)NewValue).AddMinutes(-1 * ((DateTime)NewValue).Minute);
                    NewValue = ((DateTime)NewValue).AddSeconds(-1 * ((DateTime)NewValue).Second);
                    break;
                case "n-hourly":
                    // IntervalArg is 'n' possibly semi-colon delimited
                    CalcNextCycleTime_GetArgs(IntervalArg, out arg1, out arg2);

                    if (arg1 <= 0 || arg1 >= 24) arg1 = 1;

                    NewValue = RefTime.AddHours(arg1);
                    NewValue = ((DateTime)NewValue).AddMinutes(-1 * ((DateTime)NewValue).Minute);
                    NewValue = ((DateTime)NewValue).AddSeconds(-1 * ((DateTime)NewValue).Second);
                    break;
                case "daily":
                    // IntervalArg is 'hour' of the day : 0-23
                    CalcNextCycleTime_GetArgs(IntervalArg, out arg1, out arg2);

                    if (arg1 <= 0 || arg1 > 23) arg1 = 12; // midday!

                    if (arg1 > RefTime.Hour)
                    {
                        NewValue = RefTime.AddHours(arg1 - RefTime.Hour);
                    }
                    else
                    {
                        NewValue = RefTime.AddHours(24 - RefTime.Hour + arg1);
                    }
                    NewValue = ((DateTime)NewValue).AddMinutes(-1 * ((DateTime)NewValue).Minute);
                    NewValue = ((DateTime)NewValue).AddSeconds(-1 * ((DateTime)NewValue).Second);
                    break;
                case "weekly":
                    // IntervalArg is 'day' 1-7 Mon=1 and 'hour' : 0-23 semi colon delimited
                    CalcNextCycleTime_GetArgs(IntervalArg, out arg1, out arg2);

                    if (arg1 <= 0 || arg1 > 6) arg1 = 1;

                    if (arg2 <= 0 || arg2 > 23) arg2 = 12;

                    Spot = (int)RefTime.DayOfWeek;

                    if (Spot > arg1)
                    {
                        Spot = 7 - Spot + arg1;
                    }
                    else if (Spot == arg1)
                    {
                        Spot = 7;
                    }
                    else if (Spot < arg1)
                    {
                        Spot = 7 - Spot + arg1;
                    }

                    NewValue = RefTime.AddDays(Spot);

                    if (RefTime.Hour > arg2)
                    {
                        NewValue = ((DateTime)NewValue).AddHours(RefTime.Hour - arg2);
                    }

                    NewValue = ((DateTime)NewValue).AddMinutes(-1 * ((DateTime)NewValue).Minute);
                    NewValue = ((DateTime)NewValue).AddSeconds(-1 * ((DateTime)NewValue).Second);
                    break;
                case "startup":
                    // force to a time in the past : minus a second
                    NewValue = RefTime.AddSeconds(-1);
                    break;
                case "shutdown":
                    // push it 10 years into the future
                    NewValue = RefTime.AddYears(10);
                    break;
                case "sizecycle":
                    // push it 10 years into the future
                    NewValue = RefTime.AddYears(10);
                    break;
                default:
                    NewValue = DateTime.Now;
                    break;
            }

            return NewValue;
        }

        Int32 AlphaMathGetNumber(String AlphaMathString)
        {
            Int32 i, j = 0, N = 0, Len;
            String s = String.Empty;
            const Int32 asc = 65;
            char[] c;

            if (String.IsNullOrEmpty(AlphaMathString)) return 0;

            s = AlphaMathString.Trim().ToUpper();

            c = s.ToCharArray();
            Len = c.Length;

            for (i = 0; i < Len; i++)
            {
                if (c[i] != '0')
                {
                    break;
                }
            }

            N = i - 1;

            for (; i < Len; i++)
            {
                N++;

                if (c[i] >= 'A' && c[i] <= 'Z')
                {
                    j += (Int32)(Math.Pow(26, (Len - N)) * (((int)c[i]) - asc + 1));
                }
                else
                {
                    j = 0;
                    break;
                }
            }
            return j;
        }

        String AlphaMathGetString(Int32 AlphaMathNumber)
        {
            Int32 i;
            StringBuilder charArray;
            char [] array;
            const Int32 asc = 65, setsize = 26;

            if (AlphaMathNumber <= 0)
            {
                return String.Empty;
            }

            charArray = new StringBuilder();
            while (AlphaMathNumber != 0)
            {
                i = AlphaMathNumber % setsize;

                if (i == 0)
                {
                    i = setsize;
                }

                AlphaMathNumber = (AlphaMathNumber - i) / setsize;
                    
                charArray.Append(asc + i - 1);
            }

            array = charArray.ToString().ToArray();

            array.Reverse();

            return array.ToString();
        }

        void CloseLogFile(IntPtr hFile)
        {
            if (hFile != IntPtr.Zero)
            {
                KernelImport.CloseHandle(hFile);
            }
        }

        Boolean IsLogFilePastSizeLimit(IntPtr hFile, Int32 LimitBytes)
        {
            uint CurrentLen = 0;
            uint MoveMethod = 0;

            if (hFile == IntPtr.Zero) return false;

            if (LimitBytes <= 0) return false;

            CurrentLen = KernelImport.SetFilePointer(hFile, 0, out MoveMethod, KernelImport.EMoveMethod.End);
            
            if (CurrentLen == 0xFFFFFFFF)
            {
                return false;
            }

            return (CurrentLen > LimitBytes);
        }

        IntPtr OpenLogFile(String Filename, Boolean SharedWrite)
        {
            if (SharedWrite)
            {
                return KernelImport.CreateFileA(Filename, FileAccess.Write, FileShare.ReadWrite, IntPtr.Zero, FileMode.OpenOrCreate, FileAttributes.Normal, IntPtr.Zero);
            }
            else
            {
                return KernelImport.CreateFileA(Filename, FileAccess.Write, FileShare.Read, IntPtr.Zero, FileMode.OpenOrCreate, FileAttributes.Normal, IntPtr.Zero);
            }
        }

        public Boolean InitializeLogFile(String AgentName, String Ext, String CycleLogPath, Int32 NoOfDays, Int32 FileSizeLimitInKB = 1024)
        {
            String DirSuff = String.Empty, FPrefix = String.Empty, filename = String.Empty;

            if (!CycleLogPath.EndsWith("\\"))
            {
                DirSuff = "\\";
            }

            LogFileExtention = Ext;

            // Clear the 3 days old file
            FPrefix = Ext + AgentName;
            ClearOldCycleLog(FPrefix, CycleLogPath, (Int16)NoOfDays);

            filename = FPrefix + DateTime.Now.ToString(LOGFILE_CYCLETIMEFMT) + "." + Ext;
            if (filename.Contains("\\") == false)
            {
                filename = CycleLogPath + DirSuff + filename;
            }

            if (!InitialiseLogging(filename, CycleLogPath, FPrefix, "shutdown", "", FileSizeLimitInKB, false, false, true))
            {
                throw new Exception("Error in initializing the " + filename);
            }
            return true;
        }

        void LogMessage(String Filename, params object[] Entries)
        {
            IntPtr hFile;
            Boolean UsePrefix = false;

            if (String.IsNullOrEmpty(Filename)) return;

            hFile = OpenLogFile(Filename, true);
            if (hFile == IntPtr.Zero) return;

            UsePrefix = (!String.IsNullOrEmpty(LogMessagePrefix) && LogMessagePrefix.Length > 0);

            if (Entries == null || Entries.Length == 0)
            {
                AppendToOpenLogFile(hFile, UsePrefix, null);
            }
            else
            {
                AppendToOpenLogFile(hFile, UsePrefix, Entries);
            }

            CloseLogFile(hFile);
        }

        public void WriteLog(params object[] Entries)
        {
            DateTime CurrentTime = DateTime.Now;

            if ( !mLoggingEnabled ) return;

            if (mLogFileHandle == IntPtr.Zero) return;

            if (DateTime.Compare(NextLogCycleTime,  CurrentTime) < 0 || IsLogFilePastSizeLimit(mLogFileHandle, LogFileSizeLimit))
            {
                CloseLogFile(mLogFileHandle);

                if (!CycleFile(LogFileName, LogCyclePath, LogCyclePrefix, LOGFILE_CYCLETIMEFMT))
                {
                    // what to do 
                }
                
                if (DateTime.Compare(NextLogCycleTime, CurrentTime) < 0)
                {
                    NextLogCycleTime = NextScheduledTime(CurrentTime, LogCycleKind, LogCycleArgs);
                }
                
                mLogFileHandle = OpenLogFile(LogFileName, true);
            }

            AppendToOpenLogFile(mLogFileHandle, false, Entries);
        }

        public void WriteLog(LogLevel debugLevel, params object[] Entries)
        {
            if (debugLevel <= mDebugLevel)
            {
                WriteLog(Entries);
            }
        }
    }
}
