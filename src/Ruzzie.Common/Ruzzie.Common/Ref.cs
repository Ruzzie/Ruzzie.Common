namespace Ruzzie.Common;

/// Encapsulate a Value type to pass as a reference
///   this can be used for shared access (pass a pointer to a value)
public sealed class Ref<T> where T : struct
{
    /// Read or write the value
    public T Value;

    /// <summary>
    /// Creates a new <see cref="Ref{T}"/> that is initialized with the passed value.
    /// </summary>
    /// <param name="value">The initial value</param>
    public Ref(T value)
    {
        Value = value;
    }
}