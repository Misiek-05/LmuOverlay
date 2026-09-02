using LmuOverlay.rFactor2Data;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        // Używamy MappedBuffer z podpiętą strukturą telemetrii
        private MappedBuffer<rF2Telemetry> _telemetryBuffer;
        private rF2Telemetry _telemetry = new rF2Telemetry();
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _telemetryBuffer = new MappedBuffer<rF2Telemetry>(
                rFactor2Constants.MM_TELEMETRY_FILE_NAME,
                false,
                false
            );

            try
            {
                _telemetryBuffer.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nie udało się połączyć z LMU Shared Memory:\n\n{ex}",
                    "Shared Memory Error"
                );

                return;
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _timer.Tick += UpdateTelemetry;
            _timer.Start();
        }

        private void UpdateTelemetry(object? sender, EventArgs e)
        {
            try
            {
                _telemetryBuffer.GetMappedData(ref _telemetry);

                if (_telemetry.mNumVehicles > 0)
                {
                    rF2VehicleTelemetry playerCar = _telemetry.mVehicles[0];

                    double rpm = playerCar.mEngineRPM;
                    int gear = playerCar.mGear;

                    double speedMs = Math.Sqrt(
                        playerCar.mLocalVel.x * playerCar.mLocalVel.x +
                        playerCar.mLocalVel.y * playerCar.mLocalVel.y +
                        playerCar.mLocalVel.z * playerCar.mLocalVel.z
                    );
                    double speedKmh = speedMs * 3.6;

                    SpeedText.Text = $"{speedKmh:F0} km/h";
                    GearText.Text = gear == -1 ? "R" : gear == 0 ? "N" : gear.ToString();
                    RpmText.Text = $"RPM: {rpm:F0}";
                }
            }
            catch (Exception ex)
            {

            }
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}