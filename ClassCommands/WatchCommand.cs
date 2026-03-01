using System.CommandLine;
using System.Diagnostics;
using Spectre.Console;

public class WatchCommand
{
    public async Task<List<string>> GetAllProcessNames()
    {
        return null;
    }

    public async Task ShowTable()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Id")
            .AddColumn("Name")
            .AddColumn(new TableColumn("Memory").RightAligned())
            .AddColumn(new TableColumn("CPU").RightAligned());

        var processes = Process.GetProcesses();

        // limita paralelismo pra não travar o sistema
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        var tasks = processes.Select(async process =>
        {
            await semaphore.WaitAsync();
            try
            {
                var getter = new ProcessClassGetingInfos(process.ProcessName);
                return await getter.GetProcessStructAsync();
            }
            catch
            {
                return null; // ignora processos que morreram ou deram acesso negado
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var infos in results.Where(x => x != null))
        {
            table.AddRow(
                infos.ProcessID.ToString(),
                infos.ProcessName,
                $"{infos.ProcessMemoryUsageMb}mb",
                $"{infos.ProcessCpuUsage:F2}%"
            );
        }

        AnsiConsole.Write(table);
    }

    public async Task ShowUnicProcessInfos(string ProcessName)
    {
        ProcessClassGetingInfos a = new ProcessClassGetingInfos(ProcessName);

        ProcessModel infos;

        // cria nova tabela a cada frame
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Id")
            .AddColumn("Name")
            .AddColumn(new TableColumn("Memory").RightAligned())
            .AddColumn(new TableColumn("CPU").RightAligned());

        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            while (true)
            {
                table.Rows.Clear();

                if (!Process.GetProcesses().Any(p => p.ProcessName == ProcessName))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.WriteLine("Este processo não existe mais, ai n é culpa sua n");
                    return;
                }

                infos = await a.GetProcessStructAsync();

                table.AddRow(
                    infos.ProcessID.ToString(),
                    infos.ProcessName,
                    $"{infos.ProcessMemoryUsageMb}mb",
                    $"{infos.ProcessCpuUsage:F2}%"
                );

                await Task.Delay(1000);
                ctx.Refresh();

            }
        });
    }

    public async Task ShowUnicProcessAdvancedInfos(string ProcessName)
    {
        ProcessClassGetingInfos a = new ProcessClassGetingInfos(ProcessName);

        ProcessAdvancedModel infos;

        // cria nova tabela a cada frame
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Id")
            .AddColumn("Name")
            .AddColumn("ModulesNumber")
            .AddColumn("ThreadNumber")
            .AddColumn(new TableColumn("DiskUsage").RightAligned())
            .AddColumn(new TableColumn("CPU").RightAligned())
            .AddColumn("MemoryPhisyckUsage")
            .AddColumns("MemoryVirutalUsage");


        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            while (true)
            {
                table.Rows.Clear();

                if (!Process.GetProcesses().Any(p => p.ProcessName == ProcessName))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.WriteLine("Este processo não existe mais, ai n é culpa sua n");
                    return;
                }

                infos = await a.GetProcessStructAdvencedAsync();

                table.AddRow(
                    infos.ProcessID.ToString(),
                    $"{infos.ProcessName}({infos.ProcessInstancesNumber})",
                    infos.ProcessModulesNumber.ToString(),
                    infos.ProcessThreadsNumber.ToString(),
                    $"{infos.ProcessDiskUsage:F2}mbs",
                    $"{infos.ProcessCpuUsage:F2}%",
                    infos.processMemoryData.ProcessMemoryPhisyckUsage >= 1000 ? $"{infos.processMemoryData.ProcessMemoryPhisyckUsage:F2}gb" : $"{infos.processMemoryData.ProcessMemoryPhisyckUsage}mb",
                    infos.processMemoryData.ProcessMemoryVirtualUsage >= 1000 ? $"{infos.processMemoryData.ProcessMemoryVirtualUsage:F2}gb" : $"{infos.processMemoryData.ProcessMemoryVirtualUsage}mb"
                );

                ctx.Refresh();
                await Task.Delay(1000);
            }
        });
    }

    public Command GetWatchCommand()
    {
        Option<string> BarraUP = new("/up", "/Up", "/UP", "/unicProcess");
        Option<bool> BarraA = new("/a");

        Command watchCommand = new Command("watch", "O comando watch serve para vc assistir em tempo real as metricas do sistema");

        watchCommand.Options.Add(BarraUP);
        watchCommand.Options.Add(BarraA);

        watchCommand.SetAction(async (result) =>
        {
            var value1 = result.GetValue<string>(BarraUP);
            var value2 = result.GetValue<bool>(BarraA);

            if (string.IsNullOrWhiteSpace(value1))
            {
                await ShowTable();
            }
            else
            {
                if (value2)
                    await ShowUnicProcessAdvancedInfos(value1);
                else
                    await ShowUnicProcessInfos(value1);
            }
        });

        return watchCommand;
    }
}