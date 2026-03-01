using System.CommandLine;
using System.Text;
using System.Text.Json;

public class SearchCommand
{
    // Mostra no console as informações do processo buscado pelo nome
    public async Task SearchProcessByName(string name)
    {
        // Cria objeto responsável por coletar infos do processo
        ProcessClassGetingInfos infos = new ProcessClassGetingInfos(name);

        // Obtém a struct com os dados do processo
        var ProcessInfo = await infos.GetProcessStructAsync();

        // Exibe as informações no console
        Console.WriteLine($"Id do processo: {ProcessInfo.ProcessID}");
        Console.WriteLine($"Nome do Processo: {ProcessInfo.ProcessName}");
        Console.WriteLine($"Caminho do arquivo processo: {ProcessInfo.ProcessPath}");
        Console.WriteLine($"Numero total de instancias do processo: {ProcessInfo.ProcessInstancesNumber}");
        Console.WriteLine($"Total de memoria usada pelo processo: {ProcessInfo.ProcessMemoryUsageMb:F2}mb");
        Console.WriteLine($"Total de CPU usada pelo processo: {ProcessInfo.ProcessCpuUsage:F2}%");
    }

    private void PrintMemoryData(ProcessMemoryData memoryData)
    {
        if (memoryData.ProcessMemoryPrivateUsage >= 1000)
        {
            Console.WriteLine($"Memoria privada usada pelo processo: {memoryData.ProcessMemoryPrivateUsage:F2}gb");    
        } else
        {
            Console.WriteLine($"Memoria privada usada pelo processo: {memoryData.ProcessMemoryPrivateUsage}mb");
        }
        if (memoryData.ProcessMemoryPagedUsage >= 1000)
        {
            Console.WriteLine($"Memoria paginada usada pelo processo: {memoryData.ProcessMemoryPagedUsage:F2}gb");    
        } else
        {
            Console.WriteLine($"Memoria paginada usada pelo processo: {memoryData.ProcessMemoryPagedUsage}mb");
        }
        if (memoryData.ProcessMemoryVirtualUsage >= 1000)
        {
            Console.WriteLine($"Memoria virtual usada pelo processo: {memoryData.ProcessMemoryVirtualUsage}gb");    
        } else
        {
            Console.WriteLine($"Memoria virtual usada pelo processo: {memoryData.ProcessMemoryVirtualUsage}mb");  
        }
        if (memoryData.ProcessMemoryPhisyckUsage >= 1000)
        {
            Console.WriteLine($"Memoria fisica usada pelo processo: {memoryData.ProcessMemoryPhisyckUsage}gb");
        } else
        {
            Console.WriteLine($"Memoria fisica usada pelo processo: {memoryData.ProcessMemoryPhisyckUsage}mb");
        }
    }

    private void WriteInFileMemoryData(ProcessMemoryData memoryData, StreamWriter sw)
    {
        if (memoryData.ProcessMemoryPrivateUsage >= 1000)
        {
            sw.WriteLine($"Memoria privada usada pelo processo: {memoryData.ProcessMemoryPrivateUsage:F2}gb");    
        } else
        {
            sw.WriteLine($"Memoria privada usada pelo processo: {memoryData.ProcessMemoryPrivateUsage}mb");
        }
        if (memoryData.ProcessMemoryPagedUsage >= 1000)
        {
            sw.WriteLine($"Memoria paginada usada pelo processo: {memoryData.ProcessMemoryPagedUsage:F2}gb");    
        } else
        {
            sw.WriteLine($"Memoria paginada usada pelo processo: {memoryData.ProcessMemoryPagedUsage}mb");
        }
        if (memoryData.ProcessMemoryVirtualUsage >= 1000)
        {
            sw.WriteLine($"Memoria virtual usada pelo processo: {memoryData.ProcessMemoryVirtualUsage}gb");    
        } else
        {
            sw.WriteLine($"Memoria virtual usada pelo processo: {memoryData.ProcessMemoryVirtualUsage}mb");  
        }
        if (memoryData.ProcessMemoryPhisyckUsage >= 1000)
        {
            sw.WriteLine($"Memoria fisica usada pelo processo: {memoryData.ProcessMemoryPhisyckUsage}gb");
        } else
        {
            sw.WriteLine($"Memoria fisica usada pelo processo: {memoryData.ProcessMemoryPhisyckUsage}mb");
        }
    }


    public async Task SearchAdvancedProcessByName(string name)
    {
        ProcessClassGetingInfos infos = new ProcessClassGetingInfos(name);

        var ProcessInfo = await infos.GetProcessStructAdvencedAsync();

        Console.WriteLine($"Id do processo: {ProcessInfo.ProcessID}");
        Console.WriteLine($"Nome do Processo: {ProcessInfo.ProcessName}");
        Console.WriteLine($"Caminho do arquivo processo: {ProcessInfo.ProcessPath}");
        if (ProcessInfo.ProcessIsPrivilegied)
        {
            Console.WriteLine($"Processo é privilegiado: true");
        }
        else
        {
            Console.WriteLine($"Processo é privilegiado: false");
        }
        Console.WriteLine($"Numero total de instancias do processo: {ProcessInfo.ProcessInstancesNumber}");
        Console.WriteLine($"Numero total de threads do processo: {ProcessInfo.ProcessThreadsNumber}");
        Console.WriteLine($"Numero Total de modulos do processo: {ProcessInfo.ProcessModulesNumber}");
        PrintMemoryData(ProcessInfo.processMemoryData);
        Console.WriteLine($"Total de CPU usada pelo processo: {ProcessInfo.ProcessCpuUsage:F2}%");
        Console.WriteLine($"Total de disco usado pelo process: {ProcessInfo.ProcessDiskUsage:F2}mbs");
    }

    // Extrai as informações do processo e salva em arquivo TXT
    public async Task ExtractSearchedAdvancedProcessByName(string name, string filePathExtract)
    {
        ProcessClassGetingInfos infos = new ProcessClassGetingInfos(name);
        var ProcessInfo = await infos.GetProcessStructAdvencedAsync();

        // Abre ou cria o arquivo de saída
        using (FileStream fs = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Write))
        {
            // Escritor com encoding UTF8
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine($"Id do processo: {ProcessInfo.ProcessID}");
                sw.WriteLine($"Nome do Processo: {ProcessInfo.ProcessName}");
                sw.WriteLine($"Caminho do arquivo processo: {ProcessInfo.ProcessPath}");

                if (ProcessInfo.ProcessIsPrivilegied)
                {
                    sw.WriteLine($"Processo é privilegiado: true");
                }
                else
                {
                    sw.WriteLine($"Processo é privilegiado: false");
                }

                sw.WriteLine($"Numero total de instancias do processo: {ProcessInfo.ProcessInstancesNumber}");
                sw.WriteLine($"Numero total de threads do processo: {ProcessInfo.ProcessThreadsNumber}");
                sw.WriteLine($"Numero Total de modulos do processo: {ProcessInfo.ProcessModulesNumber}");
                WriteInFileMemoryData(ProcessInfo.processMemoryData, sw);
                sw.WriteLine($"Total de CPU usada pelo processo: {ProcessInfo.ProcessCpuUsage:F2}%");
                sw.WriteLine($"Total de disco usado pelo process: {ProcessInfo.ProcessDiskUsage:F2}mbs");

                // Assinatura/autoria no final do arquivo
                sw.WriteLine($"\n\n\nHenriDev ¯\\_(ツ)_/¯");
            }
        }
    }

    public async Task ExtractSearchedProcessByName(string name, string filePathExtract)
    {
        ProcessClassGetingInfos infos = new ProcessClassGetingInfos(name);
        var ProcessInfo = await infos.GetProcessStructAsync();

        // Abre ou cria o arquivo de saída
        using (FileStream fs = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Write))
        {
            // Escritor com encoding UTF8
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                // Escreve os dados do processo no arquivo
                sw.WriteLine($"Id do processo: {ProcessInfo.ProcessID}");
                sw.WriteLine($"Nome do Processo: {ProcessInfo.ProcessName}");
                sw.WriteLine($"Caminho do arquivo processo: {ProcessInfo.ProcessPath}");
                sw.WriteLine($"Numero total de instancias do processo: {ProcessInfo.ProcessInstancesNumber}");
                sw.WriteLine($"Total de memoria usada pelo processo: {ProcessInfo.ProcessMemoryUsageMb}mb");
                sw.WriteLine($"Total de CPU usada pelo processo: {ProcessInfo.ProcessCpuUsage}");                
            }
        }
    }

    // Extrai as informações do processo e salva em JSON
    public async Task ExtractJsonSearchedProcessByName(string name, string JsonFilePathExtract)
    {
        ProcessClassGetingInfos infos = new ProcessClassGetingInfos(name);
        var ProcessInfo = await infos.GetProcessStructAsync();

        // Valida se o caminho é nulo ou não termina com .json
        if (JsonFilePathExtract == null || !(JsonFilePathExtract.EndsWith(".json")))
        {
            Console.WriteLine("o arquivo inserido é invalido baby");
            return;
        }

        // Abre/cria o arquivo JSON
        using (FileStream fs = new FileStream(JsonFilePathExtract, FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                // Serializa o objeto ProcessInfo para JSON
                var json = JsonSerializer.Serialize(ProcessInfo);

                // Escreve o JSON no arquivo
                sw.WriteLine(json);
            }
        }
    }

    // Cria e configura o comando "search" do System.CommandLine
    public Command GetSearchCommand()
    {
        // Opção para nome do processo
        Option<string> barraN = new("/n", "/name");

        // Opção para exportar TXT
        Option<string> BarraE = new("/e", "/extract");

        // Opção para exportar JSON
        Option<string> BarraEJ = new("/ej", "/extractJson");

        Option<bool> BarraA = new("/a");

        // Criação do comando principal
        Command searchCommand = new Command("search", "Realiza a busca de informações basicas de um processo");

        // Adiciona as opções ao comando
        searchCommand.Options.Add(barraN);
        searchCommand.Options.Add(BarraE);
        searchCommand.Options.Add(BarraEJ);
        searchCommand.Options.Add(BarraA);

        // Define a ação executada quando o comando for chamado
        searchCommand.SetAction(async (result) =>
        {
            Console.Clear();

            // Recupera valores das opções
            var value1 = result.GetValue<string>(barraN);
            var value2 = result.GetValue<string>(BarraE);
            var value3 = result.GetValue<string>(BarraEJ);
            var value4 = result.GetValue<bool>(BarraA);

            // Valida nome do processo
            if (string.IsNullOrWhiteSpace(value1))
            {
                Console.WriteLine("Valor que foi inserido é incorreto ou nulo");
                return;
            }
            // Se só nome foi passado -> mostra no console
            else if (string.IsNullOrWhiteSpace(value2) && string.IsNullOrWhiteSpace(value3))
            {
                if (value4)
                    await SearchAdvancedProcessByName(value1);
                else
                    await SearchProcessByName(value1);
                return;
            }
            // Se passou TXT -> exporta TXT
            else if (string.IsNullOrWhiteSpace(value3))
            {
                if (value4)
                    await ExtractSearchedAdvancedProcessByName(value1,value2);
                else 
                    await ExtractSearchedProcessByName(value1,value2);
                return;
            }
            // Se passou JSON -> exporta JSON
            else if (string.IsNullOrWhiteSpace(value2))
            {
                await ExtractJsonSearchedProcessByName(value1, value3);
                return;
            }
            // Caso o usuário use opções conflitantes
            else
            {
                Console.WriteLine("Teu commando ta todo errado o noobzão");
                return;
            }

        });

        // Retorna o comando configurado
        return searchCommand;
    }
}