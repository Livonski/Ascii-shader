using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ASCIIRenderer : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private Texture2D _cameraTexture;

    [SerializeField] private Vector2Int _resolution;

    [SerializeField] private Font _font;
    [SerializeField] private TextureBuilder _builder;

    [SerializeField] private Material _debugMaterial;


    void Start()
    {
        Debug.Log(Screen.width + " " + Screen.height);
        _renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _camera.targetTexture = _renderTexture;
        _cameraTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _resolution = new Vector2Int(_renderTexture.width, _renderTexture.height);
    }

    void Update()
    {
        RenderTexture.active = _renderTexture;
        _cameraTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _cameraTexture.Apply();
        RenderTexture.active = null;
        //Texture2D outputTexture = GenerateGrayscaleImage(_resolution, _cameraTexture);
        //outputTexture.Apply();
        Texture2D outputTexture = new Texture2D(_resolution.x, _resolution.y);
        outputTexture = ConvertToASCII(_cameraTexture, new Vector2Int(16, 16));
        outputTexture.Apply();
        _debugMaterial.mainTexture = outputTexture;
    }

    private Texture2D GenerateGrayscaleImage(Vector2Int size, Texture2D texture)
    {
        Texture2D grayscaleTexture = new Texture2D(size.x, size.y);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Color color = texture.GetPixel(x, y);
                float grayscale = (color.r + color.g + color.b) / 3;
                Color grayscaleColor = new Color(grayscale, grayscale, grayscale);
                grayscaleTexture.SetPixel(x,y,grayscaleColor);
            }
        }

        grayscaleTexture.Apply();
        return grayscaleTexture;
    }

    private Texture2D GenerateRandomTexture(Vector2Int size)
    {
        Texture2D randomTexture = new Texture2D(size.x, size.y);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                float R = Random.Range(0f, 1f);
                float G = Random.Range(0f, 1f);
                float B = Random.Range(0f, 1f);

                Color randomColor = new Color(R, G, B);
                randomTexture.SetPixel(x, y, randomColor);
            }
        }
        randomTexture.Apply();
        return randomTexture;
    }

    private Texture2D ConvertToASCII(Texture2D texture, Vector2Int glyphSize)
    {
        Texture2D output = new Texture2D(texture.width, texture.height);
        _builder.CreateNewTexture(new Vector2Int(texture.width, texture.height));
        for (int x = 0; x < _resolution.x; x += 16)
        {
            for (int y = 0; y < _resolution.y; y += 16)
            {
                float avgValue = SamplePixels(texture, new Vector2Int(x, y), glyphSize);
                Glyph glyph = _font.GetGlyph(avgValue);
                _builder.AddPixels(new Vector2Int(x, y), glyphSize, glyph.texture);
            }
        }
        output = _builder.GetTexture();
        return output;
    }

    private float SamplePixels(Texture2D texture, Vector2Int position, Vector2Int size)
    {
        float avgColor = 0f;
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                Color color = texture.GetPixel(x, y);
                avgColor += (color.r + color.g + color.b) / 3;
            }
        }

        avgColor = avgColor / (size.x * size.y);
        return avgColor;
    }
}
