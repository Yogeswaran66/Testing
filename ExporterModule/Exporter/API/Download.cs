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
    class Download
    {
        public String ImageRef { get; set; }
        public String SessionKey { get; set; }
        public String FileName { get; set; }
        public String ResponsePath { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();
        JSONValue objReq;

        public Download(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "Download.json";

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
            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_DOWNLOAD);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_DOWNLOAD);
            
            objReq.get_Member("IMAGEREF", true).SetValue(ImageRef);
            objReq.get_Member("SessionKey", true).SetValue(SessionKey);
            objReq.get_Member("FILENAME", true).SetValue(FileName);

            ResPath = ResponsePath + WFConstants.API_RESP_DOWNLOAD;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_DOWNLOAD, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath);

            return Ret;
        }
    }
}
