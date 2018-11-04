using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Modale
{
    public class mlPOSBankDeposit
    {
        [JsonProperty("POSLog")]
        public mlPOSLog_BankDeposit oML_POSLog { get; set; }
    }
    public class mlPOSLog_BankDeposit
    {

        [JsonProperty("@xmlns")]
        public string tML_xmlns { get; set; }

        [JsonProperty("@MajorVersion")]
        public string tML_MajorVersion { get; set; }

        [JsonProperty("@xmlns:xsi")]
        public string tML_XmlnsXsi { get; set; }

        [JsonProperty("@xsi:schemaLocation")]
        public string tML_XsiSchemaLocation { get; set; }

        //[JsonProperty("Transaction")]
        //public IList<Transaction_BankDeposit> Transaction { get; set; }
        //[JsonProperty("Transaction")]
        //public IList<IList<Transaction_BankDeposit>> Transaction { get; set; }
        [JsonProperty("Transaction")]
        public mlTransaction_BankDeposit[][] Transaction;
    }

    public class mlTransaction_BankDeposit
    {
        [JsonProperty("WorkstationID")]
        public string tML_WorkstationID { get; set; }

        [JsonProperty("SequenceNumber")]
        public string tML_SequenceNumber { get; set; }

        [JsonProperty("OperatorID")]
        public string tML_OperatorID { get; set; }

        [JsonProperty("CurrencyCode")]
        public string tML_CurrencyCode { get; set; }

        [JsonProperty("BusinessDayDate")]
        public string tML_BusinessDayDate { get; set; }

        [JsonProperty("BusinessUnit")]
        public mlBusinessUnit_BankDeposit oML_BusinessUnit { get; set; }

        [JsonProperty("TenderControlTransaction")]
        public mlTenderControlTransaction oML_TenderControlTransaction { get; set; }
        
    }
    public class mlBusinessUnit_BankDeposit
    {
        [JsonProperty("UnitID")]
        public string tML_UnitID { get; set; }
    }
    public class mlTenderControlTransaction
    {
        [JsonProperty("TillSettle")]
        public mlTillSettle oML_TillSettle { get; set; }
    }
    public class mlBusinessUnit
    {

        [JsonProperty("UnitID")]
        public string tML_UnitID { get; set; }
    }

    public class mlSale_BankDeposit
    {

        [JsonProperty("@TenderType")]
        public string tML_TenderType { get; set; }

        [JsonProperty("Amount")]
        public string tML_Amount { get; set; }

        [JsonProperty("BusinessUnit")]
        public mlBusinessUnit oML_BusinessUnit { get; set; }
    }

    public class mlDeposit
    {

        [JsonProperty("Amount")]
        public string tML_Amount { get; set; }

        [JsonProperty("GLAccount")]
        public string tML_GLAccount { get; set; }

        [JsonProperty("Reference")]
        public string tML_Reference { get; set; }

        [JsonProperty("BankInDate")]
        public string tML_BankInDate { get; set; }

        [JsonProperty("BusinessDate")]
        public string tML_BusinessDate { get; set; }
    }

    public class mlTenderSummary
    {

        [JsonProperty("@LedgerType")]
        public string tML_LedgerType { get; set; }

        [JsonProperty("Sales")]
        public IList<mlSale_BankDeposit> aML_Sales { get; set; }

        [JsonProperty("Deposit")]
        public mlDeposit oML_Deposit { get; set; }
    }

    public class mlTillSettle
    {

        [JsonProperty("TenderSummary")]
        public mlTenderSummary oML_TenderSummary { get; set; }
    }

}
