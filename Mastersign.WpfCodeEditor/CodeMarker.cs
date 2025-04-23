using System;
using System.Collections.Generic;

namespace Mastersign.WpfCodeEditor;

public class CodeMarker : IEquatable<CodeMarker>, IComparable<CodeMarker>
{
    public int StartLineNumber { get; set; }
    public int StartColumn { get; set; }
    public int EndLineNumber { get; set; }
    public int EndColumn { get; set; }
    public string Message { get; set; }
    public int Severity { get; set; }

    public int CompareTo(CodeMarker other)
    {
        if (StartLineNumber != other.StartLineNumber)
        {
            return StartLineNumber - other.StartLineNumber;
        }
        if (StartColumn != other.StartColumn)
        {
            return StartColumn - other.StartColumn;
        }
        if (EndLineNumber != other.EndLineNumber)
        {
            return EndLineNumber - other.EndLineNumber;
        }
        if (EndColumn != other.EndColumn)
        {
            return EndColumn - other.EndColumn;
        }
        if (Severity != other.Severity)
        {
            return Severity - other.Severity;
        }
        return Message.CompareTo(other.Message);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as CodeMarker);
    }

    public bool Equals(CodeMarker other)
    {
        return other is not null &&
               StartLineNumber == other.StartLineNumber &&
               StartColumn == other.StartColumn &&
               EndLineNumber == other.EndLineNumber &&
               EndColumn == other.EndColumn &&
               Message == other.Message &&
               Severity == other.Severity;
    }

    public override int GetHashCode()
    {
        int hashCode = 939840772;
        hashCode = hashCode * -1521134295 + StartLineNumber.GetHashCode();
        hashCode = hashCode * -1521134295 + StartColumn.GetHashCode();
        hashCode = hashCode * -1521134295 + EndLineNumber.GetHashCode();
        hashCode = hashCode * -1521134295 + EndColumn.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
        hashCode = hashCode * -1521134295 + Severity.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(CodeMarker left, CodeMarker right)
    {
        return EqualityComparer<CodeMarker>.Default.Equals(left, right);
    }

    public static bool operator !=(CodeMarker left, CodeMarker right)
    {
        return !(left == right);
    }
}
