using System.Diagnostics;
using TaskManage.WindowsInteropAPI;

namespace TaskManage.ClassUtils;

public class ProcessDiskWorkInfos
{
    // Obtém caminho do executável do processo
    public string GetFileProcessPath(Process[] process)
    {
        try
        {
            string ProcessPath = (process.FirstOrDefault().MainModule).FileName;
            return ProcessPath;
        }
        catch (System.AccessViolationException)
        {
            Console.WriteLine("O processo que o imbecil quis acessar é protegido pelo windows");
        }
        catch (System.InvalidOperationException)
        {
            Console.WriteLine("Ocorreu um erro interno dentro do programa");
        }

        return "Erro na procura do arquivo raiz do processo";
    }

    // Calcula taxa de leitura/escrita em disco do processo (MB/s)
    public async Task<double> GetMbsUsageByProcessAsync(Process[] processes)
    {
        double MbsUsage = 0;
        
        foreach (Process process in processes)
        {
            process.Refresh();

            if (wiaProcessIO.GetProcessIoCounters(process.Handle, out wiaProcessIO.IO_COUNTERS cOUNTERS))
            {
                double MbsUsageByReadOld = cOUNTERS.ReadTransferCount;
                double MbsUsageByWriteOld = cOUNTERS.WriteTransferCount;
                DateTime Intervalo = DateTime.Now;

                await Task.Delay(1000);
                process.Refresh();

                wiaProcessIO.GetProcessIoCounters(process.Handle, out wiaProcessIO.IO_COUNTERS cOUNTERSNow);
                double MbsUsageByReadNow = cOUNTERSNow.ReadTransferCount;
                double MbsUsageByWriteNow = cOUNTERSNow.WriteTransferCount;
                DateTime INtervalo = DateTime.Now;

                // Calcula taxa dividindo bytes pelo tempo e convertendo para MB
                double MbsUsageByReadTotal = (MbsUsageByReadNow - MbsUsageByReadOld) / (INtervalo - Intervalo).TotalSeconds / 1024 / 1024;
                double MbsUsageByWritenTotal = (MbsUsageByWriteNow - MbsUsageByWriteOld) / (INtervalo - Intervalo).TotalSeconds / 1024 / 1024;

                MbsUsage += MbsUsageByReadTotal + MbsUsageByWritenTotal;
            }
            else
            {
                Console.WriteLine("Ocorreu um erro na leitura de bytes usados para leitura/gravação do processo.......");
                return 0;
            }
        }

        return MbsUsage;
    }
}