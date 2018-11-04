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

       
    }
}
