namespace GearRent.Models
{
    public class FilterModel
    {

        public string Search { get; set; }
        public int? Tag { get; set; }
        public float? MinEngineSize { get; set; }
        public float? MaxEngineSize { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
