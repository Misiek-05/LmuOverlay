using LmuOverlay.rFactor2Data;
using System.Windows;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        private rF2ImportedData _importedData = new rF2ImportedData();
        private XMLoperator _XMLoperator = new XMLoperator();

        public MainWindow()
        {
            InitializeComponent();
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
                XMLButton.Content = "Export Data";
            }
            else
            {
                _XMLoperator.ExportToXML();
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
