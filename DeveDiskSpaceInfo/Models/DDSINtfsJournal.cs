using DiscUtils.Ntfs;

namespace DeveDiskSpaceInfo.Models
{
    /// <summary>
    /// The state of an NTFS volume's journal ($LogFile): whether Windows shut the volume down cleanly, and if not, what
    /// recovering it takes. The used and free space of such a volume are read after an in-memory replay of the journal
    /// (nothing is written), so they match what Windows will show after its own recovery.
    /// </summary>
    public class DDSINtfsJournal
    {
        /// <summary>
        /// Clean (shut down properly), NothingToReplay (not shut down properly, but every change reached the disk),
        /// NeedsReplay (finished changes are only in the journal) or Unsupported (can't be replayed offline, for example
        /// a hibernated Windows; <see cref="Detail"/> says why).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        public string? Detail { get; set; }

        /// <summary>The number of file records, index buffers and other pages the replay changes.</summary>
        public int PagesChanged { get; set; }

        public int RecordsToRedo { get; set; }

        /// <summary>Unfinished transactions that recovery rolls back (shown rolled back; Windows finishes it at its next boot).</summary>
        public int TransactionsToUndo { get; set; }

        /// <summary>Whether the reported contents and sizes include the journal's changes (replayed in memory).</summary>
        public bool ReplayedInMemory { get; set; }

        public bool CanReplayPersistently { get; set; }

        public string LogVersion { get; set; } = string.Empty;

        public static DDSINtfsJournal From(NtfsJournalInfo info) => new()
        {
            Status = info.Status.ToString(),
            Detail = string.IsNullOrWhiteSpace(info.Detail) ? null : info.Detail,
            PagesChanged = info.PagesChanged,
            RecordsToRedo = info.RecordsToRedo,
            TransactionsToUndo = info.TransactionsToUndo,
            ReplayedInMemory = info.ReplayedInMemory,
            CanReplayPersistently = info.CanReplayPersistently,
            LogVersion = info.LogVersion,
        };
    }
}
