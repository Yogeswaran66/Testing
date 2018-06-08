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
    class Read
    {
        public String SessionKey { get; set; }
        public String ResponsePath { get; set; }
        public String pattern { get; set; }
        public String DATAELEMENT { get; set; }
        public Int32 tableid { get; set; }
        public Int32 Maxcount { get; set; }
        public Int32 Operation { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();
        JSONValue objReq;

        public Read(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "Read.json";

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

            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_READ);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_READ);

            objReq.get_Member("SessionKey", true).SetValue(SessionKey);
            objReq.get_Member("tableid", true).SetValue(tableid);
            
            JSONValue objData = objReq.get_Member("data", true);
            objData.get_Member("OPERATION", true).SetValue(Operation);
            objData.get_Member("pattern", true).SetValue(pattern);
            //objData.get_Member("DATAELEMENT", true).SetValue(DATAELEMENT);
            objData.get_Member("MAXCOUNT", true).SetValue(Maxcount);

            ResPath = ResponsePath + WFConstants.API_RESP_READ;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_READ, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);
            return Ret;
        }
    }
}
