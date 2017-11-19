using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memory;

namespace StringCleaner
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {

            Console.Title = "";

            Mem MemAccess = new Mem();
            Random random = new Random();

            Console.WriteLine("Welcome to the string cleaner!");
            Console.WriteLine("Press enter key to continue...");

            Console.ReadLine();
            Console.Clear();

            begin:
            Console.WriteLine("Enter in the process name (.exe is obsolete).");
            string name = Console.ReadLine();

            Console.Clear();

            int pid = MemAccess.getProcIDFromName(name);

            MemAccess.OpenGameProcess(pid);

            Console.WriteLine("Enter the address of the string!");
            
            string address = Console.ReadLine();
            Console.Clear();

            if (address.Length > 16)
            {
                Console.WriteLine("Address unaccessable!");
                Console.ReadLine();
            } else
            {
                Console.WriteLine("Nulling string...");

                System.Threading.Thread.Sleep(random.Next(1000, 5000));
                Console.Clear();

                char nullchar = (char)0;
                string nullstring = nullchar.ToString();
                string type = "string";

                try
                {
                    MemAccess.WriteProcessMemory(address, type, nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring + nullstring);
                } catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error occured during process: " + ex);
                    Console.ReadKey();
                }

                Console.WriteLine("Checking.");
                System.Threading.Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Checking..");
                System.Threading.Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Checking...");

                string readstring = MemAccess.readString(address);
                if (readstring.Contains(nullstring))
                {
                    System.Threading.Thread.Sleep(random.Next(1500, 5000));

                    Console.WriteLine("Completed without exceptions.");
                    Console.ReadKey();
                } else
                {
                    Console.WriteLine("Could not null string completely :v?");
                    Console.ReadKey();
                }



                Console.Clear();
                goto begin;
            }
        }

    }
}
