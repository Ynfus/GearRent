namespace GearRent.Models
{
    public class BillModel
    {
        public int BillId { get; set; }
        public string Name { get; set; }
        public decimal TotalValue { get; set; }
        public int Days { get; set; }
        public string Date { get; set; }
        public string CarModel { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
