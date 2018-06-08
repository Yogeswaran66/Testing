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
    class WorkQUpdation
    {
        public String SessionKey { get; set; }
        public String ResponsePath { get; set; }
        public String Appdata1 { get; set; }        
        public String Type { get; set; }
        public String DATARELPATH { get; set; }
        public String FILERELPATH { get; set; }
        public Int32 LastQId { get; set; }
        public Int32 CurrentQID { get; set; }
        public Int32 WorkID { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();

        public WorkQUpdation(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");            
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "wqu.json";

            if (!ResponsePath.EndsWith("\\")) ResponsePath += "\\";

            return true;
        }

        public Boolean SubmitRequest(JSONValue objReq)
        {
            String PostData;
            Boolean Ret;
            String ResPath;

            ResponseData = new POSTRequest.ResponseData();

            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_WORKQUPDATION);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_WORKQUPDATION);

            ResPath = ResponsePath + WFConstants.API_RESP_WORKQUPDATION;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_WORKQUPDATION, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);
            return Ret;
        }
    }
}
