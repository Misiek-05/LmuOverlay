using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LmuOverlay
{
    public class TelemetryData
    {
        public byte[] mDriverName {  get; set; }
        public double fuel { get; set; }
        public string lapTime { get; set; }
        public string sector1 { get; set; }
        public string sector2 { get; set; }
        public string sector3 { get; set; }
        public double lfWheelWear {  get; set; }
        public double rfWheelWear {  get; set; }
        public double lrWheelWear {  get; set; }
        public double rrWheelWear {  get; set; }
        public double lfWheelTmp {  get; set; }
        public double rfWheelTmp {  get; set; }
        public double lrWheelTmp {  get; set; }
        public double rrWheelTmp {  get; set; }
        private string fileName { get; set; }

        public TelemetryData(byte[] mTrackName)
        {
            string trackName = Encoding.ASCII.GetString(mTrackName).TrimEnd('\0');
            this.fileName = $"{trackName}.csv";

            if (!File.Exists(this.fileName)) {
                string header = "mDriverName,fuel,lapTime,sector1,sector2,sector3," +
                                "lfWheelWear,rfWheelWear,lrWheelWear,rrWheelWear," +
                                "lfWheelTmp,rfWheelTmp,lrWheelTmp,rrWheelTmp\n";
                File.WriteAllText(this.fileName, header);
            }

        }

        public void updateData(rF2VehicleTelemetry vehicle, rF2VehicleScoring vehicleScoring)
        {
            this.mDriverName = vehicleScoring.mDriverName;
            this.fuel = Math.Round(vehicle.mFuel, 1);
            this.lapTime = convertTimeToString(vehicleScoring.mLastLapTime);
            this.sector1 = convertTimeToString(vehicleScoring.mLastSector1);
            this.sector2 = convertTimeToString(vehicleScoring.mLastSector2 - vehicleScoring.mLastSector1);
            this.sector3 = convertTimeToString(vehicleScoring.mLastLapTime - vehicleScoring.mLastSector2);
            this.lfWheelWear = Math.Round(vehicle.mWheels[0].mWear * 100, 1);
            this.rfWheelWear = Math.Round(vehicle.mWheels[1].mWear * 100, 1);
            this.lrWheelWear = Math.Round(vehicle.mWheels[2].mWear * 100, 1);
            this.rrWheelWear = Math.Round(vehicle.mWheels[3].mWear * 100, 1);
            this.lfWheelTmp = Math.Round(vehicle.mWheels[0].mTireCarcassTemperature - 273.15, 0);
            this.rfWheelTmp = Math.Round(vehicle.mWheels[1].mTireCarcassTemperature - 273.15, 0);
            this.lrWheelTmp = Math.Round(vehicle.mWheels[2].mTireCarcassTemperature - 273.15, 0);
            this.rrWheelTmp = Math.Round(vehicle.mWheels[3].mTireCarcassTemperature - 273.15, 0);
        }

        private string convertTimeToString(double time)
        {
            int minutes = (int)(time / 60);
            double seconds = time % 60;
            return $"{minutes}:{seconds:00.000}";
        }

        public void exportDataToCSV()
        {
            string driver = Encoding.ASCII.GetString(this.mDriverName).TrimEnd('\0');
            string rowData = $"{driver},{this.fuel},{this.lapTime},{this.sector1},{this.sector2},{this.sector3}," +
                             $"{this.lfWheelWear},{this.rfWheelWear},{this.lrWheelWear},{this.rrWheelWear}," +
                             $"{this.lfWheelTmp},{this.rfWheelTmp},{this.lrWheelTmp},{this.rrWheelTmp}\n";
            File.AppendAllText(this.fileName, rowData);
        }
    }
}
