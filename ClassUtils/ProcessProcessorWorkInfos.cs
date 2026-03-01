using System.Diagnostics;

namespace TaskManage.ClassUtils;

public class ProcessProcessorWorkInfos
{
    // Calcula uso de CPU manualmente comparando tempo de CPU em um intervalo
    public async Task<double> GetCpuUsageAsync(Process[] process)
    {
        double usoDeCPU = 0;

        foreach (Process processItem in process)
        {
            try
            {
                processItem.Refresh();

                TimeSpan StartCPUuSage = processItem.TotalProcessorTime;
                DateTime StartTime = DateTime.Now;
                await Task.Delay(1000);

                processItem.Refresh();

                TimeSpan EndCPUuSage = processItem.TotalProcessorTime;
                DateTime EndTime = DateTime.Now;

                // Calcula porcentagem considerando núcleos da CPU
                usoDeCPU += ((EndCPUuSage - StartCPUuSage).TotalMilliseconds / (EndTime - StartTime).TotalMilliseconds) / Environment.ProcessorCount * 100;
            }
            catch (System.Exception)
            { }
        }
        return usoDeCPU;
    }
    
    // Conta número total de threads do processo
    public int GetThreadsCount(Process[] process)
    {
        int ProcessThreadsNumber = 0;

        foreach (Process processItem in process)
        {
            processItem.Refresh();
            ProcessThreadsNumber += processItem.Threads.Count;
        }

        return ProcessThreadsNumber;
    }
}