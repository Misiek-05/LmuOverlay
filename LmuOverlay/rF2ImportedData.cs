using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmuOverlay
{
    public class rF2ImportedData
    {
        public MappedBuffer<rF2Telemetry> _telemetryBuffer {  get; set; }
        public MappedBuffer<rF2Scoring> _scoringBuffer { get; set; }
        public rF2Telemetry _telemetry = new rF2Telemetry();
        public rF2Scoring _scoring = new rF2Scoring();
        public rF2ScoringInfo _scoringInfo {  get; set; }

        // player data
        public int playerID {  get; set; }
        public rF2VehicleScoring playerVehicleScoring { get; set; }
        public rF2VehicleTelemetry playerVehicleTelemetry { get; set; }
        public byte[] mVehicleClass { get; set; }

        public rF2ImportedData() {
            playerID = 0;
        }

        public void GetPlayerID()
        {
            _scoringBuffer.GetMappedData(ref _scoring);
            _scoringInfo = _scoring.mScoringInfo;

            foreach (rF2VehicleScoring vehicleScoring in _scoring.mVehicles)
            {
                if (vehicleScoring.mIsPlayer == 1)
                {
                    playerID = vehicleScoring.mID;
                    playerVehicleScoring = vehicleScoring;
                    mVehicleClass = vehicleScoring.mVehicleClass;
                    break;
                }
            }
        }
    }
}
