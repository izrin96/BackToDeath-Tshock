using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BackToDeath_TShock
{
    public class PlayerInfo
    {
        public const string KEY = "BackToDeath_Data";

        public Vector2 lastLocation { get; set; }

        public bool isDeathYet = false;
    }
}
