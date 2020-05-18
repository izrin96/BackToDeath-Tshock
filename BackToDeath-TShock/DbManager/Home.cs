using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToDeath_TShock.DbManager
{
    public class Home
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public Home(string UUID, string Name, float X, float Y)
        {
            this.UUID = UUID;
            this.Name = Name;
            this.X = X;
            this.Y = Y;
        }
    }
}
