using System;

public sealed class DialogueChoice
{
    public string Text { get; }

    private readonly Action onSelected;

    public DialogueChoice(string text, Action onSelected)
    {
        Text = text;
        this.onSelected = onSelected;
    }

    public void Select()
    {
        onSelected?.Invoke();
    }
}
