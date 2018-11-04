using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace MDll2API.Class.ST_Class
{
   public static class cCNSP
    {
        public static string GETtVertionDll()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        /// <summary>
        /// สำหรับสร้าง Text ประกอบโครงสร้าง Json
        /// </summary>
        /// <param name="ptQuery">Query สร้าง Json</param>
        /// <param name="ptConnDB">Connnection String Databass</param>
        /// <returns></returns>
        public static string SP_SQLtExecuteJson(string ptQuery, string ptConnDB)
        {
            SqlCommand oCmdSql = new SqlCommand();
            SqlConnection oDbCon = new SqlConnection(ptConnDB);
            SqlDataReader oDbRed;
            string rtResult = "";
            try
            {
                oDbCon.Open();
                oCmdSql.Connection = oDbCon;
                oCmdSql.CommandText = ptQuery;
                oDbRed = oCmdSql.ExecuteReader();
                if (oDbRed.HasRows)
                {
                    while (oDbRed.Read()) {
                        rtResult = rtResult + oDbRed[0].ToString();
                    }
                }
                oCmdSql.Dispose();
                oDbCon.Dispose();
                return rtResult;
            }
            catch (Exception oEx)
            {
                return rtResult;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Execute คำสั่ง SQL
        /// </summary>
        /// <param name="ptQuery"></param>
        /// <param name="ptConnDB"></param>
        /// <returns>Int ตามสถานะ Rows</returns>
        public static int SP_SQLnExecute(string ptQuery, string ptConnDB)
        {
            SqlCommand oCmdSql = new SqlCommand();
            SqlConnection oDbCon = new SqlConnection(ptConnDB);
            int rnResult = 0;
            try
            {
                oDbCon.Open();
                oCmdSql.Connection = oDbCon;
                oCmdSql.CommandText = ptQuery;
                rnResult = oCmdSql.ExecuteNonQuery();
                return rnResult;
            }
            catch (Exception oEx)
            {
                return rnResult;
            }
            finally
            {
            }
        }

        public static string SP_WRItJSON(string ptJson,string ptType)
        {
            string tName1 = "";
            try
            {
                // SALE_YYYY - MM - DD:HH: mm: ss

                string tName = "";
          
                if (ptType == "SALE")
                {
                    tName = "SALE_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                else if (ptType == "BANK")
                {
                    tName = "BANKIN_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                else if (ptType == "REDEEM")
                {
                    tName = "REDEEM_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                else if (ptType == "EOD")
                {
                    tName = "DAYSUMARY_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                else if (ptType == "EDC")
                {
                    tName = "EDC_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                else if (ptType == "CASH")
                {
                    tName = "SHORTOVER_" + SP_DTEtByFormat(DateTime.Now.ToString(), "YYYYMMDDHHMMSS");
                }
                string tPathLocal = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\JsonFile\\"+ tName +".json";
                if (ptJson!="")
                {
                    File.WriteAllText(tPathLocal, ptJson);
                    tName1 = tName + ".json";
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show("wAckConfig3:WRIxJSON = " + ex.Message, "Config Inbound", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tName1;
        }

        /// <summary>
        /// Load Config XML เข้า Datatable
        /// </summary>
        /// <returns>โครงสร้าง Config XML ตามรูปแบบ Datatable</returns>
        public static DataTable SP_GEToConnDB()
        {
            XmlReader oXmlFile;
            DataSet oDs = new DataSet();
            DataTable oTbl = new DataTable();
            try
            {
                oXmlFile = XmlReader.Create(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\dbconfig.xml", new XmlReaderSettings());
                oDs.ReadXml(oXmlFile);
                oTbl = oDs.Tables[0];
                return oTbl;
            }
            catch (Exception oEx)
            {
                return null;
            }
            finally
            {
                oXmlFile = null;
                oDs = null;
                oTbl = null;
            }
        }


        /// <summary>
        /// โหลดข้อมูล  POSCenter เพื่อใช้หา ข้อมูล Master
        /// </summary>
        /// <returns>โครงสร้าง Config XML ตามรูปแบบ Datatable</returns>
        public static DataTable SP_GEToPosCnt()
        {
            XmlReader oXmlFile;
            DataSet oDs = new DataSet();
            DataTable oTbl = new DataTable();
            try
            {
                oXmlFile = XmlReader.Create(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\dbconfig.xml", new XmlReaderSettings());
                oDs.ReadXml(oXmlFile);
                oTbl = oDs.Tables[1]; //POSCENTER
                return oTbl;
            }
            catch (Exception oEx)
            {
                return null;
            }
            finally
            {
                oXmlFile = null;
                oDs = null;
                oTbl = null;
            }
        }

        /// <summary>
        /// สร้าง Query check LogHis ให้สร้าง Table กรณียังไม่สร้าง
        /// </summary>
        /// <returns>string Query สร้างตรวจสอบ Table</returns>
        public static string SP_GETtCHKDBLogHis()
        {
            string rtResult = "";
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
                rtResult = oSQL.ToString();
                return rtResult;
            }
            catch (Exception oEx)
            {
                return null;
            }
            finally
            {
                rtResult = null;
                oSQL = null;
            }
        }

        /// <summary>
        /// Get Max date Loghis
        /// </summary>
        /// <param name="ptType">Type Value ที่ต้องการ Return ตาม Function</param>
        /// <param name="ptConnDB">String Connect</param>
        /// <returns>string ค่า Max Date</returns>
        public static string SP_GETtMaxDateLogHis(string ptType,string ptConnDB)
        {
            string rtResult = "";
            StringBuilder oSQL = new StringBuilder();
            string[] atResult;
            try
            {
                oSQL.AppendLine("SELECT MAX(FTBatchNo) FROM TPOSLogHis ");
                oSQL.AppendLine("WHERE FTTransTypeGrp='" + ptType + "' AND FTRespCode='200'");
                atResult = SP_GETtExecuteScalarSQL(oSQL.ToString(), ptConnDB,"T").Split('|');
                if (atResult[1] == "1") {
                    rtResult = atResult[0];
                }
                return rtResult;
            }
            catch (Exception oEx)
            {
                return null;
            }
            finally
            {
                rtResult = null;
                oSQL = null;
            }
        }

        /// <summary>
        /// Get Value ExecuteScalar
        /// </summary>
        /// <param name="ptSql">คำสั่ง SQL</param>
        /// <param name="ptConnDB">Connection String</param>
        /// <param name="ptTypeVal">Type Value ที่ต้องการ Return T:string,N;Int,C:Double</param>
        /// <returns>สถานะ|value|Description</returns>
        public static string SP_GETtExecuteScalarSQL(string ptSql, string ptConnDB,string ptTypeVal)
        {
            SqlConnection oConn = new SqlConnection();
            SqlCommand oCmd = new SqlCommand();
            string tResult = "";
            string tValue = "";
            int nValue = 0;
            double cValue = 0;
            try
            {
                try
                {
                    if (oConn.State == ConnectionState.Open)
                    {
                        oConn.Close();
                        oConn.ConnectionString = ptConnDB;
                        oConn.Open();
                    }
                    else
                    {
                        oConn.ConnectionString = ptConnDB;
                        oConn.Open();
                    }
                }
                catch (Exception tErr)
                {
                    tResult = "|3|" + tErr.Message;
                }
                oCmd = new SqlCommand(ptSql, oConn);
                switch (ptTypeVal.ToUpper())
                {
                    case "T":
                        tValue = Convert.ToString(oCmd.ExecuteScalar());
                        tResult = tValue + "|1|Success";
                        break;
                    case "N":
                        nValue = Convert.ToInt32(oCmd.ExecuteScalar());
                        tResult = nValue + "|1|Success";
                        break;
                    case "C":
                        cValue = Convert.ToDouble(oCmd.ExecuteScalar());
                        tResult = cValue + "|1|Success";
                        break;
                }
            }
            catch (Exception tErr)
            {
                tResult = "|0|" + tErr.Message;
            }
            finally
            {
                if (oConn.State == ConnectionState.Open)
                {
                    oConn.Close();
                    oConn = null;
                    oCmd = null;
                }
            }
            return tResult;
        }
        public static string SP_DTEtByFormat(string ptDate, string ptFormat)
        {
            string tRet = "";
            DateTime odt = Convert.ToDateTime(ptDate);
            switch (ptFormat.ToUpper())
            {
                case "DD/MM/YYYY":
                    tRet = int.Parse(odt.Day.ToString()).ToString("00") + "/" + odt.Month + "/" + odt.Year;
                    break;
                case "YYYY/MM/DD":
                    tRet = odt.Year + "/" + odt.Month + "/" + int.Parse(odt.Day.ToString()).ToString("00");
                    break;
                case "YYMMDD":
                    tRet = "" + odt.Year + odt.Month + int.Parse(odt.Day.ToString()).ToString("00");
                    break;
                case "YYYYMM":
                    tRet = "" + odt.Year + odt.Month;
                    break;
                case "YYYYMMDD":
                    tRet = "" + odt.Year + int.Parse(odt.Month.ToString()).ToString("00") + int.Parse(odt.Day.ToString()).ToString("00");
                    break;
                case "YYYY-MM-DD":
                    //   { 02 / 11 / 2016 0:00:00}
                    //   tRet = odt.Year + "-" + odt.Month + "-" + int.Parse(odt.Day.ToString()).ToString("00");
                    tRet = odt.Year + "-" + int.Parse(odt.Month.ToString()).ToString("00") + "-" + int.Parse(odt.Day.ToString()).ToString("00");
                    break;
                case "HH:MM:SS":
                    // tRet = odt.Hour + ":" + odt.Minute + ":" + odt.Second;
                    tRet = int.Parse(odt.Hour.ToString()).ToString("00") + ":" + int.Parse(odt.Minute.ToString()).ToString("00") + ":" + int.Parse(odt.Second.ToString()).ToString("00");
                    break;
                case "YYYY/MM/DD HH:MM:SS":
                    tRet = odt.Year + "/" + odt.Month + "/" + int.Parse(odt.Day.ToString()).ToString("00") + " " + odt.Hour + ":" + odt.Minute + ":" + odt.Second;
                    break;
                case "YYYYMMDDHHMMSS":
                    //ใช้ตั้งชื่อไฟล์
                    //tRet = odt.Year.ToString() + odt.Month.ToString() + int.Parse(odt.Day.ToString()).ToString("00") + odt.Hour.ToString() + odt.Minute.ToString() + odt.Second.ToString();
                    tRet = odt.Year.ToString() + int.Parse(odt.Month.ToString()).ToString("00") + int.Parse(odt.Day.ToString()).ToString("00") + int.Parse(odt.Hour.ToString()).ToString("00") + int.Parse(odt.Minute.ToString()).ToString("00") + int.Parse(odt.Second.ToString()).ToString("00");
                    break;
                case "YYYYMMDDHHMM":
                    //ใช้ตั้งชื่อไฟล์
                    tRet = odt.Year.ToString() + odt.Month.ToString() + int.Parse(odt.Day.ToString()).ToString("00") + odt.Hour.ToString() + odt.Minute.ToString();
                    break;
                default:
                    tRet = ptDate;
                    break;
            }
            return tRet;
        }

        public static void SP_ADDxLog(string ptLogMsg)
        {
            // Call SP_ADDxLog(tVB_LogPath & "\Error" & Format(Date.Now, "yyyyMMdd"), Format(Date.Now, "HH:mm:ss") & "XXX") 'เรียกใช้
            // SP_WRITExLog(ptPath & ".txt", ptLog)
            SP_WRITExLog(SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY/MM/DD HH:MM:SS") + ":MsgLog:" + ptLogMsg);
        }

        public static void SP_WRITExLog(string ptLog)
        {
            string tPath = Directory.GetCurrentDirectory();
            string tName = SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY-MM-DD");
            tName = tName + "_SAPMInterface";
            tPath = tPath + "\\Log\\" + tName + ".txt";
            try
            {
                if (!(File.Exists(tPath)))
                {
                    using (StreamWriter oSw = File.CreateText(tPath))
                    {
                        oSw.Close();
                    }
                }
                using (StreamWriter oSw = File.AppendText(tPath))
                {
                    var _with1 = oSw;
                    _with1.WriteLine(ptLog);
                    _with1.Flush();
                    _with1.Close();
                }
            }
            catch (Exception ex)
            {

                // SP_ADDxLog("mCNSP:SP_WRITExLog:" + ex.Message);
            }
        }

        public static DataTable SP_SQLvExecute(string ptQeury, string tConStr)
        {
            SqlDataAdapter oDbAdt = null;
            DataSet oDbDtsSet = null;
            DataTable oDbTblDt = new DataTable();
            //  SP_SETxConClient();
            try
            {
                SqlConnection ocnForm = new SqlConnection(tConStr);
                {
                    ocnForm.Open();
                    oDbAdt = new SqlDataAdapter(ptQeury, ocnForm);
                    oDbDtsSet = new DataSet();
                    oDbAdt.Fill(oDbDtsSet, "TEMP");
                    oDbTblDt = oDbDtsSet.Tables["TEMP"];

                    ocnForm.Dispose();
                    oDbAdt.Dispose();
                    oDbDtsSet.Dispose();

                    return oDbTblDt;
                }
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            finally
            {
                if (oDbAdt != null) { oDbAdt.Dispose(); }
                if (oDbDtsSet != null) { oDbDtsSet.Dispose(); }
            }
        }

        public static DataTable SP_SQLvFillTable(string ptQeury, DataTable poDt)
        {
            SqlDataAdapter oDbAdt = null;
            //  SP_SETxConClient();
            try
            {
                SqlConnection ocnForm = new SqlConnection(cCNVB.tConStr);
                {
                    ocnForm.Open();
                    oDbAdt = new SqlDataAdapter(ptQeury, ocnForm);
                    oDbAdt.Fill(poDt);

                    ocnForm.Dispose();
                    oDbAdt.Dispose();

                    return poDt;
                }
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            finally
            {
                if (oDbAdt != null) { oDbAdt.Dispose(); }
            }
        }

        public static string GETtLang(string ptLang, string ptText)
        {
            string tText = "";
            tText = (ptLang == "TH") ? ptText.Substring(0, ptText.IndexOf("|")) : //    TH
                                       ptText.Substring((ptText.IndexOf("|") + 1), ptText.Length - (ptText.IndexOf("|") + 1)); //EN
            return tText;
        }

        public static void SP_SETxConClient()
        {
            string tCon = null;
            tCon = "Data Source = 172.16.30.14\\SQL2012";
            tCon = tCon + Environment.NewLine + ";Initial Catalog = ProjectControl";
            tCon = tCon + Environment.NewLine + ";Persist Security Info=True;User ID = sa";
            tCon = tCon + Environment.NewLine + ";Password =adasoft";
            // cCNVB.tVB_ConStr = tCon;
        }

        public static void SETxConDBMain()
        {
            // string tCon = "";

            string tPath = "\\Config\\DBCli.xml";
            string tSever = "";
            string tUsr = "";
            string tPsw = "";
            string tDB = "";
            string tTime = "";
            try
            {
                if (File.Exists(tPath))
                {
                    XmlDocument doc = new XmlDocument();
                    Directory.CreateDirectory(Path.GetDirectoryName(tPath));
                    FileStream oFs = new FileStream(tPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    doc.Load(oFs);
                    XmlNodeList elemList = doc.GetElementsByTagName("CI");

                    foreach (XmlNode node in elemList)
                    {
                        XmlElement oElement = (XmlElement)node;

                        tSever = oElement.GetElementsByTagName("Server")[0].InnerText;
                        tUsr = oElement.GetElementsByTagName("User")[0].InnerText;
                        tPsw = oElement.GetElementsByTagName("Password")[0].InnerText;
                        tDB = oElement.GetElementsByTagName("DBName")[0].InnerText;
                        tTime = oElement.GetElementsByTagName("Time")[0].InnerText;
                        cCNVB.tTDuration = oElement.GetElementsByTagName("TimeDuration")[0].InnerText;
                    }

                    oFs.Flush();
                    oFs.Close();
                }

                cCNVB.tConStr = "Data Source = " + tSever;
                cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Initial Catalog =" + tDB;
                cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Persist Security Info=True;User ID =" + tUsr;
                cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Password =" + tPsw;
                cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Connection Timeout =" + tTime;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GETtCutStr(string ptText, int pnLengFix)
        {
            //(value.Length <= 32 ? value.Substring(0, value.Length) : value.Substring(0, 32));
            string tStr = "";
            if (ptText.Length <= pnLengFix)
            {
                tStr = ptText.Substring(0, ptText.Length);
            }
            else
            {
                tStr = ptText.Substring(0, pnLengFix);
            }
            return tStr;
        }
    }
}
