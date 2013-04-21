using System.Xml;
using System.IO;
using System.Diagnostics;

namespace FaceRecognitionClient
{
    /*
     * v tejto triede by mal byt kod ktory sa vykonava pocas treningu na pozadi
     * jej zmysel je v tom ze odlahci triedu BackgroundWorkerControl od dalsieho kodu
     * v podstate jej public metody spusta BackgroundWorkerControl
     */

    class Trening
    {
        private string _biosandboxHome; // Enviroment variable BIOSANDBOX_HOME
        private Process _pBiosandbox = null;   // proces biosandboxu
        private string _tmpCaptureXml;  // docasny xml subor capture.xml


        public Trening(string biosandboxHome)
        {
            _biosandboxHome = biosandboxHome;
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

            return "";
        }

        public void KillBiosandboxProcess()
        {
            if (_biosandboxHome != null && _tmpCaptureXml != null && _pBiosandbox != null)
            {
                _pBiosandbox.Kill();
                _pBiosandbox.Close();
                File.Delete(string.Format("{0}/{1}", _biosandboxHome, _tmpCaptureXml));
            }
        }

        private void CreateTmpCaptureXml(out string tmpCaptureXml, out string tmpSavePath)
        {
            tmpCaptureXml = string.Format("capture_{0}.xml", Tools.RandomString(6));
            tmpSavePath = string.Format("faces{0}", Tools.RandomString(6));

            XmlDocument xmlTemporary = new XmlDocument();
            xmlTemporary.Load(string.Format("{0}/capture.xml", _biosandboxHome));

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
            xmlTemporary.Save(string.Format("{0}/{1}", _biosandboxHome, tmpCaptureXml));
            // vytvorenie adresaru, tiez treba potom zmazt
            Directory.CreateDirectory(string.Format("{0}/{1}", _biosandboxHome, tmpSavePath));
        }
    }
}
