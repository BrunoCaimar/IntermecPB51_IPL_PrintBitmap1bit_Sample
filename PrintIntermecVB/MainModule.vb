Imports System.Drawing
Imports System.IO
Imports System.Text

Module MainModule
    Public Sub Main(args As String())
        ' Exemplo de impressão de imagens monocromaticas 
        ' usando IPL - Impressora testada: Intermec PB51
        Console.WriteLine("Lendo imagem do disco...")
        Dim ipl_imagem = GetIPLForImageBits(GetImageBits("IMAGEN.bmp"))
        Dim ipl_to_print_imagem = File.ReadAllText("print_image_g1.txt")

        ' Salva apenas para referencia
        Console.WriteLine("Salvando IPL imagem...")
        File.WriteAllText("imagem.txt", ipl_imagem)

        Console.WriteLine("Enviando imagem para impressora...")
        sendData2Printer(ipl_imagem)

        Console.WriteLine("Enviando comando de impressão da imagem para impressora...")
        sendData2Printer(ipl_to_print_imagem)

    End Sub

    Private Function GetImageBits(path2bitmap As String) As ArrayList
        Dim bmp = New Bitmap(path2bitmap)
        Dim imagem_bits = New ArrayList()
        imagem_bits.Add(String.Format("x{0};y{1}", bmp.Height, bmp.Width))

        For y As Integer = 0 To bmp.Height - 1
            Dim linha_bit = String.Empty
            For x As Integer = 0 To bmp.Width - 1

                Dim c = bmp.GetPixel(x, y)
                Dim rgb = CInt((CInt(c.R) + CInt(c.G) + CInt(c.B)) / 3)
                linha_bit += If(rgb = 0, "1", "0")
            Next
            imagem_bits.Add(Reverse(linha_bit))
        Next
        Return imagem_bits
    End Function

    Private Function GetIPLForImageBits(dados As ArrayList) As String
        Dim imagem = New StringBuilder()
        ' Cabeçalho (c - Modo de emulação)

        imagem.AppendFormat("{0}{1}c{2}{3}", Chr(2), Chr(27), Chr(3), Environment.NewLine)

        ' P - Modo de programação
        imagem.AppendFormat("{0}{1}P{2}{3}", Chr(2), Chr(27), Chr(3), Environment.NewLine)

        Dim contador = 0
        For Each item In dados
            If contador = 0 Then
                ' Posição da Imagem na memória da impressora (G1)
                ' Nome da Imagem - 'sign'
                ' Tamanho da imagem x000;y000

                imagem.AppendFormat("{0}G1,sign;{1};{2}{3}", Chr(2), item, Chr(3), Environment.NewLine)
            Else
                imagem.AppendFormat("{0}u{1},{2};{3}{4}", Chr(2), contador - 1, item, Chr(3), Environment.NewLine)
            End If
            contador += 1
        Next
        ' Finaliza modo de programação (R)
        imagem.AppendFormat("{0}R;{1}{2}", Chr(2), Chr(3), Environment.NewLine)

        Return imagem.ToString()
    End Function

    Public Function Reverse(input As String) As String
        Return String.Concat(Enumerable.Reverse(input))
    End Function

    Private Sub sendData2Printer(dados As [String])
        Dim printPort = New Ports.SerialPort()

        If Not printPort.IsOpen Then
            printPort.PortName = "COM4"
            printPort.BaudRate = 9600
            printPort.DataBits = 8
            printPort.Open()
        End If
        printPort.Write(dados)

        printPort.Close()
    End Sub
End Module
