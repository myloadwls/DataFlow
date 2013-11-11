using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlow
{
    class Program
    {
        static void Main(string[] args)
        {

            char[] tokens = "This is a ,big array of chars.".ToArray();

            var text = new string(tokens.Select(x => char.IsLetter(x) ? x : ' ').ToArray());


            //for (int i = 0; i < tokens.Length; i++)
            //{
            //    if (!char.IsLetter(tokens[i]))
            //        tokens[i] = ' ';
            //}
            //var text = new string(tokens);

        }
    }
}
