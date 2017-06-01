namespace INVTMS.BsLogic
{
    public class InvReference
    {
        public string InvoiceNo { get; set; }
        public string DocRef { get; set; }
        public string ShipmentType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}", ShipmentType, InvoiceNo, DocRef);
        }
    }
}