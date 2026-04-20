using System.Text;
using NUnit.Framework;

namespace WikipediaAutomation.Tests.Support;

/// <summary>
/// Helpers for surfacing intermediate test state as NUnit / Allure attachments.
/// NUnit's attachment API only accepts file paths, so any string payload is first
/// written to a uniquely-named temp file, then registered with the current test context.
/// </summary>
public static class TestAttachments
{
    public static void AttachText(string fileName, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}_{fileName}");
        File.WriteAllText(path, content, Encoding.UTF8);
        TestContext.AddTestAttachment(path, fileName);
    }
}
