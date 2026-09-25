using LmuOverlay.rFactor2Data;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        private rF2ImportedData _importedData = new rF2ImportedData();
        private XMLoperator _XMLoperator = new XMLoperator();
        private DispatcherTimer _timer;

        private rF2VehicleScoring playerVehicleScoring;
        private rF2VehicleTelemetry playerVehicleTelemetry;
        private int currentLap = 0;

        private ObservableCollection<string> tracks;
        public ObservableCollection<string> Tracks
        {
            get { return tracks; }
            set { tracks = value; }
        }

        private ObservableCollection<Lap> playerScoreboard;
        public ObservableCollection<Lap> PlayerScoreboard
        {
            get { return playerScoreboard; }
            set { playerScoreboard = value; }
        }

        public MainWindow()
        {
            DataContext = this;
            playerScoreboard = new ObservableCollection<Lap>();
            tracks = new ObservableCollection<string>();

            InitializeComponent();

            currentLap = _importedData.player.playerVehicleScoring.mTotalLaps;
        }

        private void UpdateLapData(object? sender, EventArgs e)
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
                }
            }
            catch (Exception ex)
            {

            }
        }
        private string ConvertTimeToString(double time)
        {
            int minutes = (int)(time / 60);
            double seconds = time % 60;
            return $"{minutes}:{seconds:00.000}";
        }

        private void TelemetryWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Window telemetryWindow = new TelemetryWindow(_importedData, _XMLoperator);
            telemetryWindow.Show();
        }

        private void LeaderboardWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Window leaderboardWindow = new LeaderboardWindow(_importedData);
            leaderboardWindow.Show();
        }

        private void PlayerBestLapsWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Window playerBestLapWindow = new PlayerBestLapsWindow(_importedData, _XMLoperator);
            playerBestLapWindow.Show();
        }

        private void ConnectToGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rFactor2Connect();

                ConnectToGameButton.Visibility = Visibility.Hidden;
                PlayerBestLapsWindowButton.Visibility = Visibility.Visible;
                LeaderboardWindowButton.Visibility = Visibility.Visible;
                TelemetryWindowButton.Visibility = Visibility.Visible;
                XMLButton.Visibility = Visibility.Visible;

                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                _timer.Tick += UpdateLapData;
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Can't connect:\n\n{ex}",
                    "Shared Memory Error"
                );

                return;
            }
        }
        private void XMLBUtton_Click(object sender, RoutedEventArgs e)
        {
            if (_XMLoperator.imported == false)
            {
                _XMLoperator.ImportFromXML();
                foreach (var lap in _XMLoperator.laps)
                {
                    playerScoreboard.Add(lap);
                    AddTrack(lap);
                }
                XMLButton.Content = "Export Data";
            }
            else
            {
                _XMLoperator.ExportToXML();
            }

        }

        private void AddTrack(Lap lap)
        {
            if (!tracks.Contains(lap.track))
            {
                tracks.Add(lap.track);
            }
        }

        private void rFactor2Connect()
        {
            _importedData._telemetryBuffer = new MappedBuffer<rF2Telemetry>(
                rFactor2Constants.MM_TELEMETRY_FILE_NAME,
                false,
                false
            );

            _importedData._scoringBuffer = new MappedBuffer<rF2Scoring>(
                rFactor2Constants.MM_SCORING_FILE_NAME,
                false,
                false
            );

            _importedData._telemetryBuffer.Connect();
            _importedData._scoringBuffer.Connect();
            
            _importedData._scoringInfo = _importedData._scoring.mScoringInfo;
        }

    }
}
