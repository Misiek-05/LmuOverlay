using System.Windows;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TelemetryWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Window telemetryWindow = new TelemetryWindow();
            telemetryWindow.Show();
        }
    }
}
