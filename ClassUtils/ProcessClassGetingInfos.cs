using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

public class ProcessClassGetingInfos
{
    // Permissão usada para consultar o token do processo
    const int TOKEN_QUERY = 0x0008;

    // Classe de informação usada para verificar elevação (admin)
    const int TokenElevation = 20;

    // Estrutura que armazena contadores de I/O do processo (leitura/escrita em disco)
    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    // Importa função nativa do Windows para obter uso de disco do processo
    [DllImport("Kernel32.dll", SetLastError = true)]
    static extern bool GetProcessIoCounters(IntPtr ProcessHandle, out IO_COUNTERS counters);

    // Abre o token de segurança do processo
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

    // Obtém informações do token (usado aqui para verificar elevação/admin)
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(
        IntPtr TokenHandle,
        int TokenInformationClass,
        out int TokenInformation,
        int TokenInformationLength,
        out int ReturnLength);

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
        if (!OpenProcessToken(process.Handle, TOKEN_QUERY, out var token))
            return false;

        if (!GetTokenInformation(token, TokenElevation, out int elevation, sizeof(int), out _))
            return false;

        // Se elevation != 0, o processo está elevado
        return elevation != 0;
    }

    // Calcula uso de CPU manualmente comparando tempo de CPU em um intervalo
    private async Task<double> GetCpuUsageAsync(Process[] process)
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

    // Retorna uso total de memória física (Working Set) em MB
    private double GetMemoryUsage(Process[] process)
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
    private ProcessMemoryData GetProcessMemoryData(Process[] process)
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

    // Conta número total de threads do processo
    private int GetThreadsCount(Process[] process)
    {
        int ProcessThreadsNumber = 0;

        foreach (Process processItem in process)
        {
            processItem.Refresh();
            ProcessThreadsNumber += processItem.Threads.Count;
        }

        return ProcessThreadsNumber;
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

    // Obtém caminho do executável do processo
    private string GetFileProcessPath()
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
    private async Task<double> GetMbsUsageByProcessAsync(Process[] processes)
    {
        double MbsUsage = 0;

        foreach (Process process in processes)
        {
            process.Refresh();

            if (GetProcessIoCounters(process.Handle, out IO_COUNTERS cOUNTERS))
            {
                double MbsUsageByReadOld = cOUNTERS.ReadTransferCount;
                double MbsUsageByWriteOld = cOUNTERS.WriteTransferCount;
                DateTime Intervalo = DateTime.Now;

                await Task.Delay(1000);
                process.Refresh();

                GetProcessIoCounters(process.Handle, out IO_COUNTERS cOUNTERSNow);
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

    // Retorna modelo básico do processo
    public async Task<ProcessModel> GetProcessStructAsync()
    {
        try
        {
            int Pid = process.FirstOrDefault().Id;
            double MemoryUsage = GetMemoryUsage(process);
            double CpuUsageRef = await GetCpuUsageAsync(process);;
            double InstancesNumber = process.Count();                        
            string ProcessName = processName;
            string ProcessPath = GetFileProcessPath();

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
            double CpuUsageRef = await GetCpuUsageAsync(process);
            double MbsUsageRef = await GetMbsUsageByProcessAsync(process);
            double InstancesNumber = process.Length;
            int ModulesNumber = GetModulesCount(process);
            int ThreadNumber = GetThreadsCount(process);
            string ProcessName = processName;
            string ProcessPath = GetFileProcessPath();
            bool ProcessIsPrivilegied = GetProcessIsadmin(process.FirstOrDefault());
            DateTime startTimeProcess = process.FirstOrDefault().StartTime;
            ProcessMemoryData memoryData = GetProcessMemoryData(process);
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