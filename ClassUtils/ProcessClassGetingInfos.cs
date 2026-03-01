using System.ComponentModel;
using System.Diagnostics;

using TaskManage.ClassUtils;
using TaskManage.WindowsInteropAPI;

public class ProcessClassGetingInfos
{
    private ProcesseMemoryWorkInfos _memoryWorkInfos =  new ProcesseMemoryWorkInfos();
    private ProcessDiskWorkInfos _diskWorkInfos = new ProcessDiskWorkInfos();
    private ProcessProcessorWorkInfos _processorWorkInfos = new ProcessProcessorWorkInfos();
    
    // Array com todas as instâncias do processo encontrado
    public Process[] process;

    // Nome do processo pesquisado
    private string processName;

    // Construtor recebendo processos já resolvidos
    public ProcessClassGetingInfos(Process[] _process)
    {
        this.process = _process;
    }

    // Construtor recebendo apenas o nome do processo
    public ProcessClassGetingInfos(string _ProcessName)
    {
        this.processName = _ProcessName;

        try
        {
            // Busca todos os processos com esse nome
            this.process = Process.GetProcessesByName(_ProcessName);
        }
        catch (Exception)
        {
            Console.WriteLine("O nome do processo que vc inseriu n existe, é invalido, ou foi negado o acesso");
        }
    }

    // Verifica se o processo está rodando com privilégios elevados (admin)
    private bool GetProcessIsadmin(Process process)
    {
        if (!wiaProcessIO.OpenProcessToken(process.Handle, wiaProcessIO.TOKEN_QUERY, out var token))
            return false;

        if (!wiaProcessIO.GetTokenInformation(token, wiaProcessIO.TokenElevation, out int elevation, sizeof(int), out _))
            return false;

        // Se elevation != 0, o processo está elevado
        return elevation != 0;
    }
    
    // Conta módulos carregados pelo processo (DLLs, EXE etc.)
    private int GetModulesCount(Process[] process)
    {
        int ProcessModulesNumber = 0;

        foreach (Process processItem in process)
        {
            processItem.Refresh();
            ProcessModulesNumber += processItem.Modules.Count;
        }

        return ProcessModulesNumber;
    }
    
    // Retorna modelo básico do processo
    public async Task<ProcessModel> GetProcessStructAsync()
    {
        try
        {
            int Pid = process.FirstOrDefault().Id;
            double MemoryUsage = _memoryWorkInfos.GetMemoryUsage(process);
            double CpuUsageRef = await _processorWorkInfos.GetCpuUsageAsync(process);;
            double InstancesNumber = process.Count();                        
            string ProcessName = processName;
            string ProcessPath = _diskWorkInfos.GetFileProcessPath(process);

            ProcessModel processStruct = new ProcessModel();

            processStruct.ProcessID = Pid;
            processStruct.ProcessName = ProcessName;
            processStruct.ProcessPath = ProcessPath;
            processStruct.ProcessMemoryUsageMb = MemoryUsage;
            processStruct.ProcessCpuUsage = CpuUsageRef;
            processStruct.ProcessInstancesNumber = InstancesNumber;

            return processStruct;
        }
        catch (Win32Exception)
        {
            return new ProcessModel()
            {
                ProcessID = -1,
                ProcessCpuUsage = 0,
                ProcessInstancesNumber = 0,
                ProcessMemoryUsageMb = 0,
                ProcessName = "Acesso negado.exe",
                ProcessPath = ".\\\\\\.\\\\\\System deneid"
            };
        } catch (NullReferenceException)
        {
            return new ProcessModel()
            {
                ProcessID = -1,
                ProcessCpuUsage = 0,
                ProcessInstancesNumber = 0,
                ProcessMemoryUsageMb = 0,
                ProcessName = "This Process Not Exist.exe",
                ProcessPath = ".\\\\\\.\\\\\\error:404"
            };
        }
    }
    

    // Retorna modelo avançado com mais métricas
    public async Task<ProcessAdvancedModel> GetProcessStructAdvencedAsync()
    {
        try
        {
            int Pid = process.FirstOrDefault().Id;
            double CpuUsageRef = await _processorWorkInfos.GetCpuUsageAsync(process);
            double MbsUsageRef = await _diskWorkInfos.GetMbsUsageByProcessAsync(process);
            double InstancesNumber = process.Length;
            int ModulesNumber = GetModulesCount(process);
            int ThreadNumber = _processorWorkInfos.GetThreadsCount(process);
            string ProcessName = processName;
            string ProcessPath = _diskWorkInfos.GetFileProcessPath(process);
            bool ProcessIsPrivilegied = GetProcessIsadmin(process.FirstOrDefault());
            DateTime startTimeProcess = process.FirstOrDefault().StartTime;
            ProcessMemoryData memoryData = _memoryWorkInfos.GetProcessMemoryData(process);
            ProcessAdvancedModel advancedModel = new ProcessAdvancedModel();

            advancedModel.ProcessID = Pid;
            advancedModel.ProcessName = ProcessName;
            advancedModel.ProcessPath = ProcessPath;
            advancedModel.ProcessThreadsNumber = ThreadNumber;
            advancedModel.processMemoryData = memoryData;
            advancedModel.ProcessStartTime = startTimeProcess;
            advancedModel.ProcessIsPrivilegied = ProcessIsPrivilegied;
            advancedModel.ProcessCpuUsage = CpuUsageRef;
            advancedModel.ProcessDiskUsage = MbsUsageRef;

            return advancedModel;
        }
        catch (Win32Exception)
        {
            Console.WriteLine("Houve um erro após tentar acessar o um processo privilegiado..");
            return new ProcessAdvancedModel()
            {
                ProcessID = -1,
                ProcessName = "Acesso negado.exe",
                ProcessPath = ".\\\\\\.\\\\\\System deneid",
                ProcessThreadsNumber = 0,
                ProcessInstancesNumber = 0,
                ProcessDiskUsage = 0,
                ProcessCpuUsage = 0,
                ProcessIsPrivilegied = true,
                ProcessModulesNumber = 0,
                ProcessStartTime = DateTime.MinValue,
                processMemoryData = new ProcessMemoryData()
                {
                    ProcessMemoryPagedUsage = 0,
                    ProcessMemoryPhisyckUsage = 0,
                    ProcessMemoryPrivateUsage = 0,
                    ProcessMemoryVirtualUsage = 0
                }

            };
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("Houve um erro após tentar acessar um processo inexistente");
            return new ProcessAdvancedModel()
            {
                ProcessID = -1,
                ProcessName = "This Process Not Exist.exe",
                ProcessPath = ".\\\\\\.\\\\\\error:404",
                ProcessThreadsNumber = 0,
                ProcessInstancesNumber = 0,
                ProcessDiskUsage = 0,
                ProcessCpuUsage = 0,
                ProcessIsPrivilegied = false,
                ProcessModulesNumber = 0,
                ProcessStartTime = DateTime.MinValue,
                processMemoryData = new ProcessMemoryData()
                {
                    ProcessMemoryPagedUsage = 0,
                    ProcessMemoryPhisyckUsage = 0,
                    ProcessMemoryPrivateUsage = 0,
                    ProcessMemoryVirtualUsage = 0
                }

            };
        }
    }
}