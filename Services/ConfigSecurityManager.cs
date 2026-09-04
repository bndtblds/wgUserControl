using System.Security.AccessControl;
using System.Security.Principal;

namespace WgUserControl.Services;

internal sealed class ConfigSecurityManager
{
    private readonly AppLogger logger;

    public ConfigSecurityManager(AppLogger logger)
    {
        this.logger = logger;
    }

    public void SecureConfigFile(string path)
    {
        var system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

        var security = new FileSecurity();
        security.SetOwner(administrators);
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(system, FileSystemRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(administrators, FileSystemRights.FullControl, AccessControlType.Allow));
        new FileInfo(path).SetAccessControl(security);

        logger.Info($"Restrictive ACL applied to WireGuard config: {path}");
    }
}
