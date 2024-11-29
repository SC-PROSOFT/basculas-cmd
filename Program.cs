using System.IO.Ports;

namespace SerialPortExample
{
    class Program
    {
        static SerialPort? serialPort;

        const string puerto = "COM3";
        const int baudRate = 9600;   

        static string pesoPrimerCliente = ""; // Esta es para almacenar el peso del primer cliente ya que llega de a un caracter y toca contruir el valor en esta variable.

        static void Main(string[] args)
        {
            try
            {                
                serialPort = new SerialPort(puerto, baudRate, Parity.None, 8, StopBits.One);
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.ErrorReceived += SerialPort_ErrorReceived;

                Console.WriteLine($"Escuchando el puerto: {puerto}");
                Console.WriteLine($"Escuchando el baudRate: {baudRate}");

                serialPort.Open();

                Console.WriteLine("Presione cualquier tecla para salir.");
                Console.ReadKey();

                serialPort.Close();
                Console.WriteLine("Puerto serial cerrado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);                
            }
        }

        private static void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine($"Error en el puerto serial: {e.EventType}");
        }

        private static async void SerialPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e
        )
        {
            SerialPort sp = (SerialPort)sender;
            string receivedData = sp.ReadExisting();

            Console.WriteLine("Datos de bascula: " + receivedData);

            string valorArreglado = ArreglarValor(receivedData).Trim();

            if (valorArreglado.Trim() != "" && valorArreglado.Length == 6)
            {                
                var valorConNumeroDeVerificacion = AgregarNumeroDeVerificacion(valorArreglado);

                if (valorConNumeroDeVerificacion.Trim() != "")
                {
                    Console.WriteLine(valorConNumeroDeVerificacion);
                    await GuardarEnTxt(valorConNumeroDeVerificacion);
                }
                else
                {
                    await GuardarLog($"Intento guardar un vacio: valorConNumerodeVerificacion->{valorConNumeroDeVerificacion}<-, valorArreglado->{valorArreglado}<- ");
                }
                
            }
        }
        
        static async Task GuardarLog(string log)
        {
            string ruta = @"C:\PROSOFT\CACHICOB";
            string rutaArchivo = System.IO.Path.Combine(ruta, "LOGS.SCLOGS");

            try
            {
                await System.IO.File.AppendAllTextAsync(rutaArchivo, log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir el archivo local: {ex.Message}");
            }
        }

        static string ArreglarValor(string valor)
        {
            int company = 3;
            string valorArreglado = "";

            if (string.IsNullOrEmpty(valor))
            {
                return valorArreglado;
            }

            switch (company)
            {
                // indupady
                case 1:
                    valorArreglado = new string(valor.Skip(1).Reverse().ToArray()).Trim().PadLeft(6, '0');
                    
                    return valorArreglado;
                // fabian perez
                case 2:
                    if (valor.EndsWith("kg") && valor.Length > 2)
                    {
                        string stringWithoutKg = valor
                            .Substring(0, valor.Length - 2)
                            .PadLeft(6, '0');
                        valorArreglado = stringWithoutKg;
                    }

                    return valorArreglado;
                // hato grande
                // grupo hm montecarlos -> 860350196
                case 3:
                    pesoPrimerCliente = pesoPrimerCliente + valor;

                    if (pesoPrimerCliente.EndsWith("kg") && pesoPrimerCliente.Length > 2)
                    {
                        //int index = pesoPrimerCliente.IndexOf('+'); // Aqui capturamos unicamente lo que haya despues del simbolo +
                        int index = pesoPrimerCliente.IndexOfAny(new char[] { '+', '-' });

                        if (index != -1)
                        {
                            string pesoAfterPlus = pesoPrimerCliente.Substring(index + 1);
                            string stringWithoutKg = pesoAfterPlus
                                .Substring(0, pesoAfterPlus.Length - 2)
                                .Trim()
                                .PadLeft(6, '0');
                            valorArreglado = stringWithoutKg;

                            pesoPrimerCliente = "";
                        }                        
                    }

                    Console.WriteLine("Valor arreglado: " + valorArreglado);

                    return valorArreglado;
                // obdulio mayorga
                case 4:
                    int index2 = valor.IndexOf("+");

                    if (index2 != -1)
                    {
                        string pesoAfterPlus2 = valor.Substring(index2 + 1);

          

                        valorArreglado = pesoAfterPlus2.Trim().PadLeft(6, '0');

              
                    }
                    return valorArreglado;
                default:
                    return valorArreglado;
            }
        }

        static string AgregarNumeroDeVerificacion(string peso)
        {
            try
            {
                DateTime fechaActual = DateTime.Now;
                string anio = fechaActual.Year.ToString().PadLeft(4, '0');
                string mes = fechaActual.Month.ToString().PadLeft(2, '0');
                string dia = fechaActual.Day.ToString().PadLeft(2, '0');
                string hora = fechaActual.Hour.ToString().PadLeft(2, '0');
                string minuto = fechaActual.Minute.ToString().PadLeft(2, '0');

                string diaHoraMinutoUnidos = $"{dia}{hora}{minuto}";
                char[] caracteres = diaHoraMinutoUnidos.ToCharArray();
                Array.Reverse(caracteres);
                string diaHoraMinutosInvertidos = new string(caracteres);

                // Intentar convertir las cadenas a enteros y manejar cualquier excepción

                int diaHoraMinutosInvertidosInt = int.Parse(diaHoraMinutosInvertidos);
                int pesoInt = int.Parse(peso);

                int diaHoraMinutosInvertidosSumadosConElPeso =
                    diaHoraMinutosInvertidosInt + pesoInt;

                double token = Math.Round(diaHoraMinutosInvertidosSumadosConElPeso / 0.03);


                string tokenEnString = token.ToString();

                
                string tokenLimpio = new string(
                    tokenEnString
                ).PadLeft(8, '0');

                return $"{anio}{mes}{dia}{hora}{minuto}{peso}{tokenLimpio}";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (opcionalmente, puedes registrar el error)
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        static async Task GuardarEnTxt(string peso)
        {
            if (string.IsNullOrEmpty(peso))
            {
                Console.WriteLine("Error: Intento de guardar un peso vacío.");
                return;
            }

            string ruta = @"C:\PROSOFT\CACHICOB";
            string rutaArchivo = System.IO.Path.Combine(ruta, "CACHICOB.SC4");

            try
            {
                await System.IO.File.WriteAllTextAsync(rutaArchivo, peso);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir el archivo local: {ex.Message}");
            }
        }       
    }
}
