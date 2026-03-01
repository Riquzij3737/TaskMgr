using System.CommandLine;
using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics;

public class KillCommand
{
    public void KillByName(string ProcessName, bool NotQuestion = false, bool force = false)
    {
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
                        KillModes(Process.GetProcessesByName(ProcessName).First(), force);
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
                    KillModes(Process.GetProcessesByName(ProcessName).First(), force);
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

    public void KillModes(Process process, bool Force = false)
    {
        try
        {
            if (Force)
            {
                process.Kill();
                return;
            }
            else
            {
                process.CloseMainWindow();
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Houve um erro interno");
        }
    }

    public void KillMultiProcessUsingNames(string ListOfNames, bool NotQuestion = false, bool force = false)
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
                        KillByName(ProcessName, NotQuestion, force);    
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
                            KillByName(ProcessName, true, force);    
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

    public void KillByProcessID(int ProcessID, bool NotQuestion = false, bool force = true)
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
                KillByName(Process.GetProcessById(ProcessID).ProcessName, NotQuestion, force);
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
        Option<bool> BarraNq = new("/nq", "/NotQuestion");
        Option<bool> BarraF = new("/f", "/Force");
        
        killCommand.Add(BarraP);
        killCommand.Add(barraN);
        killCommand.Add(barraMN);
        killCommand.Add(BarraNq);
        killCommand.Add(BarraF);
        
        killCommand.SetAction((result) =>
        {
            var value1 = result.GetValue(BarraP);
            var value2 = result.GetValue(barraN);
            var value3 = result.GetValue(barraMN);
            var  value4 = result.GetValue(BarraF);
            var value5 = result.GetValue(BarraNq);

            if (String.IsNullOrWhiteSpace(value3))
            {
                KillByName(value2,value5,value4);
            } 
            else if (String.IsNullOrWhiteSpace(value2))
            {
                KillMultiProcessUsingNames(value3, value5, value4);
            } else if (String.IsNullOrWhiteSpace(value2) || String.IsNullOrWhiteSpace(value3))
            {
                KillByProcessID(value1,value5,value4);
            }
            else
            {
                Console.WriteLine("o comando kill foi usado de forma indevida, seu beta");
            }
        });

        return killCommand;
    }
}