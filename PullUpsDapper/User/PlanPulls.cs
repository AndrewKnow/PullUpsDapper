

namespace PullUpsDapper.Users
{
    public class PlanPulls
    {
        public int Week { get; set; }
        public int PullsPlan { get; set; }
        public IList<FactPulls> Facts { get; set; }
    }
}
