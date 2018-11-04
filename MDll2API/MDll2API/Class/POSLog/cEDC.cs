using MDll2API.Class.ST_Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cEDC
    {
        private string tC_DateTrn = "";
        private string tC_Plant = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        // public string C_POSTtEDC(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual)
        public string C_POSTtEDC(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual, string ptDTrn, string[] patPlantEDC, string ptMode)
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
            string tResp = "";
            StringBuilder oSql;
            string tConnDB = "";
            string tFunction = "9";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order ,8:Bank Deposit ,9:EDC
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04
            string tFileName = "";
            try
            {
                //tC_Plant = ptPlant;
                tC_DateTrn = ptDTrn;
                dStart = DateTime.Now;
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                // Sort  Group Function
                oRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");

                if (!(patPlantEDC == null))
                {
                    for (int nLoop = 0; nLoop < patPlantEDC.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            tC_Plant += "'" + patPlantEDC[nLoop] + "'";
                        }
                        else
                        {
                            tC_Plant += ", '" + patPlantEDC[nLoop] + "'";
                        }
                    }
                }

                if (pnAPIManual == 0)
                {
                    for (int nRow = 0; nRow < oRow.Length; nRow++)
                    {
                        tUriApi = oRow[nRow]["UrlApi"].ToString();
                        tUsrApi = oRow[nRow]["UsrApi"].ToString();
                        tPwdApi = oRow[nRow]["PwdApi"].ToString();
                        tWorkStationID = oRow[nRow]["WorkStationID"].ToString(); //*Em 61-08-04
                        tWorkStation = oRow[nRow]["WorkStation"].ToString(); //*Em 61-08-04

                        // Create Connection String Db
                        tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                        tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                        tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();

                        // Check TPOSLogHis  Existing
                        tSQL = cCNSP.SP_GETtCHKDBLogHis();
                        cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                        // Get Max FTBathNo Condition To Json
                        tLastUpd = "";
                        tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                        //  Condition ตาม FTBatchNo Get Json
                        //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                        tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation, ptMode);  //*Em 61-07-24

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
                        if (tJsonTrn == "[]") { tJsonTrn = ""; }
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

                        //WRITE JSON Gen SALE_YYYY-MM-DD:HH:mm:ss
                        tFileName = cCNSP.SP_WRItJSON(tJson, "EDC");

                        //Call API
                        if (tC_APIEnable == "true")
                        {
                            #region "Call API"
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
                            #endregion "Call API"
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
                            oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(oRow[nRow]["TopRow"]) + " * FROM TPSTSalHD with(nolock)");
                            oSql.AppendLine("    WHERE FTShdTransType IN ('03','04','05','10','11','14','15','21','22','23','26','27')");
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
                    //HttpWebRequest oWebReq = (HttpWebRequest)WebRequest.Create(tUriApi);
                    //oWebReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(tUsrApi + ":" + tPwdApi)));
                    //oWebReq.Method = "POST";
                    //tJson = ptJson;
                    //byte[] aData = Encoding.UTF8.GetBytes(tJson.ToString());

                    //oWebReq.ContentLength = aData.Length;
                    //oWebReq.ContentType = "application/json;charset=utf8";
                    //using (var oStream = oWebReq.GetRequestStream())
                    //{
                    //    oStream.Write(aData, 0, aData.Length);
                    //}
                    //using (HttpWebResponse oResp = (HttpWebResponse)oWebReq.GetResponse())
                    //{
                    //    HttpStatusCode oHttp = oResp.StatusCode;
                    //    switch (oHttp)
                    //    {
                    //        case HttpStatusCode.OK:
                    //            {
                    //                tStatusCode = "200";
                    //            }
                    //            break;
                    //        case HttpStatusCode.Accepted:
                    //            {
                    //                tStatusCode = "202";
                    //            }
                    //            break;
                    //        case HttpStatusCode.NotAcceptable:
                    //            {
                    //                tStatusCode = "406";
                    //            }
                    //            break;
                    //    }
                    //    tResCode = oResp.StatusCode.ToString();
                    //}
                    tJsonTrn = "ทดสอบ API แบบ Manaul";
                }

                string tResultGetdata = "";
                if (tJsonTrn == "")
                {
                    tResultGetdata = "ไม่พบขัอมูล";
                }
                else
                {
                    tResultGetdata = "ส่งข้อมูลสมบูรณ์";
                }
                //rtResult = "ส่งข้อมูลสมบูรณ์|Description " + tResultGetdata + "  ผลลัพท์จาก API: " + tResCode + " Code: " + tStatusCode + "|" + tJson;
                rtResult = "สถานะ:" + tResultGetdata + "|Code: " + tStatusCode + "|" + tFileName;
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
                tResp = null;
                oSql = null;
             //   oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100, string ptWorkStationID = "", string ptWorkStation = "", string ptMode = "")
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

                oSQL.AppendLine("SELECT '[' + ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP " + pnRowTop + "',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode, '') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' + '" + ptWorkStation + "' + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + '" + ptWorkStationID + "' + STUFF('00000', 6 - LEN(ROW_NUMBER() OVER(ORDER BY HD.FTShdPlantCode, HD.FDShdTransDate, RC.FTSrcGLCode)), LEN(ROW_NUMBER() OVER(ORDER BY HD.FTShdPlantCode, HD.FDShdTransDate, RC.FTSrcGLCode)), ROW_NUMBER() OVER(ORDER BY HD.FTShdPlantCode, HD.FDShdTransDate, RC.FTSrcGLCode)) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"OperatorID\":\"' + HD.FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"\",' + CHAR(10) +");  //2018-08-29 NAUY
                oSQL.AppendLine("'\"BusinessDayDate\": \"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 121) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"TenderControlTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"TillSettle\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"@LedgerType\": \"EDCSettlement\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Sales\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ISNULL(");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("    ',{' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"' + SRC.FTTdmCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY2.FTSttGrpName = 'RETURN' THEN SRC.FCSrcNet * (-1) ELSE SRC.FCSrcNet END))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessUnit\": { \"UnitID\": \"' + ISNULL(SRC.FTShdPlantCode, '') + '\" }' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM TPSTSalHD SHD with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalRC SRC with(nolock) ON SHD.FTShdPlantCode = SRC.FTShdPlantCode AND SHD.FTTmnNum = SRC.FTTmnNum AND SHD.FTShdTransNo = SRC.FTShdTransNo AND SHD.FTSRVName = SRC.FTSRVName");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY2 with(nolock) ON SHD.FTShdTransType = TTY2.FTSttTranCode AND ISNULL(TTY2.FTSttGrpName, '') <> ''");

                oSQL.AppendLine("WHERE SHD.FTShdPlantCode = HD.FTShdPlantCode AND SHD.FDShdTransDate = HD.FDShdTransDate AND SRC.FTSrcGLCode = RC.FTSrcGLCode");
                oSQL.AppendLine("AND SRC.FTTdmCode IN('T002', 'T003', 'T024', 'T025', 'T026', 'T027', 'T028')");
                oSQL.AppendLine("GROUP BY SRC.FTShdPlantCode, SRC.FTTdmCode");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, '') ,'') +CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Ending\": {' + CHAR(10) +");
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), 0)) + '\",' + CHAR(10) +"); //Defualt 0 ไปก่อน รอ Phase ถัดไป
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + ");
                oSQL.AppendLine(" CONVERT(VARCHAR(50),ISNULL((SELECT CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY2.FTSttGrpName = 'RETURN' THEN SRC.FCSrcNet * (-1) ELSE SRC.FCSrcNet END)) ");
                oSQL.AppendLine(" FROM TPSTSalHD SHD with(nolock) ");
                oSQL.AppendLine(" INNER JOIN TPSTSalRC SRC with(nolock) ON SHD.FTShdPlantCode = SRC.FTShdPlantCode AND SHD.FTTmnNum = SRC.FTTmnNum AND SHD.FTShdTransNo = SRC.FTShdTransNo  AND SHD.FTSRVName = SRC.FTSRVName");
                oSQL.AppendLine(" INNER JOIN " + tPosLnkDB + "TSysTransType TTY2 with(nolock) ON SHD.FTShdTransType = TTY2.FTSttTranCode AND ISNULL(TTY2.FTSttGrpName, '') <> '' ");

                oSQL.AppendLine(" WHERE  SHD.FTShdPlantCode = HD.FTShdPlantCode AND SHD.FDShdTransDate = HD.FDShdTransDate  AND SRC.FTSrcGLCode = RC.FTSrcGLCode ");
                oSQL.AppendLine(" AND SRC.FTTdmCode IN('T002', 'T003', 'T024', 'T025', 'T026', 'T027', 'T028')),CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), 0)) ))  ");
                oSQL.AppendLine("+    '\", ' + CHAR(10) +"); //2018-08-29  NAUY ให้ SUM จาก RC
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"GLAccount\": \"' + RC.FTSrcGLCode + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '} ' + CHAR(10) +");
                //##2018-08-27 Nauy
                //ยังไม่มี Short/Over ไม่ต้องส่ง
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Short\": { \"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), 0)) + '\" }' + CHAR(10) +"); //Defualt 0 ไปก่อน รอ Phase ถัดไป
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("'}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalRC RC with(nolock) ON HD.FTShdPlantCode = RC.FTShdPlantCode AND HD.FTTmnNum = RC.FTTmnNum AND HD.FTShdTransNo = RC.FTShdTransNo  AND HD.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");
                oSQL.AppendLine("WHERE RC.FTTdmCode IN('T002','T003','T024','T025','T026','T027','T028')");

                //if (ptLastUpd != "")
                //{
                //    oSQL.AppendLine("AND CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "' AND HD.FDShdTransDate='" + tC_DateTrn + "'");//*Em 61-08-04
                //}
                //else
                //{
                //    oSQL.AppendLine("AND HD.FDShdTransDate='" + tC_DateTrn + "'  "); ;//*Em 61-08-04
                //}

                if (ptMode == "AUTO")
                {
                    //if (ptLastUpd != "")
                    //{
                    //oSQL.AppendLine("WHERE CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "' AND HD.FCShdGrand > 0");     //*Em 61-08-06
                    //}

                    oSQL.AppendLine("AND ISNULL(HD.FTStaSentOnOff, '0') <> '1' AND HD.FCShdGrand > 0");
                }
                else if (ptMode == "MANUAL")
                {
                    //if (ptLastUpd != "")
                    //{
                    //oSQL.AppendLine("WHERE CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "' AND HD.FCShdGrand > 0");
                    oSQL.AppendLine("AND HD.FDShdTransDate = '" + tC_DateTrn + "' ");   //*Em 61-08-22//*Em 61-08-06

                    if (tC_Plant.Trim() != "")
                    {
                        oSQL.AppendLine("AND HD.FTShdPlantCode IN (" + tC_Plant + ")");
                    }
                    //}
                }

                //oSQL.AppendLine("GROUP BY HD.FTShdPlantCode,HD.FDShdTransDate,HD.FTEmpCode,RC.FTRemark");
                oSQL.AppendLine("GROUP BY HD.FTShdPlantCode,HD.FDShdTransDate,RC.FTSrcGLCode"); //2018-08-29 NAUY
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,''),'') +']'");
                oSQL.AppendLine("FOR XML PATH('')");


                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception oEx)
            {
                return "";
            }
            finally
            {
                oSQL = null;
                rtResult = null;
            }
        }
    }
}
