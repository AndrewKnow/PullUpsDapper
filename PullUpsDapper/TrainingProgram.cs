using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper
{
    public class TrainingProgram
    {
        public long Id { get ; set; }
        public int Week { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }
        public DateTime Date { get; set; }
      
        public TrainingProgram(long id, int week, int approach, int pulls, DateTime date)
        {
            Id = id;
            Week = week;
            Approach = approach;
            Pulls = pulls;
            Date = date;
        }
    }
    public class CreateProgram
    {
        private static List<TrainingProgram> UserProgram = new List<TrainingProgram>();

        public static List<TrainingProgram> CreateLvlProgram(string lvl, long userId)
        {
            DateTime date = DateTime.Now;
            int pulls;
            int pullsMax;
            pulls = 0;
            for (int i = 1; i <= 30; i++) // неделя
            {
                pulls++;
                for (int j = 1; j <= 6; j++) // подход
                {
                    switch (lvl) // уровень
                    {
                        case "Новичок":
                            pullsMax = j > 2 && j <= 4 ? pulls + 2 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax, date));
                            break;
                        case "Профи":
                            pullsMax = j > 2 && j <= 4 ? pulls + 3 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax, date));
                            break;
                        case "Турникмен":
                            pullsMax = j > 2 && j <= 4 ? pulls + 4 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax, date));
                            break;
                    }
                }
                date = date.AddDays(1);
            }

            for (int i = 0; i < UserProgram.Count; i++)
            {
                TrainingProgram program = UserProgram[i];
                Console.WriteLine($"{program.Id} {program.Week} {program.Approach} {program.Pulls} {program.Date}");
            }
            return UserProgram;
        }
    }
}
