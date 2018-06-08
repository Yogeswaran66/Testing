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
    class Documentum
    {
        public String ResponsePath { get; set; }
        public String SessionKey { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();
        
        public Documentum(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");
        }

        public Boolean Initialise()
        {
            if (!ResponsePath.EndsWith("\\")) ResponsePath += "\\";                     
            return true;
        }

        public Boolean SubmitRequest(JSONValue objReq)
        {
            String PostData;            
            Boolean Ret;
            String ResPath;

            ResponseData = new POSTRequest.ResponseData();

            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_DOCUMENTUM);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_DOCUMENTUM);

            ResPath = ResponsePath + WFConstants.API_RESP_DOCUMENTUM;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_DOCUMENTUM, PostData);

            Ret = POSTRequest.HttpPostRequest(AppSupport.ExternalURL, PostData, ref ResponseData, ResPath, "", AppSupport.ExternalUrlfieldname);
            return Ret;            
        }
    }
}
