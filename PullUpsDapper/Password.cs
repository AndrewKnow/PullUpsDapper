using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullUpsDapper
{
    internal class Password
    {
        public static string Key { get; set; }
        public static string Bot()
        {
            StreamReader f = new("Key.txt");
            while (!f.EndOfStream)
            {
                Key = f.ReadLine();
                break;
            }
            f.Close();
            return Key;
        }
        public static string DB()
        {
            StreamReader f = new("Key.txt");
            while (!f.EndOfStream)
            {
                Key = f.ReadLine();
            }
            f.Close();
            return Key;
        }
    }
}
