using System.CommandLine;
using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics;

public class KillCommand
{
    public void KillByName(string ProcessName, bool NotQuestion = false)
    {
        var processoASerMatado = Process.GetProcessesByName(ProcessName);
        
        if (String.IsNullOrWhiteSpace(ProcessName))
        {
            Console.WriteLine("pow mano, tu acha q eu faço milagre? como q eu vou adivinhar qual processo tu quer q eu mate, se tu nem especifica, ai é foda");
            return;
        }
        else if (!Process.GetProcesses().Any(x => x.ProcessName == ProcessName))
        {
            Console.WriteLine("Este Processo não existe, Sua bintola");
            return;
        }
        else
        {
            if (NotQuestion == false)
            {
                Console.WriteLine("Você deseja mesmo matar este processo?(S/N)");
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.S)
                {
                    try
                    {
                        processoASerMatado.ToList().ForEach(x => x.Kill());
                        return;
                    }
                    catch (Win32Exception)
                    {
                        Console.WriteLine("Acesso negado, tais achando q eu posso matar um processo do sistema? sua bitch");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Ok, N irei fechar o processo");
                }

            }
            else
            {
                try
                {
                    processoASerMatado.ToList().ForEach(x => x.Kill());
                    return;
                }
                catch (Win32Exception)
                {
                    Console.WriteLine("Acesso negado, tais achando q eu posso matar um processo do sistema? sua bitch");
                    return;
                }
            }
        }
    }

    public void KillMultiProcessUsingNames(string ListOfNames, bool NotQuestion = false)
    {
        if (String.IsNullOrWhiteSpace(ListOfNames))
        {
            Console.WriteLine("Tenho carinha de magico? Como eu vou adivinhar quais Processos vc quer matar?");
            return;
        }
        else
        {
            if (!ListOfNames.Contains(',') || !ListOfNames.Contains("(") || !ListOfNames.Contains(")"))
            {
                Console.WriteLine("O Comando foi usado de forma errada, sua bitch burra");
                return;
            }
            else
            {
                string[] ProcessNames = ListOfNames.Split(',','(',')');

                if (NotQuestion)
                {
                    foreach (var ProcessName in ProcessNames)
                    {
                        KillByName(ProcessName);    
                    }
                }
                else
                {
                    Console.WriteLine("Tem certeza que desejas matar estes processos?(S/N)");
                    var key = Console.ReadKey().Key;
                    if (key == ConsoleKey.S)
                    {
                        foreach (var ProcessName in ProcessNames)
                        { 
                            KillByName(ProcessName);    
                        }                            
                    }
                    else
                    {
                        Console.WriteLine("Good Option");
                        return;
                    }
                }
                
            }    
        } 
    }

    public void KillByProcessID(int ProcessID, bool NotQuestion = false)
    {
        if (ProcessID <= 0)
        {
            Console.WriteLine("Mano, Que numero é esse cara, que processo q usa esse PID?");
            return;
        }
        else
        {
            if (Process.GetProcesses().Any(x => x.Id == ProcessID))
            {
                KillByName(Process.GetProcessById(ProcessID).ProcessName, NotQuestion);
            }
            else
            {
                Console.WriteLine("O PiD inserido é invalido");
                return;
            }
        }
    }

    public Command GetKillCommmand()
    {
        Command killCommand = new Command("Kill", "O comando kill serve para matar processos no windows");
        Option<string> barraN = new("/n", "/Name");
        Option<string> barraMN = new("/mn", "/MultiNames");
        Option<int> BarraP = new("/p", "/Pid");
        
        killCommand.Add(BarraP);
        killCommand.Add(barraN);
        killCommand.Add(barraMN);
        
        killCommand.SetAction((result) =>
        {
            var value1 = result.GetValue(BarraP);
            var value2 = result.GetValue(barraN);
            var value3 = result.GetValue(barraMN);

            if (String.IsNullOrWhiteSpace(value3))
            {
                KillByName(value2);
            } 
            else if (String.IsNullOrWhiteSpace(value2))
            {
                KillMultiProcessUsingNames(value3);
            } else if (String.IsNullOrWhiteSpace(value2) || String.IsNullOrWhiteSpace(value3))
            {
                KillByProcessID(value1);
            }
            else
            {
                Console.WriteLine("o comando kill foi usado de forma indevida, seu beta");
            }
        });

        return killCommand;
    }
}