namespace INVTMS.BsLogic
{
    public class CommercialInvoice
    {
        public string COMMERCIAL_INV_NO { get; set; }
        public int? LINE_NO { get; set; }
        public string COMMODITY_DESC { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string CORP_CODE { get; set; }
        public string DIVISION_ID { get; set; }
        public int? LOAD_LOCATION_ID { get; set; }
        public int? DROP_LOCATION_ID { get; set; }
        public decimal? GROSS_WEIGHT { get; set; }
        public int? NUMBER_OF_PACKAGE { get; set; }
        public string UOM_PACKAGE { get; set; }
        public decimal? PACK_HEIGHT { get; set; }
        public decimal? PACK_LENGTH { get; set; }
        public decimal? PACK_WIDTH { get; set; }
        public decimal? VOLUME { get; set; }
        public string BATCH_NO { get; set; }
        public string SHIPMENT_TYPE { get; set; }
        public string SHIPMENT_MODE { get; set; }
    }
}