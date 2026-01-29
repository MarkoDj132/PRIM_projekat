using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Poruka
    {
        public string Posiljalac;
        public string VremenskiTrenutak;
        public string Sadrzaj;

        public Poruka(string posiljalac, string vremenskiTrenutak, string sadrzaj)
        {
            Posiljalac = posiljalac;
            VremenskiTrenutak = vremenskiTrenutak;
            Sadrzaj = sadrzaj;
        }
    }
}
