using System;
using System.Collections.Generic;
using ServQLClient;

namespace ServQLTerminal
{
    class Program
    {
        public static Connection connection { get; set; }
        public static Client client { get; set; }

        public static void ClearLine()
        {

            Console.CursorLeft = 0;
            Console.Write(new String(' ',50));
            Console.CursorLeft = 0;

        }
        public static string getPassword()
        {
            string result = string.Empty;
            int cursorPosition = Console.CursorLeft;
            ConsoleKeyInfo consoleKeyInfo;

            do
            {
                Console.CursorLeft = cursorPosition;
                Console.Write(new string(' ', result.Length + 1));
                Console.CursorLeft = cursorPosition;
                Console.Write(new string('*',result.Length));
                consoleKeyInfo = Console.ReadKey(true);
                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                        if (result.Length != 0) result = result.Remove(result.Length - 1, 1);
                        break;
                    case ConsoleKey.Enter:

                        break;
                    default:
                        result += consoleKeyInfo.KeyChar;
                        break;
                }

            }
            while (consoleKeyInfo.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return result;
        }
        static void Main(string[] args)
        {
            bool header = false;
            Console.WriteLine("ServQL Client");
            int cursor = 0;
            if (args.Length == 1) connection = new Connection(args[0]);
            else
            {
                Console.Write("IP: ");
                connection = new Connection(Console.ReadLine());
                cursor = Console.CursorTop;
                Console.WriteLine("Connecting...");
            }
            try
            {
                client = new Client(connection);

            }catch(Exception E)
            {
                Console.CursorTop = cursor;
                ClearLine();
                Console.WriteLine($"Error connecting to {connection.ip}");
                return;
            }
            String username = string.Empty;
            string password = string.Empty;
            while (!client.isLoged)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.CursorTop = cursor;
                    ClearLine();
                    Console.WriteLine($"Error connecting to {connection.ip}");
                    return;
                }

                
                Console.CursorTop = cursor;
                ClearLine();
                Console.Write("Username: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                password = getPassword();
                client.Login(username, password);
                if(!client.isLoged)
                {
                    connection.Close();
                    Console.WriteLine("incorrect user or password");

                }
            }

            string command;
            string cArgs;
            string[] commandSplited;
            Package.Response response = null;
            string prefix = "";
            while (true)
            {
                Console.Write($"{prefix}> ");
                command = Console.ReadLine();
                if (command == "exec" && prefix == "")
                {
                    prefix = "exec";
                }
                else if (command == "exit")
                {
                    if (prefix == "") break;
                    else prefix = "";
                }
                else
                {
                    if (prefix == "")
                    {
                        commandSplited = command.Split();
                        command = commandSplited[0];
                        cArgs = string.Join(' ', commandSplited);
                        cArgs = cArgs.Remove(0, command.Length).Trim();

                    }
                    else
                    {
                        cArgs = command;
                        command = prefix;
                    }
                    switch (command)
                    {
                        case "open":
                            response = client.OpenDb(cArgs);
                            break;
                        case "close":
                            response = client.CloseDb();
                            break;
                        case "list":
                            
                            if (cArgs == "")
                            {
                                header = false;
                                response = client.GetDBsI();

                            }
                            else
                            {
                            response = client.sendCommand(command, cArgs);

                            }
                            break;
                        default:
                            header = true;
                             response = client.sendCommand(command, cArgs);
                            break;

                    }
                }
                if (response != null)
                {
                    if (response?.Result == "ERROR") Console.WriteLine("ERROR");
                    else
                    {
                        foreach (string[] data in response.Data)
                        {
                            Console.WriteLine(String.Join(" , ", data));
                            if (header)
                            {
                                Console.WriteLine(new string('-',data.Length + 2));
                                header = false;
                            }

                        }
                    }
                    response = null;
                }

                
            }



        }
    }
}
