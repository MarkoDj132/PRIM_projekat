using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Program
    {
        static Dictionary<string, List<Kanal>> serveri = new Dictionary<string, List<Kanal>>();
        static int UDP_PORT = 5000;
        static int TCP_PORT = 5001;
        static Dictionary<Socket, KlijentInfo> klijenti = new Dictionary<Socket, KlijentInfo>();

        class KlijentInfo
        {
            public string Nadimak;
            public string IzabraniServer;
            public string IzabraniKanal;

            public KlijentInfo()
            {
                Nadimak = "";
                IzabraniServer = "";
                IzabraniKanal = "";
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("SERVER POKRENUT");

            KreirajServere();

            Console.WriteLine("UDP Port: " + UDP_PORT);
            Console.WriteLine("TCP Port: " + TCP_PORT);

            Thread udpThread = new Thread(UDPSlusalac);
            udpThread.Start();

            Thread tcpThread = new Thread(TCPPolling);
            tcpThread.Start();

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

        static void UDPSlusalac()
        {
            UdpClient udpServer = new UdpClient(UDP_PORT);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] primljenoPodataka = udpServer.Receive(ref remoteEP);
                string poruka = Encoding.UTF8.GetString(primljenoPodataka);

                if (poruka == "PRIJAVA")
                {
                    string odgovor = TCP_PORT.ToString();
                    byte[] odgovorBajtovi = Encoding.UTF8.GetBytes(odgovor);
                    udpServer.Send(odgovorBajtovi, odgovorBajtovi.Length, remoteEP);
                }
            }
        }

        static void TCPPolling()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, TCP_PORT));
            serverSocket.Listen(10);
            serverSocket.Blocking = false;

            List<Socket> sviSoketi = new List<Socket>();
            sviSoketi.Add(serverSocket);

            while (true)
            {
                List<Socket> proveriCitanje = new List<Socket>(sviSoketi);

                Socket.Select(proveriCitanje, null, null, 100000);

                foreach (Socket soket in proveriCitanje)
                {
                    if (soket == serverSocket)
                    {
                        Socket klijentSoket = serverSocket.Accept();
                        klijentSoket.Blocking = false;
                        sviSoketi.Add(klijentSoket);
                        klijenti.Add(klijentSoket, new KlijentInfo());
                    }
                    else
                    {
                        try
                        {
                            byte[] bafer = new byte[1024];
                            int primljeno = soket.Receive(bafer);

                            if (primljeno == 0)
                            {
                                sviSoketi.Remove(soket);
                                klijenti.Remove(soket);
                                soket.Close();
                            }
                            else
                            {
                                ObradiPoruku(soket, bafer, primljeno);
                            }
                        }
                        catch
                        {
                            sviSoketi.Remove(soket);
                            klijenti.Remove(soket);
                            soket.Close();
                        }
                    }
                }
            }
        }

        static void ObradiPoruku(Socket klijentSoket, byte[] bafer, int duzina)
        {
            string poruka = Encoding.UTF8.GetString(bafer, 0, duzina);
            KlijentInfo info = klijenti[klijentSoket];

            if (info.Nadimak == "")
            {
                info.Nadimak = poruka;
                string listaServera = NapraviListuServera();
                byte[] odgovor = Encoding.UTF8.GetBytes(listaServera);
                klijentSoket.Send(odgovor);
            }
            else if (info.IzabraniServer == "")
            {
                info.IzabraniServer = poruka;
                if (serveri.ContainsKey(poruka))
                {
                    string listaKanala = NapraviListuKanala(poruka);
                    byte[] odgovor = Encoding.UTF8.GetBytes(listaKanala);
                    klijentSoket.Send(odgovor);
                }
            }
            else if (info.IzabraniKanal == "")
            {
                info.IzabraniKanal = poruka;
                //klijent izabrao kanal, saljemo mu poruke u njemu.
                string Odgovor = posaljiPoruke(info);

                byte[] odgovor = Encoding.UTF8.GetBytes(Odgovor);
                klijentSoket.Send(odgovor);
            }
            else
            {
                Console.WriteLine($"Primljena poruka od {info.Nadimak} na {info.IzabraniServer}:{info.IzabraniKanal} - {poruka}");
                //Desifrovanje poruke
                string desfPoruka = DesifrujPoruku(poruka, info.IzabraniKanal);
                Console.WriteLine($"Desifrovana poruka: {desfPoruka}");
                Poruka p = new Poruka(info.Nadimak, DateTime.Now.ToString(), desfPoruka);
                serveri[info.IzabraniServer].Find(k => k.Naziv == info.IzabraniKanal)!.Poruke.Add(p);

                Console.WriteLine($"[{p.VremenskiTrenutak}]-[{info.IzabraniServer}]:[{info.IzabraniKanal}]:[{desfPoruka}]-[{info.Nadimak}]");

                byte[] odgovor = Encoding.UTF8.GetBytes("PRIMLJENO");
                klijentSoket.Send(odgovor);
            }
        }

        static string NapraviListuServera()
        {
            string rezultat = "";
            int brojac = 0;
            foreach (string ime in serveri.Keys)
            {
                if (brojac > 0)
                {
                    rezultat += ",";
                }
                rezultat += ime;
                brojac++;
            }
            return rezultat;
        }

        static string NapraviListuKanala(string nazivServera)
        {
            string rezultat = "";
            List<Kanal> kanali = serveri[nazivServera];

            //Sortiramo kanale

            /*for (int i = 0; i < kanali.Count; i++)
            {
                if (i > 0)
                {
                    rezultat += ",";
                }
                rezultat += kanali[i].Naziv;
            }*/

            string putanja = "serveri_lista.txt";
            string vreme = "";
            if (File.Exists(putanja))
            {
                string[] linije = File.ReadAllLines(putanja);
                if (linije.Length > 0)
                {
                    string poslednjaLinija = linije[linije.Length - 1];
                    if (poslednjaLinija.StartsWith("VREME:"))
                    {
                        vreme = poslednjaLinija.Replace("VREME:", "");
                    }
                }
            }

            List<Kanal> kanaliSaNeprocitanim = new List<Kanal>();
            List<Kanal> kanaliBezNeprocitanih = new List<Kanal>();
            DateTime Vreme;

            if (vreme == "")
            {
                Vreme = DateTime.MinValue;
            }
            else
            {
                Vreme = DateTime.Parse(vreme);
            }

            for (int i = 0; i < kanali.Count; i++)
            {
                if (kanali[i].Poruke.Count == 0)
                {
                    kanaliBezNeprocitanih.Add(kanali[i]);
                }
                else
                {
                    Poruka zadnjaPoruka = kanali[i].Poruke[kanali[i].Poruke.Count - 1];
                    DateTime t = DateTime.Parse(zadnjaPoruka.VremenskiTrenutak);
                    if (t > Vreme)
                    {
                        kanaliSaNeprocitanim.Add(kanali[i]);
                    }
                    else
                    {
                        kanaliBezNeprocitanih.Add(kanali[i]);
                    }
                }
            }

            for (int i = 0; i < kanaliSaNeprocitanim.Count; i++)
            {
                if (rezultat.Length > 0)
                {
                    rezultat += ",";
                }
                rezultat += kanaliSaNeprocitanim[i].Naziv;
            }
            for (int i = 0; i < kanaliBezNeprocitanih.Count; i++)
            {
                if (rezultat.Length > 0)
                {
                    rezultat += ",";
                }
                rezultat += kanaliBezNeprocitanih[i].Naziv;
            }

            return rezultat;
        }

        static string DesifrujPoruku(string poruka, string kljuc)
        {
            string rezultat = "";
            for (int i = 0; i < poruka.Length; i++)
            {
                char s = poruka[i];
                char k = kljuc[i % kljuc.Length];

                if (char.IsLetter(s))
                {
                    char baza = char.IsUpper(s) ? 'A' : 'a';
                    int sInt = s - baza;
                    int kInt = char.ToUpper(k) - 'A';
                    int desifrovan = (sInt - kInt + 26) % 26;
                    rezultat += (char)(desifrovan + baza);
                }
                else
                {
                    rezultat += s; // Razmak, interpunkcija ostaju isti
                }
            }
            return rezultat;
        }

        static string posaljiPoruke(KlijentInfo info)
        {
            string porukeUKanalu = "";
            foreach (KeyValuePair<string, List<Kanal>> s in serveri)
            {
                if(info.IzabraniServer == s.Key )
                {
                    foreach(Kanal k in s.Value)
                    {
                        if(k.Naziv == info.IzabraniKanal)
                        {
                            foreach(Poruka p in k.Poruke)
                            {
                                porukeUKanalu += $"[{p.VremenskiTrenutak}]-[{p.Posiljalac}]: {p.Sadrzaj}\n";
                            }
                            break;
                        }
                        
                    }
                }
            }

            return porukeUKanalu;
        }
    }
}
