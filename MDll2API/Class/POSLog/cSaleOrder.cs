using MDll2API.Class.Standard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cSaleOrder
    {
      //  public string C_POSTtSaleOrder(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual)
        public string C_POSTtSaleOrder(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual, string ptDTrn)
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
            cSP oSP = new cSP();
            string tConnDB = "";
            string tFunction = "7";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            try
            {
                dStart = DateTime.Now;
                // load Config
                oTblConfig = oSP.SP_GEToConnDB();

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
                        tSQL = oSP.SP_GETtCHKDBLogHis();
                        oSP.SP_SQLxExecute(tSQL, tConnDB);

                        // Get Max FTBathNo Condition To Json
                        tLastUpd = "";
                        tLastUpd = oSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                        //  Condition ตาม FTBatchNo Get Json
                        tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));

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
                            oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(oRow[nRow]["TopRow"]) + " * FROM TPSTSalVatHD HD with(nolock)");
                            oSql.AppendLine("    WHERE HD.FTXihDocType = '17'");

                            if (tLastUpd != "")
                            {
                                oSql.AppendLine("  AND CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '') > '" + tLastUpd + "'");
                            }

                            oSql.AppendLine("    ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                            oSP.SP_SQLxExecute(oSql.ToString(), tConnDB);

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
                oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100)
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

                oSQL.AppendLine("SELECT TOP " + pnRowTop + "");
                oSQL.AppendLine("'\"SalesOrder\" : {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"Header\" : [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + (CASE ''--ให้ส่งค่ามา");
                oSQL.AppendLine("WHEN '2' THEN '\"ACTION\": \"' + 'CHANGE' + '\",'");
                oSQL.AppendLine("WHEN '3' THEN '\"ACTION\": \"' + 'DELETE' + '\",'");
                oSQL.AppendLine("ELSE '\"ACTION\": \"' + 'CREATE' + '\",'");
                oSQL.AppendLine("END) + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TRANSTYPE\": \"' + 'ZCDO' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"DLVRY_DATE\": \"' + CONVERT(VARCHAR(8), HD.FDShdDepositDueDate, 112) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TENDER\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"TENDER_TYPE\": \"' + ISNULL(Map.FTLnmDefValue, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"AMOUNT\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), RC.FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"BONUSBUYID\": \"' + '' + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM TPSTSalVatRC RC with(nolock)");
                oSQL.AppendLine("INNER JOIN "+ tPosLnkDB +" TLNKMapping Map ON RC.FTTdmCode = Map.FTLnmUsrValue");
                oSQL.AppendLine("WHERE RC.FTTmnNum = HD.FTTmnNum");
                oSQL.AppendLine("AND RC.FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND RC.FTXihDocNo = HD.FTXihDocNo");
                oSQL.AppendLine("AND RC.FTXihDocType = HD.FTXihDocType");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, '')");
                oSQL.AppendLine("), '') + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"NAME1\": \"' + ISNULL(HD.FTXihCstName, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"NAME2\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"NAME3\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"NAME4\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"STREET\": \"' + ISNULL(HD.FTXihCstAddr1, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"CITY\": \"' + ISNULL(HD.FTXihCstAddr2, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"POSTAL_CODE\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"LAND\": \"' + 'TH' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TEL\": \"' + ISNULL(HD.FTXihCstTel, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"FAX\": \"' + ISNULL(HD.FTXihCstFax, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"DISTRICT\": \"' + ISNULL(HD.FTXihCstAddr2, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"BELNR\": \"' + ISNULL(HD.FTShdPlantCode,'') + HD.FTTmnNum + HD.FTShdTransNo + HD.FTXihDocNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"REF_BELNR\" : \"\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TEXT\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + FTData + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("(");
                oSQL.AppendLine("SELECT '1' AS FTType,");
                oSQL.AppendLine("('\"TDID\": \"ZADR\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"TDBODY\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTXihCstAddr1, '') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTXihCstAddr2, '') + '\"}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ']'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '2' AS FTType,");
                oSQL.AppendLine("('\"TDID\": \"ZDLT\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"TDBODY\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTXihDepositDueTime, '') + '\"}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ']'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '3' AS FTType,");
                oSQL.AppendLine("('\"TDID\": \"ZREM\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"TDBODY\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTXihRemark, '') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTXihEquipment, '') + '\"}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ']'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '4' AS FTType,");
                oSQL.AppendLine("('\"TDID\": \"ZMAP\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"TDBODY\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '{\"TDLINE\":\"' + ISNULL(HD.FTRemark, '') + '\"}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ']'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine(") TTmp");
                oSQL.AppendLine("WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, '')");
                oSQL.AppendLine("), '') + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"Item\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"POSEX\": \"' + CONVERT(VARCHAR, FNSdtSeqNo) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"QUANTITY\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtQtyAll)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"UOM\": \"' + ISNULL(FTPunCode, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"SHIPPING_POINT\": \"' + ISNULL(HD.FTShippingPoint, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"SELLING_PLANT\": \"' + ISNULL(HD.FTShdPlantCode, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"PRICING\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TYPE\": \"' + 'RETAILPRICE' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"AMOUNT\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtSaleAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CURRENCY\": \"THB\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SIGN\": \"+ \",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BONUSBUYID\": \"' + '' + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("ISNULL((CASE WHEN ISNULL((SELECT TOP 1 FTShdTransNo FROM TPSTSalCD CD with(nolock)");

                oSQL.AppendLine(" WHERE FTTmnNum = DT.FTTmnNum");

                oSQL.AppendLine(" AND FTShdTransNo = DT.FTShdTransNo");

                oSQL.AppendLine(" AND FNSdtSeqNo = CD.FNScdSeqNo), '') = '' THEN ''");
                oSQL.AppendLine("ELSE");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + ISNULL((");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TYPE\": \"' + ISNULL(FTScdBBYProfID, '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"AMOUNT\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCScdAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CURRENCY\": \"THB\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BONUSBUYID\": \"' + ISNULL(FTScdBBYNo, '') + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM TPSTSalCD CD with(nolock)");
                oSQL.AppendLine("WHERE FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND FNSdtSeqNo = DT.FNSdtSeqNo");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, '')");
                oSQL.AppendLine("), '') + ',' + CHAR(10)");
                oSQL.AppendLine("END),'') +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TYPE\": \"' + 'VAT' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"AMOUNT\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtVat)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CURRENCY\": \"THB\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SIGN\": \"+ \",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BONUSBUYID\": \"' + '' + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"EANCODE\": \"' + ISNULL(FTSkuCode, '') + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTSalVatDT DT with(nolock)");
                oSQL.AppendLine("WHERE DT.FTTmnNum = HD.FTTmnNum");
                oSQL.AppendLine("AND DT.FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND DT.FTXihDocNo = HD.FTXihDocNo");
                oSQL.AppendLine("AND DT.FTXihDocType = HD.FTXihDocType");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,'')");
                oSQL.AppendLine("),'') +CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTSalVatHD HD with(nolock)");
                oSQL.AppendLine("WHERE HD.FTXihDocType = '17'");

                if (ptLastUpd != "")
                {
                    oSQL.AppendLine("AND CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "'");
                }

                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,'')");
                oSQL.AppendLine("),'') +");
                oSQL.AppendLine("CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}'");
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
