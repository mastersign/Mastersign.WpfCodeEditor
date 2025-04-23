using System;
using System.Collections.Generic;

namespace Mastersign.WpfCodeEditor;

public class CodeSymbol : IEquatable<CodeSymbol>, IComparable<CodeSymbol>
{
    public string Name { get; set; }
    public string Detail { get; set; }
    public int StartLine { get; set; }
    public int StartColumn { get; set; }

    public int CompareTo(CodeSymbol other)
    {
        if (StartLine != other.StartLine)
        {
            return StartLine - other.StartLine;
        }
        if (StartColumn != other.StartColumn)
        {
            return StartColumn - other.StartColumn;
        }
        return Name.CompareTo(other.Name);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as CodeSymbol);
    }

    public bool Equals(CodeSymbol other)
    {
        return other is not null &&
               Name == other.Name &&
               Detail == other.Detail &&
               StartLine == other.StartLine &&
               StartColumn == other.StartColumn;
    }

    public override int GetHashCode()
    {
        int hashCode = -64918116;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Detail);
        hashCode = hashCode * -1521134295 + StartLine.GetHashCode();
        hashCode = hashCode * -1521134295 + StartColumn.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(CodeSymbol left, CodeSymbol right)
    {
        return EqualityComparer<CodeSymbol>.Default.Equals(left, right);
    }

    public static bool operator !=(CodeSymbol left, CodeSymbol right)
    {
        return !(left == right);
    }
}
