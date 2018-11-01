using MDll2API.Class.ReceivApp;
using MDll2API.Class.Standard;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace MDll2API
{
    public class cRedeem
    {
        cRcvRedeem oC_RcvRedeem = new cRcvRedeem();
        private string tC_Auto = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public string C_POSTtRedeem(string ptDTrn, cRcvRedeem oRcvRedeem,string ptAuto)
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
            string tFunction = "2";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            string tWorkStationID = ""; //*Em 61-08-09 Com.Sheet ML-POSC-0032
            string tWorkStation = ""; //*Em 61-08-09 Com.Sheet ML-POSC-0032
            oC_RcvRedeem = oRcvRedeem;
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

                    tWorkStationID = oRow[nRow]["WorkStationID"].ToString();   //*Em 61-08-09 Com.Sheet ML-POSC-0031
                    tWorkStation = oRow[nRow]["WorkStation"].ToString();     //*Em 61-08-09 Com.Sheet ML-POSC-0032

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
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation);  //*Em 61-08-09 Com.Sheet ML-POSC-0032

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
                    tFileName = oSP.SP_WRItJSON(tJson, "REDEEM");

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
                        #endregion
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
                        oSql.AppendLine("'' AS FTRemark,'' AS FTShdPlantCode,'" + string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dStart) + "' AS FDSendStartDateTime,");
                        oSql.AppendLine("'" + string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dEnd) + "' AS FDSendEndDateTime,");
                        oSql.AppendLine("  MAX(CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '')) AS FTBatchNo,'" + tFunction + "' AS FTTransTypeGrp,");
                        oSql.AppendLine("'" + tResCode + "' AS FTRespCode, '" + tResMsg + "' AS FTRespMsg, COUNT(FTShdTransNo) AS FTTransCount");
                        oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(oRow[nRow]["TopRow"]) + " * FROM TPSTRPremium with(nolock)");
                        if (tLastUpd != "")
                        {
                            oSql.AppendLine(" WHERE CONVERT(varchar(8),TPSTRPremium.FDDateUpd,112) + REPLACE(TPSTRPremium.FTTimeUpd,':','') >= '" + tLastUpd + "'");
                        }
                        oSql.AppendLine(" ORDER BY FDDateUpd, FTTimeUpd) TTmp");
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
                    catch (Exception ex)
                    {
                        tPosLnkDB = "";
                    }
                }

                oSQL.AppendLine("SELECT '[' + ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP " + pnRowTop + " ',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + FTShdPlantCode + '\"},' + CHAR(10) +");
                //oSQL.AppendLine("'\"WorkstationID\":\"' + FTCompName + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"SequenceNumber\":\"' + FTRPDocNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' + '"+ ptWorkStation + "' + '\",' + CHAR(10) +");    //*Em 61-08-09 Com.Sheet ML-POSC-0032
                oSQL.AppendLine("'\"SequenceNumber\":\"' + LEFT(FTRPDocNo,4) + '"+ ptWorkStationID +"' + RIGHT(FTRPDocNo,7) + '\",' + CHAR(10) +");    //*Em 61-08-09 Com.Sheet ML-POSC-0032
                oSQL.AppendLine("'\"OperatorID\":\"' + FTWhoIns + '\",' + CHAR(10) +");
                oSQL.AppendLine(" '\"CurrencyCode\":\"' + FTCurrencyCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"Reference\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"LocationID\":\"' + FTLocationID + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"LocationDescription\":\"' + FTLocationDescription + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"'+ CONVERT(VARCHAR(10),FDRPDocDate,121) +'\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"BeginDateTime\": \"'+ CONVERT(VARCHAR,FDRPDocDateMin,127) +'\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"EndDateTime\": \"'+ CONVERT(VARCHAR,FDRPDocDateMax,127) +'\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BeginDateTime\": \"'+ CONVERT(VARCHAR,FDRPDocDateMin,112) + REPLACE(CONVERT(VARCHAR,FDRPDocDateMin,108),':','') +'\",' + CHAR(10) +");    //*Em 61-08-04
                oSQL.AppendLine("'\"EndDateTime\": \"'+ CONVERT(VARCHAR,FDRPDocDateMax,112) + REPLACE(CONVERT(VARCHAR,FDRPDocDateMax,108),':','') +'\",' + CHAR(10) +");      //*Em 61-08-04
                oSQL.AppendLine("'\"InventoryControlTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"InventoryAdjustment\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine(" SELECT ',{' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + FTData + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '}'");
                oSQL.AppendLine(" FROM");
                oSQL.AppendLine("  (");
                oSQL.AppendLine("   SELECT '1' AS FTType,");
                //oSQL.AppendLine("   ('\"LineItem\": {' + CHAR(10) +");
                oSQL.AppendLine("   ('\"@DocumentType\":\"'+'PremiumGI'+'\",' + CHAR(10) +");    //*Em 61-08-14
                oSQL.AppendLine("   CHAR(9) + '\"LineItem\": {' + CHAR(10) +"); //*Em 61-08-14
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"@DocumentType\":\"' + 'PremiumGI' + '\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"ItemID\": {' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '\"@Qualifier\":\"' + 'EAN' + '\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '\"$\":\"' + PRM.FTPremiumID + '\"' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"SequenceNumber\":\"' + CONVERT(VARCHAR, PRM.FNSeqNo) + '\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"QuantityOrdered\": {' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '\"@UnitOfMeasureCode\":\"EA\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '\"$\":\"' + CONVERT(VARCHAR, PRM.FCQty) + '\"' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                //oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"IssuingPlant\":\"' + ISNULL(TTmpPremium.FTShdPlantCode,'') + '\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"IssuingPlant\":\"' + ISNULL((SELECT TOP 1 FTPrmStkPlnt FROM "+ tPosLnkDB +"TCNMBbySalArea WITH(NOLOCK) WHERE FTBBYNo = PRM.FTRPBBYNo),'') + '\",' + CHAR(10) +"); //*Nauy 61-08-24 (new design)  //*Em 61-08-21 Comm.Sheet ML-POSC-0038
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"TerminalPlant\":\"' + ISNULL(TTmpPremium.FTShdPlantCode,'') + '\",' + CHAR(10) +");
                //oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"BonusBuyID\":\"' + '' + '\"' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '\"BonusBuyID\":\"' + FTPremiumNo + '\"' + CHAR(10) +"); //*Em 61-08-04
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("   ) AS FTData");
                oSQL.AppendLine(" FROM TPSTRPremium PRM with(nolock)");
                oSQL.AppendLine("   WHERE PRM.FTRPDocNo = TTmpPremium.FTRPDocNo AND PRM.FTCompName = TTmpPremium.FTCompName");
                oSQL.AppendLine("    UNION");
                oSQL.AppendLine("     SELECT '1' AS FTType,");
                oSQL.AppendLine("    ('\"Reference\" : {' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"ReferenceTransaction\":\"' + CONVERT(VARCHAR(10), FDShdTransDate, 112) + FTTmnNum + FTShdTransNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"ReferenceTicketPlant\":\"' + ISNULL(FTShdPlantCode,'') + '\",' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '\"ReferenceBusinessDate\":\"' + CONVERT(VARCHAR(10), FDShdTransDate, 121) + '\"' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("    ) AS FTData");
                oSQL.AppendLine("    FROM TPSTRPPos with(nolock)");
                oSQL.AppendLine("    WHERE FTRPDocNo = TTmpPremium.FTRPDocNo AND FTCompName = TTmpPremium.FTCompName");
                oSQL.AppendLine("  )Tmp");
                oSQL.AppendLine("  WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine("  FOR XML PATH('')");
                oSQL.AppendLine(" ), 1, 1, '')");
                oSQL.AppendLine("), '') +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine(" '}' + CHAR(10)");
                oSQL.AppendLine(" FROM(");
                //oSQL.AppendLine(" SELECT DISTINCT ISNULL(Pos.FTRPPlantCode,'') AS FTShdPlantCode, FTCompName, Trn.FTRPDocNo, Trn.FTWhoIns");
                //oSQL.AppendLine("  , 'THB' AS FTCurrencyCode, ISNULL(Trn.FTTnmLocCode,'') AS FTLocationID, ISNULL(Trn.FTTnmLocDesc,'') AS FTLocationDescription");
                oSQL.AppendLine(" SELECT DISTINCT ISNULL(Trn.FTRPPlantCode,'') AS FTShdPlantCode, FTCompName, Trn.FTRPDocNo, Trn.FTWhoIns");    //*Em 61-08-04  
                oSQL.AppendLine("  , 'THB' AS FTCurrencyCode, ISNULL(Trn.FTCompName,'') AS FTLocationID, ISNULL((SELECT TOP 1 FTRemark FROM " + tPosLnkDB + "TCNMTerml WHERE FTCompName =Trn.FTCompName),'') AS FTLocationDescription");  //*Em 61-08-04  
                oSQL.AppendLine(" , Trn.FDRPDocDate");
                oSQL.AppendLine(" , Trn.FDRPDocDate + Min(Trn.FTTimeIns) AS FDRPDocDateMin");
                oSQL.AppendLine(" , Trn.FDRPDocDate + Max(Trn.FTTimeIns) AS FDRPDocDateMax");
                oSQL.AppendLine(" FROM " + tPosLnkDB + "TPSTRPremium Trn with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTRPPos Pos ON Trn.FTRPDocNo = Pos.FTRPDocNo");

                if (tC_Auto == "AUTO")
                {
                    oSQL.AppendLine("WHERE ISNULL(FTStaSentOnOff,'') ='' ");
                }

                else if (tC_Auto == "MANUAL")
                {
                    //if (ptLastUpd != "")
                    //{
                    //    oSQL.AppendLine("WHERE CONVERT(varchar(8),Trn.FDDateUpd,112) + REPLACE(Trn.FTTimeUpd,':','') >= '" + ptLastUpd + "' AND " + oC_RcvRedeem.Field + oC_RcvRedeem.Value + " ");
                    //}

                    if (!(oC_RcvRedeem.Value.ToString() == ")"))
                    {
                        oSQL.AppendLine("WHERE  " + oC_RcvRedeem.Field + oC_RcvRedeem.Value + " ");
                    }
                }

                oSQL.AppendLine(" GROUP BY FTCompName,Trn.FDRPDocDate,Trn.FTRPDocNo,Trn.FTRPPlantCode,Trn.FTWhoIns");     //*Em 61-08-04
                oSQL.AppendLine(" ) AS TTmpPremium");
                oSQL.AppendLine(" ORDER BY FDRPDocDate,FTRPDocNo"); //*Em 61-08-04
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine(" ),1,1,''),'') +']'");
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
