using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using FaceRecognitionClient.Threading;
using System.Threading;

namespace FaceRecognitionClient
{
    /// <summary>
    /// Interaction logic for TrainingWindow.xaml
    /// </summary>
    public partial class TrainingWindow : Window
    {
        private MainWindow _parent;
        private BackgroundWorkerControl _bc;
        private DispatcherTimer _dispatcherTimer;

        public TrainingWindow()
        {
            InitializeComponent();

            this.Top = 0;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
        }

        public void Show(MainWindow parent)
        {
            _parent = parent;
            this.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            label2.Content = 0;
            
            _dispatcherTimer.Start();

            
            _bc = new BackgroundWorkerControl(_parent.BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"));
            _bc.AsyncTrening();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            
            int i = Int32.Parse(label2.Content.ToString());
            if(i==0)
                Thread.Sleep(1000);
            i++;
            label2.Content = i;

            if (i < 7)
                label1.Content = "Pozeraj sa rovno";
            else if (i < 11)
                label1.Content = "Natoč sa doľava";
            else if (i < 15)
                label1.Content = "Natoč sa doprava";
            else
            {
                //_parent.textBox1.Text += i;
                _bc.BeginTreningEnd();
                _dispatcherTimer.Stop();
                _parent.EndAsyncOperation();
                this.Close();
            }
            
        }
    }
}
