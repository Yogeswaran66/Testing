using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using exhelper;
using JSONXLib;
using GenericPOSTRequest;
using System.IO;
using Support;

namespace API
{
    class Logout
    {
        public String SessionKey { get; set; }
        public String ResponsePath { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSONXLib.JSON();
        JSONValue objReq;

        public Logout(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");            
        }

        public Boolean Initialise()
        {
            String SampleData;
           
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "Logout.json";

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

            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_LOGOUT);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_LOGOUT);

            objReq.get_Member("SessionKey", true).SetValue(SessionKey);

            ResPath = ResponsePath + WFConstants.API_RESP_LOGOUT;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_LOGOUT, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);
            return Ret;
        }
    }
}
