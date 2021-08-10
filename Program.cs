using System;
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
            if (args == null)
            {
                Console.WriteLine("номер порта:");
                portNum = Console.ReadLine();

                Console.WriteLine("ID Slave:");
                slave = Console.ReadLine();

                Console.WriteLine("Команда:");
                command = Console.ReadLine();

                Console.WriteLine("От какого регистра");
                startRegister = ushort.Parse(Console.ReadLine());

                Console.WriteLine("До какого регистра");
                endRegister = ushort.Parse(Console.ReadLine());

                Console.WriteLine("Количество регистров");
                range = ushort.Parse(Console.ReadLine());

            }
            else if (args[0].Equals("help"))
            {
                Console.WriteLine("Command line: [COM1] [SlaveID] [COMMAND] [from reg] [to reg] [range of regs] (File)\n" +
                                    "Commands: \n" +
                                    "Coils - enumerate r/w registers \n" +
                                    "InputRegs - enumerate discrete input regs \n" +
                                    "Inputs - enumerate input regs \n" +
                                    "Holds - enumerate hold regs(range is important) \n" +
                                    "File - (optional) save to txt");
                return;
            }
            else
            {
                try
                {
                    portNum = args[0];
                    slave = args[1];
                    command = args[2];
                    startRegister = ushort.Parse(args[3]);
                    endRegister = ushort.Parse(args[4]);
                    range = ushort.Parse(args[5]);
                }
                catch
                {
                    Console.WriteLine("Error in parameters");
                    return;
                }
            }
            var port = new SerialPort(portNum, 9600, Parity.None, 8, StopBits.One);

            port.Open();
            var modb = Modbus.Device.ModbusSerialMaster.CreateRtu(port);
            switch (command)
            {
                case "Coils":
                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.WriteLine($"on reg: {startRegister} {modb.ReadCoils(byte.Parse(slave), startRegister, range)}");
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.WriteLine($"on reg: {startRegister} none");
                        }

                    }
                    break;

                case "InputRegs":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.WriteLine($"on reg: {startRegister} {modb.ReadInputRegisters(byte.Parse(slave), startRegister, range)}");
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.WriteLine($"on reg: {startRegister} none");
                        }
                    }

                    break;

                case "Inputs":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.WriteLine($"on reg: {startRegister} { modb.ReadInputs(byte.Parse(slave), startRegister, range)}");
                            Thread.Sleep(500);
                        }
                        catch
                        {
                            Console.WriteLine($"on reg: {startRegister} none");
                        }
                    }


                    break;

                case "Holds":

                    for (; startRegister < endRegister; startRegister++)
                    {
                        try
                        {
                            Console.WriteLine($"on reg: {startRegister} {modb.ReadHoldingRegisters(byte.Parse(slave), startRegister, range)}");
                            Thread.Sleep(250);
                        }

                        catch (Exception e)
                        {
                            Console.WriteLine($"on reg: {startRegister} {(e.Message.Contains("131") ? "Not correct lenght or adress" : " ") }");
                            continue;
                        }
                    }

                    break;

                default:
                    Console.WriteLine($"Comands: Coils," +
                        $"InputRegs," +
                        $"Inputs," +
                        $"Holds");
                    break;

            }
            port.Close();
            Console.WriteLine("Job Done");
            Console.ReadKey();
        }
    }
}
