using System;
using System.Runtime.InteropServices;
using System.Text;
using exgeneric;

namespace exhelper
{
    public static class IniFile
    {
        public static void WriteValue(String IniFilePath, String Section, String Key, String Value)
        {
            KernelImport.WritePrivateProfileString(Section, Key, Value, IniFilePath);
        }

        public static String ReadValue(String IniFilePath, String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            KernelImport.GetPrivateProfileString(Section, Key, "", temp, 255, IniFilePath);
            return temp.ToString();
        }
    }
}
