using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WgUserControl.Services;

namespace WgUserControl.Tests;

[TestClass]
public sealed class ServiceSecurityManagerTests
{
    [TestMethod]
    public void ExtendsExistingInteractiveUsersAceWithoutAddingDuplicate()
    {
        var descriptor = new RawSecurityDescriptor("D:(A;;CCLCSWLOCRRC;;;IU)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)");

        var changed = ServiceSecurityManager.EnsureInteractiveUsersAce(descriptor);

        Assert.IsTrue(changed);
        var iuAceCount = CountAllowAcesForInteractiveUsers(descriptor);
        Assert.AreEqual(1, iuAceCount);
        AssertInteractiveUsersMaskContainsRequiredRights(descriptor);
    }

    [TestMethod]
    public void LeavesDescriptorUnchangedWhenInteractiveUsersAlreadyHasRequiredRights()
    {
        var descriptor = new RawSecurityDescriptor("D:(A;;CCLCSWRPWPLOCRRC;;;IU)");

        var changed = ServiceSecurityManager.EnsureInteractiveUsersAce(descriptor);

        Assert.IsFalse(changed);
        Assert.AreEqual(1, CountAllowAcesForInteractiveUsers(descriptor));
    }

    private static int CountAllowAcesForInteractiveUsers(RawSecurityDescriptor descriptor)
    {
        var sid = new SecurityIdentifier("S-1-5-4");
        return descriptor.DiscretionaryAcl!.Cast<GenericAce>()
            .OfType<CommonAce>()
            .Count(ace => ace.AceQualifier == AceQualifier.AccessAllowed && ace.SecurityIdentifier.Equals(sid));
    }

    private static void AssertInteractiveUsersMaskContainsRequiredRights(RawSecurityDescriptor descriptor)
    {
        var sid = new SecurityIdentifier("S-1-5-4");
        var ace = descriptor.DiscretionaryAcl!.Cast<GenericAce>()
            .OfType<CommonAce>()
            .Single(a => a.AceQualifier == AceQualifier.AccessAllowed && a.SecurityIdentifier.Equals(sid));

        var required = (int)(NativeMethods.ServiceQueryStatus | NativeMethods.ServiceStart | NativeMethods.ServiceStop);
        Assert.AreEqual(required, ace.AccessMask & required);
    }
}
