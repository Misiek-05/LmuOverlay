using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LmuOverlay
{
    /// <summary>
    /// Interaction logic for PlayerBestLapsWindow.xaml
    /// </summary>
    public partial class PlayerBestLapsWindow : Window
    {
        private DispatcherTimer _timer;
        private XMLoperator _XMLoperator;
        private rF2ImportedData _importedData;
        private int currentLap;

        private rF2VehicleScoring playerVehicleScoring;
        private rF2VehicleTelemetry playerVehicleTelemetry;

        private ObservableCollection<Lap> playerScoreboard;
        public ObservableCollection<Lap> PlayerScoreboard
        {
            get { return playerScoreboard; }
            set { playerScoreboard = value; }
        }

        public PlayerBestLapsWindow(rF2ImportedData _importedData, XMLoperator _XMLoperator)
        {
            DataContext = this;
            playerScoreboard = new ObservableCollection<Lap>();

            InitializeComponent();

            this._importedData = _importedData;
            this._XMLoperator = _XMLoperator;

            RefreshPlayerScoreboard();      

            _importedData.GetPlayerID();
            _importedData.GetPlayerTelemetry();

            currentLap = _importedData.player.playerVehicleScoring.mTotalLaps;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += UpdateBestLaps;
            _timer.Start();
        }


        private void UpdateBestLaps(object? sender, EventArgs e)
        {
            try
            {
                _importedData.GetPlayerID();
                _importedData.GetPlayerTelemetry();

                playerVehicleScoring = _importedData.player.playerVehicleScoring;
                playerVehicleTelemetry = _importedData.player.playerVehicleTelemetry;

                if (currentLap != playerVehicleScoring.mTotalLaps)
                {
                    currentLap = playerVehicleScoring.mTotalLaps;

                    _XMLoperator.Add(new Lap(
                        Encoding.ASCII.GetString(playerVehicleTelemetry.mTrackName).Trim('\0'),
                        _importedData.currentSession,
                        ConvertTimeToString(playerVehicleScoring.mLastSector1),
                        ConvertTimeToString(playerVehicleScoring.mLastSector2 - playerVehicleScoring.mLastSector1),
                        ConvertTimeToString(playerVehicleScoring.mLastLapTime - playerVehicleScoring.mLastSector2),
                        ConvertTimeToString(playerVehicleScoring.mLastLapTime)
                    ));

                    RefreshPlayerScoreboard();
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void RefreshPlayerScoreboard()
        {
            string trackName = _importedData.GetTrackName();
            var lapTimeOnTrack = _XMLoperator.laps.Where(lap => lap.track == trackName);
            lapTimeOnTrack = lapTimeOnTrack.OrderBy(p => p.lapTime);

            foreach (var lap in lapTimeOnTrack)
            {
                if (playerScoreboard.Count == 15)
                    break;
                playerScoreboard.Add(lap);
            }
        }


        private string ConvertTimeToString(double time)
        {
            int minutes = (int)(time / 60);
            double seconds = time % 60;
            return $"{minutes}:{seconds:00.000}";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
