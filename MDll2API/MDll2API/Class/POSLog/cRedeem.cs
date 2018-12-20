using MDll2API.Modale.ReceivApp;
using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;
using System;
using System.Data;
using System.Text;
using MDll2API.Modale.POSLog;

namespace MDll2API.Class.POSLog
{
    public class cRedeem
    {
        mlRcvRedeem oC_RcvRedeem = new mlRcvRedeem();
        private string tC_Mode = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public mlRESMsg C_POSToRedeem(string ptMode,string ptTransDate, mlRcvRedeem oRcvRedeem, mlRedeem poRedeem)
        {
            string tJsonTrn = "";
            string tUriApi = "";
            string tUsrApi = "";
            string tPwdApi = "";
            string tConnDB = "";
            string tFunction = "2";  //1:Point ,2:Redeem Premium ,3:Sale & Deposit ,4:Cash Overage/Shortage ,5:EOD ,6:AutoMatic Reservation ,7:Sale Order
            DataTable oTblConfig;
            DataRow[] aoRow;
            string tWorkStationID = ""; //*Em 61-08-09 Com.Sheet ML-POSC-0032
            string tWorkStation = ""; //*Em 61-08-09 Com.Sheet ML-POSC-0032
            oC_RcvRedeem = oRcvRedeem;
            tC_Mode = ptMode;
            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            string tStaSentOnOff;
            try
            {
                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();

                // Sort  Group Function
                aoRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");
                for (int nRow = 0; nRow < aoRow.Length; nRow++)
                {
                    tUriApi = aoRow[nRow]["UrlApi"].ToString();
                    tUsrApi = aoRow[nRow]["UsrApi"].ToString();
                    tPwdApi = aoRow[nRow]["PwdApi"].ToString();

                    tWorkStationID = aoRow[nRow]["WorkStationID"].ToString();   //*Em 61-08-09 Com.Sheet ML-POSC-0031
                    tWorkStation = aoRow[nRow]["WorkStation"].ToString();     //*Em 61-08-09 Com.Sheet ML-POSC-0032

                    // Create Connection String Db
                    tConnDB = "Data Source=" + aoRow[nRow]["Server"].ToString();
                    tConnDB += "; Initial Catalog=" + aoRow[nRow]["DBName"].ToString();
                    tConnDB += "; User ID=" + aoRow[nRow]["User"].ToString() + "; Password=" + aoRow[nRow]["Password"].ToString();
                    tConnDB += "; Connection Timeout = 120";
                    // Check TPOSLogHis  Existing
                    var tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                    cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                   
                   var tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(aoRow[nRow]["TopRow"]), tWorkStationID, tWorkStation);  //*Em 61-08-09 Com.Sheet ML-POSC-0032

                  var  tExecute = cCNSP.SP_SQLtExecuteJson(tSQL, tConnDB);
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
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "REDEEM");
                    oRESMsg.tML_TimeSent = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"); //เก็บเวลาที่ส่ง ไว้ลงLog
                    oRESMsg.tML_UrlApi = tUriApi; //เก็บUrlApi ไว้ลงLog
                    if (tC_APIEnable == "true")
                    {
                        //Call API
                        var oConnectWebAPI = new cConnectWebAPI(tUriApi, tUsrApi, tPwdApi, oJson.ToString());
                        oRESMsg.tML_StatusCode = oConnectWebAPI.tC_StatusCode;

                        for (int nRow = 0; nRow < aoRow.Length; nRow++)
                        {
                            // Create Connection String Db
                            tConnDB = "Data Source=" + aoRow[nRow]["Server"].ToString();
                            tConnDB += "; Initial Catalog=" + aoRow[nRow]["DBName"].ToString();
                            tConnDB += "; User ID=" + aoRow[nRow]["User"].ToString() + "; Password=" + aoRow[nRow]["Password"].ToString();
                            tConnDB += "; Connection Timeout = 120";
                            #region "UPDATE FLAG TPSTSalHD.FTStaSentOnOff"
                            //----------------------------UPDATE FLAG TPSTSalHD.FTStaSentOnOff --------------------------------- 
                            if (ptMode.Equals("MANUAL"))
                            {
                                if (oRESMsg.tML_StatusCode == "200" || oRESMsg.tML_StatusCode == "202")
                                {
                                    var oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TPSTRPremium WITH (ROWLOCK)");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                                    //oSQL.AppendLine("   ,FTStaEOD = '1'");
                                    oSQL.AppendLine("   ,FTJsonFileName = '" + oRESMsg.tML_FileName + "'");
                                    //oSQL.AppendLine("WHERE FDRPDocDate = '" + Convert.ToDateTime(poRedeem.tML_RPDocDate).ToString("yyyy-MM-dd") + "'"); 
                                    oSQL.AppendLine("WHERE FDRPDocDate = '" + ptTransDate + "'");
                                    oSQL.AppendLine("AND FTPremiumNo = '" + poRedeem.tML_PremiumNo + "'");
                                    oSQL.AppendLine("AND FTPreMiumID = '" + poRedeem.tML_PremiumID + "'");
                                    oSQL.AppendLine("AND FTRPDocNo IN (" + poRedeem.tML_RPDocNo + ")");
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
                            else if (ptMode.Equals("AUTO"))
                            {
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

                                var oSQL = new StringBuilder();
                                oSQL.AppendLine("UPDATE TPSTRPremium WITH (ROWLOCK)");
                                oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                oSQL.AppendLine(" ,FTJsonFileName = '" + oRESMsg.tML_FileName + "'");
                                // oSQL.AppendLine("WHERE FDRPDocDate = '" + Convert.ToDateTime(poRedeem.tML_RPDocDate).ToString("yyyy-MM-dd") + "'");
                                oSQL.AppendLine("WHERE FDRPDocDate = '" + ptTransDate + "'");
                                var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
      
                            }
                            //----------------------------UPDATE FLAG TPSTSalHD.FTStaSentOnOff --------------------------------- 
                            #endregion
                        }


                        #region " Keep Log"
                        // cKeepLog.C_SETxKeepLogForReDeem(aoRow, oRESMsg);
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

        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100, string ptWorkStationID = "", string ptWorkStation = "")
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

                if (tC_Mode == "AUTO")
                {
                    oSQL.AppendLine("WHERE ISNULL(FTStaSentOnOff,'') ='' ");
                }

                else if (tC_Mode == "MANUAL")
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
            catch (Exception )
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
