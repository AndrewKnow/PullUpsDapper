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
    public class ForUserReport
    {
        public int Week { get; set; }
        public int Plan { get; set; }
        public int Fact { get; set; }
        public IList<FactPulls> Facts { get; set; }
        public ForUserReport (int week, int plan, int fact)
        {
            Week = week;
            Plan = plan;
            Fact = fact;         
        }
    }

    public class PlanPulls
    {
        public int Week { get; set; }
        public int PullsPlan { get; set; }
        public IList<FactPulls> Facts { get; set; }
    }
    public class FactPulls
    {
        public int PullsFact { get; set; }
    }
}
