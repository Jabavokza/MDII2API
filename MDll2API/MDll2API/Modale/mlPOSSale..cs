using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDll2API.Modale
{
    public class mlPOSSale
    {
        [JsonProperty("POSLog")]
        public POSLog_Sale POSLog { get; set; }
    }
    public class BusinessUnit_Sale
    {

        [JsonProperty("UnitID")]
        public string UnitID { get; set; }
    }

    public class Reference
    {

        [JsonProperty("LocationID")]
        public string LocationID { get; set; }

        [JsonProperty("LocationDescription")]
        public string LocationDescription { get; set; }
    }

    public class POSIdentity
    {

        [JsonProperty("@POSIDType")]
        public string @POSIDType { get; set; }

        [JsonProperty("POSItemID")]
        public string POSItemID { get; set; }
    }

    public class Quantity
    {

        [JsonProperty("@UnitOfMeasureCode")]
        public string @UnitOfMeasureCode { get; set; }

        [JsonProperty("$")]
        public string Quantity_Value { get; set; }
    }

    public class Associate
    {

        [JsonProperty("AssociateID")]
        public string AssociateID { get; set; }
    }

    public class TaxableAmount
    {

        [JsonProperty("@TaxIncludedInTaxableAmountFlag")]
        public string @TaxIncludedInTaxableAmountFlag { get; set; }

        [JsonProperty("$")]
        public string TaxableAmount_Value { get; set; }
    }

    public class Tax
    {

        [JsonProperty("@TaxType")]
        public string @TaxType { get; set; }

        [JsonProperty("SequenceNumber")]
        public string SequenceNumber { get; set; }

        [JsonProperty("TaxableAmount")]
        public TaxableAmount TaxableAmount { get; set; }

        [JsonProperty("TaxablePercentage")]
        public string TaxablePercentage { get; set; }

        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("Percent")]
        public string Percent { get; set; }

        [JsonProperty("TaxRuleID")]
        public string TaxRuleID { get; set; }
    }

    public class Amount
    {

        [JsonProperty("@Action")]
        public string @Action { get; set; }

        [JsonProperty("$")]
        public string Amount_Value { get; set; }
    }

    public class RetailPriceModifier
    {

        [JsonProperty("SequenceNumber")]
        public string SequenceNumber { get; set; }

        [JsonProperty("Amount")]
        public Amount Amount { get; set; }

        [JsonProperty("PromotionID")]
        public string PromotionID { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("PromotionArea")]
        public string PromotionArea { get; set; }

        [JsonProperty("PromotionProfile")]
        public string PromotionProfile { get; set; }
    }

    public class Sale_Sale
    {

        [JsonProperty("POSIdentity")]
        public POSIdentity POSIdentity { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("UnitListPrice")]
        public string UnitListPrice { get; set; }

        [JsonProperty("RegularSalesUnitPrice")]
        public string RegularSalesUnitPrice { get; set; }

        [JsonProperty("ActualSalesUnitPrice")]
        public string ActualSalesUnitPrice { get; set; }

        [JsonProperty("ExtendedAmount")]
        public string ExtendedAmount { get; set; }

        [JsonProperty("Quantity")]
        public Quantity Quantity { get; set; }

        [JsonProperty("Associate")]
        public Associate Associate { get; set; }

        [JsonProperty("Tax")]
        public Tax Tax { get; set; }

        [JsonProperty("SerialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("RetailPriceModifier")]
        public IList<RetailPriceModifier> RetailPriceModifier { get; set; }
    }

    public class CreditDebit
    {

        [JsonProperty("CardClassification")]
        public string CardClassification { get; set; }

        [JsonProperty("CreditCardCompany")]
        public string CreditCardCompany { get; set; }

        [JsonProperty("PrimaryAccountNumber")]
        public string PrimaryAccountNumber { get; set; }

        [JsonProperty("CardName")]
        public string CardName { get; set; }

        [JsonProperty("CardBrand")]
        public string CardBrand { get; set; }

        [JsonProperty("Platinum")]
        public string Platinum { get; set; }
    }

    public class Tender
    {

        [JsonProperty("@TenderType")]
        public string @TenderType { get; set; }

        [JsonProperty("@SequenceNumber")]
        public string @SequenceNumber { get; set; }

        [JsonProperty("Amount")]
        public object Amount { get; set; }

        [JsonProperty("CreditDebit")]
        public CreditDebit CreditDebit { get; set; }

        [JsonProperty("Reference")]
        public string Reference { get; set; }
    }

    public class LineItem
    {

        [JsonProperty("Sale")]
        public Sale_Sale Sale { get; set; }

        [JsonProperty("SequenceNumber")]
        public string SequenceNumber { get; set; }

        [JsonProperty("Tender")]
        public Tender Tender { get; set; }
    }

    public class Customer
    {

        [JsonProperty("CustomerID")]
        public string CustomerID { get; set; }
    }

    public class RetailTransaction
    {

        [JsonProperty("@ItemType")]
        public string @ItemType { get; set; }

        [JsonProperty("LineItem")]
        public IList<LineItem> LineItem { get; set; }

        [JsonProperty("Customer")]
        public Customer Customer { get; set; }
    }

    public class Transaction_Sale
    {

        [JsonProperty("BusinessUnit")]
        public BusinessUnit_Sale BusinessUnit { get; set; }

        [JsonProperty("WorkstationID")]
        public string WorkstationID { get; set; }

        [JsonProperty("SequenceNumber")]
        public string SequenceNumber { get; set; }

        [JsonProperty("OperatorID")]
        public string OperatorID { get; set; }

        [JsonProperty("CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("Reference")]
        public Reference Reference { get; set; }

        [JsonProperty("RetailTransaction")]
        public RetailTransaction RetailTransaction { get; set; }

        [JsonProperty("BusinessDayDate")]
        public string BusinessDayDate { get; set; }

        [JsonProperty("BeginDateTime")]
        public string BeginDateTime { get; set; }

        [JsonProperty("EndDateTime")]
        public string EndDateTime { get; set; }
    }

    public class POSLog_Sale
    {

        [JsonProperty("@xmlns")]
        public string @xmlns { get; set; }

        [JsonProperty("@MajorVersion")]
        public string @MajorVersion { get; set; }

        [JsonProperty("@xmlns:xsi")]
        public string @XmlnsXsi { get; set; }

        [JsonProperty("@xsi:schemaLocation")]
        public string @XsiSchemaLocation { get; set; }

        [JsonProperty("Transaction")]
        public IList<Transaction_Sale> Transaction { get; set; }
    }

   
}
