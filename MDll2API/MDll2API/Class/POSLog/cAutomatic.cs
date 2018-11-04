using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cAutomatic
    {
        //public string C_POSTtAutomatic(string ptJson,string ptCheck
        //                                , string ptmove_type, string ptFTTmnNum, int pnFNSdtSeqNo
        //                                , string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual)
        public string C_POSTtAutomatic(string ptJson, string ptCheck
                                     , string ptmove_type, string ptFTTmnNum, int pnFNSdtSeqNo
                                     , string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual,string ptDTrn)
        {
            string rtResult;
            string tJson = "";
            string tJsonTrn = "";
            string tSQL = "";
            string tExecute = "";
            string tLastUpd = "";
            string tResCode = "";
            string tResMsg = "";
            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";
            StringBuilder oSql;
            string tConnDB = "";
            string tFunction = "6";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            try
            {
                dStart = DateTime.Now;
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                // Sort  Group Function
                oRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");
                if (pnAPIManual == 0)
                {
                    for (int nRow = 0; nRow < oRow.Length; nRow++)
                    {
                        tUriApi = oRow[nRow]["UrlApi"].ToString();
                        tUsrApi = oRow[nRow]["UsrApi"].ToString();
                        tPwdApi = oRow[nRow]["PwdApi"].ToString();

                        // Create Connection String Db
                        tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                        tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                        tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();

                        // Check TPOSLogHis  Existing
                        tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                        cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                        // Get Max FTBathNo Condition To Json
                        tLastUpd = "";
                        tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                        //  Condition ตาม FTBatchNo Get Json
                        tSQL = C_GETtSQL(ptCheck, ptmove_type, ptFTTmnNum, pnFNSdtSeqNo, tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));

                        tExecute = cCNSP.SP_SQLtExecuteJson(tSQL, tConnDB);
                        if (tExecute != "")
                        {
                            if (tJsonTrn == "")
                            {
                                tJsonTrn = tExecute;
                            }
                            else
                            {
                                tJsonTrn = tJsonTrn + ',' + tExecute;
                            }
                        }

                    }
                }
                    
                if (pnAPIManual == 0)
                {
                    if (tJsonTrn != "")
                    {
                        tJson = "{" + Environment.NewLine;
                        tJson = tJson + "\"POSLog\": {" + Environment.NewLine;
                        //*Em 61-07-12
                        tJson = tJson + "\"@xmlns\" : \"http://themall.co.th/retail/sales_transaction\"," + Environment.NewLine;
                        tJson = tJson + "\"@MajorVersion\" : \"6\"," + Environment.NewLine;
                        tJson = tJson + "\"@xmlns:xsi\" : \"http://www.w3.org/2001/XMLSchema-instance\"," + Environment.NewLine;
                        tJson = tJson + "\"@xsi:schemaLocation\" : \"http://themall.co.th/retail/sales_transaction\"," + Environment.NewLine;
                        //+++++++++++++++++
                        tJson = tJson + "\"Transaction\": [" + Environment.NewLine;
                        tJson = tJson + tJsonTrn + Environment.NewLine;
                        tJson = tJson + "]" + Environment.NewLine;
                        tJson = tJson + "}" + Environment.NewLine;
                        tJson = tJson + "}" + Environment.NewLine;

                        //Call API
                        HttpWebRequest oWebReq = (HttpWebRequest)WebRequest.Create(tUriApi);
                        oWebReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(tUsrApi + ":" + tPwdApi)));
                        oWebReq.Method = "POST";
                        byte[] anData = Encoding.UTF8.GetBytes(tJson.ToString());

                        oWebReq.ContentLength = anData.Length;
                        oWebReq.ContentType = "application/json;charset=utf8";
                        using (var oStream = oWebReq.GetRequestStream())
                        {
                            oStream.Write(anData, 0, anData.Length);
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
                            tResCode = oResp.StatusCode.ToString();
                        }

                        dEnd = DateTime.Now;
                        for (int nRow = 0; nRow < oRow.Length; nRow++)
                        {
                            // Create Connection String Db
                            tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                            tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                            tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();

                            // Get Max FTBathNo Condition To Json
                            tLastUpd = "";
                            tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);
                            // Keep Log
                            oSql = new StringBuilder();
                            oSql.AppendLine("INSERT INTO TPOSLogHis(");
                            oSql.AppendLine("FDDateUpd, FTTimeUpd, FTWhoUpd, FDDateIns, FTTimeIns, FTWhoIns,");
                            oSql.AppendLine("FTRemark, FTShdPlantCode, FDSendStartDateTime, FDSendEndDateTime,");
                            oSql.AppendLine("FTBatchNo, FTTransTypeGrp, FTRespCode, FTRespMsg, FTTransCount)");
                            oSql.AppendLine("SELECT ");
                            oSql.AppendLine("CONVERT(VARCHAR(10), GETDATE(), 121) AS FDDateUpd, CONVERT(VARCHAR(10), GETDATE(), 108) AS FTTimeUpd,'System' AS FTWhoUpd,");
                            oSql.AppendLine("     CONVERT(VARCHAR(10), GETDATE(), 121) AS FDDateIns, CONVERT(VARCHAR(10), GETDATE(), 108) AS FTTimeIns,'System' AS FTWhoIns,");
                            oSql.AppendLine("'' AS FTRemark, ISNULL(MAX(FTShdPlantCode), '') AS FTShdPlantCode,'" + string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dStart) + "' AS FDSendStartDateTime,");
                            oSql.AppendLine("'" + string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dEnd) + "' AS FDSendEndDateTime,");
                            oSql.AppendLine("  MAX(CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '')) AS FTBatchNo,'" + tFunction + "' AS FTTransTypeGrp,");
                            oSql.AppendLine("'" + tResCode + "' AS FTRespCode, '" + tResMsg + "' AS FTRespMsg, COUNT(FTShdTransNo) AS FTTransCount");
                            oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(oRow[nRow]["TopRow"]) + " * FROM TPSTSalHD HD with(nolock)");
                            oSql.AppendLine("WHERE HD.FTTmnNum = '" + ptFTTmnNum + "'--ส่งพารามิเตอร์มา");
                            oSql.AppendLine("AND HD.FTTmnNum = '" + ptFTTmnNum + "'--ส่งพารามิเตอร์มา");
                            oSql.AppendLine("AND TPSTSalDT.FNSdtSeqNo = " + pnFNSdtSeqNo + "--ส่งพารามิเตอร์มา");
                            oSql.AppendLine("AND ISNULL(DT.FTSdtDisChgTxt, '') <> ''");
                            if (tLastUpd != "")
                            {
                                oSql.AppendLine("    AND CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '') > '" + tLastUpd + "'");
                            }
                            oSql.AppendLine("    ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                            cCNSP.SP_SQLnExecute(oSql.ToString(), tConnDB);

                        }
                    }

                }
                else
                {
                    tUriApi = ptAPIURL;
                    tUsrApi = ptAPIUsr;
                    tPwdApi = ptAPIPwd;

                    //Call API
                    HttpWebRequest oWebReq = (HttpWebRequest)WebRequest.Create(tUriApi);
                    oWebReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(tUsrApi + ":" + tPwdApi)));
                    oWebReq.Method = "POST";
                    tJson = ptJson;
                    byte[] aData = Encoding.UTF8.GetBytes(tJson.ToString());

                    oWebReq.ContentLength = aData.Length;
                    oWebReq.ContentType = "application/json;charset=utf8";
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
                        tResCode = oResp.StatusCode.ToString();
                    }
                    tJsonTrn = "ทดสอบ API แบบ Manaul";
                }


                string tResultGetdata = "";
                if (tJsonTrn == "")
                {
                    tResultGetdata = "ไม่พบขัอมูล";
                }
                else
                {
                    tResultGetdata = tJsonTrn;
                }

                // rtResult = "ส่งข้อมูลสมบูรณ์|Description " + tResultGetdata + "  ผลลัพท์จาก API: " + tResCode + " Code: " + tStatusCode + "|" + tJson;
                rtResult = "ส่งข้อมูลสมบูรณ์|Code: " + tStatusCode + " ";
                return rtResult;
            }
            catch (Exception oEx)
            {
                return "Error การทำงานเข้า catch|Description: " + oEx.Message.ToString() + "|" + tJson;
            }
            finally
            {
                 rtResult = null;
                 tJson = null;
                tJsonTrn = null;
                tSQL = null;
                tExecute = null;
                tLastUpd = null;
                tResCode = null;
                tResMsg = null;
                tUriApi = null;
                tUsrApi = null;
                tPwdApi = null;
                oSql = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptCheck"></param>
        /// <param name="ptmove_type"></param>
        /// <param name="ptFTTmnNum"></param>
        /// <param name="pnFNSdtSeqNo"></param>
        /// <returns></returns>
        private string C_GETtSQL(string ptCheck, string ptmove_type, string ptFTTmnNum, int pnFNSdtSeqNo, string ptLastUpd, Int64 pnRowTop = 100)
        {
            StringBuilder oSQL = new StringBuilder();
            string rtResult = "";
            

            try
            {

                

                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'{' + CHAR(10) +");
                oSQL.AppendLine("'\"GOODSMVT_HEADER\" : {' + CHAR(10) +");
                oSQL.AppendLine("'\"PSTNG_DATE\" : \"' + CONVERT(VARCHAR(8), GETDATE(), 112) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("'\"DOC_DATE\" : \"' + CONVERT(VARCHAR(8), HD.FDShdTransDate, 112) + '\",' + CHAR(10) +");
                oSQL.AppendLine(" '\"REF_DOC_NO\" : \"' + HD.FTTmnNum + HD.FTShdTransNo + '\",' + CHAR(10) +");
                oSQL.AppendLine(" (CASE '" + ptCheck +"'--ส่งค่ามา");
                oSQL.AppendLine("WHEN '1' THEN '\"ACTION\" : \"CHECK\"'");
                oSQL.AppendLine("WHEN '2' THEN '\"ACTION\" : \"CREATE\"'");
                oSQL.AppendLine("WHEN '3' THEN '\"ACTION\" : \"CANCEL\"'");
                oSQL.AppendLine("ELSE '\"ACTION\" : \"CHECK\"' END) +CHAR(10) +");
                oSQL.AppendLine("'},' + CHAR(10) +");
                oSQL.AppendLine("'\"GOODSMVT_CODE\" : {' + CHAR(10) +");
                oSQL.AppendLine("'\"GM_CODE\" : \"' + '04' + '\"' + CHAR(10) +");
                oSQL.AppendLine("'},' + CHAR(10) +");
                oSQL.AppendLine("'\"TESTRUN\" : {' + CHAR(10) +");
                oSQL.AppendLine("'\"TESTRUN\" : \"' + 'X' + '\"' + CHAR(10) +");
                oSQL.AppendLine("'},' + CHAR(10) +");
                oSQL.AppendLine("'\"GOODSMVT_ITEM\" : {' + CHAR(10) +");
                oSQL.AppendLine("'\"PLANT\" : \"' + ISNULL(HD.FTShdPlantCode, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"STGE_LOC\" : \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("(CASE '"+ ptmove_type + "'--ส่งค่ามา");
                oSQL.AppendLine("WHEN '1' THEN '\"MOVE_TYPE\" : \"311\",'");
                oSQL.AppendLine("WHEN '2' THEN '\"MOVE_TYPE\" : \"311\",'");
                oSQL.AppendLine("WHEN '3' THEN '\"MOVE_TYPE\" : \"312\",'");
                oSQL.AppendLine("ELSE '\"MOVE_TYPE\" : \"311\",' END) +CHAR(10) +");
                oSQL.AppendLine("'\"ENTRY_QNT\" : \"' + CONVERT(VARCHAR, DT.FCSdtQty) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"ENTRY_UOM\" : \"' + ISNULL(DT.FTPunCode, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"MOVE_PLANT\" : \"' + ISNULL(HD.FTShdPlantCode, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"MOVE_STLOC\" : \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"EAN_UPC\" : \"' + ISNULL(DT.FTSdtBarCode, '') + '\"' + CHAR(10) +");
                oSQL.AppendLine("'},' + CHAR(10) +");
                oSQL.AppendLine("'\"GOODSMVT_SERIALNUMBER\" : {' + CHAR(10) +");
                oSQL.AppendLine("'\"MATDOC_ITM\" : \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SERIALNO\" : \"' + ISNULL(DT.FTSdtDisChgTxt, '') + '\"' + CHAR(10) +");
                oSQL.AppendLine("'}' + CHAR(10) +");
                oSQL.AppendLine("'}'");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock) ON HD.FTTmnNum = DT.FTTmnNum AND HD.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("WHERE HD.FTTmnNum = '"+ ptFTTmnNum +"'--ส่งพารามิเตอร์มา");
                oSQL.AppendLine("AND HD.FTTmnNum = '" + ptFTTmnNum +"'--ส่งพารามิเตอร์มา");
                oSQL.AppendLine("AND DT.FNSdtSeqNo = " + pnFNSdtSeqNo + "--ส่งพารามิเตอร์มา");
                oSQL.AppendLine("AND ISNULL(DT.FTSdtDisChgTxt, '') <> ''");
                if (ptLastUpd != "")
                {
                    oSQL.AppendLine(" AND CONVERT(varchar(8),TPSTSalHD.FDDateUpd,112) + REPLACE(TPSTSalHD.FTTimeUpd,':','') >= '" + ptLastUpd + "'");
                }
                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception oEx)
            {
                throw oEx;

            } finally {
                oSQL = null;
                rtResult = null;
            }
        }
    }
}
