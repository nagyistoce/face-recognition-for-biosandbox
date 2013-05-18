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
using FaceRecognitionClient.Threading;
using System.ComponentModel;

namespace FaceRecognitionClient
{
    /// <summary>
    /// Interaction logic for TestingWindow.xaml
    /// </summary>
    public partial class TestingWindow : Window
    {
        private MainWindow _parent;
        private BackgroundWorkerControl _bc;
        private bool _isUdfFca1Enabled;

        public TestingWindow()
        {
            InitializeComponent();

            this.Top = 10;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 10;
        }

        public void Show(MainWindow parent, bool isUdfFca1Enabled)
        {
            _parent = parent;
            _isUdfFca1Enabled = isUdfFca1Enabled;
            this.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _bc = new BackgroundWorkerControl(this.BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"), _parent.textBox1);
            _bc.AsyncTest();
        }

        private void EndProcess()
        {
            _bc.BeginTestEnd();
            _parent.EndAsyncOperation();
            this.Close();
        }

        private void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 
            if (e.Error != null)
            {
                _parent.textBox1.Text += Tools.GetErrorMessage(e.Error.Message);
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
                _parent.textBox1.Text += Tools.GetLogMessage("Canceled");
                this.EndProcess();
            }
            else
            {
                // Finally, handle the case where the operation  
                // succeeded.
                _parent.textBox1.Text += Tools.GetLogMessage(e.Result.ToString());

                if (e.Result.ToString().StartsWith("Upload"))
                {
                    _parent.textBox1.Text += Tools.GetLogMessage("Start sending content to server");
                    _bc.AsyncTestUpload(_isUdfFca1Enabled);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.EndProcess();
        }

        // start
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.button2.IsEnabled = false;
            _parent.textBox1.Text += Tools.GetLogMessage("Start sending content to server");
            _bc.AsyncTestUpload(_isUdfFca1Enabled);
        }

        // cancel
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.EndProcess();
        }

    }
}
