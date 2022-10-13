
namespace PullUpsDapper.TrainingProgram
{
    public class LevelProgram
    {
        public string Level { get; set; }
        public int Week { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }
        public LevelProgram(string level, int week, int approach, int pulls)
        {
            Level = level;
            Week = week;
            Approach = approach;
            Pulls = pulls;
        }
    }
}
