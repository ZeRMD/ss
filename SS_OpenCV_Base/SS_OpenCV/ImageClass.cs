﻿using System;
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
        public static void Mean_solutionB(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig){
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
        ///  Mediana com OpenCV
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

    }



}
