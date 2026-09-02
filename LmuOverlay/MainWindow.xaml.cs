using LmuOverlay.rFactor2Data;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LmuOverlay
{
    public partial class MainWindow : Window
    {
        private MappedBuffer<rF2Telemetry> _telemetryBuffer;
        private MappedBuffer<rF2Scoring> _scoringBuffer;
        private rF2Telemetry _telemetry = new rF2Telemetry();
        private rF2Scoring _scoring = new rF2Scoring();
        private int playerID;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _telemetryBuffer = new MappedBuffer<rF2Telemetry>(
                rFactor2Constants.MM_TELEMETRY_FILE_NAME,
                false,
                false
            );

            _scoringBuffer = new MappedBuffer<rF2Scoring>(
                rFactor2Constants.MM_SCORING_FILE_NAME,
                false,
                false
            );

            try
            {
                _telemetryBuffer.Connect();
                _scoringBuffer.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nie udało się połączyć z LMU Shared Memory:\n\n{ex}",
                    "Shared Memory Error"
                );

                return;
            }

            playerID = 0;

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
                GetPlayerID();

                for (int i = 0; i < _telemetry.mNumVehicles; i++)
                {
                    rF2VehicleTelemetry vehicle = _telemetry.mVehicles[i];

                    if (vehicle.mID == playerID)
                    {
                        double rpm = vehicle.mEngineRPM;
                        int gear = vehicle.mGear;

                        double speedMs = Math.Sqrt(
                            vehicle.mLocalVel.x * vehicle.mLocalVel.x +
                            vehicle.mLocalVel.y * vehicle.mLocalVel.y +
                            vehicle.mLocalVel.z * vehicle.mLocalVel.z
                        );
                        double speedKmh = speedMs * 3.6;

                        SpeedKmhDisplay(speedKmh);
                        GearDisplay(gear);
                        RpmDisplay(rpm);

                        rF2Wheel[] wheels = vehicle.mWheels;
                        BreakTmpDisplay(wheels);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void SpeedKmhDisplay(double speedKmh)
        {
            SpeedText.Text = $"{speedKmh:F0} km/h";
        }

        private void GearDisplay(int gear)
        {
            switch (gear)
            {
                case -1:
                    GearText.Text = "R";
                    GearText.Foreground = Brushes.Red;
                    break;
                case 0:
                    GearText.Text = "N";
                    GearText.Foreground = Brushes.Blue;
                    break;
                default:
                    GearText.Text = gear.ToString();
                    GearText.Foreground = Brushes.Green;
                    break;
            }
        }

        private void RpmDisplay(double rpm)
        {
            RpmText.Text = $"RPM: {rpm:F0}";
        }

        private void BreakTmpDisplay(rF2Wheel[] wheels)
        {
            FrontLeftBreakTmp.Text = Math.Round(wheels[0].mBrakeTemp - 273.15, 1).ToString();
            FrontRightBreakTmp.Text = Math.Round(wheels[1].mBrakeTemp - 273.15, 1).ToString();
            RearLeftBreakTmp.Text = Math.Round(wheels[2].mBrakeTemp - 273.15, 1).ToString();
            RearRightBreakTmp.Text = Math.Round(wheels[3].mBrakeTemp - 273.15, 1).ToString();
        }

        private void GetPlayerID()
        {
            _scoringBuffer.GetMappedData(ref _scoring);
            foreach (rF2VehicleScoring vehicleScoring in _scoring.mVehicles)
            {
                if (vehicleScoring.mIsPlayer == 1)
                {
                    playerID = vehicleScoring.mID;
                    break;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}