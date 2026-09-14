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
    public partial class TelemetryWindow : Window
    {
        private rF2ImportedData _importedData;
        private TelemetryData _telemetryData;
        private rF2VehicleScoring playerVehicleScoring;
        private rF2VehicleTelemetry playerVehicleTelemetry;

        private short currentLap;
        private int currentSector;
        private bool showingLastLap = false;
        private DateTime lastLapDisplayUntil;
        private bool isTelemetryDataCreated = false;

        private DispatcherTimer _timer;
        public TelemetryWindow(rF2ImportedData _importedData)
        {
            InitializeComponent();
            this._importedData = _importedData;

            currentLap = 0;
            currentSector = 2;

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
                _importedData._telemetryBuffer.GetMappedData(ref _importedData._telemetry);
                _importedData.GetPlayerID();
                playerVehicleScoring = _importedData.playerVehicleScoring;

                for (int i = 0; i < _importedData._telemetry.mNumVehicles; i++)
                {
                    playerVehicleTelemetry = _importedData._telemetry.mVehicles[i];

                    if (!isTelemetryDataCreated) {
                        _telemetryData = new TelemetryData(playerVehicleTelemetry.mTrackName);
                        isTelemetryDataCreated = true;
                    }

                    if (playerVehicleTelemetry.mID == _importedData.playerID)
                    {

                        SpeedKmhDisplay();
                        GearDisplay();
                        RpmDisplay();

                        rF2Wheel[] wheels = playerVehicleTelemetry.mWheels;
                        BreakTmpDisplay(wheels);
                        TyreTmpDisplay(wheels);
                        TyreWearDisplay(wheels);

                        CurrentLapDisplay();
                        FuelDisplay();
                        ThrottleDisplay();
                        BrakeDisplay();
                        SteeringDisplay();

                        TimeDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void CurrentLapDisplay()
        {
            TotalLaps.Text = $"Lap: {playerVehicleScoring.mTotalLaps}";
        }
        private void TimeDisplay()
        {
            if (currentLap != playerVehicleScoring.mTotalLaps)
            {
                lastLapDisplayUntil = DateTime.Now.AddSeconds(2);
                showingLastLap = true;
                currentLap = playerVehicleScoring.mTotalLaps;

                TimeDisplayHelper(BestSector1Time, playerVehicleScoring.mBestLapSector1);
                TimeDisplayHelper(BestSector2Time, playerVehicleScoring.mBestLapSector2 - playerVehicleScoring.mBestLapSector1);
                TimeDisplayHelper(BestSector3Time, playerVehicleScoring.mBestLapTime - playerVehicleScoring.mBestLapSector2);
                TimeDisplayHelper(BestLapTime, playerVehicleScoring.mBestLapTime);
            }

            if (showingLastLap)
            {
                if (DateTime.Now < lastLapDisplayUntil)
                {
                    TimeDisplayHelper(CurrentLapTime, playerVehicleScoring.mLastLapTime);
                    TimeDisplayHelper(CurrentSector1Time, playerVehicleScoring.mLastSector1);
                    TimeDisplayHelper(CurrentSector2Time, playerVehicleScoring.mLastSector2 - playerVehicleScoring.mLastSector1);
                    TimeDisplayHelper(CurrentSector3Time, playerVehicleScoring.mLastLapTime - playerVehicleScoring.mLastSector2);
                    BetterSectorDisplay(CurrentSector3TimeStatus, playerVehicleScoring.mLastLapTime - playerVehicleScoring.mLastSector2, playerVehicleScoring.mBestLapTime - playerVehicleScoring.mBestLapSector2);
                    return;
                }

                _telemetryData.updateData(playerVehicleTelemetry, playerVehicleScoring);
                _telemetryData.exportDataToCSV();
                CurrentSector1Time.Text = " ";
                CurrentSector2Time.Text = " ";
                CurrentSector3Time.Text = " ";

                CurrentSector1TimeStatus.Fill = Brushes.Transparent;
                CurrentSector2TimeStatus.Fill = Brushes.Transparent;
                CurrentSector3TimeStatus.Fill = Brushes.Transparent;

                showingLastLap = false;
            }

            double time = _importedData._scoring.mScoringInfo.mCurrentET - playerVehicleScoring.mLapStartET;
            TimeDisplayHelper(CurrentLapTime, time);

            if (currentSector != playerVehicleScoring.mSector)
            {
                switch (currentSector)
                {
                    case 1:
                        {
                            TimeDisplayHelper(CurrentSector1Time, playerVehicleScoring.mCurSector1);
                            BetterSectorDisplay(CurrentSector1TimeStatus, playerVehicleScoring.mCurSector1, playerVehicleScoring.mBestLapSector1);
                            break;
                        }
                    case 2:
                        {
                            TimeDisplayHelper(CurrentSector2Time, playerVehicleScoring.mCurSector2 - playerVehicleScoring.mCurSector1);
                            BetterSectorDisplay(CurrentSector2TimeStatus, playerVehicleScoring.mCurSector2 - playerVehicleScoring.mCurSector1, playerVehicleScoring.mBestLapSector2 - playerVehicleScoring.mBestLapSector1);
                            break;
                        }
                }
                currentSector = playerVehicleScoring.mSector;
            }
        }

        private void BetterSectorDisplay(Rectangle rectangle, double currentSectorTime, double bestSectorTime)
        {
            if (currentSectorTime > bestSectorTime)
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

        private void SteeringDisplay()
        {
            double steering = playerVehicleTelemetry.mUnfilteredSteering;

            if (steering < 0)
            {
                SteeringLeftProgressBar.Value = 1.0 + steering;
            } else
            {
                SteeringRightProgressBar.Value = steering;
            }
        }

        private void ThrottleDisplay()
        {
            double throttleInput = playerVehicleTelemetry.mUnfilteredThrottle;
            ThrottleProgressBar.Value = throttleInput;
        }
        private void BrakeDisplay()
        {
            double brakeInput = playerVehicleTelemetry.mUnfilteredBrake;
            BrakeProgressBar.Value = brakeInput;
        }

        private void SpeedKmhDisplay()
        {
            double speedMs = Math.Sqrt(
                playerVehicleTelemetry.mLocalVel.x * playerVehicleTelemetry.mLocalVel.x +
                playerVehicleTelemetry.mLocalVel.y * playerVehicleTelemetry.mLocalVel.y +
                playerVehicleTelemetry.mLocalVel.z * playerVehicleTelemetry.mLocalVel.z
            );
            double speedKmh = speedMs * 3.6;
            SpeedText.Text = $"{speedKmh:F0} km/h";
        }

        private void GearDisplay()
        {
                       
            int gear = playerVehicleTelemetry.mGear;
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


        private void RpmDisplay()
        {
            double rpm = playerVehicleTelemetry.mEngineRPM;
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

        private void FuelDisplay()
        {
            FuelProgressBar.Maximum = playerVehicleTelemetry.mFuelCapacity;
            FuelProgressBar.Value = playerVehicleTelemetry.mFuel;
            FuelText.Text = Math.Round(playerVehicleTelemetry.mFuel, 1).ToString() + "%";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}