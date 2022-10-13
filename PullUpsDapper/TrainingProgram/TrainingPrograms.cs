
namespace PullUpsDapper.TrainingProgram
{
    public class TrainingProgram
    {
        public long Id { get; set; }
        public int Week { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }
        public TrainingProgram(long id, int week, int approach, int pulls)
        {
            Id = id;
            Week = week;
            Approach = approach;
            Pulls = pulls;
        }
    }
}
