using MDll2API.Class.Standard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using MDll2API.Class;
using System.IO;

namespace MDll2API.Class.POSLog
{
    public class cCash
    {
        private  string tC_DateTrn = "";
        private string tC_Auto = "";
        private string tC_PlantCash = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public string C_POSTtCash(string ptDTrn ,string ptMode ,string[] patPlantCash)
        {
            //=====================TEST ===========
            //try
            //{
            //    cPOSCash oPOSCash = new cPOSCash();
            //    string tPathLocal = "E:\\Sht.json";
            //    oPOSCash = JsonConvert.DeserializeObject<cPOSCash>(File.ReadAllText(tPathLocal));
            //    // string tJson1 = JsonConvert.SerializeObject(oPOSCash,Formatting.Indented);

            //    //var t1 = (from s in oPOSCash.POSLog.Transaction
            //    //          select s).ToList();

              
            //    //string tJson1 = JsonConvert.SerializeObject(oPOSCash, Formatting.Indented, new JsonSerializerSettings
            //    //{
            //    //    NullValueHandling = NullValueHandling.Ignore
            //    //});
            //}
            //catch (Exception ex)
            //{
            //}
            //=====================TEST ===========

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
            string tFileName = "";
            StringBuilder oSql;
            cSP oSP = new cSP();
            string tConnDB = "";
            string tFunction = "4";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04
            try
            {
                dStart = DateTime.Now;
                // load Config
                oTblConfig = oSP.SP_GEToConnDB();

                //tC_PlantEOD = ptPlantCash;
                if (!(patPlantCash == null))
                {
                    for (int nLoop = 0; nLoop < patPlantCash.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            tC_PlantCash += "'" + patPlantCash[nLoop] + "'";
                        }
                        else
                        {
                            tC_PlantCash += ", '" + patPlantCash[nLoop] + "'";
                        }
                    }
                }

                tC_Auto = ptMode;
                tC_DateTrn = ptDTrn;

                // Sort  Group Function
                oRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");
         
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
                    tSQL = oSP.SP_GETtCHKDBLogHis();
                    oSP.SP_SQLxExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                    tLastUpd = "";
                    tLastUpd = oSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation);  //*Em 61-07-24

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
                    if (tJsonTrn=="[]") { tJsonTrn = ""; }
                }
                

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
                    tFileName = oSP.SP_WRItJSON(tJson, "CASH");

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
                        oSql.AppendLine("    WHERE FTShdTransType IN ('03','04','05','06','10','11','14','15','21','22','23','26','27')");
                        if (tLastUpd != "")
                        {
                            oSql.AppendLine("    AND CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '') > '" + tLastUpd + "'");
                        }
                        oSql.AppendLine("    ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                        oSP.SP_SQLxExecute(oSql.ToString(), tConnDB);

                    }
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
                oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100, string ptWorkStationID = "", string ptWorkStation = "")
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
                    catch (Exception oEx)
                    {
                        tPosLnkDB = "";
                    }
                }
                //tC_DateTrn
                oSQL.AppendLine("SELECT '[' + ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP 30  ',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode, '') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' + '" + ptWorkStation + "' + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + '" + ptWorkStationID + "' + STUFF('00000', 6 - LEN(ROW_NUMBER() OVER(ORDER BY RC2.FTTdmCode)), LEN(ROW_NUMBER() OVER(ORDER BY RC2.FTTdmCode)), ROW_NUMBER() OVER(ORDER BY RC2.FTTdmCode)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"' + CONVERT(VARCHAR(10), GETDATE(), 121) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"TenderControlTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"TillSettle\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + ISNULL(");
                oSQL.AppendLine("STUFF((");

                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("    ',{' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"TenderSummary\" : {' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '\"@LedgerType\" : \"' + 'ShortOver' + '\",' + CHAR(10) +");
                oSQL.AppendLine("    (CASE WHEN(ISNULL(Tdr.FCSrcNet, 0) - ISNULL(Trn.FCSrcNet, 0)) > 0 THEN");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + '\"Over\" : {' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\":\"' + CASE WHEN ISNULL(Trn.FTTdmCode, Tdr.FTTdmCode) = 'T009' THEN 'T030' ELSE ISNULL(Trn.FTTdmCode, Tdr.FTTdmCode) END + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\":\"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (ISNULL(Tdr.FCSrcNet, 0) - ISNULL(Trn.FCSrcNet, 0)))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessDate\":\"' + CONVERT(VARCHAR(10), ISNULL(Trn.FDShdTransDate, Tdr.FDShdTransDate), 121) + '\"' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("    ELSE");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + '\"Short\" : {' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\":\"' + CASE WHEN ISNULL(Trn.FTTdmCode, Tdr.FTTdmCode) = 'T009' THEN 'T030' ELSE ISNULL(Trn.FTTdmCode, Tdr.FTTdmCode) END + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\":\"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (ISNULL(Tdr.FCSrcNet, 0) - ISNULL(Trn.FCSrcNet, 0)) * CONVERT(DECIMAL(16, 2),(-1)))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessDate\":\"' + CONVERT(VARCHAR(10), ISNULL(Trn.FDShdTransDate, Tdr.FDShdTransDate), 121) + '\"' + CHAR(10) +");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("    END) + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("    (SELECT HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName, SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN RC.FCSrcNet * (-1) ELSE RC.FCSrcNet END) AS FCSrcNet");
                oSQL.AppendLine("    FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("    INNER JOIN TPSTSalRC RC with(nolock) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("    INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName, '') <> ''");
                oSQL.AppendLine("    WHERE HD2.FTShdPlantCode = HD.FTShdPlantCode AND HD2.FDShdTransDate = HD.FDShdTransDate AND RC.FTTdmCode = RC2.FTTdmCode AND HD2.FTSRVName = RC.FTSRVName AND   HD2.FDShdTransDate = '" + tC_DateTrn + "'");
                oSQL.AppendLine("    GROUP BY HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName) Trn");
                oSQL.AppendLine("    FULL OUTER JOIN");
                oSQL.AppendLine("    (SELECT HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, SUM(RC.FCSrcNet) AS FCSrcNet");
                oSQL.AppendLine("    FROM TPSTSalHD HD2 WITH(NOLOCK)");
                oSQL.AppendLine("    INNER JOIN TPSTSalRC RC WITH(NOLOCK) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("    WHERE HD2.FTShdPlantCode = HD.FTShdPlantCode AND HD2.FDShdTransDate = HD.FDShdTransDate AND RC.FTTdmCode = RC2.FTTdmCode");
                oSQL.AppendLine("    AND HD2.FTShdTransType = '45' AND  HD2.FDShdTransDate = '" + tC_DateTrn + "' AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("    GROUP BY HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode) Tdr");
                oSQL.AppendLine("    ON Trn.FTShdPlantCode = Tdr.FTShdPlantCode AND Trn.FDShdTransDate = Tdr.FDShdTransDate");
                oSQL.AppendLine("    AND Trn.FTTdmCode = Tdr.FTTdmCode");
                oSQL.AppendLine("    WHERE(ISNULL(Trn.FCSrcNet, 0) - ISNULL(Tdr.FCSrcNet, 0)) <> 0");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, ''), '') + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");

                if (tC_Auto == "AUTO")
                {
                    oSQL.AppendLine("INNER JOIN TCNMPlnCloseSta with(nolock) ON HD.FDShdTransDate = TCNMPlnCloseSta.FDSaleDate AND HD.FTShdPlantCode = TCNMPlnCloseSta.FTPlantCode AND ISNULL(TCNMPlnCloseSta.FTStaShortOver, '0') = '0'");
                }

                oSQL.AppendLine(" INNER JOIN "+ tPosLnkDB + "TSysTransType TTY with(nolock) ON HD.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");
                oSQL.AppendLine(" INNER JOIN TPSTSalRC RC2  ON HD.FTTmnNum = RC2.FTTmnNum AND HD.FTShdTransNo = RC2.FTShdTransNo AND HD.FTShdTransType = RC2.FTShdTransType");
                oSQL.AppendLine(" INNER JOIN");
                oSQL.AppendLine(" (SELECT Trn.FTShdPlantCode, Trn.FDShdTransDate, Trn.FTTdmCode, Trn.FTSRVName, ISNULL(Trn.FCSrcNet,0) -ISNULL(Tdr.FCSrcNet, 0) as FCSrcNet from");
                oSQL.AppendLine("      (SELECT HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName, SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN RC.FCSrcNet * (-1) ELSE RC.FCSrcNet END) AS FCSrcNet");
                oSQL.AppendLine("      FROM TPSTSalHD HD2");
                oSQL.AppendLine("      INNER JOIN "+ tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName, '') <> ''");
                oSQL.AppendLine("      INNER JOIN TPSTSalRC RC with(nolock) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("      where  HD2.FDShdTransDate = '" + tC_DateTrn + "'");
                oSQL.AppendLine("      GROUP BY HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName) Trn");
                oSQL.AppendLine("        FULL OUTER JOIN");
                oSQL.AppendLine("        (SELECT HD2.FTShdPlantCode , HD2.FDShdTransDate , RC.FTTdmCode , SUM(RC.FCSrcNet) AS FCSrcNet");
                oSQL.AppendLine("        FROM TPSTSalHD HD2 WITH(NOLOCK)");
                oSQL.AppendLine("        INNER JOIN TPSTSalRC RC WITH(NOLOCK) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName= RC.FTSRVName");
                oSQL.AppendLine("        WHERE HD2.FTShdTransType = '45' and HD2.FDShdTransDate = '" + tC_DateTrn + "'");
                oSQL.AppendLine("        GROUP BY HD2.FTShdPlantCode , HD2.FDShdTransDate , RC.FTTdmCode) Tdr");
                oSQL.AppendLine("          ON Trn.FTShdPlantCode = Tdr.FTShdPlantCode AND Trn.FDShdTransDate = Tdr.FDShdTransDate");
                oSQL.AppendLine("    AND Trn.FTTdmCode = Tdr.FTTdmCode");
                oSQL.AppendLine("    WHERE(ISNULL(Trn.FCSrcNet, 0) - ISNULL(Tdr.FCSrcNet, 0)) <> 0  ) RC3 on Hd.FTShdPlantCode = RC3.FTShdPlantCode and Hd.FDShdTransDate = RC3.FDShdTransDate and RC2.FTTdmCode = RC3.FTTdmCode AND HD.FTSRVName = RC3.FTSRVName");
                
                if (tC_Auto == "AUTO")
                {
                    oSQL.AppendLine("WHERE RC3.FCSrcNet <> 0");
                    //oSQL.AppendLine("WHERE HD.FDShdTransDate = '" + tC_DateTrn + "'  and RC3.FCSrcNet <> 0");
                    oSQL.AppendLine("AND HD.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta where FDSaleDate = '" + tC_DateTrn + "' AND ISNULL(FTStaEOD, '0') = '1' AND ISNULL(FTStaShortOver, '0') = '0')");

                }
                else if (tC_Auto == "MANUAL")
                {
                    oSQL.AppendLine("WHERE HD.FDShdTransDate = '" + tC_DateTrn + "'  and RC3.FCSrcNet <> 0");
                    oSQL.AppendLine("AND HD.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta where FDSaleDate = '" + tC_DateTrn + "' AND ISNULL(FTStaEOD, '0') = '1' AND ISNULL(FTStaShortOver, '0') = '0') AND HD.FTShdPlantCode IN (" + tC_PlantCash + ")");
                }

                oSQL.AppendLine("GROUP BY HD.FTShdPlantCode,HD.FDShdTransDate,RC2.FTTdmCode");
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
