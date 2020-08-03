using System;
using System.Collections.Generic;
using ServQLClient;

namespace ServQLTerminal
{
    class Program
    {
        public static Connection connection { get; set; }
        public static Client client { get; set; }

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
            Console.WriteLine("ServQL Client");
            if (args.Length == 1) connection = new Connection(args[1]);
            else
            {
                Console.Write("IP: ");
                connection = new Connection(Console.ReadLine());
            }
            client = new Client(connection);
            
            String username = string.Empty;
            string password = string.Empty;
            bool repeat = true;
            while (!client.isLoged)
            {
                try
                {

                    connection.Open();
                }catch (Exception e)
                {
                    Console.WriteLine("Error connecting");
                    System.Environment.Exit(1);
                }
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
            Package.Response response;
            string prefix = "";
            while (true)
            {
                Console.Write($"{prefix}> ");
                command = Console.ReadLine();
                if (command == "exec" && prefix == "")
                {
                    prefix = "exec";
                }
                else if (command == "clear")
                {
                    Console.Clear();
                }
                else if(command == "exit")
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
                    response = client.sendCommand(command,cArgs);
                    if (response?.Result == "ERROR" || response == null) Console.WriteLine($"ERROR:{response.Message}");
                    else
                    {
                        foreach (string[] data in response.Data)
                        {
                            foreach(string d in data)
                            {

                                Console.Write(d + " , ");
                            }
                            Console.WriteLine();
                        }
                    }

                }
            }



        }
    }
}
