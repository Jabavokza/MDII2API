using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace POSLOG.X_Class
{
    

    [XmlRoot(ElementName = "NewDataSet")]
    public class cDbConfig
    {
        [XmlElement(ElementName = "dbconfig")]
        public List<Dbconfig> Dbconfig { get; set; }

    }
    [XmlRoot(ElementName = "dbconfig")]
    public class Dbconfig
    {
        public string tConntect = "";

        [XmlElement(ElementName = "Server")]
        public string Server { get; set; }
        [XmlElement(ElementName = "DBName")]
        public string DBName { get; set; }
        [XmlElement(ElementName = "User")]
        public string User { get; set; }
        [XmlElement(ElementName = "Password")]
        public string Password { get; set; }
        [XmlElement(ElementName = "TopRow")]
        public string TopRow { get; set; }
        [XmlElement(ElementName = "UrlApi")]
        public string UrlApi { get; set; }
        [XmlElement(ElementName = "UsrApi")]
        public string UsrApi { get; set; }
        [XmlElement(ElementName = "PwdApi")]
        public string PwdApi { get; set; }
        [XmlElement(ElementName = "GroupIndex")]
        public string GroupIndex { get; set; }
        [XmlElement(ElementName = "WorkStationID")]
        public string WorkStationID { get; set; }
        [XmlElement(ElementName = "WorkStation")]
        public string WorkStation { get; set; }
        [XmlElement(ElementName = "PointValue")]
        public string PointValue { get; set; }
        [XmlElement(ElementName = "VendorCode")]
        public string VendorCode { get; set; }
        [XmlElement(ElementName = "VendorDes")]
        public string VendorDes { get; set; }

        [XmlElement(ElementName = "DepositCode")]
        public string DepositCode { get; set; }
        [XmlElement(ElementName = "DepositDes")]
        public string DepositDes { get; set; }

        [XmlElement(ElementName = "Edit1")]
        public string Edit1 { get; set; }

        public string Conntect
        {
            get
            {
                tConntect = "Data Source = " + Server;
                tConntect = tConntect + Environment.NewLine + ";Initial Catalog = " + DBName;
                tConntect = tConntect + Environment.NewLine + ";Persist Security Info=True;User ID = " + User;
                tConntect = tConntect + Environment.NewLine + ";Password =" + Password;
                return tConntect;
            }
           
        }
    }
    public class Dbconfig1
    {
        public string GroupNo { get; set; }
        public string Connection { get; set; }
    }
    [XmlRoot(ElementName = "poscenter")]
    public class Poscenter
    {
        string tEdit = "Edit";
        [XmlElement(ElementName = "Server")]
        public string Server { get; set; }
        [XmlElement(ElementName = "DBName")]
        public string DBName { get; set; }
        [XmlElement(ElementName = "User")]
        public string User { get; set; }
        [XmlElement(ElementName = "Password")]
        public string Password { get; set; }
        [XmlElement(ElementName = "DbOwner")]
        public string DbOwner { get; set; }
        [XmlElement(ElementName = "TopRow")]
        public string TopRow { get; set; }
        [XmlElement(ElementName = "UrlApi")]
        public string UrlApi { get; set; }
        [XmlElement(ElementName = "UsrApi")]
        public string UsrApi { get; set; }
        [XmlElement(ElementName = "PwdApi")]
        public string PwdApi { get; set; }
        [XmlElement(ElementName = "GroupIndex")]
        public string GroupIndex { get; set; }
        [XmlElement(ElementName = "WorkStationID")]
        public string WorkStationID { get; set; }
        [XmlElement(ElementName = "WorkStation")]
        public string WorkStation { get; set; }
        [XmlElement(ElementName = "PointValue")]
        public string PointValue { get; set; }

        [XmlElement(ElementName = "Edit1")]
        public string Edit1
        {
            set
            {
                tEdit = "Edit";
            }
            get
            {
                return tEdit;
            }

        }
    }

}
