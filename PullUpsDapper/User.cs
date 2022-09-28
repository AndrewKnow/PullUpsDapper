using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Npgsql;
using System.Linq;
using Telegram.Bot.Types;

namespace PullUpsDapper
{
    public class User
    {
        public long IdUser { get; set; }
        public string? Name { get; set; }
    }
    public class UserDayProgram
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
