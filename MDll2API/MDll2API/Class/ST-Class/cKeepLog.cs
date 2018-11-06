using MDll2API.Modale.POSLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MDll2API.Class.ST_Class
{
    public static class cKeepLog
    {
        public static void C_SETxKeepLogForSale(DataRow[] poRow, mlRESMsg poRESMsg)
        {
            string tConnDB;
            string tFunction = "3";
            try
            {
                #region " Keep Log"
                var dEnd = DateTime.Now;
                var dStart = DateTime.Now;
                for (int nRow = 0; nRow < poRow.Length; nRow++)
                {
                    // Create Connection String Db
                    tConnDB = "Data Source=" + poRow[nRow]["Server"].ToString();
                    tConnDB += "; Initial Catalog=" + poRow[nRow]["DBName"].ToString();
                    tConnDB += "; User ID=" + poRow[nRow]["User"].ToString() + "; Password=" + poRow[nRow]["Password"].ToString();

                    // Get Max FTBathNo Condition To Json

                    var tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);
                    // Keep Log
                    var oSql = new StringBuilder();
                    oSql.AppendLine("INSERT INTO TPOSLogHis(");
                    oSql.AppendLine("FDDateUpd, FTTimeUpd, FTWhoUpd, FDDateIns, FTTimeIns, FTWhoIns,");
                    oSql.AppendLine("FTRemark, FTShdPlantCode, FDSendStartDateTime, FDSendEndDateTime,");
                    oSql.AppendLine("FTBatchNo, FTTransTypeGrp, FTRespCode, FTRespMsg, FTTransCount)");
                    oSql.AppendLine("SELECT ");
                    oSql.AppendLine("CONVERT(VARCHAR(10), GETDATE(), 121) AS FDDateUpd, CONVERT(VARCHAR(10), GETDATE(), 108) AS FTTimeUpd,'System' AS FTWhoUpd,");
                    oSql.AppendLine("CONVERT(VARCHAR(10), GETDATE(), 121) AS FDDateIns, CONVERT(VARCHAR(10), GETDATE(), 108) AS FTTimeIns,'System' AS FTWhoIns,");
                    oSql.AppendLine("'' AS FTRemark, ISNULL(MAX(FTShdPlantCode), '') AS FTShdPlantCode,'" + string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dStart) + "' AS FDSendStartDateTime,");
                    oSql.AppendLine("  MAX(CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '')) AS FTBatchNo,'" + tFunction + "' AS FTTransTypeGrp,");
                    oSql.AppendLine("'" + poRESMsg.tML_StatusCode + "' AS FTRespCode, '" + poRESMsg.tML_StatusMsg + "' AS FTRespMsg, COUNT(FTShdTransNo) AS FTTransCount");
                    oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(poRow[nRow]["TopRow"]) + " * FROM TPSTSalHD with(nolock)");
                    oSql.AppendLine("    WHERE FTShdTransType IN('03', '04', '05','06', '10', '11', '14', '15', '21', '22', '23', '26', '27')");

                    if (tLastUpd != "")
                    {
                        oSql.AppendLine("    AND CONVERT(varchar(8), FDDateUpd, 112) + REPLACE(FTTimeUpd, ':', '') > '" + tLastUpd + "'");
                    }
                    oSql.AppendLine("    ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                    cCNSP.SP_SQLnExecute(oSql.ToString(), tConnDB);
                }
                #endregion
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }

        public static void C_SETxKeepLogForReDeem(DataRow[] poRow, mlRESMsg poRESMsg)
        {
            string tConnDB;
            string tFunction = "3";
            try
            {
                #region " Keep Log"
                var dEnd = DateTime.Now;
                var dStart = DateTime.Now;
                for (int nRow = 0; nRow < poRow.Length; nRow++)
                {
                    // Create Connection String Db
                    tConnDB = "Data Source=" + poRow[nRow]["Server"].ToString();
                    tConnDB += "; Initial Catalog=" + poRow[nRow]["DBName"].ToString();
                    tConnDB += "; User ID=" + poRow[nRow]["User"].ToString() + "; Password=" + poRow[nRow]["Password"].ToString();

                    // Get Max FTBathNo Condition To Json

                    var tLastUpd = cCNSP.SP_GETtMaxDateLogHis(tFunction, tConnDB);
                    // Keep Log
                    var oSql = new StringBuilder();
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
                    oSql.AppendLine("'" + poRESMsg.tML_StatusCode + "' AS FTRespCode, '" + poRESMsg.tML_StatusMsg + "' AS FTRespMsg, COUNT(FTShdTransNo) AS FTTransCount");
                    oSql.AppendLine("FROM (SELECT TOP " + Convert.ToInt64(poRow[nRow]["TopRow"]) + " * FROM TPSTRPremium with(nolock)");
                    if (tLastUpd != "")
                    {
                        oSql.AppendLine(" WHERE CONVERT(varchar(8),TPSTRPremium.FDDateUpd,112) + REPLACE(TPSTRPremium.FTTimeUpd,':','') >= '" + tLastUpd + "'");
                    }
                    oSql.AppendLine(" ORDER BY FDDateUpd, FTTimeUpd) TTmp");
                    cCNSP.SP_SQLnExecute(oSql.ToString(), tConnDB);
                }
                #endregion
            }
            catch (Exception oEx)
            {
                throw oEx;
            }

        }
    }
}
