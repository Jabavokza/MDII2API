using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using MDll2API.Modale.POSLog;
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
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        // public string C_POSTtEDC(string ptJson, string ptAPIURL, string ptAPIUsr, string ptAPIPwd, int pnAPIManual)
        public mlRESMsg C_POSToEDC(string ptMode, string ptTransDate, string[] patPlantCode)
        {
            string tPlantCode = "";
            string tJsonTrn = "";
            string tSQL = "";
            string tExecute = "";
            string tLastUpd = "";
            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";
            //  string tResp = "";
            StringBuilder oSQL;
            string tConnDB = "";
            string tFunction = "9";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order ,8:Bank Deposit ,9:EDC
            DataTable oTblConfig;
            DataRow[] oRow;

            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04

            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            string tStaSentOnOff;
            try
            {
                //tC_Plant = ptPlant;

                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                // Sort  Group Function
                oRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");

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
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation, ptMode, tPlantCode, ptTransDate);  //*Em 61-07-24

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

                if (tJsonTrn != "")
                {
                    #region ""ประกอบร่าง Json"
                    var oFusionJSON = new cFusionJSON(tJsonTrn);
                    var oJson = oFusionJSON.oC_Json;
                    #endregion
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "EDC");
                    oRESMsg.tML_TimeSent = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"); //เก็บเวลาที่ส่ง ไว้ลงLog
                    oRESMsg.tML_UrlApi = tUriApi; //เก็บUrlApi ไว้ลงLog
                    //Call API
                    if (tC_APIEnable == "true")
                    {
                        oRESMsg.tML_StatusCode = cConnectWebAPI.C_CONtWebAPI(tUriApi, tUsrApi, tPwdApi, oJson.ToString());
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
                            tConnDB += "; Connection Timeout = 60";
                            #region "UPDATE FLAG TPSTSalHD.FTStaSentOnOff"
                            oSQL = new StringBuilder();
                            if (ptMode == "AUTO")
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND FTStaEDC = '0'");
                            }
                            else
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND FTPlantCode IN (" + tPlantCode + ")");
                            }
                            var oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tConnDB);
                            if (oRESMsg.tML_StatusCode == "200")
                            {
                                if (oDbChk.Rows.Count > 0)
                                {
                                    for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                                    {
                                        oSQL = new StringBuilder();
                                        oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                                        oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                        oSQL.AppendLine(" ,FTStaEDC = '1'");
                                        oSQL.AppendLine(" ,FTJsonFileEDC = '" + oRESMsg.tML_FileName + "'");
                                        oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                                        oSQL.AppendLine("AND FDSaleDate = '" + ptTransDate + "'");
                                        var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
                                        //if (nRowEff > 0)// 10/812/82018
                                        //{
                                        //    oRESMsg.tML_StatusMsg = oRESMsg.tML_StatusMsg + " : อัพเดตสำเร็จ";
                                        //}
                                        //else
                                        //{
                                        //    oRESMsg.tML_StatusMsg = oRESMsg.tML_StatusMsg + " : อัพเดตไม่สำเร็จ";
                                        //}
                                    }
                                }
                            }
                            #endregion
                        }



                        #region " Keep Log"
                        //  cKeepLog.C_SETxKeepLogForEDC(aoRow, oRESMsg);
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
        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop, string ptWorkStationID, string ptWorkStation, string ptMode, string ptPlantCode, string ptTransDate)
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
                    catch (Exception)
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

                    if (ptPlantCode.Trim() != "")
                    {
                        oSQL.AppendLine("AND HD.FTShdPlantCode IN (" + ptPlantCode + ")");
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
                return oEx.Message;
            }
            finally
            {
                oSQL = null;
                rtResult = null;
            }
        }
    }
}
