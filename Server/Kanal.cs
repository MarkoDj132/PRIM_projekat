using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Kanal
    {
        public string Naziv;
        public List<Poruka> Poruke;

        public Kanal(string naziv)
        {
            Naziv = naziv;
            Poruke = new List<Poruka>();
        }
    }
}
