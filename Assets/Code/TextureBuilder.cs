using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBuilder : MonoBehaviour
{
    private Vector2Int _size;
    private Texture2D _texture;

    public void AddPixels(Vector2Int position, Vector2Int size, Color[] pixels)
    {
        _texture.SetPixels(position.x, position.y, size.x, size.y, pixels);
        _texture.Apply();
    }

    public Texture2D GetTexture()
    {
        return _texture;
    }

    public void CreateNewTexture(Vector2Int size)
    {
        _size = size;
        _texture = new Texture2D(size.x,size.y);
    }
}
