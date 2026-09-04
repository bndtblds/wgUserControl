using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WgUserControl.Services;

internal sealed class ServiceSecurityManager
{
    private static readonly SecurityIdentifier InteractiveUsersSid = new("S-1-5-4");
    private static readonly int RequiredAccessMask = (int)(NativeMethods.ServiceQueryStatus | NativeMethods.ServiceStart | NativeMethods.ServiceStop);
    private readonly AppLogger logger;

    public ServiceSecurityManager(AppLogger logger)
    {
        this.logger = logger;
    }

    public void EnsureInteractiveUsersCanOperateTunnel(string serviceName)
    {
        using var scm = new SafeServiceHandle(NativeMethods.OpenSCManager(null, null, NativeMethods.ScManagerConnect));
        using var service = new SafeServiceHandle(NativeMethods.OpenService(scm.Handle, serviceName, NativeMethods.ReadControl | NativeMethods.WriteDac));

        var descriptor = ReadDacl(service.Handle);
        var updated = EnsureInteractiveUsersAce(descriptor);
        if (!updated)
        {
            logger.Info($"Service DACL already contains required Interactive Users rights: {serviceName}");
            return;
        }

        var bytes = new byte[descriptor.BinaryLength];
        descriptor.GetBinaryForm(bytes, 0);
        if (!NativeMethods.SetServiceObjectSecurity(service.Handle, NativeMethods.DaclSecurityInformation, bytes))
        {
            throw Win32ExceptionFactory.FromLastWin32Error();
        }

        logger.Info($"Service DACL updated for Interactive Users: {serviceName}");
    }

    internal static bool EnsureInteractiveUsersAce(RawSecurityDescriptor descriptor)
    {
        var dacl = descriptor.DiscretionaryAcl ?? new RawAcl(GenericAcl.AclRevision, 1);
        var changed = false;

        for (var i = 0; i < dacl.Count; i++)
        {
            if (dacl[i] is not CommonAce ace
                || ace.AceQualifier != AceQualifier.AccessAllowed
                || !ace.SecurityIdentifier.Equals(InteractiveUsersSid))
            {
                continue;
            }

            var merged = ace.AccessMask | RequiredAccessMask;
            if (merged == ace.AccessMask)
            {
                descriptor.DiscretionaryAcl = dacl;
                return changed;
            }

            dacl.RemoveAce(i);
            dacl.InsertAce(i, new CommonAce(ace.AceFlags, ace.AceQualifier, merged, ace.SecurityIdentifier, ace.IsCallback, ace.GetOpaque()));
            descriptor.DiscretionaryAcl = dacl;
            return true;
        }

        dacl.InsertAce(dacl.Count, new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, RequiredAccessMask, InteractiveUsersSid, isCallback: false, opaque: null));
        descriptor.DiscretionaryAcl = dacl;
        return true;
    }

    private static RawSecurityDescriptor ReadDacl(nint service)
    {
        NativeMethods.QueryServiceObjectSecurity(service, NativeMethods.DaclSecurityInformation, null, 0, out var needed);
        var error = Marshal.GetLastWin32Error();
        if (error != NativeMethods.ErrorInsufficientBuffer || needed == 0)
        {
            throw new Win32Exception(error);
        }

        var buffer = new byte[needed];
        if (!NativeMethods.QueryServiceObjectSecurity(service, NativeMethods.DaclSecurityInformation, buffer, needed, out _))
        {
            throw Win32ExceptionFactory.FromLastWin32Error();
        }

        return new RawSecurityDescriptor(buffer, 0);
    }
}
