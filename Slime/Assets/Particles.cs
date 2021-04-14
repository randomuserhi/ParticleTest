using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

static class CanvasExtensions
{
    public static Vector2 SizeToParent(this RawImage Image, float Padding = 0)
    {
        var Parent = Image.transform.parent.GetComponentInParent<RectTransform>();
        var Transform = Image.GetComponent<RectTransform>();
        if (!Parent) { return Transform.sizeDelta; } //if we don't have a parent, just return our current width
        Padding = 1 - Padding;
        float Ratio = Image.texture.width / (float)Image.texture.height;
        var Bounds = new Rect(0, 0, Parent.rect.width, Parent.rect.height);
        if (Mathf.RoundToInt(Transform.eulerAngles.z) % 180 == 90)
        {
            //Invert the bounds if the image is rotated
            Bounds.size = new Vector2(Bounds.height, Bounds.width);
        }
        //Size by height first
        float Height = Bounds.height * Padding;
        float Width = Height * Ratio;
        if (Width > Bounds.width * Padding)
        { //If it doesn't fit, fallback to width
            Width = Bounds.width * Padding;
            Height = Width / Ratio;
        }
        Transform.sizeDelta = new Vector2(Width, Height);
        return Transform.sizeDelta;
    }
}

public class Particles : MonoBehaviour
{
    RenderTexture Texture;
    RenderTexture DiffuseTexture;
    public RawImage Render;
    public ComputeShader Shader;
    public ComputeBuffer ParticleBuffer;

    struct Particle
    {
        Vector2 Position;
        Vector2 Velocity;
        float Angle;
        public static int Size()
        {
            return sizeof(float) * 5;
        }
    }

    Particle[] ParticleList;

    void Start()
    {
        Texture = new RenderTexture(1920, 1080, 24);
        Texture.enableRandomWrite = true;
        Texture.filterMode = FilterMode.Point;
        Texture.Create();

        DiffuseTexture = new RenderTexture(1920, 1080, 24);
        DiffuseTexture.enableRandomWrite = true;
        DiffuseTexture.filterMode = FilterMode.Point;
        DiffuseTexture.Create();

        ParticleList = new Particle[2500];

        ParticleBuffer = new ComputeBuffer(ParticleList.Length, Particle.Size());
        ParticleBuffer.SetData(ParticleList);

        Shader.SetInt("Width", Texture.width);
        Shader.SetInt("Height", Texture.height);
        Shader.SetFloat("TimeDelta", Time.fixedDeltaTime);
        Shader.SetBuffer(0, "Particles", ParticleBuffer);
        Shader.SetBuffer(3, "Particles", ParticleBuffer);
        Shader.SetInt("ParticleCount", ParticleList.Length);
        Shader.SetTexture(0, "Result", Texture);
        Shader.SetTexture(1, "Result", Texture);
        Shader.SetTexture(4, "Result", Texture);
        Shader.SetTexture(4, "DiffusedMap", DiffuseTexture);

        Shader.SetTexture(2, "Result", Texture);
        Shader.Dispatch(2, Texture.width / 8 + Texture.width % 8, Texture.height / 8 + Texture.height % 8, 1);
        Shader.Dispatch(3, ParticleList.Length / 16 + ParticleList.Length % 16, 1, 1);

        Render.texture = DiffuseTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (Render != null) Render.SizeToParent();
    }

    float TotalTime;
    void FixedUpdate()
    {
        Shader.SetFloat("Time", TotalTime);
        //Shader.Dispatch(1, Texture.width / 8 + Texture.width % 8, Texture.height / 8 + Texture.height % 8, 1);
        Shader.Dispatch(0, ParticleList.Length / 16 + ParticleList.Length % 16, 1, 1);
        Shader.Dispatch(4, DiffuseTexture.width / 8 + DiffuseTexture.width % 8, DiffuseTexture.height / 8 + DiffuseTexture.height % 8, 1);
        Graphics.Blit(DiffuseTexture, Texture);
        TotalTime += Time.fixedDeltaTime;
    }

    private void OnApplicationQuit()
    {
        ParticleBuffer.Dispose();
        Texture.Release();
    }
}
