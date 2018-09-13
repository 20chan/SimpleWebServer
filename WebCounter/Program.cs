using System;

namespace WebCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Server(typeof(Program));
            Console.WriteLine("Run until type enter..");
            s.Run();
            Console.Read();
            s.Stop();
        }

        static int ID;

        [Mapping("ctr")]
        public static string CounterCallback()
        {
            return $"<body><h3>{ID++}</h3></body>";
        }
    }
}
