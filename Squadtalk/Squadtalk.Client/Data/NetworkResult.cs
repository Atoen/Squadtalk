using System.Diagnostics.CodeAnalysis;

namespace Squadtalk.Client.Data;

public readonly record struct NetworkResult<T>(
    T? Value,                                         // nameof() needs explicit prop to work
    [property: MemberNotNullWhen(true, "Value")] bool IsSuccess)
{
    public static NetworkResult<T> Ok(T value) => new(value, true);

    public static readonly NetworkResult<T> Error = new(default, false);

    public static implicit operator NetworkResult<T>(T value) => new(value, true);

    public T ValueOr(T other) => IsSuccess ? Value : other;

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError => !IsSuccess;

    public bool ErrorOrValueIs(T value) => IsError || EqualityComparer<T>.Default.Equals(Value, value);

    public bool ErrorOrValueIsNot(T value) => IsError || !EqualityComparer<T>.Default.Equals(Value, value);

    public bool SuccessAndValueIs(T value) => IsSuccess && EqualityComparer<T>.Default.Equals(Value, value);

    public bool SuccessAndValueIsNot(T value) => IsSuccess && !EqualityComparer<T>.Default.Equals(Value, value);
}

public readonly record struct NetworkResult(bool IsSuccess)
{
    public static readonly NetworkResult Ok = new(true);

    public static readonly NetworkResult Error = new(false);

    public bool IsError => !IsSuccess;
}