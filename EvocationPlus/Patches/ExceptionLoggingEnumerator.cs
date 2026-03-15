using System;
using System.Collections;
using EvocationPlus;

internal sealed class ExceptionLoggingEnumerator : IEnumerator
{
    private readonly IEnumerator _inner;
    private readonly string _label;

    public ExceptionLoggingEnumerator(IEnumerator inner, string label)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _label = label ?? "<loading>";
    }

    public object Current => _inner.Current;

    public bool MoveNext()
    {
        try
        {
            return _inner.MoveNext();
        }
        catch (Exception ex)
        {
            Main.Mod.Logger.Log($"[EvocationPlus] LOAD EXCEPTION in {_label}\n{ex}");
            // Re-throw so Kingmaker still handles it normally (you still get the popup),
            // but now we have the real stack trace in the log.
            throw;
        }
    }

    public void Reset() => _inner.Reset();
}