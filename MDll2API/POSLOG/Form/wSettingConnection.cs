using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace POSLOG.From
{
    public partial class wSettingConnection : Form
    {
        private int nC_INDEX = 0;
        private string tC_Mode = "";

        public wSettingConnection(List<string> patData, int pnINDEX)
        {
            try
            {
                InitializeComponent();

                if (patData != null)
                {
                    nC_INDEX = pnINDEX;

                    otbServer.Text = patData[0];
                    otbDBname.Text = patData[1];
                    otbUser.Text = patData[2];
                    otbPassword.Text = patData[3];
                    otbTopRow.Text = patData[4];
                    otbUrlApi.Text = patData[5];
                    otbUserApi.Text = patData[6];
                    otbUserPwdApi.Text = patData[7];
                    otbGroupIndex.Text = patData[8];
                    otbWorkStationID.Text = patData[9];
                    otbWorkStation.Text = patData[10];
                    otbPointValue.Text = patData[11];

                    tC_Mode = "Edit";
                }
                else
                {
                    tC_Mode = "Add";
                }
            }
            catch { }
        }

        private void ocmCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();
            }
            catch { }
        }

        private void ocmSave_Click(object sender, EventArgs e)
        {
            XmlDocument oXmldoc = new XmlDocument();
            try
            {
                oXmldoc.Load(@"dbconfig.xml");

                switch (tC_Mode)
                {
                    case "Add":
                        //Creating childnode to root node using Xmlelement
                        XmlElement oXelement = oXmldoc.CreateElement("dbconfig");
                        oXmldoc.DocumentElement.AppendChild(oXelement);
                        //creating subnode to childnode using XmlElement
                        XmlElement oServer = oXmldoc.CreateElement("Server");
                        oServer.InnerText = otbServer.Text;
                        oXelement.AppendChild(oServer);

                        XmlElement oDBName = oXmldoc.CreateElement("DBName");
                        oDBName.InnerText = otbDBname.Text;
                        oXelement.AppendChild(oDBName);

                        XmlElement oUser = oXmldoc.CreateElement("User");
                        oUser.InnerText = otbUser.Text;
                        oXelement.AppendChild(oUser);

                        XmlElement oPassword = oXmldoc.CreateElement("Password");
                        oPassword.InnerText = otbPassword.Text;
                        oXelement.AppendChild(oPassword);

                        XmlElement oTopRow = oXmldoc.CreateElement("TopRow");
                        oTopRow.InnerText = otbTopRow.Text;
                        oXelement.AppendChild(oTopRow);

                        XmlElement oUrlApi = oXmldoc.CreateElement("UrlApi");
                        oUrlApi.InnerText = otbUrlApi.Text;
                        oXelement.AppendChild(oUrlApi);

                        XmlElement oUsrApi = oXmldoc.CreateElement("UsrApi");
                        oUsrApi.InnerText = otbUserApi.Text;
                        oXelement.AppendChild(oUsrApi);

                        XmlElement oPwdApi = oXmldoc.CreateElement("PwdApi");
                        oPwdApi.InnerText = otbUserPwdApi.Text;
                        oXelement.AppendChild(oPwdApi);

                        XmlElement oGroupIndex = oXmldoc.CreateElement("GroupIndex");
                        oGroupIndex.InnerText = otbGroupIndex.Text;
                        oXelement.AppendChild(oGroupIndex);

                        XmlElement oWorkStationID = oXmldoc.CreateElement("WorkStationID");
                        oWorkStationID.InnerText = otbWorkStationID.Text;
                        oXelement.AppendChild(oWorkStationID);

                        XmlElement oWorkStation = oXmldoc.CreateElement("WorkStation");
                        oWorkStation.InnerText = otbWorkStation.Text;
                        oXelement.AppendChild(oWorkStation);

                        XmlElement oPointValue = oXmldoc.CreateElement("PointValue");
                        oPointValue.InnerText = otbPointValue.Text;
                        oXelement.AppendChild(oPointValue);
                        break;
                    case "Edit":

                        XmlNode oXmlnode = oXmldoc.DocumentElement.ChildNodes.Item(nC_INDEX);

                        oXmlnode["Server"].InnerText = otbServer.Text;
                        oXmlnode["DBName"].InnerText = otbDBname.Text;
                        oXmlnode["User"].InnerText = otbUser.Text;
                        oXmlnode["Password"].InnerText = otbPassword.Text;
                        oXmlnode["TopRow"].InnerText = otbTopRow.Text;
                        oXmlnode["UrlApi"].InnerText = otbUrlApi.Text;
                        oXmlnode["UsrApi"].InnerText = otbUserApi.Text;
                        oXmlnode["PwdApi"].InnerText = otbUserPwdApi.Text;
                        oXmlnode["GroupIndex"].InnerText = otbGroupIndex.Text;
                        oXmlnode["WorkStationID"].InnerText = otbWorkStationID.Text;
                        oXmlnode["WorkStation"].InnerText = otbWorkStation.Text;
                        oXmlnode["PointValue"].InnerText = otbPointValue.Text;

                        break;
                    case "Delete":

                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }


                oXmldoc.Save(@"dbconfig.xml");
                this.Hide();
                MessageBox.Show("บันทึกเรียบร้อย");
            }
            catch { }
        }
    }
}
