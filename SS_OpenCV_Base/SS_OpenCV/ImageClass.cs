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
using System.Runtime.Remoting.Channels;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.ComTypes;
using Emgu.CV.CvEnum;
using Emgu.CV.Flann;
using System.Reflection;
using System.ComponentModel;
using Emgu.CV.Ocl;

using ResultsDLL;
using Emgu.CV.OCR;
using System.Reflection.Emit;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;

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
        /// Apenas o channel (red)
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

                double teste;

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

                            /* Maneira mais lenta
                            dataPtr[0] = BrightContrastCalculation(contrast, bright, blue);
                            dataPtr[1] = BrightContrastCalculation(contrast, bright, green);
                            dataPtr[2] = BrightContrastCalculation(contrast, bright, red);
                            */
                            
                            teste = (contrast * blue) + bright;

                            if (teste < 0)
                                dataPtr[0] = 0;
                            else
                            {
                                if (teste > 255)
                                    dataPtr[0] = 255;
                                else
                                {
                                    dataPtr[0] = (byte)Math.Round(teste);
                                }
                            }

                            teste = (contrast * green) + bright;

                            if (teste < 0)
                                dataPtr[1] = 0;
                            else
                            {
                                if (teste > 255)
                                    dataPtr[1] = 255;
                                else
                                {
                                    dataPtr[1] = (byte)Math.Round(teste);
                                }
                            }

                            teste = (contrast * red) + bright;

                            if (teste < 0)
                                dataPtr[2] = 0;
                            else
                            {
                                if (teste > 255)
                                    dataPtr[2] = 255;
                                else
                                {
                                    dataPtr[2] = (byte)Math.Round(teste);
                                }
                            }

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
            return (byte)Math.Round(result);
        }

        /// <summary>
        /// Aplica Translação
        /// dx = [-10,10] / dy = [-10,10]
        /// </summary>
        /// <param name="img">Image</param>
        public static void Translation(Image<Bgr, byte> imgDestino, Image<Bgr, byte> imgOrigem, int dx, int dy)
        {
            unsafe
            {

                MIplImage m1 = imgDestino.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrigem.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDestino.Width;
                int height = imgDestino.Height;

                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                if (nChan == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < height; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < width; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = pixelNoDestinoX - dx;
                            PixelNaOrigemY = pixelNoDestinoY - dy;

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= width) || (PixelNaOrigemY >= height) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotal) + (PixelNaOrigemX * nChan);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += padding;
                    }
                }
            }

        }

        /// <summary>
        /// Aplica Rotatação no centro da imagem
        /// angle em radianos
        /// </summary>
        /// <param name="img">Image</param>
        public static void Rotation(Image<Bgr, byte> imgDestino, Image<Bgr, byte> imgOrigem, float angle)
        {
            unsafe
            {

                MIplImage m1 = imgDestino.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrigem.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDestino.Width;
                int height = imgDestino.Height;
                int widthOrigem = imgOrigem.Width;
                int heightOrigem = imgOrigem.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                double halfWidthT = widthOrigem / 2.0;
                double halfHeight = heightOrigem / 2.0;
                double cosAngle = Math.Cos(angle);
                double sinAngle = Math.Sin(angle);

                if (nChan == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < height; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < width; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = (int)Math.Round( (pixelNoDestinoX - halfWidthT)*cosAngle - (halfHeight - pixelNoDestinoY) * sinAngle + halfWidthT);
                            PixelNaOrigemY = (int)Math.Round(halfHeight - (pixelNoDestinoX - halfWidthT) * sinAngle - (halfHeight - pixelNoDestinoY) * cosAngle );

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= widthOrigem) || (PixelNaOrigemY >= heightOrigem) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotal) + (PixelNaOrigemX * nChan);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += padding;
                    }
                }
            }

        }

        /// <summary>
        /// Aplica zoom
        /// scaleFactor
        /// </summary>
        /// <param name="img">Image</param>
        public static void Scale(Image<Bgr, byte> imgDestino, Image<Bgr, byte> imgOrigem, float scaleFactor)
        {
            unsafe
            {

                MIplImage m1 = imgDestino.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrigem.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDestino.Width;
                int height = imgDestino.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int pixelNoDestinoX, pixelNoDestinoY;
                int PixelNaOrigemX, PixelNaOrigemY;

                byte* dataPtrAjuda;

                if (nChan == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < height; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < width; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = (int)Math.Round(pixelNoDestinoX/scaleFactor); 
                            PixelNaOrigemY = (int)Math.Round(pixelNoDestinoY/scaleFactor);

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= width) || (PixelNaOrigemY >= height) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotal) + (PixelNaOrigemX * nChan);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Aplica zoom
        /// scaleFactor
        /// center x
        /// center y
        /// </summary>
        /// <param name="img">Image</param>
        public static void Scale_point_xy(Image<Bgr, byte> imgDestino, Image<Bgr, byte> imgOrigem, float scaleFactor, int centerX, int centerY)
        {
            unsafe
            {

                MIplImage m1 = imgDestino.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrigem.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDestino.Width;
                int height = imgDestino.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int pixelNoDestinoX, pixelNoDestinoY;
                int PixelNaOrigemX, PixelNaOrigemY;

                byte* dataPtrAjuda;

                double halfWidthT = width/ 2.0;
                double halfHeight = height/ 2.0;

                double scalePlacementX = (width / 2 ) / scaleFactor;
                double scalePlacementY = (height / 2 ) / scaleFactor;

                if (nChan == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < height; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < width; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = (int)Math.Round((pixelNoDestinoX / scaleFactor) - scalePlacementX + centerX);
                            PixelNaOrigemY = (int)Math.Round((pixelNoDestinoY / scaleFactor) - scalePlacementY + centerY);

                            // Fórmula
                            //PixelNaOrigemX = (int)Math.Round((pixelNoDestinoX / scaleFactor) - (widthOrigem / (2.0 * scaleFactor) ) + centerX);
                            //PixelNaOrigemY = (int)Math.Round((pixelNoDestinoY / scaleFactor) - (heightOrigem / (2.0 * scaleFactor) ) + centerY);

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= width) || (PixelNaOrigemY >= height) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotal) + (PixelNaOrigemX * nChan);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += padding;
                    }
                }
            }
        }

        /// <summary>
        ///  Method A
        /// Mean filter 3*3
        /// </summary>
        /// <param name="img">Image</param>
        public static void Mean(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino
                
                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                double somaChannels;
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;


                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de cima
                index = (nChan * grossuraBorda) + ((grossuraBorda - 1) * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda direita
                        somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                        somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                        // Linha de baixo
                        somaChannels += dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += nChan;
                }

                //Borda de baixo
                index = (nChan * grossuraBorda) + (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda direita
                        somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                        somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                        // Linha de cima
                        somaChannels += dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += nChan;
                }

                //Borda da esquerda
                index = (nChan * (grossuraBorda - 1)) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Direita
                        somaChannels += dataPtrOrigem[index + nChan + channel];
                        // Linha de cima
                        somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];
                        // Linha de baixo
                        somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += widthTotal;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda
                        somaChannels += dataPtrOrigem[index - nChan + channel];
                        // Linha de cima
                        somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];
                        // Linha de baixo
                        somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += widthTotal;
                }

                // Canto Superior Esquerdo
                index = 0;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Direita
                    somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                    // Linha de baixo
                    somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                    somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Superior Direito
                index = ((width - grossuraBorda) * nChan);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Esquerda
                    somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                    // Linha de baixo
                    somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                    somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Inferior Esquerdo
                index = (height - grossuraBorda) * widthTotal;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Direita
                    somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                    // Linha de cima
                    somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                    somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Esquerda
                    somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                    // Linha de cima
                    somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                    somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }


                //Core
                if (nChan == 3) // image in RGB
                {
                    index = (grossuraBorda * widthTotal) + (grossuraBorda * nChan);

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {
                            
                            for (channel = 0; channel < 3; channel++)
                            {
                                // Ele próprio com reset
                                somaChannels = dataPtrOrigem[index + channel];
                                //Esquerda direita
                                somaChannels += (dataPtrOrigem)[index + nChan + channel];
                                somaChannels += (dataPtrOrigem)[index - nChan + channel];
                                // Linha de cima
                                somaChannels += (dataPtrOrigem)[index - widthTotal + channel];
                                somaChannels += (dataPtrOrigem)[index - widthTotal - nChan + channel];
                                somaChannels += (dataPtrOrigem)[index - widthTotal + nChan + channel];
                                // Linha de baixo
                                somaChannels += (dataPtrOrigem)[index + widthTotal + channel];
                                somaChannels += (dataPtrOrigem)[index + widthTotal - nChan + channel];
                                somaChannels += (dataPtrOrigem)[index + widthTotal + nChan + channel];

                                dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);

                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + 2 * (grossuraBorda * nChan));
                    }
                }
                
            }
        }

        /// <summary>
        /// Method B
        /// Mean filter 3*3 
        /// </summary>
        /// <param name="img">Image</param>
        public static void Mean_solutionBA(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig){
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                double somaChannels;
                double[] somaChannelsB = new double[nChan];
                double[] somaChannelsAntigo = new double[nChan];
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;



                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de cima
                index = (nChan * grossuraBorda) + ((grossuraBorda - 1) * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda direita
                        somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                        somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                        // Linha de baixo
                        somaChannels += dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += nChan;
                }

                //Borda de baixo
                index = (nChan * grossuraBorda) + (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda direita
                        somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                        somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                        // Linha de cima
                        somaChannels += dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += nChan;
                }

                //Borda da esquerda
                index = (nChan * (grossuraBorda - 1)) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Direita
                        somaChannels += dataPtrOrigem[index + nChan + channel];
                        // Linha de cima
                        somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];
                        // Linha de baixo
                        somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += widthTotal;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Ele próprio e reset
                        somaChannels = 2 * dataPtrOrigem[index + channel];
                        // Esquerda
                        somaChannels += dataPtrOrigem[index - nChan + channel];
                        // Linha de cima
                        somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                        somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];
                        // Linha de baixo
                        somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                        somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                    }
                    index += widthTotal;
                }

                // Canto Superior Esquerdo
                index = 0;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Direita
                    somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                    // Linha de baixo
                    somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                    somaChannels += dataPtrOrigem[index + widthTotal + nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Superior Direito
                index = ((width - grossuraBorda) * nChan);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Esquerda
                    somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                    // Linha de baixo
                    somaChannels += 2 * dataPtrOrigem[index + widthTotal + channel];
                    somaChannels += dataPtrOrigem[index + widthTotal - nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Inferior Esquerdo
                index = (height - grossuraBorda) * widthTotal;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Direita
                    somaChannels += 2 * dataPtrOrigem[index + nChan + channel];
                    // Linha de cima
                    somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                    somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Ele próprio e reset
                    somaChannels = 4 * dataPtrOrigem[index + channel];
                    // Esquerda
                    somaChannels += 2 * dataPtrOrigem[index - nChan + channel];
                    // Linha de cima
                    somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                    somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];

                    dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                }

                //Core
                if (nChan == 3) // image in RGB
                {
                    index = (grossuraBorda * widthTotal) + (grossuraBorda * nChan);

                    for (channel = 0; channel < 3; channel++)
                    {
                        // Ele próprio com reset
                        somaChannelsB[channel] = dataPtrOrigem[index + channel];
                        //Esquerda direita
                        somaChannelsB[channel] += (dataPtrOrigem)[index + nChan + channel];
                        somaChannelsB[channel] += (dataPtrOrigem)[index - nChan + channel];
                        // Linha de cima
                        somaChannelsB[channel] += (dataPtrOrigem)[index - widthTotal + channel];
                        somaChannelsB[channel] += (dataPtrOrigem)[index - widthTotal - nChan + channel];
                        somaChannelsB[channel] += (dataPtrOrigem)[index - widthTotal + nChan + channel];
                        // Linha de baixo
                        somaChannelsB[channel] += (dataPtrOrigem)[index + widthTotal + channel];
                        somaChannelsB[channel] += (dataPtrOrigem)[index + widthTotal - nChan + channel];
                        somaChannelsB[channel] += (dataPtrOrigem)[index + widthTotal + nChan + channel];

                        somaChannelsAntigo[channel] = somaChannelsB[channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannelsB[channel] / 9.0);
                    }
                    
                    index += nChan;

                    for (PixelNoDestinoX = 1; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                    {

                        for (channel = 0; channel < 3; channel++)
                        {
                            // Retirar os da Esquerda (do antigo)
                            // Adicionar os da Direita (do novo)

                            // Esquerda
                            somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) + channel];
                            somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) + widthTotal + channel];
                            somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) - widthTotal + channel];

                            // Direita
                            somaChannelsB[channel] += (dataPtrOrigem)[index + nChan + channel];
                            somaChannelsB[channel] += (dataPtrOrigem)[index + nChan + widthTotal + channel];
                            somaChannelsB[channel] += (dataPtrOrigem)[index + nChan - widthTotal + channel];

                            dataPtrDestino[index + channel] = (byte)Math.Round(somaChannelsB[channel] / 9.0);

                        }
                        // advance the pointer to the next pixel
                        index += nChan;
                    }
                    //at the end of the line advance the pointer by the aligment bytes (padding)
                    index += (padding + 2 * (grossuraBorda * nChan));

                    for (PixelNoDestinoY = 1; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {

                        // Retirar os da Esquerda (do antigo)
                        // Adicionar os da Direita (do novo)
                        
                        // Esquerda
                        somaChannelsAntigo[channel] -= (dataPtrOrigem)[index - (2 * widthTotal) + channel];
                        somaChannelsAntigo[channel] -= (dataPtrOrigem)[index - (2 * widthTotal) + nChan + channel];
                        somaChannelsAntigo[channel] -= (dataPtrOrigem)[index - (2 * widthTotal) - nChan + channel];

                        // Direita
                        somaChannelsAntigo[channel] += (dataPtrOrigem)[index + widthTotal + channel];
                        somaChannelsAntigo[channel] += (dataPtrOrigem)[index + widthTotal + nChan + channel];
                        somaChannelsAntigo[channel] += (dataPtrOrigem)[index + widthTotal - nChan + channel];

                        dataPtrDestino[index + channel] = (byte)Math.Round(somaChannelsB[channel] / 9.0);

                        somaChannelsB = somaChannelsAntigo;

                        index += nChan;

                        for (PixelNoDestinoX = 1; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {

                            for (channel = 0; channel < 3; channel++)
                            {
                                // Retirar os da Esquerda (do antigo)
                                // Adicionar os da Direita (do novo)

                                // Esquerda
                                somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) + channel];
                                somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) + widthTotal + channel];
                                somaChannelsB[channel] -= (dataPtrOrigem)[index - (2 * nChan) - widthTotal + channel];

                                // Direita
                                somaChannelsB[channel] += (dataPtrOrigem)[index + nChan + channel];
                                somaChannelsB[channel] += (dataPtrOrigem)[index + nChan + widthTotal + channel];
                                somaChannelsB[channel] += (dataPtrOrigem)[index + nChan - widthTotal + channel];

                                dataPtrDestino[index + channel] = (byte)Math.Round(somaChannelsB[channel] / 9.0);

                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + 2 * (grossuraBorda * nChan));
                    }
                }

            }
        }

        /// <summary>
        /// Method C
        /// scaleFactor
        /// </summary>
        /// <param name="img">Image</param>
        public static void Mean_solutionC(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig, int size)
        {

        }

        /// <summary>
        /// Non univoform
        /// offset
        /// matrixWeight
        /// </summary>
        /// <param name="img">Image</param>
        public static void NonUniform(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig, float[,] matrix, float matrixWeight, float offset)
        {
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                double somaChannels;
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;

                float coef00 = matrix[0, 0];
                float coef01 = matrix[0, 1];
                float coef02 = matrix[0, 2];
                float coef10 = matrix[1, 0];
                float coef11 = matrix[1, 1];
                float coef12 = matrix[1, 2];
                float coef20 = matrix[2, 0];
                float coef21 = matrix[2, 1];
                float coef22 = matrix[2, 2];

                double conta;

                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de cima
                index = (nChan * grossuraBorda) + ((grossuraBorda - 1) * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Linha 0
                        somaChannels = coef00 * dataPtrOrigem[index - nChan + channel]; // 00
                        somaChannels += coef01 * dataPtrOrigem[index + channel]; // 01
                        somaChannels += coef02 * dataPtrOrigem[index + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                        somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2

                        somaChannels += coef20 * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22 * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                        conta = Math.Round(somaChannels / matrixWeight + offset);

                        if (conta < 0) 
                            conta = 0;
                        else if (conta > 255) 
                            conta = 255;

                        dataPtrDestino[index + channel] = (byte)conta;
                    }
                    index += nChan;
                }

                //Borda de baixo
                index = (nChan * grossuraBorda) + (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Linha 0

                        somaChannels = coef00 * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02 * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                        somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2
                        
                        somaChannels += coef20 * dataPtrOrigem[index - nChan + channel]; // 20
                        somaChannels += coef21 * dataPtrOrigem[index + channel]; // 21
                        somaChannels += coef22 * dataPtrOrigem[index + nChan + channel]; // 22

                        conta = Math.Round(somaChannels / matrixWeight + offset);

                        if (conta < 0)
                            conta = 0;
                        else if (conta > 255)
                            conta = 255;

                        dataPtrDestino[index + channel] = (byte)conta;
                    }
                    index += nChan;
                }

                //Borda da esquerda
                index = (nChan * (grossuraBorda - 1)) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Linha 0

                        somaChannels = coef00 * dataPtrOrigem[index - widthTotal + channel]; // 00
                        somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02 * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10 * dataPtrOrigem[index + channel]; // 10
                        somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                        somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2

                        somaChannels += coef20 * dataPtrOrigem[index + widthTotal + channel]; // 20
                        somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22 * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                        conta = Math.Round(somaChannels / matrixWeight + offset);

                        if (conta < 0)
                            conta = 0;
                        else if (conta > 255)
                            conta = 255;

                        dataPtrDestino[index + channel] = (byte)conta;
                    }
                    index += widthTotal;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Linha 0

                        somaChannels = coef00 * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02 * dataPtrOrigem[index - widthTotal + channel]; // 02

                        // Linha 1

                        somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                        somaChannels += coef12 * dataPtrOrigem[index + channel]; // 12

                        // Linha 2

                        somaChannels += coef20 * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22 * dataPtrOrigem[index + widthTotal + channel]; // 22

                        conta = Math.Round(somaChannels / matrixWeight + offset);

                        if (conta < 0)
                            conta = 0;
                        else if (conta > 255)
                            conta = 255;

                        dataPtrDestino[index + channel] = (byte)conta;
                    }
                    index += widthTotal;
                }

                // Canto Superior Esquerdo
                index = 0;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Linha 0

                    somaChannels = coef00 * dataPtrOrigem[index + channel]; // 00
                    somaChannels += coef01 * dataPtrOrigem[index + channel]; // 01
                    somaChannels += coef02 * dataPtrOrigem[index + nChan + channel]; // 02

                    // Linha 1

                    somaChannels += coef10 * dataPtrOrigem[index + channel]; // 10
                    somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                    somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                    // Linha 2

                    somaChannels += coef20 * dataPtrOrigem[index + widthTotal + channel]; // 20
                    somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                    somaChannels += coef22 * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                    conta = Math.Round(somaChannels / matrixWeight + offset);

                    if (conta < 0)
                        conta = 0;
                    else if (conta > 255)
                        conta = 255;

                    dataPtrDestino[index + channel] = (byte)conta;
                }

                // Canto Superior Direito
                index = ((width - grossuraBorda) * nChan);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Linha 0

                    somaChannels = coef00 * dataPtrOrigem[index - nChan + channel]; // 00
                    somaChannels += coef01 * dataPtrOrigem[index + channel]; // 01
                    somaChannels += coef02 * dataPtrOrigem[index + channel]; // 02

                    // Linha 1

                    somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                    somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                    somaChannels += coef12 * dataPtrOrigem[index + channel]; // 12

                    // Linha 2

                    somaChannels += coef20 * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                    somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                    somaChannels += coef22 * dataPtrOrigem[index + widthTotal + channel]; // 22

                    conta = Math.Round(somaChannels / matrixWeight + offset);

                    if (conta < 0)
                        conta = 0;
                    else if (conta > 255)
                        conta = 255;

                    dataPtrDestino[index + channel] = (byte)conta;
                }

                // Canto Inferior Esquerdo
                index = (height - grossuraBorda) * widthTotal;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Linha 0

                    somaChannels = coef00 * dataPtrOrigem[index - widthTotal + channel]; // 00
                    somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                    somaChannels += coef02 * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                    // Linha 1

                    somaChannels += coef10 * dataPtrOrigem[index + channel]; // 10
                    somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                    somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                    // Linha 2

                    somaChannels += coef20 * dataPtrOrigem[index + channel]; // 20
                    somaChannels += coef21 * dataPtrOrigem[index + channel]; // 21
                    somaChannels += coef22 * dataPtrOrigem[index + nChan + channel]; // 22

                    conta = Math.Round(somaChannels / matrixWeight + offset);

                    if (conta < 0)
                        conta = 0;
                    else if (conta > 255)
                        conta = 255;

                    dataPtrDestino[index + channel] = (byte)conta;
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Linha 0

                    somaChannels = coef00 * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                    somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                    somaChannels += coef02 * dataPtrOrigem[index - widthTotal + channel]; // 02

                    // Linha 1

                    somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                    somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                    somaChannels += coef12 * dataPtrOrigem[index + channel]; // 12

                    // Linha 2

                    somaChannels += coef20 * dataPtrOrigem[index - nChan + channel]; // 20
                    somaChannels += coef21 * dataPtrOrigem[index + channel]; // 21
                    somaChannels += coef22 * dataPtrOrigem[index + channel]; // 22

                    conta = Math.Round(somaChannels / matrixWeight + offset);

                    if (conta < 0)
                        conta = 0;
                    else if (conta > 255)
                        conta = 255;

                    dataPtrDestino[index + channel] = (byte)conta;
                }


                //Core
                if (nChan == 3) // image in RGB
                {
                    index = (grossuraBorda * widthTotal) + (grossuraBorda * nChan);

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {

                            for (channel = 0; channel < 3; channel++)
                            {
                                // Linha 0

                                somaChannels = coef00 * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                                somaChannels += coef01 * dataPtrOrigem[index - widthTotal + channel]; // 01
                                somaChannels += coef02 * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                                // Linha 1

                                somaChannels += coef10 * dataPtrOrigem[index - nChan + channel]; // 10
                                somaChannels += coef11 * dataPtrOrigem[index + channel]; // 11
                                somaChannels += coef12 * dataPtrOrigem[index + nChan + channel]; // 12

                                // Linha 2

                                somaChannels += coef20 * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                                somaChannels += coef21 * dataPtrOrigem[index + widthTotal + channel]; // 21
                                somaChannels += coef22 * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                                conta = Math.Round(somaChannels / matrixWeight + offset);

                                if (conta < 0)
                                    conta = 0;
                                else if (conta > 255)
                                    conta = 255;

                                dataPtrDestino[index + channel] = (byte)conta;
                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + 2 * (grossuraBorda * nChan));
                    }
                }

            }
        }

        /// <summary>
        /// Sobel filter
        /// </summary>
        /// <param name="img">Image</param>
        public static void Sobel(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                double somaChannels;
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;

                // Coeficientes SX
                float coef00Sx = -1;
                float coef01Sx = -2;
                float coef02Sx = -1;
                float coef20Sx = 1;
                float coef21Sx = 2;
                float coef22Sx = 1;
                
                // Coeficientes SY
                float coef00Sy = 1;
                float coef02Sy = -1;
                float coef10Sy = 2;
                float coef12Sy = -2;
                float coef20Sy = 1;
                float coef22Sy = -1;

                int sx;
                int sy;
                int s;

                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de cima
                index = (nChan * grossuraBorda) + ((grossuraBorda - 1) * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Sx
                        // Linha 0
                        somaChannels = coef00Sx * dataPtrOrigem[index - nChan + channel]; // 00
                        somaChannels += coef01Sx * dataPtrOrigem[index + channel]; // 01
                        somaChannels += coef02Sx * dataPtrOrigem[index + nChan + channel]; // 02

                        // Linha 2

                        somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22
                        
                        sx = (int)somaChannels;

                        // SY
                        // Linha 0
                        somaChannels = coef00Sy * dataPtrOrigem[index - nChan + channel]; // 00
                        somaChannels += coef02Sy * dataPtrOrigem[index + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2

                        somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                        sy = (int)somaChannels;

                        s = Math.Abs(sx) + Math.Abs(sy);

                        if (s > 255)
                            s = 255;

                        dataPtrDestino[index + channel] = (byte)s;
                    }
                    index += nChan;
                }

                //Borda de baixo
                index = (nChan * grossuraBorda) + (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {

                        // Sx
                        // Linha 0
                        somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 2

                        somaChannels += coef20Sx * dataPtrOrigem[index - nChan + channel]; // 20
                        somaChannels += coef21Sx * dataPtrOrigem[index + channel]; // 21
                        somaChannels += coef22Sx * dataPtrOrigem[index + nChan + channel]; // 22

                        sx = (int)somaChannels;

                        // SY
                        // Linha 0
                        somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2

                        somaChannels += coef20Sy * dataPtrOrigem[index - nChan + channel]; // 20
                        somaChannels += coef22Sy * dataPtrOrigem[index + nChan + channel]; // 22

                        sy = (int)somaChannels;

                        s = Math.Abs(sx) + Math.Abs(sy);

                        if (s > 255)
                            s = 255;

                        dataPtrDestino[index + channel] = (byte)s;
                    }
                    index += nChan;
                }

                //Borda da esquerda
                index = (nChan * (grossuraBorda - 1)) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Sx
                        // Linha 0
                        somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal + channel]; // 00
                        somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 2

                        somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal + channel]; // 20
                        somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                        sx = (int)somaChannels;

                        // SY
                        // Linha 0
                        somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal + channel]; // 00
                        somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                        // Linha 1

                        somaChannels += coef10Sy * dataPtrOrigem[index + channel]; // 10
                        somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                        // Linha 2

                        somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal + channel]; // 20
                        somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                        sy = (int)somaChannels;

                        s = Math.Abs(sx) + Math.Abs(sy);

                        if (s > 255)
                            s = 255;

                        dataPtrDestino[index + channel] = (byte)s;
                    }
                    index += widthTotal;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan) + (grossuraBorda * widthTotal);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // Sx
                        // Linha 0
                        somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                        somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + channel]; // 02

                        // Linha 2

                        somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                        somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + channel]; // 22

                        sx = (int)somaChannels;

                        // SY
                        // Linha 0
                        somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                        somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + channel]; // 02

                        // Linha 1

                        somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                        somaChannels += coef12Sy * dataPtrOrigem[index + channel]; // 12

                        // Linha 2

                        somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                        somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + channel]; // 22

                        sy = (int)somaChannels;

                        s = Math.Abs(sx) + Math.Abs(sy);

                        if (s > 255)
                            s = 255;

                        dataPtrDestino[index + channel] = (byte)s;
                    }
                    index += widthTotal;
                }

                // Canto Superior Esquerdo
                index = 0;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Sx
                    // Linha 0
                    somaChannels = coef00Sx * dataPtrOrigem[index + channel]; // 00
                    somaChannels += coef01Sx * dataPtrOrigem[index + channel]; // 01
                    somaChannels += coef02Sx * dataPtrOrigem[index + nChan + channel]; // 02

                    // Linha 2

                    somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal + channel]; // 20
                    somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                    somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                    sx = (int)somaChannels;

                    // SY
                    // Linha 0
                    somaChannels = coef00Sy * dataPtrOrigem[index + channel]; // 00
                    somaChannels += coef02Sy * dataPtrOrigem[index + nChan + channel]; // 02

                    // Linha 1

                    somaChannels += coef10Sy * dataPtrOrigem[index + channel]; // 10
                    somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                    // Linha 2

                    somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal + channel]; // 20
                    somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                    sy = (int)somaChannels;

                    s = Math.Abs(sx) + Math.Abs(sy);

                    if (s > 255)
                        s = 255;

                    dataPtrDestino[index + channel] = (byte)s;
                }

                // Canto Superior Direito
                index = ((width - grossuraBorda) * nChan);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Sx
                    // Linha 0
                    somaChannels = coef00Sx * dataPtrOrigem[index - nChan + channel]; // 00
                    somaChannels += coef01Sx * dataPtrOrigem[index + channel]; // 01
                    somaChannels += coef02Sx * dataPtrOrigem[index + channel]; // 02

                    // Linha 2

                    somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                    somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                    somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + channel]; // 22

                    sx = (int)somaChannels;

                    // SY
                    // Linha 0
                    somaChannels = coef00Sy * dataPtrOrigem[index - nChan + channel]; // 00
                    somaChannels += coef02Sy * dataPtrOrigem[index + channel]; // 02

                    // Linha 1

                    somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                    somaChannels += coef12Sy * dataPtrOrigem[index + channel]; // 12

                    // Linha 2

                    somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                    somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + channel]; // 22

                    sy = (int)somaChannels;

                    s = Math.Abs(sx) + Math.Abs(sy);

                    if (s > 255)
                        s = 255;

                    dataPtrDestino[index + channel] = (byte)s;
                }

                // Canto Inferior Esquerdo
                index = (height - grossuraBorda) * widthTotal;
                for (channel = 0; channel < nChan; channel++)
                {
                    // Sx
                    // Linha 0
                    somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal + channel]; // 00
                    somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                    somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                    // Linha 2

                    somaChannels += coef20Sx * dataPtrOrigem[index + channel]; // 20
                    somaChannels += coef21Sx * dataPtrOrigem[index + channel]; // 21
                    somaChannels += coef22Sx * dataPtrOrigem[index + nChan + channel]; // 22

                    sx = (int)somaChannels;

                    // SY
                    // Linha 0
                    somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal + channel]; // 00
                    somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                    // Linha 1

                    somaChannels += coef10Sy * dataPtrOrigem[index + channel]; // 10
                    somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                    // Linha 2

                    somaChannels += coef20Sy * dataPtrOrigem[index + channel]; // 20
                    somaChannels += coef22Sy * dataPtrOrigem[index + nChan + channel]; // 22

                    sy = (int)somaChannels;

                    s = Math.Abs(sx) + Math.Abs(sy);

                    if (s > 255)
                        s = 255;

                    dataPtrDestino[index + channel] = (byte)s;
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {
                    // Sx
                    // Linha 0
                    somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                    somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                    somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + channel]; // 02

                    // Linha 2

                    somaChannels += coef20Sx * dataPtrOrigem[index - nChan + channel]; // 20
                    somaChannels += coef21Sx * dataPtrOrigem[index + channel]; // 21
                    somaChannels += coef22Sx * dataPtrOrigem[index + channel]; // 22

                    sx = (int)somaChannels;

                    // SY
                    // Linha 0
                    somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                    somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + channel]; // 02

                    // Linha 1

                    somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                    somaChannels += coef12Sy * dataPtrOrigem[index + channel]; // 12

                    // Linha 2

                    somaChannels += coef20Sy * dataPtrOrigem[index - nChan + channel]; // 20
                    somaChannels += coef22Sy * dataPtrOrigem[index + channel]; // 22

                    sy = (int)somaChannels;

                    s = Math.Abs(sx) + Math.Abs(sy);

                    if (s > 255)
                        s = 255;

                    dataPtrDestino[index + channel] = (byte)s;
                }


                //Core
                if (nChan == 3) // image in RGB
                {
                    index = (grossuraBorda * widthTotal) + (grossuraBorda * nChan);

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {

                            for (channel = 0; channel < 3; channel++)
                            {
                                // Sx
                                // Linha 0
                                somaChannels = coef00Sx * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                                somaChannels += coef01Sx * dataPtrOrigem[index - widthTotal + channel]; // 01
                                somaChannels += coef02Sx * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                                // Linha 2

                                somaChannels += coef20Sx * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                                somaChannels += coef21Sx * dataPtrOrigem[index + widthTotal + channel]; // 21
                                somaChannels += coef22Sx * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                                sx = (int)somaChannels ;

                                // SY
                                // Linha 0
                                somaChannels = coef00Sy * dataPtrOrigem[index - widthTotal - nChan + channel]; // 00
                                somaChannels += coef02Sy * dataPtrOrigem[index - widthTotal + nChan + channel]; // 02

                                // Linha 1

                                somaChannels += coef10Sy * dataPtrOrigem[index - nChan + channel]; // 10
                                somaChannels += coef12Sy * dataPtrOrigem[index + nChan + channel]; // 12

                                // Linha 2

                                somaChannels += coef20Sy * dataPtrOrigem[index + widthTotal - nChan + channel]; // 20
                                somaChannels += coef22Sy * dataPtrOrigem[index + widthTotal + nChan + channel]; // 22

                                sy = (int)somaChannels;

                                s = Math.Abs(sx) + Math.Abs(sy);

                                if (s > 255)
                                    s = 255;

                                dataPtrDestino[index + channel] = (byte)s;
                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + 2 * (grossuraBorda * nChan));
                    }
                }

            }
        }
        
        /// <summary>
        /// Diferentiation filter
        /// </summary>
        /// <param name="img">Image</param>
        public static void Diferentiation(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;

                int pixel;
                int dx;
                int dy;
                int d;

                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de baixo
                index = (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim + 1; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // DX
                        dx = Math.Abs(dataPtrOrigem[index + channel] - dataPtrOrigem[index + nChan + channel]);

                        if (dx > 255)
                            dx = 255;

                        dataPtrDestino[index + channel] = (byte)dx;
                    }
                    index += nChan;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim + 1; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        // DY
                        dy = Math.Abs(dataPtrOrigem[index + channel] - dataPtrOrigem[index + widthTotal + channel]);

                        if (dy > 255)
                            dy = 255;

                        dataPtrDestino[index + channel] = (byte)dy;
                    }
                    index += widthTotal;
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {

                    dataPtrDestino[index + channel] = (byte)0;
                }

                //Core
                if (nChan == 3) // image in RGB
                {
                    index = 0;

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim + 1; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim + 1 ; PixelNoDestinoX++)
                        {

                            for (channel = 0; channel < 3; channel++)
                            {
                                pixel = dataPtrOrigem[index + channel]; // 11

                                // DX
                                dx = pixel - dataPtrOrigem[index + widthTotal + channel]; // 12

                                // DY
                                dy = pixel - dataPtrOrigem[index + nChan + channel]; // 21

                                d = Math.Abs(dx) + Math.Abs(dy);

                                if (d > 255)
                                    d = 255;

                                dataPtrDestino[index + channel] = (byte)d;
                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + (grossuraBorda * nChan));
                    }
                }

            }
        }


        /// <summary>
        /// Roberts filter
        /// </summary>
        /// <param name="img">Image</param>
        public static void Roberts(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgCopy.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                /*
                 * // Se fosse para fazer com o tamanho da janela do filtro dinâmico
                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                */


                int channel;
                int grossuraBorda = 1; // Por ser 3 x 3
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;

                int pixel;
                int dx;
                int dy;
                int d;

                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda;
                loopBorderYLim = loopCoreYLim + grossuraBorda;

                //Borda de baixo
                index = (height - grossuraBorda) * widthTotal;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim + 1; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        d = 2*Math.Abs(dataPtrOrigem[index + channel] - dataPtrOrigem[index + nChan + channel]);

                        if (d > 255)
                            d = 255;

                        dataPtrDestino[index + channel] = (byte)d;
                    }
                    index += nChan;
                }

                // Borda da direita
                index = ((width - grossuraBorda) * nChan);
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim + 1; pixelBorda++)
                {
                    for (channel = 0; channel < nChan; channel++)
                    {
                        d = 2 *Math.Abs(dataPtrOrigem[index + channel] - dataPtrOrigem[index + widthTotal + channel]);

                        if (d > 255)
                            d = 255;

                        dataPtrDestino[index + channel] = (byte)d;
                    }
                    index += widthTotal;
                }

                // Canto Inferior Direito
                index = ((width - grossuraBorda) * nChan) + ((height - grossuraBorda) * widthTotal);
                for (channel = 0; channel < nChan; channel++)
                {

                    dataPtrDestino[index + channel] = (byte)0;
                }

                //Core
                if (nChan == 3) // image in RGB
                {
                    index = 0;

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim + 1; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim + 1; PixelNoDestinoX++)
                        {

                            for (channel = 0; channel < 3; channel++)
                            {
                               
                                // DX
                                dx = dataPtrOrigem[index + channel] - dataPtrOrigem[index + widthTotal + nChan + channel];

                                // DY
                                dy = dataPtrOrigem[index + nChan + channel] - dataPtrOrigem[index + widthTotal + channel];

                                d = Math.Abs(dx) + Math.Abs(dy);

                                if (d > 255)
                                    d = 255;

                                dataPtrDestino[index + channel] = (byte)d;
                            }
                            // advance the pointer to the next pixel
                            index += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        index += (padding + (grossuraBorda * nChan));
                    }
                }

            }
        }

        /// <summary>
        ///  Mediana com OpenCV
        ///  - Recebe a imagem a alterar e uma cópia da imagem
        /// </summary>
        /// <param name="img">Image</param>
        public static void Median(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            CvInvoke.MedianBlur(imgOrig, imgDest, 3);
        }

        /// <summary>
        ///  Mediana 3D
        ///  - Recebe a imagem a alterar e uma cópia da imagem
        /// </summary>
        /// <param name="img">Image</param>
        public static void Median3D(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            
        }

        /// <summary>
        ///  Histograma em cinzentos
        /// </summary>
        /// <param name="img">Image</param>
        public static int[] Histogram_Gray(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtrImagem = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int pixelX;
                int pixelY;

                int gray;

                int[] histogramData = new int[256];

                if (nChan == 3) // image in RGB
                {
                    for (pixelY = 0; pixelY < height; pixelY++)
                    {
                        for (pixelX = 0; pixelX < width; pixelX++)
                        {

                            // convert to gray
                            gray = (int)Math.Round(((dataPtrImagem[0] + dataPtrImagem[1] + dataPtrImagem[2]) / 3.0));

                            histogramData[gray] += 1;

                            //Endereçamento absoluto
                            dataPtrImagem += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrImagem += padding;
                    }
                }
                return histogramData;
            }
        }

        /// <summary>
        /// Histograma de cinzentos
        /// </summary>
        /// - Recebe a imagem a analisar
        /// - Devolve o histograma das 3 componentes B + G + R numa matrix[3, 256]
        /// <param name="img">Image</param>
        public static int[,] Histogram_RGB(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtrImagem = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int pixelX;
                int pixelY;

                int[,] histogramData = new int[3,256];

                if (nChan == 3) // image in RGB
                {
                    for (pixelY = 0; pixelY < height; pixelY++)
                    {
                        for (pixelX = 0; pixelX < width; pixelX++)
                        {

                            histogramData[0,dataPtrImagem[0]] += 1;
                            histogramData[1,dataPtrImagem[1]] += 1;
                            histogramData[2,dataPtrImagem[2]] += 1;

                            //Endereçamento absoluto
                            dataPtrImagem += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrImagem += padding;
                    }
                }
                return histogramData;
            }
        }

        /// <summary>
        /// Histograma de cinzentos
        /// </summary>
        /// - Recebe a imagem a analisar
        /// - Devolve o histograma das 4 componentes GRAY + B + G + R numa matrix[4, 256]
        /// <param name="img">Image</param>
        public static int[,] Histogram_All(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtrImagem = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int pixelX;
                int pixelY;

                int[,] histogramData = new int[4, 256];

                if (nChan == 3) // image in RGB
                {
                    for (pixelY = 0; pixelY < height; pixelY++)
                    {
                        for (pixelX = 0; pixelX < width; pixelX++)
                        {

                            histogramData[0, (int)Math.Round(((dataPtrImagem[0] + dataPtrImagem[1] + dataPtrImagem[2]) / 3.0))] += 1; // Gray
                            histogramData[1, dataPtrImagem[0]] += 1; // Blue
                            histogramData[2, dataPtrImagem[1]] += 1; // Green
                            histogramData[3, dataPtrImagem[2]] += 1; // Red

                            //Endereçamento absoluto
                            dataPtrImagem += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrImagem += padding;
                    }
                }
                return histogramData;
            }
        }

        /// <summary>
        /// Equalizacao do Histograma
        /// </summary>
        /// - Recebe a imagem a equalizar
        /// <param name="img">Image</param>
        public static void Equalization(Image<Bgr, byte> img)
        {

        }

        /// <summary>
        /// Histograma de cinzentos
        /// </summary>
        /// - recebe a imagem a alterar e o valor de threshold
        /// - Binariza a imagem através da comparação com o threshold
        /// <param name="img">Image</param>
        public static void ConvertToBW(Emgu.CV.Image<Bgr, byte> img, int threshold)
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
                            if (Math.Round((dataPtr[0] + dataPtr[1] + dataPtr[2]) / 3.0) > threshold)
                            {
                                dataPtr[0] = 255;
                                dataPtr[1] = 255;
                                dataPtr[2] = 255;
                            } else
                            {
                                dataPtr[0] = 0;
                                dataPtr[1] = 0;
                                dataPtr[2] = 0;
                            }

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
        /// Histograma de cinzentos
        /// </summary>
        /// - recebe a imagem a alterar e o valor de threshold
        /// - Binariza a imagem através da comparação com o threshold
        /// <param name="img">Image</param>
        public static void ConvertToBW_Otsu(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image

                int width = img.Width;
                int height = img.Height;

                float tamanhoImagem = width * height;

                int threshold = 0;
                double variancia;

                int[] histogramData = new int[256];

                histogramData = Histogram_Gray(img);

                double q1 = 0;
                double q2 = 0;

                for (int x = 0; x < 256; x++)
                {
                    q2 += histogramData[x]/tamanhoImagem;
                }
                
                double u1SemDivisao = 0;
                double u2SemDivisao = 0;

                for (int y = 0; y < 256; y++)
                {
                    u2SemDivisao += (histogramData[y]/tamanhoImagem) * y;
                }

                double variancianova;

                variancia = 0;

                for (int histLoop = 0; histLoop < 254; histLoop++) {

                    u1SemDivisao += (histogramData[histLoop] / tamanhoImagem) * histLoop;
                    u2SemDivisao -= (histogramData[histLoop] / tamanhoImagem) * histLoop;
                    q1 += histogramData[histLoop] / tamanhoImagem;
                    q2 -= histogramData[histLoop] / tamanhoImagem;

                    variancianova = q1 * q2 * Math.Pow((u1SemDivisao / q1) - (u2SemDivisao / q2), 2);

                    if (variancia < variancianova)
                    {
                        variancia = variancianova;
                        threshold = histLoop;
                    }
                }

                ConvertToBW(img, threshold);
            }
        }

        /// <summary>
        /// Componentes ligados
        /// </summary>
        /// - recebe a imagem para fazer componentes ligados (tem de estar binarizada)
        /// Coloca na imagem com os bojetos identificados por labels
        /// Devolve uma lista que contém as labels
        /// <param name="img">Image</param>
        public static List<int> ConectedComponentsAlgIter(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m1 = img.MIplImage;

                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the image origem

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels;
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int pixelBorda;

                int grossuraBorda = 1; // Por ser 3 x 3
                int index;
                int indexMatrixInt;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;

                //Border
                loopBorderXLim = loopCoreXLim + grossuraBorda * 2;
                loopBorderYLim = loopCoreYLim + grossuraBorda * 2;

                // Ligar Componentes
                int pixelEsquerda = 0;
                int pixelCima = 0;
                int pixel = 0;

                int valorATomar = 0;
                int valor = 0;

                int[] matrizIntermedia = new int[width * height];

                //*****************************//*****************************//
                // Labeling e Tags
                //*****************************//*****************************//
                // Passar por todos os pixeis da imagem.
                // Fazer o labeling e ficar atento a Tags
                // Guardar as tags numa lista
                // Colocar o resultado do labeling numa matriz de inteiros (labeling deixa de ficar limitado a 255 e margens aumentadas para facilidade de código) 
                //*****************************//*****************************//

                // Canto Superior Esquerdo
                index = 0;
                indexMatrixInt = 0;

                if (dataPtr[index] == 0)
                {
                    valorATomar = ++valor;

                    matrizIntermedia[indexMatrixInt] = valorATomar;
                }
                else
                {
                    matrizIntermedia[indexMatrixInt] = 0;
                }

                //Borda de Cima
                index = (nChan * grossuraBorda) + ((grossuraBorda - 1) * widthTotal);
                indexMatrixInt = grossuraBorda;

                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderXLim; pixelBorda++)
                {
                    if (dataPtr[index] == 0)
                    {
                        pixelEsquerda = matrizIntermedia[indexMatrixInt - 1];

                        if (pixelEsquerda != 0)
                        {
                            valorATomar = pixelEsquerda;
                        }
                        else
                        {
                            valorATomar = ++valor;
                        }

                        matrizIntermedia[indexMatrixInt] = valorATomar;
                    }
                    else
                    {
                        matrizIntermedia[indexMatrixInt] = 0;
                    }

                    index += nChan;
                    indexMatrixInt += 1;
                }

                //Borda da esquerda
                index = (nChan * (grossuraBorda - 1)) + (grossuraBorda * widthTotal);
                indexMatrixInt = width;
                for (pixelBorda = 0 + grossuraBorda; pixelBorda < loopBorderYLim; pixelBorda++)
                {
                    if (dataPtr[index] == 0)
                    {
                        pixelCima = matrizIntermedia[indexMatrixInt - width];

                        if (pixelCima != 0)
                        {
                            valorATomar = pixelCima;
                        }
                        else
                        {
                            valorATomar = ++valor;
                        }

                        matrizIntermedia[indexMatrixInt] = valorATomar;
                    }
                    else
                    {
                        matrizIntermedia[indexMatrixInt] = 0;
                    }

                    index += widthTotal;
                    indexMatrixInt += width;
                }


                //Core
                List<List<int>> labels = new List<List<int>>();
                index = (grossuraBorda * widthTotal) + (grossuraBorda * nChan);
                indexMatrixInt = width + 1;

                for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                {
                    for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                    {
                        if (dataPtr[index] == 0)
                        {

                            pixelEsquerda = matrizIntermedia[indexMatrixInt - 1];

                            pixelCima = matrizIntermedia[indexMatrixInt - width];

                            if (pixelEsquerda != 0)
                            {
                                if (pixelCima != 0)
                                {
                                    if (pixelEsquerda != pixelCima)
                                    {

                                        valorATomar = Math.Min(pixelEsquerda, pixelCima);

                                        bool introduzido = false;

                                        foreach (var list in labels)
                                        {
                                            if ((list.Contains(pixelEsquerda) && list.Contains(pixelCima)))
                                            {
                                                introduzido = true;
                                            }
                                            else if (list.Contains(pixelEsquerda))
                                            {
                                                list.Add(pixelCima);
                                                introduzido = true;
                                            }
                                            else if (list.Contains(pixelCima))
                                            {
                                                list.Add(pixelEsquerda);
                                                introduzido = true;
                                            }
                                        }

                                        if (!introduzido)
                                        {
                                            List<int> newLabel = new List<int>();
                                            newLabel.Add(pixelEsquerda);
                                            newLabel.Add(pixelCima);
                                            labels.Add(newLabel);
                                        }
                                    }
                                    else
                                    {
                                        valorATomar = pixelCima;
                                    }
                                }
                                else valorATomar = pixelEsquerda;
                            }
                            else
                            {
                                if (pixelCima != 0)
                                {
                                    valorATomar = pixelCima;
                                }
                                else
                                {
                                    valorATomar = ++valor;
                                }
                            }
                            matrizIntermedia[indexMatrixInt] = valorATomar;
                        }
                        else
                        {
                            matrizIntermedia[indexMatrixInt] = 0;
                        }

                        index += nChan;
                        indexMatrixInt += 1;
                    }
                    index += (padding + 2 * (grossuraBorda * nChan));
                    indexMatrixInt += 2;
                }

                //*****************************//*****************************//
                // Equalizar as labels
                //*****************************//*****************************//
                // Passar por todas as labels.
                // Ver quais as relações entre as labels e colocar a menor destas
                // Colocar o resultado na matriz de inteiros
                //*****************************//*****************************//

                for (PixelNoDestinoY = 0; PixelNoDestinoY < height; PixelNoDestinoY++)
                {
                    for (PixelNoDestinoX = 0; PixelNoDestinoX < width; PixelNoDestinoX++)
                    {
                        pixel = matrizIntermedia[PixelNoDestinoY * width + PixelNoDestinoX];

                        if (pixel != 0)
                        {
                            foreach (var list in labels)
                            {
                                if (list.Contains(pixel))
                                {
                                    valorATomar = list.Min();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            valorATomar = 255;
                        }

                        matrizIntermedia[PixelNoDestinoY * width + PixelNoDestinoX] = valorATomar;
                    }
                }

                index = 0;

                List<int> listaobjetos = new List<int>();

                byte pixeli;

                //*****************************//*****************************//
                // Exit
                //*****************************//*****************************//
                // Colocar os resultados na imagem
                // Preparar uma lista de inteiros com as labels para retornar
                //*****************************//*****************************//

                for (PixelNoDestinoY = 0; PixelNoDestinoY < height; PixelNoDestinoY++)
                {
                    for (PixelNoDestinoX = 0; PixelNoDestinoX < width; PixelNoDestinoX++)
                    {
                        pixeli = (byte)matrizIntermedia[(width * PixelNoDestinoY) + PixelNoDestinoX];

                        if (!listaobjetos.Contains(pixeli) && pixeli != 255)
                        {
                            listaobjetos.Add(pixeli);
                        }
                        dataPtr[index] = pixeli;
                        dataPtr[index + 1] = pixeli;
                        dataPtr[index + 2] = pixeli;

                        index += nChan;
                    }
                    index += padding;
                }

                return listaobjetos;
            }
        }

        /// <summary>
        /// Deixa apenas os vermelhos da imagem
        /// </summary>
        /// - recebe a imagem a filtrar
        /// <param name="img">Image</param>
        public static void FiltroDeVermelho(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino
                
                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                byte blue, green, red;
                
                int pixelX;
                int pixelY;

                //*****************************//*****************************//
                // Deixar apenas vermelhos
                //*****************************//*****************************//
                // Passar por todos os pixeis da imagem.
                // Ver se a diferença da componente vermelha para com as outras se é maior que 50
                // Se sim colocar esse pixel super vermelho
                // Se não colocar a branco
                //*****************************//*****************************//

                if (nChan == 3) // image in RGB
                {
                    for (pixelY = 0; pixelY < height; pixelY++)
                    {
                        for (pixelX = 0; pixelX < width; pixelX++)
                        {
                            //retrieve 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            if (((red - blue > 50) && (red - green > 50)))
                            {
                                dataPtr[0] = (byte)0;
                                dataPtr[1] = (byte)0;
                                dataPtr[2] = (byte)255;
                                // if(Math.Abs(red - blue) < 30 && Math.Abs(red - green) < 30)
                            }
                            else
                            {
                                dataPtr[0] = (byte)255;
                                dataPtr[1] = (byte)255;
                                dataPtr[2] = (byte)255;
                            }

                            //Endereçamento absoluto
                            dataPtr += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Se o cinzento do pixel for > 70 entao branco
        /// Se o cinzento do pixel for > 70 entao cinzento
        /// </summary>
        /// - recebe a imagem a filtrar
        /// <param name="img">Image</param>
        public static void FiltroDeCor(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                byte blue, green, red;
                byte gray;

                int pixelX;
                int pixelY;

                //*****************************//*****************************//
                // Retirar a cor da imagem
                //*****************************//*****************************//
                // Passar por todos os pixeis da imagem.
                // Ver se o cinzento (soma das 3 componentes) é maior que 70
                // Se sim colocar esse pixel preto
                // Se não colocar a branco
                //*****************************//*****************************//

                if (nChan == 3) // image in RGB
                {
                    for (pixelY = 0; pixelY < height; pixelY++)
                    {
                        for (pixelX = 0; pixelX < width; pixelX++)
                        {
                            //retrieve 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            if (gray > 70)
                            {
                                dataPtr[0] = (byte)255;
                                dataPtr[1] = (byte)255;
                                dataPtr[2] = (byte)255;
                            }
                            else
                            {
                                dataPtr[0] = (byte)gray;
                                dataPtr[1] = (byte)gray;
                                dataPtr[2] = (byte)gray;
                            }

                            //Endereçamento absoluto
                            dataPtr += nChan;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Dilatacao com Mascara 3x3
        /// </summary>
        /// - recebe a imagem a dilatar
        /// <param name="inputImage">Image</param>
        public static void Dilatacao(Image<Bgr, byte> inputImage)
        {
            var tempData = (byte[,,])inputImage.Data.Clone();

            int neighborX;
            int neighborY;

            int x;
            int dx;
            int y;
            int dy;

            //*****************************//*****************************//
            // Dilatador
            //*****************************//*****************************//
            // Passar por todos os pixeis da imagem.
            // Ver se este contem algum na vizinhança preto
            // Se sim colocar o pixel preto
            // Se não colocar a branco
            //*****************************//*****************************//

            // Percorrer cada pixel da imagem
            for (y = 0; y < inputImage.Height; y++)
            {
                for (x = 0; x < inputImage.Width; x++)
                {
                    bool hasBlackNeighbor = false;

                    for (dy = -1; dy <= 1; dy++)
                    {
                        for (dx = -1; dx <= 1; dx++)
                        {
                            neighborX = x + dx;
                            neighborY = y + dy;

                            if ((neighborX >= 0 && neighborX < inputImage.Width) && (neighborY >= 0 && neighborY < inputImage.Height))
                            {
                                if (tempData[neighborY, neighborX, 0] == 0 &&
                                    tempData[neighborY, neighborX, 1] == 0 &&
                                    tempData[neighborY, neighborX, 2] == 0)
                                {
                                    hasBlackNeighbor = true;
                                    break;
                                }
                            }
                        }

                        if (hasBlackNeighbor) break;
                    }

                    // Modificar o pixel diretamente na imagem original
                    if (hasBlackNeighbor)
                    {
                        inputImage.Data[y, x, 0] = 0;
                        inputImage.Data[y, x, 1] = 0;
                        inputImage.Data[y, x, 2] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Recebe a lista de labels
        /// </summary>
        /// Devolve uma lista com um vetor de inteiros que contém:
        ///     a[0] = X canto sup esquerdo;
        ///     a[1] = Y canto sup esquerdo;
        ///     a[2] = Width
        ///     a[3] = Height
        /// <param name="img">Image</param> <param name="listaobjetos">List<int></param> 
        public static List<int[]> EncontrarObjetos(Emgu.CV.Image<Bgr, byte> img, List<int> listaobjetos)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int[] a;

                List<int[]> listaObjetosCortados = new List<int[]>();

                int xMax;
                int xMin;
                int yMax;
                int yMin;
                int index;

                //*****************************//*****************************//
                // Encontra objetos
                //*****************************//*****************************//
                // Passar por todas as labels que recebe na lista de inteiros
                // Vê onde estão contidos e em que coordenadas os objetos com essas labels
                // Devolve um vetor de inteiros com as coordenadas do canto superior esquerdo, comprimento e a altura de cada objeto
                //*****************************//*****************************//

                foreach (var obj in listaobjetos)
                {
                    xMax = int.MinValue;
                    xMin = int.MaxValue;
                    yMax = int.MinValue;
                    yMin = int.MaxValue;

                    a = new int[4];

                    index = 0;
                    for (PixelNoDestinoY = 0; PixelNoDestinoY < height; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < width; PixelNoDestinoX++)
                        {

                            if (dataPtr[index] == obj)
                            {
                                if (PixelNoDestinoX < xMin) xMin = PixelNoDestinoX; // Update xMin
                                if (PixelNoDestinoX > xMax) xMax = PixelNoDestinoX; // Update xMax
                                if (PixelNoDestinoY < yMin) yMin = PixelNoDestinoY; // Update yMin
                                if (PixelNoDestinoY > yMax) yMax = PixelNoDestinoY; // Update yMax
                            }

                            index += nChan;
                        }
                        index += padding;
                    }

                    a[0] = xMin;
                    a[1] = yMin;
                    a[2] = xMax-xMin; // Width
                    a[3] = yMax-yMin; // Height
                    listaObjetosCortados.Add(a);
                }

                return listaObjetosCortados;
            }
        }

        /// <summary>
        /// Recebe a imagem e a lista de objetos e devolve uma lista de objetos filtrada
        /// O filtro está desenhado para encontrar objetos com características de sinal
        /// </summary>
        /// Devolve uma lista com um vetor de inteiros que contém:
        ///     a[0] = X canto sup esquerdo;
        ///     a[1] = Y canto sup esquerdo;
        ///     a[2] = Width
        ///     a[3] = Height
        ///     a[4] = Tipo de sinal
        ///         a[4] = 0 Perigo
        ///         a[4] = 1 Obrigacao ou Limite de velocidade
        ///         a[4] = 2 Obrigacao
        /// <param name="img">Image</param> <param name="listaObjetos">List<int></param> 
        public static List<int[]> FiltrarObjetosProcurarSinal(Emgu.CV.Image<Bgr, byte> img, List<int[]> listaObjetos)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                int area = 0;
                int areaCima = 0;
                int areaBaixo = 0;
                int index = 0;

                bool objectOk;

                int[] a;

                List<int[]> listaObjetosSaida = new List<int[]>();

                double percentagem;

                //*****************************//*****************************//
                // Filtra objetos quando procuramos sinais
                //*****************************//*****************************//
                // Passar por todos os objetos
                // Vê onde estão contidos e em que coordenadas estão os objetos com essas labels
                // Devolve um vetor de inteiros com as coordenadas do canto superior esquerdo, comprimento, altura de cada objeto e tipo de sinal
                // Este filtro verifica:
                //      - Tamanho geral do objeto comparado com o tamanho da imagem
                //      - Se o objeto está contido num quadrado
                // Verifica o tipo de sinal através da:
                //      - Forma do objeto (redondo ou triangular) usando a área da metade de cima e a área da metade de baixo do objeto
                //      - Se o objeto tem parte do sinal no seu centro (alguns sinais de proibição)
                //*****************************//*****************************//

                foreach (var obj in listaObjetos)
                {
                    objectOk = true;
                    a = new int[5];
                    percentagem = ((obj[2] * obj[3]) / (width * (double)height)) * 100;

                    // Tamanho geral do objeto em pixels
                    if (percentagem < 0.5)
                    {
                        objectOk = false;
                    }

                    // Se o objeto está contido num quadrado
                    if (Math.Abs(obj[2] - obj[3]) > 55)
                    {
                        objectOk = false;
                    }
                    
                    if (objectOk)
                    {
                        // Calcular áreas do objeto
                        area = 0;
                        areaCima = 0;
                        areaBaixo = 0;

                        index = obj[0] * nChan + obj[1] * widthTotal;
                        for (PixelNoDestinoY = 0; PixelNoDestinoY < obj[3]; PixelNoDestinoY++)
                        {
                            for (PixelNoDestinoX = 0; PixelNoDestinoX < obj[2]; PixelNoDestinoX++)
                            {

                                if (dataPtr[index] == 0)
                                {
                                    area++;
                                    if (PixelNoDestinoY >= (obj[3] / 2))
                                    {
                                        areaBaixo++;
                                    }
                                    else
                                    {
                                        areaCima++;
                                    }
                                }

                                index += nChan;
                            }
                            index += (widthTotal - obj[2] * nChan);
                        }
                        // Forma do objeto(redondo ou triangular) usando a área da metade de cima e a área da metade de baixo do objeto
                        if (Math.Abs(areaCima - areaBaixo) > area / 10)
                        {
                            a[4] = 0; // perigo
                        }
                        else
                        {
                            // Se o objeto tem parte do sinal no seu centro(alguns sinais de proibição)
                            if (dataPtr[obj[2] / 2 * nChan + obj[3] / 2 * widthTotal] == 0)
                            {
                                a[4] = 2; // Obrigacao
                            }
                            else
                            {
                                a[4] = 1; // Obrigacao ou Limite de velocidade
                            }
                        }

                        a[0] = obj[0];
                        a[1] = obj[1];
                        a[2] = obj[2];
                        a[3] = obj[3];
                        listaObjetosSaida.Add(a);
                    }
                }

                return listaObjetosSaida;
            }
        }

        /// <summary>
        /// Recebe a imagem e a lista de objetos e devolve uma lista de objetos filtrada
        /// O filtro está desenhado para encontrar objetos com características de digito
        /// </summary>
        /// Devolve uma lista com um vetor de inteiros que contém:
        ///     a[0] = X canto sup esquerdo;
        ///     a[1] = Y canto sup esquerdo;
        ///     a[2] = Width
        ///     a[3] = Height
        /// <param name="img">Image</param> <param name="listaObjetos">List<int></param> 
        public static List<int[]> FiltrarObjetosDentroSinal(Emgu.CV.Image<Bgr, byte> img, List<int[]> listaObjetos)
        {
            unsafe
            {

                MIplImage m1 = img.MIplImage;
                byte* dataPtr = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                int width = img.Width;
                int height = img.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                bool objectOk;

                List<int[]> listaObjetosSaida = new List<int[]>();

                double percentagem;

                //*****************************//*****************************//
                // Filtra objetos quando procuramos digitos
                //*****************************//*****************************//
                // Passar por todos os objetos
                // Vê onde estão contidos e em que coordenadas estão os objetos com essas labels
                // Devolve um vetor de inteiros com as coordenadas do canto superior esquerdo, comprimento, altura de cada objeto
                // Este filtro verifica:
                //      - Tamanho geral do objeto comparado com o tamanho da imagem
                //      - Se o objeto tem mais altura que comprimento
                //      - Se o objeto está centrado na imagem
                //*****************************//*****************************//

                foreach (var obj in listaObjetos)
                {
                    objectOk = true;

                    // Ver se comprimento maior que altura
                    if (obj[3] - obj[2] < 0) 
                    {
                        objectOk = false;
                    }
                    
                    percentagem = ((obj[2] * obj[3]) / (width * (double)height)) * 100;
                    
                    // Tamanho geral do objeto em pixels
                    if (percentagem < 2)
                    {
                        objectOk = false;
                    }

                    // Objeto centrado na imagem
                    if (obj[1] < height / 4 || obj[1] > 3*(height / 4))
                    {
                        objectOk = false;
                    }

                    if (objectOk)
                    {
                        listaObjetosSaida.Add(obj);
                    }
                }

                return listaObjetosSaida;
            }
        }

        /// <summary>
        /// Recebe a imagem do digito (binarizada) e deteta qual o digito presente nela comparando-o com uma bateria de imagens
        /// </summary>
        /// Devolve o inteiro correspondente ao digito
        /// <param name="img">Image</param>
        public static int CompararDigito(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                //Image<Bgr, byte> img0 = new Image<Bgr, byte>("C:\\ss\\SS_OpenCV_Base\\Imagens\\digitos\\0.png");
                
                String path;
                char[] pathChars;
                Image<Bgr, byte> imgNumber;

                int melhorComparacao = 0;
                int comparacao = 0;
                int digito= 0;

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.ImageData.ToPointer(); // Pointer to the image
                int height = m.Height; 
                int width = m.Width;
                int nChan = m.NChannels;
                int padding = m.WidthStep - m.NChannels * m.Width; // alinhament bytes (padding)

                MIplImage mnum;
                byte* dataPtrNum;

                int PixelNoDestinoY;
                int PixelNoDestinoX;
                int index;

                //*****************************//*****************************//
                // Comparador de digitos
                //*****************************//*****************************//
                // Pega em cada imagem da bateria de digitos e dá resize e binariza
                // Faz comparação direta e o digito onde houve mais pixeis iguais é considerado o digito presente na imagem
                //*****************************//*****************************//

                for (int i = 0; i < 10; i++)
                {
                    path = "C:\\ss\\SS_OpenCV_Base\\Imagens\\digitos\\0.png";
                    pathChars = path.ToCharArray();
                    pathChars[path.LastIndexOf('0')] = (char)('0' + i);
                    path = new string(pathChars);

                    imgNumber = new Image<Bgr, byte>(path);
                    imgNumber = imgNumber.Resize(width, height, Inter.Linear);
                    ConvertToBW_Otsu(imgNumber);

                    mnum = imgNumber.MIplImage;
                    dataPtrNum = (byte*)mnum.ImageData.ToPointer(); // Pointer to the image
                    
                    index = 0;
                    comparacao = 0;

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < height; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < width; PixelNoDestinoX++)
                        {
                            
                            if (dataPtr[index] == dataPtrNum[index])
                            {
                                comparacao ++;
                            }

                            index += nChan;
                        }
                        index += padding;
                    }

                    if (comparacao > melhorComparacao)
                    {
                        melhorComparacao = comparacao;
                        digito = i;
                    }
                }
                Console.WriteLine(digito);
                return digito;
            }
        }

        /// <summary>
        /// Sinal Reader
        /// </summary>
        /// <param name="imgDest">imagem de destino (cópia da original)</param>
        /// <param name="imgOrig">imagem original </param>
        /// <param name="level">nivel de dificuldade da imagem</param>
        /// <param name="sinalResult">Objecto resultado - lista de sinais e respectivas informaçoes</param>
        public static void SinalReader(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig, int level, out Results sinalResult)
        {
            unsafe
            {

                sinalResult = new Results();
                
                List<int[]> objetosSinal = new List<int[]>();
                List<int[]> objetosSinalFiltrados = new List<int[]>();
                List<int> labelsSinal = new List<int>();

                List<int[]> objetosDigitos = new List<int[]>();
                List<int[]> objetosDigitosFiltrados = new List<int[]>();
                List<int> labelsDigitos = new List<int>();

                Emgu.CV.Image<Bgr, byte> workingImage;
                Emgu.CV.Image<Bgr, byte> workingImage2;
                Emgu.CV.Image<Bgr, byte> workingSignal1;
                Emgu.CV.Image<Bgr, byte> workingSignal2;

                Bgr cor = new Bgr();
                cor.Green = 0;
                cor.Red = 255;
                cor.Blue = 0;

                Rectangle rect; // x, y, width, height

                workingImage = imgOrig.Copy();

                FiltroDeVermelho(workingImage);

                ConvertToBW_Otsu(workingImage);

                workingImage2 = workingImage.Copy();

                labelsSinal = ConectedComponentsAlgIter(workingImage);

                objetosSinal = EncontrarObjetos(workingImage, labelsSinal);

                objetosSinalFiltrados = FiltrarObjetosProcurarSinal(workingImage2, objetosSinal);

                foreach (var obj in objetosSinalFiltrados)
                {
                    Sinal sinal = new Sinal();

                    rect = new Rectangle(obj[0], obj[1], obj[2], obj[3]); // x, y, width, height
                    sinal.sinalRect = rect; // RETANGULO DO SINAL
                    imgDest.Draw(rect, cor, 1);

                    if (obj[4] == 0)
                    {
                        sinal.sinalEnum = ResultsEnum.sinal_perigo; // Perigo
                    } else
                    {
                        workingSignal1 = imgOrig.Copy(rect);
                        FiltroDeCor(workingSignal1);
                        ConvertToBW_Otsu(workingSignal1);
                        
                        if (level == 2) // Para sinais com 3 números
                        {
                            Dilatacao(workingSignal1);
                        }
                        
                        workingSignal2 = workingSignal1.Copy();

                        labelsDigitos = ConectedComponentsAlgIter(workingSignal2);

                        objetosDigitos = EncontrarObjetos(workingSignal2, labelsDigitos);

                        objetosDigitosFiltrados = FiltrarObjetosDentroSinal(workingSignal2, objetosDigitos);

                        if (objetosDigitosFiltrados.Count > 1 && obj[4] == 1) // && obj[4] == 1
                        {
                            sinal.sinalEnum = ResultsEnum.sinal_limite_velocidade; // Velocidade
                            foreach (var digitoIMG in objetosDigitosFiltrados)
                            {
                                Digito digito = new Digito();
                                int digitoNum;
                                rect = new Rectangle(digitoIMG[0] + obj[0], digitoIMG[1] + obj[1], digitoIMG[2], digitoIMG[3]); // x, y, width, height
                                imgDest.Draw(rect, cor, 1);
                                digito.digitoRect = rect; // RETANGULO DO DIGITO
                                rect = new Rectangle(digitoIMG[0], digitoIMG[1], digitoIMG[2], digitoIMG[3]); // x, y, width, height
                                digitoNum = CompararDigito(workingSignal1.Copy(rect));
                                digito.digito = digitoNum.ToString(); // VALOR DO DIGITO // Transformar o inteiro em string
                                sinal.digitos.Add(digito);
                            }
                        } else
                        {
                            sinal.sinalEnum = ResultsEnum.sinal_proibicao; // Obrigacao
                        }
                    }

                    sinalResult.results.Add(sinal);
                }
            }
        }

    }
}
