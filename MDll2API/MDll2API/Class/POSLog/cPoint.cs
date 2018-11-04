using MDll2API.Class.ST_Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cPoint
    {
       // public string C_POSTtPoint(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual)
        public string C_POSTtPoint(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual, string ptDTrn)
        {
            string tJson = "";
            StringBuilder oSQL = new StringBuilder();
            string tSQL = "";
            string tExecute = "";
            string tConnDB = "";
            string tFunction = "1"; //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig = new DataTable();
            DataRow[] oRow;
            string tCHKDBLogHis;
            string tLastUpd = "";
            StringBuilder oSql;
            string tResCode = "";
            string tResMsg = "";
            DateTime dStart;
            DateTime dEnd;
            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";
            string tJsonTrn = "";
            string rtResult;
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
                        tCHKDBLogHis = cCNSP.SP_GETtCHKDBLogHis();
                        cCNSP.SP_SQLnExecute(tCHKDBLogHis, tConnDB);

                        // Get Max FTBathNo Condition To Json
                        tLastUpd = "";  // Reset
                        tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                        //  Condition ตาม FTBatchNo Get Json
                        tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
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
                        byte[] aData = Encoding.UTF8.GetBytes(tJson.ToString());

                        oWebReq.ContentLength = aData.Length;
                        oWebReq.ContentType = "application/json;charset=utf8";
                        using (var oStream = oWebReq.GetRequestStream())
                        {
                            oStream.Write(aData, 0, aData.Length);
                        }
                        using (HttpWebResponse oResp = (HttpWebResponse)oWebReq.GetResponse())
                        {
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
                            oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(oRow[nRow]["TopRow"]) + " HD.* FROM TPSTSalHD HD with(nolock)");
                            oSql.AppendLine("INNER JOIN TPSTSalePoint PNT with(nolock) ON HD.FTTmnNum = PNT.FTTmnNum AND HD.FTShdTransNo = PNT.FTShdTransNo");
                            oSql.AppendLine("     AND HD.FDShdTransDate = PNT.FDShdTransDate");
                            oSql.AppendLine("LEFT JOIN TSysTransType TrnType with(nolock) ON HD.FTShdTransType = TrnType.FTSttTranCode");

                            if (tLastUpd != "")
                            {
                                oSql.AppendLine("    AND CONVERT(varchar(8), TPSTSalHD.FDDateUpd, 112) + REPLACE(TPSTSalHD.FTTimeUpd, ':', '') > '" + tLastUpd + "'");
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

                 rtResult = "ส่งข้อมูลสมบูรณ์|Description " + tResultGetdata + "  ผลลัพท์จาก API: " + tResCode + "|" + tJson; ;
               // rtResult = "ส่งข้อมูลสมบูรณ์|Code: " + tStatusCode + " ";
                return rtResult;
            }
            catch (Exception oEx)
            {
                return "Error การทำงานเข้า catch|Description: " + oEx.Message.ToString() + "|" + tJson;
            }
            finally
            {
                tJson = null;
                oSQL = null;
                tSQL = null;
                tExecute = null;
              //  oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
                tCHKDBLogHis = null;
                tLastUpd = null;
                oSql = null;
                tResCode = null;
                tResMsg = null;
                tUriApi = null;
                tUsrApi = null;
                tPwdApi = null;
                tJsonTrn = null;
                rtResult = null;
            }
        }

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop)
        {
            StringBuilder oSQL = new StringBuilder();
            string rtResult = "";

            string tPosLnkDB = "";
            string tPosCntDB = "";

            DataTable oTblPosCenter;
            try
            {
                //2018-08-28 Add config POS Center เพื่อใช้ดึงข้อมูล Master
                //load poscenter config
                oTblPosCenter = cCNSP.SP_GEToPosCnt();
                if (oTblPosCenter != null)
                {
                    try
                    {
                        tPosLnkDB = oTblPosCenter.Rows[0]["DbOwner"].ToString();
                        tPosCntDB = oTblPosCenter.Rows[0]["DBName"].ToString();
                        if (tPosLnkDB != "")
                        {
                            tPosLnkDB = tPosCntDB + "." + tPosLnkDB + ".";
                        }
                    }
                    catch (Exception ex)
                    {
                        tPosLnkDB = "";
                    }
                }

                oSQL.AppendLine("SELECT ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP " + pnRowTop +" ',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) + ");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode,'') + '\"},' + CHAR(10) + ");
                oSQL.AppendLine("'\"WorkstationID\":\"' + HD.FTTmnNum + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + HD.FTTmnNum + HD.FTShdTransNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"' + FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"Reference\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '\"LocationID\":\"' + ISNULL(HD.FTTnmLocCode,'') + '\",' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '\"LocationDescription\":\"' + ISNULL(HD.FTTnmLocDesc,'') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"RetailTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '\"@ItemType\": \"' + ISNULL(TrnType.FTSttAbb, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '\"LineItem\": [' + CHAR(10) +");
                oSQL.AppendLine(" ISNULL(");
                oSQL.AppendLine("  STUFF((");
                oSQL.AppendLine("  SELECT");
                oSQL.AppendLine(" ',{' + CHAR(10) +");
                oSQL.AppendLine("  CHAR(9) + CHAR(9) + FTData + CHAR(10) +");
                oSQL.AppendLine("  CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\":\"' + CONVERT(VARCHAR, ROW_NUMBER() OVER(ORDER BY FTType)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("  CHAR(9) + CHAR(9) + CHAR(9) + '\"ItemLink\": \"' + '2' + '\"' + CHAR(10) +");
                oSQL.AppendLine("  CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("   FROM");
                oSQL.AppendLine("   (");
                oSQL.AppendLine("       SELECT '1' AS FTType,");
                oSQL.AppendLine("      (CHAR(9) + '\"LoyaltyReward\": {' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + '2' + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"LoyaltyID\": \"' + FTSpoMemID + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"LoyaltyProgramID\": \"' + (CASE WHEN FTSpoType IN('1', '2', '3', '4', '5') THEN 'Basic' ELSE 'Extra' END) + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CustomerName\": \"' + FTCstPointName + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ExpiryDate\": \"' + '9999-12-31' + '\",' + CHAR(10) +");  //2020-03-31
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PointsAwarded\": \"' + CONVERT(VARCHAR, FCSpoPoint) + '\",' + CHAR(10) +");
                oSQL.AppendLine("      (CASE WHEN FTSpoType IN('1', '2', '3', '4', '5') THEN");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Discount\": {\"Amount\":\"' + CONVERT(VARCHAR, FCSpoAmt) + '\"}'");
                oSQL.AppendLine("       ELSE");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Discount\": {' + CHAR(10) +");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"' + 'Substract' + '\",' + CHAR(10) +");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, FCSpoAmt) + '\"' + CHAR(10) +");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("          CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("        END) + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},') AS FTData");
                oSQL.AppendLine("       FROM TPSTSalePoint with(nolock)");
                oSQL.AppendLine("       WHERE FTTmnNum = HD.FTTmnNum AND FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("      AND FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("    ) tmp");
                oSQL.AppendLine("   WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine("  FOR XML PATH('')");
                oSQL.AppendLine("   ),1,1,'') ,'') +CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '\"Customer\": { \"CustomerID\": \"' + FTCstCode + '\" }' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '},' +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"'+ CONVERT(VARCHAR(10),HD.FDShdTransDate,121) +'\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BeginDateTime\": \"'+ CONVERT(VARCHAR,TmpHD.FDBeginDate,127) +'\",' + CHAR(10) +");
                oSQL.AppendLine("'\"EndDateTime\": \"'+ CONVERT(VARCHAR,TmpHD.FDEndDate,127) +'\"' + CHAR(10) +");
                oSQL.AppendLine("'}'");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalePoint PNT with(nolock) ON HD.FTTmnNum = PNT.FTTmnNum AND HD.FTShdTransNo = PNT.FTShdTransNo");
                oSQL.AppendLine("     AND HD.FDShdTransDate = PNT.FDShdTransDate");
                oSQL.AppendLine("LEFT JOIN "+ tPosLnkDB +"TSysTransType TrnType with(nolock) ON HD.FTShdTransType = TrnType.FTSttTranCode");
                oSQL.AppendLine("INNER JOIN (SELECT FTShdPlantCode,FDShdTransDate,MIN(FDShdSysDate+FTShdSysTime) AS FDBeginDate , MAX(FDShdSysDate+FTShdSysTime) AS FDEndDate");
                oSQL.AppendLine("	FROM TPSTSalHD");
                oSQL.AppendLine("	GROUP BY FTShdPlantCode,FDShdTransDate ");
                oSQL.AppendLine("	) TmpHD ON ISNULL(HD.FTShdPlantCode,'') = ISNULL(TmpHD.FTShdPlantCode,'') AND HD.FDShdTransDate = TmpHD.FDShdTransDate");
                if (ptLastUpd != "")
                {
                    oSQL.AppendLine("WHERE CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') >= '" + ptLastUpd + "'");
                }
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,''),'')");
                oSQL.AppendLine("FOR XML PATH('')");

                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception oEx)
            {
                return null;
            }
        }
    }
}
