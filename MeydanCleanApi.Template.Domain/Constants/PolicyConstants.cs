namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Defines authorization policy names used in <c>[Authorize(Policy = ...)]</c> attributes.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Customization Guide:</strong>
/// To add a new policy, define a string constant here and register it in the API authorization policy pipeline.
/// </para>
/// <code>
/// [Authorize(Policy = PolicyConstants.Admin)]
/// public class AdminDashboardController : ControllerBase { }
/// </code>
/// </remarks>
public static class PolicyConstants
{
    /// <summary>Policy restricting access strictly to SuperAdmin users.</summary>
    public const string SuperAdmin = "SuperAdminPolicy";

    /// <summary>Policy granting access to Admin and SuperAdmin users.</summary>
    public const string Admin = "AdminPolicy";

    /// <summary>Policy restricting access to Customer users.</summary>
    public const string Customer = "CustomerPolicy";
}
