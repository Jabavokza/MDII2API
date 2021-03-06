﻿using System;
using System.Net;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using MDll2API.Modale.ReceivApp;
using MDll2API.Modale.POSLog;
using MDll2API.Class.ST_Class;
using MDll2API.Class.X_Class;


namespace MDll2API.Class.POSLog
{
    public class cSale
    {
        mlRcvSale oC_RcvSale = new mlRcvSale();
        private string tC_Mode = "";
        private string tC_VenDor = "";
        private string tC_VenDes = "";
        private string tC_DepositCode = "";
        private string tC_DepositDes = "";
        private string tC_DateTrn = "";
        private string tC_APIEnable;
        public void CHKxAPIEnable(string ptAPIEnable)
        {
            tC_APIEnable = ptAPIEnable;
        }
        public mlRESMsg C_POSToSale(string ptMode, string ptTransDate, mlRcvSale poRcvSale, string ptVenDorCodeSale, string ptVenDes, string ptDepositCode, string ptDepositDes, string ptShdTransNo)
        {
            string tJsonTrn = "", tSQL = "", tExecute = "", tLastUpd = "", tUriApi = "", tUsrApi = "", tPwdApi = "";
            string tFunction = "3", tConnDB = "", tStaSentOnOff;
            DataTable oTblConfig;
            DataRow[] aoRow;
            Double cPointValue = 1;  //*Em 61-08-04

            cCHKDBLogHis oCHKDBLogHis = new cCHKDBLogHis();
            mlRESMsg oRESMsg = new mlRESMsg();
            mlPOSSale oPOSSale;
            try
            {
                oC_RcvSale = poRcvSale;
                tC_Mode = ptMode;
                tC_DateTrn = ptTransDate;
                tC_VenDor = ptVenDorCodeSale;
                tC_VenDes = ptVenDes;
                tC_DepositCode = ptDepositCode;
                tC_DepositDes = ptDepositDes;

                // load Config
                oTblConfig = cCNSP.SP_GEToConnDB();
                // Sort  Group Function
                aoRow = oTblConfig.Select("GroupIndex='" + tFunction + "'");

                for (int nRow = 0; nRow < aoRow.Length; nRow++)
                {
                    tUriApi = aoRow[nRow]["UrlApi"].ToString();
                    tUsrApi = aoRow[nRow]["UsrApi"].ToString();
                    tPwdApi = aoRow[nRow]["PwdApi"].ToString();
                    cPointValue = Double.Parse(aoRow[nRow]["PointValue"].ToString());  //*Em 61-08-04

                    if (cPointValue == 0)
                    {
                        cPointValue = 1;
                    }  //*Em 61-08-04
                    // Create Connection String Db
                    tConnDB = "Data Source=" + aoRow[nRow]["Server"].ToString();
                    tConnDB += "; Initial Catalog=" + aoRow[nRow]["DBName"].ToString();
                    tConnDB += "; User ID=" + aoRow[nRow]["User"].ToString() + "; Password=" + aoRow[nRow]["Password"].ToString();
                    tConnDB += "; Connection Timeout = 120";
                    // Check TPOSLogHis  Existing
                    tSQL = oCHKDBLogHis.C_GETtCHKDBLogHis();
                    cCNSP.SP_SQLnExecute(tSQL, tConnDB);

                    // Get Max FTBathNo Condition To Json
                    tLastUpd = "";
                    tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);

                    //  Condition ตาม FTBatchNo Get Json
                    //tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(oRow[nRow]["TopRow"]));
                    tSQL = C_GETtSQL(tLastUpd, Convert.ToInt64(aoRow[nRow]["TopRow"]), cPointValue); //*Em 61-08-04
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

                    oJson = oJson.Replace("amp;", ""); //คือ & เอาออก
                                                       // oJson = oJson.Replace("Description", "");
                    oRESMsg.tML_FileName = cCNSP.SP_WRItJSON(oJson.ToString(), "SALE");
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
                                    oRESMsg.tML_StatusMsg = "ส่งข้อมูลสมบูรณ์";
                                    var oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TPSTSalHD WITH (ROWLOCK)");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                                    //oSQL.AppendLine("   ,FTStaEOD = '1'");
                                    oSQL.AppendLine(" ,FTJsonFileName = '" + oRESMsg.tML_FileName + "'");
                                    oSQL.AppendLine("WHERE FTShdTransNo IN (" + ptShdTransNo + ")");
                                    var nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tConnDB);
                                }
                                else
                                {
                                    oRESMsg.tML_StatusMsg = "ส่งข้อมูลไม่สำเร็จ";
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
                                oPOSSale = new mlPOSSale();
                                oPOSSale = JsonConvert.DeserializeObject<mlPOSSale>(oJson.ToString());
                                for (int i = 0; i < oPOSSale.POSLog.aML_Transaction.Count; i++)
                                {
                                    var tTrnNo = oPOSSale.POSLog.aML_Transaction[i].SequenceNumber.Substring(oPOSSale.POSLog.aML_Transaction[i].SequenceNumber.Length - 10, 10);
                                    var oSQL = new StringBuilder();
                                    oSQL.AppendLine("UPDATE TPSTSalHD");
                                    oSQL.AppendLine("SET FTStaSentOnOff = '" + tStaSentOnOff + "'");
                                    oSQL.AppendLine(",FTJsonFileName ='" + oRESMsg.tML_FileName + "'");
                                    oSQL.AppendLine("WHERE  FTTmnNum+FTShdTransNo ='" + tTrnNo + "'");
                                    oSQL.AppendLine("AND FTShdPlantCode ='" + oPOSSale.POSLog.aML_Transaction[i].BusinessUnit.UnitID + "'");
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
                            //----------------------------UPDATE FLAG TPSTSalHD.FTStaSentOnOff --------------------------------- 
                            #endregion
                        }


                        #region " Keep Log"
                        // cKeepLog.C_SETxKeepLogForSale(aoRow, oRESMsg);
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
                oRESMsg.tML_StatusMsg = oEx.Message;
                return oRESMsg;
            }
        }
        private string C_GETtSQL(string ptLastUpd, Int64 pnRowTop = 100, Double pcPoint = 1)
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
                    catch (Exception oEx)
                    {
                        tPosLnkDB = "";
                    }
                }

                oSQL.AppendLine("SELECT ISNULL(STUFF((");
                oSQL.AppendLine("SELECT TOP " + pnRowTop + " ',{' + CHAR(10) +");
                oSQL.AppendLine("'\"BusinessUnit\":' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '{\"UnitID\":\"' + ISNULL(HD.FTShdPlantCode,'') + '\"},' + CHAR(10) +");
                oSQL.AppendLine("'\"WorkstationID\":\"' + HD.FTTmnNum + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"SequenceNumber\":\"' + CONVERT(VARCHAR(10), HD.FDShdTransDate, 112) + HD.FTTmnNum + HD.FTShdTransNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"OperatorID\":\"' + HD.FTEmpCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("'\"CurrencyCode\":\"THB\",' + CHAR(10) +");
                oSQL.AppendLine("'\"Reference\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + '\"LocationID\":\"' + ISNULL(FTTmnLocCode,'') + '\",' + CHAR(10) +");    //*Em 61-07-18
                oSQL.AppendLine("CHAR(9) + '\"LocationDescription\":\"' + ISNULL(FTTmnLocDesc,'') + '\"},' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("'\"RetailTransaction\": {' + CHAR(10) + ");


                oSQL.AppendLine("    CHAR(9) + CHAR(9) + '\"@ItemType\": \"' + ");
                oSQL.AppendLine("       CASE WHEN (SELECT '45' AS  FTdmCode FROM TPSTSalRC WHERE FTTdmCode='T008' AND FTShdTransType=HD.FTShdTransType AND FTShdTransNo =HD.FTShdTransNo AND FTTmnNum =HD.FTTmnNum AND FTSRVName=HD.FTSRVName AND FDShdTransDate =HD.FDShdTransDate  ) = '45'  THEN");
                oSQL.AppendLine("       '45'");
                oSQL.AppendLine("       WHEN ( HD.FTShdTransType = '07' and HD.FTShdStaBigLot = 'Y' )  THEN");
                oSQL.AppendLine("       '43'");
                //oSQL.AppendLine("       WHEN HD.FTShdTransType IN ('16') AND FTShdStaBigLot = 'Y' THEN");
                //oSQL.AppendLine("       '44'");
                oSQL.AppendLine("       ELSE HD.FTShdTransType");
                oSQL.AppendLine("       END");
                oSQL.AppendLine("+ '\", ' + CHAR(10) +");

                //  oSQL.AppendLine("CHAR(9) + '\"@ItemType\": \"' + ISNULL(HD.FTShdTransType, '') + '\",' + CHAR(10) + ");     //*Em 61-07-12

                oSQL.AppendLine("CHAR(9) + '\"LineItem\": [' + CHAR(10) + ");
                oSQL.AppendLine("ISNULL(");
                oSQL.AppendLine("   STUFF((");
                oSQL.AppendLine("   SELECT");
                oSQL.AppendLine("   ',{' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + FTData + CHAR(10) +");
                //oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\":\"' + CONVERT(VARCHAR, ROW_NUMBER() OVER(ORDER BY FTType)) + '\"' +");
                //*Em 61-08-09  Com.Sheet ML-POSC-0034
                oSQL.AppendLine("   (CASE WHEN FTLink = '' THEN");
                oSQL.AppendLine("   	CHAR(9) + CHAR(9) + CHAR(9) +'\"SequenceNumber\":\"' + CONVERT(VARCHAR,ROW_NUMBER() OVER (ORDER BY FTType)) + '\"'");
                oSQL.AppendLine("   ELSE");
                oSQL.AppendLine("   	CHAR(9) + CHAR(9) + CHAR(9) +'\"SequenceNumber\":\"' + CONVERT(VARCHAR,ROW_NUMBER() OVER (ORDER BY FTType)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("   	CHAR(9) + CHAR(9) + CHAR(9) +'\"ItemLink\":\"' + FTLink + '\"'");
                oSQL.AppendLine("   END) +");
                //*Em 61-08-09  Com.Sheet ML-POSC-0034
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + '}'");
                oSQL.AppendLine("   FROM(");
                oSQL.AppendLine("     SELECT");
                oSQL.AppendLine("   '1' AS FTType,");
                //oSQL.AppendLine("     (CHAR(9) + '\"Sale\":{' + CHAR(10) +");
                oSQL.AppendLine("     (CHAR(9) + '\"'+ " + tPosLnkDB + "TSysTransType.FTSttGrpName +'\":{' + CHAR(10) +");   //*Em 61-07-25
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"POSIdentity\": {' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@POSIDType\": \"' + 'EAN' + '\",' + CHAR(10) +");

                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"POSItemID\": \"' + (CASE WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' THEN '" + tC_DepositCode + "'   ELSE  FTSkuCode   END )  +'\"\' + CHAR(10) + ");

                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                // oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + FTSkuAbbName + '\",' + CHAR(10) + ");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + REPLACE(REPLACE(FTSkuAbbName,'\"','\\\"'),'''','\''') + '\",' + CHAR(10) + ");    //*Em 61-07-12
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + (CASE WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' THEN '" + tC_DepositDes + "'   ELSE   + REPLACE(REPLACE(FTSkuAbbName,'\"','\\\"'),'''','\''')    END ) +'\",\'  + CHAR(10) + ");  
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + (CASE WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' THEN '" + tC_DepositDes + "'   ELSE   + REPLACE(REPLACE(FTSkuAbbName,'\"','\"'),'''','\''')    END ) +'\",\'  + CHAR(10) + ");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"UnitListPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (CASE WHEN FTShdTransType = '11' THEN FCSdtB4DisChg ELSE  FCSdtRegPrice   END ))) + '\",' + CHAR(10) +"); [old]
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"RegularSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (CASE WHEN FTShdTransType = '11' THEN FCSdtB4DisChg ELSE  FCSdtRegPrice   END ))) + '\",' + CHAR(10) + "); [old]
                //oSQL.AppendLine("       CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"UnitListPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (CASE WHEN FTShdTransType IN ('11', '27') THEN Round(CAST(FCSdtB4DisChg AS decimal) - (CAST(FCSdtB4DisChg AS decimal) * CAST(FCSdtTax AS decimal) / (100 + CAST(FCSdtTax AS decimal))), 2) ELSE  FCSdtRegPrice   END ))) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("       CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"UnitListPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtRegPrice)) + '\",' + CHAR(10) + ");

                oSQL.AppendLine("       CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"RegularSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (CASE WHEN FTShdTransType IN ('11', '27') THEN Round(FCSdtB4DisChg  - ((FCSdtB4DisChg * FCSdtTax) / (100 + FCSdtTax)), 2)" +
                                        "WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' AND  HD.FTShdStaBigLot = 'N' THEN (FCSDTSaleAmt / FCSdtQty) " +
                                        "WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' AND  HD.FTShdStaBigLot = 'Y' THEN FCSdtSalePrice " +
                                        "ELSE FCSdtRegPrice END ))) + '\",' + CHAR(10) + ");

                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ActualSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), CASE WHEN FTShdTransType IN ('06', '16') AND ISNULL(FTSdtStaSalType, '') = '2' AND HD.FTShdStaBigLot = 'N' THEN (FCSDTSaleAmt / FCSdtQty) ELSE FCSdtSalePrice END)) + '\",' + CHAR(10) + ");   //*Em 61-07-12
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ActualSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (FCSDTSaleAmt / FCSdtQty))) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ExtendedAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtSaleAmt)) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Quantity\": {' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@UnitOfMeasureCode\": \"' + (CASE WHEN ISNULL(FTPunCode,'EA') = '' THEN 'EA' ELSE ISNULL(FTPunCode,'EA') END) + '\",' + CHAR(10) + ");  //*Em 61-07-18 
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR,(CASE WHEN " + tPosLnkDB + "TSysTransType.FTSttGrpName ='RETURN' THEN FCSdtQty *(-1) ELSE FCSdtQty END)) + '\"' + CHAR(10) + ");   //*Em 61-07-25
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Associate\": { \"AssociateID\": \"' + FTSdtSpnCode + '\" },' + CHAR(10) + ");


                oSQL.AppendLine("CASE WHEN FTShdTransType = '07' and FTShdStaBigLot = 'Y' then");
                //oSQL.AppendLine("(CASE WHEN  ISNULL(FCBdpOverShort, 0) > 0  THEN  ");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"RetailPriceModifier\": [' + CHAR(10) + ");
                oSQL.AppendLine("CHAR(9) + ISNULL( ");
                oSQL.AppendLine("STUFF((");
                oSQL.AppendLine("SELECT ',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + CONVERT(VARCHAR, ROW_NUMBER() OVER(PARTITION BY FNSdtSeqNo ORDER BY FNScdSeqNo)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"Subtract\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCScdAmt)) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionID\": \"' + FTScdBBYNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + FTScdBBYDesc + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionArea\": \"' + FTScdProArea + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionProfile\": \"' + FTScdBBYProfID + '\"' + CHAR(10) +");
              //  oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionTire \": \"' + FTScdTierID + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) + ''");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("(SELECT DTB.FNSdtSeqNo, DTB.FNSdtSeqNo as FNScdSeqNo, CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), (DTB.FCSdtRegPrice - FCSdtSalePrice) * DTB.FCsdtQty)) as FCScdAmt, HDB.FTShdBBYNo as FTScdBBYNo, HDB.FTShdBBYDesc as FTScdBBYDesc,");
                oSQL.AppendLine("(SELECT MAX(FTPmtAreCode)");
                oSQL.AppendLine("FROM " + tPosLnkDB + "TCNMPmtAre where FTPmtAreBBYNo = HDB.FTShdBBYNo) as FTScdProArea, HDB.FTShdBBYProfID as FTScdBBYProfID");
                oSQL.AppendLine("FROM TPSTSalDT DTB");
                oSQL.AppendLine("INNER JOIN TPSTSalHD HDB");
                oSQL.AppendLine("ON DTB.FTTmnNum = HDB.FTTmnNum AND DTB.FTShdTransNo = HDB.FTShdTransNo");
                oSQL.AppendLine("AND DTB.FDShdTransDate = HDB.FDShdTransDate  AND DTB.FTSRVName = HDB.FTSRVName");
                oSQL.AppendLine("AND DTB.FCsdtQty > 1");
                oSQL.AppendLine("AND HDB.FTShdStaBigLot = 'Y'");
                oSQL.AppendLine("AND DTB.FTTmnNum = TPSTSalDT.FTTmnNum AND DTB.FTShdTransNo = TPSTSalDT.FTShdTransNo");
                oSQL.AppendLine("AND DTB.FDShdTransDate = TPSTSalDT.FDShdTransDate AND DTB.FTSkuCode = TPSTSalDT.FTSkuCode AND DTB.FTSRVName = TPSTSalDT.FTSRVName");
                oSQL.AppendLine("AND DTB.FNSdtSeqNo = TPSTSalDT.FNSdtSeqNo");
                oSQL.AppendLine(") as xdata");
                oSQL.AppendLine("        FOR XML PATH('') ");
                oSQL.AppendLine("        ), 1, 1, ''), '') + CHAR(10) + ");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) ");

                oSQL.AppendLine("     WHEN ISNULL((SELECT TOP 1 FTShdTransNo FROM TPSTSalCD with(nolock) ");
                oSQL.AppendLine("     WHERE FTTmnNum = TPSTSalDT.FTTmnNum AND FTShdTransNo = TPSTSalDT.FTShdTransNo  AND TPSTSalDT.FTSRVName = TPSTSalCD.FTSRVName");
                oSQL.AppendLine("     AND FDShdTransDate = TPSTSalDT.FDShdTransDate AND FNSdtSeqNo = TPSTSalDT.FNSdtSeqNo), '') = ''");
                oSQL.AppendLine("     AND");
                oSQL.AppendLine("     ISNULL((SELECT TOP 1 FTShdTransNo FROM TPSTSalePoint with(nolock) ");
                oSQL.AppendLine("     WHERE FTTmnNum = TPSTSalDT.FTTmnNum AND FTShdTransNo = TPSTSalDT.FTShdTransNo  AND TPSTSalDT.FTSRVName = TPSTSalePoint.FTSRVName");
                oSQL.AppendLine("     AND FDShdTransDate = TPSTSalDT.FDShdTransDate AND FNSdtSeqNo = TPSTSalDT.FNSdtSeqNo), '') = ''  THEN '' ");

                oSQL.AppendLine("   ELSE");
                oSQL.AppendLine("   CASE WHEN (FTShdTransType NOT IN('07','06', '16') AND ISNULL(FTSdtStaSalType, '') <> '2') or (FTShdTransType IN('06', '16') AND ISNULL(FTSdtStaSalType, '') = '1') OR (FTShdTransType IN ('07') AND ISNULL(FTSdtStaSalType, '') = '2') THEN");
                //oSQL.AppendLine("(CASE WHEN  ISNULL(FCBdpOverShort, 0) > 0  THEN  ");
                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"RetailPriceModifier\": [' + CHAR(10) + ");
                oSQL.AppendLine("        CHAR(9) + ISNULL( ");
                oSQL.AppendLine("        STUFF((");

                oSQL.AppendLine("SELECT ',{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + CONVERT(VARCHAR, ROW_NUMBER() OVER(PARTITION BY FNSdtSeqNo ORDER BY FNSdtSeqNo)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"Subtract\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCScdAmt)) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionID\": \"' + FTScdBBYNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + FTScdBBYDesc + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionArea\": \"' + FTScdProArea + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionProfile\": \"' + FTScdBBYProfID + '\"' + CHAR(10) +");
               // oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionTire \": \"'+ FTScdTierID +'\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) + ''");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("(select FNSdtSeqNo, FNScdSeqNo, FCScdAmt, FTScdBBYNo, FTScdBBYDesc, FTScdProArea, FTScdBBYProfID");
                oSQL.AppendLine(" FROM TPSTSalCD with(nolock)");
                oSQL.AppendLine("WHERE FTTmnNum = TPSTSalDT.FTTmnNum AND FTShdTransNo = TPSTSalDT.FTShdTransNo");
                oSQL.AppendLine("AND FDShdTransDate = TPSTSalDT.FDShdTransDate AND FNSdtSeqNo = TPSTSalDT.FNSdtSeqNo  AND FTSRVName = TPSTSalDT.FTSRVName");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT dtp.FNSdtSeqNo, FTSpoID as FNScdSeqNo, CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSpoPoint * " + pcPoint + ")) as FCScdAmt, po.FTSpoBBYNo as FTScdBBYNo, po.FTSpoBBYDesc as FTScdBBYDesc, po.FTSpoProArea as FTScdProArea, po.FTSpoBBYProfID as FTScdBBYProfID");
                oSQL.AppendLine("FROM TPSTSalePoint po");
                oSQL.AppendLine("INNER JOIN TPSTSalDT dtp");
                oSQL.AppendLine("ON po.FTTmnNum = dtp.FTTmnNum AND po.FTShdTransNo = dtp.FTShdTransNo");
                oSQL.AppendLine("AND po.FDShdTransDate = dtp.FDShdTransDate AND po.FTSpoID = dtp.FTSkuCode  AND po.FTSRVName = dtp.FTSRVName");
                oSQL.AppendLine("AND po.FTTmnNum = TPSTSalDT.FTTmnNum AND po.FTShdTransNo = TPSTSalDT.FTShdTransNo");
                oSQL.AppendLine("AND po.FDShdTransDate = TPSTSalDT.FDShdTransDate AND po.FTSpoID = TPSTSalDT.FTSkuCode  AND po.FTSRVName = TPSTSalDT.FTSRVName");
                oSQL.AppendLine(") as xdata");
                oSQL.AppendLine("        FOR XML PATH('') ");
                oSQL.AppendLine("        ), 1, 1, ''), '') + CHAR(10) + ");

                oSQL.AppendLine("        CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '],' + CHAR(10) ");

                oSQL.AppendLine(" ELSE");
                oSQL.AppendLine(" ''");
                oSQL.AppendLine(" END");

                oSQL.AppendLine("     END + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Tax\": {' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TaxType\": \"VAT\",' + CHAR(10) + ");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + CONVERT(VARCHAR, FNSdtSeqNo) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + CONVERT(VARCHAR,ROW_NUMBER() OVER (PARTITION BY FNSdtSeqNo ORDER BY FNSdtSeqNo)) + '\",' + CHAR(10) + ");    //*Em 61-07-18
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxableAmount\": {' + CHAR(10) + ");
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TaxIncludedInTaxableAmountFlag\": \"' + 'true' + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TaxIncludedInTaxableAmountFlag\": \"' + 'false' + '\",' + CHAR(10) + ");  //*Em 61-07-19  Fixed false
                //oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtSaleAmt)) + '\"' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtVatable)) + '\"' + CHAR(10) + "); //*Em 61-07-12
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxablePercentage\": \"100.00\",' + CHAR(10) + ");   //*Em 61-07-19  Fixed 100
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSdtVat)) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Percent\": \"' + CONVERT(VARCHAR, FCSdtTax) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxRuleID\": \"' + (CASE FTTaxCode WHEN '00' THEN 'ON' WHEN '01' THEN 'O7' WHEN '02' THEN 'O0' END) + '\"' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SerialNumber\": \"' + FTSdtDisChgTxt + '\"' + ");
                oSQL.AppendLine("     CASE WHEN ISNULL(HD.FTShdDepReTransNo,'') = '' THEN '' ");
                oSQL.AppendLine("     ELSE ");
                oSQL.AppendLine("        CASE WHEN ISNULL((SELECT TOP 1 FTShdTransNo FROM TPSTSalHD with(nolock) ");
                //oSQL.AppendLine("        WHERE FTTmnNum = TPSTSalHD.FTTmnNum AND FTShdTransNo = TPSTSalHD.FTShdDepReTransNo) ,'') = '' THEN '' ");
                oSQL.AppendLine("        WHERE FTTmnNum = HD.FTShdDepRefTmnNum AND FTShdTransNo = HD.FTShdDepReTransNo AND FTShdTransType = '03' AND FTSRVName =HD.FTSRVName) ,'') = '' THEN '' ");   //*Em 61-07-25
                oSQL.AppendLine("        ELSE ");
                oSQL.AppendLine("           (SELECT TOP 1");
                oSQL.AppendLine("           ',' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TransactionLink\": {' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessUnit\": { \"UnitID\": \"' + ISNULL(HD.FTShdPlantCode,'') + '\" },' + CHAR(10) +");
                //oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"WorkstationID\": \"' + FTShdDepRefTmnNum + '\",' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"WorkstationID\": \"' + HD.FTShdDepRefTmnNum + '\",' + CHAR(10) +");  //*Em 61-08-04
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BusinessDayDate\": { \"Date\": \"' + CONVERT(VARCHAR(10), FDShdTransDate, 121) + '\" },' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"' + CONVERT(VARCHAR(10), FDShdTransDate, 112) + FTTmnNum + FTShdTransNo + '\"' + CHAR(10) +");
                oSQL.AppendLine("            CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10)");
                oSQL.AppendLine("            FROM TPSTSalHD with(nolock)");
                //oSQL.AppendLine("            WHERE FTTmnNum = TPSTSalHD.FTTmnNum AND FTShdTransNo = TPSTSalHD.FTShdDepReTransNo)");
                oSQL.AppendLine("            WHERE FTTmnNum = HD.FTShdDepRefTmnNum AND FTShdTransNo = HD.FTShdDepReTransNo AND FTShdTransType = '03' AND FTSRVName=HD.FTSRVName)"); //*Em 61-07-25
                oSQL.AppendLine("        END");
                oSQL.AppendLine("    END +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},') AS FTData");
                oSQL.AppendLine(",'' AS FTLink"); //*Em 61-08-09  Com.Sheet ML-POSC-0034
                oSQL.AppendLine("FROM TPSTSalDT with(nolock)");
                oSQL.AppendLine("WHERE FTTmnNum = HD.FTTmnNum AND FTShdTransNo = HD.FTShdTransNo AND FTSRVName=HD.FTSRVName");
                //oSQL.AppendLine("AND FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("AND FDShdTransDate = HD.FDShdTransDate  AND FCSdtQty > 0");    //*Em 61-07-12
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("	SELECT");
                oSQL.AppendLine("   '1' AS FTType,");
                oSQL.AppendLine("     (CHAR(9) + '\"'+ " + tPosLnkDB + "TSysTransType.FTSttGrpName + \'\":{' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"POSIdentity\": {' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@POSIDType\": \"' + 'EAN' + '\",' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"POSItemID\": \"' + '" + tC_VenDor + "' +'\"' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Description\": \"' + '" + tC_VenDes + "' + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"UnitListPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HD15.FCShdGrand)) + '\",\' + CHAR(10) +");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"RegularSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HD15.FCShdGrand)) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ActualSalesUnitPrice\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HD15.FCShdGrand)) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ExtendedAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HD15.FCShdGrand)) + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Quantity\": {' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@UnitOfMeasureCode\": \"' + 'EA' + '\",' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + '1' + '\"' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) + ");
                oSQL.AppendLine("     CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Associate\": { \"AssociateID\": \"''\" },' + CHAR(10) + ");

                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Tax\": {' + CHAR(10) + ");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TaxType\": \"VAT\",' + CHAR(10) + ");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SequenceNumber\": \"1\",' + CHAR(10) + ");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxableAmount\": {' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TaxIncludedInTaxableAmountFlag\": \"false\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"0.00\"' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxablePercentage\": \"100.00\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +  '\"Amount\": \"0.00\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Percent\": \"0\",' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"TaxRuleID\" : \"O0\" ' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SerialNumber\" : \"\"'  +");
                oSQL.AppendLine("   CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},') AS FTData");
                oSQL.AppendLine(",'' AS FTLink");
                oSQL.AppendLine("FROM TPSTSalHD HD15 with(nolock)");
                //oSQL.AppendLine("INNER JOIN TPSTSalDT DT15 with(nolock) ON DT15.FTTmnNum = HD15.FTTmnNum AND DT15.FTShdTransNo = HD15.FTTmnNum AND DT15.FDShdTransDate = HD15.FDShdTransDate");
                oSQL.AppendLine("WHERE HD15.FTTmnNum = HD.FTTmnNum AND HD15.FTShdTransNo = HD.FTShdTransNo AND HD15.FTSRVName=HD.FTSRVName");
                oSQL.AppendLine("AND HD15.FDShdTransDate = HD.FDShdTransDate  --AND HD15.FCShdGrand> 0");
                oSQL.AppendLine("and HD15.FTShdTransType IN ('15','21')");

                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("   '2' AS FTType,");
                oSQL.AppendLine("    (CASE FTTdmCode");
                oSQL.AppendLine("        WHEN 'T012' THEN");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + '\"Tender\": {' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"' + FTTdmCode + '\",' + CHAR(10) +");       //*Em 61-07-12
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@SequenceNumber\": \"' + CONVERT(VARCHAR, FNSrcSeqNo) + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt - (FCSrcChg+FCSrcRndMnyChg))) + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"ForeignCurrency\": {' + CHAR(10) +");
                //oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CurrencyCode\": \"' + FTRteCode + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CurrencyCode\": \"' + ISNULL(FTRteCode,'') + '\",' + CHAR(10) +"); //*Em 61-07-18
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"OriginalFaceAmount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt - (FCSrcChg+FCSrcRndMnyChg) )) + '\"' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Reference\": \"\"' + CHAR(10) +"); //*Em 61-07-25
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("        ELSE");
                oSQL.AppendLine("           CHAR(9) + '\"Tender\":{' + CHAR(10) +");
                //oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"' + FTTdmType + '\",' + CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"' + CASE WHEN FTTdmCode = 'T009' THEN 'T030' ELSE FTTdmCode END + '\",' + CHAR(10) +");        //*Em 61-07-12
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@SequenceNumber\": \"' + CONVERT(VARCHAR, FNSrcSeqNo) + '\",' + CHAR(10) +");
                oSQL.AppendLine("           (CASE FTTdmCode");
                oSQL.AppendLine("           WHEN 'T001' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\"'");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Amount\": {' + CHAR(10) + ");    //*Em 61-07-12
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Currency\": \"'+ (CASE WHEN ISNULL(FTRteCode,'') = '' THEN 'THB' ELSE FTRteCode END) +'\",' + CHAR(10) +");    //*Em 61-07-12
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCSrcNet)) +'\"' + CHAR(10) +");    //*Em 61-07-12
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");    //*Em 61-07-12
                oSQL.AppendLine("           WHEN 'T002' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditDebit\":{' + CHAR(10) +");
              //  oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardType\": \"' + (SELECT MAX(FTLNMCardTypeName) FROM "+ tPosLnkDB + "TLNKMapping WHERE LeFT(FTLNMRangeCard,8)= LEFT(FTSrcCardNo,8)) + '\",' + CHAR(10) +");// Kin 20 - 12 - 2018
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardClassification\": \"' + FTSrcCrdCls + '\",' + CHAR(10) +");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditCardCompany\": \"' + FTSrcRetDocRef + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditCardCompany\": \"' + FTSrcCrdName + '\",' + CHAR(10) +"); //*Kin 12-12-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PrimaryAccountNumber\": \"' + FTSrcCardNo + '\",' + CHAR(10) +");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardName\": \"' + FTSrcCrdName + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardName\": \"' + FTSrcRetDocRef + '\",' + CHAR(10) +"); //*Kin 12-12-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardBrand\": \"' + FTSrcCrdBrd + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Platinum\": \"' + FTSrcStaPlat + '\",'  + CHAR(10) +");

                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BonusBuyID\": \"' + FTSrcBBYNo + '\"' + CHAR(10) +");

                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("           WHEN 'T003' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditDebit\":{' + CHAR(10) +");
              //  oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardType\": \"' + (SELECT MAX(FTLNMCardTypeName) FROM " + tPosLnkDB + "TLNKMapping WHERE LeFT(FTLNMRangeCard,8)= LEFT(FTSrcCardNo,8)) + '\",' + CHAR(10) +");// Kin 20 - 12 - 2018
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardClassification\": \"' + FTSrcCrdCls + '\",' + CHAR(10) +");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditCardCompany\": \"' + FTSrcRetDocRef + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CreditCardCompany\": \"' + FTSrcCrdName + '\",' + CHAR(10) +"); //*Kin 12-12-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PrimaryAccountNumber\": \"' + FTSrcCardNo + '\",' + CHAR(10) +");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardName\": \"' + FTSrcCrdName + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardName\": \"' + FTSrcRetDocRef + '\",' + CHAR(10) +"); //*Kin 12-12-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CardBrand\": \"' + FTSrcCrdBrd + '\",' + CHAR(10) +");
                oSQL.AppendLine("              CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Platinum\": \"' + FTSrcStaPlat + '\",' + CHAR(10) +");

                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"BonusBuyID\": \"' + FTSrcBBYNo + '\"' + CHAR(10) +");

                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("           WHEN 'T004' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                ////oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) + ");    //*Em 61-07-25
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Currency\": \"'+ (CASE WHEN ISNULL(FTRteCode,'') = '' THEN 'THB' ELSE FTRteCode END) +'\",' + CHAR(10) +");    //*Em 61-07-25
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"'+ CONVERT(VARCHAR,CONVERT(DECIMAL(18,2),FCSrcNet)) +'\"' + CHAR(10) +");    //*Em 61-07-25
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'  + CHAR(10) +");    //*Em 61-07-25
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Voucher\":{' + CHAR(10) +");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TypeCode\": \"' + 'Gift Voucher' + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"SerialNumber\": \"' + FTSrcCardNo + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"IssuingStoreNumberID\": \"' + ISNULL(FTShdPlantCode,'') + '\"' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("           WHEN 'T005' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Coupon\":{' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionCode\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CouponMaterial\": \"' + FTSrcCardNo + '\"' + CHAR(10) +");
                oSQL.AppendLine("              CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("           WHEN 'T006' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");   //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Coupon\":{' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionCode\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CouponMaterial\": \"' + FTSrcCardNo + '\"' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("            WHEN 'T007' THEN");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Coupon\":{' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"PromotionCode\": \"' + '' + '\",' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"CouponMaterial\": \"' + FTSrcCardNo + '\"' + CHAR(10) +");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("           ELSE");
                //oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcAmt)) + '\"'");
                oSQL.AppendLine("               CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSrcNet)) + '\",'");    //*Em 61-07-18
                oSQL.AppendLine("           END) +CHAR(10) +");
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Reference\": \"\"' + CHAR(10) +"); //*Em 61-07-25
                oSQL.AppendLine("           CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine("   END) AS FTData");
                oSQL.AppendLine(",'' AS FTLink"); //*Em 61-08-09  Com.Sheet ML-POSC-0034
                oSQL.AppendLine("FROM TPSTSalRC with(nolock)");
                oSQL.AppendLine("WHERE FTTmnNum = HD.FTTmnNum AND FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("AND FTShdPlantCode = HD.FTShdPlantCode");
                oSQL.AppendLine("AND FTSRVName = HD.FTSRVName");
                oSQL.AppendLine("AND FCSrcNet<> 0");


                //*Em 61-08-06  Rounding ให้ถือว่าเป็น Tender อย่างหนึ่ง ++++++++++++
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '3' AS FTType,");
                oSQL.AppendLine("CASE WHEN CONVERT(DECIMAL(18,2),HD.FCShdRnd) = CONVERT(DECIMAL(18, 2), 0) THEN ''");
                oSQL.AppendLine("ELSE");
                oSQL.AppendLine("(CHAR(9) + '\"Tender\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"'+'T032'+'\",' + CHAR(10) +");     //*Em 61-08-04
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@SequenceNumber\": \"' + CONVERT(VARCHAR, ISNULL((SELECT MAX(FNSrcSeqNo) AS FNSrcSeqNo FROM TPSTSalRC with(nolock) WHERE FTTmnNum = HD.FTTmnNum AND FTShdTransNo = HD.FTShdTransNo AND FDShdTransDate = HD.FDShdTransDate AND FTSRVName=HD.FTSRVName),0)+1) + '\",' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Currency\": \"THB\",' + CHAR(10) +");
                //oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Action\": \"Subtract\",' + CHAR(10) +");      '*Em 61-08-17  เอาออก
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' +   CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HD.FCShdRnd)  * (-1))  + '\"' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) + '') END AS FTData");
                oSQL.AppendLine(",'' AS FTLink"); //*Em 61-08-09  Com.Sheet ML-POSC-0034

                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT");
                oSQL.AppendLine("'2' AS FTType,");
                oSQL.AppendLine("(");
                oSQL.AppendLine("CHAR(9) + '\"Tender\":{' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@TenderType\": \"T029\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@SequenceNumber\": \"' + CASE WHEN  HDB.FCShdRnd <> 0 THEN '2' else '1' END + '\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"Amount\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Currency\": \"THB\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"' + CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), HDB.FCShdGrand)) + '\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '}' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},'");
                oSQL.AppendLine(") AS FTData,''");
                oSQL.AppendLine("FROM TPSTSalHD HDB with(nolock)");
                oSQL.AppendLine("WHERE HDB.FTTmnNum = HD.FTTmnNum AND HDB.FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND HDB.FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("AND HDB.FTSRVName = HD.FTSRVName");
                oSQL.AppendLine("AND HDB.FTShdTransType = '07'");

                //*Em 61-08-09  Com.Sheet ML-POSC-0034
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '4' AS FTType, ");
                oSQL.AppendLine("(CHAR(9) + '\"LoyaltyReward\": {' + CHAR(10) +");
                oSQL.AppendLine(" CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"SequenceNumber\": \"'+ CONVERT(VARCHAR,ROW_NUMBER() OVER (ORDER BY Pnt.FTTmnNum,Pnt.FTShdTransNo)) +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"LoyaltyID\": \"'+FTSpoMemID+'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"LoyaltyProgramID\": \"' + 'Extra' +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"CustomerName\": \"'+ FTCstPointName +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"ExpiryDate\": \"'+'9999-12-31'+'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"PointsAwarded\": {'+ CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Type\": \"PointsEarned\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"'+ CONVERT(VARCHAR,FCSpoPoint) +'\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR,FCSpoPoint*(-1)) +'\"}' + CHAR(10) +");
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSpoPoint * " + pcPoint + ")) +'\"}' + CHAR(10) +");  //2018-08-29 NAUY
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSpoAmt )) +'\"}' + CHAR(10) +"); //Kin 12-12-2018
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},') AS FTData");
                oSQL.AppendLine(",CONVERT(VARCHAR,ISNULL(DT.FNSdtSeqNo,'')) AS FTLink");
                oSQL.AppendLine("FROM TPSTSalePoint Pnt with(nolock)");
                oSQL.AppendLine("LEFT JOIN TPSTSalDT DT with(nolock) ON Pnt.FTTmnNum = DT.FTTmnNum AND Pnt.FTShdTransNo = DT.FTShdTransNo AND Pnt.FTSpoID = DT.FTSkuCode");
                oSQL.AppendLine("WHERE Pnt.FTTmnNum = HD.FTTmnNum AND Pnt.FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND Pnt.FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("AND Pnt.FTSRVName = HD.FTSRVName");
                oSQL.AppendLine("AND FTSpoType IN('1','6','7')");
                oSQL.AppendLine("UNION ALL");
                oSQL.AppendLine("SELECT '5' AS FTType,");
                oSQL.AppendLine("(CHAR(9) + '\"LoyaltyReward\": {' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"SequenceNumber\": \"'+ CONVERT(VARCHAR,ROW_NUMBER() OVER (PARTITION BY FTTmnNum,FDShdTransDate,FTShdTransNo,FTSpoMemID,HD.FTCstPointName ORDER BY FTTmnNum)) +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"LoyaltyID\": \"'+FTSpoMemID+'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"LoyaltyProgramID\": \"'+ 'Basic' +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"CustomerName\": \"'+ HD.FTCstPointName +'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"ExpiryDate\": \"'+'9999-12-31'+'\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"PointsAwarded\": {'+ CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"@Type\": \"PointsEarned\",' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '\"$\": \"'+ CONVERT(VARCHAR,FCSpoPoint) +'\"' + CHAR(10) +");
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},' + CHAR(10) +");
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR,FCSpoPoint*(-1)) +'\"}'  + CHAR(10) +");
                //oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSpoPoint  * " + pcPoint + ")) +'\"}'  + CHAR(10) +"); //2018-08-29 NAUY
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) +'\"Discount\": {\"Amount\":\"'+ CONVERT(VARCHAR, CONVERT(DECIMAL(18, 2), FCSpoAmt )) +'\"}' + CHAR(10) +"); //Kin 12-12-2018
                oSQL.AppendLine("CHAR(9) + CHAR(9) + CHAR(9) + CHAR(9) + '},') AS FTData");
                oSQL.AppendLine(",'' AS FTLink");
                oSQL.AppendLine("FROM");
                oSQL.AppendLine("( SELECT FTTmnNum,FDShdTransDate,FTShdTransNo,FTSpoMemID,SUM(FCSpoAmt)AS FCSpoAmt,SUM(FCSpoPoint) AS FCSpoPoint");
                oSQL.AppendLine("FROM TPSTSalePoint with(nolock)");
                oSQL.AppendLine("WHERE FTTmnNum = HD.FTTmnNum AND FTShdTransNo = HD.FTShdTransNo");
                oSQL.AppendLine("AND FDShdTransDate = HD.FDShdTransDate");
                oSQL.AppendLine("AND FTSRVName = HD.FTSRVName");
                oSQL.AppendLine("AND FTSpoType IN('2','3','4','5')");
                oSQL.AppendLine("GROUP BY FTTmnNum,FDShdTransDate,FTShdTransNo,FTSpoMemID) TTmpPoint");

                oSQL.AppendLine(" ) tmp");
                oSQL.AppendLine("WHERE ISNULL(FTData, '') <> ''");
                oSQL.AppendLine("  FOR XML PATH('')");
                oSQL.AppendLine("  ),1,1,'') ,'') +CHAR(10) +");
                oSQL.AppendLine("  CHAR(9) + CHAR(9) + '],' + CHAR(10) +");
                //oSQL.AppendLine("   CHAR(9) + '\"Customer\": { \"CustomerID\": \"' + FTCstCode + '\" }' + CHAR(10) +");
                oSQL.AppendLine("   CHAR(9) + '\"Customer\": { \"CustomerID\": \"' + ISNULL((SELECT FTSrcCardNo FROM TPSTSalRC WHERE FTTdmCode = 'T008' AND FTShdTransNo = HD.FTShdTransNo AND FDShdTransDate = HD.FDShdTransDate AND FTTmnNum = HD.FTTmnNum AND FTSRVName=HD.FTSRVName),'') + '\" }' + CHAR(10) +");  //*Em 61-07-12
                oSQL.AppendLine("   CHAR(9) + '},' +");
                oSQL.AppendLine("'\"BusinessDayDate\": \"' + CONVERT(VARCHAR(10),HD.FDShdTransDate,121) + '\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"BeginDateTime\": \"'+ CONVERT(VARCHAR,TmpHD.FDBeginDate,127) +'\",' + CHAR(10) +");
                //oSQL.AppendLine("'\"EndDateTime\": \"'+ CONVERT(VARCHAR,TmpHD.FDEndDate,127) +'\"' + CHAR(10) +");
                //oSQL.AppendLine("'\"BeginDateTime\": \"'+ TmpHD.FDBeginDate +'\",' + CHAR(10) +");  //*Em 61-07-12
                //oSQL.AppendLine("'\"EndDateTime\": \"'+ TmpHD.FDEndDate +'\"' + CHAR(10) +");   //*Em 61-07-12
                oSQL.AppendLine("'\"BeginDateTime\": \"'+ CONVERT(VARCHAR(8),HD.FDDateIns,112)+REPLACE(HD.FTTimeIns,':','') +'\",' + CHAR(10) +");  //*Em 61-07-18
                oSQL.AppendLine("'\"EndDateTime\": \"'+ CONVERT(VARCHAR(8),HD.FDDateUpd,112)+REPLACE(HD.FTTimeUpd,':','') +'\"' + CHAR(10) +");   //*Em 61-07-18
                oSQL.AppendLine("'}'");
                oSQL.AppendLine("FROM TPSTSalHD HD with(nolock)");
                //oSQL.AppendLine("LEFT JOIN TSysTransType with(nolock) ON HD.FTShdTransType = TSysTransType.FTSttTranCode");
                oSQL.AppendLine("INNER JOIN " + tPosLnkDB + "TSysTransType with(nolock) ON HD.FTShdTransType = " + tPosLnkDB + " TSysTransType.FTSttTranCode AND ISNULL(TSysTransType.FTSttGrpName,'') <> ''");    //*Em 61-08-06
                //oSQL.AppendLine("INNER JOIN (SELECT FTShdPlantCode,FDShdTransDate,MIN(FDShdSysDate+FTShdSysTime) AS FDBeginDate , MAX(FDShdSysDate+FTShdSysTime) AS FDEndDate");
                oSQL.AppendLine("INNER JOIN (SELECT FTShdPlantCode,FDShdTransDate,MIN(CONVERT(VARCHAR(8),FDShdSysDate,112)+REPLACE(FTShdSysTime,':','')) AS FDBeginDate , MAX(CONVERT(VARCHAR(8),FDShdSysDate,112)+REPLACE(FTShdSysTime,':','')) AS FDEndDate");    //*Em 61-07-12
                oSQL.AppendLine("	FROM TPSTSalHD");
                oSQL.AppendLine("	WHERE FTShdTransType IN('03','04','05','06','07','10','11','14','15','16','21','22','23','26','27')");

                if (tC_Mode == "AUTO")
                {
                    oSQL.AppendLine("   AND ISNULL(FTStaSentOnOff, '') = ''");
                }

                oSQL.AppendLine("   AND FCShdGrand > 0 ");
                oSQL.AppendLine("	GROUP BY FTShdPlantCode,FDShdTransDate ");
                oSQL.AppendLine("	) TmpHD ON ISNULL(HD.FTShdPlantCode,'') = ISNULL(TmpHD.FTShdPlantCode,'') AND HD.FDShdTransDate = TmpHD.FDShdTransDate");

                if (tC_Mode == "AUTO")
                {
                    oSQL.AppendLine("WHERE ISNULL(HD.FTStaSentOnOff, '0') <> '1' AND HD.FCShdGrand > 0");
                }
                else if (tC_Mode == "MANUAL")
                {
                    oSQL.AppendLine("WHERE HD.FCShdGrand > 0 AND " + oC_RcvSale.Field + oC_RcvSale.Value + " ");
                }

                oSQL.AppendLine("ORDER BY HD.FTShdPlantCode,HD.FDShdTransDate");    //*Em 61-08-04
                oSQL.AppendLine("FOR XML PATH ('')");
                oSQL.AppendLine("),1,1,''),'')");
                oSQL.AppendLine("FOR XML PATH ('')");

                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            finally
            {
                oSQL = null;
                rtResult = null;
            }
        }
    }
}
