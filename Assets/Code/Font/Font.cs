using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Font : MonoBehaviour
{
    [SerializeField] private Texture2D _fontTexture;
    [SerializeField] private Vector2Int _glyphSize;

    [SerializeField] private Glyph[] _glyphs;

    public Glyph GetGlyph(float targetCoverage)
    {
        float dCoverage = 0;
        for (int i = 0; i < _glyphs.Length; i++)
        {
            dCoverage = _glyphs[i].coverage - targetCoverage;
            if (dCoverage >= 0)
            {
                return _glyphs[i];
            }
        }
        return _glyphs[_glyphs.Length - 1];
    }

    private void Start()
    {
        GenerateGlyphs();
    }

    private void GenerateGlyphs()
    {
        _glyphs = new Glyph[_fontTexture.width/_glyphSize.x * _fontTexture.height/_glyphSize.y];
        int ID = 0;
        for (int y = 0; y < _fontTexture.height; y += _glyphSize.y)
        {
            for (int x = 0; x < _fontTexture.width; x += _glyphSize.x)
            {
                Color[] slice = _fontTexture.GetPixels(x,y,_glyphSize.x, _glyphSize.y);
                float coverage = CalculateCoverage(slice);

                Glyph newGlyph = new Glyph(ID,slice,coverage);
                _glyphs[ID] = newGlyph;

                ID++;
            }
        }
        _glyphs = SortGlyphs(_glyphs);
        //PrintDebugInfo();
    }

    private float CalculateCoverage(Color[] glyph)
    {
        float coverage = 0f;
        float size = glyph.Length;
        float nonBlankPixels = 0;
        foreach (Color c in glyph)
        {
            if (c.r != 0)
                nonBlankPixels++;
        }
        coverage = nonBlankPixels / size;

        return coverage;
    }

    //TODO: implement better sorting algorythm
    private Glyph[] SortGlyphs(Glyph[] glyph)
    {
        for (int j = 0; j < glyph.Length; j++)
        { 
            for (int i = 0; i < glyph.Length - 1; i++)
            {
                if (glyph[i].coverage > glyph[i + 1].coverage)
                {
                    glyph = Swap(glyph, i, i + 1);
                }
            }
        }
        return glyph;
    }

    private Glyph[] Swap(Glyph[] arr, int a, int b)
    {
        Glyph tmp = arr[a];
        arr[a] = arr[b];
        arr[b] = tmp;
        return arr;
    }

    private void PrintDebugInfo()
    {
        foreach (Glyph glyph in _glyphs)
        {
            Debug.Log($"Glyph {glyph.ID}, coverage: {glyph.coverage}");
        }
    }
}

[System.Serializable]
public struct Glyph
{
    public int ID;
    public Color[] texture;
    public float coverage;

    public Glyph (int ID, Color[] texture, float coverage)
    {
        this.ID = ID;
        this.texture = texture;
        this.coverage = coverage;
    }
}
