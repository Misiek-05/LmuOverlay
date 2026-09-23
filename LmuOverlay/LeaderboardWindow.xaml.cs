using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LmuOverlay
{
    /// <summary>
    /// Interaction logic for LeaderboardWindow.xaml
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        private rF2ImportedData _importedData;
        private DispatcherTimer _timer;
        private List<LeaderboardItem> drivers = new List<LeaderboardItem>();
        public LeaderboardWindow(rF2ImportedData _importedData)
        {
            DataContext = this;
            leaderboard = new ObservableCollection<LeaderboardItem>();

            InitializeComponent();
            this._importedData = _importedData;

            _importedData.GetPlayerID();
            foreach (rF2VehicleScoring vehicleScoring in _importedData._scoring.mVehicles)
            {
                if (Encoding.ASCII.GetString(vehicleScoring.mVehicleClass).TrimEnd('\0') == Encoding.ASCII.GetString(_importedData.player.mVehicleClass).TrimEnd('\0'))
                {
                    drivers.Add(new LeaderboardItem(
                        vehicleScoring.mIsPlayer,
                        vehicleScoring.mID,
                        vehicleScoring.mPlace,
                        ConvertName(Encoding.UTF8.GetString(vehicleScoring.mDriverName).TrimEnd('\0'))
                    ));
                }
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };

            _timer.Tick += UpdateLeaderboard;
            _timer.Start();
        }

        private ObservableCollection<LeaderboardItem> leaderboard;
        public ObservableCollection<LeaderboardItem> Leaderboard
        {
            get { return leaderboard; }
            set { leaderboard = value; }
        }

        private void UpdateLeaderboard(object? sender, EventArgs e)
        {
            
            leaderboard.Clear();
            _importedData.GetPlayerID();

            SessionAndTimeDisplay();
            foreach (var driver in drivers)
                driver.updated = 0;

            foreach (rF2VehicleScoring vehicleScoring in _importedData._scoring.mVehicles)
            {
                if (Encoding.ASCII.GetString(vehicleScoring.mVehicleClass).TrimEnd('\0') == Encoding.ASCII.GetString(_importedData.player.mVehicleClass).TrimEnd('\0'))
                {
                    if (WasDriverOnGrid(vehicleScoring))
                    {
                        UpdateDriverScoring(vehicleScoring);
                    }
                    else
                    {
                        drivers.Add(new LeaderboardItem(
                            vehicleScoring.mIsPlayer,
                            vehicleScoring.mID,
                            vehicleScoring.mPlace,
                            ConvertName(Encoding.UTF8.GetString(vehicleScoring.mDriverName).TrimEnd('\0'))
                        ));
                        drivers.ElementAt(drivers.Count - 1).updated = 1;
                    }
                }
            }

            if (_importedData.currentSession == Session.Practice || _importedData.currentSession == Session.Qualify)
                UpdateGap();
            if (_importedData.currentSession == Session.Race)
                UpdateRaceGap();

            driversOnLeaderboard(drivers);

        }

        private void UpdateGap()
        {
            var sortedDrivers = drivers.OrderBy(i => i.place).ToList();
            double leaderTime = sortedDrivers[0].bestLapTimeDouble;

            for (int i = 1; i < sortedDrivers.Count; i++)
            {
                double gap = sortedDrivers[i].bestLapTimeDouble - leaderTime;
                sortedDrivers[i].gap = ConvertGapTimeToString(gap);
            }
        }

        private void UpdateRaceGap()
        {
            var sortedDrivers = drivers.OrderBy(i => i.place).ToList();
            int driverIndex = 0;

            for (int i = 0; i < sortedDrivers.Count; i++) 
            { 
                if (sortedDrivers[i].isPlayer == 1)
                {
                    driverIndex = i;
                    sortedDrivers[i].gap = "-";
                    break;
                } 
            }

            double totalGapBehind = 0;
            for (int i = driverIndex + 1; i < sortedDrivers.Count; i++)
            {
                totalGapBehind += sortedDrivers[i].gapDouble;
                sortedDrivers[i].gap = ConvertGapBehindTimeToString(totalGapBehind);
            }

            double totalGapInFront = 0;
            for (int i = driverIndex; i > 0; i--) 
            { 
                totalGapInFront += sortedDrivers[i].gapDouble;
                sortedDrivers[i - 1].gap = ConvertGapInFrontTimeToString(totalGapInFront);
            }

        }

        private bool WasDriverOnGrid(rF2VehicleScoring vehicleScoring)
        {
            foreach (var driver in drivers) 
            {
                if (driver.mID == vehicleScoring.mID)
                    return true;
            }
            return false;
        }

        private void UpdateDriverScoring(rF2VehicleScoring vehicleScoring)
        {
            foreach (var driver in drivers) {
                if (driver.mID == vehicleScoring.mID)
                {
                    if (_importedData.currentSession == Session.Race) { 
                        driver.Update(
                            vehicleScoring.mPlace,
                            ConvertTimeToString(vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastSector2 - vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastLapTime - vehicleScoring.mLastSector2),
                            ConvertTimeToString(vehicleScoring.mLastLapTime),
                            ConvertTimeToString(vehicleScoring.mBestLapTime),
                            vehicleScoring.mBestLapTime,
                            ConvertTimeToString(vehicleScoring.mTimeBehindNext),
                            vehicleScoring.mTimeBehindNext,
                            driverStatus(vehicleScoring)
                        );
                        driver.updated = 1;
                    } 

                    if (_importedData.currentSession == Session.Qualify || _importedData.currentSession == Session.Practice)
                    {
                        driver.Update(
                            vehicleScoring.mPlace,
                            ConvertTimeToString(vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastSector2 - vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastLapTime - vehicleScoring.mLastSector2),
                            ConvertTimeToString(vehicleScoring.mLastLapTime),
                            ConvertTimeToString(vehicleScoring.mBestLapTime),
                            vehicleScoring.mBestLapTime,
                            "-",
                            vehicleScoring.mTimeBehindNext,
                            driverStatus(vehicleScoring)
                        );
                        driver.updated = 1;
                    }
                }
            }
        }

        private string driverStatus(rF2VehicleScoring vehicleScoring)
        {
            switch (vehicleScoring.mFinishStatus)
            {
                case 1:
                    return "FIN";
                case 2:
                    return "DNF";
                case 3:
                    return "DSQ";
            }

            if (vehicleScoring.mInPits == 1)
                return "PIT";

            return " ";
        }

        private void driversOnLeaderboard(List<LeaderboardItem> drivers)
        {
            var sortedDrivers = drivers.OrderBy(i => i.place).ToList();
            for (int i = 0; i < drivers.Count; i++)
            {
                sortedDrivers[i].place = i + 1;
                if (sortedDrivers[i].isPlayer == 1)
                    _importedData.player.place = i + 1;
            }

            // Current leader of the race / fastest driver in qualify 
            leaderboard.Add(sortedDrivers[0]);

            if (sortedDrivers.Count <= 16) // leader + 7 in front + player + 7 behind
            {
                for (int i = 1; i < drivers.Count; i++)
                    leaderboard.Add(sortedDrivers[i]);
            }
            else
            {
                int placeInFront = _importedData.player.place - 7; // 7 drivers in front of player 
                int placeBehind = _importedData.player.place + 7; // 7 drivers behind player

                if (placeInFront < 1)
                {
                    placeBehind -= placeInFront;
                    placeBehind++;
                    placeInFront = 1;
                }

                if (placeBehind > sortedDrivers.Count)
                {
                    int over = placeBehind - sortedDrivers.Count;
                    placeInFront -= over;
                    placeBehind = sortedDrivers.Count;
                }

                for (int i = placeInFront; i < placeBehind; i++)
                    leaderboard.Add(sortedDrivers[i]);
            }

        }

        private void SessionAndTimeDisplay()
        {
            double currentTime = _importedData._scoringInfo.mCurrentET;
            double endTime = _importedData._scoringInfo.mEndET;

            switch (_importedData.currentSession)
            {
                case Session.Practice:
                    {
                        SessionTextBlock.Text = "Practice";
                        TimeDisplayHelper(TimeTextBlock, endTime - currentTime);
                        break;
                    }
                case Session.Qualify:
                    {
                        SessionTextBlock.Text = "Qualify";
                        TimeDisplayHelper(TimeTextBlock, endTime - currentTime);
                        break;
                    }
                case Session.Warmup:
                    {
                        SessionTextBlock.Text = "Warmup";
                        TimeDisplayHelper(TimeTextBlock, 0);
                        break;
                    }
                case Session.Race:
                    {
                        SessionTextBlock.Text = "Race";
                        TimeDisplayHelper(TimeTextBlock, endTime - currentTime);
                        break;
                    }
            }

        }

        private void TimeDisplayHelper(TextBlock textBlock, double time)
        {
            int totalMinutes = (int)(time / 60);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            double seconds = time % 60;

            textBlock.Text = $"{hours}:{minutes:00}:{seconds:00}";
        }
       
        private string ConvertGapTimeToString(double time)
        {
            double seconds = time % 60;
            string returnString = $"+{seconds:00.000}";
            if (returnString.Contains("-"))
                return "-";
            return returnString;
        }

        private string ConvertGapBehindTimeToString(double time)
        {
            double seconds = time % 60;
            string returnString = $"+{seconds:00.000}";
            return returnString;
        }
        private string ConvertGapInFrontTimeToString(double time)
        {
            double seconds = time % 60;
            string returnString = $"-{seconds:00.000}";
            return returnString;
        }

        private string ConvertTimeToString(double time)
        {
            string returnString = $"{(int)(time / 60)}:{(time % 60):00.000}";
            if (time == 0)
                return "     -     ";

            if (returnString.Contains("-"))
                return "     -     ";
            return returnString;
        }

        private string ConvertName(string name)
        {
            var splits = name.Split(' ');
            string nameShorter = splits[0][0].ToString() + " ";
            if (splits.Length > 2)
            {
                nameShorter += splits[1][0] + " ";
                nameShorter += splits[2];
            }
            else
            {
                nameShorter += splits[1];
            }
            return nameShorter;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
