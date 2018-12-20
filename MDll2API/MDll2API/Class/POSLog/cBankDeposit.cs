
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

        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public mlRESMsg C_POSToBankDeposit(string ptMode, string ptTransDate, string[] patPlantCode)
        {

            string tJson = "";
            string tJsonTrn = "";
            string tSQL = "";
            string tExecute = "";
            string tLastUpd = "";

            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";

            StringBuilder oSQL;
            string tConnDB = "";
            string tFunction = "8";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order ,8:Bank Deposit
            DataTable oTblConfig;
            DataRow[] oRow;
            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04
            mlPOSBankDeposit oPOSBankDeposit = new mlPOSBankDeposit();

            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            string tStaSentOnOff;
            string tPlantCode = "";
            try
            {
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                if (!(patPlantCode == null))
                {
                    for (int nLoop = 0; nLoop < patPlantCode.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            tPlantCode += "'" + patPlantCode[nLoop] + "'";
                        }
                        else
                        {
                            tPlantCode += ", '" + patPlantCode[nLoop] + "'";
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
                    tConnDB += "; Connection Timeout = 120";
                    // Check TPOSLogHis  Existing
                    tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                    cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                    tLastUpd = "";
                    tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation, ptMode, tPlantCode, ptTransDate);  //*Em 61-07-24

                    tExecute = cCNSP.SP_SQLtExecuteJson(tSQL, tConnDB);
                    if (tExecute == "[]")
                    {
                        tExecute = "";
                    }
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
                    #region ""ประกอบร่าง Json"
                    var oFusionJSON = new cFusionJSON(tJsonTrn);
                    var oJson = oFusionJSON.oC_Json;
                    #endregion
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "BANK");
                    oRESMsg.tML_TimeSent = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"); //เก็บเวลาที่ส่ง ไว้ลงLog
                    oRESMsg.tML_UrlApi = tUriApi; //เก็บUrlApi ไว้ลงLog

                    if (tC_APIEnable == "true")
                    {
                        var oConnectWebAPI = new cConnectWebAPI(tUriApi, tUsrApi, tPwdApi, oJson.ToString());
                        oRESMsg.tML_StatusCode = oConnectWebAPI.tC_StatusCode;

                        if (oRESMsg.tML_StatusCode == "200" || oRESMsg.tML_StatusCode == "202")
                        {
                            tStaSentOnOff = "1";
                            oRESMsg.tML_StatusMsg = "ส่งข้อมูลสมบูรณ์";
                        }
                        else
                        {
                            tStaSentOnOff = "2";
                            oRESMsg.tML_StatusMsg = "ส่งข้อมูลไม่สำเร็จ";
                        };

                        for (int nRow = 0; nRow < oRow.Length; nRow++)
                        {
                            // Create Connection String Db
                            tConnDB = "Data Source=" + oRow[nRow]["Server"].ToString();
                            tConnDB += "; Initial Catalog=" + oRow[nRow]["DBName"].ToString();
                            tConnDB += "; User ID=" + oRow[nRow]["User"].ToString() + "; Password=" + oRow[nRow]["Password"].ToString();
                            tConnDB += "; Connection Timeout = 120";
                            #region "UPDATE FLAG TPSTSalHD.FTStaSentOnOff"
                            if (ptMode == "AUTO")
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND ISNULL(FTStaBankIn,'0') = '0'");
                            }
                            else
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND FTPlantCode IN (" + tPlantCode + ")");
                            }
                            var oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tConnDB);
                            if (oDbChk.Rows.Count > 0)// 10/812/82018
                            {
                                for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                                {
                                    oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                    oSQL.AppendLine("   ,FTStaBankIn = '1'");
                                    oSQL.AppendLine("   ,FTJsonFileBankIn = '" + oRESMsg.tML_FileName + "'");
                                    oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                                    oSQL.AppendLine("AND FDSaleDate = '" + ptTransDate + "'");
                                    var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
                                }
                            }

                            oPOSBankDeposit = JsonConvert.DeserializeObject<mlPOSBankDeposit>(oJson.ToString());
                            for (int i = 0; i < oPOSBankDeposit.oML_POSLog.Transaction.Length; i++)
                            {
                                for (int j = 0; j < oPOSBankDeposit.oML_POSLog.Transaction[i].Length; j++)
                                {
                                    string tDate = oPOSBankDeposit.oML_POSLog.Transaction[i][j].tML_BusinessDayDate;
                                    string tOper = oPOSBankDeposit.oML_POSLog.Transaction[i][j].tML_OperatorID;
                                    string tPlant = oPOSBankDeposit.oML_POSLog.Transaction[i][j].oML_BusinessUnit.tML_UnitID;

                                    oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TPSTBankDeposit");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                    oSQL.AppendLine(",FTJsonFileName ='" + oRESMsg.tML_FileName + "'");
                                    oSQL.AppendLine("WHERE  FTBdpPlantCode ='" + tPlant + "'");
                                    oSQL.AppendLine("AND  FTBdpDepositBy ='" + tOper + "'");
                                    oSQL.AppendLine("AND FDBdpDepositDate ='" + tDate + "'");
                                    var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);  
                                }
                            }
                            #endregion
                        }

                        #region " Keep Log"
                        //  cKeepLog.C_SETxKeepLogForBank(aoRow, oRESMsg);
                        #endregion
                    }
                    else
                    {
                        oRESMsg.tML_StatusCode = "001";
                        oRESMsg.tML_StatusMsg = "ฟังก์ชั่น APIไม่ทำงาน";
                    }
                }
                else
                {
                    oRESMsg.tML_StatusCode = "000";
                    oRESMsg.tML_StatusMsg = "ไม่พบข้อมูลที่จะส่ง";
                }
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }

        }
        private string C_GETtSQL(string ptLastUpd, long pnRowTop, string ptWorkStationID, string ptWorkStation, string ptMode, string ptPlantCode, string ptTransDate)
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
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) +  IIF((FCBdpDepositAmt > FCBdpActualAmt) AND (FCBdpActualAmt >= 0),' ,\"Over\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }',',\"Short\": { \"Amount\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCBdpOverShort)) +'\" }') ");
                oSQL.AppendLine(" ELSE ");
                oSQL.AppendLine(" '' ");
                oSQL.AppendLine(" END) + ");
           //     oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("'}' + CHAR(10)");
                oSQL.AppendLine("FROM TPSTBankDeposit WITH(NOLOCK)");

                oSQL.AppendLine("JOIN TCNMPlnCloseSta with(nolock) ON TPSTBankDeposit.FDBdpSaleDate = TCNMPlnCloseSta.FDSaleDate ");
                oSQL.AppendLine("AND TPSTBankDeposit.FTBdpPlantCode = TCNMPlnCloseSta.FTPlantCode");
                oSQL.AppendLine("WHERE ISNULL(TPSTBankDeposit.FTBdpApproveBy,'') <> '' AND ISNULL(TPSTBankDeposit.FTStaSentOnOff, '0') <> '1' ");

                if (ptMode == "AUTO")
                {
                    oSQL.AppendLine("AND FDBdpSaleDate = '" + ptTransDate + "'  ");
                    oSQL.AppendLine("AND TCNMPlnCloseSta.FTStaEOD = '1'");
                }
                else if (ptMode == "MANUAL")
                {
                    oSQL.AppendLine("AND FDBdpSaleDate = '" + ptTransDate + "' AND FTBdpPlantCode IN (" + ptPlantCode + ")");
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
