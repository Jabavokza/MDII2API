using MDll2API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using WinAppTest.ST_Class;
using WinAppTest.X_Class;
using System.Linq;
using MDll2API.Class;
using MDll2API.Class.ReceivApp;
using log4net;

namespace WinAppTest
{
    public partial class wTestCallDll : Form
    {
        private readonly ILog oC_Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int nW_CDStr = 10;
        private int nW_CDWait = 10;

        private DataTable oW_DtSale = new DataTable();
        private DataTable oW_DtRdm = new DataTable();
        private DataTable oW_DtBnk = new DataTable();

        private string tW_Json = "";
        private string tW_URL = "";
        private string tW_USER = "";
        private string tW_PSS = "";
        
        private DataGridViewCheckBoxColumn oW_ocbColSale = new DataGridViewCheckBoxColumn();
        private DataGridViewCheckBoxColumn oW_ocbColRdm = new DataGridViewCheckBoxColumn();
        private DataGridViewCheckBoxColumn oW_ocbColBnk = new DataGridViewCheckBoxColumn();
        private DataGridViewLinkColumn oW_olcCol2 = new DataGridViewLinkColumn();
       // private DataGridViewCell oCell1= null;
        private cDbConfig oW_DbConfig = new cDbConfig();
        private string tW_ConSale = "";
        private string tW_ConRdm = "";
        private string tW_VenDorCodeSale = "";
        private string tW_VenDes = "";
        private string tW_DepositCode = "";
        private string tW_DepositDes = "";

        public wTestCallDll()
        {
            InitializeComponent();
        }

        #region "FORM"
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Set User Password 
                tW_USER = "devposctr";
                tW_PSS = "Pn3Rbmd52";
                tW_URL = "http://wdipid.themall.co.th/RESTAdapter/TMGPOS/v1/POSLog";
                ontMain.Visible = false;
                this.Text = "POSLOG v" + cCNSP.GETtVertionDll();
                SETxConfigDB();
                //----------- ลบ ทิ้งที่หลัง -------------------
                otbDTrn.Text  = cCNSP.SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY-MM-DD");
                //----------- ลบ ทิ้งที่หลัง -------------------
                W_SETxDataToGridCon();

                // MIke
                //ocmSendRdm_Click_1(null, null);
            }
            catch (Exception oEx)
            {
                MessageBox.Show("wTestCallDll:SETxConfigDB = " + oEx.Message, "Main", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void wTestCallDll_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
//                ontMain.Visible = true;
                // ontSAPAuto.ShowBalloonTip(1000);
            }
        }
        private void ontMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
//            ontMain.Visible = false;
        }
        #endregion "FORM"

        private void SETxConfigDB()
        {
            string tPath1 = "";
            string tPath2 = "";
            XmlSerializer oXmlSrl = new XmlSerializer(typeof(cDbConfig));
            StreamReader oSr= null;
            try
            {
                tPath1 = Application.StartupPath;
                tPath2 = tPath1 + "\\dbconfig.xml";

                oSr = new StreamReader(tPath2);
                oW_DbConfig  = (cDbConfig)oXmlSrl.Deserialize(oSr);
                oSr.Close();
                try
                {
                    var oDbConfigSale = (from oObj in oW_DbConfig.Dbconfig
                                     where oObj.GroupIndex == "3"
                                     select oObj).ToList();

                    var oDbConfigRdm= (from oObj in oW_DbConfig.Dbconfig
                                         where oObj.GroupIndex == "2"
                                         select oObj).ToList();

                    tW_ConSale = oDbConfigSale[0].Conntect;
                    tW_ConRdm = oDbConfigRdm[0].Conntect;
                    tW_VenDorCodeSale = oDbConfigSale[0].VendorCode;
                    tW_VenDes= oDbConfigSale[0].VendorDes;
                    tW_DepositCode = oDbConfigSale[0].DepositCode;
                    tW_DepositDes = oDbConfigSale[0].DepositDes;

                    //string tCon = null;
                    //tCon = "Data Source = " + oDbConfig[0].Server;
                    //tCon = tCon + Environment.NewLine + ";Initial Catalog = " + oDbConfig[0].DBName;
                    //tCon = tCon + Environment.NewLine + ";Persist Security Info=True;User ID = " + oDbConfig[0].User;
                    //tCon = tCon + Environment.NewLine + ";Password =" + oDbConfig[0].Password;
                    //cCNVB.tConStr = tCon;
                }
                catch (Exception oEx) { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("wTestCallDll:SETxConfigDB = " + ex.Message, "Config Inbound", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                oSr.Close();
            }
        }

        #region"METHOD"

        private string W_GETtSale()
        {
            string tResult1;
            string tResult2;
            string[] atResult;
            cSale oSale = new cSale();
            int nAPIManual = 0;  // 0: Auto,1: Manual
            try
            {
                nAPIManual = 0;
                // tResult = oSale.C_POSTtSale(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);

                tResult1 = oSale.C_POSTtSale(otbDTrn.Text,null, tW_VenDorCodeSale, tW_VenDes , tW_DepositCode, tW_DepositDes, "AUTO");

                atResult = tResult1.Split('|');
                tResult1 = atResult[0] + Environment.NewLine;
                tResult2 = atResult[1] + Environment.NewLine;
                oC_Log.Debug("[RES Sale Status]=" + tResult2 + "[Message]=" + tResult1);
            }
            catch (Exception oEx) { }
            finally
            {
                tResult1 = null;
                tResult2 = null;
                oSale = null;

            }
            return tResult1;
        }

        private void W_GETxCash(string ptMode = "")
        {
            //----------------------------TEST-------------------
            string tResult1 = "", tChk = "", tUPD = "", tDateToDay = "", tResult3 = "";
            string tResult2;
            int nRowEff;
            cCash oCash;
            DataTable oDtChk, oDbChk;
            StringBuilder oSQL;
            string[] atResult;
            try
            {
                oDtChk = new DataTable();
                oCash = new cCash();
                oSQL = new StringBuilder();
                oDbChk = new DataTable();

                //tDateToDay = DateTime.Now.ToString("yyyy-MM-dd");
                tDateToDay = "2018-09-25";

                if (ptMode != "MANUAL")
                {
                    tResult1 = oCash.C_POSTtCash(otbDTrn.Text, ptMode, "");
                }
                else
                {
                    tResult1 = oCash.C_POSTtCash(otbDateShortManual.Text, ptMode, otbShortPlant.Text);
                }
                
                atResult = tResult1.Split('|');
                tResult1 = atResult[0];
                //rtResult += atResult[1] + Environment.NewLine;
                tResult2 = atResult[1];
                tResult3 = atResult[2];

                oC_Log.Debug("[RES ShortOver4 Status] = " + tResult2 + "[Message]=" + tResult1);

                //if (tResult2 == "200")
                //{
                //check staclode and update flag
                //oSQL.AppendLine("SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WITH (ROWLOCK)");
                //oSQL.AppendLine("WHERE FDSaleDate = '2018-09-07'");
                //oSQL.AppendLine("AND FTPlantCode = '17KA'");
                //oSQL.AppendLine("AND FTStaSentOnOff = '2'");

                //oDtChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConSale);

                //if (oDtChk.Rows.Count == 1)
                //{
                //    //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
                //    //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
                //}
                if (ptMode != "MANUAL")
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                    oSQL.AppendLine("AND FTStaShortOver = '0'");
                }
                else
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                }
                //oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                //oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                //oSQL.AppendLine("AND FTStaShortOver = '0'");
                //oSQL.AppendLine("AND FTPlantCode = '17KA'");

                oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConSale);

                if (oDbChk.Rows.Count > 0)
                {
                    for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                    {
                        oSQL.Clear();
                        oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                        oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                        oSQL.AppendLine("   ,FTStaShortOver = '1'");
                        oSQL.AppendLine("   ,FTJsonFileShortOver = '" + tResult3 + "'");
                        oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                        oSQL.AppendLine("AND FDSaleDate = '" + tDateToDay + "'");

                        nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tW_ConSale);
                    }

                    if (ptMode == "MANUAL")
                    {
                        MessageBox.Show("ShortOver4 = " + tResult1, "ShortOver4", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                //}
            }
            catch (Exception oEx) { }
            finally
            {
                tResult1 = null;
                tResult2 = null;
                oCash = null;
                atResult = null;
            }
        }

        private void W_GETxEDC(string ptMode = "")
        {
            cEDC oEDC;
            DataTable oDtChk, oDbChk;
            int nAPIManual = 0, nRowEff;
            StringBuilder oSQL;
            string[] atResult;
            string tResult1;
            string tChk = "", tDateToDay = "", tResult2 = "", tResult3 = "";
            try
            {
                oDtChk = new DataTable();
                oDbChk = new DataTable();
                oSQL = new StringBuilder();
                oEDC = new cEDC();
                //tResult = oEDC.C_POSTtEDC(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);

                if (ptMode != "MANUAL")
                {
                    tResult1 = oEDC.C_POSTtEDC(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), 1, otbDTrn.Text, ptMode);
                }
                else
                {
                    tResult1 = oEDC.C_POSTtEDC(tW_Json, otbUrl.Text, otbUser.Text, otbPassword.Text, nAPIManual, otbDateEDCMaual.Text, ptMode);
                }

                atResult = tResult1.Split('|');
                tResult1 = atResult[0];
                // rtResult += atResult[1] + Environment.NewLine;
                //   oC_Log.Debug("RES EDC9 =" + rtResult);
                tResult2 = atResult[1];
                tResult3 = atResult[2];
                oC_Log.Debug("[RES EDC Status]=" + tResult2 + "[Message]=" + tResult1);

                //tDateToDay = DateTime.Now.ToString("yyyy-MM-dd");
                tDateToDay = "2018-09-25";

                //if (tResult2 == "200")
                //{
                //    //check staclode and update flag
                //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='3' ";
                //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
                //    if (oDtChk.Rows.Count == 1)
                //    {
                //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
                //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
                //    }
                if (ptMode != "MANUAL")
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                    oSQL.AppendLine("AND FTStaEDC = '0'");
                }
                else
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                }

                //oSQL.AppendLine("AND FTPlantCode = '17KA'");

                oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConSale);

                if (oDbChk.Rows.Count > 0)
                {
                    for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                    {
                        oSQL.Clear();
                        oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                        oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                        oSQL.AppendLine("   ,FTStaEDC = '1'");
                        oSQL.AppendLine("   ,FTJsonFileEDC = '" + tResult3 + "'");
                        oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                        oSQL.AppendLine("AND FDSaleDate = '" + tDateToDay + "'");

                        nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tW_ConSale);
                    }

                    if (ptMode == "MANUAL")
                    {
                        MessageBox.Show("EDC9 = " + tResult1, "EDC9", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                //}
            }
            catch (Exception oEx) { }
            finally
            {
                tResult1 = null;
                tResult2 = null;
                oEDC = null;
                atResult = null;
           
            }
        }

        private void W_SETxDataToGridCon()
        {
            DataSet oDataSet = new DataSet();
            string tXmlFilePath = @"dbconfig.xml";
            try
            {
                oDataSet.ReadXml(tXmlFilePath, XmlReadMode.InferSchema);

                foreach (DataTable oTable in oDataSet.Tables)
                {
                    if (oTable.TableName == "dbconfig")
                    {
                        ogdDataConnection.DataSource = oTable;
                    }
                }
            }
            catch { }
        }

        private void W_GETxBankIn(string ptMode = "")
        {
            string tResult1, tDateToDay, tResult3 = "", tResult2;
            cBankDeposit oBankIn;
            string[] atResult;
            string tChk = "";
            int nRowEff = 0;
            DataTable oDtChk, oDbChk;
            StringBuilder oSQL;
            try
            {
                oDtChk = new DataTable();
                oDbChk = new DataTable();

                oSQL = new StringBuilder();
                oBankIn = new cBankDeposit();

                if (ptMode != "MANUAL")
                {
                    tResult1 = oBankIn.C_POSTtBankDeposit(otbDTrn.Text, null, ptMode);
                }
                else
                {
                    tResult1 = oBankIn.C_POSTtBankDeposit(otbBankInManual.Text, null, ptMode);
                }

                atResult = tResult1.Split('|');
                tResult1 = atResult[0];
                tResult2 = atResult[1];
                tResult3 = atResult[2];

                oC_Log.Debug("[RES BankIn Status]=" + tResult2 + "[Message] = " + tResult1);
                //if (tResult2 == "200")
                //{
                //    //check staclode and update flag
                //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='3' ";
                //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
                //    if (oDtChk.Rows.Count == 1)
                //    {
                //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
                //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
                //    }
                //}
                tDateToDay = "2018-09-25";

                if (ptMode != "MANUAL")
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                    oSQL.AppendLine("AND FTStaBankIn = '0'");
                }
                else
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                }

                oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConSale);

                if (oDbChk.Rows.Count > 0)
                {
                    for (int nLoop = 0; nLoop < oDbChk.Rows.Count; nLoop++)
                    {
                        oSQL.Clear();
                        oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                        oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                        oSQL.AppendLine("   ,FTStaBankIn = '1'");
                        oSQL.AppendLine("   ,FTJsonFileBankIn = '" + tResult3 + "'");
                        oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                        oSQL.AppendLine("AND FDSaleDate = '" + tDateToDay + "'");

                        nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tW_ConSale);
                    }

                    if (ptMode == "MANUAL")
                    {
                        MessageBox.Show("BankIn8 = " + tResult1, "BankIn8", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

            }
            catch { }
            finally
            {
                tResult1 = null;
                tResult2 = null;
                oBankIn = null;
                atResult = null;
            }
        }

        private void W_GETxAuto()
        {
            string rtResult;
            cAutomatic oAutomatic = new cAutomatic();
            string[] atResult;
            string tResult;
            int nAPIManual = 0;  // 0: Auto,1: Manual
            try
            {
                nAPIManual = 0;


                //tResult = oAutomatic.C_POSTtAutomatic(otbJson.Text, otbCheck.Text.Trim()
                //                        , otbMove.Text.Trim(), otbTmnNum.Text.Trim(), Convert.ToInt32(otbSeqNo.Text.Trim())
                //                        , ocbAPIUrl.Text.Trim(), otbAPIUsr.Text.Trim(), otbAPIPwd.Text.Trim(), nAPIManual);
                //atResult = tResult.Split('|');
                //  rtResult = atResult[0] + Environment.NewLine;
                // rtResult += atResult[1] + Environment.NewLine;

            }
            catch { }
            finally
            {
                rtResult = null;
                oAutomatic = null;
                atResult = null;
                tResult = null;
            }
        }

        private void W_GETxSaleOrder()
        {
            string rtResult;
            cSaleOrder oSaleOrder = new cSaleOrder();
            string[] atResult;
            string tResult;
            int nAPIManual = 0;  // 0: Auto,1: Manual
            try
            {
                nAPIManual = 0;

                // tResult = oSaleOrder.C_POSTtSaleOrder(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);
                tResult = oSaleOrder.C_POSTtSaleOrder(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual, otbDTrn.Text);
                atResult = tResult.Split('|');
                rtResult = atResult[0] + Environment.NewLine;
                //rtResult += atResult[1] + Environment.NewLine;

            }
            catch { }
            finally
            {
                rtResult = null;
                oSaleOrder = null;
                atResult = null;
                tResult = null;
            }
        }

        private void W_GETxPoint()
        {
            string rtResult;
            cPoint oPoint = new cPoint();
            string[] atResult;
            string tResult;
            int nAPIManual = 0;  // 0: Auto,1: Manual
            try
            {
                nAPIManual = 0;


                // tResult = oPoint.C_POSTtPoint(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);
                tResult = oPoint.C_POSTtPoint(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual, otbDTrn.Text);
                atResult = tResult.Split('|');
                rtResult = atResult[0] + Environment.NewLine;
                // rtResult += atResult[1] + Environment.NewLine;

            }
            catch { }
            finally
            {
                rtResult = null;
                oPoint = null;
                atResult = null;
                tResult = null;
            }
        }

        private string W_SETtEOD(string ptMode = "")
        {
            string tResult1 = "", tResult2 = "", tResult3 = "", tDateToDay = "", tResultUseEOD = "";
            string[] atResult;
            int nRowEff = 0;
            DataTable oDbChk;
            StringBuilder oSQL;
            cEOD oEOD;
            try
            {
                oSQL = new StringBuilder();
                oDbChk = new DataTable();
                oEOD = new cEOD();

                //tDateToDay = DateTime.Now.ToString("yyyy-MM-dd");
                tDateToDay = "2018-09-25";

                if (ptMode != "MANUAL")
                {
                    tResult1 = oEOD.C_POSTtEOD(otbDTrn.Text, "", ptMode);
                }
                else
                {
                    tResult1 = oEOD.C_POSTtEOD(otbEOD.Text, otbPlantEOD.Text, ptMode);
                }

                atResult = tResult1.Split('|');
                tResult1 = atResult[0];
                tResult2 = atResult[1];
                tResult3 = atResult[2];

                oC_Log.Debug("[RES DaySummary5 Status] = " + tResult2 + "[Message]=" + tResult1);

                //if (tResult2 == "200")
                //{
                // Check Staclode And Update Flag
                if (ptMode != "MANUAL")
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                    oSQL.AppendLine("AND FTStaEOD = '0'");
                }
                else
                {
                    oSQL.AppendLine("SELECT FDSaleDate, FTPlantCode FROM TCNMPlnCloseSta WITH (NOLOCK)");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + tDateToDay + "'");
                }

                //oSQL.AppendLine("AND FTPlantCode = '17KA'");

                oDbChk = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConSale);

                if (oDbChk.Rows.Count > 0)
                {
                    for (int nLoop = 0;nLoop < oDbChk.Rows.Count;nLoop++)
                    {
                        oSQL.Clear();
                        oSQL.AppendLine("UPDATE TCNMPlnCloseSta WITH (ROWLOCK)");
                        oSQL.AppendLine("SET FTStaSentOnOff = '1'");
                        oSQL.AppendLine("   ,FTStaEOD = '1'");
                        oSQL.AppendLine("   ,FTJsonFileEOD = '" + tResult3 + "'");
                        oSQL.AppendLine("WHERE FTPlantCode = '" + oDbChk.Rows[nLoop]["FTPlantCode"].ToString() + "'");
                        oSQL.AppendLine("AND FDSaleDate = '" + tDateToDay + "'");

                        nRowEff = cCNSP.SP_SQLnExecute(oSQL.ToString(), tW_ConSale);

                        if (nRowEff > 0)
                        {
                            tResultUseEOD = "OK";
                        }
                    }

                    if (ptMode == "MANUAL")
                    {
                        MessageBox.Show("DaySummary5 = " + tResult1, "DaySummary5", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                //}
            }
            catch
            {
                tResultUseEOD = "";
            }
            finally
            {
                tResult1 = null;
                tResult2 = null;
                atResult = null;
                oEOD = null;
            }
            return tResultUseEOD;
        }

        private void W_GETxRedeem()
        {
            string rtResult;
            cRedeem oRedeem = new cRedeem();
            string[] atResult;
            string tResult;
            int nAPIManual = 0;  // 0: Auto,1: Manual
            try
            {
                nAPIManual = 0;


                //  tResult = oRedeem.C_POSTtRedeem(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);
                //   tResult = oRedeem.C_POSTtRedeem(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual, otbDTrn.Text);

                //atResult = tResult.Split('|');
                //rtResult = atResult[0] + Environment.NewLine;
                //  rtResult += atResult[1] + Environment.NewLine;

            }
            catch (Exception oEx) { }
            finally
            {
                rtResult = null;
                oRedeem = null;
                atResult = null;
                tResult = null;
            }
        }

        private void SETxNotify(string ptTitle, string ptMsg, int pnTime, string ptOnOff, int pnNotiIC)
        {
            // ToolipIcon
            try
            {
                if (pnNotiIC == 0)
                {
//                    ontMain.BalloonTipIcon = ToolTipIcon.None;
                    Program.oC_MPosLogNotifyIco.BalloonTipIcon = ToolTipIcon.None;
                }
                else if (pnNotiIC == 1)
                {
                    //                   ontMain.BalloonTipIcon = ToolTipIcon.Info;
                    Program.oC_MPosLogNotifyIco.BalloonTipIcon = ToolTipIcon.Info;
                }
                else if (pnNotiIC == 2)
                {
                    //                   ontMain.BalloonTipIcon = ToolTipIcon.Warning;
                    Program.oC_MPosLogNotifyIco.BalloonTipIcon = ToolTipIcon.Warning;
                }
                else if (pnNotiIC == 3)
                {
                    //                    ontMain.BalloonTipIcon = ToolTipIcon.Error;
                    Program.oC_MPosLogNotifyIco.BalloonTipIcon = ToolTipIcon.Error;
                }
                // ICON
                //if (ptOnOff == "ON")
                //{
                //    ontSAPAuto.Icon = Properties.Resources.ic_themallOn;
                //}
                //else
                //{
                //    ontSAPAuto.Icon = Properties.Resources.ic_themallOff;
                //}

                Program.oC_MPosLogNotifyIco.BalloonTipText = ptMsg;
                Program.oC_MPosLogNotifyIco.BalloonTipTitle = ptTitle;
                Program.oC_MPosLogNotifyIco.ShowBalloonTip(pnTime);
            }
            catch (Exception oEx)
            {
                oC_Log.Error(oEx.Message);
            }
        }

        private void GRDxIniSale()
        {
            try
            {
                //  ogdSale
                int n = 1;
                //---------------------------------------  Sale ------------------------------------
                ogdSale.DataSource = null;
                ogdSale.Columns.Clear();
                ogdSale.ColumnCount = 7;
                ogdSale.AutoGenerateColumns = false;
                ogdSale.RowHeadersVisible = false;

                oW_ocbColSale.Width = 50;
                oW_ocbColSale.HeaderText = "เลือก";
                oW_ocbColSale.Tag = "select|เลือก";
                oW_ocbColSale.Name = "ocbSelect";
                oW_ocbColSale.DataPropertyName = "ocbSelect";
                //ocbCol.ReadOnly = true;
                oW_ocbColSale.Visible = true;
                ogdSale.Columns.Insert(0, oW_ocbColSale);


                ogdSale.Columns[n].HeaderText = "Store/Plant";
                ogdSale.Columns[n].Name = "FTShdPlantCode";
                //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
                ogdSale.Columns[n].DataPropertyName = "FTShdPlantCode";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 100;
                n++;

                ogdSale.Columns[n].HeaderText = "Transaction date";
                ogdSale.Columns[n].Name = "FDShdTransDate";
                //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
                ogdSale.Columns[n].DataPropertyName = "FDShdTransDate";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 120;
                n++;

                ogdSale.Columns[n].HeaderText = "FTTmnNum";
                ogdSale.Columns[n].Name = "FTTmnNum";
                //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
                ogdSale.Columns[n].DataPropertyName = "FTTmnNum";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 100;
                n++;

                ogdSale.Columns[n].HeaderText = "FHShdTransNo";
                ogdSale.Columns[n].Name = "FTShdTransNo";
                //ogdSku.Columns[n].Tag = "Name|ชื่อ";
                ogdSale.Columns[n].DataPropertyName = "FTShdTransNo";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 100;
                n++;

                ogdSale.Columns[n].HeaderText = "FTTranType";
                ogdSale.Columns[n].Name = "FTShdTransType";
                //ogdSku.Columns[n].Tag = "Name|ชื่อ";
                ogdSale.Columns[n].DataPropertyName = "FTShdTransType";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 100;
                n++;

                ogdSale.Columns[n].HeaderText = "FCAmtNet";
                ogdSale.Columns[n].Name = "FCShdTotal";
                //ogdSku.Columns[n].Tag = "Name|ชื่อ";
                ogdSale.Columns[n].DataPropertyName = "FCShdTotal";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 100;
                n++;

                ogdSale.Columns[n].HeaderText = "FTStaSentOnOff";
                ogdSale.Columns[n].Name = "FTStaSend";
                //ogdSku.Columns[n].Tag = "Name|ชื่อ";
                ogdSale.Columns[n].DataPropertyName = "FTStaSend";
                ogdSale.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                ogdSale.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                ogdSale.Columns[n].ReadOnly = true;
                ogdSale.Columns[n].Visible = true;
                ogdSale.Columns[n].Width = 120;
                n++;
                //---------------------------------------  Sale ------------------------------------
            }
            catch (Exception ex)
            {
                MessageBox.Show("wTestCallDll:GRDxIniSale = " + ex.Message);
            }
        }

        private void GRDxIniRedeem()
        {
            int n = 1;
            //--------------------------------------- Redeem ------------------------------------
            ogdRdm.DataSource = null;
            ogdRdm.Columns.Clear();
            ogdRdm.ColumnCount = 4;
            ogdRdm.AutoGenerateColumns = false;
            ogdRdm.RowHeadersVisible = false;

            oW_ocbColRdm.Width = 50;
            oW_ocbColRdm.HeaderText = "เลือก";
            oW_ocbColRdm.Tag = "select|เลือก";
            oW_ocbColRdm.Name = "ocbSelect";
            oW_ocbColRdm.DataPropertyName = "ocbSelect";
            //ocbCol.ReadOnly = true;
            oW_ocbColRdm.Visible = true;
            ogdRdm.Columns.Insert(0, oW_ocbColRdm);


            ogdRdm.Columns[n].HeaderText = "FDRPDocDate";
            ogdRdm.Columns[n].Name = "FDRPDocDate";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            ogdRdm.Columns[n].DataPropertyName = "FDRPDocDate";
            ogdRdm.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdRdm.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdRdm.Columns[n].ReadOnly = true;
            ogdRdm.Columns[n].Visible = true;
            ogdRdm.Columns[n].Width = 100;
            n++;

            ogdRdm.Columns[n].HeaderText = "FTPremiumNo";
            ogdRdm.Columns[n].Name = "FTPremiumNo";
            //ogdBnk.Columns[n].Tag = "User|ผู้ใช้";
            ogdRdm.Columns[n].DataPropertyName = "FTPremiumNo";
            ogdRdm.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdRdm.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdRdm.Columns[n].ReadOnly = true;
            ogdRdm.Columns[n].Visible = true;
            ogdRdm.Columns[n].Width = 120;
            n++;

            ogdRdm.Columns[n].HeaderText = "FTPreMiumID";
            ogdRdm.Columns[n].Name = "FTPreMiumID";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            ogdRdm.Columns[n].DataPropertyName = "FTPreMiumID";
            ogdRdm.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdRdm.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdRdm.Columns[n].ReadOnly = true;
            ogdRdm.Columns[n].Visible = true;
            ogdRdm.Columns[n].Width = 100;
            n++;

            ogdRdm.Columns[n].HeaderText = "FTStaSentOnOff";
            ogdRdm.Columns[n].Name = "FTStaSend";
            //ogdSku.Columns[n].Tag = "Name|ชื่อ";
            ogdRdm.Columns[n].DataPropertyName = "FTStaSend";
            ogdRdm.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdRdm.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdRdm.Columns[n].ReadOnly = true;
            ogdRdm.Columns[n].Visible = true;
            ogdRdm.Columns[n].Width = 120;
            n++;
            //---------------------------------------  Redeem ------------------------------------
        }

        private void GRDxIniBnk()
        {
            int n = 1;
            //---------------------------------------  Bank ------------------------------------
            ogdBnk.DataSource = null;
            ogdBnk.Columns.Clear();
            ogdBnk.ColumnCount = 5;
            ogdBnk.AutoGenerateColumns = false;
            ogdBnk.RowHeadersVisible = false;

            oW_ocbColBnk.Width = 50;
            oW_ocbColBnk.HeaderText = "เลือก";
            oW_ocbColBnk.Tag = "select|เลือก";
            oW_ocbColBnk.Name = "ocbSelect";
            oW_ocbColBnk.DataPropertyName = "ocbSelect";
            //ocbCol.ReadOnly = true;
            oW_ocbColBnk.Visible = true;
            ogdBnk.Columns.Insert(0, oW_ocbColBnk);


            ogdBnk.Columns[n].HeaderText = "FDBdpSaleDate";
            ogdBnk.Columns[n].Name = "FDBdpDepositDate";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            ogdBnk.Columns[n].DataPropertyName = "FDBdpDepositDate";
            ogdBnk.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdBnk.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdBnk.Columns[n].ReadOnly = true;
            ogdBnk.Columns[n].Visible = true;
            ogdBnk.Columns[n].Width = 100;
            n++;

            ogdBnk.Columns[n].HeaderText = "FTBdpPlantCode";
            ogdBnk.Columns[n].Name = "FTBdpPlantCode";
            //ogdBnk.Columns[n].Tag = "User|ผู้ใช้";
            ogdBnk.Columns[n].DataPropertyName = "FTBdpPlantCode";
            ogdBnk.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdBnk.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdBnk.Columns[n].ReadOnly = true;
            ogdBnk.Columns[n].Visible = true;
            ogdBnk.Columns[n].Width = 120;
            n++;

            ogdBnk.Columns[n].HeaderText = "FCBdpOverShort";
            ogdBnk.Columns[n].Name = "FCBdpOverShort";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            ogdBnk.Columns[n].DataPropertyName = "FCBdpOverShort";
            ogdBnk.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdBnk.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdBnk.Columns[n].ReadOnly = true;
            ogdBnk.Columns[n].Visible = true;
            ogdBnk.Columns[n].Width = 100;
            n++;

            ogdBnk.Columns[n].HeaderText = "FTStaSentOnOff";
            ogdBnk.Columns[n].Name = "FTStaSend";
            //ogdSku.Columns[n].Tag = "Name|ชื่อ";
            ogdBnk.Columns[n].DataPropertyName = "FTStaSend";
            ogdBnk.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
            ogdBnk.Columns[n].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ogdBnk.Columns[n].ReadOnly = true;
            ogdBnk.Columns[n].Visible = true;
            ogdBnk.Columns[n].Width = 120;
            n++;
            //---------------------------------------  Bank ------------------------------------
        }

        #endregion"METHOD"

        #region "BUTTON"

        #region "BUTTON_Sale"

        private void ocmSchSale_Click(object sender, EventArgs e)
        {
            string tSql1 = "";
            string tSql2 = "";
            tSql1 = tSql1 + Environment.NewLine + "SELECT FTShdPlantCode, FDShdTransDate, FTTmnNum, FTShdTransNo, FTShdTransType, FCShdTotal, FTStaSentOnOff";
            tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'Uncen' ELSE 'Sent' END AS FTStaSend";
            tSql1 = tSql1 + Environment.NewLine + "FROM TPSTSalHD";
          
            if (otbPlantSale.Text == "" && otbTrnDSale.Text == "" && otbTerNoSale.Text == ""  )
            {
                MessageBox.Show("กรุณากรอก อย่างใดอยากหนึ่ง", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

       
            if (otbPlantSale.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTShdPlantCode='"+ otbPlantSale.Text.ToUpper().Trim() + "' ";
                if (otbTrnDSale.Text !="") { tSql1 = tSql1+tSql2 + Environment.NewLine + "AND FDShdTransDate='"+ otbTrnDSale.Text.Trim() + "' "; }
                if (otbTerNoSale.Text != "") { tSql1 = tSql1+ tSql2 + Environment.NewLine + "AND FTShdTransNo='"+ otbTerNoSale.Text.Trim() + "' "; }
            }
            else if (otbTrnDSale.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FDShdTransDate='" + otbTrnDSale.Text.Trim() + "' ";
                if (otbPlantSale.Text != "") { tSql1 = tSql1+tSql2 + Environment.NewLine + "AND FTShdPlantCode='" + otbPlantSale.Text.ToUpper().Trim() + "' "; }
                if (otbTerNoSale.Text != "") { tSql1 = tSql1+tSql2 + Environment.NewLine + "AND FTShdTransNo='" + otbTerNoSale.Text.Trim() + "' "; }
            }
            else if (otbTerNoSale.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTShdTransNo='" + otbTerNoSale.Text.Trim() + "' ";
                if (otbPlantSale.Text != "") { tSql1 = tSql1+ tSql2 + Environment.NewLine + "AND FTShdPlantCode='" + otbPlantSale.Text.ToUpper().Trim() + "' "; }
                if (otbTrnDSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FDShdTransDate='" + otbTrnDSale.Text.Trim() + "' "; }
            }

            oW_DtSale = cCNSP.SP_SQLvExecute(tSql1, tW_ConSale);

            if (oW_DtSale != null && oW_DtSale.Rows.Count > 0)
            {
                GRDxIniSale();
                ogbSendSale.Enabled = true;
                ogdSale.DataSource = oW_DtSale;
            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ocmSendSale_Click(object sender, EventArgs e)
        {
            cSale oSale = new cSale();
            string tResult = "";
            string tVal = "";
            string[] atResult;
            string tResult1 = "";
            string tResult2 = "";
            string tChk = "";
            DataTable oDtChk = new DataTable();
            tVal = "(";
            foreach (DataGridViewRow row in ogdSale.Rows)
            {
                bool bIsSelected = Convert.ToBoolean(row.Cells["ocbSelect"].Value);
                if (bIsSelected)
                {
                    tVal = tVal+ "'" + row.Cells["FTShdPlantCode"].Value.ToString() 
                                     + row.Cells["FTTmnNum"].Value.ToString()
                                     + row.Cells["FTShdTransNo"].Value.ToString()
                                     +'_'+ cCNSP.SP_DTEtByFormat(row.Cells["FDShdTransDate"].Value.ToString(), "YYYYMMDD")
                              + "',";
                }
            }
            tVal = tVal.Substring(0, tVal.Length - 1);
            tVal = tVal+")";
            cRcvSale oRcvSale = new cRcvSale()
            {
                TypeName = "Sale",
                TableName = "TPSTSalHD",
                Field = "(HD.FTShdPlantCode+HD.FTTmnNum+HD.FTShdTransNo+'_'+CONVERT(varchar(8),HD.FDShdTransDate,112)) IN",
                Value = tVal
            };
            tResult = oSale.C_POSTtSale(otbDTrn.Text, oRcvSale, tW_VenDorCodeSale,tW_VenDes, tW_DepositCode, tW_DepositDes, "MANUAL");

            atResult = tResult.Split('|');

            tResult1 = atResult[0] + Environment.NewLine;
            tResult2 = atResult[1] + Environment.NewLine;

          
            oC_Log.Debug("[RES Manual Sale Status]=" + tResult2 + "[Message]=" + tResult1);
            MessageBox.Show("[RES Manual Sale Status]=" + tResult2 + "[Message]=" + tResult1, "Manual Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (tResult2 == "200")
            //{
            //    //check staclode and update flag
            //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='4' ";
            //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
            //    if (oDtChk.Rows.Count == 1)
            //    {
            //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
            //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
            //    }
            //}
        }

        #endregion "BUTTON_Sale"

        #region "BUTTON_Redeem"

        private void ocmSchRdm_Click(object sender, EventArgs e)
        {
            string tSql1 = "";
            string tSql2 = "";
            tSql1 = tSql1 + Environment.NewLine + "SELECT FDRPDocDate,FTPremiumNo,FTPreMiumID,FTStaSentOnOff ";
            tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'Uncen' ELSE 'Sent' END AS FTStaSend";
            tSql1 = tSql1 + Environment.NewLine + "FROM TPSTRpremium";

            if (otbDateRdm.Text == "" && otbNoRdm.Text == "" && otbIdRdm.Text == "")
            {
                MessageBox.Show("กรุณากรอก อย่างใดอยากหนึ่ง", "Reedeem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

          // = FTShdPlantCode
            if (otbDateRdm.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FDRPDocDate='" + otbDateRdm.Text.ToUpper().Trim() + "' ";
                if (otbNoRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTPremiumNo='" + otbNoRdm.Text.Trim() + "' "; }
                if (otbIdRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTPreMiumID='" + otbIdRdm.Text.Trim() + "' "; }
            }
            else if (otbNoRdm.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTPremiumNo='" + otbNoRdm.Text.Trim() + "' ";
                if (otbDateRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FDRPDocDate='" + otbDateRdm.Text.ToUpper().Trim() + "' "; }
                if (otbIdRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTPreMiumID='" + otbIdRdm.Text.Trim() + "' "; }
            }
            else if (otbIdRdm.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTPreMiumID='" + otbIdRdm.Text.Trim() + "' ";
                if (otbDateRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FDRPDocDate='" + otbDateRdm.Text.ToUpper().Trim() + "' "; }
                if (otbNoRdm.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTPremiumNo='" + otbNoRdm.Text.Trim() + "' "; }
            }

            oW_DtRdm = cCNSP.SP_SQLvExecute(tSql1, tW_ConRdm);

            if (oW_DtRdm != null && oW_DtRdm.Rows.Count > 0)
            {
                GRDxIniRedeem();
                ogbSendRdm.Enabled = true;
                ogdRdm.DataSource = oW_DtRdm;
            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Reedeem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        
        private void ocmSendRdm_Click(object sender, EventArgs e)
        {
            cRedeem oRedeem = new cRedeem();
            string tResult1 = "";
            string tResult2 = "";
            string tVal = "";
            string tChk = "";
            DataTable oDtChk = new DataTable();
            string[] atResult;
            tVal = "(";
            foreach (DataGridViewRow row in ogdRdm.Rows)
            {
                bool bIsSelected = Convert.ToBoolean(row.Cells["ocbSelect"].Value);
                if (bIsSelected)
                {
                   // FTPremiumNo,FTPreMiumID
                    tVal = tVal + "'" + row.Cells["FTPremiumNo"].Value.ToString()
                                     + row.Cells["FTPreMiumID"].Value.ToString()
                                     + '_' + cCNSP.SP_DTEtByFormat(row.Cells["FDRPDocDate"].Value.ToString(), "YYYYMMDD")
                              + "',";

                }
            }
            tVal = tVal.Substring(0, tVal.Length - 1);
            tVal = tVal + ")";
            cRcvRedeem oRcvRedeem = new cRcvRedeem()
            {
                TypeName = "Redeem",
                TableName = "TPSTRpremium",
                Field = "(Trn.FTPremiumNo+Trn.FTPreMiumID+'_'+CONVERT(varchar(8),Trn.FDRPDocDate,112)) IN",
                Value = tVal
            };
            tResult1 = oRedeem.C_POSTtRedeem(otbDTrn.Text, oRcvRedeem,"MANUAL");
            atResult = tResult1.Split('|');

            tResult1 = atResult[0] + Environment.NewLine;
            tResult2 = atResult[1] + Environment.NewLine;

            oC_Log.Debug("[RES Manual Redeem Status]=" + tResult2 + "[Message]=" + tResult1);
            MessageBox.Show("[RES Manual Redeem Status]=" + tResult2 + "[Message]=" + tResult1, "Manual Redeem", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (tResult2 == "200")
            //{
            //    //check staclode and update flag
            //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='4' ";
            //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
            //    if (oDtChk.Rows.Count == 1)
            //    {
            //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
            //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
            //    }
            //}

        }

        #endregion "BUTTON_Redeem"

        #region "BUTTON_Bank"

        private void ocmSchBnk_Click(object sender, EventArgs e)
        {
            string tSql1 = "";
            string tSql2 = "";
            tSql1 = tSql1 + Environment.NewLine + "SELECT FDBdpDepositDate,FTBdpPlantCode,FCBdpOverShort,FTStaSentOnOff";
            tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0')<> '1'  THEN 'Uncen' ELSE 'Sent' END AS FTStaSend";
            tSql1 = tSql1 + Environment.NewLine + "FROM TPSTBankDeposit";

            if (otbDBnk.Text == "" && otbPlantBnk.Text == "" )
            {
                MessageBox.Show("กรุณากรอก อย่างใดอยากหนึ่ง", "Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

  
            if (otbPlantBnk.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTBdpPlantCode='" + otbPlantBnk.Text.ToUpper().Trim() + "' ";
                if (otbDBnk.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FDBdpDepositDate='" + otbDBnk.Text.Trim() + "' "; }
              
            }
            else if (otbDBnk.Text.Trim() != "")
            {
                tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FDBdpDepositDate='" + otbDBnk.Text.Trim() + "' ";
                if (otbPlantBnk.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTBdpPlantCode='" + otbPlantBnk.Text.ToUpper().Trim() + "' "; }
            }
           

            oW_DtBnk = cCNSP.SP_SQLvExecute(tSql1, tW_ConSale);

            if (oW_DtBnk != null && oW_DtBnk.Rows.Count > 0)
            {
                GRDxIniBnk();
                ogbSendBnk.Enabled = true;
                ogdBnk.DataSource = oW_DtBnk;
            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ocmSendBnk_Click(object sender, EventArgs e)
        {
            cBankDeposit oBank = new cBankDeposit();
            string tResult1 = "";
            string tResult2 = "";
            string tVal = "";
            string tChk = "";
            string[] atResult;
            DataTable oDtChk = new DataTable();

            tVal = "(";

            foreach (DataGridViewRow row in ogdBnk.Rows)
            {
                bool bIsSelected = Convert.ToBoolean(row.Cells["ocbSelect"].Value);
                if (bIsSelected)
                {
                    // FTPremiumNo,FTPreMiumID
                    tVal = tVal + "'" + row.Cells["FTBdpPlantCode"].Value.ToString()
                                     + '_' + cCNSP.SP_DTEtByFormat(row.Cells["FDBdpDepositDate"].Value.ToString(), "YYYYMMDD")
                              + "',";

                }
            }

            tVal = tVal.Substring(0, tVal.Length - 1);
            tVal = tVal + ")";

            cRcvBank oRcvBank= new cRcvBank()
            {
                TypeName = "Bank",
                TableName = "TPSTBankDeposit",
                Field = "(FTBdpPlantCode+'_'+CONVERT(varchar(8),FDBdpDepositDate,112)) IN",
                Value = tVal
            };

            tResult1 = oBank.C_POSTtBankDeposit(otbDTrn.Text, oRcvBank,"MANUAL");
   
            atResult = tResult1.Split('|');
            tResult1 = atResult[0] + Environment.NewLine;
            tResult2 = atResult[1] + Environment.NewLine;

            oC_Log.Debug("[RES Manual BankIn Status]=" + tResult2 + "[Message]=" + tResult1);
            MessageBox.Show("[RES Manual BankIn Status]=" + tResult2 + "[Message]=" + tResult1, "Manual BankIn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (tResult2 == "200")
            //{
            //    //check staclode and update flag
            //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='4' ";
            //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
            //    if (oDtChk.Rows.Count == 1)
            //    {
            //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
            //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
            //    }
            //}
        }

        #endregion "BUTTON_Bank"

        #region "BUTTON_EOD"

        private void ocmSendEOD_Click(object sender, EventArgs e)
        {
            try
            {
                W_SETtEOD("MANUAL");
            }
            catch { }
            //string tResult1 = "";
            //string tResult2 = "";
            //string tChk = "";
            //string[] atResult;
            //DataTable oDtChk = new DataTable();
            //cEOD oEOD = new cEOD();
            //// tResult = oEOD.C_POSTtEOD(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), 0, otbDTrn.Text);
            //tResult1 = oEOD.C_POSTtEOD(otbDEOD.Text, otbPlantEOD.Text,"MANUAL");

            //atResult = tResult1.Split('|');
            //tResult1 = atResult[0] + Environment.NewLine;
            //tResult2 = atResult[1] + Environment.NewLine;

            //oC_Log.Debug("[RES Manual DaySummary Status]=" + tResult2 + "[Message]=" + tResult1);
            //MessageBox.Show("[RES Manual DaySummary Status]=" + tResult2 + "[Message]=" + tResult1, "Manual DaySummary", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //if (tResult2 == "200")
            //{
            //    //check staclode and update flag
            //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='"+ otbDEOD.Text + "' AND  FTPlantCode='"+ otbPlantEOD.Text + "' AND FTStaSentOnOff='4' ";
            //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
            //    if (oDtChk.Rows.Count == 1)
            //    {
            //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
            //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
            //    }
            //}

        }

        #endregion "BUTTON_EOD"

        private void ocmRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                W_SETxDataToGridCon();
            }
            catch { }
        }

        private void ocmAdd_Click(object sender, EventArgs e)
        {
            wEditConnection oEditConnection;
            try
            {
                oEditConnection = new wEditConnection(null, ogdDataConnection.CurrentCell.RowIndex);
                oEditConnection.Show();
            }
            catch { }
        }

        private void ocmTestConnection_Click(object sender, EventArgs e)
        {
            DataSet oDataSet = new DataSet();
            SqlConnection oDbCon;
            string tXmlFilePath = @"dbconfig.xml", tConnection = "";
            try
            {
                oDataSet.ReadXml(tXmlFilePath, XmlReadMode.InferSchema);

                foreach (DataTable oTable in oDataSet.Tables)
                {
                    tConnection += @"Data Source = " + oTable.Rows[ogdDataConnection.CurrentCell.RowIndex][0].ToString() + ";";
                    tConnection += @"Initial Catalog = " + oTable.Rows[ogdDataConnection.CurrentCell.RowIndex][1].ToString() + ";";
                    tConnection += @"User ID = " + oTable.Rows[ogdDataConnection.CurrentCell.RowIndex][2].ToString() + ";";
                    tConnection += @"Password = " + oTable.Rows[ogdDataConnection.CurrentCell.RowIndex][3].ToString() + ";";
                }

                oDbCon = new SqlConnection(tConnection);

                try
                {
                    oDbCon.Open();
                    MessageBox.Show("Connection OK");
                }
                catch (Exception oEx)
                {
                    MessageBox.Show("Connection Error :" + oEx.Message);
                }
            }
            catch { }
        }

        private void ocmEdit_Click(object sender, EventArgs e)
        {
            try
            {
                ogdDataConnection_CellDoubleClick();
            }
            catch { }
        }

        private void ocmAct_Click(object sender, EventArgs e)
        {
            //string
            if (olaSchSta2.Text == "Disable")
            {
                olaSchSta2.Text = "Enable";
                ocmAct.Text = "Enable";
                olaSta.Text = "Enable";
                otmStart.Interval = (Convert.ToInt32(otbShcSS.Text) * 1000);
                nW_CDStr = Convert.ToInt32(otbShcSS.Text);

                nW_CDWait = Convert.ToInt32(otbShcSS.Text);
                otmWait.Enabled = true;
                label3.Text = label3.Text = "Off";

                otmStart.Enabled = true;
            }
            else
            {
                olaSchSta2.Text = "Disable";
                ocmAct.Text = "Disable";
                olaSta.Text = "Enable";
                otmStart.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                W_SETtEOD("AUTO");
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxCash("AUTO");
            }
            catch { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxEDC("AUTO");
            }
            catch { }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxBankIn("AUTO");
            }
            catch { }
        }

        #endregion "BUTTON"

        #region "TIME"

        private void otmStart_Tick(object sender, EventArgs e)
        {
            string tSale = "", tDaySum = "";
            try
            {
                label3.Text = label3.Text = "On";

                //=============Sale==========
                tSale = W_GETtSale();
                //=============Sale==========

                tDaySum = W_SETtEOD("AUTO");

                if (tDaySum == "OK")
                {
                    W_GETxCash("AUTO");
                    W_GETxEDC("AUTO");
                    W_GETxBankIn("AUTO");
                }

                otmWait.Enabled = true;
                nW_CDWait = Convert.ToInt32(otbShcSS.Text);
                label3.Text = label3.Text = "Off";
            }
            catch { }  
        }

        private void otmWait_Tick(object sender, EventArgs e)
        {
            nW_CDWait--;
            label3.Text = label3.Text = nW_CDWait.ToString() + " Wait";
            if (nW_CDWait == 0)
            {
                otmStart.Enabled = true;
                otmWait.Enabled = false;
                nW_CDStr = Convert.ToInt32(otbShcSS.Text);
            }
        }

        #endregion "TIME"

        #region "GRID"

        private void ogdDataConnection_CellDoubleClick(object sender = null, DataGridViewCellEventArgs e = null)
        {
            List<string> atValue = new List<string>();
            wEditConnection oEditConnection;
            try
            {
                for (int nLoopCol = 0; nLoopCol < ogdDataConnection.ColumnCount; nLoopCol++)
                {
                    atValue.Add(ogdDataConnection.Rows[ogdDataConnection.CurrentCell.RowIndex].Cells[nLoopCol].Value.ToString());
                }

                oEditConnection = new wEditConnection(atValue, ogdDataConnection.CurrentCell.RowIndex);
                oEditConnection.Show();
            }
            catch { }
        }

        private void ogdSale_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdSale,"Sale");
        }

        private void ogdRdm_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdRdm, "Redeem");
        }

        private void ogdBnk_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdBnk, "Bank");
        }

        private void SETxPrcSelectGrd(int pnRow ,int pnCol, DataGridView ogdView,string ptType)
        {
            try
            {
                DataGridViewCheckBoxCell oChk = (DataGridViewCheckBoxCell)ogdView.Rows[pnRow].Cells[0];
                bool bSelected = Convert.ToBoolean(oChk.Value);
                if (bSelected == false)
                {
                    oChk.Value = true;
                    ogdView.EndEdit();
                   // MessageBox.Show(ptType+":FTShdTransNo:"+ ogdView.Rows[pnRow].Cells["FTShdTransNo"].Value +":Select");
                }
                else if (bSelected == true)
                {
                    oChk.Value = false;
                    ogdView.EndEdit();
                   // MessageBox.Show(ptType + ":FTShdTransNo:" + ogdView.Rows[pnRow].Cells["FTShdTransNo"].Value + ":Unselect");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("wTestCallDll:SETxPrcSelectGrd = "+ex.Message, ptType, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion "GRID"

        #region"RADIO"

        // ---------------------------- Sale -----------------------------------
        private void ordAllSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                ogdSale.DataSource = oW_DtSale;
            }
        }

        private void ordSendSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtSale);
                oViewSale.RowFilter = "FTStaSend = 'Sent' ";
                ogdSale.DataSource = oViewSale;
            }
        }

        private void ordUnSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtSale);
                oViewSale.RowFilter = "FTStaSend = 'Uncen' ";
                ogdSale.DataSource = oViewSale;
            }
        }

        // ---------------------------- Sale -----------------------------------
        // ---------------------------- Bank -----------------------------------
        private void ordAllBnk_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                ogdBnk.DataSource = oW_DtBnk;
            }
        }

        private void ordSendBnk_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtBnk);
                oViewSale.RowFilter = "FTStaSend = 'Sent' ";
                ogdBnk.DataSource = oViewSale;
            }
        }

        private void ordUnBnk_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtBnk);
                oViewSale.RowFilter = "FTStaSend = 'Uncen' ";
                ogdBnk.DataSource = oViewSale;
            }
        }
        // ---------------------------- Bank -----------------------------------

        #endregion"RADIO"

        #region "TEXT"

        //------------------------- Sale -----------------------------------
        private void otbPlantSale_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchSale.PerformClick();
            }
        }

        private void otbTrnDSale_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchSale.PerformClick();
            }
        }

        private void otbTerNoSale_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchSale.PerformClick();
            }
        }

        //------------------------- Redeem -----------------------------------
        private void otbDateRdm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchRdm.PerformClick();
            }
        }

        private void otbNoRdm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchRdm.PerformClick();
            }
        }

        private void otbIdRdm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchRdm.PerformClick();
            }
        }

        //------------------------- Bank -----------------------------------
        private void otbDBnk_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchBnk.PerformClick();
            }
        }

        private void otbPlantBnk_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ocmSchBnk.PerformClick();
            }
        }

        #endregion "TEXT"

        private void otaMain1_Selected(object sender, TabControlEventArgs e)
        {
            //string tTabName = "";
            //try
            //{
            //    tTabName = otaMain1.SelectedTab.Name;

            //    if (tTabName == "otaMenu")
            //    {
            //        otcManin2.Visible = true;
            //    }               
            //    else if (tTabName == "otaDBTest")
            //    {
            //        otcManin2.Visible = false;
            //    }
            //}
            //catch { }
        }

        private void ocmSendRdm_Click_1(object sender, EventArgs e)
        {
            cRedeem oRedeem = new cRedeem();
            DataTable oDtChk = new DataTable();
            string tResult1 = "", tResult2 = "", tVal = "", tChk = "";
            string[] atResult;
            try
            {
                tVal = "(";

                foreach (DataGridViewRow oRow in ogdRdm.Rows)
                {
                    bool bIsSelected = Convert.ToBoolean(oRow.Cells["ocbSelect"].Value);
                    if (bIsSelected)
                    {
                        // FTPremiumNo,FTPreMiumID
                        tVal = tVal + "'" + oRow.Cells["FTPremiumNo"].Value.ToString()
                                         + oRow.Cells["FTPreMiumID"].Value.ToString()
                                         + '_' + cCNSP.SP_DTEtByFormat(oRow.Cells["FDRPDocDate"].Value.ToString(), "YYYYMMDD")
                                  + "',";

                    }
                }

                tVal = tVal.Substring(0, tVal.Length - 1);
                tVal = tVal + ")";
                cRcvRedeem oRcvRedeem = new cRcvRedeem()
                {
                    TypeName = "Redeem",
                    TableName = "TPSTRpremium",
                    Field = "(Trn.FTPremiumNo+Trn.FTPreMiumID+'_'+CONVERT(varchar(8),Trn.FDRPDocDate,112)) IN",
                    Value = tVal
                };
                tResult1 = oRedeem.C_POSTtRedeem(otbDTrn.Text, oRcvRedeem, "MANUAL");
                atResult = tResult1.Split('|');

                tResult1 = atResult[0] + Environment.NewLine;
                tResult2 = atResult[1] + Environment.NewLine;

                oC_Log.Debug("[RES Manual Redeem Status]=" + tResult2 + "[Message]=" + tResult1);
                MessageBox.Show("[RES Manual Redeem Status]=" + tResult2 + "[Message]=" + tResult1, "Manual Redeem", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
            //if (tResult2 == "200")
            //{
            //    //check staclode and update flag
            //    tChk = "SELECT TOP 1 FTStaSentOnOff FROM TCNMPlnCloseSta WHERE FDSaleDate ='2018-09-07' AND  FTPlantCode='17KA' AND FTStaSentOnOff='4' ";
            //    oDtChk = cCNSP.SP_SQLvExecute(tChk, tW_ConSale);
            //    if (oDtChk.Rows.Count == 1)
            //    {
            //        //tUPD = "UPDATE TCNMPlnCloseSta SET FTStaSentOnOff='1' ";
            //        //cCNSP.SP_SQLxExecute(tUPD, tW_ConSale);
            //    }
            //}

        }

        private void wTestCallDll_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Program.bPcClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void otmAutoFuc_Tick(object sender, EventArgs e)
        {
            try
            {
                W_SETtEOD("MANUAL");
            }
            catch { }
        }

        private void ocmShortManual_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxCash("MANUAL");
            }
            catch { }
        }

        private void ocmSchEDC_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxEDC("MANUAL");
            }
            catch { }
        }

        private void ocmBankIn_Click(object sender, EventArgs e)
        {
            try
            {
                W_GETxBankIn("MANUAL");
            }
            catch { }
        }
    }
}
