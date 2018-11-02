using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Class.Modale
{
    public class mlPOSBankDeposit
    {
        [JsonProperty("POSLog")]
        public POSLog_BankDeposit POSLog { get; set; }
    }
    public class POSLog_BankDeposit
    {

        [JsonProperty("@xmlns")]
        public string @xmlns { get; set; }

        [JsonProperty("@MajorVersion")]
        public string @MajorVersion { get; set; }

        [JsonProperty("@xmlns:xsi")]
        public string @XmlnsXsi { get; set; }

        [JsonProperty("@xsi:schemaLocation")]
        public string @XsiSchemaLocation { get; set; }

        //[JsonProperty("Transaction")]
        //public IList<Transaction_BankDeposit> Transaction { get; set; }
        //[JsonProperty("Transaction")]
        //public IList<IList<Transaction_BankDeposit>> Transaction { get; set; }
        [JsonProperty("Transaction")]
        public Transaction_BankDeposit[][] Transaction;
    }

    public class Transaction_BankDeposit
    {
        [JsonProperty("WorkstationID")]
        public string WorkstationID { get; set; }

        [JsonProperty("SequenceNumber")]
        public string SequenceNumber { get; set; }

        [JsonProperty("OperatorID")]
        public string OperatorID { get; set; }

        [JsonProperty("CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("BusinessDayDate")]
        public string BusinessDayDate { get; set; }

        [JsonProperty("BusinessUnit")]
        public BusinessUnit_BankDeposit BusinessUnit { get; set; }

        [JsonProperty("TenderControlTransaction")]
        public TenderControlTransaction TenderControlTransaction { get; set; }
        
    }
    public class BusinessUnit_BankDeposit
    {
        [JsonProperty("UnitID")]
        public string UnitID { get; set; }
    }
    public class TenderControlTransaction
    {
        [JsonProperty("TillSettle")]
        public TillSettle TillSettle { get; set; }
    }
    public class BusinessUnit
    {

        [JsonProperty("UnitID")]
        public string UnitID { get; set; }
    }

    public class Sale_BankDeposit
    {

        [JsonProperty("@TenderType")]
        public string @TenderType { get; set; }

        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("BusinessUnit")]
        public BusinessUnit BusinessUnit { get; set; }
    }

    public class Deposit
    {

        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("GLAccount")]
        public string GLAccount { get; set; }

        [JsonProperty("Reference")]
        public string Reference { get; set; }

        [JsonProperty("BankInDate")]
        public string BankInDate { get; set; }

        [JsonProperty("BusinessDate")]
        public string BusinessDate { get; set; }
    }

    public class TenderSummary
    {

        [JsonProperty("@LedgerType")]
        public string @LedgerType { get; set; }

        [JsonProperty("Sales")]
        public IList<Sale_BankDeposit> Sales { get; set; }

        [JsonProperty("Deposit")]
        public Deposit Deposit { get; set; }
    }

    public class TillSettle
    {

        [JsonProperty("TenderSummary")]
        public TenderSummary TenderSummary { get; set; }
    }

}
