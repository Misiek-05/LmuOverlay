using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmuOverlay
{
    public class Player
    {
        public int playerID { get; set; }
        public int place {  get; set; } 
        public rF2VehicleScoring playerVehicleScoring { get; set; }
        public rF2VehicleTelemetry playerVehicleTelemetry { get; set; }
        public byte[] mVehicleClass { get; set; }

        public Player()
        {

        }
    }
}
