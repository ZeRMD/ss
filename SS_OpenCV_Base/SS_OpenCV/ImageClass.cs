using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.Management.Instrumentation;
using System.Windows.Forms;
using Emgu.CV.ImgHash;
using System.Linq;
using Emgu.CV.XFeatures2D;
using System.Diagnostics.Eventing.Reader;

namespace SS_OpenCV
{
    class ImageClass
    {

        /// <summary>
        /// Image Negative using EmguCV library
        /// Slower method
        /// </summary>
        /// <param name="img">Image</param>
        public static void NegativeLento(Image<Bgr, byte> img)
        {
            int x, y;

            Bgr aux;
            for (y = 0; y < img.Height; y++)
            {
                for (x = 0; x < img.Width; x++)
                {
                    // acesso pela biblioteca : mais lento 
                    aux = img[y, x];
                    img[y, x] = new Bgr(255 - aux.Blue, 255 - aux.Green, 255 - aux.Red);
                }
            }
        }

        /// <summary>
        /// Image Negative com acesso direto a memória
        /// </summary>
        /// <param name="img">Image</param>
        public static void Negative(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image
                byte blue, green, red;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.NChannels; // number of channels - 3
                int padding = m.WidthStep - m.NChannels * m.Width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrieve 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            //Calculate the negative (newChannelValue = 255 - oldChannelValue) and store in the image

                            dataPtr[0] = (byte)(255 - blue);
                            dataPtr[1] = (byte)(255 - green);
                            dataPtr[2] = (byte)(255 - red);

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Convert to gray
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void ConvertToGray(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image
                byte blue, green, red, gray;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.NChannels; // number of channels - 3
                int padding = m.WidthStep - m.NChannels * m.Width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrieve 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            // store in the image
                            dataPtr[0] = gray;
                            dataPtr[1] = gray;
                            dataPtr[2] = gray;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Grey com acesso direto a memória e uma das componentes
        /// channel = 0 (blue) / channel = 1 (green) / channel = 2 (red)
        /// </summary>
        /// <param name="img">Image</param>
        public static void ConvertToGrayChannel(Image<Bgr, byte> img, int channel)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image

                int width = img.Width;
                int height = img.Height;
                int nChan = m.NChannels; // number of channels - 3
                int padding = m.WidthStep - m.NChannels * m.Width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {

                            //Calculate the negative (newChannelValue = 255 - oldChannelValue) and store in the image

                            dataPtr[0] = dataPtr[channel];
                            dataPtr[1] = dataPtr[channel];
                            dataPtr[2] = dataPtr[channel];

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Image Negative com acesso direto a memória
        /// channel = 0 (blue) / channel = 1 (green) / channel = 2 (red)
        /// </summary>
        /// <param name="img">Image</param>
        public static void RedChannel(Image<Bgr, byte> img)
        {
            ConvertToGrayChannel(img, 2);
        }

        /// <summary>
        /// Aplica contraste e brilho com acesso direto a memória
        /// brilho = [-255, 255] / contraste = [0, 3]
        /// </summary>
        /// <param name="img">Image</param>
        public static void BrightContrast(Image<Bgr, byte> img, int bright, double contrast)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image
                byte blue, green, red;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.NChannels; // number of channels - 3
                int padding = m.WidthStep - m.NChannels * m.Width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrieve 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            //Calculate the negative (newChannelValue = 255 - oldChannelValue) and store in the image

                            dataPtr[0] = BrightContrastCalculation(contrast, bright, blue);
                            dataPtr[1] = BrightContrastCalculation(contrast, bright, green);
                            dataPtr[2] = BrightContrastCalculation(contrast, bright, red);

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        public static byte BrightContrastCalculation(double contrast, int bright, byte channel)
        {
            double result = (contrast * channel) + bright;
            if(result < 0)
                return 0;
            if (result > 255)
                return 255;
            return (byte)result;
        } 
    }
}
