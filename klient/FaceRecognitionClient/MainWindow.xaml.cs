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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.ComponentModel;
using FaceRecognitionClient.Threading;
using System.Threading;

namespace FaceRecognitionClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartAsyncOperation()
        {
            button1.IsEnabled = false;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            radioButton1.IsEnabled = false;
            radioButton2.IsEnabled = false;

            progressBar1.IsIndeterminate = true;
        }

        private void EndAsyncOperation()
        {
            button1.IsEnabled = true;
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            radioButton1.IsEnabled = true;
            radioButton2.IsEnabled = true;
          
            progressBar1.IsIndeterminate = false;
        }

        // upload osob TODO refaktorizacia
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            StartAsyncOperation();
            BackgroundWorkerControl bc = new BackgroundWorkerControl(BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"));
            bc.AsyncUploadPersons("persones.xml", "db.xml");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            StartAsyncOperation();
            BackgroundWorkerControl bc = new BackgroundWorkerControl(BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"));

            // s UDF
            if ((bool)radioButton1.IsChecked)
            {
                bc.AsyncComparePersonsWithUDF("test.xml");
            }
        }

        public void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 
            if (e.Error != null)
            {
                textBox1.Text = e.Error.Message;
            }
            else if (e.Cancelled) // sem by nikdy nemal vbehnut
            {
                // Next, handle the case where the user canceled  
                // the operation. 
                // Note that due to a race condition in  
                // the DoWork event handler, the Cancelled 
                // flag may not have been set, even though 
                // CancelAsync was called.
                textBox1.Text = "Canceled";
            }
            else
            {
                // Finally, handle the case where the operation  
                // succeeded.
                textBox1.Text = e.Result.ToString();
            }
            EndAsyncOperation();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //test 
            StartAsyncOperation();
            TrainingWindow t = new TrainingWindow();
            t.Show(this);

            //StartAsyncOperation();
            //BackgroundWorkerControl bc = new BackgroundWorkerControl(BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"));
            //bc.AsyncTrening();
        }

    }
}
