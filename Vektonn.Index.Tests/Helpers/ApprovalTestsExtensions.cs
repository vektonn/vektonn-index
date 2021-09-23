using ApprovalTests;
using ApprovalTests.Namers;

namespace Vektonn.Index.Tests.Helpers
{
    public static class ApprovalTestsExtensions
    {
        public static void VerifyApprovalAsJson<T>(this T obj, string objectName)
        {
            NamerFactory.AdditionalInformation = objectName;
            Approvals.VerifyWithExtension(obj.ToPrettyJson(), ".json");
        }
    }
}
