using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

#nullable disable

namespace Client
{
    class Program
    {
        static string nadimak;
        static int UDP_PORT = 5000;
        static int TCP_PORT;
        static Socket tcpSocket;
        static string izabraniKanal;
        static string trenutakPrestanka;

        static void Main(string[] args)
        {
            PrijaviSeUDP();
            PoveziSeTCP();
            OdaberiServerIKanal();

            while (true)
            {
                Console.Write("Unesi poruku (prazno za izlaz): ");
                string poruka = Console.ReadLine();
                if (poruka == "")
                    break;
                PosaljiPoruku(poruka);
            }

            //Cuvamo vremenski trenutak
            SacuvajVremenskiTrenutak();
            tcpSocket.Close();
        }

        static void PrijaviSeUDP()
        {
            Console.Write("Unesi nadimak: ");
            nadimak = Console.ReadLine();

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), UDP_PORT);

            byte[] sendData = Encoding.UTF8.GetBytes("PRIJAVA");
            udpSocket.SendTo(sendData, serverEndPoint);

            byte[] recvData = new byte[256];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpSocket.ReceiveFrom(recvData, ref remoteEndPoint);

            string odgovor = Encoding.UTF8.GetString(recvData).Trim('\0');
            TCP_PORT = int.Parse(odgovor);

            udpSocket.Close();
        }

        static void PoveziSeTCP()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TCP_PORT);
            tcpSocket.Connect(serverEndPoint);

            byte[] sendData = Encoding.UTF8.GetBytes(nadimak);
            tcpSocket.Send(sendData);

            byte[] recvData = new byte[1024];
            int bytesRead = tcpSocket.Receive(recvData);
            string listaServera = Encoding.UTF8.GetString(recvData, 0, bytesRead);
            Console.WriteLine("Dostupni serveri: " + listaServera);
        }

        static void OdaberiServerIKanal()
        {
            Console.Write("Izaberi server: ");
            string server = Console.ReadLine();
            byte[] sendData = Encoding.UTF8.GetBytes(server);
            tcpSocket.Send(sendData);

            byte[] recvData = new byte[1024];
            int bytesRead = tcpSocket.Receive(recvData);
            string listaKanala = Encoding.UTF8.GetString(recvData, 0, bytesRead);
            Console.WriteLine("Dostupni kanali: " + listaKanala);

            Console.Write("Izaberi kanal: ");
            izabraniKanal = Console.ReadLine();
            sendData = Encoding.UTF8.GetBytes(izabraniKanal);
            tcpSocket.Send(sendData);

            recvData = new byte[4096];
            bytesRead = tcpSocket.Receive(recvData);
            string potvrda = Encoding.UTF8.GetString(recvData, 0, bytesRead);

            if (string.IsNullOrEmpty(potvrda) || potvrda.Trim() == "")
            {
                Console.WriteLine("\n--- Kanal je prazan ---");
            }
            else
            {
                Console.WriteLine("\n--- Poruke u kanalu ---");
                Console.WriteLine(potvrda);
            }

            Console.WriteLine("\nMozete poceti slanje poruka (prazno za izlaz):\n");
            SacuvajServerUDatoteku(server, nadimak);
        }

        static void PosaljiPoruku(string poruka)
        {
            string sifrovano = Sifruj(poruka, izabraniKanal);
            byte[] sendData = Encoding.UTF8.GetBytes(sifrovano);
            tcpSocket.Send(sendData);

            byte[] recvData = new byte[256];
            int bytesRead = tcpSocket.Receive(recvData);
            string potvrda = Encoding.UTF8.GetString(recvData, 0, bytesRead);
            Console.WriteLine(potvrda);
        }

        static void SacuvajServerUDatoteku(string nazivServera, string nadimak)
        {
            string putanja = "serveri_lista.txt";
            Dictionary<string, string> serveri = new Dictionary<string, string>();

            if (File.Exists(putanja))
            {
                string[] linije = File.ReadAllLines(putanja);
                foreach (string linija in linije)
                {
                    string[] delovi = linija.Split('|');
                    if (delovi.Length == 2)
                    {
                        serveri[delovi[0]] = delovi[1];
                    }
                }
            }

            serveri[nazivServera] = nadimak;

            List<string> noviSadrzaj = new List<string>();
            foreach (var par in serveri)
            {
                noviSadrzaj.Add($"{par.Key}|{par.Value}");
            }

            File.WriteAllLines(putanja, noviSadrzaj);
        }

        static string Sifruj(string tekst, string kljuc)
        {
            string prosireniKljuc = "";
            for (int i = 0; i < tekst.Length; i++)
            {
                prosireniKljuc = prosireniKljuc + kljuc[i % kljuc.Length];
            }

            string rezultat = "";
            for (int i = 0; i < tekst.Length; i++)
            {
                char c = tekst[i];
                if (char.IsLetter(c))
                {
                    char baza;
                    if (char.IsUpper(c))
                        baza = 'A';
                    else
                        baza = 'a';

                    char kljucChar;
                    if (char.IsUpper(prosireniKljuc[i]))
                        kljucChar = prosireniKljuc[i];
                    else
                        kljucChar = char.ToUpper(prosireniKljuc[i]);

                    int pomak = kljucChar - 'A';
                    int novi = (c - baza + pomak) % 26;
                    rezultat = rezultat + (char)(baza + novi);
                }
                else
                {
                    rezultat = rezultat + c;
                }
            }
            return rezultat;
        }

        static void SacuvajVremenskiTrenutak()
{
    string putanja = "serveri_lista.txt";
    string vreme = "\nVREME:" + DateTime.Now.ToString();
    if (File.Exists(putanja))
    {
        File.AppendAllText(putanja, vreme);
    }
}
    }
}
