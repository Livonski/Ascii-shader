using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ASCIIRenderer : MonoBehaviour
{
    [SerializeField] private ComputeShader _ASCIIShader;

    [SerializeField] private Camera _camera;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private RenderTexture _outputTexture;
    [SerializeField] private Texture2D _cameraTexture;

    [SerializeField] private Font _font;

    [SerializeField] private Material _outputMaterial;

    [SerializeField] private float _contrast;

    private Vector2Int _resolution;


    private Texture2DArray _glyphTextureArray;
    private ComputeBuffer _glyphCoveragesBuffer;

    public void OnFontsGenerated()
    {
        InitializeGlyphTextureArray();
        _outputTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        _outputTexture.enableRandomWrite = true;
        _outputTexture.Create();
        _cameraTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        _resolution = new Vector2Int(_renderTexture.width, _renderTexture.height);

        _camera.targetTexture = _renderTexture;

        InitializeGlyphCoveragesBuffer();
        _ASCIIShader.SetTexture(0, "glyphTextures", _glyphTextureArray);
        _ASCIIShader.SetInt("textureWidth", _resolution.x);
        _ASCIIShader.SetInt("textureHeight", _resolution.y);
        _ASCIIShader.SetInt("numGlyphs", _font._glyphs.Length);
        _ASCIIShader.SetFloat("contrast", _contrast);
    }

    void Update()
    {
        RenderTexture.active = _renderTexture;
        _cameraTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _cameraTexture.Apply();
        RenderTexture.active = null;

        _ASCIIShader.SetTexture(0, "cameraTexture", _cameraTexture);
        _ASCIIShader.SetTexture(0, "outputTexture", _outputTexture);
        _ASCIIShader.SetFloat("contrast", _contrast);

        int threadGroupsX = Mathf.CeilToInt(_resolution.x / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_resolution.y / 16.0f);
        _ASCIIShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        ApplyOuputTexture();
    }

    private void InitializeGlyphTextureArray()
    {
        int glyphCount = _font._glyphs.Length;
        int glyphSize = 16;

        _glyphTextureArray = new Texture2DArray(glyphSize,glyphSize, glyphCount, TextureFormat.ARGB32, false);

        for (int i = 0; i < glyphCount; i++)
        {
            Texture2D glyphTexture = new Texture2D(glyphSize, glyphSize, TextureFormat.ARGB32, false);
            glyphTexture.SetPixels(_font._glyphs[i].texture);
            glyphTexture.Apply();

            Graphics.CopyTexture(glyphTexture, 0, 0, _glyphTextureArray, i, 0);
        }
    }

    private void InitializeGlyphCoveragesBuffer()
    {
        int glyphCount = _font._glyphs.Length;
        float[] glyphCoverages = new float[glyphCount];

        for (int i = 0; i < glyphCount; i++)
        {
            glyphCoverages[i] = _font._glyphs[i].coverage;
        }

        _glyphCoveragesBuffer = new ComputeBuffer(glyphCoverages.Length, sizeof(float));
        _glyphCoveragesBuffer.SetData(glyphCoverages);

        _ASCIIShader.SetBuffer(0, "glyphCoverages", _glyphCoveragesBuffer);
    }

    private void ApplyOuputTexture()
    {
        if(_outputMaterial != null)
            _outputMaterial.mainTexture = _outputTexture;
    }

    private void OnDestroy()
    {
        if (_glyphTextureArray != null)
            Destroy(_glyphTextureArray);
        if (_glyphCoveragesBuffer != null)
            _glyphCoveragesBuffer.Release();
    }
}
