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
    class Upload
    {
        public String SessionKey { get; set; }
        public String is_new { get; set; }
        public String ImageRef { get; set; }
        public String FileName { get; set; }
        public String ResponsePath { get; set; }
        public String FileHash { get; set; }
        public String ConvertToPDF { get; set; }
        public String FilePath { get; set; }
        public POSTRequest.ResponseData ResponseData;

        String Url, InputPath;
        JSON objJSON = new JSON();
        JSONValue objReq;

        public Upload(String strUrl)
        {
            Url = strUrl;
            ResponsePath = AppConfiguration.GetValueFromAppConfig("Paths_JsonReqResponse", "");
            InputPath = AppConfiguration.GetValueFromAppConfig("Paths_SampleData", "");
        }

        public Boolean Initialise()
        {
            String SampleData;
            
            if (!InputPath.EndsWith("\\")) InputPath += "\\";
            SampleData = InputPath + "Upload.json";

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

            Helper.SafeKill(ResponsePath + WFConstants.API_REQ_UPLOAD);
            Helper.SafeKill(ResponsePath + WFConstants.API_RESP_UPLOAD);

            objReq.get_Member("SessionKey", true).SetValue(SessionKey);
            objReq.get_Member("is_new", true).SetValue(is_new);
            objReq.get_Member("ImageRef", true).SetValue(ImageRef);
            objReq.get_Member("filehash", true).SetValue(FileHash);
            objReq.get_Member("ConvertToPDF", true).SetValue(int.Parse(ConvertToPDF));
            objReq.get_Member("FileName", true).SetValue(FileName);

            ResPath = ResponsePath + WFConstants.API_RESP_UPLOAD;
            PostData = objJSON.ToString(objReq);
            File.WriteAllText(ResponsePath + WFConstants.API_REQ_UPLOAD, PostData);

            Ret = POSTRequest.HttpPostRequest(Url, PostData, ref ResponseData, ResPath, FilePath);

            return Ret;
        }
    }
}
