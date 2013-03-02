using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Xml;

namespace FaceRecognitionClient.Threading
{
    public delegate void DBackgroundWorkerCallback(object sender, RunWorkerCompletedEventArgs e);
    class BackgroundWorkerControl
    {
        private BackgroundWorker _worker;
        private DBackgroundWorkerCallback _callback;

        public BackgroundWorkerControl(DBackgroundWorkerCallback callback)
        {
            _worker = new BackgroundWorker();
            _callback = callback;

            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_callback);
        }

        private string UploadPersons()
        {
            // nacitanie osob
            XmlDocument xmlPersones = new XmlDocument(); //* create an xml document object.
            xmlPersones.Load(@"C:\Users\Tomas\Desktop\Timovy projekt\Release32\Release32\persones.xml"); //* load the XML document from the specified file.

            // nacitanie vektorov
            XmlDocument xmlVectors = new XmlDocument();
            xmlVectors.Load(@"C:\Users\Tomas\Desktop\Timovy projekt\Release32\Release32\db.xml");

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

        public void AsyncUploadPersons()
        {
            _worker.DoWork += new DoWorkEventHandler(UploadPersonsDoWork);
            
            _worker.RunWorkerAsync();
        }

        private string ComparePersonsWithUDF()
        {
            XmlDocument xmlPersones = new XmlDocument(); //* create an xml document object.
            xmlPersones.Load(@"C:\Users\Tomas\Desktop\Timovy projekt\Release32\Release32\test.xml"); //* load the XML document from the specified file.

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

        public void AsyncComparePersonsWithUDF()
        {
            _worker.DoWork += new DoWorkEventHandler(ComparePersonsWithUDFDoWork);

            _worker.RunWorkerAsync();
        }
    }
}
