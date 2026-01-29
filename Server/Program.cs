namespace Server
{
    internal class Program
    {
        static Dictionary<string, List<Kanal>> serveri = new Dictionary<string, List<Kanal>>();
        static int UDP_PORT = 5000;
        static int TCP_PORT = 5001;

        static void Main(string[] args)
        {
            Console.WriteLine("SERVER POKRENUT");

            KreirajServere();

            Console.WriteLine("UDP Port: " + UDP_PORT);
            Console.WriteLine("TCP Port: " + TCP_PORT);
            Console.ReadKey();
        }

        static void KreirajServere()
        {
            List<Kanal> kanali1 = new List<Kanal>();
            kanali1.Add(new Kanal("opsti"));
            kanali1.Add(new Kanal("random"));
            serveri.Add("TestServer", kanali1);

            List<Kanal> kanali2 = new List<Kanal>();
            kanali2.Add(new Kanal("mreze"));
            kanali2.Add(new Kanal("ers"));
            serveri.Add("Fakultet", kanali2);

            Console.WriteLine("Kreirano servera: " + serveri.Count);
        }
    }
}
