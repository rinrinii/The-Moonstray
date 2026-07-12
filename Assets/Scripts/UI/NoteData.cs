using UnityEngine;

[CreateAssetMenu(menuName = "Moonstray/Note")]
public class NoteData : ScriptableObject
{
    public string title;

    [TextArea(8, 30)]
    public string content;
}
