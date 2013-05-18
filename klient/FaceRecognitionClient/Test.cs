using System.Xml;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections;
using System.Threading;
using System.Windows.Controls;

namespace FaceRecognitionClient
{
    /*
     * v tejto triede by mal byt kod ktory sa vykonava pocas testovania na pozadi
     * jej zmysel je v tom ze odlahci triedu BackgroundWorkerControl od dalsieho kodu
     * v podstate jej public metody spusta BackgroundWorkerControl
     */

    class Test
    {
        private string _biosandboxHome;         // Enviroment variable BIOSANDBOX_HOME
        private Process _pBiosandboxCapture = null;    // proces biosandboxu
        private Process _pBiosandboxTrain = null;
        private string _tmpCaptureXml;          // docasny xml subor capture.xml
        private string _tmpSavePath;            // docasna zlozka faces, snimky trenovacej osoby

        // upload snimkov
        private string _tmpTrainTxt;        // docasny txt subor train.txt, obsahuje cestu k snimkom
        private string _tmpTrainXml;        // docasny xml subor train.xml, konfigurak pre biosandbox.exe
        private string _tmpDbXml;           // docasny xml subor db.xml, vystupne trenovacie vektory
        private DateTime[] _times;

        //private string _trainName;
        private TextBox _textBox;

        private struct SFile
        {
            public string filePath;
            public DateTime fileCreated;
        }

        public Test(string biosandboxHome, TextBox textbox)
        {
            _biosandboxHome = biosandboxHome;
            _textBox = textbox;
        }

        public string DoTest()
        {
            string tmpCaptureXml, tmpSavePath;
            this.CreateTmpCaptureXml(out tmpCaptureXml, out tmpSavePath);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = _biosandboxHome;

            startInfo.FileName = "biosandbox.exe";
            startInfo.Arguments = tmpCaptureXml;

            //Vista or higher check
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                startInfo.Verb = "runas";
            }
            _pBiosandboxCapture = Process.Start(startInfo);

            _tmpCaptureXml = tmpCaptureXml;
            _tmpSavePath = tmpSavePath;
            return "Start test";
        }

        public string DoUpload()
        {
            if (_tmpSavePath.Length < 1)
                return "ERROR: Temporary save path does not exist.";

            //if (_times == null)
            //    return "ERROR: Capture times are empty.";
            _times = new DateTime[1];
            _times[0] = DateTime.Now;

            _tmpTrainTxt = CreateTrainTxt(_times);
            CreateTrainXml(out _tmpTrainXml, out _tmpDbXml);

            // spustenie trenovania vybranych obrazkov
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = _biosandboxHome;

            startInfo.FileName = "biosandbox.exe";
            startInfo.Arguments = _tmpTrainXml;

            //Vista or higher check
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                startInfo.Verb = "runas";
            }
            _pBiosandboxTrain = Process.Start(startInfo);

            while (_pBiosandboxTrain.HasExited == false)
            {
                Thread.Sleep(250);
            }

            return "Upload " + FeaturesUploadRequest();
        }

        private string FeaturesUploadRequest()
        {
            int vectorsCount;
            // nacitanie vektorov
            XmlDocument xmlVectors = new XmlDocument();
            xmlVectors.Load(string.Format("{0}/{1}", _biosandboxHome, _tmpDbXml));

            // nove xml pre request
            XmlDocument request = new XmlDocument();
            XmlNode requestRoot = request.AppendChild(request.CreateElement("Test"));

            // pridanie elementu datas
            // minus 1 preto, lebo jeden element data je pouzity v inom kontexte ako data trenovacieho vektoru
            vectorsCount = xmlVectors.GetElementsByTagName("data").Count - 1;
       
            // pridanie vektorov
            for (int i = 0; i < vectorsCount; i++)
            {
                XmlNode vector = xmlVectors.GetElementsByTagName(string.Format("features_{0}", i)).Item(0);
                string personVector = vector.LastChild.InnerText;

                // pridanie vektoru
                XmlNode requestData = requestRoot.AppendChild(request.CreateElement("Vector"));
                requestData.InnerText = personVector;
                XmlAttribute id = requestData.Attributes.Append(request.CreateAttribute("id"));
                id.InnerText = i.ToString();
            }

            string requestString = request.OuterXml;

            ServiceReference3.recognitionwsdlPortType client = new ServiceReference3.recognitionwsdlPortTypeClient();
            //_textBox.Text += Tools.GetLogMessage("Start sending content to server"); 
            return client.udfRecognitionTest2("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + requestString);
        }

        private void CreateTrainXml(out string tmpTrainXml, out string tmpDbXml)
        {
            tmpTrainXml = string.Format("train{0}.xml", Tools.RandomString(6));
            tmpDbXml = string.Format("db{0}.xml", Tools.RandomString(6));

            XmlDocument xmlTemporary = new XmlDocument();
            xmlTemporary.Load(string.Format("{0}\\train.xml", _biosandboxHome));

            XmlNodeList input = xmlTemporary.GetElementsByTagName("Input");
            XmlNodeList postprocessing = xmlTemporary.GetElementsByTagName("Postprocessing");

            XmlNodeList inputModule = input[0].ChildNodes;
            XmlNodeList postprocessingModule = postprocessing[0].ChildNodes;

            foreach (XmlAttribute a in inputModule[0].Attributes)
            {
                if (a.Name == "file")
                    a.Value = _tmpTrainTxt;
            }

            foreach (XmlAttribute a in postprocessingModule[0].Attributes)
            {
                if (a.Name == "database")
                    a.Value = tmpDbXml;
            }

            // POZOR POZOR
            // ulozenie docasnych suborov, pozor treba neskor zmazat
            xmlTemporary.Save(string.Format("{0}\\{1}", _biosandboxHome, tmpTrainXml));
        }

        private string CreateTrainTxt(DateTime[] times)
        {
            ArrayList list = new ArrayList();
            foreach (string path in Directory.GetFiles(string.Format("{0}\\{1}", _biosandboxHome, _tmpSavePath)))
            {
                DateTime fileCreated = File.GetCreationTime(path);
                SFile sfile = new SFile();
                sfile.filePath = path;
                sfile.fileCreated = fileCreated;

                list.Add(sfile);
            }

            ArrayList nearestFiles = FindNearestFiles(times, list, 1);

            string traintxt = string.Format("train{0}.txt", Tools.RandomString(6));
            string trainpath = string.Format("{0}\\{1}", _biosandboxHome, traintxt);
            // Create a file to write to. 
            if (!File.Exists(trainpath))
            {
                using (StreamWriter sw = File.CreateText(trainpath))
                {
                    for (int i = 0; i < nearestFiles.Count; i++)
                    {
                        SFile nearest = (SFile)nearestFiles[i];
                        if (i == nearestFiles.Count - 1)
                            sw.Write(nearest.filePath);
                        else
                            sw.WriteLine(nearest.filePath);
                    }
                    sw.Close();
                }
            }

            return traintxt;
        }

        private ArrayList FindNearestFiles(DateTime[] times, ArrayList allFiles, int n)
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < n; i++)
            {
                foreach (DateTime time in times)
                {
                    TimeSpan diff = TimeSpan.MaxValue;
                    int minindex = 0;

                    for (int j = 0; j < allFiles.Count; j++)
                    {
                        SFile sfile = (SFile)allFiles[j];
                        TimeSpan tmp = sfile.fileCreated - time;
                        tmp = tmp.Duration();

                        if (tmp < diff)
                        {
                            diff = tmp;
                            minindex = j;
                        }
                    }

                    result.Add((SFile)allFiles[minindex]);
                    allFiles.RemoveAt(minindex);
                }
            }

            return result;
        }

        public void KillBiosandboxProcess()
        {
            if (_biosandboxHome != null && _tmpCaptureXml != null && _pBiosandboxCapture != null && _pBiosandboxCapture.HasExited == false)
            {
                try
                {
                    _pBiosandboxCapture.Kill();
                    _pBiosandboxCapture.Close();
                    _pBiosandboxCapture = null;
                }
                catch (Exception exc)
                {
                    _textBox.Text += Tools.GetLogMessage(exc.Message);
                }
            }

            if (_biosandboxHome != null && _tmpCaptureXml != null && _pBiosandboxTrain != null && _pBiosandboxTrain.HasExited == false)
            {
                try
                {
                    _pBiosandboxTrain.Kill();
                    _pBiosandboxTrain.Close();
                    _pBiosandboxTrain = null;
                }
                catch (Exception exc)
                {
                    _textBox.Text += Tools.GetLogMessage(exc.Message);
                }
            }
        }

        public void RemoveTemporaryFiles()
        {
            try
            {
                File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpCaptureXml));
                File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpDbXml));
                File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpTrainTxt));
                File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpTrainXml));
                Directory.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpSavePath), true);
            }
            catch (Exception e)
            {
                _textBox.Text += Tools.GetErrorMessage(e.Message);
            }
        }

        private void CreateTmpCaptureXml(out string tmpCaptureXml, out string tmpSavePath)
        {
            tmpCaptureXml = string.Format("capture_{0}.xml", Tools.RandomString(6));
            tmpSavePath = string.Format("faces{0}", Tools.RandomString(6));

            XmlDocument xmlTemporary = new XmlDocument();
            xmlTemporary.Load(string.Format("{0}\\capture.xml", _biosandboxHome));

            XmlNodeList nodes = xmlTemporary.GetElementsByTagName("Finishing");
            XmlNodeList childnodes = nodes[0].ChildNodes;

            foreach (XmlNode node in childnodes)
            {
                if (node.Name == "Module")
                {
                    XmlAttributeCollection atributes = node.Attributes;

                    foreach (XmlAttribute a in atributes)
                    {
                        if (a.Name == "savePath")
                        {
                            a.Value = tmpSavePath + "/";
                        }
                    }
                }
            }

            // POZOR POZOR
            // ulozenie docasnych suborov, pozor treba neskor zmazat
            xmlTemporary.Save(string.Format("{0}\\{1}", _biosandboxHome, tmpCaptureXml));
            // vytvorenie adresaru, tiez treba potom zmazt
            Directory.CreateDirectory(string.Format("{0}\\{1}", _biosandboxHome, tmpSavePath));
        }
    }
}
