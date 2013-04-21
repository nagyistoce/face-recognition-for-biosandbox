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
using System.ComponentModel;

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

            this.Top = 10;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 10;
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
            
            _bc = new BackgroundWorkerControl(this.BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"));
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
                this.EndProcess();
            }
        }

        private void EndProcess()
        {
            _bc.BeginTreningEnd();
            _parent.EndAsyncOperation();
            _dispatcherTimer.Stop();
            this.Close();        
        }

        private void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 
            if (e.Error != null)
            {
                _parent.textBox1.Text += "\nLog: " + e.Error.Message;
                this.EndProcess();
            }
            else if (e.Cancelled) // sem by nikdy nemal vbehnut
            {
                // Next, handle the case where the user canceled  
                // the operation. 
                // Note that due to a race condition in  
                // the DoWork event handler, the Cancelled 
                // flag may not have been set, even though 
                // CancelAsync was called.
                _parent.textBox1.Text += "\nLog: " + "Canceled";
                this.EndProcess();
            }
            else
            {
                // Finally, handle the case where the operation  
                // succeeded.
                _parent.textBox1.Text = e.Result.ToString();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.EndProcess();
        }
    }
}
