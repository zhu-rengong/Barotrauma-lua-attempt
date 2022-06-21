﻿#nullable enable
namespace Barotrauma
{
    public abstract class Result<T, TError>
        where T: notnull
        where TError: notnull
    {
        public abstract bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public static Success<T, TError> Success(T value)
            => new Success<T, TError>(value);
        
        public static Failure<T, TError> Failure(TError error)
            => new Failure<T, TError>(error);
    }

    public sealed class Success<T, TError> : Result<T, TError>
        where T: notnull
        where TError: notnull
    {
        public readonly T Value;
        public override bool IsSuccess => true;

        public Success(T value)
        {
            Value = value;
        }
    }

    public sealed class Failure<T, TError> : Result<T, TError>
        where T: notnull
        where TError: notnull
    {
        public readonly TError Error;

        public override bool IsSuccess => false;

        public Failure(TError error)
        {
            Error = error;
        }
    }
}
