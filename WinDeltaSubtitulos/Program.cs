using System;

namespace delta
{
    class Program
    {
        static void Main(string[] args)
        {
            // Objeto AplicacionDelta
            AplicacionDelta appDelta = new AplicacionDelta();
            
            try
            {
                // Parse input usuario
                string nombreArchivoIn = null;
                TimeSpan? deltaTiempo = null;
                appDelta.ParseOk = appDelta.ParseInput(args, out nombreArchivoIn, out appDelta.NombreArchivoOut, 
                                                        out deltaTiempo, out appDelta.MensajeError);
                if(!appDelta.ParseOk)
                {
                    Console.WriteLine(appDelta.MensajeError);
                    Console.WriteLine("Forma de uso: dotnet run 'nombre-archivo' 'deltaTiempo([-]ss.fff)'");
                    return;
                }

                // Procesa archivo
                appDelta.ProcesaOk = appDelta.ProcesaArchivo(nombreArchivoIn, appDelta.NombreArchivoOut, 
                                                    deltaTiempo.GetValueOrDefault(), out appDelta.MensajeError);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error: Excepción no manejada. Tipo: '{0}. Mensaje: '{1}'", 
                                        ex.GetType(), ex.Message);
            }
            finally
            {
                // Cierra programa
                Console.WriteLine();
                if(!appDelta.ProcesaOk)
                {
                    Console.WriteLine("Error: '{0}'", appDelta.MensajeError);
                }
                else
                {
                    Console.WriteLine("Proceso finalizado. Archivo corregido: '{0}'", appDelta.NombreArchivoOut);
                }
                Console.WriteLine();
            }
        }
    }
}
