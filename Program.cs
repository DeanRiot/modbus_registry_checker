using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace RegChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            string portNum;
            string slave;
            string command;
            ushort startRegister;
            ushort endRegister;
            ushort range;
            bool saveToFile = false;
            var result = new System.Collections.Generic.List<string>();

            if (args.Length == 0)
            {
                try
                {
                    Console.WriteLine("Port:");
                    portNum = Console.ReadLine();

                    Console.WriteLine("ID Slave:");
                    slave = Console.ReadLine();

                    Console.WriteLine("Command:");
                    command = Console.ReadLine();

                    Console.WriteLine("From register:");
                    startRegister = ushort.Parse(Console.ReadLine());

                    Console.WriteLine("To register:");
                    endRegister = ushort.Parse(Console.ReadLine());

                    Console.WriteLine("Range:");
                    range = ushort.Parse(Console.ReadLine());

                    Console.WriteLine("Save to File?");
                    saveToFile = Console.ReadLine().Equals("y") ? true : false;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error in parameter");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }
            else if (args.Length == 1 && args[0].Equals("help"))
            {
                Console.WriteLine("Command line: [COM1] [SlaveID] [COMMAND] [from reg] [to reg] [range of regs] (File)\n" +
                                    "Commands: \n" +
                                    "Coils - enumerate r/w registers \n" +
                                    "InputRegs - enumerate discrete input regs \n" +
                                    "Inputs - enumerate input regs \n" +
                                    "Holds - enumerate hold regs(range is important) \n" +
                                    "File - (optional) save to txt (y/n)");
                return;
            }
            else if (args.Length == 5 || args.Length == 6)
            {
                try
                {
                    portNum = args[0];
                    slave = args[1];
                    command = args[2];
                    startRegister = ushort.Parse(args[3]);
                    endRegister = ushort.Parse(args[4]);
                    range = ushort.Parse(args[5]);
                    saveToFile = args[6] != null && args[6].Equals("y") ? true : false;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error in parameters");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid number of parameters");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            var port = new SerialPort(portNum, 9600, Parity.None, 8, StopBits.One);

            try
            {
                Console.WriteLine("Opening port");
                port.Open();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{portNum} busy or not connected ");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            var modb = Modbus.Device.ModbusSerialMaster.CreateRtu(port);

            switch (command)
            {
                case "Coils":
                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            var message = $"on reg: {startRegister} {modb.ReadCoils(byte.Parse(slave), startRegister, range)}";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var message = $"on reg: {startRegister} none";
                            Console.WriteLine($"on reg: {startRegister} none");
                            if (saveToFile) result.Add(message);
                        }

                    }
                    break;

                case "InputRegs":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            var message = $"on reg: {startRegister} {modb.ReadInputRegisters(byte.Parse(slave), startRegister, range)}";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var message = $"on reg: {startRegister} none";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                        }
                    }

                    break;

                case "Inputs":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            var message = $"on reg: {startRegister} { modb.ReadInputs(byte.Parse(slave), startRegister, range)}";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var message = $"on reg: {startRegister} none";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                        }
                    }


                    break;

                case "Holds":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            var message = $"on reg: {startRegister} {modb.ReadHoldingRegisters(byte.Parse(slave), startRegister, range)}";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                            Thread.Sleep(250);
                        }

                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var message = $"on reg: {startRegister} {(e.Message.Contains("131") ? "Not correct lenght or adress" : " ") }";
                            Console.WriteLine(message);
                            if (saveToFile) result.Add(message);
                            continue;
                        }
                    }

                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Comands: Coils," +
                        $"InputRegs," +
                        $"Inputs," +
                        $"Holds");
                    break;
            }

            port.Close();
            Console.ForegroundColor = ConsoleColor.White;

            if (saveToFile)
            {
                var fileName = DateTime.Now.ToString();
                fileName = fileName.Replace(" ", "_").Replace(":", "_").Replace(".", "_");
                Console.WriteLine($"Saving Data To File  {fileName}");
                using (StreamWriter sw = new StreamWriter(Path.GetFullPath($"{fileName}.txt")))
                {
                    foreach (string repotrString in result)
                    {
                        sw.WriteLine(repotrString);
                    }

                }
            }
            Console.WriteLine("Job Done");
            Console.ReadKey();
        }

    }
}