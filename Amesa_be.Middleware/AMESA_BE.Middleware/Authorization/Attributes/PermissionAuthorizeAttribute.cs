using Microsoft.AspNetCore.Authorization;
using AMESA_be.common.Enums;
using System.Security.Permissions;

namespace AMESA_be.Middleware.Authorization.Attributes
{
    public class AuthorizeUserPermissionAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "Permissions.";
        const string POLICY_TYPE_PREFIX = $"{POLICY_PREFIX}Type";
        const string POLICY_VALUE_PREFIX = $"{POLICY_PREFIX}Value";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AuthorizeUserPermissionAttribute(string permissionType, params Permissions[] permissionValues)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            PermissionType = permissionType;
            if (permissionValues != null)
            {
                PermissionValues = permissionValues.Select(p => p.ToString()).ToArray();
            }
        }


        private string _permissionType;
        public string PermissionType
        {
            get
            {
                return _permissionType;
            }
            set
            {
                _permissionType = value;
                Policy = $"{POLICY_TYPE_PREFIX}{value};{POLICY_VALUE_PREFIX}{string.Join(":", PermissionValues)}";
            }
        }

        private string[] _permissionValues;
        public string[] PermissionValues
        {
            get
            {
                return _permissionValues ?? Array.Empty<string>();
            }
            set
            {
                if (value != null)
                {
                    _permissionValues = value;
                    Policy = $"{POLICY_TYPE_PREFIX}{PermissionType};{POLICY_VALUE_PREFIX}{string.Join(":", value)}";
                }
            }
        }
    }
}
