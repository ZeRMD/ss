using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ResultsDLL
{


    public enum ResultsEnum
    {
        unknown,
        sinal_perigo,
        sinal_limite_velocidade,
        sinal_proibicao
    };

    /// <summary>
    /// Guarda informação de um digito
    /// </summary>
    public class Digito
    {
        string delimeterD = "%";
        public Rectangle digitoRect; //Rectangulo que envolve o digito
        public string digito; //  digito (texto)
        public int area; // auxiliar apenas para filtrar pequenos objectos

        public Digito()
        {
        }

        //Para ajustar o retangulo para as coordenadas da imagem original
        public void AddRef(Point p)
        {
            digitoRect.Offset(p);   
        }
            public Digito(string digitoIn)
        {
            char[] delimitersRect = { ',', '=' };
            string[] infos = digitoIn.Split(delimeterD.ToCharArray());
            string[] rectStr = infos[1].Substring(1, infos[1].Length - 2).Split(delimitersRect);
            digito = infos[0];
            digitoRect = new Rectangle(int.Parse(rectStr[1]), int.Parse(rectStr[3]), int.Parse(rectStr[5]), int.Parse(rectStr[7]));
        }


        public string ToString()
        {
            StringBuilder sbDigitos = new StringBuilder();
            

            return  digito + delimeterD + digitoRect.ToString();
        }

    }
    /// <summary>
    /// Guarda informação de um sinal
    /// </summary>
    public class Sinal
    {
        string delimeterA = "$";
        string delimeterB = "&";

        //nivel 1
        public ResultsEnum sinalEnum = ResultsEnum.unknown; // identificação do tipo de sinal
        public Rectangle sinalRect; //Rectangulo que envolve o sinal
        public int area; // para filtrar objetos de pequenas dimensões
        
        //nivel 3
        public List<Digito> digitos = new List<Digito>();  // lista de digitos 

        

        /// <summary>
        /// simple constructor
        /// </summary>
        public Sinal() { }

        /// <summary>
        /// load constructor
        /// </summary>
        /// <param name="info"></param>
        public Sinal(string info)
        {
            string[] infos = info.Split(delimeterA.ToCharArray());

            //tipo de sinal
            sinalEnum = (ResultsEnum)Enum.Parse(typeof(ResultsEnum), infos[0].Remove(0,1));

            //rectangle 
            char[] delimitersRect = { ',', '=' };
            string[] rectStr = infos[1].Substring(1, infos[1].Length-2).Split(delimitersRect);
            sinalRect = new Rectangle(int.Parse(rectStr[1]), int.Parse(rectStr[3]), int.Parse(rectStr[5]), int.Parse(rectStr[7]));


            if ((infos.Length >= 2) && infos[2] != "")
            {
                //lista de digitos
                string[] digitosStr = infos[2].Substring(1, infos[2].Length - 3).Split(delimeterB.ToCharArray());

                foreach (string digitoStr in digitosStr)
                {
                    if (digitoStr == "")
                        continue;
                    Digito digitoObj = new Digito(digitoStr.Substring(1, digitoStr.Length-2));
                    digitos.Add(digitoObj);

                }
            }

        }

        public override string  ToString()
        {

            StringBuilder sb = new StringBuilder();

            StringBuilder sbDigitos = new StringBuilder();
            foreach (Digito dig in digitos)
            {
                sbDigitos.Append("(" + dig.ToString() + ")" + delimeterB);
            }
            
            string digStr = digitos.Count>0 ?  delimeterA +"{" + sbDigitos.ToString() + "}":"";

            return "(" + sinalEnum.ToString() + delimeterA + sinalRect.ToString() +
                digStr;

           
            
        }


        public string ToString_SS_Files()
        {
            StringBuilder sbDigitos = new StringBuilder();
            foreach (Digito dig in digitos)
            {
                sbDigitos.Append("(" + dig.ToString() + ")" + delimeterB);
            }


            string digStr = digitos.Count > 0 ? delimeterA + "{" + sbDigitos.ToString() + "}" : "";

            return "(" + sinalEnum.ToString() + delimeterA + sinalRect.ToString() + digStr;
        }
    }


    /// <summary>
    /// Guarda a lista de sinais 
    /// </summary>
    public class Results
    {
        string delimeter = "|";
        public List<Sinal> results = new List<Sinal>();


        public override string ToString()
        {
            string result = "";
            foreach (Sinal sinal in results)
            {
                result += sinal.ToString() + delimeter;
            }
            return result ;
        }

        public string ToString_SS_Eval()
        {
            string result = "";
            foreach (Sinal sinal in results)
            {
                result += sinal.ToString_SS_Files() + delimeter;
            }
            return result;
        }

        public Results() { }

        public Results(string info)
        {
            string[] sinais = info.Split(delimeter.ToCharArray());

            for (int i = 0; i < sinais.Length - 1; i++)
            {
                results.Add(new Sinal(sinais[i]));

            }
        }
    }


}