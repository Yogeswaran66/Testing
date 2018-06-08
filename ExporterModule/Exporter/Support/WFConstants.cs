using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Support
{
    static class WFConstants
    {
        public const String API_LOGIN = "login";
        public const String API_LOGOUT = "logout";
        public const String API_READ = "rt";
        public const String API_GETWORKITEM = "gwn";
        public const String API_WORKQUPDATE = "wqu";
        public const String API_DOWNLOADFILE = "df";
        public const String API_UPLOADFILE = "uf";
        public const String API_GETDATA = "gwd";
        public const String API_DOCUMENTUM = "documentum";
        
        public const String API_RESP_LOGIN = "LoginResponse.json";
        public const String API_REQ_LOGIN = "LoginRequest.json";

        public const String API_RESP_LOGOUT = "LogoutResponse.json";
        public const String API_REQ_LOGOUT = "LogoutRequest.json";

        public const String API_RESP_GETDATA = "GetdataResponse.json";
        public const String API_REQ_GETDATA = "GetdataRequest.json";

        public const String API_RESP_UPLOAD = "UploadResponse.json";
        public const String API_REQ_UPLOAD = "UploadRequest.json";

        public const String API_RESP_DOWNLOAD = "DownloadResponse.tif";
        public const String API_REQ_DOWNLOAD = "DownloadRequest.json";

        public const String API_RESP_GETWORKITEM = "GetWorkitemRes.json";
        public const String API_REQ_GETWORKITEM = "GetWorkitemRequest.json";

        public const String API_RESP_WORKQUPDATION = "WorkQUpdationResponse.json";
        public const String API_REQ_WORKQUPDATION = "WorkQUpdationRequest.json";

        public const String API_RESP_READ = "ReadResponse.json";
        public const String API_REQ_READ = "ReadRequest.json";

        public const String API_RESP_DOCUMENTUM = "DocumentumResponse.json";
        public const String API_REQ_DOCUMENTUM = "DocumentumRequest.json";

        public const String ERROR_STATUS_IS_NULL = "Status field is not available in response file";
        public const String ERROR_STATUS_VALUE = "Request Failed";
        public const String ERROR_RESPONSE_EMPTY = "Not received response data from server";
        public const String ERROR_IN_POST_REQUEST = "Error in sending post request to server";

        public const Int32 PRM_UPLOAD_ISNEW = 0;
        public const Int32 PRM_UPLOAD_IMGFILE = 1;
        public const Int32 PRM_UPLOAD_CONVERTTOPDF = 2;

        public const Int32 PRM_WQU_TYPE= 0;
        public const Int32 PRM_WQU_LASTQID= 1;
        public const Int32 PRM_WQU_CURQID= 2;
        public const Int32 PRM_WQU_WORKID = 3;
        public const Int32 PRM_WQU_APPDATA1 = 4;
        public const Int32 PRM_WQU_DATARELPATH = 5;

        public const Int32 PRM_GWI_BY_ID = 0;
        public const Int32 PRM_GWI_BY_APPDATA1 = 0;

        public const Int32 API_READQ_EXPORTER= 8;
    }
}
