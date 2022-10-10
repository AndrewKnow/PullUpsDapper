
namespace PullUpsDapper.Users
{
    public class UserDayProgram
    {
        public static bool DayReport { get; set; }
        //public static bool DayReportPlus { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }

        public UserDayProgram(int approach, int pulls)
        {
            Approach = approach;
            Pulls = pulls;
        }
    }
}
