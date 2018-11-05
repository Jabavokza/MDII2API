
using MDll2API.Modale.ReceivApp;
using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using MDll2API.Modale.POSLog;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cBankDeposit
    {
        private cRcvBank oC_RcvBank = new cRcvBank();
        private string tC_DateTrn = "";
        private string tC_Auto = "";
        private string tC_PlantCode = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public string C_POSTtBankDeposit(string ptDTrn, cRcvBank poRcvBank, string ptAuto, string[] patPlantBnk = null)
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
            string tConnDB = "";
            string tFunction = "8";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order ,8:Bank Deposit
            DataTable oTblConfig;
            DataRow[] oRow;
            DateTime dStart;
            DateTime dEnd;
            string tStatusCode = "";
            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04
            mlPOSBankDeposit oPOSBankDeposit = null;
            tC_DateTrn = ptDTrn;
            oC_RcvBank = poRcvBank;
            tC_Auto = ptAuto;
            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis(); 
            try
            {
                dStart = DateTime.Now;
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                if (!(patPlantBnk == null))
                {
                    for (int nLoop = 0; nLoop < patPlantBnk.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            tC_PlantCode += "'" + patPlantBnk[nLoop] + "'";
                        }
                        else
                        {
                            tC_PlantCode += ", '" + patPlantBnk[nLoop] + "'";
                        }
                    }
                }

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
                    tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                    cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                    tLastUpd = "";
                    tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation);  //*Em 61-07-24

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

                    if (tJsonTrn == "[]")
                    {
                        tJsonTrn = "";
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
                    tFileName = cCNSP.SP_WRItJSON(tJson, "BANK");

                    //Call API
                        if (tC_APIEnable == "true")
                        {
                            cConWebAPI.C_CONtWebAPI(tUriApi, tUsrApi, tPwdApi, tJson);
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
                        cCNSP.SP_SQLnExecute(oSql.ToString(), tConnDB);

                    }

                    //for (int i = 0; i < oPOSBankDeposit1.POSLog.Transaction.Length; i++)
                    //{
                    //    for (int j=0;j< oPOSBankDeposit1.POSLog.Transaction[i].Length;j++)
                    //    {
                    //        string tDate = oPOSBankDeposit1.POSLog.Transaction[i][j].BusinessDayDate;
                    //    }
                    //    //string tUPD = "";
                    //    //string tDate = oPOSBankDeposit1.POSLog.Transaction[0][i].BusinessDayDate;
                    //    //string tOper = oPOSBankDeposit1.POSLog.Transaction[0][i].OperatorID;
                    //    //string tPlant = oPOSBankDeposit1.POSLog.Transaction[0][i].BusinessUnit.UnitID;
                    //    //tUPD = "UPDATE TPSTBankDeposit SET FTStaSentOnOff='1' WHERE FTBdpPlantCode='" + tPlant + "' AND FTBdpDepositBy='" + tOper + "' AND FDBdpDepositDate='" + tDate + "' ";
                    //}


                    //----------------------------UPDATE FLAG TPSTSalHD.FTStaSentOnOff ---------------------------------
                    oPOSBankDeposit = JsonConvert.DeserializeObject<mlPOSBankDeposit>(tJson);

                    for (int i = 0; i < oPOSBankDeposit.oML_POSLog.Transaction.Length; i++)
                    {
                        for (int j = 0; j < oPOSBankDeposit.oML_POSLog.Transaction[i].Length; j++)
                        {
                            string tUPD = "";
                            string tDate = oPOSBankDeposit.oML_POSLog.Transaction[i][j].tML_BusinessDayDate;
                            string tOper = oPOSBankDeposit.oML_POSLog.Transaction[i][j].tML_OperatorID;
                            string tPlant = oPOSBankDeposit.oML_POSLog.Transaction[i][j].oML_BusinessUnit.tML_UnitID;
                            tUPD = "UPDATE TPSTBankDeposit SET FTStaSentOnOff='1',FTJsonFileName='" + tFileName + "' WHERE FTBdpPlantCode='" + tPlant + "' AND FTBdpDepositBy='" + tOper + "' AND FDBdpDepositDate='" + tDate + "' ";
                            cCNSP.SP_SQLnExecute(tUPD, tConnDB);
                        }

                    }
                    //----------------------------UPDATE FLAG TPSTSalHD.FTStaSentOnOff ---------------------------------

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
              //  oSP = null;
                tConnDB = null;
                tFunction = null;
                oTblConfig = null;
                oRow = null;
            }
        }
        private string C_GETtSQL(string ptLastUpd, long pnRowTop = 100, string ptWorkStationID = "", string ptWorkStation = "")
        {
            StringBuilder oSQL = new StringBuilder();
            try
            {
                oSQL.AppendLine("SELECT '[' + ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP " + pnRowTop + "',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(FTBdpPlantCode,'') + '\"},'+ CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' +  '" + ptWorkStation + "' + '\",'+ CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10),FDBdpSaleDate,112)+'" + ptWorkStationID + "'+ STUFF('00000', 6-LEN(ROW_NUMBER() OVER(ORDER BY FTBdpPlantCode,FDBdpSaleDate,FTBdpDocNo)) , LEN(ROW_NUMBER() OVER(ORDER BY FTBdpPlantCode,FDBdpSaleDate,FTBdpDocNo)), ROW_NUMBER() OVER(ORDER BY FTBdpPlantCode,FDBdpSaleDate,FTBdpDocNo)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"' + FTBdpDepositBy + '\",'+ CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",'+ CHAR(10) +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"'+ CONVERT(VARCHAR(10),FDBdpDocDate,121) +'\",'+ CHAR(10) +");
                oSQL.AppendLine("'\"TenderControlTransaction\": {'+ CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"TillSettle\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"@LedgerType\": \"BankIn\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Sales\": [' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + ISNULL(");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT ");
                oSQL.AppendLine("	',{' + CHAR(10) +");
                oSQL.AppendLine("	CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"T001\",' + CHAR(10) +");
                oSQL.AppendLine("	CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpActualAmt)) +'\",' + CHAR(10) +");
                oSQL.AppendLine("	CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessUnit\": { \"UnitID\": \"'+ ISNULL(FTBdpPlantCode,'') +'\" }' + CHAR(10) +");
                oSQL.AppendLine("	CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' ");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,'') ,'') + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Deposit\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpDepositAmt)) +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"GLAccount\": \"'+ FTBdpGLAcc +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Reference\": \"'+ FTBdpPaySlipNo +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BankInDate\": \"'+ CONVERT(VARCHAR(10),FDBdpDepositDate,121) +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessDate\": \"'+ CONVERT(VARCHAR(10),FDBdpDocDate,121) +'\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");

                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) +  IIF(FCBdpDepositAmt>FCBdpActualAmt,',\"Over\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }',',\"Short\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }') + CHAR(10) +"); 
                oSQL.AppendLine("(CASE WHEN  ISNULL(FCBdpOverShort, 0) > 0  THEN");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) +  IIF((FCBdpDepositAmt > FCBdpActualAmt) AND (FCBdpActualAmt >= 0),',\"Over\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }',',\"Short\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }') ");
                oSQL.AppendLine(" ELSE ");
                oSQL.AppendLine(" '' ");
                oSQL.AppendLine(" END) + ");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("'}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTBankDeposit WITH(NOLOCK)");
                oSQL.AppendLine("WHERE ISNULL(FTBdpApproveBy,'') <> '' AND ISNULL(FTStaSentOnOff, '0') <> '1'   ");

                if (tC_Auto == "AUTO")
                {
                    //if (ptLastUpd != "")
                    //{
                    //    oSQL.AppendLine("AND CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "' AND FDBdpSaleDate='" + tC_DateTrn + "' ");//*Em 61-08-04
                    //}
                    //else
                    //{
                    oSQL.AppendLine("AND FDBdpSaleDate = '" + tC_DateTrn + "'  ");
                    //}
                }
                else if (tC_Auto == "MANUAL")
                {
                    //if (ptLastUpd != "")
                    //{
                    //    oSQL.AppendLine("AND CONVERT(varchar(8),HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "' AND FDBdpSaleDate='" + tC_DateTrn + "'AND " + oC_RcvBank.Field + oC_RcvBank.Value + " ");//*Em 61-08-04
                    //}
                    //else
                    //{
                    //oSQL.AppendLine("AND FDBdpSaleDate='" + tC_DateTrn + "' AND " + oC_RcvBank.Field + oC_RcvBank.Value + " AND FTBdpPlantCode IN (" + tC_PlantCode +")");
                    //}
                    oSQL.AppendLine("AND FDBdpSaleDate = '" + tC_DateTrn + "' AND FTBdpPlantCode IN (" + tC_PlantCode + ")");
                }

                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,''),'') +']'");
                oSQL.AppendLine("FOR XML PATH('')");
                return oSQL.ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
