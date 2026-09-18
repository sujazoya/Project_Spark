using System;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Circuit
{
    public readonly struct SparkCircuitConnection :
        IEquatable<SparkCircuitConnection>
    {
        public ulong Id { get; }

        public SparkTerminal A { get; }

        public SparkTerminal B { get; }

        public SparkConnectionKind Kind { get; }

        public SparkConnectionDirection Direction { get; }

        public SparkCircuitConnection(
            ulong id,
            SparkTerminal a,
            SparkTerminal b,
            SparkConnectionKind kind,
            SparkConnectionDirection direction)
        {
            Id = id;
            A = a;
            B = b;
            Kind = kind;
            Direction = direction;
        }

        public bool IsValid =>
            Id != 0UL &&
            A != null &&
            B != null;

        public bool Contains(
            SparkTerminal terminal)
        {
            return A == terminal ||
                   B == terminal;
        }

        public SparkTerminal GetOther(
            SparkTerminal terminal)
        {
            if (A == terminal)
            {
                return B;
            }

            if (B == terminal)
            {
                return A;
            }

            return null;
        }

        public bool Connects(
            SparkTerminal first,
            SparkTerminal second)
        {
            return
                (A == first && B == second) ||
                (A == second && B == first);
        }

        public bool Equals(
            SparkCircuitConnection other)
        {
            return Id == other.Id;
        }

        public override bool Equals(
            object obj)
        {
            return obj is SparkCircuitConnection other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(
            SparkCircuitConnection left,
            SparkCircuitConnection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            SparkCircuitConnection left,
            SparkCircuitConnection right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return
                $"Connection {Id}: " +
                $"{A?.name ?? "null"} <-> " +
                $"{B?.name ?? "null"} " +
                $"[{Kind}, {Direction}]";
        }
    }
}