using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;

namespace SpaceHosting.Index.Tests.Helpers
{
    public class ApprovalTestsReporter : FirstWorkingReporter
    {
        public ApprovalTestsReporter()
            : base(
                TortoiseGitTextDiffReporter.INSTANCE,
                RiderReporter.INSTANCE,
                FrameworkAssertReporter.INSTANCE
            )
        {
        }
    }
}
