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
using System.ComponentModel.DataAnnotations;
namespace PullUpsDapper
{
    public class User
    {
       // [Key]
        public long IdUser { get; set; }
        public string? Name { get; set; }
        public string? Level { get; set; }
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
