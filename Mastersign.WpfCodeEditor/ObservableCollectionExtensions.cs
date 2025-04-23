using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mastersign.WpfCodeEditor;

internal static class ObservableCollectionExtensions
{

    public static bool UpdateSorted<T>(this ObservableCollection<T> collection, ICollection<T> items)
        where T : IComparable<T>
    {
        var changed = false;
        var hashes = items.Select(s => s.GetHashCode()).ToArray();
        var existingHashes = collection.Select(s => s.GetHashCode()).ToArray();

        // remove obsolete items
        var droppedIndices = new List<int>(capacity: collection.Count);
        for (var i = 0; i < collection.Count; i++)
        {
            var hash = collection[i].GetHashCode();
            if (!hashes.Contains(hash))
            {
                droppedIndices.Add(i);
            }
        }
        changed = droppedIndices.Count > 0;
        for (var i = droppedIndices.Count - 1; i >= 0; i--)
        {
            collection.RemoveAt(droppedIndices[i]);
        }

        // insert new items in a sorted manner
        foreach (var item in items)
        {
            if (existingHashes.Contains(item.GetHashCode())) continue;
            changed = true;
            var inserted = false;
            for (var i = 0; i < collection.Count; i++)
            {
                if (item.CompareTo(collection[i]) > 0)
                {
                    collection.Insert(i + 1, item);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                collection.Add(item);
            }
        }

        return changed;
    }
}
