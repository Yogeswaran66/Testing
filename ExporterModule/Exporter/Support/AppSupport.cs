using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using exhelper;
using System.IO;
using System.Windows.Forms;
using JSONXLib;
using API;

namespace exhelper
{
    public static class AppSupport
    {
        public static String JsonReqResponse { get; set; }
        public static String SampleData { get; set; }
        public static String URL { get; set; }
        public static String ExternalURL { get; set; }
        public static String ExternalUrlfieldname { get; set; }
        public static String Modulo { get; set; }
        public static Int32 mModDivision { get; set; }
        public static Int32 mModVal { get; set; }
        public static Int32 SleepTime { get; set; }
        public static Boolean ConvertPdf { get; set; }
        public static String BaseImagePath { get; set; }
        public static String TempPath { get; set; }
        public static String PDFConverterExePath { get; set; }
        public static Logging objLog { get; set; }
        public static String CrashRecoveryPath { get; set; }
        public static String LogExpiryDuration { get; set; }

        public const String AGENTNAME = "EXPORTER";

        public static Boolean Initialize(String[] args)
        {
            Boolean Ret;
            String UserName, PWD, LogPath;
            String[] ModuloArr;
            Int32 NoOfDays, Pdfval;
            String Temp;

            URL = AppConfiguration.GetValueFromAppConfig("URL", "");
            if (String.IsNullOrEmpty(URL))
            {
                MessageBox.Show("URL is empty in app.config");
                return false;
            }
            if (!URL.EndsWith("/"))
                URL += "/";

            ExternalURL = AppConfiguration.GetValueFromAppConfig("ExternalUrl", "");
            if (String.IsNullOrEmpty(ExternalURL))
            {
                MessageBox.Show("ExternalUrl is empty in app.config");
                return false;
            }
            if (ExternalURL.EndsWith("/"))
                ExternalURL = ExternalURL.TrimEnd('/');

            ExternalUrlfieldname = AppConfiguration.GetValueFromAppConfig("ExternalUrlfieldname", "");
            if (String.IsNullOrEmpty(ExternalUrlfieldname))
            {
                MessageBox.Show("ExternalUrlfieldname is empty in app.config");
                return false;
            }            
            
            Ret = ReadReqResPath();
            if (!Ret)
                return false;

            if (!JsonReqResponse.EndsWith("\\")) JsonReqResponse += "\\";
            if (!Directory.Exists(JsonReqResponse)) Directory.CreateDirectory(JsonReqResponse);

            Ret = ReadInputPath();
            if (!Ret)
                return false;

            if (!Directory.Exists(SampleData))
            {
                MessageBox.Show("Paths_SampleData in app.config is Invalid " + SampleData);
                return false;
            }

            if (!SampleData.EndsWith("\\")) SampleData += "\\";
            String[] FileNames = { "Login.json", "Logout.json", "Read.json", "Getdata.json", "GetWorkitem_WorkID.json" };

            Ret = CheckFileExists(SampleData, FileNames);
            if (!Ret)
                return false;

            UserName = AppConfiguration.GetValueFromAppConfig("LoginId", "");
            if (String.IsNullOrEmpty(UserName))
            {
                MessageBox.Show("UserName is null in AppConfig");
                return false;
            }

            PWD = AppConfiguration.GetValueFromAppConfig("Password", "");
            if (String.IsNullOrEmpty(PWD))
            {
                MessageBox.Show("Password is null in AppConfig");
                return false;
            }

            ExternalURL = AppConfiguration.GetValueFromAppConfig("ExternalURL", "");
            if (String.IsNullOrEmpty(ExternalURL))
            {
                MessageBox.Show("ExternalUrl is null in AppConfig");
                return false;
            }

            Modulo = AppConfiguration.GetValueFromAppConfig("Modulo", "");
            if (!String.IsNullOrEmpty(Modulo))
            {
                ModuloArr = Modulo.Split('/');
                mModVal = Convert.ToInt32(ModuloArr[0]);
                mModDivision = Convert.ToInt32(ModuloArr[1]);
            }
            else
            {
                mModDivision = 0;
            }

            SleepTime = 5;

            NoOfDays = Convert.ToInt32(AppConfiguration.GetValueFromAppConfig("LogExpiryDuration", "3"));
            objLog = new Logging();
            LogPath = PathHelper.FixPath("Log", Environment.CurrentDirectory);
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            objLog.DebugLevel = LogLevel.FULL_DEBUG;
            objLog.InitializeLogFile(AGENTNAME, "LOG", LogPath, NoOfDays);

            Pdfval = 0;
            Temp = AppConfiguration.GetValueFromAppConfig("SearchablePDF", "0"); ;
            if (Temp.Length != 0)
            {
                Int32.TryParse(Temp, out Pdfval);
                if (Pdfval == 1)
                    ConvertPdf = true;
            }            

            TempPath = PathHelper.FixPath("Temp", Environment.CurrentDirectory);
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            if (!TempPath.EndsWith("\\")) TempPath += "\\";

            CrashRecoveryPath = PathHelper.FixPath("CrashRecovery", Environment.CurrentDirectory);
            if (!Directory.Exists(CrashRecoveryPath))
            {
                Directory.CreateDirectory(CrashRecoveryPath);
            }
            if (!CrashRecoveryPath.EndsWith("\\")) CrashRecoveryPath += "\\";

            if (!Helper.SafeFileExists(CrashRecoveryPath + "Recovery.ini"))
            {
                File.Create(CrashRecoveryPath + "Recovery.ini");
            }
            CrashRecoveryPath += "Recovery.ini";

            if (ConvertPdf)
            {
                PDFConverterExePath = AppConfiguration.GetValueFromAppConfig("PDFConverterExePath", "");
                if (String.IsNullOrEmpty(PDFConverterExePath))
                {
                    MessageBox.Show("PDFConverterExePath is null in AppConfig");
                    return false;
                }
                if (!PDFConverterExePath.EndsWith("\\")) PDFConverterExePath += "\\";

                PDFConverterExePath = PDFConverterExePath + "naps2.console.exe";
                if (!Helper.SafeFileExists(PDFConverterExePath))
                {
                    MessageBox.Show("PDFConverterExe is missing in " + PDFConverterExePath);
                    return false;
                }

                BaseImagePath = AppConfiguration.GetValueFromAppConfig("BaseImagePath", "");
                if (String.IsNullOrEmpty(BaseImagePath))
                {
                    MessageBox.Show("BaseImagePath is null in AppConfig");
                    return false;
                }
                if (!BaseImagePath.EndsWith("\\")) BaseImagePath += "\\";
                if (!Directory.Exists(BaseImagePath))
                {
                    MessageBox.Show("BaseImagePath Folder Not Exist " + BaseImagePath);
                    return false;
                }
            }

            objLog.WriteLog("MakesearchablePDF", Pdfval);
            if(PDFConverterExePath != null)
                objLog.WriteLog("PDFConverterExePath", PDFConverterExePath);

            objLog.WriteLog("ExternalUrl", ExternalURL);

            return true;
        }

        private static Boolean ReadReqResPath()
        {
            String key = "Paths_JsonReqResponse";

            JsonReqResponse = AppConfiguration.GetValueFromAppConfig(key, "");
            if (String.IsNullOrEmpty(JsonReqResponse))
            {
                MessageBox.Show(key + "value is empty in app.config");
                return false;
            }

            return true;
        }

        private static Boolean ReadInputPath()
        {
            String key = "Paths_SampleData";

            SampleData = AppConfiguration.GetValueFromAppConfig(key, "");
            if (String.IsNullOrEmpty(SampleData))
            {
                MessageBox.Show(key + "value is empty in app.config");
                return false;
            }

            return true;
        }

        public static Boolean CheckFileExists(String Path, String[] Files)
        {
            if (Files.Length == 0)
                return false;

            for (int i = 0; i < Files.Length; i++)
            {
                if (!Helper.SafeFileExists(Path + Files[i]))
                {
                    MessageBox.Show(Files[i] + " not exists :" + Path);
                    return false;
                }
            }
            return true;
        }
    }
}
