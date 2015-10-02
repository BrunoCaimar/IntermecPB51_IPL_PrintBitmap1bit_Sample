using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintIntermec
{
    class Program
    {

        static void Main(string[] args)
        {
            //var imagem = Xxxx();
            //GetImageFormat(Xxxx());
            //return;

            //var dados = System.IO.File.ReadAllText(@"C:\temp\ipl_imagem.txt");
            //printFile(dados);
            //var dados3 = System.IO.File.ReadAllText(@"C:\temp\ipl_imagem3.txt");
            //printFile(dados3);
            //var dados2 = System.IO.File.ReadAllText(@"C:\temp\ipl_imagem2.txt");
            //printFile(dados2);
            //Console.ReadKey();

            Console.WriteLine("Lendo imagem do disco...");
            var ipl_imagem = GetIPLForImageBits(GetImageBits(@"C:\temp\IMAGEN.bmp"));
            var ipl_to_print_imagem = File.ReadAllText(@"C:\temp\galo_print.txt");
            var galo = File.ReadAllText(@"C:\temp\galo.txt");

            // Salva apenas para referencia
            File.WriteAllText(@"C:\temp\imagem.txt", ipl_imagem);

            Console.WriteLine("Enviando imagem para impressora...");
            sendData2Printer(ipl_imagem);

            Console.WriteLine("Enviando comando de impressão da imagem para impressora...");
            sendData2Printer(ipl_to_print_imagem);

        }

        static ArrayList GetImageBits(string path2bitmap)
        {
            var bmp = new Bitmap(path2bitmap);
            var imagem_bits = new ArrayList();
            // HACK - Normaliza o tamanho 
            var xx = bmp.Height;
            var yy = bmp.Width;
            var xy = string.Format("x{0};y{1}", xx, yy);
            imagem_bits.Add(xy);

            for (int y = 0; y < bmp.Height; y++)
            {
                var linha_bit = "";
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
                (char)2,
                (char)27,
                (char)3,
                Environment.NewLine);

            // P - Modo de programação
            imagem.AppendFormat("{0}{1}P{2}{3}",
                (char)2,
                (char)27,
                (char)3,
                Environment.NewLine);

            var contador = 0;
            foreach (var item in dados)
            {
                if (contador == 0)
                {
                    // Posição da Imagem na memória da impressora (G1)
                    // Nome da Imagem - galo
                    // Tamanho da imagem x000;y000
                    imagem.AppendFormat("{0}G1,galo;{1};{2}{3}",
                        (char)2,
                        item,
                        (char)3,
                        Environment.NewLine);
                }
                else
                {
                    imagem.AppendFormat("{0}u{1},{2};{3}{4}",
                        (char)2,
                        contador - 1,
                        item,
                        (char)3,
                        Environment.NewLine);
                }
                contador++;
            }
            // Finaliza modo de programação (R)
            imagem.AppendFormat("{0}R;{1}{2}",
                (char)2,
                (char)3,
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
            printPort.DataReceived += PrintPort_DataReceived;
            printPort.ErrorReceived += PrintPort_ErrorReceived;
            printPort.PinChanged += PrintPort_PinChanged;

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

        private static void PrintPort_PinChanged(object sender, System.IO.Ports.SerialPinChangedEventArgs e)
        {
            Console.Out.WriteLine("PrintPort_PinChanged");
            Console.Out.WriteLine("PrintPort_PinChanged" + e.ToString());
        }

        private static void PrintPort_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            Console.Out.WriteLine("PrintPort_ErrorReceived");
            Console.Out.WriteLine("PrintPort_ErrorReceived" + e.ToString());
        }

        private static void PrintPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Console.Out.WriteLine("PrintPort_DataReceived");
            Console.Out.WriteLine("PrintPort_DataReceived" + e.ToString());
        }
    }
}
