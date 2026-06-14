// -----------------------------------------------------------------------------
//  Glass.Message — the kind of real-world operation a progress bar represents, so
//  it can animate a directional "flow" that matches what the user is waiting for
//  (data going out, data coming in, a two-way sync, or in-place processing).
//
//  File        : GlassProgressActivity.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

namespace Glass;

/// <summary>
/// Describes the operation a progress bar is tracking. The bar paints animated
/// directional stripes over its lit region to match — flowing forward for an
/// upload or export, backward for an incoming download or restore, easing both
/// ways for a two-way sync — giving the user an at-a-glance sense of what is
/// happening.
/// </summary>
/// <remarks>
/// <para>
/// The activity is purely a visual hint layered on top of the existing
/// determinate or indeterminate bar; it never changes the value or completion
/// semantics. Set it once via <see cref="GlassBuilder.ProgressActivity"/>, or
/// change it live through <see cref="GlassProgressController.SetActivity"/> as an
/// operation moves between phases (e.g. compressing, then uploading).
/// </para>
/// <para>
/// Members are grouped by the motion they convey: <b>outgoing</b> operations flow
/// forward, <b>incoming</b> ones flow backward, <b>two-way</b> ones ease back and
/// forth, and <b>in-place</b> ones shimmer forward.
/// </para>
/// </remarks>
public enum GlassProgressActivity
{
    /// <summary>No directional motion — a plain bar. This is the default.</summary>
    None,

    // --- Outgoing: stripes flow forward --------------------------------------

    /// <summary>A local copy/move between two locations. Stripes flow forward.</summary>
    FileTransfer,

    /// <summary>Sending data out to a server or peer. Stripes flow forward, briskly.</summary>
    Upload,

    /// <summary>Writing data out to an external file or destination. Flows forward.</summary>
    Export,

    /// <summary>Saving a copy to a backup target. A steady forward flow.</summary>
    Backup,

    /// <summary>Compressing or archiving (e.g. zipping). Flows forward.</summary>
    Compress,

    /// <summary>Encrypting data. Flows forward.</summary>
    Encrypt,

    /// <summary>Installing or updating software. A steady, unhurried forward flow.</summary>
    Install,

    /// <summary>Buffering or streaming media. A continuous forward flow.</summary>
    Stream,

    // --- Incoming: stripes flow backward -------------------------------------

    /// <summary>Receiving data from a server or peer. Stripes flow backward (incoming).</summary>
    Download,

    /// <summary>Reading data in from an external file or source. Flows backward.</summary>
    Import,

    /// <summary>Restoring from a backup. A steady backward (incoming) flow.</summary>
    Restore,

    /// <summary>Extracting or decompressing (e.g. unzipping). Flows backward.</summary>
    Extract,

    /// <summary>Decrypting data. Flows backward.</summary>
    Decrypt,

    // --- Two-way / handshake: stripes ease back and forth --------------------

    /// <summary>A two-way reconcile (e.g. cloud sync). Stripes ease back and forth.</summary>
    Sync,

    /// <summary>Establishing a connection or handshake. A quicker back-and-forth.</summary>
    Connecting,

    // --- In-place processing: a forward shimmer ------------------------------

    /// <summary>Scanning, indexing, or searching. A fast forward shimmer.</summary>
    Search,

    /// <summary>General CPU-bound work (processing, rendering, verifying). Flows forward.</summary>
    Processing,
}
