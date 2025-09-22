public class WordFinder
{
    private readonly HashSet<string> _dictionary; // substrings of rows and columns
    private readonly int _maxLen;
    private readonly bool _caseInsensitive;

    public WordFinder(IEnumerable<string> matrix)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));

        var rows = new List<string>();
        foreach (var line in matrix)
        {
            if (line == null) throw new ArgumentException("Rows cannot be null.");
            rows.Add(line);
        }
        if (rows.Count == 0) throw new ArgumentException("The matrix must have at least one row.");

        int width = rows[0].Length;
        if (width == 0) throw new ArgumentException("Rows cannot be empty.");
        if (rows.Count > 64 || width > 64) throw new ArgumentException("The matrix cannot exceed 64x64.");
        for (int i = 1; i < rows.Count; i++)
        {
            if (rows[i].Length != width) throw new ArgumentException("All rows must have the same length.");
        }

        _caseInsensitive = true; // by default ignore case
        if (_caseInsensitive)
        {
            for (int i = 0; i < rows.Count; i++) rows[i] = rows[i].ToUpperInvariant();
        }

        _maxLen = Math.Max(rows.Count, width);

        // Build columns as strings (top->bottom)
        var cols = new string[width];
        var colChars = new char[rows.Count];
        for (int c = 0; c < width; c++)
        {
            for (int r = 0; r < rows.Count; r++)
                colChars[r] = rows[r][c];
            cols[c] = new string(colChars);
        }

        // Precompute all contiguous substrings of rows and columns
        _dictionary = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in rows)
            AddSubstrings(line, _dictionary);
        foreach (var line in cols)
            AddSubstrings(line, _dictionary);
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        if (wordstream == null) throw new ArgumentNullException(nameof(wordstream));

        // Count how many times each word appears in the stream (for ordering)
        var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in wordstream)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var w = raw.Trim();
            if (w.Length == 0 || w.Length > _maxLen) continue; // impossible by size

            if (freq.ContainsKey(w)) freq[w]++;
            else freq[w] = 1;
        }
        if (freq.Count == 0) return Array.Empty<string>();

        // Filter those that actually exist in the board (using normalization if applicable)
        var found = new List<(string Word, int Count)>();
        foreach (var kv in freq)
        {
            var key = _caseInsensitive ? kv.Key.ToUpperInvariant() : kv.Key;
            if (_dictionary.Contains(key))
                found.Add((kv.Key, kv.Value));
        }
        if (found.Count == 0) return Array.Empty<string>();

        // Sort by frequency desc and alphabetically asc; return up to 10 distinct words
        found.Sort((a, b) =>
        {
            int cmp = b.Count.CompareTo(a.Count);
            if (cmp != 0) return cmp;
            return string.Compare(a.Word, b.Word, StringComparison.OrdinalIgnoreCase);
        });

        var result = new List<string>(10);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in found)
        {
            if (used.Add(item.Word))
            {
                result.Add(item.Word);
                if (result.Count == 10) break;
            }
        }
        return result;
    }

    private static void AddSubstrings(string s, HashSet<string> set)
    {
        for (int i = 0; i < s.Length; i++)
        {
            for (int len = 1; i + len <= s.Length; len++)
            {
                set.Add(s.Substring(i, len));
            }
        }
    }
}
