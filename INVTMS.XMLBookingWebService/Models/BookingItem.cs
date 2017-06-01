namespace INVTMS.XMLBookingWebService.Models
{
    public class BookingItem
    {
        public string CommercialInvoice { get; set; }
        public string DivisionCode { get; set; }
        public string LotNo { get; set; }
        public string BLNo { get; set; }
        public string HAWB { get; set; }
        public string MAWB { get; set; }
        public string VesselNo { get; set; }
        public string ProductType { get; set; }
        public string PartNo { get; set; }
        public string Description { get; set; }
        public int Qty { get; set; }
        public string Unit { get; set; }
        public decimal WeightPerUnit { get; set; }
        public decimal CbmPerUnit { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public StationInfo Load { get; set; }
        public StationInfo Drop { get; set; }
    }
}