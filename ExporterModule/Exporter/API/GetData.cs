using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using exhelper;
using GenericPOSTRequest;
using JSONXLib;
using System.IO;
using Support;

namespace API
{
    class GetData
    {
        public String SessionKey { get; set; }
        //public Int32 Option { get; set; }
        public String ResponsePath { get; set; }
        public String ReqPath { get; set; }
        public String WorkID { get; set; }
        public String DataRelativePath { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSONXLib.JSON();
        JSONValue objReq;

        public GetData(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");            
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "Getdata.json";

            if (!ResponsePath.EndsWith("\\")) ResponsePath += "\\";
            
            objReq = objJSON.ParseFile(SampleData);
            if (objReq == null)
                return false;

            return true;
        }

        public Boolean SubmitRequest()
        {
            String PostData;
            Boolean Ret;
            String ResPath;

            ResponseData = new POSTRequest.ResponseData();
            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_GETDATA);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_GETDATA);
           
            objReq.get_Member("SessionKey", true).SetValue(SessionKey);
            //objReq.get_Member("Option", true).SetValue(Option);   
            objReq.get_Member("workid", true).SetValue(int.Parse(WorkID));
            objReq.get_Member("dataref", true).SetValue(DataRelativePath);

            ResPath = ResponsePath + WFConstants.API_RESP_GETDATA;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_GETDATA, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);
            return Ret;
        }
    }
}
