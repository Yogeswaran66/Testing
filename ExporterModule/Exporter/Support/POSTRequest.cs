using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Net;

namespace GenericPOSTRequest
{
    public class POSTRequest
    {
        public struct ResponseData
        {
            public String ErrorMsg;
            public String Status;
        }        

        private static String GetImageBoundary()
        {
            return "7cd1d6371ec";
        }

        private static String GetImagePostSuffix()
        {
            String suffix;
            suffix = "\r\n--" + GetImageBoundary() + "--\r\n";
            return (suffix);

        }

        private static String GetImageHeader()
        {
            String header;
            header = "Content-Type: multipart/form-data; boundary=" + GetImageBoundary() + "\r\n";
            return header;
        }

        private static String GetUploadImagePostPrefix(String FieldName, String FileFieldName, String FileName, String Boundary, String DatatoSend)
        {
            String prefix;

            prefix = "--" + Boundary + "\r\n";
            prefix += "Content-Disposition: form-data; name=\"method\"" + "\r\n\r\n";
            prefix += "POST" + "\r\n";
            prefix += "--" + Boundary + "\r\n";
            prefix += "Content-Disposition: form-data; name=\"" + FieldName + "\"" + "\r\n\r\n";
            prefix += DatatoSend + "\r\n";
            prefix += "--" + Boundary + "\r\n";

            if (String.IsNullOrEmpty(FileName))
            {
                return (prefix);
            }

            prefix += "Content-Disposition: form-data; name=\"" + FileFieldName + "\"; filename=\"" + FileName + "\"" + "\r\n";
            prefix += "Content-Type: image/tiff" + "\r\n\r\n";

            return (prefix);
        }

        private static String GetUploadImagePostPrefix_documentum(String FieldName, String FileFieldName, String FileName, String Boundary, String DatatoSend)
        {
            String prefix;

            prefix = "--" + Boundary + "\r\n";
            prefix += "Content-Disposition: form-data; name=\"" + FieldName + "\"" + "\r\n\r\n";
            prefix += DatatoSend;


            if (String.IsNullOrEmpty(FileName))
            {
                prefix += GetImagePostSuffix();
                return (prefix);
            }
            else
            {
                prefix += "--" + Boundary + "\r\n";
            }

            prefix += "Content-Disposition: form-data; name=\"" + FileFieldName + "\"; filename=\"" + FileName + "\"" + "\r\n";
            prefix += "Content-Type: image/tiff" + "\r\n\r\n";

            return (prefix);
        }

        private static bool IsValidURI(string url)
        {
            if (String.IsNullOrEmpty(url))
                return false;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return false;

            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return false;

            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public static bool HttpPostRequest(String url, String postData, ref ResponseData RD, String Path, String file = "", String datafieldname = "")
        {
            String HttpReq;

            if (!IsValidURI(url))
            {
                RD.ErrorMsg = "URL is Invalid";
                RD.Status = "Invalid";                
                return false;
            }

            if (String.IsNullOrEmpty(Path))
            {
                RD.ErrorMsg = "Output Path is Invalid";
                RD.Status = "Invalid";
                return false;
            }

            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";

            request.ContentType = "multipart/form-data; boundary=" + GetImageBoundary();

            HttpWebResponse response;
            try
            {
                Stream datastream = request.GetRequestStream();
                if (datafieldname.Length == 0)
                {
                    HttpReq = GetUploadImagePostPrefix("data", "file", file, GetImageBoundary(), postData);
                }
                else
                {
                    HttpReq = GetUploadImagePostPrefix_documentum(datafieldname, "file", file, GetImageBoundary(), postData);
                }

                byte[] byteArray = Encoding.UTF8.GetBytes(HttpReq);

                datastream.Write(byteArray, 0, byteArray.Length);

                if (!String.IsNullOrEmpty(file))
                {
                    byte[] byteArray1 = System.IO.File.ReadAllBytes(file);
                    byte[] byteArray2 = Encoding.UTF8.GetBytes(GetImagePostSuffix());

                    datastream.Write(byteArray1, 0, byteArray1.Length);
                    datastream.Write(byteArray2, 0, byteArray2.Length);
                }

                response = (HttpWebResponse)request.GetResponse();

                if (String.IsNullOrEmpty(response.ContentType))
                {
                    RD.ErrorMsg = "Please check the API Name : " + url;
                    RD.Status = response.StatusDescription;
                }
            }
            catch(Exception e)
            {
                RD.ErrorMsg = "Please check the URL : " + e.Message;
                RD.Status = "Invalid";
                return false;
            }

            if (HttpStatusCode.OK != response.StatusCode)
            {
                RD.ErrorMsg ="Error in Reading Response from Server";
                RD.Status = response.StatusDescription;
                return false;
            }

            Stream dataStream = response.GetResponseStream();
            bool readsuccess = false;
            
            FileStream File = new FileStream(Path, FileMode.Create, FileAccess.Write);
            byte[] MyBuffer = new byte[4096];
            int byteRead;

            try
            {
                while (0 < (byteRead = dataStream.Read(MyBuffer, 0, MyBuffer.Length)))
                {
                    File.Write(MyBuffer, 0, byteRead);
                }
                readsuccess = true;
            }
            catch(Exception e)
            {
                RD.ErrorMsg = "Error in Reading DataStream|" + e.Message;
                RD.Status = response.StatusDescription;
            }

            File.Close();
            response.Close();

            RD.ErrorMsg = "";
            RD.Status = response.StatusDescription;

            return readsuccess;
        }
    }
}
