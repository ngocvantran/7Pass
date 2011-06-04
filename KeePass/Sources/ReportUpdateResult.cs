using KeePass.Storage;

namespace KeePass.Sources
{
    internal delegate void ReportUpdateResult(
        DatabaseInfo info, SyncResults result,
        string errorMsg);
}