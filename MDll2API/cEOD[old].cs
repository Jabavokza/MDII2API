using MDll2API.Class.Standard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace MDll2API
{
    public class cEOD
    {
        private string tC_DateTrn = "";
        private string tC_PlantEOD = "";
        private string tC_Auto = "";

        public string C_POSTtEOD(string ptDEOD,  string ptPlantEOD,string ptAuto)
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
            string tFileName = "";
            StringBuilder oSql;
            cSP oSP = new cSP();
            string tConnDB = "";
            string tFunction = "5";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            string tWorkStationID = ""; //*Em 61-07-23
            string tWorkStation = ""; //*Em 61-07-24
            tC_DateTrn = ptDEOD;
            tC_PlantEOD = ptPlantEOD;
            tC_Auto = ptAuto;
            try
            {
                dStart = DateTime.Now;
                // load Config
                oTblConfig = oSP.SP_GEToConnDB();

                // Sort  Group Function
                oRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");
          
                for (int nRow = 0; nRow < oRow.Length; nRow++)
                    {
                        tUriApi = oRow[nRow]["UrlApi"].ToString();
                        tUsrApi = oRow[nRow]["UsrApi"].ToString();
                        tPwdApi = oRow[nRow]["PwdApi"].ToString();
                        tWorkStationID = oRow[nRow]["WorkStationID"].ToString(); //*Em 61-07-23
                        tWorkStation = oRow[nRow]["WorkStation"].ToString(); //*Em 61-07-23

                        // Create Connection String Db
                        tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                        tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                        tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();

                        // Check TPOSLogHis  Existing
                        tSQL = oSP.SP_GETtCHKDBLogHis();
                        oSP.SP_SQLxExecute(tSQL, tConnDB);

                        // Get Max FTBathNo Condition To Json
                        tLastUpd = "";
                        tLastUpd = oSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                        //  Condition ตาม FTBatchNo Get Json
                        //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                        //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID);  //*Em 61-07-23
                        tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID,tWorkStation);  //*Em 61-07-24

                        tExecute = oSP.SP_SQLxExecuteJson(tSQL, tConnDB);
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
                    if (tJsonTrn != "")
                    {
                        tJson = "{" + Environment.NewLine;
                        tJson = tJson + "\"POSLog\": {" + Environment.NewLine;
                        //*Em 61-07-20
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
                        tFileName = oSP.SP_WRItJSON(tJson, "EOD");

                        //Call API
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
                        dEnd = DateTime.Now;
                        for (int nRow = 0; nRow < oRow.Length; nRow++)
                        {
                            // Create Connection String Db
                            tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                            tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                            tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();

                            // Get Max FTBathNo Condition To Json
                            tLastUpd = "";
                            tLastUpd = oSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);
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
                            if (tLastUpd != "")
                            {
                                oSql.AppendLine("    WHERE CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '') > '" + tLastUpd + "'");
                            }
                            oSql.AppendLine("    ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                            oSP.SP_SQLxExecute(oSql.ToString(), tConnDB);

                        }
                    }

                string tResultGetdata = "";
                if (tJsonTrn == "")
                {
                    tResultGetdata = "Get Database ไม่ได้";
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
                oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }
        public string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100,string ptWorkStationID = "",string ptWorkStation = "")
        {
            StringBuilder oSQL = new StringBuilder();
            string rtResult = "";
            string tPosLnkDB = "";
            string tPosCntDB = "";
     
            cSP oSP = new cSP();
           
            DataTable oTblPosCenter;

            try
            {
                //2018-08-28 Add config POS Center เพื่อใช้ดึงข้อมูล Master
                //load poscenter config
                oTblPosCenter = oSP.SP_GEToPosCnt();
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
                oSQL.AppendLine("SELECT ',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode,'') + '\"},' + CHAR(10) +");
                //oSQL.AppendLine("'\"WorkstationID\":\"' + FTTmnNum + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"WorkstationID\":\"' + '" + ptWorkStationID  + "' + '\",' + CHAR(10) +");      //*Em 61-07-23  
               // oSQL.AppendLine("'\"WorkstationID\":\"' + '" + ptWorkStation + "' + '\",' + CHAR(10) +");      //*Em 61-07-23 
                oSQL.AppendLine("'\"WorkstationID\":\"' + HD.FTTmnNum + '\",' + CHAR(10) +");      //*Em 61-07-23 
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10),HD.FDShdTransDate,112) + '"+ ptWorkStationID  +"' + STUFF('00000', 6-LEN(ROW_NUMBER() OVER(ORDER BY FTShdPlantCode,FTTmnNum,FDShdTransDate,FTEmpCode )) , LEN(ROW_NUMBER() OVER(ORDER BY FTShdPlantCode,FTTmnNum,FDShdTransDate,FTEmpCode )), ROW_NUMBER() OVER(ORDER BY FTShdPlantCode,FTTmnNum,FDShdTransDate,FTEmpCode )) + '\",' + CHAR(10) +");     //*Em 61-07-23
                oSQL.AppendLine("'\"OperatorID\":\"' + HD.FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"' + CONVERT(VARCHAR(10), FDShdTransDate, 121) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"BeginDateTime\": \"' + CONVERT(VARCHAR, (FDShdTransDate + MIN(FTShdSysTime)), 127) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"EndDateTime\": \"' + CONVERT(VARCHAR, (FDShdTransDate + MAX(FTShdSysTime)), 127) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"BeginDateTime\": \"' + CONVERT(VARCHAR,FDShdTransDate,112) + REPLACE(MIN(FTShdSysTime),':','') + '\",' + CHAR(10) +");  //*Em 61-07-23  
                oSQL.AppendLine("'\"BeginDateTime\": \"' + CONVERT(VARCHAR,FDShdTransDate,112) + REPLACE(MIN(HD.FTTimeIns),':','') + '\",' + CHAR(10) +");  //*Em 61-07-23   NAUY 2018-09-06 change time
//                oSQL.AppendLine("'\"EndDateTime\": \"' + CONVERT(VARCHAR,FDShdTransDate,112) + REPLACE(MAX(FTShdSysTime),':','') + '\",' + CHAR(10) +");    //*Em 61-07-23  
                oSQL.AppendLine("'\"EndDateTime\": \"' + CONVERT(VARCHAR,FDShdTransDate,112) + REPLACE(MAX(HD.FTTimeUpd),':','') + '\",' + CHAR(10) +");    //*Em 61-07-23   NAUY 2018-09-06 change time
                oSQL.AppendLine("'\"TenderControlTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + '\"OperatorID\": \"' + FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + '\"TillSettle\": [' + CHAR(10) +");
                oSQL.AppendLine("   ISNULL(");
                oSQL.AppendLine("   STUFF((");
                oSQL.AppendLine("   SELECT");
                oSQL.AppendLine("   ',{' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + FTData + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("   FROM(");

                oSQL.AppendLine("   SELECT ");
                oSQL.AppendLine("		 '1' AS FTType,");
                oSQL.AppendLine("           ('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + HD3.FTShdTransType + '\",' + CHAR(10) +" );

                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD3.FTShdTransNo))) + '\",\' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(DT.FCSdtSaleAmt))) + '\"'");

                oSQL.AppendLine("           ) AS FTData");
                oSQL.AppendLine("           FROM TPSTSalHD HD3 with(nolock)");
                oSQL.AppendLine("           INNER JOIN TPSTSalDT DT with(nolock) ");
                oSQL.AppendLine("           ON HD3.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("           AND HD3.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("           AND HD3.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("		   INNER JOIN TPSTSalRC RC3");
                oSQL.AppendLine("		   ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("           AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("           AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("           WHERE HD3.FTShdTransType IN('03','06' ,'07', '10', '11', '13')");
                oSQL.AppendLine("		   AND RC3.FTTdmCode <> 'T008'");
                oSQL.AppendLine("		   AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("           AND HD3.FTShdPlantCode = HD.FTShdPlantCode");
               // oSQL.AppendLine("           AND DT.FCSdtTax > 0");
                oSQL.AppendLine("           AND DT.FCSdtQty > 0");
                oSQL.AppendLine("           GROUP BY HD3.FTEmpCode,HD3.FTShdTransType");
                oSQL.AppendLine("		  UNION ALL");
                oSQL.AppendLine("		   SELECT  ");
                oSQL.AppendLine("		 '1' AS FTType,");
                oSQL.AppendLine("           ('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"@ItemType\": \"45\",' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD3.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(DT.FCSdtSaleAmt))) + '\"'");
                oSQL.AppendLine("           ) AS FTData");
                oSQL.AppendLine("           FROM TPSTSalHD HD3 with(nolock)");
                oSQL.AppendLine("           INNER JOIN TPSTSalDT DT with(nolock) ");
                oSQL.AppendLine("           ON HD3.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("           AND HD3.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("           AND HD3.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("		   INNER JOIN TPSTSalRC RC3");
                oSQL.AppendLine("		   ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("           AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("           AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("           WHERE HD3.FTShdTransType IN('03','06', '07', '10', '11', '13')");
                oSQL.AppendLine("		   AND RC3.FTTdmCode = 'T008'");
                oSQL.AppendLine("		   AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("           AND HD3.FTShdPlantCode = HD.FTShdPlantCode");
               // oSQL.AppendLine("           AND DT.FCSdtTax > 0");
                oSQL.AppendLine("           AND DT.FCSdtQty > 0");
                oSQL.AppendLine("           GROUP BY HD3.FTEmpCode,HD3.FTShdTransType");


                oSQL.AppendLine("    UNION ALL");
                oSQL.AppendLine("    SELECT");
                oSQL.AppendLine("      '2' AS FTType,");
                oSQL.AppendLine("     ('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'IPV' + '\",' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + FTShdTransType + '\",' + CHAR(10) +");  //*Em 61-07-23  
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(FTShdTransNo))) + '\",' + CHAR(10) +");  //*Em 61-08-06
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": {' + CHAR(10) +");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"Subtract\",' + CHAR(10) +");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(FCShdGrand))) + '\"' + CHAR(10) +");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(FCShdGrand)*(-1))) + '\"'");       //*Em 61-07-24
                oSQL.AppendLine("     ) AS FTData");
                oSQL.AppendLine("      FROM TPSTSalHD with(nolock)");
                oSQL.AppendLine("     WHERE FTShdTransType IN('05','16', '17', '26', '27', '28')");
                oSQL.AppendLine("     AND FTTmnNum = HD.FTTmnNum AND FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("     AND TPSTSalHD.FTShdPlantCode=HD.FTShdPlantCode"); //NAUY 2018-09-06
                //oSQL.AppendLine("     GROUP BY FTEmpCode");
                oSQL.AppendLine("     GROUP BY FTEmpCode,FTShdTransType");  //*Em 61-07-23 
                oSQL.AppendLine(" UNION ALL");
                oSQL.AppendLine("SELECT '3' AS FTType,");
                oSQL.AppendLine("   ('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + DT.FTShdTransType + '\",' + CHAR(10) +");    //*Em 61-07-23 
                
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + ");
                //oSQL.AppendLine("       CASE WHEN (SELECT '44' AS  FTdmCode FROM TPSTSalRC WHERE FTTdmCode='T008' AND FTShdTransType=FTShdTransType AND FTShdTransNo =FTShdTransNo AND FTTmnNum =FTTmnNum AND FTSRVName=FTSRVName AND FDShdTransDate =FDShdTransDate  ) = '44'  THEN");
                //oSQL.AppendLine("       '44' ");
                //oSQL.AppendLine("       ELSE TPSTSalHD.FTShdTransType");
                //oSQL.AppendLine("       END");
                //oSQL.AppendLine("+ '\", ' + CHAR(10) +");

                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(DT.FTShdTransNo))) + '\",' + CHAR(10) +");   //*Em 61-08-06
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2),  SUM(DT.FCSdtSaleAmt) * (-1))) + '\"'");       //*Em 61-07-24
                oSQL.AppendLine("    ) AS FTData");
                oSQL.AppendLine("   FROM TPSTSalHD HD4 with(nolock)");
                oSQL.AppendLine("   INNER JOIN TPSTSalDT DT with(nolock) ");
                oSQL.AppendLine("   ON HD4.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("   AND HD4.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("   AND HD4.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("   WHERE HD4.FTShdTransType = '04'");
                oSQL.AppendLine("   AND DT.FTTmnNum = HD.FTTmnNum AND DT.FDShdTransDate = HD.FDShdTransDate AND HD4.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("     AND HD4.FTShdPlantCode=HD.FTShdPlantCode"  ); //NAUY 2018-09-06
                //oSQL.AppendLine("    GROUP BY FTEmpCode");
                oSQL.AppendLine("    GROUP BY HD4.FTEmpCode,DT.FTShdTransType");   //*Em 61-07-23 
                oSQL.AppendLine(" UNION ALL");
                oSQL.AppendLine(" SELECT '4' AS FTType,");
                oSQL.AppendLine("    ('\"@TransType\": \"' + 'Discount' + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + CONVERT(VARCHAR, TPSTSalCD.FNDctNo) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + CONVERT(VARCHAR, CD.FNDctNo) + '\",' + CHAR(10) +");     //*Em 61-08-02
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + CONVERT(VARCHAR, CD.FTScdBBYProfID) + '\",' + CHAR(10) +");     //*Em 61-08-14
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"MiscellaneousDiscounts\": {' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Count\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(TPSTSalCD.FNDctNo))) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"Subtract\",' + CHAR(10) +");
                ////oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(TPSTSalCD.FCScdAmt))) + '\"' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(TPSTSalCD.FCScdAmt)*(-1))) + '\"' + CHAR(10) +");   //*Em 61-07-24
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),SUM(TPSTSalCD.FCScdAmt)*(-1))) +'\",' + CHAR(10) +");    //*Em 61-07-24
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Count\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),COUNT(TPSTSalCD.FNDctNo))) +'\"' + CHAR(10) +");          //*Em 61-07-24
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),SUM(CD.FCScdAmt)*(-1))) +'\",' + CHAR(10) +");    //*Em 61-08-02
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Count\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),COUNT(CD.FNDctNo))) +'\"' + CHAR(10) +");          //*Em 61-08-02
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"Count\": \"'+ CONVERT(VARCHAR,CONVERT(int,COUNT(CD.FNDctNo))) +'\"' + CHAR(10) +");   //*Em 61-08-06
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("    ) AS FTData");
                //oSQL.AppendLine("    FROM TPSTSalHD with(nolock)");
                //oSQL.AppendLine("    INNER JOIN TPSTSalCD  with(nolock) ON TPSTSalHD.FTTmnNum = TPSTSalCD.FTTmnNum");
                //oSQL.AppendLine("    AND TPSTSalHD.FTShdTransNo = TPSTSalCD.FTShdTransNo");
                //oSQL.AppendLine("    AND TPSTSalHD.FTShdTransType = TPSTSalCD.FTShdTransType");
                //oSQL.AppendLine("    AND TPSTSalHD.FDShdTransDate = TPSTSalCD.FDShdTransDate");
                ////oSQL.AppendLine("    AND(TPSTSalCD.FNDctNo > 3 AND TPSTSalCD.FNDctNo <> 21)");
                //oSQL.AppendLine("    WHERE TPSTSalHD.FTShdTransType IN('03', '07', '10', '11', '13')");
                //oSQL.AppendLine("    AND TPSTSalHD.FTTmnNum = HD.FTTmnNum AND TPSTSalHD.FDShdTransDate = HD.FDShdTransDate AND TPSTSalHD.FTEmpCode = HD.FTEmpCode");
                //oSQL.AppendLine("    GROUP BY TPSTSalHD.FTEmpCode, TPSTSalCD.FNDctNo");
                //*Em 61-08-02
                oSQL.AppendLine("    FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("    INNER JOIN TPSTSalCD CD with(nolock) ON HD2.FTTmnNum = CD.FTTmnNum");
                oSQL.AppendLine("    AND HD2.FTShdTransNo = CD.FTShdTransNo");
                oSQL.AppendLine("    AND HD2.FTShdTransType = CD.FTShdTransType");
                oSQL.AppendLine("    AND HD2.FDShdTransDate = CD.FDShdTransDate");
                oSQL.AppendLine("    AND HD2.FTSRVName = CD.FTSRVName");
                oSQL.AppendLine("    WHERE HD2.FTShdTransType IN('03','06', '07', '10', '11', '13')");
                oSQL.AppendLine("    AND HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("     AND HD2.FTShdPlantCode=HD.FTShdPlantCode"); //NAUY 2018-09-06
                //oSQL.AppendLine("    GROUP BY HD2.FTEmpCode, CD.FNDctNo");
                oSQL.AppendLine("    GROUP BY HD2.FTEmpCode, CD.FTScdBBYProfID");   //*Em 61-08-14
                //++++++++++++++++
                oSQL.AppendLine(" UNION ALL");
                oSQL.AppendLine("  SELECT");
                oSQL.AppendLine("   '5' AS FTType,");
                oSQL.AppendLine("    ('\"@TransType\": \"' + 'Tax' + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'VAT' + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + FTShdTransType + '\",' + CHAR(10) +");   //*Em 61-07-23 
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(FTShdTransNo))) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(ISNULL(FCShdVat, 0)))) + '\"'");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'VAT' + '\",' + CHAR(10) +");    //*Em 61-08-02
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(HD2.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-02
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN ISNULL(HD2.FCShdVat,0)*(-1) ELSE ISNULL(HD2.FCShdVat,0) END))) + '\"'");   //*Em 61-08-02
                //oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(DT.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-03
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(DT.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-06
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN ISNULL(DT.FCSdtVat,0)*(-1) ELSE ISNULL(DT.FCSdtVat,0) END))) + '\"'");   //*Em 61-08-03
                oSQL.AppendLine("   ) AS FTData");
                //oSQL.AppendLine("   FROM TPSTSalHD with(nolock)");
                //oSQL.AppendLine("   WHERE FTShdTransType IN('03', '06', '07', '10', '11', '13', '15', '19', '14')");
                //oSQL.AppendLine("     AND FTTmnNum = HD.FTTmnNum AND FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                ////oSQL.AppendLine("    GROUP BY FTEmpCode");
                //oSQL.AppendLine("    GROUP BY FTEmpCode,FTShdTransType");   //*Em 61-07-23 
                //*Em 61-08-02
                oSQL.AppendLine("   FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("   INNER JOIN TPSTSalDT DT with(nolock) ON HD2.FTTmnNum = DT.FTTmnNum AND HD2.FTShdTransNo = DT.FTShdTransNo  AND HD2.FTSRVName = DT.FTSRVName  ");    //*Em 61-08-03
                oSQL.AppendLine("   INNER JOIN "+ tPosLnkDB + "TSysTransType TTY ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");
                oSQL.AppendLine("   WHERE HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("   AND HD2.FTShdPlantCode=HD.FTShdPlantCode"); //NAUY 2018-09-06
               // oSQL.AppendLine("   AND DT.FCSdtTax > 0");  //*Em 61-08-03
                oSQL.AppendLine("   AND DT.FCSdtQty > 0");
                oSQL.AppendLine("   GROUP BY HD2.FTEmpCode");
                //++++++++++++++++++++
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '6' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Tender' + '\",' + CHAR(10) +");
                //oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + TPSTSalRC.FTTdmCode + '\",' + CHAR(10) +");
                //oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(TPSTSalRC.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + RC.FTTdmCode + '\",' + CHAR(10) +");    //*Em 61-08-02
                //oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), COUNT(RC.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-02
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(RC.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-06
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + '\"Ending\": {' + CHAR(10) +");
                //oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(TPSTSalRC.FCSrcAmt))) + '\"' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN RC.FCSrcNet*(-1) ELSE RC.FCSrcNet END))) + '\"' + CHAR(10) +");    //*Em 61-08-02
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine(") AS FTData");
                //oSQL.AppendLine(" FROM TPSTSalHD with(nolock)");
                //oSQL.AppendLine(" INNER JOIN TPSTSalRC  with(nolock) ON TPSTSalHD.FTTmnNum = TPSTSalRC.FTTmnNum");
                //oSQL.AppendLine(" AND TPSTSalHD.FTShdTransNo = TPSTSalRC.FTShdTransNo");
                //oSQL.AppendLine(" AND TPSTSalHD.FTShdTransType = TPSTSalRC.FTShdTransType");
                //oSQL.AppendLine(" AND TPSTSalHD.FDShdTransDate = TPSTSalRC.FDShdTransDate");
                //oSQL.AppendLine(" WHERE TPSTSalHD.FTShdTransType IN('03', '06', '07', '10', '11', '13', '15', '19', '22', '24', '14')");
                //oSQL.AppendLine(" AND TPSTSalHD.FTTmnNum = HD.FTTmnNum AND TPSTSalHD.FDShdTransDate = HD.FDShdTransDate AND TPSTSalHD.FTEmpCode = HD.FTEmpCode");
                //oSQL.AppendLine(" GROUP BY TPSTSalRC.FTTdmCode");
                //*Em 61-08-02
                oSQL.AppendLine(" FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine(" INNER JOIN TPSTSalRC RC with(nolock) ON HD2.FTTmnNum = RC.FTTmnNum");
                oSQL.AppendLine(" AND HD2.FTShdTransNo = RC.FTShdTransNo");
                oSQL.AppendLine(" AND HD2.FTShdTransType = RC.FTShdTransType");
                oSQL.AppendLine(" AND HD2.FDShdTransDate = RC.FDShdTransDate");
                oSQL.AppendLine(" AND HD2.FTShdPlantCode = RC.FTShdPlantCode");
                oSQL.AppendLine(" AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine(" INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");
                oSQL.AppendLine(" AND HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine(" Where HD2.FTShdPlantCode=HD.FTShdPlantCode"); //NAUY 2018-09-06
                oSQL.AppendLine(" GROUP BY RC.FTTdmCode");
                //++++++++++++++
                //*Em 61-08-07  ++++++++++++++++++++++++++++++++++++++++++
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '7' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Tender' + '\",' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'T032' + '\",' + CHAR(10) +");    //*Em 61-08-02
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD2.FTShdTransNo))) + '\",' + CHAR(10) +");    //*Em 61-08-06
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + '\"Ending\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN HD2.FCShdRnd*(-1) ELSE HD2.FCShdRnd END)*(-1)))  + '\"' + CHAR(10) +");    //*Em 61-08-02
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");
                oSQL.AppendLine("WHERE ISNULL(FCShdRnd,0) <> 0 AND FTTmnNum = HD.FTTmnNum AND FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("   AND HD2.FTShdPlantCode=HD.FTShdPlantCode"); //NAUY 2018-09-06
                oSQL.AppendLine("GROUP BY FTEmpCode");
                //*Em 61-08-07  ++++++++++++++++++++++++++++++++++++++++++
                oSQL.AppendLine(" ) tmp");
                oSQL.AppendLine("WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine(" FOR XML PATH('')");
                oSQL.AppendLine(" ), 1, 1, '') ,'') +CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("'}'");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");  //*Em 61-08-06

                if (tC_Auto == "AUTO")
                {
                    if (ptLastUpd != "")
                    {
                        oSQL.AppendLine("WHERE CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "'  AND FDShdTransDate='" + tC_DateTrn + "'  ");
                    }
                    else
                    {
                        oSQL.AppendLine("WHERE FDShdTransDate='" + tC_DateTrn + "'   ");   //*Em 61-08-22
                    }
                }
                else if (tC_Auto == "MANUAL")
                {
                    if (ptLastUpd != "")
                    {
                        oSQL.AppendLine("WHERE CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "'  AND FDShdTransDate='" + tC_DateTrn + "' AND FTShdPlantCode='" + tC_PlantEOD + "' ");
                    }
                    else
                    {
                        oSQL.AppendLine("WHERE FDShdTransDate = '" + tC_DateTrn + "' AND FTShdPlantCode=  '" + tC_PlantEOD + "'  ");   //*Em 61-08-22
                    }
                }

                oSQL.AppendLine("GROUP BY FTEmpCode,FTTmnNum,FTShdPlantCode,FDShdTransDate");   //*Em 61-08-22
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
            finally
            {
                oSQL = null;
                rtResult = null;
            }
        }
    }
}
