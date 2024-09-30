using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ease.io_lib;


namespace Ease.io_cmd
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("No command provided. Please specify a command: add, delete, update, get.");
                return;
            }

            EaseLib easeLib = new EaseLib();
            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "add":
                        if (args.Length == 3) // missing optional guid
                        {
                            string name = args[1];
                            DateTime expirationDate = DateTime.Parse(args[2]);
                            Guid guid = Guid.NewGuid();
                            easeLib.Add(name, guid, expirationDate);
                        }
                        else if (args.Length == 4) 
                        {
                            string name = args[1];
                            Guid guid = Guid.Parse(args[2]);
                            DateTime expirationDate = DateTime.Parse(args[3]);
                            easeLib.Add(name, guid, expirationDate);
                        }
                        else
                        {
                            Console.WriteLine("Invalid number of parameters for 'add'. Usage: add <name> [guid] <expirationDate>");
                        }
                        break;
                    case "delete":
                        if (args.Length == 2)
                        {
                            Guid guid = Guid.Parse(args[1]);
                            easeLib.Delete(guid);
                        }
                        else
                        {
                            Console.WriteLine("Invalid number of parameters for 'delete'. Usage: delete <guid>");
                        }
                        break;

                    case "update":
                        if (args.Length == 4)
                        {
                            Guid guid = Guid.Parse(args[1]);
                            string name = args[2];
                            DateTime expirationDate = DateTime.Parse(args[3]);
                            easeLib.Update(guid, name, expirationDate);
                        }
                        else
                        {
                            Console.WriteLine("Invalid number of parameters for 'update'. Usage: update <guid> <name> <expirationDate>");
                        }
                        break;

                    case "get":
                        if (args.Length == 2)
                        {
                            Guid guid = Guid.Parse(args[1]);
                            easeLib.Get(guid);
                        }
                        else
                        {
                            Console.WriteLine("Invalid number of parameters for 'get'. Usage: get <guid>");
                        }
                        break;
                    case "list":
                        List<User> users= easeLib.getAllUsersInCache();
                        break;
                    default:
                        Console.WriteLine($"Unknown command '{command}'. Available commands: add, delete, update, get.");
                        break;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Invalid format for guid or date: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

        }
    }


   
}
