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
    public class UserDayResult
    {
        public DateTime Date { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }

        public UserDayResult(DateTime date, int approach, int pulls)
        {
            Date = date;
            Approach = approach;
            Pulls = pulls;  
        }

    }

}
