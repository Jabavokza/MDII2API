using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WinAppTest.ST_Class
{
    public class cCNVB
    {
        public static bool bIntoUser = false;

        public static string tUserName = "";

        public static string tTDuration = "";

        public static string tConStr = "";

        public static string tLang = "EN";

        public static string tTdate = "FDDateUpd,FTTimeUpd,FTWhoUpd,FDDateIns, FTTimeIns, FTWhoIns";

        public static string tFdate = "CONVERT(VARCHAR(10),GETDATE(),121),CONVERT(VARCHAR(8),GETDATE(),108),'admin',CONVERT([VARCHAR](10),GETDATE(),(121)),CONVERT([VARCHAR](8),GETDATE(),(108)),'admin'";

        public static string tTdate1 = "FDDateIns, FTTimeIns, FTWhoIns";

        public static string tFdate1 = "CONVERT([VARCHAR](10),GETDATE(),(121)),CONVERT([VARCHAR](8),GETDATE(),(108)),'admin'";

        public static string tUPDdate = "FDDateUpd=CONVERT(VARCHAR(10),GETDATE(),121),FTTimeUpd=CONVERT(VARCHAR(8),GETDATE(),108),FTWhoUpd ='admin'";

        public static DialogResult oDialogResult;
    }
}
