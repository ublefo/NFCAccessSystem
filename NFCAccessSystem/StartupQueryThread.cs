using NFCAccessSystem.Data;

namespace NFCAccessSystem;

public class StartupQueryThread
{
    public static void LaunchQuery(AccessSystemContext context)
    {
        // one-off query in the background to mitigate first-query latency
        context.Users.FirstOrDefault(u => u.TagUid == "FFFFFFFF");
    }
}