using System;
using System.Collections.Generic;

namespace FlyleafLib.MediaPlayer;

#nullable enable

/// <summary>
/// Releases ChickenRice subtitles in step with the playback position.
///
/// The upstream engine can produce subtitles much faster than real time. Dumping
/// the whole result into the subtitle list at once is not useful for playback and
/// used to be the only thing LLPlayer could do. This buffer holds generated
/// subtitles until the playhead is within <c>lead</c> of their start time, then
/// forwards them in timestamp order (so SubManager's binary search stays valid).
/// <c>maxPending</c> is a safety valve for a paused playhead.
/// </summary>
public sealed class ChickenRiceProgressiveBuffer
{
    private readonly List<SubtitleASRData> _pending = [];
    private readonly Func<TimeSpan> _playbackTime;
    private readonly TimeSpan _lead;
    private readonly Action<SubtitleASRData> _deliver;
    private readonly int _maxPending;

    public int PendingCount => _pending.Count;

    public ChickenRiceProgressiveBuffer(
        Func<TimeSpan> playbackTime,
        TimeSpan lead,
        Action<SubtitleASRData> deliver,
        int maxPending = 200)
    {
        _playbackTime = playbackTime ?? throw new ArgumentNullException(nameof(playbackTime));
        _lead = lead < TimeSpan.Zero ? TimeSpan.Zero : lead;
        _deliver = deliver ?? throw new ArgumentNullException(nameof(deliver));
        _maxPending = Math.Max(1, maxPending);
    }

    public void Add(SubtitleASRData data) => _pending.Add(data);

    /// <summary>Called repeatedly while the engine runs (and once after it exits).</summary>
    public void Pump()
    {
        if (_pending.Count == 0)
            return;

        _pending.Sort(static (a, b) => a.StartTime.CompareTo(b.StartTime));

        TimeSpan limit = _playbackTime() + _lead;
        while (_pending.Count > 0 &&
               (_pending[0].StartTime <= limit || _pending.Count > _maxPending))
        {
            _deliver(_pending[0]);
            _pending.RemoveAt(0);
        }
    }

    /// <summary>Deliver everything still buffered (used when no final result follows).</summary>
    public void Flush()
    {
        if (_pending.Count == 0)
            return;

        _pending.Sort(static (a, b) => a.StartTime.CompareTo(b.StartTime));
        foreach (SubtitleASRData data in _pending)
            _deliver(data);

        _pending.Clear();
    }

    /// <summary>Drop buffered previews when the authoritative final SRT replaces them.</summary>
    public void Clear() => _pending.Clear();
}
