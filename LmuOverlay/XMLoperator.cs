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
        private ObservableCollection<Lap> laps;

        public XMLoperator() 
        {
            laps = new ObservableCollection<Lap>();
        }

        public void Add(Lap lap)
        {
            laps.Add(lap);
        }

        public void ExportToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Lap>));
            using (StreamWriter writer = new StreamWriter($"LapTimes.xml"))
            {
                serializer.Serialize(writer, laps);
            }
        }
    }
}
