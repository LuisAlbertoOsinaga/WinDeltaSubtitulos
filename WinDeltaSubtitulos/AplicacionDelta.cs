using System;
using System.IO;
using System.Text;

namespace delta
{
    public class AplicacionDelta
    {
             // propiedades resultado
            public string MensajeError = null;
            public string NombreArchivoOut = null;
            public bool ParseOk = false;
            public bool ProcesaOk = false;

            public string AplicaDeltaTiempo(string horaAntes, TimeSpan deltaTiempo)
            {
                try
                {
                    string horaActual = null;
                    TimeSpan tsHoraActual = TimeSpan.Parse(horaAntes);
                    tsHoraActual = tsHoraActual + deltaTiempo;
                    horaActual = tsHoraActual.ToString();
                    if(horaActual.Length == 8)
                        horaActual = horaActual + ",000";
                    if(horaActual.Length > 12)
                        horaActual = horaActual.Remove(12);

                    return horaActual;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error. Excepción en AplicaDeltaTiempo. Tipo: '{0}. Mensaje: '{1}'" + 
                                    ". HoraAntes: {2}. DeltaTiempo: {3}",
                                    ex.GetType(), ex.Message, horaAntes, deltaTiempo);
                    return null;
                }
            }

            public bool ParseInput(string[] args, out string nombreArchivoIn, out string nombreArchivoOut, 
                                        out TimeSpan? deltaTiempo, out string mensajeError)
            {
                // Inicializa variables salida
                nombreArchivoIn = null;
                deltaTiempo = null;
                nombreArchivoOut = null;
                mensajeError = null;

                //
                // nombre de archivo de entrada
                //

                if(args.Length < 1)
                {
                    // Consigue nombre de archivo de subtitulos
                    Console.WriteLine();
                    Console.Write("Nombre de archivo de subtítulos: ");
                    nombreArchivoIn = Console.ReadLine();
                }
                else
                    nombreArchivoIn = args[0];

                if(!File.Exists(nombreArchivoIn))
                {
                    mensajeError = string.Format("Error. Archivo '{0}' no existe", nombreArchivoIn);
                    return false;
                }
                nombreArchivoOut = Path.GetFileNameWithoutExtension(nombreArchivoIn) + "_out" 
                                                + Path.GetExtension(nombreArchivoIn);

                //
                // delta tiempo
                //

                string strDeltaTiempo = null;
                if(args.Length < 2)
                {
                    // Consigue delta tiempo a sumar en el archivo
                    Console.WriteLine();
                    Console.WriteLine("Delta de tiempo ([-]ss.fff): ");
                    strDeltaTiempo = Console.ReadLine();
                }
                else
                    strDeltaTiempo = args[1];
                bool haySignoMenos = (strDeltaTiempo[0] == '-');
                int indiceInsert = haySignoMenos ? 1 : 0;
                strDeltaTiempo = strDeltaTiempo.Insert(indiceInsert, "00:00:"); // aumenta horas y minutos
                // Console.WriteLine(strDeltaTiempo);   // para debug

                TimeSpan tsDeltaTiempo;    
                bool parseDeltaTiempoOk = TimeSpan.TryParse(strDeltaTiempo, out tsDeltaTiempo);
                if(!parseDeltaTiempoOk)
                {
                    mensajeError = string.Format("Error. Delta de tiempo '{0}' debe estar en formato: [-]ss.fff", 
                                                strDeltaTiempo);
                    return false;                                
                }
                deltaTiempo = tsDeltaTiempo;
            
                return true;
            }

            public bool ProcesaArchivo(string nombreArchivoIn, string nombreArchivoOut, 
                                            TimeSpan deltaTiempo, out string mensajeError)
            {
                // Variables de salida
                mensajeError = null;

                // Objetos para manejos de archivo de entrada y salida
                StreamReader reader = null;
                StreamWriter writer = null;

                try
                {
                    // Abre Archivo In
                    reader = new StreamReader(new FileStream(nombreArchivoIn, FileMode.Open), 
                                                                Encoding.UTF7);

                    // Abre Archivo Out
                    writer = new StreamWriter(new FileStream(nombreArchivoOut, FileMode.Create), 
                                                                Encoding.UTF8);

                    // Procesa archivo
                    string rowIn = reader.ReadLine();
                    while(rowIn != null)
                    {
                        // Procesa linea
                        string rowOut = ProcesaLinea(rowIn, deltaTiempo);
                        // Console.WriteLine(rowOut); // para debug
                        writer.WriteLine(rowOut);
                    
                        rowIn = reader.ReadLine();
                    }

                    return true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error. Excepción en ProcesaArchivo. Tipo: '{0}. Mensaje: '{1}'",
                                        ex.GetType(), ex.Message);
                    return false;
                }
                finally
                {
                    // Cierra archivos
                    if(writer != null)
                        writer.Dispose();
                    if(reader != null)    
                        reader.Dispose();
                }
            }

            public string ProcesaLinea(string row, TimeSpan deltaTiempo)
            {
                try
                {
                    string rowOut = row;

                    int i = row.IndexOf("-->");
                    if(i > 0)
                    {
                        string[] partes = row.Split(' ', '-', '>');
                    
                        string horaAntes = partes[0].Replace(',', '.');
                        string horaIzq = AplicaDeltaTiempo( horaAntes, deltaTiempo);

                        horaAntes = partes[partes.Length - 1].Replace(',', '.');
                        string horaDer = AplicaDeltaTiempo( horaAntes, deltaTiempo);

                        rowOut = horaIzq + " --> " + horaDer;
                    }
                    return rowOut;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error. Excepción en ProcesaLinea. Tipo: '{0}. Mensaje: '{1}'" + 
                                        ". Línea: {2}",
                                        ex.GetType(), ex.Message, row);
                    return null;
                }
            }
    }
}