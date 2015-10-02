using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

// Exemplo de impressão de imagens monocromaticas 
// usando IPL - Impressora testada: Intermec PB51

namespace PrintIntermec
{
    class Program
    {
        private const char STX = (char)2;
        private const char ESC = (char)27;
        private const char ETX = (char)3;

        static void Main(string[] args)
        {
            Console.WriteLine("Lendo imagem do disco...");
            var ipl_imagem = GetIPLForImageBits(GetImageBits(@"IMAGEN.bmp"));
            var ipl_to_print_imagem = File.ReadAllText(@"print_image_g1.txt");

            // Salva apenas para referencia
            File.WriteAllText(@"imagem.txt", ipl_imagem);

            Console.WriteLine("Enviando imagem para impressora...");
            sendData2Printer(ipl_imagem);

            Console.WriteLine("Enviando comando de impressão da imagem para impressora...");
            sendData2Printer(ipl_to_print_imagem);

        }

        static ArrayList GetImageBits(string path2bitmap)
        {
            var bmp = new Bitmap(path2bitmap);
            var imagem_bits = new ArrayList();
            var xy = string.Format("x{0};y{1}", bmp.Height, bmp.Width);
            imagem_bits.Add(xy);

            for (int y = 0; y < bmp.Height; y++)
            {
                var linha_bit = string.Empty;
                for (int x = 0; x < bmp.Width; x++)
                {

                    var c = bmp.GetPixel(x, y);
                    var rgb = (int)((c.R + c.G + c.B) / 3);
                    linha_bit += rgb == 0 ? "1" : "0";
                }
                imagem_bits.Add(Reverse(linha_bit));
            }
            return imagem_bits;
        }

        static string GetIPLForImageBits(ArrayList dados)
        {
            var imagem = new StringBuilder();
            // Cabeçalho (c - Modo de emulação)
            imagem.AppendFormat("{0}{1}c{2}{3}",
                STX,
                ESC,
                ETX,
                Environment.NewLine);

            // P - Modo de programação
            imagem.AppendFormat("{0}{1}P{2}{3}",
                STX,
                ESC,
                ETX,
                Environment.NewLine);

            var contador = 0;
            foreach (var item in dados)
            {
                if (contador == 0)
                {
                    // Posição da Imagem na memória da impressora (G1)
                    // Nome da Imagem - sign
                    // Tamanho da imagem x000;y000
                    imagem.AppendFormat("{0}G1,sign;{1};{2}{3}",
                        STX,
                        item,
                        ETX,
                        Environment.NewLine);
                }
                else
                {
                    imagem.AppendFormat("{0}u{1},{2};{3}{4}",
                        STX,
                        contador - 1,
                        item,
                        ETX,
                        Environment.NewLine);
                }
                contador++;
            }
            // Finaliza modo de programação (R)
            imagem.AppendFormat("{0}R;{1}{2}",
                STX,
                ETX,
                Environment.NewLine);

            return imagem.ToString();
        }

        public static string Reverse(string input)
        {
            return string.Concat(Enumerable.Reverse(input));
        }

        static void sendData2Printer(String dados)
        {
            var printPort = new System.IO.Ports.SerialPort();

            if (!printPort.IsOpen)
            {
                printPort.PortName = "COM4";
                printPort.BaudRate = 9600;
                printPort.DataBits = 8;
                printPort.Open();
            }
            printPort.Write(dados);

            printPort.Close();
        }
    }
}
