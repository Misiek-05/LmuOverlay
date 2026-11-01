using LmuOverlay.rFactor2Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LmuOverlay
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private rF2ImportedData _importedData = new rF2ImportedData();
        private XMLoperator _XMLoperator = new XMLoperator();
        private DispatcherTimer _timer;

        private rF2VehicleScoring playerVehicleScoring;
        private rF2VehicleTelemetry playerVehicleTelemetry;
        private int currentLap = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private ObservableCollection<string> tracks;
        public ObservableCollection<string> Tracks
        {
            get { return tracks; }
            set { tracks = value; }
        }

        private string _selectedTrack;
        public string SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (_selectedTrack == value) return;
                _selectedTrack = value;
                OnPropertyChanged();
                SortTracksOnScoreboard();
            }
        }

        public void SortTracksOnScoreboard()
        {
            if (_selectedTrack == "All") {
                playerScoreboard.Clear();
                foreach (var lap in _XMLoperator.laps)
                    playerScoreboard.Add(lap);
                AverageGrid.Visibility = Visibility.Hidden;
            } else
            {
                playerScoreboard.Clear();
                foreach (var lap in _XMLoperator.laps)
                    if (_selectedTrack == lap.track)
                        playerScoreboard.Add(lap);
                AverageGrid.Visibility = Visibility.Visible;
            }
        }


        private ObservableCollection<Lap> playerScoreboard;
        public ObservableCollection<Lap> PlayerScoreboard
        {
            get { return playerScoreboard; }
            set { playerScoreboard = value; }
        }

        private ObservableCollection<Session> sessions;
        public ObservableCollection<Session> Sessions
        {
            get { return sessions; }
            set { sessions = value; }
        }

        private Session _selectedSession;
        public Session SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (_selectedSession == value) return;
                _selectedSession = value;
                OnPropertyChanged();
                AverageTime();
            }
        }

        private void AverageTime()
        {
            if (playerScoreboard == null || playerScoreboard.Count == 0)
            {
                ClearAverages();
                return;
            }

            var laps = (_selectedSession == Session.All)
                ? playerScoreboard.ToList()
                : playerScoreboard.Where(l => l.session == _selectedSession).ToList();

            var timed = laps
                .Select(l => new { Lap = l, Time = ParseTime(l.lapTime) })
                .Where(x => x.Time > 0)
                .ToList();

            if (timed.Count == 0)
            {
                ClearAverages();
                return;
            }

            double best = timed.Min(x => x.Time);
            var valid = timed.Where(x => x.Time <= best * 1.07).ToList(); 

            double avgS1 = valid.Average(x => ParseTime(x.Lap.sector1));
            double avgS2 = valid.Average(x => ParseTime(x.Lap.sector2));
            double avgS3 = valid.Average(x => ParseTime(x.Lap.sector3));
            double avgLap = valid.Average(x => x.Time);

            Sector1Time.Text = $"Average Sector 1: {ConvertTimeToString(avgS1)}";
            Sector2Time.Text = $"Average Sector 2: {ConvertTimeToString(avgS2)}";
            Sector3Time.Text = $"Average Sector 3: {ConvertTimeToString(avgS3)}";
            LapTime.Text = $"Average lap time: {ConvertTimeToString(avgLap)}";
        }

        private void ClearAverages()
        {
            Sector1Time.Text = "Average Sector 1: -";
            Sector2Time.Text = "Average Sector 2: -";
            Sector3Time.Text = "Average Sector 3: -";
            LapTime.Text = "Average lap time: -";
        }

        public MainWindow()
        {
            DataContext = this;
            playerScoreboard = new ObservableCollection<Lap>();
            tracks = new ObservableCollection<string>();
            sessions = new ObservableCollection<Session> {Session.Practice, Session.Qualify, Session.Race, Session.All};

            InitializeComponent();

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
        private static double ParseTime(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            var parts = s.Split(':');
            if (parts.Length != 2) return 0;

            return int.Parse(parts[0]) * 60 +
                   double.Parse(parts[1].Replace(',', '.'), CultureInfo.InvariantCulture);
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

                XMLButton.Visibility = Visibility.Visible;
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
                AddTrack(new Lap("All", 0, "", "", "", ""));
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
