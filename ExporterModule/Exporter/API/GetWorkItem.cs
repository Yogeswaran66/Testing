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
    class GetWorkItem
    {
        public String SessionKey { get; set; }
        public Int32 WorkID { get; set; }
        public String Appdata1 { get; set; }
        public String DataRelativePath { get; set; }
        public String FileRelativePath { get; set; }
        public String ResponsePath { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();
        JSONValue objReq;

        public GetWorkItem(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "GetWorkitem_WorkID.json";

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
            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_GETWORKITEM);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_GETWORKITEM);
           
            objReq.get_Member("SessionKey", true).SetValue(SessionKey);
            JSONValue objData = objReq.get_Member("data", true);

            if(WorkID !=0)
                objData.get_Member("Id", true).SetValue(WorkID);
            else
                objData.get_Member("Appdata1", true).SetValue(Appdata1);

            ResPath = ResponsePath + WFConstants.API_RESP_GETWORKITEM;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_GETWORKITEM, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);
            return Ret;
        }
    }
}
