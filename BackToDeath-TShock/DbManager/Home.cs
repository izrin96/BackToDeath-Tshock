using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToDeath_TShock.DbManager
{
    public class Home
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public Home(int UserID, string Name, float X, float Y)
        {
            this.UserID = UserID;
            this.Name = Name;
            this.X = X;
            this.Y = Y;
        }
    }
}
