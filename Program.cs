using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
public class Program 
{
    public static async Task<int> Main(string[] args)
    {              
        RootCommand root = new RootCommand("TaskManage é um comando de console usado para gerenciar processos no windows\n\nfeito por HenriDev, Espero que goste:D");  
        SearchCommand searchCommand = new SearchCommand();                                
        WatchCommand watchCommand= new WatchCommand();
        KillCommand killCommand = new KillCommand();

        root.Subcommands.Add(searchCommand.GetSearchCommand());           
        root.Subcommands.Add(watchCommand.GetWatchCommand());
        root.Subcommands.Add(killCommand.GetKillCommmand());

        return await root.Parse(args).InvokeAsync();
    }
}