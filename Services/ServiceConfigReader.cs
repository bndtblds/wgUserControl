using System.Runtime.InteropServices;

namespace WgUserControl.Services;

internal static class ServiceConfigReader
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct QueryServiceConfigStruct
    {
        public uint ServiceType;
        public uint StartType;
        public uint ErrorControl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string BinaryPathName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LoadOrderGroup;
        public uint TagId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Dependencies;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ServiceStartName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DisplayName;
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryServiceConfig(nint service, nint queryServiceConfig, uint bufferSize, out uint bytesNeeded);

    public static string ReadServiceStartName(string serviceName)
    {
        using var scm = new SafeServiceHandle(NativeMethods.OpenSCManager(null, null, NativeMethods.ScManagerConnect));
        using var service = new SafeServiceHandle(NativeMethods.OpenService(scm.Handle, serviceName, NativeMethods.ServiceQueryConfig));

        QueryServiceConfig(service.Handle, 0, 0, out var needed);
        var buffer = Marshal.AllocHGlobal((int)needed);
        try
        {
            if (!QueryServiceConfig(service.Handle, buffer, needed, out _))
            {
                throw Win32ExceptionFactory.FromLastWin32Error();
            }

            var config = Marshal.PtrToStructure<QueryServiceConfigStruct>(buffer);
            return config.ServiceStartName;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
