using System;
using System.Reflection;
using ServiceStack;

namespace Acme
{
    public static class MyAssistants
     {
        public static void Patch()
        {
            var licenseUtils = typeof(LicenseUtils);
            var members = licenseUtils.FindMembers(MemberTypes.All, BindingFlags.NonPublic | BindingFlags.Static, null, null);
            Type activatedLicenseType = null;
            foreach (var memberInfo in members)
            {
                if (memberInfo.Name.Equals("__activatedLicense", StringComparison.OrdinalIgnoreCase) && memberInfo is FieldInfo fieldInfo)
                    activatedLicenseType = fieldInfo.FieldType;
            }

            if (activatedLicenseType == null) return;

            var licenseKey = new LicenseKey
            {
                Expiry = DateTime.Now.AddYears(100),
                Ref = "ServiceStack",
                Name = "Enterprise",
                Type = LicenseType.Enterprise
            };
            var constructor = activatedLicenseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(LicenseKey) }, null);
            
            if (constructor == null) return;

            var activatedLicense = constructor.Invoke(new object[] { licenseKey });
            var activatedLicenseField = licenseUtils.GetField("__activatedLicense", BindingFlags.NonPublic | BindingFlags.Static);
            if (activatedLicenseField != null)
                activatedLicenseField.SetValue(null, activatedLicense);
            // LicenseUtils.HasLicensedFeature(LicenseFeature.All) is now true
        }
    }
}
