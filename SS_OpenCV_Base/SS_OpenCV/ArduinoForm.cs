using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Globalization;

namespace SS_OpenCV
{
    public partial class ArduinoForm : Form
    {

        Image<Bgr, Byte> img = null; // working image
        string title_bak = "";

        public ArduinoForm()
        {
            InitializeComponent();
            serialPort1.BaudRate = 115200;
            serialPort1.PortName = "COM5";
        }

        /// <summary>
        /// Opens a new image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                img = new Image<Bgr, byte>(openFileDialog1.FileName);
                Text = title_bak + " [" +
                        openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1) +
                        "]";
                ImageViewer.Image = img;
                ImageViewer.Refresh();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageViewer.Image.Save(saveFileDialog1.FileName);
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try{
                Image<Bgr, Byte> imgEcra = null; // working image
                imgEcra = img.Copy();

                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadLine();

                
                Console.WriteLine("Data Received:");
                Console.Write(indata);

                string[] partes = indata.Split('/');

                Console.WriteLine("Temperatura:");
                Console.WriteLine(partes[0]);
                Console.WriteLine("Luz:");
                Console.WriteLine(partes[1]);

                float temperatura, luz;
                temperatura = float.Parse(partes[0], CultureInfo.InvariantCulture);
                luz = float.Parse(partes[1], CultureInfo.InvariantCulture);
                Console.WriteLine("BOOM");

                int luzTraduzidaParaFuncao = (int)((luz / 20000) * 255);
                float temperaturaTraduzidaParaFuncao = (temperatura / 65) * 3;


                ImageClass.BrightContrast(imgEcra, luzTraduzidaParaFuncao, temperaturaTraduzidaParaFuncao);
                ImageViewer.Image = imgEcra;
                ImageViewer.Refresh();

            } catch(Exception exep)
            {

            }         
        }

        private void serialPortToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            InputBox formCOM = new InputBox("COM Number:");
            formCOM.ShowDialog();
            string comNum = formCOM.ValueTextBox.Text;
            formCOM.Hide();

            serialPort1.PortName = "COM" + comNum;
        }

        private void connectDisconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            if (serialPort1.IsOpen) 
            {
                serialPort1.Close();
            } else
            {
                serialPort1.Open();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
