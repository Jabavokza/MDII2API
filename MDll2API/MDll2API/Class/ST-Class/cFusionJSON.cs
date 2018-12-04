using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Class.ST_Class
{
    public class cFusionJSON
    {
        public StringBuilder oC_Json { get; set; }
        public cFusionJSON(string ptJsonTrn)
        {
            try
            {
                #region ""ประกอบร่าง Json"
                var oJson = new StringBuilder();
                oJson.AppendLine("{");
                oJson.AppendLine("\"POSLog\": {");
                oJson.AppendLine("\"@xmlns\" : \"http://themall.co.th/retail/sales_transaction\",");
                oJson.AppendLine("\"@MajorVersion\" : \"6\",");
                oJson.AppendLine("\"@xmlns:xsi\" : \"http://www.w3.org/2001/XMLSchema-instance\",");
                oJson.AppendLine("\"@xsi:schemaLocation\" : \"http://themall.co.th/retail/sales_transaction\",");
                oJson.AppendLine("\"Transaction\": [");
                oJson.AppendLine(ptJsonTrn);
                oJson.AppendLine("]");
                oJson.AppendLine("}");
                oJson.AppendLine("}");
                #endregion
                oC_Json = oJson;
            }
            catch (Exception oEx)
            {
                throw oEx;
            }

        }
    }
}
