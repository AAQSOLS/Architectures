namespace ModularSaaS.Application.Shared.Constants;

public static class ValidationConstants
{
    public const int MinPasswordLength = 8;
    public const int MaxNameLength = 100;
    public const int MaxTenantNameLength = 150;
    public const int MaxTenantIdentifierLength = 100;
    public const int MaxDescriptionLength = 250;
    public const string TenantIdentifierRegex = @"^[a-z0-9-]+$";

    public static class Messages
    {
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email address is required.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordMinLength = "Password must be at least 8 characters long.";
        public const string AdminPasswordRequired = "Admin password is required.";
        public const string AdminPasswordMinLength = "Admin password must be at least 8 characters long.";
        public const string FirstNameRequired = "First name is required.";
        public const string LastNameRequired = "Last name is required.";
        public const string AdminFirstNameRequired = "Admin first name is required.";
        public const string AdminLastNameRequired = "Admin last name is required.";
        public const string AdminEmailRequired = "Admin email is required.";
        public const string TenantNameRequired = "Tenant name is required.";
        public const string TenantNameMaxLength = "Tenant name must not exceed 150 characters.";
        public const string TenantIdentifierRequired = "Tenant slug/identifier is required.";
        public const string TenantIdentifierMaxLength = "Tenant identifier must not exceed 100 characters.";
        public const string TenantIdentifierInvalid = "Tenant identifier may only contain lowercase letters, numbers, and hyphens.";
        public const string RoleNameRequired = "Role name is required.";
        public const string PermissionsRequired = "Permissions list is required.";
        public const string ResetTokenRequired = "Reset token is required.";
        public const string NewPasswordRequired = "New password is required.";
        public const string NewPasswordMinLength = "New password must be at least 8 characters long.";
    }
}
