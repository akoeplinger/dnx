// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Framework.CommonTestUtils;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;

namespace Microsoft.Framework.PackageManager
{
    public static class DnuTestUtils
    {
        public static int ExecDnu(string runtimeHomePath,
                                  string subcommand,
                                  string arguments,
                                  out string stdOut,
                                  out string stdErr,
                                  IDictionary<string, string> environment = null,
                                  string workingDir = null)
        {
            string program, commandLine;
            var runtimeRoot = Directory.EnumerateDirectories(Path.Combine(runtimeHomePath, "runtimes"), Constants.RuntimeNamePrefix + "*").First();
            var isMono = ((IRuntimeEnvironment)CallContextServiceLocator.Locator.ServiceProvider.GetService(typeof(IRuntimeEnvironment))).RuntimeType == "Mono";
            if (isMono)
            {
                program = Path.Combine(runtimeRoot, "bin", "dnu");
                commandLine = string.Format("{0} {1}", subcommand, arguments);
            }
            else
            {
                program = "cmd";
                var dnuCmdPath = Path.Combine(runtimeRoot, "bin", "dnu.cmd");
                commandLine = string.Format("/C {0} {1} {2}", dnuCmdPath, subcommand, arguments);
            }

            var exitCode = TestUtils.Exec(program, commandLine, out stdOut, out stdErr, environment, workingDir);

            return exitCode;
        }

        public static int ExecDnu(string runtimeHomePath,
                                  string subcommand,
                                  string arguments,
                                  IDictionary<string, string> environment = null,
                                  string workingDir = null)
        {
            string stdOut, stdErr;
            return ExecDnu(runtimeHomePath, subcommand, arguments, out stdOut, out stdErr, environment, workingDir);
        }
    }
}
