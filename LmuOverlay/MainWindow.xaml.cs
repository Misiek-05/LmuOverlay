using LmuOverlay.rFactor2Data;
using System.Windows;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        public rF2ImportedData _importedData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TelemetryWindowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rFactor2Connect();
                Window telemetryWindow = new TelemetryWindow(_importedData);
                telemetryWindow.Show();
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
        }
    }
}
