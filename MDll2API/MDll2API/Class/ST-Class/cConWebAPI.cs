using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MDll2API.Class.ST_Class
{
    public static class cConWebAPI
    {
        public static string C_CONtWebAPI(string ptUriApi, string ptUsrApi, string ptPwdApi, string ptJson)
        {
            #region "Call API"
            string tStatusCode = "" ;
            try
            {
                ptJson = " '"+ ptJson + "'  "; //ทดสอบ MOCK-API *เวลาใช้งานจริงจะลบออก 

                byte[] aData = Encoding.UTF8.GetBytes(ptJson.ToString());
                HttpWebRequest oWebReq = (HttpWebRequest)WebRequest.Create(ptUriApi);
                oWebReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(ptUsrApi + ":" + ptPwdApi)));
                oWebReq.Method = "POST";
                oWebReq.ContentType = "application/json;charset=utf8";
                oWebReq.ContentLength = aData.Length;

                using (var oStream = oWebReq.GetRequestStream())
                {
                    oStream.Write(aData, 0, aData.Length);
                }

                using (HttpWebResponse oResp = (HttpWebResponse)oWebReq.GetResponse())
                {
                    HttpStatusCode oHttp = oResp.StatusCode;

                    switch (oHttp)
                    {
                        case HttpStatusCode.OK:
                            {
                                tStatusCode = "200";
                            }
                            break;
                        case HttpStatusCode.Accepted:
                            {
                                tStatusCode = "202";
                            }
                            break;
                        case HttpStatusCode.NotAcceptable:
                            {
                                tStatusCode = "406";
                            }
                            break;
                    }
                }
                return tStatusCode;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }

            #endregion "Call API"
        }
    }
}
