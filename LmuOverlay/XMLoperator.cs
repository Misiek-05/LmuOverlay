using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace LmuOverlay
{
    public class XMLoperator
    {
        public ObservableCollection<Lap> laps { get; set; }
        public bool imported { get; set; }

        public XMLoperator() 
        {
            imported = false;
            laps = new ObservableCollection<Lap>();

        }

        public void Add(Lap lap)
        {
            if (LapValidToAdd(lap))
                laps.Add(lap);
        }

        public bool LapValidToAdd(Lap lap)
        {
            return !(lap.sector1.Contains("-")
                || lap.sector2.Contains("-")
                || lap.sector3.Contains("-")
                || lap.lapTime.Contains("-")
                || lap.lapTime == "0:00,000");
        }

        public void ExportToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Lap>));
            using (StreamWriter writer = new StreamWriter($"LapTimes.xml"))
            {
                serializer.Serialize(writer, laps);
            }
        }

        public void ImportFromXML()
        {
            if (!File.Exists("LapTimes.xml")) return;

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Lap>));
            using (StreamReader reader = new StreamReader("LapTimes.xml"))
            {
                var lapsImported = (ObservableCollection<Lap>)serializer.Deserialize(reader);

                foreach (var lap in lapsImported)
                    laps.Add(lap);
            }
            imported = true;
        }
    }
}
