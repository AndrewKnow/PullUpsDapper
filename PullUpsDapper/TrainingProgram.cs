using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper
{
    public class TrainingProgram
    {
        public static long Id { get; set; }
        public static int Week { get; set; }
        public static int Approach { get; set; }
        public static int Pulls { get; set; }
        public static DateTime Date { get; set; }
      
        public TrainingProgram(long id, int week, int approach, int pulls, DateTime date)
        {
            Id = id;
            Week = week;
            Approach = approach;
            Pulls = pulls;
            Date = date;
        }

        public class CreateProgram // вложенный класс
        {
            public static List<TrainingProgram> CreateLvlProgram(string lvl, long userId)
            {
                DateTime date = DateTime.Now;
                List<TrainingProgram> userProgram = new();
                int pulls;
                for (int i = 1; i < 31; i++) // неделя
                {
                    pulls = 0;
                    for (int j = 0; i < 6; i++) // подход
                    {
                        switch (lvl) // уровень
                        {
                            case "Новичок":
                                userProgram.Add(new TrainingProgram(userId, i, j, j <= 2 &&  j >= 3 ? pulls + 2 : pulls + 1, date));
                                break;
                            case "Профи":
                                userProgram.Add(new TrainingProgram(userId, i, j, j <= 2 && j >= 3 ? pulls + 3 : pulls + 2, date));
                                break;
                            case "Турникмен":
                                userProgram.Add(new TrainingProgram(userId, i, j, j <= 2 && j >= 3 ? pulls + 4 : pulls + 3, date));
                                break;
                        }
                    }
                    date = date.AddDays(1);
                }

                foreach (TrainingProgram program in userProgram)
                {
                    Console.WriteLine($"{string.Join(", ", program)}");
                }
                return userProgram;
            }
        }
    }
}

