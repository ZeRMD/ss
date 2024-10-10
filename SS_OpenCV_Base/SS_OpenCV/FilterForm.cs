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

namespace SS_OpenCV
{
    public partial class FilterForm : Form
    {
        public float[,] matrix = new float[3, 3];

        public float weight;

        public float offset;

        public FilterForm()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "Custom";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RetirarMatrizForm();

            DialogResult = DialogResult.OK;
        }

        private void RetirarMatrizForm()
        {
            // 00
            if (!float.TryParse(textBox1.Text, out matrix[0, 0]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 01
            if (!float.TryParse(textBox2.Text, out matrix[0, 1]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 02
            if (!float.TryParse(textBox3.Text, out matrix[0, 2]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 10
            if (!float.TryParse(textBox4.Text, out matrix[1, 0]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 11
            if (!float.TryParse(textBox5.Text, out matrix[1, 1]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 12
            if (!float.TryParse(textBox6.Text, out matrix[1, 2]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 20
            if (!float.TryParse(textBox7.Text, out matrix[2, 0]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 21
            if (!float.TryParse(textBox8.Text, out matrix[2, 1]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }
            // 22
            if (!float.TryParse(textBox9.Text, out matrix[2, 2]))
            {
                MessageBox.Show("Matriz tem de conter um número inteiro!");
                return;
            }

            if (!float.TryParse(textBox10.Text, out weight))
            {
                MessageBox.Show("Weight tem de conter um número inteiro!");
                return;
            }

            if (!float.TryParse(textBox11.Text, out offset))
            {
                MessageBox.Show("Offset tem de conter um número inteiro!");
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarComboBox();
        }

        private void AtualizarComboBox()
        {

            string[,] matrix = new string[3,3];

            string weight = "0";
            string offset = "0";

            switch (comboBox1.Text)
            {
                case "Gaussiana":
                    matrix[0, 0] = "1";
                    matrix[0, 1] = "2";
                    matrix[0, 2] = "1";

                    matrix[1, 0] = "2";
                    matrix[1, 1] = "4";
                    matrix[1, 2] = "2";

                    matrix[2, 0] = "1";
                    matrix[2, 1] = "2";
                    matrix[2, 2] = "1";

                    weight = "16";

                    offset = "0";

                    break;
                case "Realce de Contornos":
                    matrix[0, 0] = "-1";
                    matrix[0, 1] = "-1";
                    matrix[0, 2] = "-1";

                    matrix[1, 0] = "-1";
                    matrix[1, 1] = "9";
                    matrix[1, 2] = "-1";

                    matrix[2, 0] = "-1";
                    matrix[2, 1] = "-1";
                    matrix[2, 2] = "-1";

                    weight = "1";

                    offset = "0";

                    break;
                case "Laplacian Hard":
                    matrix[0, 0] = "1";
                    matrix[0, 1] = "-2";
                    matrix[0, 2] = "1";

                    matrix[1, 0] = "-2";
                    matrix[1, 1] = "4";
                    matrix[1, 2] = "-2";

                    matrix[2, 0] = "1";
                    matrix[2, 1] = "-2";
                    matrix[2, 2] = "1";

                    weight = "1";

                    offset = "0";

                    break;
                case "Linhas Verticais":
                    matrix[0, 0] = "0";
                    matrix[0, 1] = "0";
                    matrix[0, 2] = "0";

                    matrix[1, 0] = "-1";
                    matrix[1, 1] = "2";
                    matrix[1, 2] = "-1";

                    matrix[2, 0] = "0";
                    matrix[2, 1] = "0";
                    matrix[2, 2] = "0";
                    
                    weight = "1";

                    offset = "128";

                    break;
                case "Custom":
                    matrix[0, 0] = "1";
                    matrix[0, 1] = "1";
                    matrix[0, 2] = "1";

                    matrix[1, 0] = "1";
                    matrix[1, 1] = "1";
                    matrix[1, 2] = "1";

                    matrix[2, 0] = "1";
                    matrix[2, 1] = "1";
                    matrix[2, 2] = "1";

                    weight = "9";

                    offset = "0";
                    break;
            }
            EscreverMatrixUI(matrix, offset, weight);
        }

        private void EscreverMatrixUI(string[,] matrix, string offset, string weight)
        {
            //00
            textBox1.ResetText();
            textBox1.AppendText(matrix[0,0]);
            //01
            textBox2.ResetText();
            textBox2.AppendText(matrix[0,1]);
            //02
            textBox3.ResetText();
            textBox3.AppendText(matrix[0,2]);
            
            //10
            textBox4.ResetText();
            textBox4.AppendText(matrix[1,0]);
            //11
            textBox5.ResetText();
            textBox5.AppendText(matrix[1,1]);
            //12
            textBox6.ResetText();
            textBox6.AppendText(matrix[1,2]);
            
            //20
            textBox7.ResetText();
            textBox7.AppendText(matrix[2,0]);
            //21
            textBox8.ResetText();
            textBox8.AppendText(matrix[2,1]);
            //22
            textBox9.ResetText();
            textBox9.AppendText(matrix[2,2]);

            //weight
            textBox10.ResetText();
            textBox10.AppendText(weight);
            
            //offset
            textBox11.ResetText();
            textBox11.AppendText(offset);

        }

    }
}
