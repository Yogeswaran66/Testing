using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSONXLib;
using API;
using System.IO;
using exhelper;
using Support;
using System.Threading;
using GenericPOSTRequest;
using System.Diagnostics;
using System.Windows.Forms;

namespace Support
{
    public class DoExport
    {
        public Int32 CurrentExecCount { get; set; }
        public Int32 WIAppdata1 { get; set; }
        IFLoadTestReporter objInterface;

        public struct UpdateWInfoRecord
        {
            public Int32 Id;
            public String Appdata1;
            public String DataRelPath;
        }
        public List<UpdateWInfoRecord> ReadInfo { get; set; }

        Login objLogin;
        Read objread;
        GetWorkItem objGetWI;
        GetData objGetData;
        Upload objUpload;
        Download objDownload;
        WorkQUpdation objwqu;
        Documentum objDocumentum;
        Logout objLogout;

        public DoExport(IFLoadTestReporter objUserIF)
        {
            objInterface = objUserIF;
        }

        public void DoAgentOps()
        {
            Boolean Ret;

            Ret = DoLogin();

            if (!Ret)
                return;

            DoProcessing();

            Ret = DoLogout();
        }

        public Boolean DoLogin()
        {
            Boolean Ret;
            JSON objJSON = new JSON();
            String Responsefile;
            JSONValue objStatus, objRes, objData, SessKey;
            Int64 StartTime;
            Double ElpasedTime;

            objLogin.ResponsePath = AppSupport.JsonReqResponse;
            Responsefile = AppSupport.JsonReqResponse + WFConstants.API_RESP_LOGIN;

            StartTime = DateTime.Now.Ticks;
            Ret = objLogin.SubmitRequest();

            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objLogin.ResponseData.ErrorMsg, "ErrorStatus", objLogin.ResponseData.Status);
                return false;
            }

            if (!Helper.SafeFileExists(Responsefile))
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, "Response File is not available : ");
                return false;
            }

            objRes = objJSON.ParseFile(Responsefile);
            if (objRes == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            objStatus = objRes.get_Member("STATUS", true);
            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value == "INVALID" || objStatus.Value == "FAILURE" || objStatus.Value == "FAILED")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, WFConstants.ERROR_STATUS_VALUE);
                return false;
            }

            objData = objRes.get_Member("DATA", true);
            if (objData == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, "DATA is not available in Response path");
                return false;
            }

            SessKey = objData.get_Member("SESSIONKEY", true);
            if (SessKey == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGIN, "SESSIONKEY is not available in Response path");
                return false;
            }

            objLogin.SessionKey = SessKey.Value;
            SetSessionKey(objLogin.SessionKey);
            return true;
        }

        public Boolean ValidateModulo(Int32 ID)
        {
            Int32 Val;

            if (AppSupport.mModDivision == 0)
                return true;

            Val = ID % AppSupport.mModDivision;
            if (Val == AppSupport.mModVal)
                return true;

            return false;
        }

        private void SetSessionKey(String SKey)
        {
            objread.SessionKey = SKey;
            objGetWI.SessionKey = SKey;
            objGetData.SessionKey = SKey;
            objUpload.SessionKey = SKey;
            objDownload.SessionKey = SKey;
            objwqu.SessionKey = SKey;
            objDocumentum.SessionKey = SKey;
            objLogout.SessionKey = SKey;
        }

        public Boolean InitialiseAllApiObj()
        {
            Boolean Ret;

            objLogin = new Login(AppSupport.URL + WFConstants.API_LOGIN);
            objread = new Read(AppSupport.URL + WFConstants.API_READ);
            objGetWI = new GetWorkItem(AppSupport.URL + WFConstants.API_GETWORKITEM);
            objGetData = new GetData(AppSupport.URL + WFConstants.API_GETDATA);
            objUpload = new Upload(AppSupport.URL + WFConstants.API_UPLOADFILE);
            objDownload = new Download(AppSupport.URL + WFConstants.API_DOWNLOADFILE);
            objwqu = new WorkQUpdation(AppSupport.URL + WFConstants.API_WORKQUPDATE);
            objDocumentum = new Documentum(AppSupport.URL);
            objLogout = new Logout(AppSupport.URL + WFConstants.API_LOGOUT);

            objLogin.LoginId = AppConfiguration.GetValueFromAppConfig("LoginId", "");
            objLogin.Password = AppConfiguration.GetValueFromAppConfig("Password", "");

            Ret = objLogin.Initialise();
            if (!Ret)
                return Ret;

            Ret = objread.Initialise();
            if (!Ret)
                return Ret;

            Ret = objGetData.Initialise();
            if (!Ret)
                return Ret;

            Ret = objUpload.Initialise();
            if (!Ret)
                return Ret;

            Ret = objDownload.Initialise();
            if (!Ret)
                return false;

            Ret = objwqu.Initialise();
            if (!Ret)
                return Ret;

            Ret = objGetWI.Initialise();
            if (!Ret)
                return Ret;

            Ret = objDocumentum.Initialise();
            if (!Ret)
                return Ret;

            Ret = objLogout.Initialise();
            if (!Ret)
                return Ret;

            return Ret;
        }

        private void GetSomeSleep(Int32 SleepTimeSeconds)
        {
            Int32 SleepTimeTotal = SleepTimeSeconds * 1000;
            Int32 SleepTime = 500, TotalSlept = 0;

            if (SleepTimeSeconds <= 0) return;

            while (true)
            {
                Application.DoEvents();
                if (!objInterface.CanContinueProcessing())
                    break;

                Thread.Sleep(SleepTime);
                TotalSlept += SleepTime;
                if (TotalSlept > SleepTimeTotal)
                {
                    break;
                }
            }
        }

        public Boolean DoProcessing()
        {
            Boolean Ret;
            Int32 i;
            Int32 WorkID;
            String Appdata1, Lastworkid;

            AppSupport.objLog.WriteLog("DoProcessing", "Starts");

            Ret = true;
            while (Ret)
            {
                
            ReadAgain:
                if (!objInterface.CanContinueProcessing())                    
                    return true;

                Ret = DoRead();
                if (!Ret)
                {
                    AppSupport.objLog.WriteLog("Read", "FAILED");
                    return false;
                }

                if (ReadInfo.Count == 0)
                {
                    GetSomeSleep(AppSupport.SleepTime);
                    goto ReadAgain;
                }

                for (i = 0; i < ReadInfo.Count; i++)
                {
                    if (!objInterface.CanContinueProcessing())
                        return true;

                    objInterface.PreparetoProcessItem();

                    WorkID = ReadInfo[i].Id;
                    Appdata1 = ReadInfo[i].Appdata1;

                    objInterface.SetCurrentItem(Appdata1);

                    AppSupport.objLog.WriteLog("GetWorkitem", "AppData1:" + Appdata1, "STARTS");
                    Ret = DoGetWorkitemById(WorkID);
                    if (Ret)
                        AppSupport.objLog.WriteLog("GetWorkitem", "AppData1:" + Appdata1, "SUCCESS");
                    else
                        AppSupport.objLog.WriteLog("GetWorkitem", "AppData1:" + Appdata1, "FAILED");

                    AppSupport.objLog.WriteLog("GetData", "AppData1:" + Appdata1, "STARTS");
                    Ret = DoGetdata(WorkID);
                    if (Ret)
                        AppSupport.objLog.WriteLog("GetData", "AppData1:" + Appdata1, "SUCCESS");
                    else
                    {
                        AppSupport.objLog.WriteLog("GetData", "AppData1:" + Appdata1, "FAILED");
                        return false;
                    }

                    Lastworkid = IniFile.ReadValue(AppSupport.CrashRecoveryPath,"LastWorkID", "Export");
                    if (Lastworkid == WorkID.ToString())
                    {
                        AppSupport.objLog.WriteLog("WorkID:" + WorkID, "DoDocumentum Done");
                        goto UpdateCase;
                    }
                    
                    if (AppSupport.ConvertPdf)
                    {
                        AppSupport.objLog.WriteLog("ConvertPdf", "AppData1:" + Appdata1, "STARTS");
                        DoMakeSearchablePdf();
                        AppSupport.objLog.WriteLog("ConvertPdf", "AppData1:" + Appdata1, "ENDS");
                    }

                    AppSupport.objLog.WriteLog("DocumentumUpload", "AppData1:" + Appdata1, "STARTS");
                    Ret = DoDocumentum();
                    if (Ret)
                        AppSupport.objLog.WriteLog("DocumentumUpload", "AppData1:" + Appdata1, "SUCCESS");
                    else
                    {
                        AppSupport.objLog.WriteLog("DocumentumUpload", "AppData1:" + Appdata1, "FAILED");
                        return false;
                    }

                    IniFile.WriteValue(AppSupport.CrashRecoveryPath,"LastWorkID", "Export", WorkID.ToString());

                UpdateCase:
                    AppSupport.objLog.WriteLog("UpdateWorkitem", "AppData1:" + Appdata1, "STARTS");
                    Ret = DoWorkqUpdation(WorkID, Appdata1);
                    if (Ret)
                        AppSupport.objLog.WriteLog("UpdateWorkitem", "AppData1:" + Appdata1, "SUCCESS");
                    else
                    {
                        AppSupport.objLog.WriteLog("UpdateWorkitem", "AppData1:" + Appdata1, "FAILED");
                        return false;
                    }

                    objInterface.CompletedItem();
                    objInterface.SetCount();
                    objInterface.SetCurrentItem("");
                }
            }

            AppSupport.objLog.WriteLog("DoProcessing", "Ends");
            return Ret;
        }

        public Boolean DoRead()
        {
            Boolean Ret;
            Int32 i;
            JSON objJSON = new JSON();
            JSONValue objRes, objSection, objRecord, objField;
            UpdateWInfoRecord objUpdateWInfoRecord;
            Int64 StartTime;
            Double ElpasedTime;
            
            ReadInfo = new List<UpdateWInfoRecord>();

            ReadInfo.Clear();
            objread.Maxcount = 1024;
            objread.tableid = WFConstants.API_READQ_EXPORTER;
            objread.Operation = 1;
            objread.pattern = "";

            StartTime = DateTime.Now.Ticks;
            Ret = objread.SubmitRequest();
            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_READ, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objread.ResponseData.ErrorMsg, "ErrorStatus", objread.ResponseData.Status);
                return false;
            }

            objRes = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_READ);
            if (objRes == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_READ, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            JSONValue objStatus = objRes.get_Member("STATUS", true);
            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_READ, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value == "INVALID" || objStatus.Value == "FAILURE" || objStatus.Value == "FAILED")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_READ, WFConstants.ERROR_STATUS_VALUE);
                return false;
            }

            try
            {
                objSection = objRes.get_Member("SECTION", true);
                objRecord = objSection.get_ArrayElement(0).get_Member("RECORD", true);

                if (objRecord.NumElements == 0)
                    return true;

                for (i = 0; i < objRecord.NumElements; i++)
                {
                    objField = objRecord.get_ArrayElement(i).get_Member("FIELD", true);
                    objUpdateWInfoRecord.Id = objField.get_Member("ID").Value;
                    objUpdateWInfoRecord.Appdata1 = objField.get_Member("APPDATA1").Value;
                    objUpdateWInfoRecord.DataRelPath = objField.get_Member("DATARELPATH").Value;

                    if (ValidateModulo(objUpdateWInfoRecord.Id))
                        ReadInfo.Add(objUpdateWInfoRecord);
                }
            }
            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_READ, "Error in parsing response file " + ex.Message);
                return false;
            }
            return true;
        }

        public Boolean DoGetdata(Int32 WorkId)
        {
            Boolean Ret;
            JSON objJSON = new JSON();
            JSONValue objGetDataRes;
            Int64 StartTime;
            Double ElpasedTime;

            objGetData.WorkID = WorkId.ToString();
            objGetData.DataRelativePath = objGetWI.DataRelativePath;

            StartTime = DateTime.Now.Ticks;
            Ret = objGetData.SubmitRequest();
            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETDATA, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objGetData.ResponseData.ErrorMsg, "ErrorStatus", objGetData.ResponseData.Status);
                return false;
            }

            objGetDataRes = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETDATA);
            if (objGetData == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETDATA, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            if (!Helper.SafeFileExists(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETDATA))
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETDATA, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            return true;
        }

        public void DoMakeSearchablePdf()
        {
            JSON objJSON = new JSON();
            JSONValue objGetDataRes, objBaseData, objDocArr, objDoc, objMem;
            String TransactionId, ImagingExportFilename, InputFile, OutputFile, Status;
            Int32 i;
            Boolean Ret;

            objGetDataRes = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETDATA);
            if (objGetData == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETDATA, WFConstants.ERROR_RESPONSE_EMPTY);
                return;
            }

            try
            {
                objBaseData = objGetDataRes.get_Member("BASEDETAILS", true);
                objMem = objBaseData.get_Member("TRANSACTIONID", true);
                TransactionId = objMem.Value;

                objDocArr = objGetDataRes.get_Member("DOCUMENTS", true);
                if (objDocArr == null)
                    return;

                for (i = 0; i < objDocArr.NumElements; i++)
                {
                    objDoc = objDocArr.get_ArrayElement(i);
                    objMem = objDoc.get_Member("IMAGINGEXPORTFILENAME", true);
                    ImagingExportFilename = objMem.Value;

                    InputFile = AppSupport.BaseImagePath + TransactionId + "\\" + ImagingExportFilename;
                    if (!Helper.SafeFileExists(InputFile))
                    {
                        AppSupport.objLog.WriteLog("DoMakeSearchablePdf", "InputPdf File is Missing", InputFile);
                        continue;
                    }

                    OutputFile = AppSupport.TempPath + "temp_" + ImagingExportFilename ;

                    Ret = ConvertPdfFile(InputFile, OutputFile);
                    if (Ret)
                    {
                        Helper.SafeKill(InputFile);
                        File.Copy(OutputFile, InputFile, true);
                        Helper.SafeKill(OutputFile);
                        Status = "SUCCESS";
                    }
                    else
                    {
                        Status = "FAILED";
                    }

                    AppSupport.objLog.WriteLog("ConvertPdfFile", "InputFile", InputFile, Status);
                }
            }

            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog("DoMakeSearchablePdf", ex.Message);
                return;
            }
        }

        public Boolean ConvertPdfFile(String InputFile, String OutputFile)
        {
            Boolean ProcessTimedout, Ret;
            String InputArgs;
            const Int32 PROCESS_TIMEOUT = 900;

            Helper.SafeKill(OutputFile);
            ProcessTimedout = Ret = false;

            InputArgs = "-i " + InputFile + " -o" + OutputFile + " -n 0";
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            proc.StartInfo.FileName = AppSupport.PDFConverterExePath;
            proc.StartInfo.Arguments = InputArgs;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            ProcessTimedout = !proc.WaitForExit(PROCESS_TIMEOUT * 1000);
            AppSupport.objLog.WriteLog("ExitCode", proc.ExitCode);
            if (ProcessTimedout)
            {
                proc.Kill();
            }
            else if (proc.HasExited)
            {
                if (Helper.SafeFileExists(OutputFile))
                {
                    Ret = true;
                }
            }

            proc.Close();
            return Ret;
        }

        public Boolean DoDocumentum()
        {
            JSON objJSON = new JSON();
            JSONValue objGetDataRes, objDocumentumRes, objStatus;
            Boolean Ret;

            objGetDataRes = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETDATA);
            if (objGetDataRes == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }
            
            Ret = objDocumentum.SubmitRequest(objGetDataRes);
            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objDocumentum.ResponseData.ErrorMsg, "ErrorStatus", objDocumentum.ResponseData.Status);
                return false;
            }

            objDocumentumRes = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_DOCUMENTUM);
            if (objDocumentumRes == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            try
            {
                objStatus = objDocumentumRes.get_Member("STATUS", true);
            }

            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, "Error in parsing Documentum response file", ex.Message);
                return false;
            }

            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value != "SUCCESS")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_DOCUMENTUM, WFConstants.ERROR_STATUS_VALUE, objStatus.Value);
                return false;
            }

            return true;
        }

        public Boolean DoWorkqUpdation(Int32 WorkId, String Appdata1)
        {
            Boolean Ret;
            JSON objJSON = new JSON();
            JSONValue objData, objSection, objRecord, objField, objMem, objAttribute, objResponse, objStatus;
            Int64 StartTime;
            Double ElpasedTime;
            JSONValue objGetWI, objUpdateWIReq;

            objwqu.WorkID = WorkId;
            objwqu.Appdata1 = Appdata1;
            objwqu.Type = "UPDATION";
            objwqu.CurrentQID = 0;

            try
            {
                objGetWI = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETWORKITEM);
                objSection = objGetWI.get_Member("SECTION", true);
                objRecord = objSection.get_ArrayElement(0).get_Member("RECORD", true);
                objField = objRecord.get_ArrayElement(0).get_Member("FIELD", true);
                objMem = objField.get_Member("LASTQUEUEID", true);
                objwqu.LastQId = objMem.Value;
                objMem = objField.get_Member("DATARELPATH", true);
                objwqu.DATARELPATH = objMem.Value;
                objMem = objField.get_Member("FILERELPATH", true);
                objwqu.FILERELPATH = objMem.Value;
            }

            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, "Error in parsing GetWorkitem response file", ex.Message);
                return false;
            }

            objUpdateWIReq = prepareWQUpdationReq();

            StartTime = DateTime.Now.Ticks;
            Ret = objwqu.SubmitRequest(objUpdateWIReq);
            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objwqu.ResponseData.ErrorMsg, "ErrorStatus", objwqu.ResponseData.Status);
                return false;
            }

            objResponse = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_WORKQUPDATION);
            if (objResponse == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            try
            {
                objData = objResponse.get_Member("data", true);
                objSection = objData.get_Member("SECTION", true);
                objRecord = objSection.get_ArrayElement(0).get_Member("RECORD", true);
                objAttribute = objRecord.get_ArrayElement(0).get_Member("ATTRIBUTES", true);
                objStatus = objAttribute.get_Member("STATUS", true);
            }
            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, "Error in parsing WorkqueueUpdation response file", ex.Message);
                return false;
            }

            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value == "INVALID" || objStatus.Value == "FAILURE" || objStatus.Value == "FAILED")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, WFConstants.ERROR_STATUS_VALUE, objStatus.Value);
                return false;
            }

            return true;
        }

        public JSONValue prepareWQUpdationReq()
        {
            JSON objjson = new JSON();
            JSONValue objGWISection, objGWIRecord, objGWIField, objGWIWorkItemType, objWQUData, objWQUSection, objWQURecord, objWQUAttribute, objWQUField;

            JSONValue objWQUreq = objjson.ParseFile(AppSupport.SampleData + "wqu.json");
            JSONValue objGWIRes = objjson.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETWORKITEM);
            JSONValue objGDRes = objjson.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETDATA);
            try
            {
                objWQUreq.get_Member("SessionKey", true).SetValue(objLogin.SessionKey);

                //fetch workitem type from getwitem
                objGWISection = objGWIRes.get_Member("SECTION", true);
                objGWIRecord = objGWISection.get_ArrayElement(0).get_Member("RECORD", true);
                objGWIField = objGWIRecord.get_ArrayElement(0).get_Member("FIELD", true);
                objGWIWorkItemType = objGWIField.get_Member("WORKITEMTYPE", true);

                //set type and workinfodata
                objWQUData = objWQUreq.get_Member("data", true);
                objWQUSection = objWQUData.get_Member("SECTION", true);
                objWQURecord = objWQUSection.get_ArrayElement(0).get_Member("RECORD", true);
                objWQUAttribute = objWQURecord.get_ArrayElement(0).get_Member("ATTRIBUTES", true);
                objWQUAttribute.get_Member("TYPE", true).SetValue("UPDATION");
                objWQUAttribute.get_Member("WORKINFODATA", true).SetValue(objjson.ToString(objGDRes, true));

                //set workitemtype 
                objWQUreq.get_Member("WorkItemType").SetValue(objGWIWorkItemType.Value);

                //fill wqu fileld info using gwi field obj
                objWQURecord.get_ArrayElement(0).get_Member("ATTRIBUTES", true).get_Member("WORKQUEUELOGDATA").SetValue("");

                objWQUField = objWQURecord.get_ArrayElement(0).get_Member("FIELD", true);
                objWQUField.get_Member("ID").SetValue(objGWIField.get_Member("ID").Value);
                objWQUField.get_Member("APPDATA1").SetValue(objGWIField.get_Member("APPDATA1").Value);
                objWQUField.get_Member("APPDATA2").SetValue(objGWIField.get_Member("APPDATA2").Value);
                objWQUField.get_Member("APPDATA3").SetValue(objGWIField.get_Member("APPDATA3").Value);
                objWQUField.get_Member("DATARELPATH").SetValue(objGWIField.get_Member("DATARELPATH").Value);
                objWQUField.get_Member("FILERELPATH").SetValue(objGWIField.get_Member("FILERELPATH").Value);
                objWQUField.get_Member("LASTQUEUEID").SetValue(objGWIField.get_Member("CURRENTQUEUEID").Value);
                objWQUField.get_Member("CURRENTQUEUEID").SetValue(0);
                objWQUField.get_Member("CREATEDON").SetValue(objGWIField.get_Member("CREATEDON").Value);
                objWQUField.get_Member("MODIFIEDON").SetValue(objGWIField.get_Member("MODIFIEDON").Value);
                objWQUField.get_Member("CREATEDBY").SetValue(objGWIField.get_Member("CREATEDBY").Value);
                objWQUField.get_Member("MODIFIEDBY").SetValue(objGWIField.get_Member("MODIFIEDBY").Value);
                objWQUField.get_Member("STATUS").SetValue(objGWIField.get_Member("STATUS").Value);
                objWQUField.get_Member("WORKITEMTYPE").SetValue(objGWIField.get_Member("WORKITEMTYPE").Value);
                objWQUField.get_Member("WORKITEMCATEGORY").SetValue(objGWIField.get_Member("WORKITEMCATEGORY").Value);
                objWQUField.get_Member("WORKITEMSOURCE").SetValue(objGWIField.get_Member("WORKITEMSOURCE").Value);
                objWQUField.get_Member("WORKITEMGROUP").SetValue(objGWIField.get_Member("WORKITEMGROUP").Value);
                objWQUField.get_Member("PROCESSINGUNIT").SetValue(objGWIField.get_Member("PROCESSINGUNIT").Value);
                objWQUField.get_Member("PRIORITY").SetValue(objGWIField.get_Member("PRIORITY").Value);

            }
            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_WORKQUPDATE, "Error in preparing workqueueupdation request", ex.Message);
                return null;
            }

            return objWQUreq;
        }

        public Boolean DoGetWorkitemById(Int32 WorkId)
        {
            Boolean Ret;
            JSON objJSON = new JSON();
            JSONValue objResponse, objSection, objRecord, objField, objMem, objReq;
            Int64 StartTime;
            Double ElpasedTime;

            objGetWI.WorkID = WorkId;

            StartTime = DateTime.Now.Ticks;
            Ret = objGetWI.SubmitRequest();
            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETWORKITEM, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objGetWI.ResponseData.ErrorMsg, "ErrorStatus", objGetWI.ResponseData.Status);
                return false;
            }

            objReq = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETWORKITEM);
            if (objReq == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETWORKITEM, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            JSONValue objStatus = objReq.get_Member("STATUS", true);
            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETWORKITEM, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value == "INVALID" || objStatus.Value == "FAILURE" || objStatus.Value == "FAILED")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETWORKITEM, WFConstants.ERROR_STATUS_VALUE, objStatus.Value);
                return false;
            }

            objGetWI.WorkID = 0;
            try
            {
                objResponse = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_GETWORKITEM);
                objSection = objResponse.get_Member("SECTION", true);
                objRecord = objSection.get_ArrayElement(0).get_Member("RECORD", true);
                objField = objRecord.get_ArrayElement(0).get_Member("FIELD", true);
                objMem = objField.get_Member("ID", true);
                objGetWI.WorkID = objMem.Value;
                objMem = objField.get_Member("DATARELPATH", true);
                objGetWI.DataRelativePath = objMem.Value;
                objMem = objField.get_Member("FILERELPATH", true);
                objGetWI.FileRelativePath = objMem.Value;
            }
            catch (Exception ex)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_GETWORKITEM, "Error in parsing response file", ex.Message);
                return false;
            }

            return (objGetWI.WorkID != 0);
        }

        public Boolean DoLogout()
        {
            Boolean Ret;
            JSON objJSON = new JSON();
            JSONValue objStatus;
            Int64 StartTime;
            Double ElpasedTime;

            StartTime = DateTime.Now.Ticks;
            Ret = objLogout.SubmitRequest();
            ElpasedTime = DateTime.Now.Ticks - StartTime;

            if (!Ret)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGOUT, WFConstants.ERROR_IN_POST_REQUEST, "ErrorMessage", objLogout.ResponseData.ErrorMsg, "ErrorStatus", objLogout.ResponseData.Status);
                return false;
            }

            JSONValue objReq = objJSON.ParseFile(AppSupport.JsonReqResponse + WFConstants.API_RESP_LOGOUT);
            if (objReq == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGOUT, WFConstants.ERROR_RESPONSE_EMPTY);
                return false;
            }

            objStatus = objReq.get_Member("STATUS", true);
            if (objStatus == null)
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGOUT, WFConstants.ERROR_STATUS_IS_NULL);
                return false;
            }

            if (objStatus.Value == "INVALID" || objStatus.Value == "FAILURE" || objStatus.Value == "FAILED")
            {
                AppSupport.objLog.WriteLog(WFConstants.API_LOGOUT, WFConstants.ERROR_STATUS_VALUE, objStatus.Value);
                return false;
            }
            return true;
        }
    }
}
