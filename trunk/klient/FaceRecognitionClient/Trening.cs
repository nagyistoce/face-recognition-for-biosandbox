using System.Xml;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections;
using System.Threading;

namespace FaceRecognitionClient
{
    /*
     * v tejto triede by mal byt kod ktory sa vykonava pocas treningu na pozadi
     * jej zmysel je v tom ze odlahci triedu BackgroundWorkerControl od dalsieho kodu
     * v podstate jej public metody spusta BackgroundWorkerControl
     */

    class Trening
    {
        private string _biosandboxHome;         // Enviroment variable BIOSANDBOX_HOME
        private Process _pBiosandbox = null;    // proces biosandboxu
        private string _tmpCaptureXml;          // docasny xml subor capture.xml
        private string _tmpSavePath;            // docasna zlozka faces, snimky trenovacej osoby

        // upload snimkov
        private string _tmpTrainTxt;        // docasny txt subor train.txt, obsahuje cestu k snimkom
        private string _tmpTrainXml;        // docasny xml subor train.xml, konfigurak pre biosandbox.exe
        private string _tmpDbXml;           // docasny xml subor db.xml, vystupne trenovacie vektory
        private DateTime[] _times = null;
        private string _trainName;

        private struct SFile
        {
            public string filePath;
            public DateTime fileCreated;
        }


        public Trening(string biosandboxHome)
        {
            _biosandboxHome = biosandboxHome;
        }

        public DateTime[] Times
        {
            set
            {
                _times = value;
            }
        }

        public string TrainName
        {
            set
            {
                _trainName = value;
            }
        }

        public string DoTrening()
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
            _pBiosandbox = Process.Start(startInfo);

            _tmpCaptureXml = tmpCaptureXml;
            _tmpSavePath = tmpSavePath;
            return "Start trening";
        }

        public string DoUpload()
        {
            if (_tmpSavePath.Length < 1)
                return "ERROR: Temporary save path does not exist.";

            if (_times == null)
                return "ERROR: Capture times are empty.";

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
            _pBiosandbox = Process.Start(startInfo);

            while (_pBiosandbox.HasExited == false)
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
            XmlNode requestRoot = request.AppendChild(request.CreateElement("Upload"));

            //  sparsovanie a vytvorenie noveho pre jedneho cloveka s vektormi
            XmlNode requestPerson = requestRoot.AppendChild(request.CreateElement("Person"));

            // pridanie elementu datas
            XmlNode requestDatas = requestPerson.AppendChild(request.CreateElement("Datas"));
            XmlAttribute requestDatasSize = requestDatas.Attributes.Append(request.CreateAttribute("size"));
            vectorsCount = xmlVectors.GetElementsByTagName("data").Count - 1;
            requestDatasSize.InnerText = vectorsCount.ToString();    // minus 1 preto, lebo jeden element data je pouzity v inom kontexte ako data trenovacieho vektoru

            // pridanie noveho mena
            XmlNode requestName = requestPerson.AppendChild(request.CreateElement("Name"));
            XmlAttribute requestNameValue = requestName.Attributes.Append(request.CreateAttribute("value"));
            requestNameValue.InnerText = _trainName;
     
            // pridanie vektorov
            for (int i = 0; i < vectorsCount; i++)
            {
                XmlNode vector = xmlVectors.GetElementsByTagName(string.Format("features_{0}", i)).Item(0);
                string personVector = vector.LastChild.InnerText;

                // pridanie vektoru
                XmlNode requestData = requestDatas.AppendChild(request.CreateElement("Data"));
                requestData.InnerText = personVector;
            }

            string requestString = request.OuterXml;

            ServiceReference2.uploadwsdlPortTypeClient client = new ServiceReference2.uploadwsdlPortTypeClient();
            return client.uploadAndTest("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + requestString);
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
            if (_biosandboxHome != null && _tmpCaptureXml != null && _pBiosandbox != null)
            {
                _pBiosandbox.Kill();
                _pBiosandbox.Close();
                _pBiosandbox = null;
            }
        }

        public void RemoveTemporaryFiles()
        {
            File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpCaptureXml));
            File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpDbXml));
            File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpTrainTxt));
            File.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpTrainXml));
            Directory.Delete(string.Format("{0}\\{1}", _biosandboxHome, _tmpSavePath), true);
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
