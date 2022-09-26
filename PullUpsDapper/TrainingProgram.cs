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
    public class CreateProgram // вложенный класс?
    {

        private static List<TrainingProgram> UserProgram = new List<TrainingProgram>();

        //public static IList<TrainingProgram> UserProgramList
        //{
        //    get => UserProgram;
        //    set => UserProgram.Add((TrainingProgram)value);
        //}

        public static List<TrainingProgram> CreateLvlProgram(string lvl, long userId)
        //public static void CreateLvlProgram(string lvl, long userId)
        {
            DateTime date = DateTime.Now;
            //List<TrainingProgram> userProgram = new();
            int pulls;
            pulls = 0;
            for (int i = 1; i <= 30; i++) // неделя
            {
                pulls++;
                for (int j = 1; j <= 6; j++) // подход
                {
                    switch (lvl) // уровень
                    {
                        case "Новичок":
                            UserProgram.Add(new TrainingProgram(userId, i, j, j <= 2 && j >= 3 ? pulls + 2 : pulls + 1, date));
                            break;
                        case "Профи":
                            UserProgram.Add(new TrainingProgram(userId, i, j, j <= 2 && j >= 3 ? pulls + 3 : pulls + 1, date));
                            break;
                        case "Турникмен":
                            UserProgram.Add(new TrainingProgram(userId, i, j, j <= 2 && j >= 3 ? pulls + 4 : pulls + 1, date));

                            //var a = UserProgramList[0];

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

/*

 public abstract class Sportsman : Interface
{
   ...
   private List<Pro> pro_list = new List<Pro>();
   ...
 
   public IList<Pro> Test
    { 
        get => pro_list;
        set => pro_list.Add((Pro)value); 
    }
 
   public void ProManAdd(object Object)
    {
        Pro s = Object as Pro;
        if (s != null)
        {
            Test.Add(s);
        }
    }
}
и добавление объекта
C#Выделить код
1
2
3
4
5
 private void button4_Click(object sender, EventArgs e)
        {
            Pro pro = new Pro("asd", "vsdwe", "qwhbe", 0, "hjqbwe", "kqwbe", "qjkwben");
            pro.ProManAdd(pro);
        }


*/