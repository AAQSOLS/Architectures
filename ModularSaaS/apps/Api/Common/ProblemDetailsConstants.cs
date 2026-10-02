namespace ModularSaaS.Api.Common;

internal static class ProblemDetailsConstants
{
    public const string ContentType = "application/problem+json";
    public const string ErrorsKey = "errors";
    public const string DomainExceptionName = "DomainException";

    public const string TenantRequiredTitle = "Tenant Required";
    public const string TenantRequiredType = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";

    public const string ValidationTitle = "Validation Failed";
    public const string ValidationType = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";

    public const string NotFoundTitle = "Resource Not Found";
    public const string NotFoundType = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";

    public const string UnauthorizedTitle = "Unauthorized";
    public const string UnauthorizedType = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1";

    public const string BusinessRuleViolationTitle = "Business Rule Violation";
    public const string BusinessRuleViolationType = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2";

    public const string InternalServerErrorTitle = "Internal Server Error";
    public const string InternalServerErrorDetail = "An unexpected error occurred. Please contact support.";
    public const string InternalServerErrorType = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
}
