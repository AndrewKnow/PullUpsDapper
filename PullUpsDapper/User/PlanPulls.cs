using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper.Users
{
    public class PlanPulls
    {
        public int Week { get; set; }
        public int PullsPlan { get; set; }
        public IList<FactPulls> Facts { get; set; }
    }
}
