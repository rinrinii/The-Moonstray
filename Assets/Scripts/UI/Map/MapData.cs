using UnityEngine;

[CreateAssetMenu(menuName = "Moonstray/Map Data")]
public class MapData : ScriptableObject
{
    public string sceneName;
    public string regionTitle;
    public Sprite mapSprite;

    [Header("World Bounds")]
    public Vector2 worldMin;
    public Vector2 worldMax;
}