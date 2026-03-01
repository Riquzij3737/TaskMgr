using System.Runtime.InteropServices;

namespace TaskManage.WindowsInteropAPI;

public class wiaProcessIO
{
    // Permissão usada para consultar o token do processo
    public const int TOKEN_QUERY = 0x0008;

    // Classe de informação usada para verificar elevação (admin)
    public const int TokenElevation = 20;

    // Estrutura que armazena contadores de I/O do processo (leitura/escrita em disco)
    [StructLayout(LayoutKind.Sequential)]
    public struct IO_COUNTERS
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
    public static extern bool GetProcessIoCounters(IntPtr ProcessHandle, out IO_COUNTERS counters);

    // Abre o token de segurança do processo
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

    // Obtém informações do token (usado aqui para verificar elevação/admin)
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool GetTokenInformation(
        IntPtr TokenHandle,
        int TokenInformationClass,
        out int TokenInformation,
        int TokenInformationLength,
        out int ReturnLength);
}