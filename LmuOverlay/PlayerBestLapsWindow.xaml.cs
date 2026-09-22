using System;
using System.Collections.Generic;
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
    /// Interaction logic for PlayerBestLapsWindow.xaml
    /// </summary>
    public partial class PlayerBestLapsWindow : Window
    {
        private DispatcherTimer _timer;

        public PlayerBestLapsWindow(rF2ImportedData _importedData)
        {
            InitializeComponent();

            _timer = new DispatcherTimer();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += UpdateBestLaps;
            _timer.Start();
        }

        private void UpdateBestLaps(object? sender, EventArgs e)
        {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
