using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper.Users
{
    public class ForUserReport
    {
        public int Week { get; set; }
        public int Plan { get; set; }
        public int Fact { get; set; }
        public string DateBegin { get; set; }
        public string DateEnd { get; set; }
        public IList<FactPulls> Facts { get; set; }
        public ForUserReport(int week, int plan, int fact, string dateBegin, string dateEnd)
        {
            Week = week;
            Plan = plan;
            Fact = fact;
            DateBegin = dateBegin;
            DateEnd = dateEnd;
        }
    }
}
