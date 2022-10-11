
namespace PullUpsDapper.DayResults
{
    public class DayResult
    {
        public long Id { get; set; }
        public int Week { get; set; }
        public DateTime Date { get; set; }
        public int Pulls { get; set; }
        public DayResult(long id, int week, DateTime date, int pulls)
        {
            Id = id;
            Week = week;
            Date = date;
            Pulls = pulls;
        }
    }
}
