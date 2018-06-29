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

        private List<Vector2> lastLocation { get; set; }

        public bool canBack = false;

        public PlayerInfo()
        {
            lastLocation = new List<Vector2>();
        }

        public Vector2 popLastLocation()
        {
            Vector2 vector = lastLocation[0];

            if (lastLocation.Count > 0)
            {
                vector = lastLocation[lastLocation.Count - 1];
                lastLocation.RemoveAt(lastLocation.Count - 1);
            }
            else
                canBack = false;

            return vector;
        }

        public void pushLastLocation(Vector2 vector)
        {
            lastLocation.Add(vector);
            canBack = true;

            if (lastLocation.Count == 5)
                lastLocation.RemoveAt(0);
        }

        public int getLastLocationCount()
        {
            return lastLocation.Count;
        }
    }
}
