using LmuOverlay.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public LeaderboardWindow(rF2ImportedData _importedData)
        {
            DataContext = this;
            leaderboard = new ObservableCollection<LeaderboardItem>();

            InitializeComponent();
            this._importedData = _importedData;

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
            var drivers = new List<LeaderboardItem>();

            foreach (rF2VehicleScoring vehicleScoring in _importedData._scoring.mVehicles)
            {
                if (Encoding.ASCII.GetString(vehicleScoring.mVehicleClass).TrimEnd('\0') == Encoding.ASCII.GetString(_importedData.player.mVehicleClass).TrimEnd('\0'))
                {
                    if (_importedData.currentSession == Session.Practice || _importedData.currentSession == Session.Qualify)
                    {
                        drivers.Add(new LeaderboardItem(
                            vehicleScoring.mPlace,
                            ConvertName(Encoding.UTF8.GetString(vehicleScoring.mDriverName).TrimEnd('\0')),
                            ConvertTimeToString(vehicleScoring.mBestSector1),
                            ConvertTimeToString(vehicleScoring.mBestSector2 - vehicleScoring.mBestSector1),
                            ConvertTimeToString(vehicleScoring.mBestLapTime - vehicleScoring.mBestSector2),
                            ConvertTimeToString(vehicleScoring.mBestLapTime),
                            "     -     "
                        ));
                    }

                    if (_importedData.currentSession == Session.Warmup || _importedData.currentSession == Session.Race)
                    {
                        drivers.Add(new LeaderboardItem(
                            vehicleScoring.mPlace,
                            ConvertName(Encoding.UTF8.GetString(vehicleScoring.mDriverName).TrimEnd('\0')),
                            ConvertTimeToString(vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastSector2 - vehicleScoring.mLastSector1),
                            ConvertTimeToString(vehicleScoring.mLastLapTime - vehicleScoring.mLastSector2),
                            ConvertTimeToString(vehicleScoring.mLastLapTime),
                            ConvertTimeToString(vehicleScoring.mTimeBehindNext)
                        ));
                    }


                    if (vehicleScoring.mIsPlayer == 1)
                    {
                        _importedData.player.place = vehicleScoring.mPlace;
                    }
                }
            }

            var sortedDrivers = drivers.OrderBy(i => i.place).ToList();
            for (int i = 0; i < drivers.Count; i++) {
                var item = sortedDrivers[i];
                item.place = i + 1;
                leaderboard.Add(item);
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

        private string ConvertTimeToString(double time)
        {
            if (time == 0)
                return "      -   ";
            return $"{(int)(time / 60)}:{(time % 60):00.000}";
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
