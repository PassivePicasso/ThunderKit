using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderKit.Core.Pipelines
{
    [Serializable]
    public struct LogEntry
    {
        [Serializable]
        public struct SDateTime : IEquatable<SDateTime>
        {
            public long ticks;
            public SDateTime(long ticks)
            {
                this.ticks = ticks;
            }

            public override bool Equals(object obj)
            {
                return obj is SDateTime time && Equals(time);
            }

            public bool Equals(SDateTime other)
            {
                return ticks == other.ticks;
            }

            public override int GetHashCode()
            {
                return 1040981437 + ticks.GetHashCode();
            }

            public static bool operator ==(SDateTime left, SDateTime right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SDateTime left, SDateTime right)
            {
                return !(left == right);
            }

            public static implicit operator DateTime(SDateTime sdt) => new DateTime(sdt.ticks);
            public static implicit operator SDateTime(DateTime sdt) => new SDateTime(sdt.Ticks);
        }

        public LogLevel logLevel;
        [SerializeField] private SDateTime internalTime;
        public string message;
        public string[] context;
        public DateTime time => internalTime;

        public LogEntry(LogLevel logLevel, DateTime time, string message, string[] context)
        {
            this.logLevel = logLevel;
            this.internalTime = time;
            this.message = message;
            this.context = context;
        }

        public override bool Equals(object obj)
        {
            return obj is LogEntry other &&
                   logLevel == other.logLevel &&
                   internalTime == other.internalTime &&
                   message == other.message &&
                   EqualityComparer<object[]>.Default.Equals(context, other.context);
        }

        public override int GetHashCode()
        {
            int hashCode = 1903218229;
            hashCode = hashCode * -1521134295 + logLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + internalTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(message);
            hashCode = hashCode * -1521134295 + EqualityComparer<object[]>.Default.GetHashCode(context);
            return hashCode;
        }

        public void Deconstruct(out LogLevel logLevel, out DateTime time, out string message, out object[] context)
        {
            logLevel = this.logLevel;
            time = this.internalTime;
            message = this.message;
            context = this.context;
        }

        public static implicit operator (LogLevel logLevel, DateTime time, string message, string[] context)(LogEntry value)
        {
            return (value.logLevel, value.internalTime, value.message, value.context);
        }

        public static implicit operator LogEntry((LogLevel logLevel, DateTime time, string message, string[] context) value)
        {
            return new LogEntry(value.logLevel, value.time, value.message, value.context);
        }
    }
}