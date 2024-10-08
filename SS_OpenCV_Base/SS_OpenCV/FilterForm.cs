using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            if (comboBox1.Text == "")
            {

            }
        }
    }
}
