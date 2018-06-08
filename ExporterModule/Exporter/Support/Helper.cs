using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace exhelper
{
    public static class Helper
    {

        public const String CONST_EMPTY = "";
                        
        /************************ Dll Import to find files and directory **************************************************/

        public const Int32 MAX_PATH = 260;
        public const Int32 INVALID_HANDLE_VALUE = -1;
        public static IntPtr retval = new IntPtr(INVALID_HANDLE_VALUE);

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public String cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public String cAlternateFileName;
        }

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindFirstFile(String lpFileName, out WIN32_FIND_DATA lpFindData);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindData);

        [DllImport("kernel32", SetLastError = true)]
        public static extern Boolean FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void CopyMemory(IntPtr pDst, IntPtr pSrc, Int32 ByteLen);

        /**************************************************** End ************************************************************/

        /************************* Functions that manipulate Files/Directories/Path  **************************************/
        
        /* Deletes all files and folders on a specified path */
        public static Boolean EmptyDirectory(String DirPath)
        {
            if (!IsValidDirectory(DirPath)) return false;

            DirectoryInfo dirInfo = new DirectoryInfo(DirPath);

            /* Collection of all the directories at the specified path */
            DirectoryInfo[] subdir = dirInfo.GetDirectories();
            foreach (DirectoryInfo dir in subdir)
                dir.Delete(true);

            /* Collection of all the files at the specified path */
            FileInfo[] fInfo = dirInfo.GetFiles();
            foreach (FileInfo file in fInfo)
                file.Delete();
            return true;
        }

        /* Close fileStream object */
        public static Boolean EndMatchingFile(FileStream obj)
        {
            if (obj == null) return false;

            obj.Close();
            return true;
        }

        public static Boolean FixPath(String path)
        {
            if (String.IsNullOrEmpty(path)) return false;

            if (!path.EndsWith("\\"))
                path = path + "\\";

            return true;
        }

        public static String FixPath(String path, String refDir)
        {
            if (path.Length == 0)
            {
                return "";
            }

            String str = path;
            switch (path)
            {
                case ".":
                case ".\\":
                    str = Directory.GetCurrentDirectory();
                    break;
                case "..":
                case "..\\":
                    str = GetCurrentDir();
                    str = GetParentDir(str);
                    break;
                default:

                    if (Regex.IsMatch(str, @"^[A-Za-z]{1}:\\$"))
                        break;
                    if (Regex.IsMatch(str, @"^[A-Za-z]{1}:\\"))
                    {
                        if (path.EndsWith("\\"))
                            str = str.Remove(str.Length - 1);
                        break;
                    }
                    if (refDir.Length > 0)
                        str = refDir + ":\\" + str;
                    break;
            }

            return str;
        }

        /* Checks if any folder or file exist on the specified path */
        public static Boolean CheckIfDirIsEmpty(String DirPath)
        {
            if (!IsValidDirectory(DirPath))
            {
                return false;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(DirPath);

            /* Collection of all the directories and files at the specified path */
            DirectoryInfo[] subdir = dirInfo.GetDirectories();

            if ((subdir.Length == 0) && (dirInfo.GetFiles().Length == 0))
            {
                return true;
            }

            return false;
        }

        /* Returns collection of all the directories at the specified path */
        public static DirectoryInfo[] FindDirectories(String DirPath)
        {
            if (!IsValidDirectory(DirPath))
            {
                return null;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(DirPath);

            return new DirectoryInfo(DirPath).GetDirectories();
        }

        /* Returns collection of all the files at the specified path */
        public static FileInfo[] FindFiles(String DirPath)
        {
            if (!IsValidDirectory(DirPath))
            {
                return null;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(DirPath);

            return new DirectoryInfo(DirPath).GetFiles();
        }

        public static Boolean CreateAPath(String DirPath)
        {
            DirPath = DirPath.Trim();
            if (DirPath.Length < 3)
            {
                return false;   // Not sure on this
            }

            if (!IsValidDriveLetter(DirPath.Substring(0, 3)))
            {
                return false;  // done to get first 3 drive letters 
            }

            if (Directory.Exists(DirPath))
            {
                return true;// path already exists
            }

            DirectoryInfo Dinfo = Directory.CreateDirectory(DirPath);

            return Dinfo.Exists;
        }

        public static Boolean IsValidDriveLetter(String DriveLetter)// should be like "D:\\"
        {
            Boolean returnValue = false;
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.Name.Equals(DriveLetter, StringComparison.CurrentCultureIgnoreCase))
                {
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        public static String GetParentDir(String DirPath)
        {
            if (!IsValidDirectory(DirPath))
            {
                return null;
            }

            return Directory.GetParent(DirPath).ToString();
        }

        public static String GetCurrentDir()
        {
            return Directory.GetCurrentDirectory();
        }

        public static Boolean IsValidDirectory(String DirPath)
        {
            DirPath = DirPath.Trim();

            /* If the path does not exist, cannot check it */
            if ((DirPath.Length == 0) || !Directory.Exists(DirPath))
            {
                return false;
            }

            return true;
        }

        /* Checks if a file exists on a specified path */
        public static Boolean SafeFileExists(String FileName)
        {
            FileName = FileName.Trim();

            if ((FileName.Length == 0) || !File.Exists(FileName))
            {
                return false;
            }

            return true;
        }

        /* returns file size */
        public static Int32 SafeFileLen(String FileName)
        {
            if (!SafeFileExists(FileName))
            {
                return -1;
            }

            FileInfo finfo = new FileInfo(FileName);

            return (Int32)finfo.Length;   /* Length of a blank file is 0 bytes */
        }

        /* Deletes a file */
        public static Boolean SafeKill(String FileName)
        {
            if (!SafeFileExists(FileName))
            {
                return false;
            }

            FileInfo finfo = new FileInfo(FileName);

            finfo.Delete();

            return true;
        }

        public static String GetFileExtn(String FileNameWithPath)
        {
            if (FileNameWithPath.Length == 0)
            {
                return null;
            }

            // not sure to check if file exists
            if (!SafeFileExists(FileNameWithPath))
            {
                return null;
            }

            FileInfo finfo = new FileInfo(FileNameWithPath);

            return finfo.Extension;
        }
        
        public static String GetFilePath(String FileNameWithPath)
        {
            if (FileNameWithPath.Length == 0) return null;

            if (!SafeFileExists(FileNameWithPath)) return null;
            FileInfo finfo = new FileInfo(FileNameWithPath);

            return finfo.DirectoryName;
        }

        public static String GetNameWithoutPath(String FileNameWithPath)
        {
            if (FileNameWithPath.Length == 0) return null;

            if (!SafeFileExists(FileNameWithPath)) return null;
            FileInfo finfo = new FileInfo(FileNameWithPath);

            return finfo.Name;
        }

        public static String CombinePath(String Path, String FileName)
        {
            if (!FixPath(Path)) return "";
            if (!Path.EndsWith("\\")) Path += "\\";

            String FileNameWithPath = Path + FileName;

            if (!SafeFileExists(FileNameWithPath)) return "";

            return FileNameWithPath;
        }

        /************************* Functions that manipulate Strings **************************************/
        public static String[] GetStringTokenArray(String DelimetedTokens, Char Delim)
        {
            if (DelimetedTokens.Length == 0) return null;
            return DelimetedTokens.Split(Delim);
        }

        public static void GetStringToken(String S, ref String Token, ref Int32 Start, Char Delim)
        {
            Int32 CharSpot;
            try
            {
                if (String.IsNullOrEmpty(S))
                {
                    Token = String.Empty;
                }

                if (!S.EndsWith(Convert.ToString(Delim)))
                {
                    S += Delim;
                }

                CharSpot = S.IndexOf(Delim, Start);
                if (CharSpot == 0)
                {
                    Token = String.Empty;
                }

                Token = S.Substring(Start, CharSpot - Start);
                Start = CharSpot + 1;
            }
            catch (Exception)
            {
                //todo    
            }
        }

        public static Int32 LongHexVal(String HexVal)
        {
            if (HexVal.Length == 0) return -1;
            int num = Int32.Parse(HexVal, System.Globalization.NumberStyles.HexNumber);

            return num;
        }

        public static String ValtoHex(Int32 val)
        {
            return val.ToString("X");
        }

        public static Int32 HashString(String StringToHash)
        {
            Int32 Hash = 0;

            for (Int32 i = 0; i <= StringToHash.Length - 1; i++)
            {
                Hash += System.Convert.ToChar(StringToHash.Substring(i, 1));
            }

            return Hash & 255;
        }

        /************************* Functions that manipulate Date **************************************/
        public static Int32 LastDayOfMonthEx(Int32 yyyy, Int32 mm)
        {
            if (mm < 1 || mm > 12)
            {
                return 0;
            }

            if (yyyy < 1000 || yyyy > 9999)
            {
                return 0;//need to check
            }

            return DateTime.DaysInMonth(yyyy, mm);
        }

        public static Int32 DateValidate(String date)
        {
            Int32 result = 0;
            const Int32 BADDATE_FORMAT = -2;
            const Int32 BADDATE_FUTURE = -1;
            const Int32 OKDATE = 0;
            DateTime D;

            try
            {
                D = Convert.ToDateTime(date);
            }
            catch (Exception)
            {
                return BADDATE_FORMAT;
            }

            if (D > DateTime.Now)
            {
                result = BADDATE_FUTURE;
            }
            else
            {
                result = OKDATE;
            }

            return result;
        }

        public static Boolean DeleteFromList<T>(List<T> list, Int32 pos)
        {
            if (list == null)
            {
                return false;
            }

            if (pos < 0 || pos > list.Capacity)
            {
                return false;
            }

            list.RemoveAt(pos);

            return true;
        }

        public static Boolean AddToList<T>(List<T> list, T val)
        {
            if (list == null)
            {
                return false;
            }

            list.Add(val);

            return true;
        }

        public static String GetTimeIn(String StrTime, String format)
        {
            DateTime newDate;
            DateTime.TryParseExact(StrTime, format, null, DateTimeStyles.None, out newDate);

            return newDate.ToString();
        }
        
        /************************* Functions that read file ************************************************/
        public static String ReadFileAsSingleBuffer(String FileName)
        {
            String fileContents = null;
            try
            {
                fileContents = File.ReadAllText(FileName, System.Text.Encoding.UTF8);
            }
            catch (Exception)
            {
                //todo
            }

            return fileContents;
        }

        public static void GetTokenCount(String S, String Delim, out Int32 Count)
        {
            Boolean endswithdelim;

            if (String.IsNullOrEmpty(S))
            {
                Count = 0;
                return;
            }

            endswithdelim = S.EndsWith(Delim);
            if (!endswithdelim)
            {
                S += Delim;
            }

            Count = 0;
            foreach (Char c in S)
            {
                if (Convert.ToString(c) == Delim)
                {
                    Count++;
                }
            }
        }

        public static void GetTokenNumber(String S, String Token, Char Delim, out Int32 Number)
        {
            Number = 0;
            String[] strArr = null;
            Int32 i = 0;

            if (Token.Length == 0)
            {
                Number = 0;
            }

            if (Token.Length >= 1)
            {
                if (Token.EndsWith(Convert.ToString(Delim)))
                {
                    Token = Token.Substring(0, Token.Length - 1);
                }
                if (Token.Length == 0)
                {
                    Number = 0;
                }
            }

            if (Delim == ';')
            {
                if (Token.Contains("|"))
                {
                    Token = Token.Substring(0, Token.IndexOf('|'));
                }
            }

            strArr = S.Split(Convert.ToChar(Delim));
            for (i = 0; i < strArr.Length; i++)
            {
                if (Delim == ';')
                {
                    if (strArr[i].Contains('|'))
                    {
                        strArr[i] = strArr[i].Substring(0, Token.IndexOf('|'));
                    }
                }
                if (strArr[i] == Token)
                {
                    Number = i + 1;
                }
            }
        }

        public static Int32 IsADate(String date, ref String Corrected, String Kind)
        {
            try
            {
                //read date(kind) format from ini file 
                //To do
                DateTime dt = DateTime.Parse(date);
                Corrected = date;
                //return 0;
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }

        public static void RemoveMultiSpaces(String value, out String Target)
        {
            var newString = new StringBuilder();
            Boolean previousIsWhitespace = false;

            for (Int32 i = 0; i < value.Length; i++)
            {
                if (Char.IsWhiteSpace(value[i]))
                {
                    if (previousIsWhitespace)
                    {
                        continue;
                    }
                    previousIsWhitespace = true;
                }
                else
                {
                    previousIsWhitespace = false;
                }
                newString.Append(value[i]);
            }
            Target = newString.ToString();
        }

        public static void ReplaceCharacterInString(String S, Char FindChar, Char ReplaceChar)
        {
            if (String.IsNullOrEmpty(S))
            {
                S = String.Empty;
            }
            else
            {
                S = S.Replace(FindChar, ReplaceChar);
            }
        }

        public static void SplitStringToken(String S, Char Delim, ref String FirstToken, ref String SecondToken)
        {
            Int32 len = S.Length, i;

            switch (len)
            {
                case 0:
                    FirstToken = CONST_EMPTY;
                    SecondToken = CONST_EMPTY;
                    break;
                case 1:
                    if (S == Convert.ToString(Delim))
                    {
                        FirstToken = S;
                        SecondToken = CONST_EMPTY;
                    }
                    break;
                default:
                    for (i = 0; i < len; i++)
                    {
                        if (S[i] == Delim)
                        {
                            FirstToken = S.Substring(0, i);
                            S = S.Remove(0, i + 1);
                            SecondToken = S;
                            break;
                        }
                    }
                    break;
            }
        }

        /**********************************************Functions to Find Files And Directory *************************************/
        public static String FirstMatchingFile(String Pattern, ref IntPtr Handle)
        {
            String fname = "";
            WIN32_FIND_DATA FFileData;

            try
            {
                Handle = FindFirstFile(Pattern, out FFileData);
                do
                {
                    if (Handle == retval)
                    {
                        break;
                    }

                    if ((FFileData.dwFileAttributes & FileAttributes.Directory) == 0 && FFileData.cFileName != "." && FFileData.cFileName != "..")
                    {
                        fname = FFileData.cFileName;
                        return fname;
                    }

                    if (!FindNextFile(Handle, out FFileData))
                    {
                        break;
                    }

                } while (true);

                FindClose(Handle);
            }
            catch (Exception)
            {
                //todo
            }
            return fname;
        }

        public static String NextMatchingFile(ref IntPtr Handle)
        {
            String fname = "";
            WIN32_FIND_DATA FFileData;

            try
            {
                do
                {
                    if (!FindNextFile(Handle, out FFileData))
                    {
                        return String.Empty;
                    }

                    if ((FFileData.dwFileAttributes & FileAttributes.Directory) == 0 && FFileData.cFileName != "." && FFileData.cFileName != "..")
                    {
                        fname = FFileData.cFileName;
                        return fname;
                    }

                    if ((Handle == retval) || (FFileData.dwFileAttributes & FileAttributes.Directory) == 0)
                    {
                        break;
                    }

                } while (true);

                FindClose(Handle);
            }
            catch (Exception)
            {
                //TODO
            }
            return fname;
        }

        public static String FileMatchingPattern(String PatternWithPath)
        {
            String fname = "";
            WIN32_FIND_DATA FFileData;
            IntPtr Handle;

            try
            {
                Handle = FindFirstFile(PatternWithPath, out FFileData);
                do
                {
                    if (Handle == retval)
                    {
                        break;
                    }

                    if ((FFileData.dwFileAttributes & FileAttributes.Directory) == 0 && FFileData.cFileName != "." && FFileData.cFileName != "..")
                    {
                        fname = FFileData.cFileName;
                        return fname;
                    }

                    if (!FindNextFile(Handle, out FFileData))
                    {
                        break;
                    }

                } while (true);

                FindClose(Handle);
            }
            catch (Exception)
            {
                //todo
            }
            return fname;
        }

        public static String FirstFolder(String Pattern, ref IntPtr Handle)
        {
            String fname = "";
            WIN32_FIND_DATA FFileData;
            try
            {
                Handle = FindFirstFile(Pattern, out FFileData);
                do
                {
                    if (Handle == retval)
                    {
                        break;
                    }

                    if ((FFileData.dwFileAttributes & FileAttributes.Directory) != 0 && FFileData.cFileName != "." && FFileData.cFileName != "..")
                    {
                        fname = FFileData.cFileName;
                        return fname;
                    }

                    if (!FindNextFile(Handle, out FFileData))
                    {
                        break;
                    }

                } while (true);

            }
            catch (Exception)
            {
                //todo
            }
            return fname;
        }

        public static String NextFolder(ref IntPtr Handle)
        {
            String fname = "";
            WIN32_FIND_DATA FFileData;
            try
            {
                do
                {
                    if (Handle == retval)
                    {
                        break;
                    }

                    if (!FindNextFile(Handle, out FFileData) || (FFileData.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        return String.Empty;
                    }

                    if ((FFileData.dwFileAttributes & FileAttributes.Directory) != 0 && FFileData.cFileName != "." && FFileData.cFileName != "..")
                    {
                        fname = FFileData.cFileName;
                        return fname;
                    }
                } while (true);

                FindClose(Handle);
            }
            catch (Exception)
            {
                //todo
            }

            return fname;
        }

        public static String Right(this String sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (String.IsNullOrEmpty(sValue))
            {
                //Set valid empty String as String could be null
                sValue = String.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the String no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            //Return the String
            return sValue;
        }
        public static String Left(this String sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (String.IsNullOrEmpty(sValue))
            {
                //Set valid empty String as String could be null
                sValue = String.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the String no longer than the max length
                sValue = sValue.Substring(0, iMaxLength);
            }
            //Return the String
            return sValue;
        }

        public static Byte[] StringToByte(String str)
        {
            UTF8Encoding encoding = null;
            Byte[] ByteArr;

            try
            {
                encoding = new System.Text.UTF8Encoding();
                ByteArr = encoding.GetBytes(str);
                return ByteArr;
            }

            catch (Exception)
            {
                return null;
            }
        }

        public static String ByteToString(Byte[] Arr)
        {
            String result = null;

            try
            {
                result = System.Text.Encoding.UTF8.GetString(Arr);
            }

            catch (Exception)
            {
                return null;
            }

            return result;
        }

        public static void ByteArrToLongArray(Byte[] SrcArray, Int32 srcOffset, ref Int32[] DestArray, Int32 destOffset)
        {
            try
            {
                Buffer.BlockCopy(SrcArray, srcOffset, DestArray, destOffset,8);
            }
            catch (Exception)
            {
            }
        }

        public static void LongArrayToByteArr(Int32[] SrcArray, Int32 srcOffset, ref Byte[] DestArray, Int32 destOffset)
        {
            try
            {
                Buffer.BlockCopy(SrcArray, srcOffset, DestArray, destOffset, 8);
            }
            catch (Exception)
            {
            }
        }

        public static String sZTrim(String str)
        {
            Int32 NullSpot = 0;
            String Temp = new String('\0', 0);
            NullSpot = Temp.IndexOf(str);
            if (NullSpot == 0)
            {
                return str;
            }
            else
            {
                return Left(str, NullSpot - 1);
            }
        }

        public static String GetLastErr()
        {
            return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }
        

    }/* End Of class */
}/* End Of name space */
