﻿using MDll2API;
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
using System.Linq;
using MDll2API.Class.POSLog;
using MDll2API.Modale.ReceivApp;
using MDll2API.Class.ST_Class;
using log4net;
using cCNSP = MDll2API.Class.ST_Class.cCNSP;
using MDll2API.Modale.POSLog;
using POSLOG.X_Class;
using System.Reflection;

namespace POSLOG.From
{
    public partial class wMain : Form
    {
        private readonly ILog oC_Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int nW_CDStr = 10;
        private int nW_CDWait = 10;
        private mlRESMsg oRESMsg;

        private DataTable oW_DtSale1 = new DataTable();
        private DataTable oW_DtRdm = new DataTable();
        private DataTable oW_DtBnk = new DataTable();

        private DataGridViewCheckBoxColumn oW_ocbColSale = new DataGridViewCheckBoxColumn();
        private DataGridViewCheckBoxColumn oW_ocbColRdm = new DataGridViewCheckBoxColumn();
        private DataGridViewCheckBoxColumn oW_ocbColBnk = new DataGridViewCheckBoxColumn();

        private DataGridViewLinkColumn oW_olcCol2 = new DataGridViewLinkColumn();

        private DataGridView oW_AnotherGrid = new DataGridView();
        // private DataGridViewCell oCell1= null;

        private cDbConfig oC_DbConfig = new cDbConfig();

        private string tW_Dbcon1 = "";
        private string tW_Dbcon2 = "";
        private string tW_ConRdm = "";
        private string tW_VenDorCodeSale = "";
        private string tW_VenDes = "";
        private string tW_DepositCode = "";
        private string tW_DepositDes = "";
        private string tW_TableCase = "";

        public wMain()
        {
            InitializeComponent();
        }

        #region "FORM"

        private void wMain_Load(object sender, EventArgs e)
        {
            try
            {
                // Set User Password 

                ontMain.Visible = false;
                this.Text = "POSLOG v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
                SETxConfigDB();
                //----------- ลบ ทิ้งที่หลัง -------------------
                otbDTrn.Text = cCNSP.SP_DTEtByFormat(DateTime.Now.ToString(), "YYYY-MM-DD");
                //----------- ลบ ทิ้งที่หลัง -------------------
                W_SETxDataToGridCon();

                // MIke
                oW_AnotherGrid = ogdSale;
                tW_TableCase = "TPSTSalHD";

                //SET Schedule Task Status
                olaSchSta.ForeColor = System.Drawing.Color.Red;
                olaCountDown.Visible = false;
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

        private void wTestCallDll_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Program.bPcClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        #endregion "FORM"

        private void SETxConfigDB()
        {
            XmlSerializer oXmlSrl = new XmlSerializer(typeof(cDbConfig));
            StreamReader oSr = null;
            try
            {
                var tPath = "dbconfig.xml";

                oSr = new StreamReader(tPath);
                oC_DbConfig = (cDbConfig)oXmlSrl.Deserialize(oSr);
                oSr.Close();
                try
                {
                    var oDbConfigSale = (from oObj in oC_DbConfig.Dbconfig
                                         where oObj.GroupIndex == "3"
                                         select oObj).ToList();

                    var oDbConfigRdm = (from oObj in oC_DbConfig.Dbconfig
                                        where oObj.GroupIndex == "2"
                                        select oObj).ToList();

                    tW_Dbcon1 = oDbConfigSale[0].Conntect;
                    tW_Dbcon2 = oDbConfigSale[1].Conntect;
                    tW_ConRdm = oDbConfigRdm[0].Conntect;
                    tW_VenDorCodeSale = oDbConfigSale[0].VendorCode;
                    tW_VenDes = oDbConfigSale[0].VendorDes;
                    tW_DepositCode = oDbConfigSale[0].DepositCode;
                    tW_DepositDes = oDbConfigSale[0].DepositDes;
                }
                catch (Exception)
                {

                }
            }
            catch (Exception oEx)
            {
                MessageBox.Show("wTestCallDll:SETxConfigDB = " + oEx.Message, "Config Inbound", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                oSr.Close();
            }
        }

        #region"METHOD"

        private mlRESMsg W_GEToSale(string ptMode, mlRcvSale poRcvSale, string ptTransDate, string ptShdTransNo)
        {

            oRESMsg = new mlRESMsg();
            try
            {
                var oSale = new cSale();
                if (ockAPI.Checked == true)
                {
                    oSale.CHKxAPIEnable("true");

                }

                if (ptMode.Equals("AUTO"))
                {
                    oRESMsg = oSale.C_POSToSale(ptMode, ptTransDate, null, tW_VenDorCodeSale, tW_VenDes, tW_DepositCode, tW_DepositDes, null);
                }
                else if (ptMode.Equals("MANUAL"))
                {
                    oRESMsg = oSale.C_POSToSale(ptMode, ptTransDate, poRcvSale, tW_VenDorCodeSale, tW_VenDes, tW_DepositCode, tW_DepositDes, ptShdTransNo);
                }
                oC_Log.Info(" [Sale Time] = " + oRESMsg.tML_TimeSent + " [Sale URL] = " + oRESMsg.tML_UrlApi + " [Sale FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[Sale Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg);
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }

        private mlRESMsg W_GEToCash(string ptMode, string ptTransDate, string[] patPlantCode)
        {
            //----------------------------TEST-------------------
            cCash oCash;
            DataTable oDtChk, oDbChk;
            StringBuilder oSQL;
            oRESMsg = new mlRESMsg();
            try
            {
                oDtChk = new DataTable();
                oCash = new cCash();
                oSQL = new StringBuilder();
                oDbChk = new DataTable();

                //tDateToDay = DateTime.Now.ToString("yyyy-MM-dd");

                if (ptMode == "AUTO")
                {
                    if (ockAPI.Checked == true)
                    {
                        oCash.CHKxAPIEnable("true");
                    }
                    oRESMsg = oCash.C_POSToCash(ptMode, ptTransDate, null);
                }
                else if (ptMode == "MANUAL")
                {
                    oRESMsg = oCash.C_POSToCash(ptMode, ptTransDate, patPlantCode);
                }
                oC_Log.Info(" [ShortOver Time] = " + oRESMsg.tML_TimeSent + " [ShortOver URL] = " + oRESMsg.tML_UrlApi + " [ShortOver FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[RES ShortOver Status] = " + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg);
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }

        private mlRESMsg W_GEToEDC(string ptMode, string ptTransDate, string[] patPlantCode)
        {
            cEDC oEDC;
            DataTable oDtChk, oDbChk;
            StringBuilder oSQL;
            oRESMsg = new mlRESMsg();
            try
            {
                oDtChk = new DataTable();
                oDbChk = new DataTable();
                oSQL = new StringBuilder();
                oEDC = new cEDC();

                if (ockAPI.Checked == true)
                {
                    oEDC.CHKxAPIEnable("true");
                }
                if (ptMode == "AUTO")
                {
                    oRESMsg = oEDC.C_POSToEDC(ptMode, ptTransDate, patPlantCode);
                }
                else if (ptMode == "MANUAL")
                {
                    oRESMsg = oEDC.C_POSToEDC(ptMode, ptTransDate, patPlantCode);
                }
                oC_Log.Info(" [EDC Time] = " + oRESMsg.tML_TimeSent + " [EDC URL] = " + oRESMsg.tML_UrlApi + " [EDC FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[RES EDC Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg);

                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
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

        private mlRESMsg W_GEToBankIn(string ptMode, string ptTransDate, string[] patPlantCode)
        {
            cBankDeposit oBankIn;
            oRESMsg = new mlRESMsg();
            try
            {
                oBankIn = new cBankDeposit();

                if (ockAPI.Checked == true)
                {
                    oBankIn.CHKxAPIEnable("true");
                }
                if (ptMode == "AUTO")
                {
                    oRESMsg = oBankIn.C_POSToBankDeposit(ptMode, ptTransDate, patPlantCode);
                }
                else if (ptMode == "MANUAL")
                {
                    oRESMsg = oBankIn.C_POSToBankDeposit(ptMode, ptTransDate, patPlantCode);
                }
                oC_Log.Info(" [BankIn Time] = " + oRESMsg.tML_TimeSent + " [BankIn URL] = " + oRESMsg.tML_UrlApi + " [BankIn FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[RES BankIn Status]=" + oRESMsg.tML_StatusCode + "[Message] = " + oRESMsg.tML_StatusMsg);
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }

        //private void W_GETxSaleOrder()
        //{
        //    string rtResult;
        //    cSaleOrder oSaleOrder = new cSaleOrder();
        //    string[] atResult;
        //    string tResult;
        //    int nAPIManual = 0;  // 0: Auto,1: Manual
        //    try
        //    {
        //        nAPIManual = 0;

        //        // tResult = oSaleOrder.C_POSTtSaleOrder(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);
        //        tResult = oSaleOrder.C_POSTtSaleOrder(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual, otbDTrn.Text);
        //        atResult = tResult.Split('|');
        //        rtResult = atResult[0] + Environment.NewLine;
        //        //rtResult += atResult[1] + Environment.NewLine;

        //    }
        //    catch { }
        //    finally
        //    {
        //        rtResult = null;
        //        oSaleOrder = null;
        //        atResult = null;
        //        tResult = null;
        //    }
        //}

        //private void W_GETxPoint()
        //{
        //    string rtResult;
        //    cPoint oPoint = new cPoint();
        //    string[] atResult;
        //    string tResult;
        //    int nAPIManual = 0;  // 0: Auto,1: Manual
        //    try
        //    {
        //        nAPIManual = 0;


        //        // tResult = oPoint.C_POSTtPoint(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual);
        //        tResult = oPoint.C_POSTtPoint(tW_Json, tW_URL.Trim(), tW_USER.Trim(), tW_PSS.Trim(), nAPIManual, otbDTrn.Text);
        //        atResult = tResult.Split('|');
        //        rtResult = atResult[0] + Environment.NewLine;
        //        // rtResult += atResult[1] + Environment.NewLine;

        //    }
        //    catch { }
        //    finally
        //    {
        //        rtResult = null;
        //        oPoint = null;
        //        atResult = null;
        //        tResult = null;
        //    }
        //}

        private mlRESMsg W_SEToEOD(string ptMode, string ptTransDate, string[] patPlantCode = null)
        {
            oRESMsg = new mlRESMsg();
            try
            {
                var oEOD = new cEOD();
                if (ockAPI.Checked == true)
                {
                    oEOD.CHKxAPIEnable("true");
                }
                if (ptMode == "AUTO")
                {
                    oRESMsg = oEOD.C_POSToEOD(ptMode, ptTransDate, null);
                }
                else if (ptMode == "MANUAL")
                {
                    oRESMsg = oEOD.C_POSToEOD(ptMode, ptTransDate, patPlantCode);
                }
                oC_Log.Info(" [EOD Time] = " + oRESMsg.tML_TimeSent + " [EOD URL] = " + oRESMsg.tML_UrlApi + " [EOD FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[EOD Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg +"[StatusUpdate]="+ oRESMsg.tML_StatusUpdate);
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }

        private mlRESMsg W_GEToRedeem(string ptMode, string ptTransDate, mlRcvRedeem poRcvRedeem, mlRedeem poRedeem)
        {
            oRESMsg = new mlRESMsg();
            try
            {
                var oRedeem = new cRedeem();
                if (ockAPI.Checked == true)
                {
                    oRedeem.CHKxAPIEnable("true");
                }
                if (ptMode.Equals("AUTO"))
                {
                    oRESMsg = oRedeem.C_POSToRedeem(ptMode, ptTransDate, null, null);
                }
                else if (ptMode.Equals("MANUAL"))
                {
                    oRESMsg = oRedeem.C_POSToRedeem(ptMode, ptTransDate, poRcvRedeem, poRedeem);
                }
                oC_Log.Info("[Redeem Time] = " + oRESMsg.tML_TimeSent + " [Redeem URL] = " + oRESMsg.tML_UrlApi + " [Redeem FileName] = " + oRESMsg.tML_FileName);
                oC_Log.Debug("[Redeem Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg);
                return oRESMsg;
            }
            catch (Exception oEx)
            {
                throw oEx;
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

        private void GRDxIniBnk(DataGridView poGridview)
        {
            int nInt = 1;
            //---------------------------------------  Bank ------------------------------------
            poGridview.DataSource = null;
            poGridview.Columns.Clear();
            poGridview.ColumnCount = 5;
            poGridview.AutoGenerateColumns = false;
            poGridview.RowHeadersVisible = false;

            oW_ocbColBnk.Width = 50;
            oW_ocbColBnk.HeaderText = "เลือก";
            oW_ocbColBnk.Tag = "select|เลือก";
            oW_ocbColBnk.Name = "ocbSelect";
            oW_ocbColBnk.DataPropertyName = "ocbSelect";
            //ocbCol.ReadOnly = true;
            oW_ocbColBnk.Visible = true;
            poGridview.Columns.Insert(0, oW_ocbColBnk);


            poGridview.Columns[nInt].HeaderText = "FDBdpSaleDate";
            poGridview.Columns[nInt].Name = "FDBdpDepositDate";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            poGridview.Columns[nInt].DataPropertyName = "FDBdpDepositDate";
            poGridview.Columns[nInt].SortMode = DataGridViewColumnSortMode.NotSortable;
            poGridview.Columns[nInt].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            poGridview.Columns[nInt].ReadOnly = true;
            poGridview.Columns[nInt].Visible = true;
            poGridview.Columns[nInt].Width = 100;
            nInt++;

            poGridview.Columns[nInt].HeaderText = "FTBdpPlantCode";
            poGridview.Columns[nInt].Name = "FTBdpPlantCode";
            //ogdBnk.Columns[n].Tag = "User|ผู้ใช้";
            poGridview.Columns[nInt].DataPropertyName = "FTBdpPlantCode";
            poGridview.Columns[nInt].SortMode = DataGridViewColumnSortMode.NotSortable;
            poGridview.Columns[nInt].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            poGridview.Columns[nInt].ReadOnly = true;
            poGridview.Columns[nInt].Visible = true;
            poGridview.Columns[nInt].Width = 120;
            nInt++;

            poGridview.Columns[nInt].HeaderText = "FCBdpOverShort";
            poGridview.Columns[nInt].Name = "FCBdpOverShort";
            //ogdSku.Columns[n].Tag = "User|ผู้ใช้";
            poGridview.Columns[nInt].DataPropertyName = "FCBdpOverShort";
            poGridview.Columns[nInt].SortMode = DataGridViewColumnSortMode.NotSortable;
            poGridview.Columns[nInt].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            poGridview.Columns[nInt].ReadOnly = true;
            poGridview.Columns[nInt].Visible = true;
            poGridview.Columns[nInt].Width = 100;
            nInt++;

            poGridview.Columns[nInt].HeaderText = "FTStaSentOnOff";
            poGridview.Columns[nInt].Name = "FTStaSend";
            //ogdSku.Columns[n].Tag = "Name|ชื่อ";
            poGridview.Columns[nInt].DataPropertyName = "FTStaSend";
            poGridview.Columns[nInt].SortMode = DataGridViewColumnSortMode.NotSortable;
            poGridview.Columns[nInt].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            poGridview.Columns[nInt].ReadOnly = true;
            poGridview.Columns[nInt].Visible = true;
            poGridview.Columns[nInt].Width = 120;
            nInt++;
            //---------------------------------------  Bank ------------------------------------
        }
        #endregion


        #region "BUTTON"

        #region "BUTTON_Sale"

        private void ocmSchSale_Click(object sender, EventArgs e)
        {
            string tSql1 = "", tAnd = "AND", tNewLine = "\r\n";
            bool bAndLogic = true;
            try
            {
                if (otbTrnDSale.Text.Trim() == "")
                {
                    MessageBox.Show("กรุณากรอกวันที่", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                tSql1 += "SELECT FTShdPlantCode, CONVERT(char(10), FDShdTransDate,126) AS FDShdTransDate, FTTmnNum, FTShdTransNo, FTShdTransType, FCShdTotal, CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1' THEN FTStaSentOnOff ELSE '0' END AS FTStaSentOnOff " + tNewLine;
                tSql1 += ",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'Uncen' ELSE 'Sent' END AS FTStaSend" + tNewLine;
                tSql1 += "FROM TPSTSalHD" + tNewLine;
                //if (otbPlantSale.Text == "" && otbTrnDSale.Text == "" && otbTerNoSale.Text == "")
                //{
                //    MessageBox.Show("กรุณากรอก อย่างใดอยากหนึ่ง", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (otbPlantSale.Text.Trim() != "" || otbTrnDSale.Text.Trim() != "")
                {
                    tSql1 += "WHERE ";
                }

                if (otbPlantSale.Text.Trim() != "")
                {
                    tSql1 = tSql1 + "FTShdPlantCode = '" + otbPlantSale.Text.ToUpper().Trim() + "' ";
                    bAndLogic = true;
                    //if (otbTrnDSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FDShdTransDate = '" + otbTrnDSale.Text.Trim() + "' "; }
                    //if (otbTerNoSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FTShdTransNo = '" + otbTerNoSale.Text.Trim() + "' "; }
                }

                if (otbTrnDSale.Text.Trim() != "")
                {
                    if (bAndLogic)
                    {
                        tAnd = "";
                    }
                    else
                    {
                        tAnd = "AND";
                    }

                    tSql1 = tSql1 + tAnd + " FDShdTransDate = '" + otbTrnDSale.Text.Trim() + "' ";
                    bAndLogic = false;
                    //if (otbPlantSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FTShdPlantCode = '" + otbPlantSale.Text.ToUpper().Trim() + "' "; }
                    //if (otbTerNoSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FTShdTransNo = '" + otbTerNoSale.Text.Trim() + "' "; }
                }

                if (otbTerNoSale.Text.Trim() != "")
                {
                    if (bAndLogic)
                    {
                        tAnd = "";
                    }
                    else
                    {
                        tAnd = "AND";
                    }

                    tSql1 = tSql1 + tAnd + " FTTmnNum = '" + otbTerNoSale.Text.Trim() + "' ";
                    //if (otbPlantSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FTShdPlantCode = '" + otbPlantSale.Text.ToUpper().Trim() + "' "; }
                    //if (otbTrnDSale.Text != "") { tSql1 = tSql1 + tSql2 + Environment.NewLine + " AND FDShdTransDate = '" + otbTrnDSale.Text.Trim() + "' "; }
                }

                tSql1 += tNewLine + "AND FTShdTransType IN('03', '04', '05', '06', '07', '10', '11', '14', '15', '16', '21', '22', '23', '26', '27')";
                tSql1 += tNewLine + "ORDER BY FDShdTransDate ASC";

              var  oW_DT1 = cCNSP.SP_SQLvExecute(tSql1, tW_Dbcon1);
              var  oW_DT2 = cCNSP.SP_SQLvExecute(tSql1, tW_Dbcon2);

                DataTable oDataMerge = new DataTable();
                oDataMerge.Merge(oW_DT1);
                oDataMerge.Merge(oW_DT2);
                if (oDataMerge != null && oDataMerge.Rows.Count > 0)
                {
                    //GRDxIniSale();
                    ogbSendSale.Enabled = true;

                    oDataMerge.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
                    ogdSale.DataSource = oDataMerge;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ordAllSale.Checked = true;
            }
            catch (Exception oEx){ }
        }
        private void ocmSendSale_Click(object sender, EventArgs e)
        {
            int nLoop = 0;
            bool bCheck = false;
            string tTransDate = "", tShdTransNo = "";
            cSale oSale = new cSale();
            string tVal = "";
            DataTable oDtChk = new DataTable();
            oRESMsg = new mlRESMsg();
            try
            {

                foreach (DataGridViewRow oRow in ogdSale.Rows)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ogdSale.Rows[nLoop].Cells[0].Value.ToString()))
                        {
                            bCheck = Convert.ToBoolean(ogdSale.Rows[nLoop].Cells[0].Value.ToString());
                        }
                        else
                        {
                            bCheck = false;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (bCheck)
                    {
                        tVal += "'" + oRow.Cells["FTShdPlantCode"].Value.ToString()
                                            + oRow.Cells["FTTmnNum"].Value.ToString()
                                            + oRow.Cells["FTShdTransNo"].Value.ToString()
                                            + '_' + cCNSP.SP_DTEtByFormat(oRow.Cells["FDShdTransDate"].Value.ToString(), "YYYYMMDD")
                                    + "',";

                        tVal = tVal.Substring(0, tVal.Length - 1);

                        tVal += ",";

                        tShdTransNo += "'" + oRow.Cells["FTShdTransNo"].Value.ToString() + "',";
                    }
                    tTransDate = Convert.ToDateTime(ogdSale.Rows[nLoop].Cells["FDShdTransDate"].Value.ToString()).ToString("yyyy-MM-dd");
                    nLoop++;
                }
                tVal = tVal.Substring(0, tVal.Length - 1);
                tVal += ")";

                tShdTransNo = tShdTransNo.Substring(0, tShdTransNo.Length - 1);

                mlRcvSale oRcvSale = new mlRcvSale()
                {
                    TypeName = "Sale",
                    TableName = "TPSTSalHD",
                    Field = "(HD.FTShdPlantCode+HD.FTTmnNum+HD.FTShdTransNo+'_'+CONVERT(varchar(8),HD.FDShdTransDate,112)) IN (",
                    Value = tVal
                };
                if (tVal.Length > 10)
                {
                    oRESMsg = W_GEToSale("MANUAL", oRcvSale, tTransDate, tShdTransNo);
                }
                MessageBox.Show("[RES Manual Sale Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region "BUTTON_Redeem"
        private void ocmSchRdm_Click(object sender, EventArgs e)
        {
            string tSql1 = "";
            string tSql2 = "";

            tSql1 = tSql1 + Environment.NewLine + "SELECT CONVERT(char(10), FDRPDocDate,126) AS FDRPDocDate,FTRPDocNo,FTPremiumNo,FTPreMiumID,FCPrmCondition,FTOption,FNSeqNo,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff";
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
                //GRDxIniRedeem();
                oW_DtRdm.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
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
            DataTable oDtChk = new DataTable();
            StringBuilder oSQL;
            string tVal = "", tTransDate = "", tRPDocNo = "";
            int nLoop = 0;
            bool bCheck;
            mlRedeem mlRedeem = new mlRedeem();
            mlRcvRedeem oRcvRedeem;
            try
            {
                oSQL = new StringBuilder();
                tVal = "(";

                foreach (DataGridViewRow oRow in ogdRdm.Rows)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ogdRdm.Rows[nLoop].Cells[0].Value.ToString()))
                        {
                            bCheck = Convert.ToBoolean(ogdRdm.Rows[nLoop].Cells[0].Value.ToString());
                        }
                        else
                        {
                            bCheck = false;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (bCheck)
                    {
                        tVal = tVal + "'" + oRow.Cells["FTRPDocNo"].Value.ToString()
                                           + oRow.Cells["FTPremiumNo"].Value.ToString()
                                           //+ oRow.Cells["FCPrmCondition"].Value.ToString()
                                           + oRow.Cells["FTOption"].Value.ToString()
                                           + oRow.Cells["FTPremiumID"].Value.ToString()
                                           //+ oRow.Cells["FNSeqNo"].Value.ToString()
                                           + '_' + cCNSP.SP_DTEtByFormat(oRow.Cells["FDRPDocDate"].Value.ToString(), "YYYYMMDD")
                                           + "',";

                        tVal = tVal.Substring(0, tVal.Length - 1);

                        tVal = tVal + ",";

                        mlRedeem.tML_PremiumNo = oRow.Cells["FTPremiumNo"].Value.ToString();
                        mlRedeem.tML_PremiumID = oRow.Cells["FTPreMiumID"].Value.ToString();
                        mlRedeem.tML_RPDocDate = oRow.Cells["FDRPDocDate"].Value.ToString();
                        mlRedeem.tML_RPDocNo += "'" + oRow.Cells["FTRPDocNo"].Value.ToString() + "',";
                    }

                    tTransDate = Convert.ToDateTime(ogdRdm.Rows[nLoop].Cells["FDRPDocDate"].Value.ToString()).ToString("yyyy-MM-dd");
                    nLoop++;
                }

                tVal = tVal.Substring(0, tVal.Length - 1);
                tVal = tVal + ")";

                mlRedeem.tML_RPDocNo = tRPDocNo.Substring(0, mlRedeem.tML_RPDocNo.Length - 1);
                oRcvRedeem = new mlRcvRedeem()
                {
                    TypeName = "Redeem",
                    TableName = "TPSTRpremium",
                    Field = "(Trn.FTRPDocNo+Trn.FTPremiumNo+Trn.FTOption+Trn.FTPremiumID+'_'+CONVERT(varchar(8),Trn.FDRPDocDate,112)) IN",
                    Value = tVal
                };
                var oRESMsg = W_GEToRedeem("MANUAL", tTransDate, oRcvRedeem, mlRedeem);
                MessageBox.Show("[RES Manual Redeem Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual Redeem", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region "BUTTON_Bank"
        private void ocmSchBnk_Click(object sender, EventArgs e)
        {
            string tSql1 = "", tSql2 = "", tNewLine = "\r\n";
            try
            {
                if (otbDBnk.Text.Trim() == "")
                {
                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //tSql1 = tSql1 + Environment.NewLine + "SELECT FDBdpDepositDate,FTBdpPlantCode,FCBdpOverShort,FTStaSentOnOff";
                //tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0')<> '1'  THEN 'Uncen' ELSE 'Sent' END AS FTStaSend";
                //tSql1 = tSql1 + Environment.NewLine + "FROM TPSTBankDeposit";

                if (otbDBnk.Text == "" && otbPlantBnk.Text == "")
                {
                    tSql1 = tSql1 + Environment.NewLine + "SELECT CONVERT(char(10), FDBdpSaleDate,126) AS FDBdpSaleDate,FTBdpPlantCode,FCBdpOverShort,ISNULL(FTStaSentOnOff, '0')  AS FTStaSentOnOff";
                    tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend";
                    tSql1 = tSql1 + Environment.NewLine + "FROM TPSTBankDeposit";
                    //MessageBox.Show("กรุณากรอก อย่างใดอยากหนึ่ง", "Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //return;
                }
                else
                {
                    tSql1 = tSql1 + Environment.NewLine + "SELECT CONVERT(char(10), FDBdpSaleDate,126) AS FDBdpSaleDate,FTBdpPlantCode,FCBdpOverShort,ISNULL(FTStaSentOnOff, '0')  AS FTStaSentOnOff";
                    tSql1 = tSql1 + Environment.NewLine + ",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend";
                    tSql1 = tSql1 + Environment.NewLine + "FROM TPSTBankDeposit";

                    if (otbPlantBnk.Text.Trim() != "")
                    {
                        tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FTBdpPlantCode = '" + otbPlantBnk.Text.ToUpper().Trim() + "' ";

                        if (otbDBnk.Text != "")
                        {
                            tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FDBdpDepositDate = '" + otbDBnk.Text.Trim() + "' ";
                        }
                    }
                    else if (otbDBnk.Text.Trim() != "")
                    {
                        tSql1 = tSql1 + tSql2 + Environment.NewLine + "WHERE FDBdpSaleDate = '" + otbDBnk.Text.Trim() + "' ";

                        if (otbPlantBnk.Text != "")
                        {
                            tSql1 = tSql1 + tSql2 + Environment.NewLine + "AND FTBdpPlantCode = '" + otbPlantBnk.Text.ToUpper().Trim() + "' ";
                        }
                    }
                }

                tSql1 += tNewLine + "ORDER BY FDBdpSaleDate ASC";

                var oW_DT1 = cCNSP.SP_SQLvExecute(tSql1, tW_Dbcon1);
                var oW_DT2 = cCNSP.SP_SQLvExecute(tSql1, tW_Dbcon2);

                DataTable oDataMerge = new DataTable();
                oDataMerge.Merge(oW_DT1);
                oDataMerge.Merge(oW_DT2);
                if (oDataMerge != null && oDataMerge.Rows.Count > 0)
                {
                    ogbSendSale.Enabled = true;

                    oDataMerge.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
                    ogdBnk.DataSource = oDataMerge;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch { }
        }
        private void ocmSendBnk_Click(object sender, EventArgs e)
        {
            bool bCheck;
            string tTransDate = "", tValuePlantCode = "";
            List<string> atValuePlantCodeList;
            int nLoop = 0;
            try
            {
                atValuePlantCodeList = new List<string>();

                foreach (DataGridViewRow oRow in ogdBnk.Rows)
                {
                    if (!string.IsNullOrEmpty(ogdBnk.Rows[nLoop].Cells[0].Value.ToString()))
                    {
                        bCheck = Convert.ToBoolean(ogdBnk.Rows[nLoop].Cells[0].Value.ToString());
                    }
                    else
                    {
                        bCheck = false;
                    }
                    if (bCheck)
                    {
                        tTransDate = Convert.ToDateTime(ogdBnk.Rows[nLoop].Cells["FDBdpSaleDate"].Value.ToString()).ToString("yyyy-MM-dd");
                        tValuePlantCode = ogdBnk.Rows[nLoop].Cells["FTBdpPlantCode"].Value.ToString();
                        atValuePlantCodeList.Add(tValuePlantCode);
                    }
                    nLoop++;
                }
                var oRESMsg = W_GEToBankIn("MANUAL", tTransDate, atValuePlantCodeList.ToArray());
                MessageBox.Show("[RES Manual BankIn Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual BankIn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region "BUTTON_EOD"
        private void ocmSchEOD_Click(object sender, EventArgs e)
        {
            StringBuilder oSQL;
            DataTable oDbTCNMPlnCloseSta;
            try
            {
                if (otbEOD.Text.Trim() == "")
                {
                    MessageBox.Show("กรุณากรอกวันที่", "EOD", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                oSQL = new StringBuilder();
                oDbTCNMPlnCloseSta = new DataTable();

                if (otbEOD.Text.Trim() == "" && otbPlantEOD.Text.Trim() == "")
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,FTStaEOD");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                    //MessageBox.Show("กรอกข้อมูลไม่ครบ", "Manual DaySummary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,FTStaEOD");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + otbEOD.Text + "'");

                    if (otbPlantEOD.Text.Trim() != "")
                    {
                        oSQL.AppendLine("AND FTPlantCode = '" + otbPlantEOD.Text + "'");
                    }

                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                }

                var oW_DT1 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);
                var oW_DT2 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon2);

                DataTable oDataMerge = new DataTable();
                oDataMerge.Merge(oW_DT1);
                oDataMerge.Merge(oW_DT2);
                if (oDataMerge != null && oDataMerge.Rows.Count > 0)
                {
                    
                    ogbSendSale.Enabled = true;

                    oDataMerge.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
                    ogdDaySum.DataSource = oDataMerge;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            }
            catch { }
        }
        private void ocmSendEOD_Click(object sender, EventArgs e)
        {
            bool bCheck;
            string tValueSaleDate = "", tValuePlantCode = "";
            List<string> atPlantCodeList;
            int nLoop = 0;
            try
            {
                atPlantCodeList = new List<string>();

                foreach (DataGridViewRow oRow in ogdDaySum.Rows)
                {
                    if (!string.IsNullOrEmpty(ogdDaySum.Rows[nLoop].Cells[0].Value.ToString()))
                    {
                        bCheck = Convert.ToBoolean(ogdDaySum.Rows[nLoop].Cells[0].Value.ToString());
                    }
                    else
                    {
                        bCheck = false;
                    }

                    if (bCheck)
                    {
                        tValueSaleDate = Convert.ToDateTime(ogdDaySum.Rows[nLoop].Cells["FDSaleDate"].Value.ToString()).ToString("yyyy-MM-dd");
                        tValuePlantCode = ogdDaySum.Rows[nLoop].Cells["FTPlantCode"].Value.ToString();
                        atPlantCodeList.Add(tValuePlantCode);
                    }
                    nLoop++;
                }
                var oRESMsg = W_SEToEOD("MANUAL", tValueSaleDate, atPlantCodeList.ToArray());
                MessageBox.Show("[RES Manual DaySummary Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual DaySummary", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region"BUTTON_Cash"
        private void ocmSchCash_Click(object sender, EventArgs e)
        {
            StringBuilder oSQL;
            DataTable oDbTCNMPlnCloseSta;
            try
            {
                if (otbDateShortManual.Text.Trim() == "")
                {
                    MessageBox.Show("กรุณากรอกวันที่", "Short/Over", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                oDbTCNMPlnCloseSta = new DataTable();
                oSQL = new StringBuilder();

                if (otbDateShortManual.Text.Trim() == "" && otbShortPlant.Text.Trim() == "")
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaShortOver, '0') AS FTStaShortOver");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                    //oSQL.AppendLine("WHERE FDSaleDate = '" + otbDateShortManual.Text + "'");
                    //MessageBox.Show("กรอกข้อมูลไม่ครบ", "Manual ShortOver", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaShortOver, '0') AS FTStaShortOver");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + otbDateShortManual.Text + "'");
                    oSQL.AppendLine("AND FTStaEOD = '1'");
                    //oSQL.AppendLine("AND FTStaShortOver = '0'");

                    if (!(otbShortPlant.Text == ""))
                    {
                        oSQL.AppendLine("AND FTPlantCode = '" + otbShortPlant.Text + "'");
                    }

                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                }

                var oW_DT1 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);
                var oW_DT2 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon2);

                DataTable oDataMerge = new DataTable();
                oDataMerge.Merge(oW_DT1);
                oDataMerge.Merge(oW_DT2);
                if (oDataMerge != null && oDataMerge.Rows.Count > 0)
                {

                    ogbSendSale.Enabled = true;

                    oDataMerge.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
                    ogdShortOver.DataSource = oDataMerge;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch { }
        }
        private void ocmSendCash_Click(object sender, EventArgs e)
        {
            bool bCheck;
            string tTransDate = "", tPlantCode = "";
            List<string> atPlantCodeList;
            int nLoop = 0;
            try
            {
                atPlantCodeList = new List<string>();

                foreach (DataGridViewRow oRow in ogdShortOver.Rows)
                {
                    if (!string.IsNullOrEmpty(ogdShortOver.Rows[nLoop].Cells[0].Value.ToString()))
                    {
                        bCheck = Convert.ToBoolean(ogdShortOver.Rows[nLoop].Cells[0].Value.ToString());
                    }
                    else
                    {
                        bCheck = false;
                    }

                    if (bCheck)
                    {

                        tTransDate = Convert.ToDateTime(ogdShortOver.Rows[nLoop].Cells["FDSaleDate"].Value.ToString()).ToString("yyyy-MM-dd");
                        tPlantCode = ogdShortOver.Rows[nLoop].Cells["FTPlantCode"].Value.ToString();
                        atPlantCodeList.Add(tPlantCode);
                    }

                    nLoop++;
                }
                var oRESMsg = W_GEToCash("MANUAL", tTransDate, atPlantCodeList.ToArray());
                MessageBox.Show("[RES Manual Short/Over Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual Short/Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region"BUTTON_EDC"
        private void ocmSchEDC_Click(object sender, EventArgs e)
        {
            StringBuilder oSQL;
            DataTable oDbTCNMPlnCloseSta;
            try
            {
                if (otbEDCDate.Text.Trim() == "")
                {
                    MessageBox.Show("กรุณากรอกวันที่", "EDC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                oDbTCNMPlnCloseSta = new DataTable();
                oSQL = new StringBuilder();

                if (otbEDCDate.Text.Trim() == "" && otbSchPlant.Text.Trim() == "")
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEDC, '0') AS FTStaEDC");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaEDC, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                    //MessageBox.Show("กรอกข้อมูลไม่ครบ", "Manual EDC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEDC, '0') AS FTStaEDC");
                    oSQL.AppendLine(",CASE WHEN ISNULL(FTStaEDC, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                    oSQL.AppendLine("FROM TCNMPlnCloseSta");
                    oSQL.AppendLine("WHERE FDSaleDate = '" + otbEDCDate.Text + "'");
                    oSQL.AppendLine("AND FTStaEOD = '1'");

                    if (otbSchPlant.Text.Trim() != "")
                    {
                        oSQL.AppendLine("AND FTPlantCode = '" + otbSchPlant.Text + "'");
                    }

                    oSQL.AppendLine("ORDER BY FDSaleDate ASC");
                }

                var oW_DT1 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);
                var oW_DT2 = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon2);

                DataTable oDataMerge = new DataTable();
                oDataMerge.Merge(oW_DT1);
                oDataMerge.Merge(oW_DT2);
                if (oDataMerge != null && oDataMerge.Rows.Count > 0)
                {

                    ogbSendSale.Enabled = true;

                    oDataMerge.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);
                    ogdEDC.DataSource = oDataMerge;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch { }
        }
        private void ocmSendEDC_Click(object sender, EventArgs e)
        {
            bool bCheck;
            string tValueSaleDate = "", tValuePlantCode = "", tFirstDate = "";
            List<string> atValuePlantCodeList;
            int nLoop = 0;
            try
            {
                atValuePlantCodeList = new List<string>();

                foreach (DataGridViewRow oRow in ogdEDC.Rows)
                {
                    if (!string.IsNullOrEmpty(ogdEDC.Rows[nLoop].Cells[0].Value.ToString()))
                    {
                        bCheck = Convert.ToBoolean(ogdEDC.Rows[nLoop].Cells[0].Value.ToString());
                    }
                    else
                    {
                        bCheck = false;
                    }

                    if (bCheck)
                    {
                        if (nLoop == 0)
                        {
                            tFirstDate = cCNSP.SP_DTEtByFormat(oRow.Cells["FDShdTransDate"].Value.ToString(), "YYYYMMDD");
                        }

                        if (!(tFirstDate == cCNSP.SP_DTEtByFormat(oRow.Cells["FDShdTransDate"].Value.ToString(), "YYYYMMDD")))
                        {
                            otbTrnDSale.Focus();
                            MessageBox.Show("กรุณาเลือกวันที่เดียวกัน");
                            return;
                        }

                        tValueSaleDate = Convert.ToDateTime(ogdEDC.Rows[nLoop].Cells["FDSaleDate"].Value.ToString()).ToString("yyyy-MM-dd");
                        tValuePlantCode = ogdEDC.Rows[nLoop].Cells["FTPlantCode"].Value.ToString();
                        atValuePlantCodeList.Add(tValuePlantCode);
                    }
                    nLoop++;

                }
                var oRESMsg = W_GEToEDC("MANUAL", tValueSaleDate, atValuePlantCodeList.ToArray());
                MessageBox.Show("[RES Manual EDC Status]=" + oRESMsg.tML_StatusCode + "[Message]=" + oRESMsg.tML_StatusMsg, "Manual EDC", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
        }
        #endregion

        #region"BUTTON_ALL"
        private void ocmCheckAll_Click(object sender, EventArgs e)
        {
            //DataGridViewCheckBoxCell oChk;
            try
            {
                foreach (DataGridViewRow oRow in oW_AnotherGrid.Rows)
                {
                    oW_AnotherGrid.Rows[oRow.Index].SetValues(true);
                }

                oW_AnotherGrid.EndEdit();
            }
            catch { }
        }
        #endregion

        #region"BUTTON_TAB_Connection"
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
            wSettingConnection oEditConnection;
            try
            {
                oEditConnection = new wSettingConnection(null, ogdDataConnection.CurrentCell.RowIndex);
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
            if (olaSchSta.Text.Equals("Disable"))
            {
                olaSchSta.Text = "Enable";
                olaSchSta.ForeColor = System.Drawing.Color.Green;
                olaCountDown.Visible = true;
                ocmAct.Text = "Disable";
                olaSta.Text = "Schedule : Enable";
                otmStart.Interval = (Convert.ToInt32(otbShcSS.Text) * 1000);
                nW_CDStr = Convert.ToInt32(otbShcSS.Text);

                nW_CDWait = Convert.ToInt32(otbShcSS.Text);
                otmWait.Enabled = true;
                olaCountDown.Text = "On";
                otmStart.Enabled = true;
                otbShcSS.Enabled = false;
            }
            else if (olaSchSta.Text.Equals("Enable"))
            {
                olaSchSta.Text = "Disable";
                olaSchSta.ForeColor = System.Drawing.Color.Red;
                ocmAct.Text = "Enable";
                olaSta.Text = "Schedule : Disable";
                olaCountDown.Text = "Off";
                otmStart.Enabled = false;
                otmWait.Enabled = false;
                otbShcSS.Enabled = true;
            }
        }
        #endregion

        #endregion "BUTTON"

        #region "TIME"

        private void otmStart_Tick(object sender, EventArgs e)
        {
            oRESMsg = new mlRESMsg();
            try
            {
                // var tTransDate = DateTime.Now.ToString("YYYY-MM-DD");
                var tTransDate = otbDTrn.Text;
                //  olaCountDown.Text = olaCountDown.Text = "On";
                if (ockSaleAuto.Checked == true)
                {
                    W_GEToSale("AUTO", null, tTransDate, null);
                }

                if (ockRmdAuto.Checked == true)
                {
                    W_GEToRedeem("AUTO", tTransDate, null, null);
                }
                if (ockDaySumAuto.Checked == true)
                {
                    var oRESMsg = W_SEToEOD("AUTO", tTransDate, null);
                    //if (oRESMsg.tML_StatusUpdate == "อัพเดตสำเร็จ")
                    //{
                      
                    //}
                }
                if (ockShortOverAuto.Checked == true)
                {
                    W_GEToCash("AUTO", tTransDate, null);
                }
                if (ockEDCAuto.Checked == true)
                {
                    W_GEToEDC("AUTO", tTransDate, null);
                }
                if (ockBankInAuto.Checked == true)
                {
                    W_GEToBankIn("AUTO", tTransDate, null);
                }
                otmWait.Enabled = true;
                nW_CDWait = Convert.ToInt32(otbShcSS.Text);
                //  olaCountDown.Text = olaCountDown.Text = "Off";
            }
            catch (Exception oEx)
            {
                //MessageBox.Show("wMain : //otmStart_Tick" + oEx.Message,"", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw oEx;
            }
        }

        private void otmWait_Tick(object sender, EventArgs e)
        {
            nW_CDWait--;
            olaCountDown.Text = olaCountDown.Text = nW_CDWait.ToString() + " Wait";
            if (nW_CDWait == 0)
            {
                otmStart.Enabled = true;
                otmWait.Enabled = false;
                nW_CDStr = Convert.ToInt32(otbShcSS.Text);
            }
        }

        #endregion 

        #region "GRID"

        private void ogdDataConnection_CellDoubleClick(object sender = null, DataGridViewCellEventArgs e = null)
        {
            List<string> atValue = new List<string>();
            wSettingConnection oEditConnection;
            try
            {
                for (int nLoopCol = 0; nLoopCol < ogdDataConnection.ColumnCount; nLoopCol++)
                {
                    atValue.Add(ogdDataConnection.Rows[ogdDataConnection.CurrentCell.RowIndex].Cells[nLoopCol].Value.ToString());
                }

                oEditConnection = new wSettingConnection(atValue, ogdDataConnection.CurrentCell.RowIndex);
                oEditConnection.Show();
            }
            catch { }
        }

        private void ogdSale_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdSale, "Sale");
        }

        private void ogdRdm_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdRdm, "Redeem");
        }

        private void ogdBnk_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SETxPrcSelectGrd(e.RowIndex, e.ColumnIndex, ogdBnk, "Bank");
        }

        private void SETxPrcSelectGrd(int pnRow, int pnCol, DataGridView ogdView, string ptType)
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
                MessageBox.Show("wTestCallDll:SETxPrcSelectGrd = " + ex.Message, ptType, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region"RADIO"

        // ---------------------------- Sale -----------------------------------
        private void ordAllSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                ogdSale.DataSource = oW_DtSale1;
            }
        }

        private void ordSendSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtSale1);
                oViewSale.RowFilter = "FTStaSend = 'Sent' ";
                ogdSale.DataSource = oViewSale;
            }
        }

        private void ordUnSale_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                DataView oViewSale = new DataView(oW_DtSale1);
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

        #endregion

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


        private void AllCheckBoxes_CheckedChanged(Object sender, EventArgs e)
        {
            RadioButton oRb;
            StringBuilder oSQL;
            DataTable oDbAnother;
            string tNameGridName = "";
            TextBox oAnotherBoxDate;
            try
            {
                oAnotherBoxDate = new TextBox();

                if (((RadioButton)sender).Checked)
                {
                    oRb = (RadioButton)sender;

                    switch (tW_TableCase)
                    {
                        case "TCNMPlnCloseSta":
                            if (oRb.Text == "Send")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                tNameGridName = oW_AnotherGrid.Name;

                                switch (tNameGridName)
                                {
                                    case "ogdDaySum":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate, FTPlantCode, ISNULL(FTStaEOD, '0') AS FTStaEOD");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaSentOnOff = '1'");
                                        oAnotherBoxDate = otbEOD;
                                        break;
                                    case "ogdShortOver":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaShortOver, '0') AS FTStaShortOver");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oSQL.AppendLine("AND ISNULL(FTStaShortOver, '0') = '1'");
                                        oAnotherBoxDate = otbDateShortManual;
                                        break;
                                    case "ogdEDC":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEDC, '0') AS FTStaEDC");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaEDC, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oSQL.AppendLine("AND ISNULL(FTStaEDC, '0') = '1'");
                                        oAnotherBoxDate = otbEDCDate;
                                        break;
                                }

                                if (oAnotherBoxDate.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDSaleDate = '" + oAnotherBoxDate.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDSaleDate ASC");

                                if (oAnotherBoxDate.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "Unsend")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                //oSQL.AppendLine("SELECT FDSaleDate,FTPlantCode,FTStaSentOnOff");
                                //oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                //oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                //oSQL.AppendLine("WHERE FTStaSentOnOff <> '1'");

                                tNameGridName = oW_AnotherGrid.Name;

                                switch (tNameGridName)
                                {
                                    case "ogdDaySum":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEOD, '0') AS FTStaEOD");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaSentOnOff <> '1'");
                                        oAnotherBoxDate = otbEOD;
                                        break;
                                    case "ogdShortOver":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaShortOver, '0') AS FTStaShortOver");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oSQL.AppendLine("AND ISNULL(FTStaShortOver, '0') <> '1'");
                                        oAnotherBoxDate = otbDateShortManual;
                                        break;
                                    case "ogdEDC":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEDC, '0') AS FTStaEDC");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaEDC, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oSQL.AppendLine("AND ISNULL(FTStaEDC, '0') <> '1' ");
                                        oAnotherBoxDate = otbEDCDate;
                                        break;
                                }

                                if (otbEOD.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDSaleDate = '" + otbEOD.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDSaleDate ASC");

                                if (oAnotherBoxDate.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "All")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                //oSQL.AppendLine("SELECT FDSaleDate,FTPlantCode,FTStaSentOnOff");
                                //oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                //oSQL.AppendLine("FROM TCNMPlnCloseSta");

                                tNameGridName = oW_AnotherGrid.Name;

                                switch (tNameGridName)
                                {
                                    case "ogdDaySum":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEOD, '0') AS FTStaEOD");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaSentOnOff = '1'");
                                        oAnotherBoxDate = otbEOD;
                                        break;
                                    case "ogdShortOver":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaShortOver, '0') AS FTStaShortOver");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oAnotherBoxDate = otbDateShortManual;
                                        break;
                                    case "ogdEDC":
                                        oSQL.AppendLine("SELECT CONVERT(char(10), FDSaleDate,126) AS FDSaleDate,FTPlantCode,ISNULL(FTStaEDC, '0') AS FTStaEDC");
                                        oSQL.AppendLine(",CASE WHEN ISNULL(FTStaShortOver, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                        oSQL.AppendLine("FROM TCNMPlnCloseSta");
                                        oSQL.AppendLine("WHERE FTStaEOD = '1'");
                                        oAnotherBoxDate = otbEDCDate;
                                        break;
                                }


                                if (otbEOD.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDSaleDate = '" + otbEOD.Text + "'");
                                }

                                if (!(otbPlantEOD.Text == ""))
                                {
                                    oSQL.AppendLine("AND FTPlantCode = '" + otbPlantEOD.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDSaleDate ASC");

                                if (oAnotherBoxDate.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            break;
                        case "TPSTSalHD":
                            if (oRb.Text == "Send")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT FTShdPlantCode,CONVERT(char(10), FDShdTransDate,126) AS FDShdTransDate, FTTmnNum, FTShdTransNo, FTShdTransType, FCShdTotal, CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '' THEN FTStaSentOnOff ELSE '0' END AS FTStaSentOnOff ");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTSalHD");
                                oSQL.AppendLine("WHERE FTStaSentOnOff = '1'");

                                if (otbTrnDSale.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDShdTransDate = '" + otbTrnDSale.Text + "'");
                                }

                                oSQL.AppendLine("AND FTShdTransType IN('03', '04', '05', '06', '07', '10', '11', '14', '15', '16', '21', '22', '23', '26', '27')");
                                oSQL.AppendLine("ORDER BY FDShdTransDate ASC");

                                if (otbTrnDSale.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "Unsend")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT FTShdPlantCode, CONVERT(char(10), FDShdTransDate,126) AS FDShdTransDate, FTTmnNum, FTShdTransNo, FTShdTransType, FCShdTotal, CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '' THEN FTStaSentOnOff ELSE '0' END AS FTStaSentOnOff ");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTSalHD");
                                oSQL.AppendLine("WHERE FTStaSentOnOff <> '1'");

                                if (otbTrnDSale.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDShdTransDate = '" + otbTrnDSale.Text + "'");
                                }

                                oSQL.AppendLine("AND FTShdTransType IN('03', '04', '05', '06', '07', '10', '11', '14', '15', '16', '21', '22', '23', '26', '27')");
                                oSQL.AppendLine("ORDER BY FDShdTransDate ASC");

                                if (otbTrnDSale.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "All")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT FTShdPlantCode, CONVERT(char(10), FDShdTransDate,126) AS FDShdTransDate, FTTmnNum, FTShdTransNo, FTShdTransType, FCShdTotal, CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '' THEN FTStaSentOnOff ELSE '0' END AS FTStaSentOnOff ");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTSalHD");

                                if (otbTrnDSale.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("WHERE FDShdTransDate = '" + otbTrnDSale.Text + "'");
                                }

                                if (!(otbPlantEOD.Text == ""))
                                {
                                    oSQL.AppendLine("AND FTShdPlantCode = '" + otbPlantSale.Text + "'");
                                }

                                oSQL.AppendLine("AND FTShdTransType IN('03', '04', '05', '06', '07', '10', '11', '14', '15', '16', '21', '22', '23', '26', '27')");
                                oSQL.AppendLine("ORDER BY FDShdTransDate ASC");

                                if (otbTrnDSale.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            break;
                        case "TPSTRpremium":
                            if (oRb.Text == "Send")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDRPDocDate,126) AS FDRPDocDate,FTRPDocNo,FTPremiumNo,FTPreMiumID,FCPrmCondition,FTOption,FNSeqNo,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTRpremium");
                                oSQL.AppendLine("WHERE ISNULL(FTStaSentOnOff, '0') = '1'");

                                if (otbDateRdm.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDRPDocDate = '" + otbDateRdm.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDRPDocDate ASC");

                                if (otbDateRdm.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Redeem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConRdm);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "Unsend")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDRPDocDate,126) AS FDRPDocDate,FTRPDocNo,FTPremiumNo,FTPreMiumID,FCPrmCondition,FTOption,FNSeqNo,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTRpremium");
                                oSQL.AppendLine("WHERE ISNULL(FTStaSentOnOff, '0') <> '1'");

                                if (otbDateRdm.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDRPDocDate = '" + otbDateRdm.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDRPDocDate ASC");

                                if (otbDateRdm.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Redeem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConRdm);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "All")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDRPDocDate,126) AS FDRPDocDate,FTRPDocNo,FTPremiumNo,FTPreMiumID,FCPrmCondition,FTOption,FNSeqNo,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTRpremium");

                                if (otbDateRdm.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("WHERE FDRPDocDate = '" + otbDateRdm.Text + "'");
                                }

                                if (!(otbNoRdm.Text == ""))
                                {
                                    oSQL.AppendLine("AND FTPremiumNo = '" + otbNoRdm.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDRPDocDate ASC");

                                if (otbDateRdm.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "Redeem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_ConRdm);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            break;
                        case "TPSTBankDeposit":
                            if (oRb.Text == "Send")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDBdpSaleDate,126) AS FDBdpSaleDate,FTBdpPlantCode,FCBdpOverShort,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTBankDeposit");
                                oSQL.AppendLine("WHERE FTStaSentOnOff = '1'");

                                if (otbDBnk.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDBdpDepositDate = '" + otbDBnk.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDBdpSaleDate ASC");

                                if (otbDBnk.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "Unsend")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDBdpSaleDate,126) AS FDBdpSaleDate,FTBdpPlantCode,FCBdpOverShort,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTBankDeposit");
                                oSQL.AppendLine("WHERE ISNULL(FTStaSentOnOff, '0') <> '1'");

                                if (otbDBnk.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("AND FDBdpDepositDate = '" + otbDBnk.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDBdpSaleDate ASC");

                                if (otbDBnk.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            else if (oRb.Text == "All")
                            {
                                oDbAnother = new DataTable();
                                oSQL = new StringBuilder();

                                oSQL.AppendLine("SELECT CONVERT(char(10), FDBdpSaleDate,126) AS FDBdpSaleDate,FTBdpPlantCode,FCBdpOverShort,ISNULL(FTStaSentOnOff, '0') AS FTStaSentOnOff");
                                oSQL.AppendLine(",CASE WHEN ISNULL(FTStaSentOnOff, '0') <> '1'  THEN 'UnSent' ELSE 'Sent' END AS FTStaSend");
                                oSQL.AppendLine("FROM TPSTBankDeposit");

                                if (otbDBnk.Text.Trim() != "")
                                {
                                    oSQL.AppendLine("WHERE FDBdpDepositDate = '" + otbDBnk.Text + "'");
                                }

                                if (!(otbPlantEOD.Text == ""))
                                {
                                    oSQL.AppendLine("AND FTBdpPlantCode = '" + otbPlantBnk.Text + "'");
                                }

                                oSQL.AppendLine("ORDER BY FDBdpSaleDate ASC");

                                if (otbDBnk.Text.Trim() == "")
                                {
                                    MessageBox.Show("กรุณากรอกวันที่", "BankIn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                oDbAnother = cCNSP.SP_SQLvExecute(oSQL.ToString(), tW_Dbcon1);

                                oDbAnother.Columns.Add("เลือก", typeof(Boolean)).SetOrdinal(0);

                                oW_AnotherGrid.DataSource = oDbAnother;
                            }
                            break;
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }
                }
            }
            catch (Exception) { }
        }

        private void otcManin2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tName = "";
            try
            {
                tName = otcManin2.SelectedTab.Name;

                if (tName == "otbSale")
                {
                    oW_AnotherGrid = ogdSale;
                    tW_TableCase = "TPSTSalHD";
                }
                else if (tName == "ocmRdm")
                {
                    oW_AnotherGrid = ogdRdm;
                    tW_TableCase = "TPSTRpremium";
                }
                else if (tName == "ocmDSum")
                {
                    oW_AnotherGrid = ogdDaySum;
                    tW_TableCase = "TCNMPlnCloseSta";
                }
                else if (tName == "ocmShtOvr")
                {
                    oW_AnotherGrid = ogdShortOver;
                    tW_TableCase = "TCNMPlnCloseSta";
                }
                else if (tName == "ocmEDC")
                {
                    oW_AnotherGrid = ogdEDC;
                    tW_TableCase = "TCNMPlnCloseSta";
                }
                else if (tName == "ocmBnkIn")
                {
                    oW_AnotherGrid = ogdBnk;
                    tW_TableCase = "TPSTBankDeposit";
                }
            }
            catch { }
        }

        //private void ockDaySumAuto_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (ockDaySumAuto.Checked == true)
        //    {
        //        ockShortOverAuto.Checked = true;
        //        ockEDCAuto.Checked = true;
        //        ockBankInAuto.Checked = true;
        //        ockShortOverAuto.Enabled = true;
        //        ockEDCAuto.Enabled = true;
        //        ockBankInAuto.Enabled = true;
            
        //    }
        //    else
        //    {
        //        ockShortOverAuto.Checked = false;
        //        ockEDCAuto.Checked = false;
        //        ockBankInAuto.Checked = false;
        //        ockShortOverAuto.Enabled = false;
        //        ockEDCAuto.Enabled = false;
        //        ockBankInAuto.Enabled = false;
               
        //    }
        //}

    }
}
