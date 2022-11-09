
using System.Collections;

namespace PullUpsDapper.Users
{
    public class UserDayProgram /*: IEnumerable*/
    {
        public static bool DayReport { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }

        public UserDayProgram(int approach, int pulls)
        {
            Approach = approach;
            Pulls = pulls;
        }
    }
}
