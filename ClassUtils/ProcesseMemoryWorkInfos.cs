using System.Diagnostics;

namespace TaskManage.ClassUtils;

public class ProcesseMemoryWorkInfos
{
    // Retorna uso total de memória física (Working Set) em MB
    public double GetMemoryUsage(Process[] process)
    {
        double memoryTotalUsage = 0;

        foreach (Process processItem in process)
        {
            processItem.Refresh();

            // WorkingSet64 retorna bytes -> convertemos para MB
            memoryTotalUsage += (processItem.WorkingSet64 / 1024 / 1024);
        }

        return memoryTotalUsage;
    }

    // Retorna vários tipos de memória do processo
    public ProcessMemoryData GetProcessMemoryData(Process[] process)
    {
        double memoryPrivateUsage = 0;
        double MemoryPhisickUsage = 0;
        double MemoryVirtualUsage = 0;
        double MemoryPagedUsage = 0;

        foreach (Process processItem in process)
        {
            processItem.Refresh();

            memoryPrivateUsage += (processItem.PrivateMemorySize64 / 1024 / 1024);
            MemoryPhisickUsage += (processItem.WorkingSet64 / 1024 / 1024);
            MemoryVirtualUsage += (processItem.VirtualMemorySize64 / 1024 / 1024);
            MemoryPagedUsage += (processItem.PagedMemorySize64 / 1024 / 1024);
        }

        // Retorna struct preenchido com os dados
        return new ProcessMemoryData()
        {
            ProcessMemoryPagedUsage = MemoryPagedUsage,
            ProcessMemoryPhisyckUsage = MemoryPhisickUsage,
            ProcessMemoryPrivateUsage = memoryPrivateUsage,
            ProcessMemoryVirtualUsage = MemoryVirtualUsage
        };
    }
}