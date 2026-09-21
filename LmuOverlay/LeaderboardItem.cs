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
        public byte isPlayer {  get; set; }
        public int mID { get; set; }
        public int place {  get; set; }
        public string driverName { get; set; }
        public string sector1 { get; set; }
        public string sector2 { get; set; }
        public string sector3 { get; set; }
        public string lastLapTime { get; set; }
        public string bestLapTime { get; set; }
        public double bestLapTimeDouble { get; set; }
        public string gap { get; set; }
        public double gapDouble { get; set; }
        public string status { get; set; }
        public double updated { get; set; }

        public LeaderboardItem(byte isPlayer, int mID, int place, string driverName, string sector1 = "-", string sector2 = "-", string sector3 = "-", string lastLapTime = "-", string bestLapTime = "-", double bestLapTimeDouble = 0.0, string gap = "-", double gapDouble = 0, string status = " ")
        {
            this.isPlayer = isPlayer;
            this.mID = mID;
            this.place = place;
            this.driverName = driverName;
            this.sector1 = sector1;
            this.sector2 = sector2;
            this.sector3 = sector3;
            this.lastLapTime = lastLapTime;
            this.bestLapTime = bestLapTime;
            this.bestLapTimeDouble = bestLapTimeDouble;
            this.gap = gap;
            this.gapDouble = gapDouble;
        }

        public void Update(int place, string sector1, string sector2, string sector3, string lastLapTime, string bestLapTime, double bestLapTimeDouble, string gap, double gapDouble, string status)
        {
            this.place = place;
            this.sector1 = sector1;
            this.sector2 = sector2;
            this.sector3 = sector3;
            this.lastLapTime = lastLapTime;
            this.bestLapTime = bestLapTime;
            this.bestLapTimeDouble = bestLapTimeDouble;
            this.gap = gap;
            this.gapDouble= gapDouble;
            this.status = status;
        }
    }
}
