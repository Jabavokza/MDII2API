using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using MDll2API.Modale.POSLog;
using System;
using System.Data;
using System.Net;
using System.Text;

namespace MDll2API.Class.POSLog
{
    public class cEOD
    {
        private string tC_Auto = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public mlRESMsg C_POSTtEOD(string ptMode, string ptTransDate, string[] patPlantCode)
        {
            //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            string tWorkStationID = "", tWorkStation = "", tJsonTrn = "", tSQL = "", tExecute = "", tLastUpd = "", tUriApi = "", tUsrApi = "", tPwdApi = "", tConnDB = "", tFunction = "5";

            StringBuilder oSQL;
            DataTable oTblConfig;
            DataRow[] oRow;
            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            string ptPlantCode = "";
            string tStaSentOnOff;
            try
            {
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();
                tC_Auto = ptMode;

                //tC_PlantEOD = ptPlantEOD;
                if (!(patPlantCode == null))
                {
                    for (int nLoop = 0; nLoop < patPlantCode.Length; nLoop++)
                    {
                        if (int.Equals(nLoop, 0))
                        {
                            ptPlantCode += "'" + patPlantCode[nLoop] + "'";
                        }
                        else
                        {
                            ptPlantCode += ", '" + patPlantCode[nLoop] + "'";
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
                    tWorkStationID = oRow[nRow]["WorkStationID"].ToString(); //*Em 61-07-23
                    tWorkStation = oRow[nRow]["WorkStation"].ToString(); //*Em 61-07-23

                    // Create Connection String Db
                    tConnDB = "Data Source = " + oRow[nRow]["Server"].ToString();
                    tConnDB += "; Initial Catalog = " + oRow[nRow]["DBName"].ToString();
                    tConnDB += "; User ID = " + oRow[nRow]["User"].ToString() + "; Password = " + oRow[nRow]["Password"].ToString();

                    // Check TPOSLogHis  Existing
                    tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                    cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                    tLastUpd = "";
                    tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID);  //*Em 61-07-23
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]), tWorkStationID, tWorkStation, ptPlantCode, ptTransDate);  //*Em 61-07-24

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
                    else
                    {
                        if (tJsonTrn == "[]")
                        {
                            tJsonTrn = "";
                        }
                    }
                }
                if (tJsonTrn != "")
                {
                    #region ""ประกอบร่าง Json"
                    var oFusionJSON = new cFusionJSON(tJsonTrn);
                    var oJson = oFusionJSON.oC_Json;
                    #endregion
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "EOD");

                    //Call API
                    if (tC_APIEnable == "true")
                    {
                        oRESMsg.tML_StatusCode = cConWebAPI.C_CONtWebAPI(tUriApi, tUsrApi, tPwdApi, oJson.ToString());
                        if (oRESMsg.tML_StatusCode == "200")
                        {
                            tStaSentOnOff = "1";
                            oRESMsg.tML_StatusMsg = "ส่งข้อมูลสมบูรณ์";
                        }
                        else
                        {
                            tStaSentOnOff = "2";
                            oRESMsg.tML_StatusMsg = "ส่งข้อมูลไม่สำเร็จ";
                        };

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
                            oSQL.AppendLine("WHERE FDSaleDate = '" + ptTransDate + "'");;
                            oSQL.AppendLine("AND FTPlantCode IN (" + ptPlantCode + ")");
                            //oSQL.AppendLine("AND FTPlantCode = '"+ patPlantCode + "'")
                        }
                        var oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tConnDB);
                        if (oDbChk.Rows.Count > 0)
                        {
                            for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                            {
                                oSQL = new StringBuilder();
                                oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                                oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                oSQL.AppendLine(" ,FTStaEOD = '1'");
                                oSQL.AppendLine(" ,FTJsonFileEOD = '" + oRESMsg.tML_FileName + "'");
                                oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                                oSQL.AppendLine("AND FDSaleDate = '" + ptTransDate + "'");
                                var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
                            }
                            //if (nRowEff > 0)
                            //{
                            //    oRESMsg.tML_StatusMsg = "OK";
                            //}
                        }
                        #endregion

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

        public string C_GETtSQL(string ptLastUpd, long pnRowTop, string ptWorkStationID, string ptWorkStation, string ptPlantCode,string ptTransDate)
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

                oSQL.AppendLine("SELECT ISNULL(STUFF((");
                oSQL.AppendLine("SELECT ',{' + CHAR(10) + ");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode, '') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' + HD.FTTmnNum + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + '" + ptWorkStationID + "' + STUFF('00000', 6 - LEN(ROW_NUMBER() OVER(ORDER BY FTShdPlantCode, FTTmnNum, FDShdTransDate, FTEmpCode)), LEN(ROW_NUMBER() OVER(ORDER BY FTShdPlantCode, FTTmnNum, FDShdTransDate, FTEmpCode)), ROW_NUMBER() OVER(ORDER BY FTShdPlantCode, FTTmnNum, FDShdTransDate, FTEmpCode)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"' + HD.FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"' + CONVERT(VARCHAR(10), FDShdTransDate, 121) + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"BeginDateTime\": \"' + CONVERT(VARCHAR, FDShdTransDate, 112) + REPLACE(MIN(HD.FTTimeIns), ':', '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"EndDateTime\": \"' + CONVERT(VARCHAR, FDShdTransDate, 112) + REPLACE(MAX(HD.FTTimeUpd), ':', '') + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"TenderControlTransaction\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"OperatorID\": \"' + FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"TillSettle\": [' + CHAR(10) +");
                oSQL.AppendLine("ISNULL(");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + FTData + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("FROM(");

                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'1' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' +   FTShdTransType + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(FCSdtSaleAmt))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("(SELECT HD3.FTShdTransType, HD3.FTEmpCode, HD3.FTShdTransNo, (CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END) AS FCSdtSaleAmt");
                oSQL.AppendLine("FROM TPSTSalHD HD3 with(nolock)");
                oSQL.AppendLine("LEFT JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD3.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = DT.FTSRVName");

                oSQL.AppendLine("LEFT JOIN TPSTSalRC RC3");

                oSQL.AppendLine("ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("WHERE HD3.FTShdTransType IN('03', '06', '07', '10', '11', '13', '15')");

                oSQL.AppendLine("AND RC3.FTTdmCode <> 'T008'");
                oSQL.AppendLine("AND HD3.FTShdStaBigLot <> 'Y'");
                oSQL.AppendLine("AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD3.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");

                oSQL.AppendLine("AND HD3.FCShdGrand > 0");

                oSQL.AppendLine("and HD3.FDShdTransDate = '" + ptTransDate + "'");

                oSQL.AppendLine("UNION ALL");

                oSQL.AppendLine("SELECT HD3.FTShdTransType, HD3.FTEmpCode, HD3.FTShdTransNo, HD3.FCShdGrand as FCSdtSaleAmt");
                oSQL.AppendLine("FROM TPSTSalHD HD3 with(nolock)");

                oSQL.AppendLine("INNER JOIN TPSTSalRC RC3");

                oSQL.AppendLine("ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("WHERE HD3.FTShdTransType IN('15')");

                oSQL.AppendLine("AND RC3.FTTdmCode <> 'T008'");
                oSQL.AppendLine("AND HD3.FTShdStaBigLot <> 'Y'");

                oSQL.AppendLine("AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD3.FTShdPlantCode = HD.FTShdPlantCode");

                oSQL.AppendLine("AND HD3.FCShdGrand > 0");

                oSQL.AppendLine("and HD3.FDShdTransDate = '" + ptTransDate + "') XData");
                oSQL.AppendLine("GROUP BY XData.FTEmpCode, XData.FTShdTransType");

                oSQL.AppendLine("UNION ALL");

                oSQL.AppendLine("SELECT");

                oSQL.AppendLine("'1' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"45\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD3.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD3 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD3.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = DT.FTSRVName");

                oSQL.AppendLine("INNER JOIN TPSTSalRC RC3");

                oSQL.AppendLine("ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("WHERE HD3.FTShdTransType IN('03', '06', '07', '10', '11', '13')");

                oSQL.AppendLine("AND RC3.FTTdmCode = 'T008'");
                oSQL.AppendLine("AND HD3.FTShdStaBigLot <> 'Y'");
                oSQL.AppendLine("AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD3.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");

                oSQL.AppendLine("AND HD3.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD3.FTEmpCode, HD3.FTShdTransType");

                oSQL.AppendLine("UNION ALL");

                oSQL.AppendLine("SELECT");

                oSQL.AppendLine("'1' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"43\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD3.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD3 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD3.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = DT.FTSRVName");

                oSQL.AppendLine("INNER JOIN TPSTSalRC RC3");

                oSQL.AppendLine("ON HD3.FTTmnNum = RC3.FTTmnNum");
                oSQL.AppendLine("AND HD3.FTShdTransNo = RC3.FTShdTransNo");
                oSQL.AppendLine("AND HD3.FTSRVName = RC3.FTSRVName");
                oSQL.AppendLine("WHERE HD3.FTShdTransType IN('07')");

                oSQL.AppendLine("AND HD3.FTShdStaBigLot = 'Y'");
                oSQL.AppendLine("AND HD3.FTTmnNum = HD.FTTmnNum AND HD3.FDShdTransDate = HD.FDShdTransDate AND FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD3.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");

                oSQL.AppendLine("AND HD3.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD3.FTEmpCode, HD3.FTShdTransType");


                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'2' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + HD5.FTShdTransType + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD5.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END) * (-1))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD5 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD5.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD5.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD5.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("AND HD5.FTShdStaBigLot <> 'Y'");
                oSQL.AppendLine("AND HD5.FTShdTransType IN('05', '16', '17', '26', '27', '28')");
                oSQL.AppendLine("AND HD5.FTTmnNum = HD.FTTmnNum AND HD5.FDShdTransDate = HD.FDShdTransDate AND HD5.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD5.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FTShdTransType NOT IN('16') AND ISNULL(DT.FTSdtStaSalType, '') <> '2'");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");
                oSQL.AppendLine("AND HD5.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD5.FTEmpCode, HD5.FTShdTransType");
                //
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'2' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"44\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD5.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END) * (-1))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD5 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD5.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD5.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD5.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("AND HD5.FTShdStaBigLot = 'Y'");
                oSQL.AppendLine("AND HD5.FTShdTransType IN('16')");
                oSQL.AppendLine("AND HD5.FTTmnNum = HD.FTTmnNum AND HD5.FDShdTransDate = HD.FDShdTransDate AND HD5.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD5.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FTShdTransType NOT IN('17') AND ISNULL(DT.FTSdtStaSalType, '') <> '2'");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");
                oSQL.AppendLine("AND HD5.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD5.FTEmpCode, HD5.FTShdTransType");
                //
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '3' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Sale' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + DT.FTShdTransType + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(DT.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN DT.FCSdtSaleAmt = 0 THEN DT.FCSdtDis ELSE DT.FCSdtSaleAmt END) * (-1))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD4 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock)");
                oSQL.AppendLine("ON HD4.FTTmnNum = DT.FTTmnNum");
                oSQL.AppendLine("AND HD4.FTShdTransNo = DT.FTShdTransNo");
                oSQL.AppendLine("AND HD4.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("WHERE HD4.FTShdTransType = '04'");
                oSQL.AppendLine("AND DT.FTTmnNum = HD.FTTmnNum AND DT.FDShdTransDate = HD.FDShdTransDate AND HD4.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD4.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");
                oSQL.AppendLine("AND HD4.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD4.FTEmpCode, DT.FTShdTransType");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '4' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Discount' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + CONVERT(VARCHAR, CD.FTScdBBYProfID) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"MiscellaneousDiscounts\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CD.FCScdAmt) * (-1))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Count\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(CD.FNDctNo))) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD2 with(nolock)");

                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock) ON HD2.FTTmnNum = DT.FTTmnNum AND HD2.FTShdTransNo = DT.FTShdTransNo AND HD2.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("INNER JOIN TPSTSalCD CD with(nolock) ON HD2.FTTmnNum = CD.FTTmnNum");
                oSQL.AppendLine("AND DT.FTShdTransNo = CD.FTShdTransNo");
                oSQL.AppendLine("AND DT.FTShdTransType = CD.FTShdTransType");
                oSQL.AppendLine("AND DT.FDShdTransDate = CD.FDShdTransDate");
                oSQL.AppendLine("AND Dt.FNSdtSeqNo = CD.FNSdtSeqNo");
                oSQL.AppendLine("AND DT.FTSRVName = CD.FTSRVName");
                oSQL.AppendLine("WHERE HD2.FTShdTransType IN('03', '06', '07', '10', '11', '13')");
                oSQL.AppendLine("AND HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD2.FTShdPlantCode = HD.FTShdPlantCode AND (DT.FTShdTransType NOT IN('06') AND ISNULL(DT.FTSdtStaSalType, '') <> '2' OR DT.FTShdTransType IN('06') AND ISNULL(DT.FTSdtStaSalType, '') = '1')");

                oSQL.AppendLine("AND DT.FCSdtQty > 0");

                oSQL.AppendLine("AND HD2.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD2.FTEmpCode, CD.FTScdBBYProfID");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'5' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Tax' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'VAT' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(DT.FTShdTransNo) + (select Count(*) from  [dbo].[TPSTSalHD] HD15 where HD15.FTTmnNum = HD.FTTmnNum AND HD15.FDShdTransDate = HD.FDShdTransDate AND HD15.FTEmpCode = HD.FTEmpCode AND HD15.FTShdPlantCode = HD.FTShdPlantCode and  HD15.FTShdTransType ='15'))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TotalNetSalesAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN ISNULL(DT.FCSdtVat, 0) * (-1) ELSE ISNULL(DT.FCSdtVat, 0) END))) + '\"'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalDT DT with(nolock) ON HD2.FTTmnNum = DT.FTTmnNum AND HD2.FTShdTransNo = DT.FTShdTransNo  AND HD2.FTSRVName = DT.FTSRVName");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName, '') <> ''");
                oSQL.AppendLine("WHERE HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("AND HD2.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND DT.FCSdtQty > 0");
                //oSQL.AppendLine("AND DT.FCSdtVat > 0");
                oSQL.AppendLine("AND HD2.FCShdGrand > 0");
                oSQL.AppendLine("GROUP BY HD2.FTEmpCode");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '6' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Tender' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + CASE WHEN RC.FTTdmCode = 'T009' THEN 'T030' ELSE RC.FTTdmCode END + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(RC.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Ending\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN RC.FCSrcNet * (-1) ELSE RC.FCSrcNet END))) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("INNER JOIN TPSTSalRC RC with(nolock) ON HD2.FTTmnNum = RC.FTTmnNum");
                oSQL.AppendLine("AND HD2.FTShdTransNo = RC.FTShdTransNo");
                oSQL.AppendLine("AND HD2.FTShdTransType = RC.FTShdTransType");
                oSQL.AppendLine("AND HD2.FDShdTransDate = RC.FDShdTransDate");
                oSQL.AppendLine("AND HD2.FTShdPlantCode = RC.FTShdPlantCode");
                oSQL.AppendLine("AND HD2.FTSRVName = RC.FTSRVName");
                oSQL.AppendLine("AND HD2.FCShdGrand > 0");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName, '') <> ''");
                oSQL.AppendLine("AND HD2.FTTmnNum = HD.FTTmnNum AND HD2.FDShdTransDate = HD.FDShdTransDate AND HD2.FTEmpCode = HD.FTEmpCode");
                oSQL.AppendLine("Where HD2.FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("GROUP BY RC.FTTdmCode");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '7' AS FTType,");
                oSQL.AppendLine("('\"@TransType\": \"' + 'Tender' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + 'T032' + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TransactionCount\": \"' + CONVERT(VARCHAR, CONVERT(int, COUNT(HD2.FTShdTransNo))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + '\"TenderSummary\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '\"Ending\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), SUM(CASE WHEN TTY.FTSttGrpName = 'RETURN' THEN HD2.FCShdRnd * (-1) ELSE HD2.FCShdRnd END) * (-1))) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine(") AS FTData");
                oSQL.AppendLine("FROM TPSTSalHD HD2 with(nolock)");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD2.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName, '') <> ''");

                if (tC_Auto == "AUTO")
                {
                    //if (ptLastUpd != "")
                    //{
                    oSQL.AppendLine("WHERE HD2.FCShdRnd <> 0 ");
                    //    oSQL.AppendLine("AND HD2.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta where FDSaleDate = '" + tC_DateTrn + "' AND ISNULL(FTStaEOD, '0') = '0')");
                    //}
                    //else
                    //{
                    //oSQL.AppendLine("WHERE HD2.FDShdTransDate='" + tC_DateTrn + "'  AND HD2.FCShdRnd <> 0  ");   //*Em 61-08-22
                    //}
                    //oSQL.AppendLine("INNER JOIN TCNMPlnCloseSta with(nolock) ON HD2.FDShdTransDate = TCNMPlnCloseSta.FDSaleDate AND HD2.FTShdPlantCode = TCNMPlnCloseSta.FTPlantCode AND ISNULL(TCNMPlnCloseSta.FTStaEOD, '0') = '0'");
                }
                else if (tC_Auto == "MANUAL")
                {
                    //if (ptLastUpd != "")
                    //{
                    //    oSQL.AppendLine("WHERE CONVERT(varchar(8),HD2.FDDateUpd,112) + REPLACE(HD2.FTTimeUpd,':','') > '" + ptLastUpd + "'  AND HD2.FDShdTransDate='" + tC_DateTrn + "' AND HD2.FTShdPlantCode='" + tC_PlantEOD + "' AND HD2.FCShdRnd <> 0");
                    //}
                    //else
                    //{
                    oSQL.AppendLine("WHERE HD2.FDShdTransDate = '" + ptTransDate + "' AND HD2.FTShdPlantCode IN(" + ptPlantCode + ")  AND HD2.FCShdRnd <> 0 ");   //*Em 61-08-22
                    //}
                }

                oSQL.AppendLine("GROUP BY FTEmpCode");
                oSQL.AppendLine(") tmp");
                oSQL.AppendLine("WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("), 1, 1, '') ,'') +CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + ']' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("'}'");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType TTY with(nolock) ON HD.FTShdTransType = TTY.FTSttTranCode AND ISNULL(TTY.FTSttGrpName,'') <> ''");

                if (tC_Auto == "AUTO")
                {
                    //    if (ptLastUpd != "")
                    //   {
                    oSQL.AppendLine("WHERE  HD.FDShdTransDate = '" + ptTransDate + "'");
                    oSQL.AppendLine("AND HD.FTShdPlantCode IN(SELECT FTPlantCode FROM[dbo].TCNMPlnCloseSta WHERE FDSaleDate = '" + ptTransDate + "' AND ISNULL(FTStaEOD, '0') = '0')");
                    //   }
                    //   else
                    //  {
                    //oSQL.AppendLine("INNER JOIN TCNMPlnCloseSta with(nolock) ON HD.FDShdTransDate = TCNMPlnCloseSta.FDSaleDate AND HD.FTShdPlantCode = TCNMPlnCloseSta.FTPlantCode AND ISNULL(TCNMPlnCloseSta.FTStaEOD, '0') = '0' AND HD.FDShdTransDate = '" + tC_DateTrn + "'"); //*Em 61-08-22
                    //  }
                }
                else if (tC_Auto == "MANUAL")
                {
                    //if (ptLastUpd != "")
                    //{
                    //    oSQL.AppendLine("WHERE CONVERT(varchar(8), HD.FDDateUpd,112) + REPLACE(HD.FTTimeUpd,':','') > '" + ptLastUpd + "'  AND HD.FDShdTransDate='" + tC_DateTrn + "' AND HD.FTShdPlantCode='" + tC_PlantEOD + "'");
                    //}
                    //else
                    //{
                    oSQL.AppendLine("WHERE HD.FDShdTransDate = '" + ptTransDate + "' AND HD.FTShdPlantCode IN (" + ptPlantCode + ")");   //*Em 61-08-22
                    //}
                }

                oSQL.AppendLine("GROUP BY FTEmpCode,FTTmnNum,FTShdPlantCode,FDShdTransDate");
                oSQL.AppendLine("FOR XML PATH('')");
                oSQL.AppendLine("),1,1,''),'')");
                oSQL.AppendLine("FOR XML PATH('')");

                //oSQL.AppendLine("GROUP BY FTEmpCode,FTTmnNum,FTShdPlantCode,FDShdTransDate");   //*Em 61-08-22
                //oSQL.AppendLine("FOR XML PATH('')");
                //oSQL.AppendLine("),1,1,''),'')");
                //oSQL.AppendLine("FOR XML PATH('')");

                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception)
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
