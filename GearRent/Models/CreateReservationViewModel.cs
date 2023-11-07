namespace GearRent.Models
{
    public class CreateReservationViewModel
    {
        public int CarId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string UserId { get; set; }
        public int SelectedBillingInfoId { get; set; }
        public ReservationStatus Status { get; set; }
        public List<BillingInfo> BillingInfoOptions { get; set; }
        public BillingInfo NewBillingInfo { get; set; }
    }
}
