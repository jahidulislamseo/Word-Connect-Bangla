# 📱 WordConnect Bangla — Complete Game Architecture
### Senior Game Developer Handoff Document

---

## 🗂️ Project Structure

```
WordConnectBangla/
├── Scripts/
│   ├── Core/
│   │   ├── LetterBoardController.cs   ← Drag-connect tile system
│   │   └── GameController.cs          ← Main game loop & word validation
│   ├── Data/
│   │   ├── DictionaryManager.cs       ← JSON loader + word lookup API
│   │   ├── LevelDatabase.cs           ← ScriptableObject for designed levels
│   │   └── MiniJSON.cs               ← Embedded JSON parser (no dependencies)
│   ├── Audio/
│   │   └── AudioManager.cs            ← Pronunciation + SFX + TTS fallback
│   ├── UI/
│   │   └── WordFoundPopup.cs          ← Animated Bangla meaning card
│   ├── Managers/
│   │   ├── StreakAchievementManager.cs ← Streaks, WOTD, achievements
│   │   ├── AdManager.cs               ← AdMob rewarded/banner/interstitial
│   │   └── PerformanceManager.cs      ← Adaptive quality for low-end devices
│   └── GameModes/
│       └── GameModeControllers.cs     ← Daily, Vocabulary, Speed modes
├── StreamingAssets/
│   ├── dictionary.json                ← 100 sample words (expand to 1000+)
│   └── Audio/                         ← Place apple.mp3, bird.mp3 etc. here
└── Resources/
    └── Audio/Pronunciation/           ← Alternative: load via Resources.Load
```

---

## 🚀 Setup Guide (5 Minutes)

### Step 1 — Import into Unity
1. Create a new **Unity 2022 LTS** project (URP recommended)
2. Copy the `Scripts/` and `StreamingAssets/` folders into `Assets/`
3. Install **TextMeshPro** via Package Manager (required for labels)

### Step 2 — Scene Hierarchy
```
Scene
├── [GameManagers]              ← Empty GO with all singleton scripts
│    ├── DictionaryManager.cs
│    ├── AudioManager.cs
│    ├── AdManager.cs
│    ├── PerformanceManager.cs
│    ├── StreakManager.cs
│    └── AchievementManager.cs
├── Canvas (Screen Space - Overlay)
│    ├── LetterBoard             ← LetterBoardController.cs here
│    │    └── TileRoot           ← Tiles spawn inside this RectTransform
│    ├── HUD
│    │    ├── ScoreLabel (TMP)
│    │    ├── LevelLabel (TMP)
│    │    ├── StreakLabel (TMP)
│    │    └── ProgressBar (Slider)
│    ├── WordFoundPopup          ← WordFoundPopup.cs + CanvasGroup
│    │    ├── CardPanel
│    │    │    ├── EnglishWord (TMP)
│    │    │    ├── PhoneticLabel (TMP)
│    │    │    ├── BanglaLabel (TMP)
│    │    │    ├── CategoryBadge
│    │    │    └── SpeakerButton
│    │    └── ScoreLabel (TMP)
│    ├── WordOfTheDayPanel       ← WordOfTheDayManager.cs
│    └── LevelCompletePanel
└── GameController (GO)         ← GameController.cs
```

### Step 3 — TileRoot Prefab
Create `LetterTile` prefab:
- **Image** component (the hexagon/circle background)
- **TextMeshProUGUI** child (the letter)
- **EventTrigger** component  
- **Animator** with triggers: `Bounce`, `Complete`
- Attach `LetterTile.cs`

### Step 4 — Audio
Place `.mp3` files in `StreamingAssets/Audio/`:
```
StreamingAssets/Audio/
├── apple.mp3
├── bird.mp3
├── book.mp3
... (one file per word, filename = the word)
```
Or use **Google TTS** (automatic fallback, no files needed).

### Step 5 — Fonts
For correct Bangla rendering:
1. Download **Noto Sans Bengali** (free, Google Fonts)
2. Import to Unity as a TMP font asset
3. Assign to `BanglaLabel` TextMeshProUGUI components

---

## 📖 Dictionary System

### Format
```json
{
  "apple": {
    "bn":       "আপেল",
    "phonetic": "/ˈæp.əl/",
    "audio":    "apple",
    "category": "fruit",
    "difficulty": 1
  }
}
```

### Expanding to 1,000+ Words
The sample `dictionary.json` has 100 words. To expand:
1. Use **Wiktionary API**: `https://en.wiktionary.org/api/rest_v1/page/summary/{word}`
2. Use **Google Translate API** for automated BN translations
3. Add pronunciation audio via **ResponsiveVoice**, **Google TTS**, or **Amazon Polly**

### API Fallback (Advanced Words)
```csharp
// In DictionaryManager, add online lookup:
IEnumerator LookupOnline(string word)
{
    string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
    using var req = UnityWebRequest.Get(url);
    yield return req.SendWebRequest();
    // Parse response and cache locally
}
```

---

## 🎮 Game Modes Summary

| Mode | Description | Timer | Special |
|------|-------------|-------|---------|
| **Classic** | Level progression 1-∞ | None | Star rating (1-3★) |
| **Daily** | Same puzzle for all users | Resets midnight | Leaderboard-ready |
| **Vocabulary** | Category themes | None | Spaced repetition |
| **Speed** | Find max words in 60s | 60s (+3s/word) | Combo multiplier |

---

## 💰 Monetization Flow

```
Player runs out of hints
        ↓
AdManager.ShowRewardedAd()
        ↓
[Ad plays 5-30s]
        ↓
OnHintRewarded() → +2 hints
        ↓
Player continues
```

**Premium IAP** (₹199 / $2.49):
- Remove all ads
- Unlimited hints  
- Full dictionary unlock
- Offline audio pack

---

## 🔊 Audio Priority Chain

```
1. Resources/Audio/Pronunciation/{word}.mp3   ← Fastest (already in build)
2. StreamingAssets/Audio/{word}.mp3           ← Good for large audio packs
3. Google TTS (online)                         ← Fallback, needs internet
4. Android native TTS                          ← Last resort, robotic voice
```

---

## 📊 Performance Targets

| Device Tier | RAM | FPS | Particles | Shadows |
|-------------|-----|-----|-----------|---------|
| Low         | <1.5GB | 30 | 30% | Off |
| Mid         | 1.5-3GB | 45 | 65% | Hard only |
| High        | >3GB | 60 | 100% | Full |

---

## 🏆 Achievement IDs

| ID | Trigger | Title |
|----|---------|-------|
| `first_word` | 1 word learned | First Word! |
| `words_10` | 10 words | Getting Started |
| `words_50` | 50 words | Word Enthusiast |
| `words_100` | 100 words | Vocabulary Builder |
| `words_500` | 500 words | **Vocabulary Master** |
| `streak_7` | 7-day streak | Week Warrior |
| `streak_30` | 30-day streak | Monthly Master |
| `speed_5` | Speed mode 5 words | Speed Typer |
| `perfect_level` | No wrong attempts | Perfect Level |

---

## 🔧 Key C# APIs at a Glance

```csharp
// Validate a word
DictionaryManager.Instance.TryGetWord("apple", out WordEntry entry);

// Play pronunciation
AudioManager.Instance.PlayPronunciation("apple", entry.audio);

// Show popup
wordPopup.ShowWord("APPLE", entry, scoreGained: 50);

// Show rewarded ad for hint
AdManager.Instance.ShowRewardedAd(OnHintEarned);

// Record for spaced repetition
VocabularyMode.RecordForReview("apple", wasHard: false);

// Get Word of the Day
var (word, entry) = DictionaryManager.Instance.GetWordOfTheDay();

// Unlock achievement
AchievementManager.Instance.Unlock("first_word");
```

---

## ⚡ Performance Tips

1. **Pool your tiles** — use `PerformanceManager.Pool<T>` instead of Instantiate/Destroy
2. **Compress audio** — use Vorbis (OGG) at 64kbps for mobile, not WAV
3. **Atlas your UI sprites** — pack all UI images into a single Sprite Atlas
4. **Lazy-load levels** — only load the next level's assets, not the whole database
5. **StreamingAssets on Android** — always use `UnityWebRequest`, never `File.Read`
6. **Bangla font atlas** — pre-generate the TMP font atlas for Bengali Unicode range

---

## 📦 Recommended Third-Party Packages

| Package | Purpose | Cost |
|---------|---------|------|
| Google AdMob Unity | Rewarded/Banner ads | Free |
| Unity IAP | Premium purchases | Free |
| Newtonsoft.Json | Better JSON parsing | Free (via NuGet) |
| DOTween | Smooth UI animations | Free/Pro |
| Google TTS API | High-quality pronunciation | Pay-per-use |

---

*Built with Unity 2022 LTS — Android target API 33+*  
*Min SDK: API 21 (Android 5.0) for maximum reach*
