namespace ModularSaaS.Application.Identity.Permissions;

public static class AppPermissions
{
    public static class Identity
    {
        public const string UsersRead = "Identity.Users.Read";
        public const string UsersWrite = "Identity.Users.Write";
        public const string UsersDelete = "Identity.Users.Delete";
        public const string RolesRead = "Identity.Roles.Read";
        public const string RolesWrite = "Identity.Roles.Write";
        public const string RolesDelete = "Identity.Roles.Delete";
        public const string PermissionsRead = "Identity.Permissions.Read";
    }

    public static class Tenancy
    {
        public const string SettingsRead = "Tenancy.Settings.Read";
        public const string SettingsWrite = "Tenancy.Settings.Write";
        public const string Manage = "Tenancy.Tenants.Manage";
    }

    public static IReadOnlyList<string> All =>
    [
        Identity.UsersRead,
        Identity.UsersWrite,
        Identity.UsersDelete,
        Identity.RolesRead,
        Identity.RolesWrite,
        Identity.RolesDelete,
        Identity.PermissionsRead,
        Tenancy.SettingsRead,
        Tenancy.SettingsWrite,
        Tenancy.Manage
    ];
}
