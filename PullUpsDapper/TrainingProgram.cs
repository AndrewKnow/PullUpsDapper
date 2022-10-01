using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper
{
    public class TrainingProgram
    {
        public long Id { get; set; }
        public int Week { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }
        public TrainingProgram(long id, int week, int approach, int pulls)
        {
            Id = id;
            Week = week;
            Approach = approach;
            Pulls = pulls;
        }
    }
    public class LevelProgram
    {
        public string Level { get; set; }
        public int Week { get; set; }
        public int Approach { get; set; }
        public int Pulls { get; set; }
        public LevelProgram(string level, int week, int approach, int pulls)
        {
            Level = level;
            Week = week;
            Approach = approach;
            Pulls = pulls;
        }
    }
    public class DayResult
    {
        public long Id { get; set; }
        public int Week { get; set; }
        public DateTime Date { get; set; }
        public int Pulls  { get; set; }
        public DayResult(long id, int week, DateTime date, int pulls)
        {
            Id = id;
            Week = week;
            Date = date;
            Pulls = pulls;
        }
    }

    public class CreateProgram
    {
        public static List<TrainingProgram> UserProgram = new List<TrainingProgram>();
        public static List<DayResult> DayResult = new List<DayResult>();
        public static List<LevelProgram> LevelProgram = new List<LevelProgram>();
        public static List<DayResult> CreateUserProgram(string lvl, long userId)
        {
            DayResult.Clear();

            DateTime date = DateTime.Now;
            int pulls;
            int pullsMax;
            pulls = 0;
            for (int i = 1; i <= 30; i++) // неделя
            {
                pulls++;
                for (int j = 1; j <= 6; j++) // подход
                {
                    DayResult.Add(new DayResult(userId, i, date, 0));
                    switch (lvl) // уровень
                    {
                        case "Новичок":
                            pullsMax = j > 2 && j <= 4 ? pulls + 2 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax));
                            break;
                        case "Профи":
                            pullsMax = j > 2 && j <= 4 ? pulls + 3 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax));
                            break;
                        case "Турникмен":
                            pullsMax = j > 2 && j <= 4 ? pulls + 4 : pulls + 1;
                            UserProgram.Add(new TrainingProgram(userId, i, j, pullsMax));
                            break;
                    }
                    date = date.AddDays(1);
                }
            }

            //for (int i = 0; i < UserProgram.Count; i++)
            //{
            //    TrainingProgram program = UserProgram[i];
            //    Console.WriteLine($"{program.Id} {program.Week} {program.Approach} {program.Pulls}");
            //}
            for (int i = 0; i < DayResult.Count; i++)
            {
                DayResult program = DayResult[i];
                Console.WriteLine($"{program.Id} {program.Week} {program.Date} {program.Pulls}");
            }
            return DayResult;
        }

        public static List<LevelProgram> CreareProgramLevel() // Функция администратора
        {
            LevelProgram.Clear();

            DateTime date = DateTime.Now;
            int pulls = 0;
            int pullsLvl1 = 0;
            int pullsLvl2 = 0;
            int pullsLvl3 = 0;
            for (int i = 1; i <= 30; i++) // неделя
            {
                pullsLvl1++;
                pullsLvl2++;
                pullsLvl3++;
                for (int j = 1; j <= 6; j++) // подход
                {
                    pulls = pullsLvl1;
                    LevelProgram.Add(new LevelProgram("Новичок", i, j, j > 2 && j <= 4 ? pulls + 1 : pullsLvl1));

                    pulls = pullsLvl2;
                    LevelProgram.Add(new LevelProgram("Профи", i, j, j > 2 && j <= 4 ? pulls + 2 : pullsLvl2 + 1));

                    pulls = pullsLvl3;
                    LevelProgram.Add(new LevelProgram("Турникмен", i, j, j > 2 && j <= 4 ? pulls + 3 : pullsLvl3 + 2));
                    
                    date = date.AddDays(1);
                }
            }
            return LevelProgram;
        }
    }
}
