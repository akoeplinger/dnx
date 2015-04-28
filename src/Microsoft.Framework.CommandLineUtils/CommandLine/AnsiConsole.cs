// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.Framework.Runtime.Common.CommandLine
{
    internal class AnsiConsole
    {
        private AnsiConsole(TextWriter writer, IServiceProvider services)
        {
            Writer = writer;

            var runtimeEnv = (IRuntimeEnvironment)services.GetService(typeof(IRuntimeEnvironment));
            var shouldParse = runtimeEnv.OperatingSystem == "Windows" || runtimeEnv.RuntimeType == "Mono";

            if (shouldParse)
            {
                WriteLine = new WriteDelegate(WriteLineParse);
                OriginalForegroundColor = Console.ForegroundColor;
            }
            else
            {
                WriteLine = new WriteDelegate(WriteLineNoParse);
            }
        }

        private int _boldRecursion;
        public delegate void WriteDelegate(string message);
        public WriteDelegate WriteLine;
        static private AnsiConsole Output = null;
        static private AnsiConsole Error = null;

        public static AnsiConsole GetOutput(IServiceProvider services)
        {
            if (Output == null)
            {
                Output = new AnsiConsole(Console.Out, services);
            }
            return Output;
        }

        public static AnsiConsole GetError(IServiceProvider services)
        {
            if (Error == null)
            {
                Error = new AnsiConsole(Console.Error, services);
            }
            return Error;
        }

        public TextWriter Writer { get; }

        public ConsoleColor OriginalForegroundColor { get; }

        private void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = (ConsoleColor)(((int)Console.ForegroundColor & 0x08) | ((int)color & 0x07));
        }

        private void SetBold(bool bold)
        {
            _boldRecursion += bold ? 1 : -1;
            if (_boldRecursion > 1 || (_boldRecursion == 1 && !bold))
            {
                return;
            }

            Console.ForegroundColor = (ConsoleColor)((int)Console.ForegroundColor ^ 0x08);
        }

        private void WriteLineNoParse(string message)
        {
            Console.WriteLine(message);
        }

        private void WriteLineParse(string message)
        {
            var sb = new StringBuilder();
            var escapeScan = 0;
            for (; ;)
            {
                var escapeIndex = message.IndexOf("\x1b[", escapeScan);
                if (escapeIndex == -1)
                {
                    var text = message.Substring(escapeScan);
                    sb.Append(text);
                    Writer.Write(text);
                    break;
                }
                else
                {
                    var startIndex = escapeIndex + 2;
                    var endIndex = startIndex;
                    while (endIndex != message.Length &&
                        message[endIndex] >= 0x20 &&
                        message[endIndex] <= 0x3f)
                    {
                        endIndex += 1;
                    }

                    var text = message.Substring(escapeScan, escapeIndex - escapeScan);
                    sb.Append(text);
                    Writer.Write(text);
                    if (endIndex == message.Length)
                    {
                        break;
                    }

                    switch (message[endIndex])
                    {
                        case 'm':
                            int value;
                            if (int.TryParse(message.Substring(startIndex, endIndex - startIndex), out value))
                            {
                                switch (value)
                                {
                                    case 1:
                                        SetBold(true);
                                        break;
                                    case 22:
                                        SetBold(false);
                                        break;
                                    case 30:
                                        SetColor(ConsoleColor.Black);
                                        break;
                                    case 31:
                                        SetColor(ConsoleColor.Red);
                                        break;
                                    case 32:
                                        SetColor(ConsoleColor.Green);
                                        break;
                                    case 33:
                                        SetColor(ConsoleColor.Yellow);
                                        break;
                                    case 34:
                                        SetColor(ConsoleColor.Blue);
                                        break;
                                    case 35:
                                        SetColor(ConsoleColor.Magenta);
                                        break;
                                    case 36:
                                        SetColor(ConsoleColor.Cyan);
                                        break;
                                    case 37:
                                        SetColor(ConsoleColor.Gray);
                                        break;
                                    case 39:
                                        SetColor(OriginalForegroundColor);
                                        break;
                                }
                            }
                            break;
                    }

                    escapeScan = endIndex + 1;
                }
            }
            Writer.WriteLine();
        }
    }
}
