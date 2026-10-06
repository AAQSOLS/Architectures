namespace ModularSaaS.Api.Common;

public static class ApiRoutes
{
    public const string PlatformSegment = "platform";
    public const string PlatformPathSegment = "/platform";
    public const string VersionPrefix = "api/v{version:apiVersion}";
    public const string PlatformPrefix = $"{VersionPrefix}/{PlatformSegment}";

    public static class Auth
    {
        public const string Prefix = $"{VersionPrefix}/auth";
        public const string Login = "login";
        public const string Refresh = "refresh";
        public const string Revoke = "revoke";
        public const string ForgotPassword = "forgot-password";
        public const string ResetPassword = "reset-password";
    }

    public static class PlatformAuth
    {
        public const string Prefix = $"{VersionPrefix}/platform";
        public const string Login = "auth/login";
        public const string Impersonate = "tenants/{tenantId:guid}/impersonate";
    }

    public static class PlatformTenants
    {
        public const string Prefix = $"{VersionPrefix}/platform/tenants";
        public const string ById = "{id:guid}";
        public const string Suspend = "{id:guid}/suspend";
        public const string Activate = "{id:guid}/activate";
    }

    public static class Users
    {
        public const string Prefix = $"{VersionPrefix}/users";
        public const string Me = "me";
        public const string Register = "register";
        public const string ChangePassword = "change-password";
        public const string AssignRole = "{id:guid}/roles";
        public const string RemoveRole = "{id:guid}/roles/{roleId:guid}";
    }

    public static class Roles
    {
        public const string Prefix = $"{VersionPrefix}/roles";
        public const string ById = "{id:guid}";
    }

    public static class Permissions
    {
        public const string Prefix = $"{VersionPrefix}/permissions";
    }
}
