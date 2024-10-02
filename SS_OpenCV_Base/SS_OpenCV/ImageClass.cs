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

                int widthDestino = imgDestino.Width;
                int heightDestino = imgDestino.Height;
                int widthOrigem = imgOrigem.Width;
                int heightOrigem = imgOrigem.Height;
                int nChanDestino = m1.NChannels; // number of channels - 3
                int paddingDestino = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotalDestino = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                if (nChanDestino == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < heightDestino; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < widthDestino; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = pixelNoDestinoX - dx;
                            PixelNaOrigemY = pixelNoDestinoY - dy;

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= widthOrigem) || (PixelNaOrigemY >= heightOrigem) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotalDestino) + (PixelNaOrigemX * nChanDestino);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChanDestino;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += paddingDestino;
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

                int widthDestino = imgDestino.Width;
                int heightDestino = imgDestino.Height;
                int widthOrigem = imgOrigem.Width;
                int heightOrigem = imgOrigem.Height;
                int nChanDestino = m1.NChannels; // number of channels - 3
                int paddingDestino = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotalDestino = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                double halfWidthT = widthOrigem / 2.0;
                double halfHeight = heightOrigem / 2.0;
                double cosAngle = Math.Cos(angle);
                double sinAngle = Math.Sin(angle);

                if (nChanDestino == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < heightDestino; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < widthDestino; pixelNoDestinoX++)
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
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotalDestino) + (PixelNaOrigemX * nChanDestino);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChanDestino;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += paddingDestino;
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

                int widthDestino = imgDestino.Width;
                int heightDestino = imgDestino.Height;
                int widthOrigem = imgOrigem.Width;
                int heightOrigem = imgOrigem.Height;
                int nChanDestino = m1.NChannels; // number of channels - 3
                int paddingDestino = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotalDestino = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                if (nChanDestino == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < heightDestino; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < widthDestino; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = (int)Math.Round(pixelNoDestinoX/scaleFactor); 
                            PixelNaOrigemY = (int)Math.Round(pixelNoDestinoY/scaleFactor);

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= widthOrigem) || (PixelNaOrigemY >= heightOrigem) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotalDestino) + (PixelNaOrigemX * nChanDestino);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChanDestino;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += paddingDestino;
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

                int widthDestino = imgDestino.Width;
                int heightDestino = imgDestino.Height;
                int widthOrigem = imgOrigem.Width;
                int heightOrigem = imgOrigem.Height;
                int nChanDestino = m1.NChannels; // number of channels - 3
                int paddingDestino = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotalDestino = m1.WidthStep;
                int pixelNoDestinoX, pixelNoDestinoY;

                int PixelNaOrigemX;
                int PixelNaOrigemY;

                byte* dataPtrAjuda;

                double halfWidthT = widthOrigem / 2.0;
                double halfHeight = heightOrigem / 2.0;

                double scalePlacementX = (widthOrigem / 2 ) / scaleFactor;
                double scalePlacementY = (heightOrigem / 2 ) / scaleFactor;

                if (nChanDestino == 3) // image in RGB
                {
                    for (pixelNoDestinoY = 0; pixelNoDestinoY < heightDestino; pixelNoDestinoY++)
                    {
                        for (pixelNoDestinoX = 0; pixelNoDestinoX < widthDestino; pixelNoDestinoX++)
                        {
                            PixelNaOrigemX = (int)Math.Round((pixelNoDestinoX / scaleFactor) - scalePlacementX + centerX);
                            PixelNaOrigemY = (int)Math.Round((pixelNoDestinoY / scaleFactor) - scalePlacementY + centerY);

                            // Fórmula
                            //PixelNaOrigemX = (int)Math.Round((pixelNoDestinoX / scaleFactor) - (widthOrigem / (2.0 * scaleFactor) ) + centerX);
                            //PixelNaOrigemY = (int)Math.Round((pixelNoDestinoY / scaleFactor) - (heightOrigem / (2.0 * scaleFactor) ) + centerY);

                            if ((PixelNaOrigemX < 0) || (PixelNaOrigemX >= widthOrigem) || (PixelNaOrigemY >= heightOrigem) || (PixelNaOrigemY < 0))
                            {
                                dataPtrDestino[0] = 0;
                                dataPtrDestino[1] = 0;
                                dataPtrDestino[2] = 0;
                            }
                            else
                            {
                                //Endereçamento absoluto
                                dataPtrAjuda = dataPtrOrigem + (PixelNaOrigemY * widthTotalDestino) + (PixelNaOrigemX * nChanDestino);
                                //retrieve 3 colour components
                                dataPtrDestino[0] = (byte)dataPtrAjuda[0];
                                dataPtrDestino[1] = (byte)dataPtrAjuda[1];
                                dataPtrDestino[2] = (byte)dataPtrAjuda[2];
                            }

                            //Endereçamento absoluto
                            dataPtrDestino += nChanDestino;
                        }
                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += paddingDestino;
                    }
                }
            }
        }

        /// <summary>
        /// Aplica zoom
        /// scaleFactor
        /// </summary>
        /// <param name="img">Image</param>
        public static void Mean(Image<Bgr, byte> imgDest, Image<Bgr, byte> imgOrig)
        {
            unsafe
            {

                MIplImage m1 = imgDest.MIplImage;
                byte* dataPtrDestino = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino
                byte* dataPtrDestino1 = (byte*)m1.ImageData.ToPointer(); // Pointer to the imagem destino

                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int widthDestino = imgDest.Width;
                int heightDestino = imgDest.Height;
                int widthOrigem = imgOrig.Width;
                int heightOrigem = imgOrig.Height;
                int nChanDestino = m1.NChannels; // number of channels - 3
                int paddingDestino = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotalDestino = m1.WidthStep;

                int PixelNoDestinoX;
                int PixelNoDestinoY;

                byte* dataPtrAjuda;

                int pixelBorda;

                int grossuraBorda = 1; // Por ser 3x3
                // LOOP NO Y
                int loopCoreXLim = widthDestino - 2;
                // LOOP NO X
                int loopCoreYLim = heightDestino - 2;

                int channel;

                double[] somaChannels = new double[3];

                //Border
                
                //Borda de cima
                dataPtrAjuda = dataPtrOrigem + (grossuraBorda * nChanDestino);
                dataPtrDestino1 = dataPtrDestino + (grossuraBorda * nChanDestino);
                for (pixelBorda = 0; pixelBorda < loopCoreXLim; pixelBorda++)
                {
                    
                    for (channel = 0; channel < 3; channel++)
                    {
                        //Reset
                        somaChannels[channel] = 0;

                        // Ele próprio
                        somaChannels[channel] += 2 * dataPtrAjuda[channel];
                        //Esquerda direita
                        somaChannels[channel] += 2 * (dataPtrAjuda + nChanDestino)[channel];
                        somaChannels[channel] += 2 * (dataPtrAjuda - nChanDestino)[channel];
                        // Linha de baixo
                        somaChannels[channel] += (dataPtrAjuda + widthTotalDestino - nChanDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda + widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda + widthTotalDestino + nChanDestino)[channel];

                    }
                    
                    dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                    dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                    dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                    dataPtrDestino1 += nChanDestino;
                    dataPtrAjuda += nChanDestino;
                }
                
                //Borda de baixo
                dataPtrAjuda = dataPtrOrigem + (grossuraBorda * nChanDestino) + ((heightDestino - 1)  * widthTotalDestino);
                dataPtrDestino1 = dataPtrDestino + (grossuraBorda * nChanDestino) + ((heightDestino - 1) * widthTotalDestino);
                for (pixelBorda = 0; pixelBorda < loopCoreXLim; pixelBorda++)
                {

                    for (channel = 0; channel < 3; channel++)
                    {
                        //Reset
                        somaChannels[channel] = 0;

                        // Ele próprio
                        somaChannels[channel] += 2 * dataPtrAjuda[channel];
                        //Esquerda direita
                        somaChannels[channel] += 2 * (dataPtrAjuda + nChanDestino)[channel];
                        somaChannels[channel] += 2 * (dataPtrAjuda - nChanDestino)[channel];
                        // Linha de cima
                        somaChannels[channel] += (dataPtrAjuda - widthTotalDestino - nChanDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda - widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda - widthTotalDestino + nChanDestino)[channel];

                    }

                    dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                    dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                    dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                    dataPtrDestino1 += nChanDestino;
                    dataPtrAjuda += nChanDestino;
                }
                
                //Borda da esquerda
                dataPtrAjuda = dataPtrOrigem + (grossuraBorda * widthTotalDestino);
                dataPtrDestino1 = dataPtrDestino + (grossuraBorda * widthTotalDestino);
                for (pixelBorda = 0; pixelBorda < loopCoreYLim + 1; pixelBorda++)
                {

                    for (channel = 0; channel < 3; channel++)
                    {
                        // Reset
                        somaChannels[channel] = 0;

                        // Ele próprio
                        somaChannels[channel] += 2 * dataPtrAjuda[channel];
                        // Esquerda direita
                        somaChannels[channel] += (dataPtrAjuda + nChanDestino)[channel];
                        // Linha de cima
                        somaChannels[channel] += 2 *(dataPtrAjuda - widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda - widthTotalDestino + nChanDestino)[channel];
                        // Linha de baixo
                        somaChannels[channel] += 2 *(dataPtrAjuda + widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda + widthTotalDestino + nChanDestino)[channel];

                    }

                    dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                    dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                    dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                    dataPtrDestino1 += widthTotalDestino;
                    dataPtrAjuda += widthTotalDestino;
                }

                //Borda da direta
                dataPtrAjuda = dataPtrOrigem + (grossuraBorda * widthTotalDestino) + (widthDestino - grossuraBorda);
                dataPtrDestino1 = dataPtrDestino + (grossuraBorda * widthTotalDestino) + (widthDestino - grossuraBorda);
                for (pixelBorda = 0; pixelBorda < loopCoreYLim + 1; pixelBorda++)
                {

                    for (channel = 0; channel < 3; channel++)
                    {
                        //Reset
                        somaChannels[channel] = 0;

                        // Ele próprio
                        somaChannels[channel] += 2 * dataPtrAjuda[channel];
                        //Esquerda direita
                        somaChannels[channel] += (dataPtrAjuda - nChanDestino)[channel];
                        // Linha de cima
                        somaChannels[channel] += 2 * (dataPtrAjuda - widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda - widthTotalDestino - nChanDestino)[channel];
                        // Linha de baixo
                        somaChannels[channel] += 2 * (dataPtrAjuda + widthTotalDestino)[channel];
                        somaChannels[channel] += (dataPtrAjuda + widthTotalDestino - nChanDestino)[channel];

                    }

                    dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                    dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                    dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                    
                    dataPtrDestino1[0] = (byte)0;
                    dataPtrDestino1[1] = (byte)255;
                    dataPtrDestino1[2] = (byte)0;

                    dataPtrDestino1 += widthTotalDestino;
                    dataPtrAjuda += widthTotalDestino;
                }

                //Borda as pontas
                dataPtrAjuda = dataPtrOrigem + (nChanDestino * widthTotalDestino) + (widthDestino - grossuraBorda);
                dataPtrDestino1 = dataPtrDestino + (nChanDestino * widthTotalDestino) + (widthDestino - grossuraBorda);
                
                //Canto sup esquerdo
                dataPtrAjuda = dataPtrOrigem;
                dataPtrDestino1 = dataPtrDestino;

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda + nChanDestino)[channel];
                    // Linha de baixo
                    somaChannels[channel] += 2 * (dataPtrAjuda + widthTotalDestino)[channel];
                    somaChannels[channel] += (dataPtrAjuda + widthTotalDestino + nChanDestino)[channel];

                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                
                //Canto sup direito
                dataPtrAjuda += widthDestino;
                dataPtrDestino1 += widthDestino;

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda - nChanDestino)[channel];
                    // Linha de baixo
                    somaChannels[channel] += 2 * (dataPtrAjuda + widthTotalDestino)[channel];
                    somaChannels[channel] += (dataPtrAjuda + widthTotalDestino - nChanDestino)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                /*
                //Canto inf direito
                dataPtrAjuda += (heightDestino * widthTotalDestino);
                dataPtrDestino1 += (heightDestino * widthTotalDestino);

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda - nChanDestino)[channel];
                    // Linha de cima
                    somaChannels[channel] += 2 * (dataPtrAjuda - widthTotalDestino)[channel];
                    somaChannels[channel] += (dataPtrAjuda - widthTotalDestino - nChanDestino)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                //Canto inf esquerdo
                dataPtrAjuda -= widthDestino;
                dataPtrDestino1 -= widthDestino;

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda + nChanDestino)[channel];
                    // Linha de cima
                    somaChannels[channel] += 2 * (dataPtrAjuda - widthTotalDestino)[channel];
                    somaChannels[channel] += (dataPtrAjuda - widthTotalDestino + nChanDestino)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                */  

                //Core
                if (nChanDestino == 3) // image in RGB
                {
                    dataPtrAjuda = dataPtrOrigem + (grossuraBorda * widthTotalDestino) + (grossuraBorda * nChanDestino);
                    dataPtrDestino += (grossuraBorda * widthTotalDestino) + (grossuraBorda * nChanDestino);

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {
                            
                            for (channel = 0; channel < 3; channel++)
                            {
                                //Reset
                                somaChannels[channel] = 0;

                                // Ele próprio
                                somaChannels[channel] += dataPtrAjuda[channel];
                                //Esquerda direita
                                somaChannels[channel] += (dataPtrAjuda + nChanDestino)[channel];
                                somaChannels[channel] += (dataPtrAjuda - nChanDestino)[channel];
                                // Linha de cima
                                somaChannels[channel] += (dataPtrAjuda - widthTotalDestino - nChanDestino)[channel];
                                somaChannels[channel] += (dataPtrAjuda - widthTotalDestino)[channel];
                                somaChannels[channel] += (dataPtrAjuda - widthTotalDestino + nChanDestino)[channel];
                                // Linha de baixo
                                somaChannels[channel] += (dataPtrAjuda + widthTotalDestino - nChanDestino)[channel];
                                somaChannels[channel] += (dataPtrAjuda + widthTotalDestino)[channel];
                                somaChannels[channel] += (dataPtrAjuda + widthTotalDestino + nChanDestino)[channel];

                            }
                            //Calculate the negative (newChannelValue = 255 - oldChannelValue) and store in the image

                            dataPtrDestino[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                            dataPtrDestino[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                            dataPtrDestino[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                            

                            // advance the pointer to the next pixel
                            dataPtrDestino += nChanDestino;
                            dataPtrAjuda += nChanDestino;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += (paddingDestino + 2 * (grossuraBorda * nChanDestino));
                        dataPtrAjuda += (paddingDestino + 2 * (grossuraBorda * nChanDestino));

                    }
                }
            }
        }
    }

}
