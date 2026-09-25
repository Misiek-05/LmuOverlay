using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
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

            _importedData.GetPlayerID();
            _importedData.GetPlayerTelemetry();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            _timer.Tick += RefreshPlayerScoreboard;
            _timer.Start();
        }

        private void RefreshPlayerScoreboard(object? sender, EventArgs e)
        {
            playerScoreboard.Clear();   

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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
