using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AMESA_be.Middleware.Authorization.PolicyProviders
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "Permissions.";
        const string POLICY_TYPE_PREFIX = $"{POLICY_PREFIX}Type";
        const string POLICY_VALUE_PREFIX = $"{POLICY_PREFIX}Value";
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy>(null!)!;
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(policyName.Substring(POLICY_PREFIX.Length)))
            {
                var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                //retrieve the policy type and values from the policy name
                var permissions = policyName.Split(';');
                string? permissionTypeValue = ExtractPermissionType(permissions);
                string[]? permissionValues = ExtractPermissionValues(permissions);

                //add policy requirements
                if (permissionTypeValue is not null && permissionValues is not null)
                {
                    policy.RequireClaim(permissionTypeValue, permissionValues);
                }
                else if (permissionTypeValue is not null)
                {
                    policy.RequireClaim(permissionTypeValue);
                }
                return Task.FromResult(policy.Build())!;
            }

            return Task.FromResult<AuthorizationPolicy>(null!)!;
        }

        private static string[]? ExtractPermissionValues(string[] permissions)
        {
            string[]? permissionValues = null;
            var concatPermissions = permissions.FirstOrDefault(p => p.StartsWith(POLICY_VALUE_PREFIX, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(concatPermissions) && concatPermissions != POLICY_VALUE_PREFIX)
            {
                permissionValues = concatPermissions.Substring(POLICY_VALUE_PREFIX.Length).Split(':');
            }

            return permissionValues;
        }

        private static string? ExtractPermissionType(string[] permissions)
        {
            string? permissionTypeValue = null;
            var permissionType = permissions.FirstOrDefault(p => p.StartsWith(POLICY_TYPE_PREFIX, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(permissionType) && permissionType != POLICY_TYPE_PREFIX)
            {
                permissionTypeValue = permissionType.Substring(POLICY_TYPE_PREFIX.Length);
            }

            return permissionTypeValue;
        }
    }
}
