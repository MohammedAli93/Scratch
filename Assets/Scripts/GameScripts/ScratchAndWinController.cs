using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScratchAndWinController : MonoBehaviour
{
    public Texture2D texture;
    public Texture2D brushTexture1;
    public Texture2D brushTexture2;
    public Texture2D brushTexture3;
    public GameObject particles;
    private SpriteRenderer spriteRenderer;
    private Texture2D canvasTexture;
    public delegate void SetPixelsBrushDelegate(float progress);
    public event SetPixelsBrushDelegate OnSetPixelsBrush;
    private int totalPixels = 0;
    private int totalPixelsRevealed = 0;
    private bool isAnimating = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Material material = GetComponent<Renderer>().material;
        canvasTexture = CreateCanvasTexture(texture);
        material.SetTexture("_Texture", texture);
        material.SetTexture("_Mask", canvasTexture);
        texture.Apply();
        totalPixels = texture.width * texture.height;
    }

    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 textureCoord = WorldToTextureCoord(mousePosition);
        SetPixelsBrush(textureCoord, brushTexture1, Color.clear);
    }

    private Vector2 WorldToTextureCoord(Vector2 worldPos)
    {
        Vector2 spritePos = spriteRenderer.transform.position;
        Vector2 relativePos = worldPos - spritePos;
        relativePos.x += spriteRenderer.sprite.bounds.size.x / 2;
        relativePos.y += spriteRenderer.sprite.bounds.size.y / 2;
        Vector2 textureCoord = new Vector2(
            relativePos.x * texture.width / spriteRenderer.sprite.bounds.size.x,
            relativePos.y * texture.height / spriteRenderer.sprite.bounds.size.y
        );
        return textureCoord;
    }

    private Vector3 TextureCoordToWorldCoord(Vector2 textureCoord)
    {
        // Get the size of the sprite in pixels
        Vector2 spriteSize = spriteRenderer.sprite.rect.size;

        // Convert pixel coordinates (0,0 at bottom-left) to normalized coordinates (0,0 to 1,1)
        Vector2 normalizedPosition = new Vector2(textureCoord.x / spriteSize.x, textureCoord.y / spriteSize.y);

        // Convert normalized position to local position on the sprite
        Vector2 localPosition = new Vector2(
            (normalizedPosition.x - 0.5f) * spriteRenderer.bounds.size.x,
            (normalizedPosition.y - 0.5f) * spriteRenderer.bounds.size.y
        );

        // Translate to world position
        return spriteRenderer.transform.TransformPoint(localPosition);
    }

    private void SetPixelsBrush(Vector2 position, Texture2D brushTexture, Color color, bool showParticles = true)
    {
        var left = (int)position.x - brushTexture.width / 2;
        var top = (int)position.y - brushTexture.height / 2;
        bool refreshCanvas = false;

        for (int x = 0; x < brushTexture.width; x++)
        {
            for (int y = 0; y < brushTexture.height; y++)
            {
                var brushColor = brushTexture.GetPixel(x, y);
                if (brushColor.a == 0)
                    continue;
                int canvasX = left + x;
                int canvasY = top + y;
                if (canvasX < 0 || canvasX >= canvasTexture.width || canvasY < 0 || canvasY >= canvasTexture.height)
                    continue;
                var canvasColor = canvasTexture.GetPixel(canvasX, canvasY);
                if (canvasColor.a == 0)
                    continue;
                refreshCanvas = true;
                totalPixelsRevealed++;
                canvasTexture.SetPixel(canvasX, canvasY, color);
            }
        }
        if (refreshCanvas) {
            canvasTexture.Apply();
            if (showParticles)
                Instantiate(particles, TextureCoordToWorldCoord(position), Quaternion.identity);
            if (!isAnimating)
            {
                float progress = (float)totalPixelsRevealed / totalPixels;
                OnSetPixelsBrush?.Invoke(progress);
            }
        }
    }

    private Texture2D CreateCanvasTexture(Texture2D original)
    {
        Texture2D canvas = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        var colors = new Color[canvas.width * canvas.height];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.black;
        canvas.SetPixels(colors);
        canvas.Apply();
        return canvas;
    }

    public void RevealCanvas()
    {
        canvasTexture.SetPixels(new Color[canvasTexture.width * canvasTexture.height]);
        canvasTexture.Apply();
    }

    public void RandomRevealCanvasAnimated()
    {
        isAnimating = true;
        int random = UnityEngine.Random.Range(0, 3);
        if (random == 0)
            StartCoroutine(RevealCanvasAnimated1());
        else if (random == 1)
            StartCoroutine(RevealCanvasAnimated2());
        else if (random == 2)
            StartCoroutine(RevealCanvasAnimated3());
        else
            StartCoroutine(RevealCanvasAnimated4());
    }

    private void StopAnimating()
    {
        isAnimating = false;
        OnSetPixelsBrush?.Invoke(1.0f);
    }

    private IEnumerator RevealCanvasAnimated1()
    {
        int precision = 20;
        float totalTime = 0.75f;
        var rows = Mathf.CeilToInt(texture.width / (float)precision);
        var cols = Mathf.CeilToInt(texture.height / (float)precision);
        float timePer = totalTime / (rows * cols);
        bool showParticles = false;
        for (int y = cols; y >= 0; y--)
        {
            for (int x = 0; x < rows; x++)
            {
                SetPixelsBrush(new Vector2(x * precision + precision / 2, y * precision + precision / 2), brushTexture3, Color.clear, showParticles);
                showParticles = !showParticles;
                yield return new WaitForSeconds(timePer);
            }
        }
        yield return new WaitForSeconds(0.1f);
        StopAnimating();
    }

    private IEnumerator RevealCanvasAnimated2()
    {
        bool showParticles = false;
        for (int i = 0; i < 6; i++)
        {
            SetPixelsBrush(new Vector2(UnityEngine.Random.Range(0, texture.width), UnityEngine.Random.Range(0, texture.height)), brushTexture2, Color.clear, showParticles);
            showParticles = !showParticles;
            yield return new WaitForSeconds(0.125f);
        }
        RevealCanvas();
        StopAnimating();
    }

    private IEnumerator RevealCanvasAnimated3()
    {
        int precision = 20;
        float totalTime = 0.75f;
        var rows = Mathf.CeilToInt(texture.width / (float)precision);
        var cols = Mathf.CeilToInt(texture.height / (float)precision);
        float timePer = totalTime / (rows * cols);
        bool showParticles = false;
        for (int y = cols; y >= 0; y--)
        {
            int x = 0;
            for (int i = 0; i < cols - y; i++)
            {
                var position = new Vector2((x + i) * precision + precision / 2, (y + i) * precision + precision / 2);
                SetPixelsBrush(position, brushTexture3, Color.clear, showParticles);
                showParticles = !showParticles;
                yield return new WaitForSeconds(timePer);
            }
        }
        for (int x = 0; x < rows; x++)
        {
            int y = 0;
            for (int i = 0; i < rows - x; i++)
            {
                var position = new Vector2((x + i) * precision + precision / 2, (y + i) * precision + precision / 2);
                SetPixelsBrush(position, brushTexture3, Color.clear, showParticles);
                showParticles = !showParticles;
                yield return new WaitForSeconds(timePer);
            }
        }
        StopAnimating();
    }

    private IEnumerator RevealCanvasAnimated4()
    {
        int precision = 20;
        float totalTime = 0.75f;
        var rows = Mathf.CeilToInt(texture.width / (float)precision);
        var cols = Mathf.CeilToInt(texture.height / (float)precision);
        float timePer = totalTime / (rows * cols);
        bool showParticles = false;
        for (int x = rows; x >= 0; x--)
        {
            int y = 0;
            for (int i = 0; i < rows - x; i++)
            {
                var position = new Vector2((x + i) * precision + precision / 2, (y + i) * precision + precision / 2);
                SetPixelsBrush(position, brushTexture3, Color.clear, showParticles);
                showParticles = !showParticles;
                yield return new WaitForSeconds(timePer);
            }
        }
        for (int y = 0; y < cols; y++)
        {
            int x = 0;
            for (int i = 0; i < cols - y; i++)
            {
                var position = new Vector2((x + i) * precision + precision / 2, (y + i) * precision + precision / 2);
                SetPixelsBrush(position, brushTexture3, Color.clear, showParticles);
                showParticles = !showParticles;
                yield return new WaitForSeconds(timePer);
            }
        }
        StopAnimating();
    }

    public bool CheckIfCanvasRectIsRevealed(Rect rect)
    {
        var rectX = (int)rect.x;
        var rectY = (int)rect.y;
        for (int x = rectX; x < rectX + rect.width; x++)
        {
            for (int y = rectY; y < rectY + rect.height; y++)
            {
                if (x < 0 || x >= canvasTexture.width || y < 0 || y >= canvasTexture.height)
                    return false;
                if (canvasTexture.GetPixel(x, y).a > 0)
                    return false;
            }
        }
        return true;
    }

    public void SetPixelsRect(Rect rect, Color color)
    {
        var rectX = (int)rect.x;
        var rectY = (int)rect.y;
        for (int x = rectX; x < rectX + rect.width; x++)
        {
            for (int y = rectY; y < rectY + rect.height; y++)
            {
                if (x < 0 || x >= canvasTexture.width || y < 0 || y >= canvasTexture.height)
                    continue;
                canvasTexture.SetPixel(x, y, color);
            }
        }
        canvasTexture.Apply();
    }
}
