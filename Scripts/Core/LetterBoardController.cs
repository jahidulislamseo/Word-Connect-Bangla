using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  LetterTile  –  one hexagonal / circular letter cell
// ============================================================
[RequireComponent(typeof(Image), typeof(EventTrigger))]
public class LetterTile : MonoBehaviour,
    IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    // ── Inspector refs ────────────────────────────────────
    [Header("Visuals")]
    [SerializeField] TextMeshProUGUI letterLabel;
    [SerializeField] Image           tileImage;
    [SerializeField] Color           normalColor    = new Color(0.18f, 0.22f, 0.38f);
    [SerializeField] Color           selectedColor  = new Color(0.32f, 0.76f, 0.60f);
    [SerializeField] Color           completedColor = new Color(0.96f, 0.78f, 0.22f);

    // ── Public state ──────────────────────────────────────
    public char   Letter    { get; private set; }
    public int    TileIndex { get; private set; }
    public bool   IsSelected { get; private set; }

    // ── Internal ──────────────────────────────────────────
    LetterBoardController _board;
    Animator              _anim;

    // ── Init ──────────────────────────────────────────────
    public void Initialise(char letter, int index, LetterBoardController board)
    {
        Letter    = char.ToUpper(letter);
        TileIndex = index;
        _board    = board;
        _anim     = GetComponent<Animator>();

        if (letterLabel != null)
            letterLabel.text = Letter.ToString();

        SetNormal();
    }

    // ── Touch / Mouse events ──────────────────────────────
    public void OnPointerDown(PointerEventData e)  => _board.BeginSelection(this);
    public void OnPointerEnter(PointerEventData e) => _board.AddToSelection(this);
    public void OnPointerUp(PointerEventData e)    => _board.SubmitSelection();

    // ── Visual states ─────────────────────────────────────
    public void SetNormal()
    {
        IsSelected = false;
        tileImage.color = normalColor;
        transform.localScale = Vector3.one;
    }

    public void SetSelected()
    {
        IsSelected = true;
        tileImage.color = selectedColor;
        transform.localScale = Vector3.one * 1.12f;
        _anim?.SetTrigger("Bounce");
    }

    public void SetCompleted()
    {
        tileImage.color = completedColor;
        _anim?.SetTrigger("Complete");
    }

    public void PlayWrong()
    {
        StartCoroutine(WrongShake());
    }

    IEnumerator WrongShake()
    {
        tileImage.color = Color.red;
        float t = 0f;
        while (t < 0.4f)
        {
            float shake = Mathf.Sin(t * 60f) * 6f;
            transform.localPosition += new Vector3(shake, 0, 0);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = Vector3.zero;
        SetNormal();
    }
}

// ============================================================
//  LetterBoardController  –  manages the ring / grid of tiles
//  and produces a word when the player lifts their finger
// ============================================================
public class LetterBoardController : MonoBehaviour
{
    // ── Inspector refs ────────────────────────────────────
    [Header("Tile Setup")]
    [SerializeField] LetterTile     tilePrefab;
    [SerializeField] RectTransform  boardRoot;      // parent for tiles
    [SerializeField] float          ringRadius = 160f;

    [Header("Preview")]
    [SerializeField] TextMeshProUGUI currentWordPreview; // top label "A-P-P..."
    [SerializeField] LineRenderer    connectionLine;

    [Header("Events")]
    public event Action<string>  OnWordFormed;     // caller validates
    public event Action          OnSelectionReset;

    // ── Runtime state ─────────────────────────────────────
    private List<LetterTile>  _allTiles       = new List<LetterTile>();
    private List<LetterTile>  _selected       = new List<LetterTile>();
    private bool              _dragging       = false;

    // ── Public API ────────────────────────────────────────

    /// <summary>Set up the board with the provided letters (e.g. "ABCDEF").</summary>
    public void BuildBoard(string letters)
    {
        ClearBoard();
        letters = letters.ToUpper();

        for (int i = 0; i < letters.Length; i++)
        {
            float angle = (360f / letters.Length) * i - 90f; // start at top
            float rad   = angle * Mathf.Deg2Rad;
            var   pos   = new Vector2(
                              Mathf.Cos(rad) * ringRadius,
                              Mathf.Sin(rad) * ringRadius);

            var tile = Instantiate(tilePrefab, boardRoot);
            tile.GetComponent<RectTransform>().anchoredPosition = pos;
            tile.Initialise(letters[i], i, this);
            _allTiles.Add(tile);
        }
    }

    // ── Drag gesture handlers (called from LetterTile) ────

    public void BeginSelection(LetterTile tile)
    {
        ResetSelection();
        _dragging = true;
        AddToSelection(tile);
    }

    public void AddToSelection(LetterTile tile)
    {
        if (!_dragging || _selected.Contains(tile)) return;
        _selected.Add(tile);
        tile.SetSelected();
        UpdatePreview();
        UpdateConnectionLine();
    }

    public void SubmitSelection()
    {
        _dragging = false;
        if (_selected.Count >= 2)
        {
            string word = BuildSelectedWord();
            OnWordFormed?.Invoke(word);
        }
        else
        {
            ResetSelection();
        }
    }

    // ── Visual helpers ────────────────────────────────────

    void UpdatePreview()
    {
        if (currentWordPreview == null) return;
        currentWordPreview.text = string.Join("-",
            _selected.ConvertAll(t => t.Letter.ToString()));
    }

    void UpdateConnectionLine()
    {
        if (connectionLine == null) return;
        connectionLine.positionCount = _selected.Count;
        for (int i = 0; i < _selected.Count; i++)
        {
            var rt  = _selected[i].GetComponent<RectTransform>();
            var pos = boardRoot.TransformPoint(rt.anchoredPosition3D);
            connectionLine.SetPosition(i, pos);
        }
    }

    // ── Outcome responses (called by GameController) ──────

    public void AnimateCorrectWord()
    {
        foreach (var t in _selected) t.SetCompleted();
        StartCoroutine(ResetAfterDelay(0.8f));
    }

    public void AnimateWrongWord()
    {
        foreach (var t in _selected) t.PlayWrong();
        StartCoroutine(ResetAfterDelay(0.5f));
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetSelection();
    }

    // ── Internals ─────────────────────────────────────────

    string BuildSelectedWord() =>
        string.Concat(_selected.ConvertAll(t => t.Letter.ToString()));

    void ResetSelection()
    {
        foreach (var t in _selected) t.SetNormal();
        _selected.Clear();
        if (currentWordPreview != null) currentWordPreview.text = "";
        if (connectionLine != null)   connectionLine.positionCount = 0;
        OnSelectionReset?.Invoke();
    }

    void ClearBoard()
    {
        foreach (var t in _allTiles) Destroy(t.gameObject);
        _allTiles.Clear();
        ResetSelection();
    }
}
