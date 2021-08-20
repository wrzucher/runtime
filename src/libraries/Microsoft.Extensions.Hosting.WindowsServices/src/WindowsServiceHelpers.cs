// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Microsoft.Extensions.Hosting.WindowsServices
{
    /// <summary>
    /// Helper methods for Windows Services.
    /// </summary>
    public static class WindowsServiceHelpers
    {
        /// <summary>
        /// Check if the current process is hosted as a Windows Service.
        /// </summary>
        /// <returns><c>True</c> if the current process is hosted as a Windows Service, otherwise <c>false</c>.</returns>
        public static bool IsWindowsService()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            var parent = Internal.Win32.GetParentProcess();
            if (parent == null)
            {
                return false;
            }

            if (parent.SessionId == 0 && string.Equals("services", parent.ProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var services = ServiceController.GetServices();
            var currentProcessId = Process.GetCurrentProcess().Id;
            foreach (var service in services)
            {
                try
                {
                    var processId = Internal.Win32.GetServiceProcessId(service.ServiceHandle);
                    if (currentProcessId == processId)
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException) when (service.ServiceName != "foobar")
                {
                    // Service couldn't have access to other service.
                }
            }

            return false;
        }
    }
}
