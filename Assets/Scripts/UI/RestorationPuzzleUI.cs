using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RestorationPuzzleUI : MonoBehaviour
{
    private const int ColumnCount = 3;
    private const int RowCount = 3;
    private const float TileSize = 185f;
    private const float CompletionPauseSeconds = 1.35f;

    private readonly List<Button> tiles = new();
    private readonly List<int> rotations = new();
    private readonly List<Sprite> runtimeSprites = new();
    private readonly PlayerMovementFreezeHandle movementFreeze = new();

    private VisualElement root;
    private VisualElement board;
    private Button closeButton;
    private Label completeLabel;
    private Label titleLabel;
    private Label instructionsLabel;
    private Action onSolved;
    private Action onCancelled;
    private bool isOpen;
    private bool completing;
    private bool cursorWasVisible;
    private CursorLockMode previousCursorLock;
    private Coroutine completionRoutine;
    private DisplayStyle hudDisplayBeforePuzzle;
    private bool hudVisibilityCaptured;

    public bool IsOpen => isOpen;

    public void Initialize(VisualElement gameplayRoot)
    {
        root = gameplayRoot?.Q<VisualElement>("RestorationPuzzleRoot");
        board = root?.Q<VisualElement>("RestorationPuzzleBoard");
        closeButton = root?.Q<Button>("RestorationPuzzleCloseButton");
        completeLabel = root?.Q<Label>("RestorationPuzzleCompleteLabel");
        titleLabel = root?.Q<Label>("RestorationPuzzleTitle");
        instructionsLabel = root?.Q<Label>("RestorationPuzzleInstructions");

        if (closeButton != null)
        {
            closeButton.clicked -= Cancel;
            closeButton.clicked += Cancel;
        }

        CloseWithoutCallback();
    }

    public void Open(
        Texture2D restoredImage,
        Action solved,
        Action cancelled = null,
        string title = "Restore the Ruined Garden",
        string instructions = "Rotate each fragment to reconstruct the garden.")
    {
        if (root == null || board == null)
        {
            Initialize(GameplayUIManager.Instance?.RootVisualElement);
        }

        if (root == null || board == null || restoredImage == null)
        {
            Debug.LogError(
                "RestorationPuzzleUI: UI or restored image is missing.");
            cancelled?.Invoke();
            return;
        }

        onSolved = solved;
        onCancelled = cancelled;
        if (titleLabel != null)
            titleLabel.text = title;
        if (instructionsLabel != null)
            instructionsLabel.text = instructions;
        completing = false;
        isOpen = true;

        if (completeLabel != null)
            completeLabel.style.display = DisplayStyle.None;
        if (closeButton != null)
            closeButton.SetEnabled(true);
        board.style.opacity = 1f;

        BuildPuzzle(restoredImage);
        GameplayUIManager.Instance?.SuppressSecondaryPanels(this);
        HideHud();
        movementFreeze.Acquire();

        cursorWasVisible = UnityEngine.Cursor.visible;
        previousCursorLock = UnityEngine.Cursor.lockState;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Position;
    }

    private void Update()
    {
        if (isOpen && !completing && Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    private void BuildPuzzle(Texture2D texture)
    {
        ClearPuzzle();
        bool hasRotatedTile = false;

        for (int row = 0; row < RowCount; row++)
        {
            VisualElement puzzleRow = new();
            puzzleRow.name = $"RestorationPuzzleRow{row}";
            puzzleRow.style.width = TileSize * ColumnCount;
            puzzleRow.style.height = TileSize;
            puzzleRow.style.flexDirection = FlexDirection.Row;
            puzzleRow.style.flexShrink = 0f;
            board.Add(puzzleRow);

            for (int column = 0; column < ColumnCount; column++)
            {
                int index = row * ColumnCount + column;
                Rect textureRect = GetTextureRect(texture, row, column);
                Sprite sprite = Sprite.Create(
                    texture,
                    textureRect,
                    new Vector2(0.5f, 0.5f),
                    100f);
                runtimeSprites.Add(sprite);

                Button tile = new();
                tile.name = $"RestorationPuzzleTile{index}";
                tile.tooltip = "Rotate fragment clockwise";
                tile.style.width = TileSize;
                tile.style.height = TileSize;
                tile.style.flexShrink = 0f;
                tile.style.marginLeft = 0f;
                tile.style.marginRight = 0f;
                tile.style.marginTop = 0f;
                tile.style.marginBottom = 0f;
                tile.style.paddingLeft = 0f;
                tile.style.paddingRight = 0f;
                tile.style.paddingTop = 0f;
                tile.style.paddingBottom = 0f;
                tile.style.borderLeftWidth = 1f;
                tile.style.borderRightWidth = 1f;
                tile.style.borderTopWidth = 1f;
                tile.style.borderBottomWidth = 1f;
                tile.style.borderLeftColor = new Color(0.2f, 0.14f, 0.1f);
                tile.style.borderRightColor = new Color(0.2f, 0.14f, 0.1f);
                tile.style.borderTopColor = new Color(0.2f, 0.14f, 0.1f);
                tile.style.borderBottomColor = new Color(0.2f, 0.14f, 0.1f);
                tile.style.backgroundImage = new StyleBackground(sprite);
                tile.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;

                int turns = UnityEngine.Random.Range(0, 4);
                rotations.Add(turns);
                hasRotatedTile |= turns != 0;
                ApplyRotation(tile, turns);

                int capturedIndex = index;
                tile.clicked += () => RotateTile(capturedIndex);
                tiles.Add(tile);
                puzzleRow.Add(tile);
            }
        }

        if (!hasRotatedTile)
        {
            rotations[0] = 1;
            ApplyRotation(tiles[0], 1);
        }
    }

    private static Rect GetTextureRect(
        Texture2D texture,
        int visualRow,
        int column)
    {
        float width = texture.width / (float)ColumnCount;
        float height = texture.height / (float)RowCount;
        int textureRow = RowCount - 1 - visualRow;
        return new Rect(column * width, textureRow * height, width, height);
    }

    private void RotateTile(int index)
    {
        if (!isOpen || completing || index < 0 || index >= tiles.Count)
            return;

        rotations[index] = (rotations[index] + 1) % 4;
        ApplyRotation(tiles[index], rotations[index]);

        if (!IsSolved())
            return;

        completing = true;
        if (closeButton != null)
            closeButton.SetEnabled(false);
        if (completeLabel != null)
            completeLabel.style.display = DisplayStyle.Flex;
        board.style.opacity = 0.42f;

        completionRoutine = StartCoroutine(CompleteAfterPause());
    }

    private IEnumerator CompleteAfterPause()
    {
        yield return new WaitForSecondsRealtime(CompletionPauseSeconds);

        completionRoutine = null;
        Action solved = onSolved;
        CloseInternal();
        solved?.Invoke();
    }

    private static void ApplyRotation(VisualElement tile, int turns)
    {
        tile.style.rotate = new Rotate(new Angle(turns * 90f, AngleUnit.Degree));
        bool solved = turns == 0;
        Color border = solved
            ? new Color(0.84f, 0.73f, 0.3f)
            : new Color(0.2f, 0.14f, 0.1f);
        tile.style.borderLeftColor = border;
        tile.style.borderRightColor = border;
        tile.style.borderTopColor = border;
        tile.style.borderBottomColor = border;
    }

    private bool IsSolved()
    {
        foreach (int turns in rotations)
        {
            if (turns != 0)
                return false;
        }

        return true;
    }

    private void Cancel()
    {
        if (!isOpen || completing)
            return;

        Action cancelled = onCancelled;
        CloseInternal();
        cancelled?.Invoke();
    }

    public void CloseWithoutCallback()
    {
        StopCompletionRoutine();

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            root.pickingMode = PickingMode.Ignore;
        }

        if (isOpen)
            RestorePlayerControl();

        isOpen = false;
        completing = false;
        onSolved = null;
        onCancelled = null;
        ClearPuzzle();
    }

    private void CloseInternal()
    {
        StopCompletionRoutine();

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            root.pickingMode = PickingMode.Ignore;
        }

        RestorePlayerControl();
        isOpen = false;
        onSolved = null;
        onCancelled = null;
        ClearPuzzle();
    }

    private void StopCompletionRoutine()
    {
        if (completionRoutine == null)
            return;

        StopCoroutine(completionRoutine);
        completionRoutine = null;
    }

    private void RestorePlayerControl()
    {
        RestoreHud();
        movementFreeze.Release();
        UnityEngine.Cursor.visible = cursorWasVisible;
        UnityEngine.Cursor.lockState = previousCursorLock;
    }

    private void HideHud()
    {
        VisualElement hud = GameplayUIManager.Instance?.HudContainer;
        if (hud == null)
            return;

        hudDisplayBeforePuzzle = hud.resolvedStyle.display;
        hudVisibilityCaptured = true;
        hud.style.display = DisplayStyle.None;
    }

    private void RestoreHud()
    {
        if (!hudVisibilityCaptured)
            return;

        VisualElement hud = GameplayUIManager.Instance?.HudContainer;
        if (hud != null)
            hud.style.display = hudDisplayBeforePuzzle;

        hudVisibilityCaptured = false;
    }

    private void ClearPuzzle()
    {
        board?.Clear();
        tiles.Clear();
        rotations.Clear();

        foreach (Sprite sprite in runtimeSprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }

        runtimeSprites.Clear();
    }

    private void OnDisable()
    {
        if (isOpen)
            CloseWithoutCallback();
    }
}
