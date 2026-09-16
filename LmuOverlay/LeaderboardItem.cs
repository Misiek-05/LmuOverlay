using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LmuOverlay
{
    public class LeaderboardItem
    {
        public int place {  get; set; }
        public string driverName { get; set; }
        public string sector1 { get; set; }
        public string sector2 { get; set; }
        public string sector3 { get; set; }
        public string lastLapTime { get; set; }
        public string gap { get; set; }

        public LeaderboardItem(int place, string driverName, string sector1, string sector2, string sector3, string lastLapTime, string gap)
        {
            this.place = place;
            this.driverName = driverName;
            this.sector1 = sector1;
            this.sector2 = sector2;
            this.sector3 = sector3;
            this.lastLapTime = lastLapTime;
            this.gap = gap;
        }
    }
}
