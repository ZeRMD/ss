using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SS_OpenCV
{ 
    public partial class MainForm : Form
    {
        Image<Bgr, Byte> img = null; // working image
        Image<Bgr, Byte> imgBlack = null; // black image
        Image<Bgr, Byte> imgUndo = null; // undo backup image - UNDO
        string title_bak = "";

        public MainForm()
        {
            InitializeComponent();
            title_bak = Text;
        }

        /// <summary>
        /// Opens a new image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                img = new Image<Bgr, byte>(openFileDialog1.FileName);
                Text = title_bak + " [" +
                        openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1) +
                        "]";
                imgUndo = img.Copy();
                ImageViewer.Image = img;
                ImageViewer.Refresh();
            }
        }

        /// <summary>
        /// Saves an image with a new name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageViewer.Image.Save(saveFileDialog1.FileName);
            }
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// restore last undo copy of the working image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imgUndo == null) // verify if the image is already opened
                return; 
            Cursor = Cursors.WaitCursor;
            img = imgUndo.Copy();

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Change visualization mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // zoom
            if (autoZoomToolStripMenuItem.Checked)
            {
                ImageViewer.SizeMode = PictureBoxSizeMode.Zoom;
                ImageViewer.Dock = DockStyle.Fill;
            }
            else // with scroll bars
            {
                ImageViewer.Dock = DockStyle.None;
                ImageViewer.SizeMode = PictureBoxSizeMode.AutoSize;
            }
        }

        /// <summary>
        /// Show authors form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AuthorsForm form = new AuthorsForm();
            form.ShowDialog();
        }

        /// <summary>
        /// Calculate the image negative
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.Negative(img);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Call image convertion to gray scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.ConvertToGray(img);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Call automated image processing check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void evalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EvalForm eval = new EvalForm();
            eval.ShowDialog();

        }

        private void ImageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            int aux_x = 0;
            int aux_y = 0;
            if (ImageViewer.SizeMode == PictureBoxSizeMode.Zoom)
            {
                aux_x = (int)(e.X / ImageViewer.ZoomScale + ImageViewer.HorizontalScrollBar.Value * ImageViewer.ZoomScale);
                aux_y = (int)(e.Y / ImageViewer.ZoomScale + ImageViewer.VerticalScrollBar.Value * ImageViewer.ZoomScale);

            }
            else
            {
                aux_x = (int)(e.X / ImageViewer.ZoomScale + ImageViewer.HorizontalScrollBar.Value * ImageViewer.ZoomScale);
                aux_y = (int)(e.Y / ImageViewer.ZoomScale + ImageViewer.VerticalScrollBar.Value * ImageViewer.ZoomScale);
            }


            if (img != null && aux_y < img.Height && aux_x < img.Width)
                statusLabel.Text = "X:" + aux_x + "  Y:" + aux_y + " - BGR = (" + img.Data[aux_y, aux_x, 0] + "," + img.Data[aux_y, aux_x, 1] + "," + img.Data[aux_y, aux_x, 2] + ")";

        }

        /// <summary>
        /// Calculate the image negative
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void negativeLentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.NegativeLento(img);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Calculate the image grey using only red
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.ConvertToGrayChannel(img, 2);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Calculate the image grey using only green
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.ConvertToGrayChannel(img, 1);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        /// <summary>
        /// Calculate the image grey using only blue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.ConvertToGrayChannel(img, 0);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        private void brightContrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            InputBox form1 = new InputBox("Brilho?"); 
            form1.ShowDialog();
            int brilho = Convert.ToInt32(form1.ValueTextBox.Text);
            form1.Hide();

            if ((brilho < -255) && (brilho > 255))
                return;

            InputBox form2 = new InputBox("Contraste?"); 
            form2.ShowDialog();
            int contraste= Convert.ToInt32(form2.ValueTextBox.Text);
            form2.Hide();

            if ((contraste < 0) && (contraste > 3))
                return;

            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.BrightContrast(img, brilho, contraste);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        /// <summary>
        /// Translation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void translationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            InputBox form1 = new InputBox("Dx?");
            form1.ShowDialog();
            int dx = Convert.ToInt32(form1.ValueTextBox.Text);
            form1.Hide();

            if ((dx < -10) && (dx > 10))
                return;

            InputBox form2 = new InputBox("Dy?");
            form2.ShowDialog();
            int dy = Convert.ToInt32(form2.ValueTextBox.Text);
            form2.Hide();

            if ((dy < -10) && (dy > 10))
                return;

            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.Translation(img, img.Copy(), dx, dy);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        /// <summary>
        /// Rotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            InputBox form1 = new InputBox("Angle?");
            form1.ShowDialog();
            double angle = Convert.ToDouble(form1.ValueTextBox.Text);
            form1.Hide();

            if ((angle < -360) && (angle > 360))
                return;

            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.Rotation(img, img.Copy(), (float)angle);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            InputBox form1 = new InputBox("Scale Factor?");
            form1.ShowDialog();
            double scaleFactor = Convert.ToDouble(form1.ValueTextBox.Text);
            form1.Hide();

            if ((scaleFactor <= 0))
                return;

            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.Scale(img, img.Copy(), (float)scaleFactor);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        private void zoomWithMouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;

            InputBox form1 = new InputBox("Scale Factor?");
            form1.ShowDialog();
            double scaleFactor = Convert.ToDouble(form1.ValueTextBox.Text);
            form1.Hide();

            if ((scaleFactor <= 0))
                return;

            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.Scale(img, img.Copy(), (float)scaleFactor);

            ImageViewer.Image = img;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor
        }

        private void nonUniformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] matrixUI = new float[3, 3];

            FilterForm ff = new FilterForm();
            if(ff.ShowDialog() == DialogResult.OK)
            {
                matrixUI = ff.matrix;
            }
        }
    }




}