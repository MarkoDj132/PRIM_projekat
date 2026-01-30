using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        static string nadimak;
        static int UDP_PORT = 5000;
        static int TCP_PORT;
        static Socket tcpSocket;

        static void Main(string[] args)
        {
            Console.WriteLine("KLIJENT POKRENUT");

            Console.Write("Unesite vase ime/nadimak: ");
            nadimak = Console.ReadLine();

            PrijaviSeUDP();
            PoveziSeTCP();
            OdaberiServerIKanal();

            Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
            tcpSocket.Close();
        }

        static void PrijaviSeUDP()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, UDP_PORT);

            byte[] porukaBytes = Encoding.UTF8.GetBytes("PRIJAVA");
            udpClient.Send(porukaBytes, porukaBytes.Length, serverEndPoint);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] odgovor = udpClient.Receive(ref remoteEP);
            string tcpPortString = Encoding.UTF8.GetString(odgovor);
            TCP_PORT = int.Parse(tcpPortString);

            Console.WriteLine($"Primljen TCP port: {TCP_PORT}");
            udpClient.Close();
        }

        static void PoveziSeTCP()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(IPAddress.Loopback, TCP_PORT);
            Console.WriteLine("Povezan na TCP server");

            byte[] nadimakBytes = Encoding.UTF8.GetBytes(nadimak);
            tcpSocket.Send(nadimakBytes);

            byte[] bafer = new byte[1024];
            int primljeno = tcpSocket.Receive(bafer);
            string listaServera = Encoding.UTF8.GetString(bafer, 0, primljeno);
            Console.WriteLine($"Dostupni serveri: {listaServera}");
        }

        static void OdaberiServerIKanal()
        {
            Console.Write("Izaberite server: ");
            string server = Console.ReadLine();

            byte[] serverBytes = Encoding.UTF8.GetBytes(server);
            tcpSocket.Send(serverBytes);

            byte[] bafer = new byte[1024];
            int primljeno = tcpSocket.Receive(bafer);
            string listaKanala = Encoding.UTF8.GetString(bafer, 0, primljeno);
            Console.WriteLine($"Dostupni kanali: {listaKanala}");

            Console.Write("Izaberite kanal: ");
            string kanal = Console.ReadLine();

            byte[] kanalBytes = Encoding.UTF8.GetBytes(kanal);
            tcpSocket.Send(kanalBytes);

            primljeno = tcpSocket.Receive(bafer);
            string potvrda = Encoding.UTF8.GetString(bafer, 0, primljeno);
            Console.WriteLine($"Odgovor servera: {potvrda}");
        }
    }
}
