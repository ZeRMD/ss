using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using Emgu.CV.Structure;
using Emgu.CV;

namespace SS_OpenCV
{
    public partial class HistogramForm : Form
    {
        public int[] hgray;
        public int[,] hrgb;
        public int[,] hcompleto;

        public HistogramForm(int[] hgray1, int[,] hrgb1, int[,] hcompleto1)
        {
            InitializeComponent();
            hgray = hgray1;
            hrgb = hrgb1;
            hcompleto = hcompleto1;
            comboBox1.SelectedItem = "Histograma Gray";
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarComboBox();
        }

        private void AtualizarComboBox()
        {
            switch (comboBox1.SelectedItem)
            {
                case "Histograma Gray":
                    chart1.Series.Clear();
                    Series seriesGray = new Series("Gray")
                    {
                        Color = Color.Gray,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    chart1.Series.Add(seriesGray);

                    DataPointCollection listGray = seriesGray.Points;
                    for (int i = 0; i < hgray.Length; i++)
                    {
                        listGray.AddXY(i, hgray[i]);
                    }
                    chart1.ResumeLayout();
                    break;
                case "Histograma RGB":
                    chart1.Series.Clear();
                    Series seriesBlue = new Series("Blue")
                    {
                        Color = Color.Blue,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    Series seriesGreen = new Series("Green")
                    {
                        Color = Color.Green,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    Series seriesRed = new Series("Red")
                    {
                        Color = Color.Red,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    chart1.Series.Add(seriesBlue);
                    chart1.Series.Add(seriesGreen);
                    chart1.Series.Add(seriesRed);
                    DataPointCollection listBlue = seriesBlue.Points;
                    DataPointCollection listGreen = seriesGreen.Points;
                    DataPointCollection listRed = seriesRed.Points;
                    for (int i = 0; i < hgray.Length; i++)
                    {
                        listBlue.AddXY(i, hrgb[0,i]);
                        listGreen.AddXY(i, hrgb[1,i]);
                        listRed.AddXY(i, hrgb[2,i]);
                    }
                    chart1.ResumeLayout();
                    break;
                case "Histograma Gray e RGB":
                    chart1.Series.Clear();
                    Series seriesGrayC = new Series("Gray")
                    {
                        Color = Color.Gray,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    Series seriesBlueC = new Series("Blue")
                    {
                        Color = Color.Blue,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    Series seriesGreenC = new Series("Green")
                    {
                        Color = Color.Green,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    Series seriesRedC = new Series("Red")
                    {
                        Color = Color.Red,
                        ChartType = SeriesChartType.Column // or whatever type you want
                    };
                    chart1.Series.Add(seriesGrayC);
                    chart1.Series.Add(seriesBlueC);
                    chart1.Series.Add(seriesGreenC);
                    chart1.Series.Add(seriesRedC);
                    DataPointCollection listGrayC = seriesGrayC.Points;
                    DataPointCollection listBlueC = seriesBlueC.Points;
                    DataPointCollection listGreenC = seriesGreenC.Points;
                    DataPointCollection listRedC = seriesRedC.Points;
                    for (int i = 0; i < hgray.Length; i++)
                    {
                        listGrayC.AddXY(i, hcompleto[0,i]);
                        listBlueC.AddXY(i, hcompleto[1, i]);
                        listGreenC.AddXY(i, hcompleto[2, i]);
                        listRedC.AddXY(i, hcompleto[3, i]);
                    }
                    chart1.ResumeLayout();
                    break;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
