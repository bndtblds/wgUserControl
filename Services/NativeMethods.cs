using System.Runtime.InteropServices;

namespace WgUserControl.Services;

internal static class NativeMethods
{
    public const uint ScManagerConnect = 0x0001;
    public const uint ScManagerEnumerateService = 0x0004;
    public const uint ServiceQueryConfig = 0x0001;
    public const uint ServiceQueryStatus = 0x0004;
    public const uint ServiceStart = 0x0010;
    public const uint ServiceStop = 0x0020;
    public const uint ServiceEnumerateDependents = 0x0008;
    public const uint ServiceInterrogate = 0x0080;
    public const uint ReadControl = 0x00020000;
    public const uint WriteDac = 0x00040000;
    public const uint DaclSecurityInformation = 0x00000004;
    public const uint ServiceWin32 = 0x00000030;
    public const uint ServiceStateAll = 0x00000003;
    public const int ErrorMoreData = 234;
    public const int ErrorInsufficientBuffer = 122;
    public const int ErrorAccessDenied = 5;

    public enum ServiceControl : uint
    {
        Stop = 0x00000001
    }

    public enum ScStatusType
    {
        ProcessInfo = 0
    }

    public enum ServiceState : uint
    {
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatusProcess
    {
        public uint ServiceType;
        public ServiceState CurrentState;
        public uint ControlsAccepted;
        public uint Win32ExitCode;
        public uint ServiceSpecificExitCode;
        public uint CheckPoint;
        public uint WaitHint;
        public uint ProcessId;
        public uint ServiceFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EnumServiceStatusProcess
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ServiceName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DisplayName;
        public ServiceStatusProcess ServiceStatusProcess;
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint OpenSCManager(string? machineName, string? databaseName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint OpenService(nint serviceControlManager, string serviceName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseServiceHandle(nint handle);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool StartService(nint service, uint numServiceArgs, nint serviceArgVectors);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ControlService(nint service, ServiceControl control, out ServiceStatusProcess serviceStatus);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool QueryServiceStatusEx(nint service, ScStatusType infoLevel, nint buffer, uint bufferSize, out uint bytesNeeded);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumServicesStatusEx(
        nint serviceControlManager,
        int infoLevel,
        uint serviceType,
        uint serviceState,
        nint services,
        uint bufferSize,
        out uint bytesNeeded,
        out uint servicesReturned,
        ref uint resumeHandle,
        string? groupName);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool QueryServiceObjectSecurity(
        nint service,
        uint securityInformation,
        byte[]? securityDescriptor,
        uint bufferSize,
        out uint bytesNeeded);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetServiceObjectSecurity(
        nint service,
        uint securityInformation,
        byte[] securityDescriptor);
}
