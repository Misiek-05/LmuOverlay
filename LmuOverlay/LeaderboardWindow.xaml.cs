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
            leaderboard = new ObservableCollection<string>();

            InitializeComponent();
            this._importedData = _importedData;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _timer.Tick += UpdateLeaderboard;
            _timer.Start();
        }

        private ObservableCollection<string> leaderboard;
        public ObservableCollection<string> Leaderboard
        {
            get { return leaderboard; }
            set { leaderboard = value; }
        }

        private void UpdateLeaderboard(object? sender, EventArgs e)
        {
            leaderboard.Clear();
            _importedData.GetPlayerID();

            foreach (rF2VehicleScoring vehicleScoring in _importedData._scoring.mVehicles)
            {
                if (Encoding.ASCII.GetString(vehicleScoring.mVehicleClass).TrimEnd('\0') == Encoding.ASCII.GetString(_importedData.mVehicleClass).TrimEnd('\0'))
                {
                    leaderboard.Add(Encoding.ASCII.GetString(vehicleScoring.mDriverName).TrimEnd('\0'));
                }
                Console.WriteLine($"{vehicleScoring.mVehicleClass}, {_importedData.mVehicleClass}");
            }

        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
