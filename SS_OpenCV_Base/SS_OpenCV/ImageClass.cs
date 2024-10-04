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
                
                MIplImage m2 = imgOrig.MIplImage;
                byte* dataPtrOrigem = (byte*)m2.ImageData.ToPointer(); // Pointer to the imagem origem

                int width = imgDest.Width;
                int height = imgDest.Height;
                int nChan = m1.NChannels; // number of channels - 3
                int padding = m1.WidthStep - m1.NChannels * m1.Width; // alinhament bytes (padding)
                int widthTotal = m1.WidthStep;

                int PixelNoDestinoX, PixelNoDestinoY;

                byte* dataPtrAjuda;

                int pixelBorda;

                int raioFiltro = 3; // Por ser 3 x 3
                int grossuraBorda = (raioFiltro - 1) / 2;
                int channel;
                double somaChannels;
                int borda;
                int index;

                // LOOP NO Y
                int loopCoreXLim = width - 1 - grossuraBorda;
                int loopCoreYLim = height - 1 - grossuraBorda;
                int loopBorderXLim;
                int loopBorderYLim;


                //Border
                for (borda = 1; borda <= grossuraBorda; borda ++)
                {
                    loopBorderXLim = loopCoreXLim + borda;
                    loopBorderYLim = loopCoreYLim + borda;

                    //Borda de cima
                    index = (nChan * borda) + ((borda - 1) * widthTotal);
                    for (pixelBorda = 0 + borda; pixelBorda < loopBorderXLim; pixelBorda++)
                    {
                        for (channel = 0; channel < raioFiltro; channel++)
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
                    index = (nChan * borda) + (height - borda) * widthTotal;
                    for (pixelBorda = 0 + borda; pixelBorda < loopBorderXLim; pixelBorda++)
                    {
                        for (channel = 0; channel < raioFiltro; channel++)
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
                    index = (nChan * (borda - 1)) + (borda * widthTotal);
                    for (pixelBorda = 0 + borda; pixelBorda < loopBorderYLim; pixelBorda++)
                    {
                        for (channel = 0; channel < raioFiltro; channel++)
                        {
                            // Ele próprio e reset
                            somaChannels = 2 * dataPtrOrigem[index + channel];
                            // Direita
                            somaChannels += dataPtrOrigem[index + nChan + channel];
                            // Linha de cima
                            somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                            somaChannels += dataPtrOrigem[index - widthTotal + nChan + channel];
                            // Linha de baixo
                            somaChannels += dataPtrOrigem[index - widthTotal + channel];
                            somaChannels += 2 * dataPtrOrigem[index - widthTotal + nChan + channel];

                            dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                        }
                        index += widthTotal;
                    }

                    // Borda da direita
                    index = ((width - borda) * nChan) + (borda * widthTotal);
                    for (pixelBorda = 0 + borda; pixelBorda < loopBorderYLim; pixelBorda++)
                    {
                        for (channel = 0; channel < raioFiltro; channel++)
                        {
                            // Ele próprio e reset
                            somaChannels = 2 * dataPtrOrigem[index + channel];
                            // Esquerda
                            somaChannels += dataPtrOrigem[index - nChan + channel];
                            // Linha de cima
                            somaChannels += 2 * dataPtrOrigem[index - widthTotal + channel];
                            somaChannels += dataPtrOrigem[index - widthTotal - nChan + channel];
                            // Linha de baixo
                            somaChannels += dataPtrOrigem[index - widthTotal + channel];
                            somaChannels += 2 * dataPtrOrigem[index - widthTotal - nChan + channel];

                            dataPtrDestino[index + channel] = (byte)Math.Round(somaChannels / 9.0);
                        }
                        index += widthTotal;
                    }
                }

                /*
                //Borda as pontas
                dataPtrAjuda = dataPtrOrigem + (nChan * widthTotal) + (width - grossuraBorda);
                dataPtrDestino1 = dataPtrDestino + (nChan * widthTotal) + (width - grossuraBorda);
                
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
                    somaChannels[channel] += 2 * (dataPtrAjuda + nChan)[channel];
                    // Linha de baixo
                    somaChannels[channel] += 2 * (dataPtrAjuda + widthTotal)[channel];
                    somaChannels[channel] += (dataPtrAjuda + widthTotal + nChan)[channel];

                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                
                //Canto sup direito
                dataPtrAjuda += width;
                dataPtrDestino1 += width;

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda - nChan)[channel];
                    // Linha de baixo
                    somaChannels[channel] += 2 * (dataPtrAjuda + widthTotal)[channel];
                    somaChannels[channel] += (dataPtrAjuda + widthTotal - nChan)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);
                /*
                //Canto inf direito
                dataPtrAjuda += (height * widthTotal);
                dataPtrDestino1 += (height * widthTotal);

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda - nChan)[channel];
                    // Linha de cima
                    somaChannels[channel] += 2 * (dataPtrAjuda - widthTotal)[channel];
                    somaChannels[channel] += (dataPtrAjuda - widthTotal - nChan)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                //Canto inf esquerdo
                dataPtrAjuda -= width;
                dataPtrDestino1 -= width;

                for (channel = 0; channel < 3; channel++)
                {
                    //Reset
                    somaChannels[channel] = 0;

                    // Ele próprio
                    somaChannels[channel] += 4 * dataPtrAjuda[channel];
                    //Esquerda direita
                    somaChannels[channel] += 2 * (dataPtrAjuda + nChan)[channel];
                    // Linha de cima
                    somaChannels[channel] += 2 * (dataPtrAjuda - widthTotal)[channel];
                    somaChannels[channel] += (dataPtrAjuda - widthTotal + nChan)[channel];
                }

                dataPtrDestino1[0] = (byte)Math.Round(somaChannels[0] / 9.0);
                dataPtrDestino1[1] = (byte)Math.Round(somaChannels[1] / 9.0);
                dataPtrDestino1[2] = (byte)Math.Round(somaChannels[2] / 9.0);

                  

                //Core
                if (nChan == 3) // image in RGB
                {
                    dataPtrAjuda = dataPtrOrigem + (grossuraBorda * widthTotal) + (grossuraBorda * nChan);
                    dataPtrDestino += (grossuraBorda * widthTotal) + (grossuraBorda * nChan);

                    for (PixelNoDestinoY = 0; PixelNoDestinoY < loopCoreYLim; PixelNoDestinoY++)
                    {
                        for (PixelNoDestinoX = 0; PixelNoDestinoX < loopCoreXLim; PixelNoDestinoX++)
                        {
                            
                            for (channel = 0; channel < 3; channel++)
                            {
                                // Ele próprio com reset
                                somaChannels = dataPtrAjuda[channel];
                                //Esquerda direita
                                somaChannels += (dataPtrAjuda + nChan)[channel];
                                somaChannels += (dataPtrAjuda - nChan)[channel];
                                // Linha de cima
                                somaChannels += (dataPtrAjuda - widthTotal - nChan)[channel];
                                somaChannels += (dataPtrAjuda - widthTotal)[channel];
                                somaChannels += (dataPtrAjuda - widthTotal + nChan)[channel];
                                // Linha de baixo
                                somaChannels += (dataPtrAjuda + widthTotal - nChan)[channel];
                                somaChannels += (dataPtrAjuda + widthTotal)[channel];
                                somaChannels += (dataPtrAjuda + widthTotal + nChan)[channel];

                                dataPtrDestino[channel] = (byte)Math.Round(somaChannels / 9.0);

                            }

                            // advance the pointer to the next pixel
                            dataPtrDestino += nChan;
                            dataPtrAjuda += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtrDestino += (padding + 2 * (grossuraBorda * nChan));
                        dataPtrAjuda += (padding + 2 * (grossuraBorda * nChan));

                    }
                }
                */
            }
        }
    }

}
