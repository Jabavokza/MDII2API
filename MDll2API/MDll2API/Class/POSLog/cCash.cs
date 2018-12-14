
using System;
using System.Text;
using System.Data;
using System.Net;
using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using MDll2API.Modale.POSLog;

namespace MDll2API.Class.POSLog
{
    public class cCash
    {
      //  private string ptPlantCode = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public mlRESMsg C_POSToCash(string ptMode ,string ptTransDate,string[] patPlantCash)
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

            string tJsonTrn = "";
            string tSQL = "";
            string tExecute = "";
            string tLastUpd = "";
            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";
            StringBuilder oSQL;
            string tConnDB = "";
            string tFunction = "4";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] oRow;

            string tWorkStationID = ""; //*Em 61-08-04
            string tWorkStation = ""; //*Em 61-08-04
            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            string tPlantCode = "";
            string tStaSentOnOff;
            try
            { 
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                //tC_PlantEOD = ptPlantCash;
                if (!(patPlantCash == null))
                {
                    for (int nLoop = 0; nLoop < patPlantCash.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            tPlantCode += "'" + patPlantCash[nLoop] + "'";
                        }
                        else
                        {
                            tPlantCode += ", '" + patPlantCash[nLoop] + "'";
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
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation,ptMode,tPlantCode,ptTransDate);  //*Em 61-07-24

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
                    if (tJsonTrn=="[]") { tJsonTrn = ""; }
                }
                

                if (tJsonTrn != "")
                {
                    #region ""ประกอบร่าง Json"
                    var oFusionJSON = new cFusionJSON(tJsonTrn);
                    var oJson = oFusionJSON.oC_Json;
                    #endregion
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "CASH");

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
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND FTStaEOD = '0'");
                                //oSQL.AppendLine("AND FTPlantCode = '" + patPlantCode + "'");
                            }
                            else if (ptMode == "MANUAL")
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                                oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");
                                oSQL.AppendLine("AND FTPlantCode IN (" + tPlantCode + ")");
                                //oSQL.AppendLine("AND FTPlantCode = '"+ patPlantCode + "'");
                            }

                            var oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tConnDB);
                            if (oDbChk.Rows.Count > 0)// 10/812/82018
                            {
                                for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                                {
                                    oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                    oSQL.AppendLine("   ,FTStaEDC = '1'");
                                    oSQL.AppendLine("   ,FTJsonFileEDC = '" + oRESMsg.tML_FileName + "'");
                                    oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                                    oSQL.AppendLine("AND FDSaleDate = '" + ptTransDate + "'");
                                    var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
                                    //if (nRowEff > 0)
                                    //{
                                    //    oRESMsg.tML_StatusMsg = oRESMsg.tML_StatusMsg + " : อัพเดตสำเร็จ";
                                    //}
                                    //else
                                    //{
                                    //    oRESMsg.tML_StatusMsg = oRESMsg.tML_StatusMsg + " : อัพเดตไม่สำเร็จ";
                                    //}
                                }
                            }
                            #endregion

                        }




                        #region " Keep Log"
                        //  cKeepLog.C_SETxKeepLogForEOD(aoRow, oRESMsg);
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

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop, string ptWorkStationID , string ptWorkStation,string ptMode,string ptPlantCode,string ptTransDate )
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
                    catch (Exception )
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
                oSQL.AppendLine("    WHERE HD2.FTShdPlantCode = HD.FTShdPlantCode AND HD2.FDShdTransDate = HD.FDShdTransDate AND RC.FTTdmCode = RC2.FTTdmCode AND HD2.FTSRVName = RC.FTSRVName AND   HD2.FDShdTransDate = '" + ptTransDate + "'");
                oSQL.AppendLine("    GROUP BY HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName) Trn");
                oSQL.AppendLine("    FULL OUTER JOIN");
                oSQL.AppendLine("    (SELECT HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, SUM(RC.FCSrcNet) AS FCSrcNet");
                oSQL.AppendLine("    FROM TPSTSalHD HD2 WITH(NOLOCK)");
                oSQL.AppendLine("    INNER JOIN TPSTSalRC RC WITH(NOLOCK) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("    WHERE HD2.FTShdPlantCode = HD.FTShdPlantCode AND HD2.FDShdTransDate = HD.FDShdTransDate AND RC.FTTdmCode = RC2.FTTdmCode");
                oSQL.AppendLine("    AND HD2.FTShdTransType = '45' AND  HD2.FDShdTransDate = '" + ptTransDate + "' AND HD2.FTSRVName = RC.FTSRVName");
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

                if (ptMode == "AUTO")
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
                oSQL.AppendLine("      where  HD2.FDShdTransDate = '" + ptTransDate + "'");
                oSQL.AppendLine("      GROUP BY HD2.FTShdPlantCode, HD2.FDShdTransDate, RC.FTTdmCode, HD2.FTSRVName) Trn");
                oSQL.AppendLine("        FULL OUTER JOIN");
                oSQL.AppendLine("        (SELECT HD2.FTShdPlantCode , HD2.FDShdTransDate , RC.FTTdmCode , SUM(RC.FCSrcNet) AS FCSrcNet");
                oSQL.AppendLine("        FROM TPSTSalHD HD2 WITH(NOLOCK)");
                oSQL.AppendLine("        INNER JOIN TPSTSalRC RC WITH(NOLOCK) ON HD2.FTTmnNum = RC.FTTmnNum AND HD2.FTShdTransNo = RC.FTShdTransNo AND HD2.FTSRVName= RC.FTSRVName");
                oSQL.AppendLine("        WHERE HD2.FTShdTransType = '45' and HD2.FDShdTransDate = '" + ptTransDate + "'");
                oSQL.AppendLine("        GROUP BY HD2.FTShdPlantCode , HD2.FDShdTransDate , RC.FTTdmCode) Tdr");
                oSQL.AppendLine("          ON Trn.FTShdPlantCode = Tdr.FTShdPlantCode AND Trn.FDShdTransDate = Tdr.FDShdTransDate");
                oSQL.AppendLine("    AND Trn.FTTdmCode = Tdr.FTTdmCode");
                oSQL.AppendLine("    WHERE(ISNULL(Trn.FCSrcNet, 0) - ISNULL(Tdr.FCSrcNet, 0)) <> 0  ) RC3 on Hd.FTShdPlantCode = RC3.FTShdPlantCode and Hd.FDShdTransDate = RC3.FDShdTransDate and RC2.FTTdmCode = RC3.FTTdmCode AND HD.FTSRVName = RC3.FTSRVName");
                
                if (ptMode == "AUTO")
                {
                    oSQL.AppendLine("WHERE RC3.FCSrcNet <> 0");
                    //oSQL.AppendLine("WHERE HD.FDShdTransDate = '" + tC_DateTrn + "'  and RC3.FCSrcNet <> 0");
                    oSQL.AppendLine("AND HD.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta where FDSaleDate = '" + ptTransDate + "' AND ISNULL(FTStaEOD, '0') = '1' AND ISNULL(FTStaShortOver, '0') = '0')");

                }
                else if (ptMode == "MANUAL")
                {
                    oSQL.AppendLine("WHERE HD.FDShdTransDate = '" + ptTransDate + "'  and RC3.FCSrcNet <> 0");
                    oSQL.AppendLine("AND HD.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta where FDSaleDate = '" + ptTransDate + "' AND ISNULL(FTStaEOD, '0') = '1' AND ISNULL(FTStaShortOver, '0') = '0') AND HD.FTShdPlantCode IN (" + ptPlantCode + ")");
                }

                oSQL.AppendLine("GROUP BY HD.FTShdPlantCode,HD.FDShdTransDate,RC2.FTTdmCode");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,''),'') +']'");
                oSQL.AppendLine("FOR XML PATH('')");
   
                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception )
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
