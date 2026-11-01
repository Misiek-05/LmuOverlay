using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmuOverlay
{
    public enum Session
    {
        Practice = 0,
        Qualify = 1,
        Warmup = 2,
        Race = 3,
        All = 4
    };

    public class rF2ImportedData
    {
        public MappedBuffer<rF2Telemetry> _telemetryBuffer {  get; set; }
        public MappedBuffer<rF2Scoring> _scoringBuffer { get; set; }
        public rF2Telemetry _telemetry = new rF2Telemetry();
        public rF2Scoring _scoring = new rF2Scoring();
        public rF2ScoringInfo _scoringInfo {  get; set; }
        public Session currentSession { get; set; }

        // player data
        public Player player;

        public rF2ImportedData() {
            player = new Player();
            player.playerID = 0;
        }

        public void GetPlayerID()
        {
            _scoringBuffer.GetMappedData(ref _scoring);
            _scoringInfo = _scoring.mScoringInfo;

            foreach (rF2VehicleScoring vehicleScoring in _scoring.mVehicles)
            {
                if (vehicleScoring.mIsPlayer == 1)
                {
                    player.playerID = vehicleScoring.mID;
                    player.playerVehicleScoring = vehicleScoring;
                    player.mVehicleClass = vehicleScoring.mVehicleClass;
                    currentSession = GetSession();
                    break;
                }
            }
        }

        public Session GetSession()
        {
            int session = _scoringInfo.mSession;
            if (session <= 4)
                return Session.Practice;
            if (session <= 8)
                return Session.Qualify;
            if (session == 9)
                return Session.Warmup;
            return Session.Race;    
        }

        public string GetTrackName()
        {
            _telemetryBuffer.GetMappedData(ref _telemetry);
            return $"{Encoding.ASCII.GetString(_telemetry.mVehicles[0].mTrackName).Trim('\0')}";
        }

        public void GetPlayerTelemetry()
        {
            _telemetryBuffer.GetMappedData(ref _telemetry);
            
            for (int i = 0; i < _telemetry.mNumVehicles; i++)
            {
                rF2VehicleTelemetry tmp = _telemetry.mVehicles[i];
                if (tmp.mID == player.playerID)
                {
                    player.playerVehicleTelemetry = tmp;
                    return;
                }
            }
        }
    }
}
