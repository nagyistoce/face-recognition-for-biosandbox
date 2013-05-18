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
        private TrainingWindow _train = null;
        private TestingWindow _test = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartAsyncOperation()
        {
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            radioButton1.IsEnabled = false;
            radioButton2.IsEnabled = false;

            progressBar1.IsIndeterminate = true;
        }

        public void EndAsyncOperation()
        {
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            radioButton1.IsEnabled = true;
            radioButton2.IsEnabled = true;
          
            progressBar1.IsIndeterminate = false;
        }
// 
//         // upload osob
//         private void button1_Click(object sender, RoutedEventArgs e)
//         {
//             StartAsyncOperation();
//             BackgroundWorkerControl bc = new BackgroundWorkerControl(BackgroundWorkerCompleted, Environment.ExpandEnvironmentVariables("%BIOSANDBOX_HOME%"), this.textBox1);
//             bc.AsyncUploadPersons("persones.xml", "db.xml");
//         }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            StartAsyncOperation();

            _test = new TestingWindow();
            _test.Show(this, (this.radioButton1.IsChecked == true));
        }

//         private void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//         {
//             // First, handle the case where an exception was thrown. 
//             if (e.Error != null)
//             {
//                 textBox1.Text += Tools.GetErrorMessage(e.Error.Message);
//             }
//             else if (e.Cancelled) // sem by nikdy nemal vbehnut
//             {
//                 // Next, handle the case where the user canceled  
//                 // the operation. 
//                 // Note that due to a race condition in  
//                 // the DoWork event handler, the Cancelled 
//                 // flag may not have been set, even though 
//                 // CancelAsync was called.
//                 textBox1.Text += Tools.GetLogMessage("Canceled");
//             }
//             else
//             {
//                 // Finally, handle the case where the operation  
//                 // succeeded.
//                 textBox1.Text += Tools.GetLogMessage(e.Result.ToString());
//             }
//             EndAsyncOperation();
//         }

        // trenovacie vzorky
        private void button3_Click(object sender, RoutedEventArgs e)
        {        
            StartAsyncOperation();

            if (textBox2.Text.Length < 1)
            {
                textBox1.Text += Tools.GetErrorMessage("Pred trenovanim je potrebne zadat meno osoby.");
                EndAsyncOperation();
                return;
            }

            _train = new TrainingWindow();
            _train.Show(this, textBox2.Text);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_train != null)
            {
                _train.Close();
            }

            if (_test != null)
            {
                _test.Close();
            }
        }
    }
}
