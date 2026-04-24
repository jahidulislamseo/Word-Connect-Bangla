using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// ============================================================
//  WordEntry  –  one record from dictionary.json
// ============================================================
[Serializable]
public class WordEntry
{
    public string bn;          // Bangla meaning
    public string phonetic;    // IPA phonetic spelling
    public string audio;       // audio clip name (without extension)
    public string category;    // fruit / animal / action …
    public int    difficulty;  // 1 = easy, 2 = medium, 3 = hard
}

// ============================================================
//  DictionaryManager  –  singleton, loads JSON at startup
// ============================================================
public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance { get; private set; }

    // ── public events ──────────────────────────────────────
    public event Action OnDictionaryLoaded;

    // ── state ──────────────────────────────────────────────
    private Dictionary<string, WordEntry> _dict =
        new Dictionary<string, WordEntry>(StringComparer.OrdinalIgnoreCase);

    public bool IsLoaded { get; private set; }

    // ── Unity lifecycle ────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadDictionary());
    }

    // ── Loading ────────────────────────────────────────────
    IEnumerator LoadDictionary()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "dictionary.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android StreamingAssets need UnityWebRequest
        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                ParseJson(req.downloadHandler.text);
            else
                Debug.LogError($"[Dict] Load error: {req.error}");
        }
#else
        if (File.Exists(path))
            ParseJson(File.ReadAllText(path));
        else
            Debug.LogError($"[Dict] File not found: {path}");
        yield return null;
#endif

        IsLoaded = true;
        OnDictionaryLoaded?.Invoke();
        Debug.Log($"[Dict] Loaded {_dict.Count} words.");
    }

    void ParseJson(string json)
    {
        // Unity's JsonUtility can't deserialise a root dictionary,
        // so we use a tiny manual parser wrapper.
        var raw = JsonUtility.FromJson<DictionaryWrapper>(
                      WrapForUnity(json));
        if (raw?.entries == null) return;
        foreach (var e in raw.entries)
            _dict[e.word] = e.data;
    }

    // Converts  { "apple":{…}, … }  →  {"entries":[{"word":"apple","data":{…}},…]}
    // using MiniJSON (or you can swap in Newtonsoft.Json)
    string WrapForUnity(string json)
    {
        // ── Use Newtonsoft.Json (recommended) ──────────────
        // If you have Newtonsoft.Json in your project, uncomment:
        //   var dict = Newtonsoft.Json.JsonConvert.DeserializeObject
        //              <Dictionary<string,WordEntry>>(json);
        //   _dict = dict;
        //   return "{}";                // skip the wrapper

        // ── Fallback: MiniJSON embedded below ─────────────
        var obj = MiniJSON.Json.Deserialize(json)
                  as Dictionary<string, object>;
        if (obj == null) return "{}";

        foreach (var kv in obj)
        {
            var d = kv.Value as Dictionary<string, object>;
            if (d == null) continue;
            _dict[kv.Key] = new WordEntry
            {
                bn         = GetStr(d, "bn"),
                phonetic   = GetStr(d, "phonetic"),
                audio      = GetStr(d, "audio"),
                category   = GetStr(d, "category"),
                difficulty = GetInt(d, "difficulty")
            };
        }
        return "{}";   // dictionary already filled
    }

    // ── Public API ─────────────────────────────────────────

    /// <summary>Returns true and populates <paramref name="entry"/> when
    /// the word exists in the local dictionary.</summary>
    public bool TryGetWord(string word, out WordEntry entry) =>
        _dict.TryGetValue(word.ToLower(), out entry);

    /// <summary>Returns all words that match a given category.</summary>
    public List<string> GetWordsByCategory(string category)
    {
        var result = new List<string>();
        foreach (var kv in _dict)
            if (string.Equals(kv.Value.category, category,
                              StringComparison.OrdinalIgnoreCase))
                result.Add(kv.Key);
        return result;
    }

    /// <summary>Returns all words at a given difficulty (1/2/3).</summary>
    public List<string> GetWordsByDifficulty(int difficulty)
    {
        var result = new List<string>();
        foreach (var kv in _dict)
            if (kv.Value.difficulty == difficulty)
                result.Add(kv.Key);
        return result;
    }

    /// <summary>Returns a random word entry (for Word-of-the-Day, etc.).</summary>
    public (string word, WordEntry entry) GetRandomWord(int maxDifficulty = 3)
    {
        var eligible = new List<(string, WordEntry)>();
        foreach (var kv in _dict)
            if (kv.Value.difficulty <= maxDifficulty)
                eligible.Add((kv.Key, kv.Value));

        if (eligible.Count == 0)
            return (null, null);

        var pick = eligible[UnityEngine.Random.Range(0, eligible.Count)];
        return pick;
    }

    /// <summary>Returns the Word-of-the-Day (deterministic per calendar day).</summary>
    public (string word, WordEntry entry) GetWordOfTheDay()
    {
        var keys  = new List<string>(_dict.Keys);
        keys.Sort();                              // stable order
        int seed  = DateTime.Now.DayOfYear + DateTime.Now.Year * 365;
        int index = seed % keys.Count;
        string w  = keys[index];
        return (w, _dict[w]);
    }

    // ── Helpers ───────────────────────────────────────────
    static string GetStr(Dictionary<string, object> d, string k) =>
        d.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";

    static int GetInt(Dictionary<string, object> d, string k) =>
        d.TryGetValue(k, out var v) && int.TryParse(v?.ToString(), out int i) ? i : 1;

    // Unity serialisation wrapper (unused if Newtonsoft is available)
    [Serializable] class DictionaryWrapper
    {
        public List<WordPair> entries;
    }
    [Serializable] class WordPair
    {
        public string    word;
        public WordEntry data;
    }
}
