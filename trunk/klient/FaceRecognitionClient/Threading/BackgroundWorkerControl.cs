using System;
using System.ComponentModel;
using System.Xml;
using System.Windows.Controls;

namespace FaceRecognitionClient.Threading
{
    public delegate void DBackgroundWorkerCallback(object sender, RunWorkerCompletedEventArgs e);

    class BackgroundWorkerControl
    {
        private BackgroundWorker _worker;
        private DBackgroundWorkerCallback _callback;

        private string _biosandboxHome; // Enviroment variable BIOSANDBOX_HOME
        private string _filePersones;   // persones.xml - nas format pre mena osob a ich trenovacich vektorov
        private string _fileDb;         // db.xml - biosandbox
        private string _fileTest;       // test.xml - nas format pre testovaci vektor

        private Trening _trening = null;       // automaticke trenovanie vektorov
        private Test _test = null;             // automaticke testovanie vektorov

        private TextBox _textbox;

        public BackgroundWorkerControl(DBackgroundWorkerCallback callback, string biosandboxHome, TextBox textBox)
        {
            _biosandboxHome = biosandboxHome;
            _worker = new BackgroundWorker();
            _callback = callback;

            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_callback);

            _textbox = textBox;
        }

        //
        // Manualny upload osoby do databazy
        //
        private string UploadPersons()
        {
            // nacitanie osob
            XmlDocument xmlPersones = new XmlDocument(); //* create an xml document object.
            xmlPersones.Load(string.Format("{0}/{1}", _biosandboxHome, _filePersones)); //* load the XML document from the specified file.

            // nacitanie vektorov
            XmlDocument xmlVectors = new XmlDocument();
            xmlVectors.Load(string.Format("{0}/{1}", _biosandboxHome, _fileDb));

            // nove xml pre request
            XmlDocument request = new XmlDocument();
            XmlNode requestRoot = request.AppendChild(request.CreateElement("Upload"));


            //  sparsovanie a vytvorenie noveho xml s menami a vektormi
            XmlNodeList persones = xmlPersones.GetElementsByTagName("Person");
            foreach (XmlNode person in persones)
            {
                XmlNode requestPerson = requestRoot.AppendChild(request.CreateElement("Person"));

                // pridanie elementu datas
                XmlNode requestDatas = requestPerson.AppendChild(request.CreateElement("Datas"));
                XmlAttribute requestDatasSize = requestDatas.Attributes.Append(request.CreateAttribute("size"));
                requestDatasSize.InnerText = (person.ChildNodes.Count - 1).ToString();    // 1 je meno a zvysne su vektory

                foreach (XmlNode child in person.ChildNodes)
                {
                    if (child.Name == "Name")
                    {
                        string personName = child.Attributes["value"].Value;

                        // pridanie noveho mena
                        XmlNode requestName = requestPerson.AppendChild(request.CreateElement("Name"));
                        XmlAttribute requestNameValue = requestName.Attributes.Append(request.CreateAttribute("value"));
                        requestNameValue.InnerText = personName;
                    }
                    if (child.Name == "opencv-matrix")
                    {
                        string opnecvId = child.Attributes["id"].Value;

                        XmlNode vector = xmlVectors.GetElementsByTagName(opnecvId).Item(0);
                        string personVector = vector.LastChild.InnerText;

                        // pridanie vektoru
                        // vyhodit ine znaky, a nechat len medzery
                        XmlNode requestData = requestDatas.AppendChild(request.CreateElement("Data"));
                        requestData.InnerText = personVector;
                    }
                }

            }
            string requestString = request.OuterXml;

            ServiceReference2.uploadwsdlPortTypeClient client = new ServiceReference2.uploadwsdlPortTypeClient();
            return client.uploadAndTest("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + requestString);
        }

        private void UploadPersonsDoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation 
            // to the Result property of the DoWorkEventArgs 
            // object. This is will be available to the  
            // RunWorkerCompleted eventhandler.
            e.Result = UploadPersons();
        }

        public void AsyncUploadPersons(string filePersones, string fileDb)
        {
            _filePersones = filePersones;
            _fileDb = fileDb;
            _worker.DoWork += new DoWorkEventHandler(UploadPersonsDoWork);
            _worker.RunWorkerAsync();
        }

        //
        //  Porovnanie testovacich vektorov pomocou UDF
        //
        private string ComparePersonsWithUDF()
        {
            XmlDocument xmlPersones = new XmlDocument(); //* create an xml document object.
            xmlPersones.Load(string.Format("{0}/{1}", _biosandboxHome, _fileTest)); //* load the XML document from the specified file.

            //textBox1.Text = xmlPersones.OuterXml;

            try
            {
                ServiceReference3.recognitionwsdlPortTypeClient client = new ServiceReference3.recognitionwsdlPortTypeClient();
                return client.udfRecognitionTest(xmlPersones.OuterXml);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void ComparePersonsWithUDFDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            e.Result = ComparePersonsWithUDF();
        }

        public void AsyncComparePersonsWithUDF(string fileTest)
        {
            _fileTest = fileTest;
            _worker.DoWork += new DoWorkEventHandler(ComparePersonsWithUDFDoWork);
            _worker.RunWorkerAsync();
        }

        //
        //  Spustenie automatizovaneho treningu
        //
        private void TreningDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            _trening = new Trening(_biosandboxHome, _textbox);

            e.Result = _trening.DoTrening();
        }

        public void AsyncTrening()
        {
            _worker.DoWork += new DoWorkEventHandler(TreningDoWork);
            _worker.RunWorkerAsync();
        }

        private void TreningUploadDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //_trening = new Trening(_biosandboxHome);

            e.Result = _trening.DoUpload();
        }

        public void AsyncTreningUpload(DateTime[] times, string name)
        {
            _worker.DoWork += new DoWorkEventHandler(TreningUploadDoWork);
            _trening.Times = times;
            _trening.TrainName = name;
            _worker.RunWorkerAsync();
        }

        public void BeginTreningEnd()
        {
            if (_trening != null)
            {
                _trening.KillBiosandboxProcess();
                _worker.DoWork -= new DoWorkEventHandler(TreningDoWork);
            }
        }

        public void BeginTreningUploadEnd()
        {
            if (_trening != null)
            {
                _worker.DoWork -= new DoWorkEventHandler(TreningUploadDoWork);
                _trening.RemoveTemporaryFiles();
            }
        }

        //
        //  Spustenie automatizovaneho testovania
        //
        private void TestDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            _test = new Test(_biosandboxHome, _textbox);

            e.Result = _test.DoTest();
        }

        public void AsyncTest()
        {
            _worker.DoWork += new DoWorkEventHandler(TestDoWork);
            _worker.RunWorkerAsync();
        }

        private void TestUploadDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            e.Result = _test.DoUpload();
        }

        public void AsyncTestUpload()
        {
            _worker.DoWork -= new DoWorkEventHandler(TestDoWork);
            _worker.DoWork += new DoWorkEventHandler(TestUploadDoWork);
            
            _worker.RunWorkerAsync();
        }

        public void BeginTestEnd()
        {
            if (_test != null)
            {
                _test.KillBiosandboxProcess();
                _worker.DoWork -= new DoWorkEventHandler(TestDoWork);
                _worker.DoWork -= new DoWorkEventHandler(TestUploadDoWork);
                _test.RemoveTemporaryFiles();
                _test = null;
            }
        }
    }
}
