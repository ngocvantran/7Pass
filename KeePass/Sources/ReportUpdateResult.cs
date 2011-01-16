using KeePass.Storage;

namespace KeePass.Sources
{
    internal delegate void ReportUpdateResult(
        DatabaseInfo info, bool success, string errorMsg);
}