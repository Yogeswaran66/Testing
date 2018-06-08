using System;
using System.IO;
using System.Collections.Generic;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace exgeneric
{
    public static class KernelImport
    {
        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        public enum GET_FILEEX_INFO_LEVELS
        {
            GetFileExInfoStandard,
            GetFileExMaxInfoLevel
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [DllImport("kernel32.dll")]
        public static extern Int32 GetLastError();

        [DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateFile(
              [MarshalAs(UnmanagedType.LPTStr)] string filename,
              [MarshalAs(UnmanagedType.U4)] FileAccess access,
              [MarshalAs(UnmanagedType.U4)] FileShare share,
              IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
              [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
              [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
              IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateFileA(
                [MarshalAs(UnmanagedType.LPStr)] string filename,
                [MarshalAs(UnmanagedType.U4)] FileAccess access,
                [MarshalAs(UnmanagedType.U4)] FileShare share,
                IntPtr securityAttributes,
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
                IntPtr templateFile);

         [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
         public static extern IntPtr CreateFileW(
              [MarshalAs(UnmanagedType.LPWStr)] string filename,
              [MarshalAs(UnmanagedType.U4)] FileAccess access,
              [MarshalAs(UnmanagedType.U4)] FileShare share,
              IntPtr securityAttributes,
              [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
              [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
              IntPtr templateFile);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SetFilePointer(
             [In] IntPtr hFile, 
             [In] uint lDistanceToMove,
             [Out] out uint lpDistanceToMoveHigh,
             [In] EMoveMethod dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            IntPtr hFile, 
            [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead, 
            out uint lpNumberOfBytesRead, 
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(
            IntPtr hFile, 
            byte [] lpBuffer,
            uint nNumberOfBytesToWrite, 
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlushFileBuffers(IntPtr hFile);

        [DllImport("kernel32.dll")]
        public static extern bool LockFile(
            IntPtr hFile, 
            uint dwFileOffsetLow, 
            uint dwFileOffsetHigh, 
            uint nNumberOfBytesToLockLow, 
            uint nNumberOfBytesToLockHigh);

        [DllImport("kernel32.dll")]
        public static extern bool UnlockFile(
            IntPtr hFile, 
            uint dwFileOffsetLow,
            uint dwFileOffsetHigh, 
            uint nNumberOfBytesToUnlockLow,
            uint nNumberOfBytesToUnlockHigh);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesEx(string lpFileName, GET_FILEEX_INFO_LEVELS fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA fileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateEvent(
              Int32 lpEventAttributes,
              Int32 bManualReset,
              Int32 bInitialState,
              string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateEventA(
              Int32 lpEventAttributes,
              Int32 bManualReset,
              Int32 bInitialState,
              string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr SetEvent(Int32 hEvent);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr WaitForSingleObject(
              Int32 hHandle,
              Int32 dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CloseHandle(Int32 hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr OpenEvent(
              Int32 dwDesiredAccess,
              Boolean bInheritHandle,
              String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr OpenEventA(
              Int32 dwDesiredAccess,
              Int32 bInheritHandle,
              String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           Int32 nSize,
           string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 WritePrivateProfileString(
            string section,
            string key,
            string val, 
            string filePath);
    }
}
