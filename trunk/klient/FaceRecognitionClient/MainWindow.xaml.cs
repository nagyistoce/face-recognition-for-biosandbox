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

        // skuska
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ServiceReference1.hellowsdlPortTypeClient client = new ServiceReference1.hellowsdlPortTypeClient();
            string response = client.hello("Tomas");

            this.textBox1.Text = response;
        }

        // upload osob TODO refaktorizacia
        private void button1_Click(object sender, RoutedEventArgs e)
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
            textBox1.Text = requestString;

            ServiceReference2.uploadwsdlPortTypeClient client = new ServiceReference2.uploadwsdlPortTypeClient();
            string response = client.uploadAndTest("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + requestString);

            textBox1.Text = response;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // s UDF
            if ((bool)radioButton1.IsChecked)
            {
                XmlDocument xmlPersones = new XmlDocument(); //* create an xml document object.
                xmlPersones.Load(@"C:\Users\Tomas\Desktop\Timovy projekt\Release32\Release32\test.xml"); //* load the XML document from the specified file.

                //textBox1.Text = xmlPersones.OuterXml;

                try
                {
                    ServiceReference3.recognitionwsdlPortTypeClient client = new ServiceReference3.recognitionwsdlPortTypeClient();
                    string response = client.udfRecognitionTest(xmlPersones.OuterXml);

                    textBox1.Text = response;
                }
                catch (Exception ex)
                {
                    textBox1.Text = ex.Message;
                }
            }
        }
    }
}
