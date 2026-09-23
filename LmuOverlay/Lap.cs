using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmuOverlay
{
    public class Lap
    {
        public string track { get; set; }
        public Session session { get; set; }
        public string sector1 { get; set; }
        public string sector2 { get; set; }
        public string sector3 { get; set; }
        public string lapTime { get; set; }

        protected Lap()
        {

        }

        public Lap(string track, Session session, string sector1, string sector2, string sector3, string lapTime)
        {
            this.track = track;
            this.session = session;
            this.sector1 = sector1;
            this.sector2 = sector2;
            this.sector3 = sector3;
            this.lapTime = lapTime;
        }

        public override string ToString()
        {
            return $"{track}, {session}, {sector1}, {sector2}, {sector3}, {lapTime}";
        }
    }
}
