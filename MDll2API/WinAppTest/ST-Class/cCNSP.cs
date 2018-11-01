using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;

namespace WinAppTest.ST_Class
{
    public class cCNSP
    {
        public static string GETtVertionDll()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static DataTable SP_SQLvExecute(string ptQeury,string tConStr)
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
            catch (Exception ex)
            {
                //SP_SETxMessage("cCNSP:SP_SQLvExecute\n" + ex.Message);
                cCNSP.SP_ADDxLog("API:SP_SQLvExecute:Excute DataTable  = " + ex.Message);
                return null;
            }
            finally
            {
                if (oDbAdt != null) { oDbAdt.Dispose(); }
                if (oDbDtsSet != null) { oDbDtsSet.Dispose(); }
            }
        }

        public static int SP_SQLnExecute(string ptQeury, string tConStr)
        {
            int nRowEff = 0;
            // SqlDataAdapter oDbAdt = null;
            SqlCommand oCmdSql = new SqlCommand();
            SqlConnection oDbCon = new SqlConnection(tConStr);
            try
            {
                //  SP_SETxConClient();
                oDbCon.Open();
                oCmdSql.Connection = oDbCon;
                oCmdSql.CommandText = ptQeury;
                nRowEff = oCmdSql.ExecuteNonQuery();

                //  oDbAdt.Dispose();
                oCmdSql.Dispose();
                oDbCon.Dispose();
                return nRowEff;
            }
            catch (Exception ex)
            {

                //SP_SETxMessage("cCNSP:SP_SQLxExecute\n" + ex.Message);
                // cCNSP.SP_ADDxLog("API:SP_SQLvExecute:Excute SP_SQLxExecute = " + ex.Message);
                return nRowEff;
            }
            finally
            {
                //   if (oDbAdt != null) { oDbAdt.Dispose(); }
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
            catch (Exception ex)
            {
                //SP_SETxMessage("cCNSP:SP_SQLvExecute\n" + ex.Message);
                cCNSP.SP_ADDxLog("API:SP_SQLvExecute:Excute DataTable  = " + ex.Message);
                return null;
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
            string tPath2 = "";
            string tPath1 = Application.StartupPath;
            tPath2 = tPath1 + "\\Config\\DBCli.xml";
            string tSever = "";
            string tUsr = "";
            string tPsw = "";
            string tDB = "";
            string tTime = "";
            try
            {
                if (File.Exists(tPath2))
                {
                    XmlDocument doc = new XmlDocument();
                    Directory.CreateDirectory(Path.GetDirectoryName(tPath2));
                    FileStream oFs = new FileStream(tPath2, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

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

                //cCNVB.tConStr = "Data Source = 172.16.30.151";
                //cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Initial Catalog =POSSDB_Center";
                //cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Persist Security Info=True;User ID =SA";
                //cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Password =P@ssw0rd";
                //cCNVB.tConStr = cCNVB.tConStr + Environment.NewLine + ";Connection Timeout =0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("wConDB1:SETxReadConfigXML = " + ex.Message, "Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // return tCon;
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
                case "YYYYMMDD":
                    //ใช้ตั้งชื่อไฟล์
                    tRet = odt.Year.ToString() + int.Parse(odt.Month.ToString()).ToString("00") + int.Parse(odt.Day.ToString()).ToString("00");
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
            SP_WRITExLog(cCNSP.SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY/MM/DD HH:MM:SS") + ":MsgLog:" + ptLogMsg);
        }

        public static void SP_WRITExLog(string ptLog)
        {
            string tPath = Directory.GetCurrentDirectory();
            string tName = cCNSP.SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY-MM-DD");
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

        //public static void SETxLogDB(cLog poLog, object poLogVB6)
        //{
        //    try
        //    {
        //        Type oTypeLog = poLogVB6.GetType();
        //        string tFTBchCode = oTypeLog.GetProperty("FTBchCode").GetValue(poLogVB6, null).ToString();
        //        string tFTDeviceID = oTypeLog.GetProperty("FTDeviceID").GetValue(poLogVB6, null).ToString();
        //        string tFTLogCode = oTypeLog.GetProperty("FTLogCode").GetValue(poLogVB6, null).ToString();
        //        string tFTShdDocNo = oTypeLog.GetProperty("FTShdDocNo").GetValue(poLogVB6, null).ToString();
        //        string tFTShdDocType = oTypeLog.GetProperty("FTShdDocType").GetValue(poLogVB6, null).ToString();
        //        string tFTReqType = oTypeLog.GetProperty("FTReqType").GetValue(poLogVB6, null).ToString();
        //        string tFNStep = oTypeLog.GetProperty("FNStep").GetValue(poLogVB6, null).ToString();
        //        string tFDShdDocDate = oTypeLog.GetProperty("FDShdDocDate").GetValue(poLogVB6, null).ToString();
        //        tFDShdDocDate = SP_DTEtByFormat(tFDShdDocDate, "YYYY-MM-DD");

        //        string tFTResCode = "";
        //        string tFTResMsg = "";
        //        string tFTResShwMsg = "";
        //        string tFTResPara = "";

        //        if (poLog.FTResCode == null)
        //        {
        //            tFTResCode = "";
        //        }
        //        else
        //        {
        //            tFTResCode = poLog.FTResCode.Replace("'", "\"");
        //        }

        //        if (poLog.FTResMsg == null)
        //        {
        //            tFTResMsg = "";
        //        }
        //        else
        //        {
        //            tFTResMsg = poLog.FTResMsg.Replace("'", "\"");
        //        }


        //        if (poLog.FTResShwMsg == null)
        //        {
        //            tFTResShwMsg = "";
        //        }
        //        else
        //        {
        //            tFTResShwMsg = poLog.FTResShwMsg.Replace("'", "\"");
        //        }


        //        if (poLog.FTResPara == null)
        //        {
        //            tFTResPara = "";
        //        }
        //        else
        //        {
        //            tFTResPara = poLog.FTResPara.Replace("'", "\"");
        //        }


        //        string tSql = "";
        //        tSql = "INSERT INTO  TPSTLogT1C (";
        //        tSql = tSql + Environment.NewLine + cCNVB.tVB_Tdate1;
        //        tSql = tSql + Environment.NewLine + ",[FTBchCode],[FTDeviceID],[FTLogCode],[FTShdDocNo],[FTShdDocType]";
        //        tSql = tSql + Environment.NewLine + ",[FTReqType],[FNStep],[FTServiceName],[FTReqPara] ,[FTResPara]";
        //        tSql = tSql + Environment.NewLine + ",[FTEarnOnlineFlag],[FTResCode],[FTResMsg],[FTResShwMsg]";
        //        tSql = tSql + Environment.NewLine + ",[FDShdDocDate]";
        //        tSql = tSql + Environment.NewLine + ")VALUES(";
        //        tSql = tSql + Environment.NewLine + cCNVB.tVB_Fdate1;
        //        tSql = tSql + Environment.NewLine + ",'" + tFTBchCode + "','" + tFTDeviceID + "','" + tFTLogCode + "','" + tFTShdDocNo + "','" + tFTShdDocType + "'    ";
        //        tSql = tSql + Environment.NewLine + ",'" + tFTReqType + "'," + tFNStep + ",'" + poLog.FTServiceName + "','" + poLog.FTReqPara + "','" + tFTResPara + "' ";
        //        tSql = tSql + Environment.NewLine + ",'" + poLog.FTEarnOnlineFlag + "','" + tFTResCode + "','" + tFTResMsg + "','" + tFTResShwMsg + "' ";
        //        tSql = tSql + Environment.NewLine + ",'" + tFDShdDocDate + "')";
        //        cCNSP.SP_SQLxExecute(tSql);
        //        //  cCNSP.SP_ADDxLog("TestLog = "+tSql); // ลบภายหลัง Nutto 2018-04-19
        //    }
        //    catch (Exception ex)
        //    {
        //        // MessageBox.Show("wMemInfoSrv:SETxLogDB = " + ex.Message);
        //        throw;
        //    }
        //}

        public static void SHWxMessage(string ptLanguage, string ptMSgCode, string ptCaption
            , MessageBoxButtons poMSBtn, MessageBoxIcon poMSIcon)
        {

            if (ptLanguage == "") { ptLanguage = "TH"; }

            string[] aMsgArr;
            string tMsg = "";
            string tNewLine = "";
            //tMsgArr(0) ภาษาไทย   tMsgArr(1) ภาษาอังกฤษ
            aMsgArr = ptMSgCode.Split(';');
            if (aMsgArr.Length == 1)
            {
                tMsg = ptLanguage == "TH" ? aMsgArr[0].ToString() : aMsgArr[1].ToString();
                cCNVB.oDialogResult = MessageBox.Show(tMsg + tNewLine, ptCaption, poMSBtn, poMSIcon);
                return;
            }

            tMsg = ptLanguage == "TH" ? aMsgArr[0].ToString() : aMsgArr[1].ToString();

            int n = aMsgArr[1].ToString().IndexOf('\r');
            if (n != -1)
            {
                if (ptLanguage == "EN") { tMsg = tMsg.Substring(0, n); }
                tNewLine = aMsgArr[1].ToString().Substring(n);
            }

            cCNVB.oDialogResult = MessageBox.Show(tMsg + tNewLine, ptCaption, poMSBtn, poMSIcon);
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
