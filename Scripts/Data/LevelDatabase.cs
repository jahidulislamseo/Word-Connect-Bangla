using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
//  LevelDatabase  –  ScriptableObject holding all hand-crafted
//  levels. Falls back to procedural generation beyond the list.
//
//  Create via: Assets > Create > WordConnect > Level Database
// ============================================================
[CreateAssetMenu(fileName = "LevelDatabase",
                 menuName  = "WordConnect/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] List<LevelData> levels = new List<LevelData>();

    public LevelData GetLevel(int index)
    {
        if (index < levels.Count)
            return levels[index];

        // Procedural fallback for any level beyond designed ones
        return null;   // GameController handles null → calls GenerateLevel()
    }

    public int DesignedLevelCount => levels.Count;

#if UNITY_EDITOR
    // Helper to auto-populate sample levels in Editor
    [ContextMenu("Populate Sample Levels")]
    void PopulateSamples()
    {
        levels = new List<LevelData>
        {
            // Level 1  – very easy, 4-letter words
            new LevelData
            {
                letters     = "ABCDEFG",
                targetWords = new List<string> { "bad", "bag", "cab", "cad", "fad", "gab" }
            },
            // Level 2
            new LevelData
            {
                letters     = "RAINBOW",
                targetWords = new List<string> { "rain", "born", "warn", "worn", "brow" }
            },
            // Level 3
            new LevelData
            {
                letters     = "PLANETS",
                targetWords = new List<string> { "plant", "plane", "slate", "tales", "least" }
            },
            // Level 4
            new LevelData
            {
                letters     = "FLOWERS",
                targetWords = new List<string> { "lower", "rowel", "fowls", "flows", "owls" }
            },
            // Level 5
            new LevelData
            {
                letters     = "DRAGONS",
                targetWords = new List<string> { "dragon", "roads", "organ", "groan", "gonad" }
            },
        };
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
