using LmuOverlay.rFactor2Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private rF2VehicleScoring playerVehicle;
        private short currentLap;

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
            currentLap = 0;

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
                        TyreTmpDisplay(wheels);
                        TyreWearDisplay(wheels);

                        FuelDisplay(vehicle);
                        ThrottleDisplay(vehicle);
                        BrakeDisplay(vehicle);
                        SteeringDisplay(vehicle);

                        TimeDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void TimeDisplay()
        {
            if (currentLap != playerVehicle.mTotalLaps)
            {
                currentLap = playerVehicle.mTotalLaps;
                CurrentSector1Time.Text = " ";
                CurrentSector2Time.Text = " ";
                CurrentSector3Time.Text = " ";
                CurrentLapTime.Text = " ";

                CurrentSector1TimeStatus.Fill = Brushes.Transparent;
                CurrentSector2TimeStatus.Fill = Brushes.Transparent;
                CurrentSector3TimeStatus.Fill = Brushes.Transparent;


                TimeDisplayHelper(BestSector1Time, playerVehicle.mBestLapSector1);
                TimeDisplayHelper(BestSector2Time, playerVehicle.mBestLapSector2 - playerVehicle.mBestLapSector1);
                TimeDisplayHelper(BestSector3Time, playerVehicle.mBestLapTime - playerVehicle.mBestLapSector2);
                TimeDisplayHelper(BestLapTime, playerVehicle.mBestLapTime);
            }

            double time = _scoring.mScoringInfo.mCurrentET - playerVehicle.mLapStartET;
            TimeDisplayHelper(CurrentLapTime, time);

            switch (playerVehicle.mSector)
            {
                case 1: // Sector 1
                    {
                        TimeDisplayHelper(CurrentSector1Time, time);
                        //BetterSectorDisplay(CurrentSector1TimeStatus, time, playerVehicle.mBestLapSector1);
                        break;
                    }
                case 2: // Sector 2
                    {
                        TimeDisplayHelper(CurrentSector2Time, time - playerVehicle.mCurSector1);
                        //BetterSectorDisplay(CurrentSector2TimeStatus, time - playerVehicle.mCurSector1, playerVehicle.mBestLapSector2 - playerVehicle.mBestLapSector1);
                        break;
                    }
                case 0: // Sector 3
                    {
                        TimeDisplayHelper(CurrentSector3Time, time - playerVehicle.mCurSector2);
                        //BetterSectorDisplay(CurrentSector3TimeStatus, time - playerVehicle.mCurSector2, playerVehicle.mBestLapTime - playerVehicle.mBestLapSector2);
                        break;
                    }
            }

        }

        private void BetterSectorDisplay(Rectangle rectangle, double currentSectorTime, double bestSectorTime)
        {
            if (currentSectorTime < bestSectorTime)
                rectangle.Fill = Brushes.Yellow;
            else
                rectangle.Fill = Brushes.Green;
            
        }
        private void TimeDisplayHelper(TextBlock textBlock, double time)
        {
            int minutes = (int)(time / 60);
            double seconds = time % 60;
            textBlock.Text = $"{minutes}:{seconds:00.000}";
        }

        private void SteeringDisplay(rF2VehicleTelemetry vehicle)
        {
            double steering = vehicle.mUnfilteredSteering;

            if (steering < 0)
            {
                SteeringLeftProgressBar.Value = 1.0 + steering;
            } else
            {
                SteeringRightProgressBar.Value = steering;
            }
        }

        private void ThrottleDisplay(rF2VehicleTelemetry vehicle)
        {
            double throttleInput = vehicle.mUnfilteredThrottle;
            ThrottleProgressBar.Value = throttleInput;
        }
        private void BrakeDisplay(rF2VehicleTelemetry vehicle)
        {
            double brakeInput = vehicle.mUnfilteredBrake;
            BrakeProgressBar.Value = brakeInput;
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
            if (rpm < 6000)
            {
                RpmLight1.Fill = Brushes.Transparent;
                RpmLight2.Fill = Brushes.Transparent;
                RpmLight3.Fill = Brushes.Transparent;
                RpmLight4.Fill = Brushes.Transparent;
                RpmLight5.Fill = Brushes.Transparent;
                RpmLight6.Fill = Brushes.Transparent;
                RpmLight7.Fill = Brushes.Transparent;
                RpmLight8.Fill = Brushes.Transparent;
            }
            if (rpm > 6000)
                RpmLight1.Fill = Brushes.Green;
            if (rpm > 6100)
                RpmLight2.Fill = Brushes.Green;
            if (rpm > 6200)
                RpmLight3.Fill = Brushes.Green;
            if (rpm > 6300)
                RpmLight4.Fill = Brushes.Red;
            if (rpm > 6400)
                RpmLight5.Fill = Brushes.Red;
            if (rpm > 6500)
                RpmLight6.Fill = Brushes.Red;
            if (rpm > 6600)
                RpmLight7.Fill = Brushes.Blue;
            if (rpm > 6700)
            {
                RpmLight1.Fill = Brushes.Blue;
                RpmLight2.Fill = Brushes.Blue;
                RpmLight3.Fill = Brushes.Blue;
                RpmLight4.Fill = Brushes.Blue;
                RpmLight5.Fill = Brushes.Blue;
                RpmLight6.Fill = Brushes.Blue;
                RpmLight7.Fill = Brushes.Blue;
                RpmLight8.Fill = Brushes.Blue;
            }

        }

        private void BreakTmpDisplay(rF2Wheel[] wheels)
        {
            BreakTmpDisplayHelper(FrontLeftBreakTmp, wheels[0]);
            BreakTmpDisplayHelper(FrontRightBreakTmp, wheels[1]);
            BreakTmpDisplayHelper(RearLeftBreakTmp, wheels[2]);
            BreakTmpDisplayHelper(RearRightBreakTmp, wheels[3]);
        }

         private void BreakTmpDisplayHelper(TextBlock textBlock, rF2Wheel wheel)
        {
            textBlock.Text = Math.Round(wheel.mBrakeTemp - 273.15, 1).ToString();
            textBlock.Foreground = GetBreakTmpColor(wheel.mBrakeTemp - 273.15);
        }

        private SolidColorBrush GetBreakTmpColor(double breakTmp)
        {
            if (breakTmp < 250)
                return Brushes.LightBlue;
            if (breakTmp < 450)
                return Brushes.LightGreen;
            if (breakTmp < 650)
                return Brushes.Green;
            if (breakTmp < 750)
                return Brushes.Yellow;
            return Brushes.Red; // over 750 Celsius
        }

        private void TyreTmpDisplay(rF2Wheel[] wheels)
        {
            TyreTmpDisplayHelper(FrontLeftWheelTmp, wheels[0]);
            TyreTmpDisplayHelper(FrontRightWheelTmp, wheels[1]);
            TyreTmpDisplayHelper(RearLeftWheelTmp, wheels[2]);
            TyreTmpDisplayHelper(RearRightWheelTmp, wheels[3]);
        }

        private void TyreTmpDisplayHelper(TextBlock textBlock, rF2Wheel wheel)
        {
            textBlock.Text = Math.Round(wheel.mTireCarcassTemperature - 273.15, 1).ToString();
            if (wheel.mDetached == 1)
            {
                textBlock.Text = "-";
                textBlock.Foreground = Brushes.Red;
            }
            else
                textBlock.Foreground = GetTyreTmpColor(wheel.mTireCarcassTemperature - 273.15);
        }

        private SolidColorBrush GetTyreTmpColor(double TyreTmp)
        {
            if (TyreTmp < 50)
                return Brushes.LightBlue;
            if (TyreTmp < 65)
                return Brushes.LightGreen;
            if (TyreTmp < 85)
                return Brushes.LimeGreen;
            if (TyreTmp < 95)
                return Brushes.Yellow;
            return Brushes.Red; // over 95 Celsius
        }

        private void TyreWearDisplay(rF2Wheel[] wheels)
        {
            TyreWearDisplayHelper(FrontLeftWheelWear, wheels[0]);
            TyreWearDisplayHelper(FrontRightWheelWear, wheels[1]);
            TyreWearDisplayHelper(RearLeftWheelWear, wheels[2]);
            TyreWearDisplayHelper(RearRightWheelWear, wheels[3]);
        }

        private void TyreWearDisplayHelper(TextBlock textBlock, rF2Wheel wheel)
        {
            textBlock.Text = Math.Round(wheel.mWear * 100, 0).ToString() + "%";
            if (wheel.mFlat == 1 || wheel.mDetached == 1)
                textBlock.Foreground = Brushes.Red;
            else
                textBlock.Foreground = GetTyreWearColor(wheel.mWear);
        }

        private SolidColorBrush GetTyreWearColor(double TyreWear)
        {
            if (TyreWear < 0.3)
                return Brushes.Red;
            if (TyreWear < 0.5)
                return Brushes.Orange;
            if (TyreWear < 0.7)
                return Brushes.Yellow;
            if (TyreWear < 0.85)
                return Brushes.LightGreen;
            return Brushes.LimeGreen;
        }

        private void FuelDisplay(rF2VehicleTelemetry vehicle)
        {
            FuelProgressBar.Maximum = vehicle.mFuelCapacity;
            FuelProgressBar.Value = vehicle.mFuel;
            FuelText.Text = Math.Round(vehicle.mFuel, 1).ToString() + "%";
        }

        private void GetPlayerID()
        {
            _scoringBuffer.GetMappedData(ref _scoring);
            foreach (rF2VehicleScoring vehicleScoring in _scoring.mVehicles)
            {
                if (vehicleScoring.mIsPlayer == 1)
                {
                    playerID = vehicleScoring.mID;
                    playerVehicle = vehicleScoring;
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