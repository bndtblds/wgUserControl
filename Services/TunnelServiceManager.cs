using System.ComponentModel;
using System.Runtime.InteropServices;
using WgUserControl.Models;

namespace WgUserControl.Services;

internal sealed class TunnelServiceManager
{
    private readonly AppLogger logger;

    public TunnelServiceManager(AppLogger logger)
    {
        this.logger = logger;
    }

    public IReadOnlyList<string> DiscoverManagedServiceNames()
    {
        using var scm = OpenScm(NativeMethods.ScManagerConnect | NativeMethods.ScManagerEnumerateService);
        uint resume = 0;
        NativeMethods.EnumServicesStatusEx(scm.Handle, 0, NativeMethods.ServiceWin32, NativeMethods.ServiceStateAll, 0, 0, out var needed, out _, ref resume, null);
        var error = Marshal.GetLastWin32Error();
        if (error != NativeMethods.ErrorMoreData || needed == 0)
        {
            return [];
        }

        var buffer = Marshal.AllocHGlobal((int)needed);
        try
        {
            resume = 0;
            if (!NativeMethods.EnumServicesStatusEx(scm.Handle, 0, NativeMethods.ServiceWin32, NativeMethods.ServiceStateAll, buffer, needed, out _, out var returned, ref resume, null))
            {
                throw Win32ExceptionFactory.FromLastWin32Error();
            }

            var size = Marshal.SizeOf<NativeMethods.EnumServiceStatusProcess>();
            var names = new List<string>();
            for (var i = 0; i < returned; i++)
            {
                var item = Marshal.PtrToStructure<NativeMethods.EnumServiceStatusProcess>(buffer + (i * size));
                if (TunnelNameService.IsManagedServiceName(item.ServiceName))
                {
                    names.Add(item.ServiceName);
                }
            }

            logger.Info($"Detected {names.Count} managed tunnel services.");
            return names;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public bool ServiceExists(string serviceName)
    {
        try
        {
            using var scm = OpenScm(NativeMethods.ScManagerConnect);
            using var service = OpenService(scm, serviceName, NativeMethods.ServiceQueryStatus);
            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    public TunnelRuntimeStatus GetStatus(string serviceName)
    {
        using var scm = OpenScm(NativeMethods.ScManagerConnect);
        using var service = OpenService(scm, serviceName, NativeMethods.ServiceQueryStatus);
        return QueryStatus(service.Handle);
    }

    public void Start(string serviceName)
    {
        using var scm = OpenScm(NativeMethods.ScManagerConnect);
        using var service = OpenService(scm, serviceName, NativeMethods.ServiceStart | NativeMethods.ServiceQueryStatus);
        if (!NativeMethods.StartService(service.Handle, 0, 0))
        {
            throw Win32ExceptionFactory.FromLastWin32Error();
        }

        logger.Info($"Service start requested: {serviceName}");
    }

    public void Stop(string serviceName)
    {
        using var scm = OpenScm(NativeMethods.ScManagerConnect);
        using var service = OpenService(scm, serviceName, NativeMethods.ServiceStop | NativeMethods.ServiceQueryStatus);
        if (!NativeMethods.ControlService(service.Handle, NativeMethods.ServiceControl.Stop, out _))
        {
            throw Win32ExceptionFactory.FromLastWin32Error();
        }

        logger.Info($"Service stop requested: {serviceName}");
    }

    internal static TunnelRuntimeStatus MapStatus(NativeMethods.ServiceState state) => state switch
    {
        NativeMethods.ServiceState.Running => TunnelRuntimeStatus.Running,
        NativeMethods.ServiceState.Stopped => TunnelRuntimeStatus.Stopped,
        NativeMethods.ServiceState.StartPending => TunnelRuntimeStatus.StartPending,
        NativeMethods.ServiceState.StopPending => TunnelRuntimeStatus.StopPending,
        _ => TunnelRuntimeStatus.Unknown
    };

    private static TunnelRuntimeStatus QueryStatus(nint service)
    {
        var size = Marshal.SizeOf<NativeMethods.ServiceStatusProcess>();
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            if (!NativeMethods.QueryServiceStatusEx(service, NativeMethods.ScStatusType.ProcessInfo, buffer, (uint)size, out _))
            {
                throw Win32ExceptionFactory.FromLastWin32Error();
            }

            var status = Marshal.PtrToStructure<NativeMethods.ServiceStatusProcess>(buffer);
            return MapStatus(status.CurrentState);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static SafeServiceHandle OpenScm(uint access) =>
        new(NativeMethods.OpenSCManager(null, null, access));

    private static SafeServiceHandle OpenService(SafeServiceHandle scm, string serviceName, uint access) =>
        new(NativeMethods.OpenService(scm.Handle, serviceName, access));
}
