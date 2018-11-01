using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Class
{
    public class cPOSCash
    {
        [JsonProperty("POSLog")]
        public POSLog POSLog { get; set; }
    }
    public class POSLog
    {

        [JsonProperty("@xmlns")]
        public string @xmlns { get; set; }

    //    [JsonProperty("@MajorVersion")]
    //    public string @MajorVersion { get; set; }

    //    [JsonProperty("@xmlns:xsi")]
    //    public string @xmlns:xsi { get; set; }

    //[JsonProperty("@xsi:schemaLocation")]
    //public string @xsi:schemaLocation { get; set; }

        [JsonProperty("Transaction")]
        public Transaction_Cash[][] Transaction;
    }
    public class Transaction_Cash
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
        public BusinessUnit_Cash BusinessUnit { get; set; }

        //Check ว่า ไม่มีเลยตัดทิ้ง
        [JsonProperty("TenderControlTransaction")]
        public TenderControlTransaction_Cash TenderControlTransaction { get; set; }

    }
  
    public class TenderControlTransaction_Cash
    {

        [JsonProperty("TillSettle")]
        //[JsonConverter(typeof(cCHKxType))]
        public IList<TillSettle_Cash> TillSettle { get; set; }

    }
    public class TillSettle_Cash
    {
        [JsonProperty("TenderSummary")]
        public TenderSummary_Cash TenderSummary { get; set; }
    }
    public class TenderSummary_Cash
    {

        [JsonProperty("@LedgerType")]
        public string @LedgerType { get; set; }

        [JsonProperty("Over")]
        public  Over Over { get; set; }
        //    "Over" : {
        //"@TenderType":"T001",
        //"Amount":"7506.75",
        //"BusinessDate":"2018-09-11"
        //}
    }
    public class Over
    {
        [JsonProperty("@TenderType")]
        public string @TenderType { get; set; }
        [JsonProperty("Amount")]
        public string Amount { get; set; }
        [JsonProperty("BusinessDate")]
        public string BusinessDate { get; set; }
    }

    public class Sale_Cash
    {

        [JsonProperty("@TenderType")]
        public string @TenderType { get; set; }

        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("BusinessUnit")]
        public BusinessUnit_Cash BusinessUnit { get; set; }
    }
    public class BusinessUnit_Cash
    {

        [JsonProperty("UnitID")]
        public string UnitID { get; set; }
    }

    public class cCHKxType : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // throw new NotImplementedException();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader poJsonRD, Type poType, object poExistingValue, JsonSerializer poSerializer)
        {
            object oVal = new Object();
            if (poJsonRD.TokenType == JsonToken.StartObject)
            {

            }
            else if (poJsonRD.TokenType == JsonToken.StartArray)
            {
                oVal = poSerializer.Deserialize(poJsonRD, poType);
            }
            return oVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

    }
}
