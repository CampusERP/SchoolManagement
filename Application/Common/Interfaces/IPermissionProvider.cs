namespace Application.Common.Interfaces;

public interface IPermissionProvider
{
    List<string> GetPermissionsForRole(string role);
    List<string> GetPlatformAdminPermissions();
}