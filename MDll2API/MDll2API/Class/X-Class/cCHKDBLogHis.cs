using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Class.X_Class
{
   public class cCHKDBLogHis
    {
        /// <summary>
        /// สร้าง Query check LogHis ให้สร้าง Table กรณียังไม่สร้าง
        /// </summary>
        /// <returns>string Query สร้างตรวจสอบ Table</returns>
        public string C_GETtCHKDBLogHis()
        {
            StringBuilder oSQL = new StringBuilder();
            try
            {
                oSQL.AppendLine("IF OBJECT_ID(N'dbo.TPOSLogHis', N'U') IS NOT NULL");
                oSQL.AppendLine("BEGIN ");
                oSQL.AppendLine(" CREATE TABLE TPOSLogHis(");
                oSQL.AppendLine(" FDDateUpd datetime,");
                oSQL.AppendLine(" FTTimeUpd varchar(8),");
                oSQL.AppendLine(" FTWhoUpd varchar(50),");
                oSQL.AppendLine(" FDDateIns datetime,");
                oSQL.AppendLine(" FTTimeIns varchar(8),");
                oSQL.AppendLine(" FTWhoIns varchar(50),");
                oSQL.AppendLine(" FTRemark varchar(100),");
                oSQL.AppendLine(" FTPlantCode varchar(10),");
                oSQL.AppendLine(" FDSendStartDateTime datetime,");
                oSQL.AppendLine(" FDSendEndDateTime datetime,");
                oSQL.AppendLine(" FTBatchNo varchar(20),");
                oSQL.AppendLine(" FTTransTypeGrp varchar(20),");
                oSQL.AppendLine(" FTRespCode varchar(20),");
                oSQL.AppendLine(" FTRespMsg varchar(100),");
                oSQL.AppendLine(" FTTransCount int ");
                oSQL.AppendLine(" PRIMARY KEY (FTBatchNo,FTTransTypeGrp)) ");
                oSQL.AppendLine("END");
                return oSQL.ToString();
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
    }
}
