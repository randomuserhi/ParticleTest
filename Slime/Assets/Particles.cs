using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    RenderTexture Texture;
    RenderTexture DiffuseTexture;
    RenderTexture DisplayTexture;
    public Texture2D EnvironmentTexture;
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
        Application.runInBackground = true;

        Texture = new RenderTexture(1920, 1080, 24);
        Texture.enableRandomWrite = true;
        Texture.filterMode = FilterMode.Point;
        Texture.Create();

        DiffuseTexture = new RenderTexture(1920, 1080, 24);
        DiffuseTexture.enableRandomWrite = true;
        DiffuseTexture.filterMode = FilterMode.Point;
        DiffuseTexture.Create();

        DisplayTexture = new RenderTexture(1920, 1080, 24);
        DisplayTexture.enableRandomWrite = true;
        //DisplayTexture.filterMode = FilterMode.Point;
        DisplayTexture.Create();

        transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = DisplayTexture;

        ParticleList = new Particle[500000];

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
        Shader.SetTexture(0, "EnvironmentMap", EnvironmentTexture);
        Shader.SetTexture(4, "EnvironmentMap", EnvironmentTexture);
        Shader.SetTexture(4, "DiffusedMap", DiffuseTexture);

        //Shader.SetTexture(2, "EnvironmentMap", EnvironmentTexture);
        //Shader.Dispatch(2, EnvironmentTexture.width / 8 + EnvironmentTexture.width % 8, EnvironmentTexture.height / 8 + EnvironmentTexture.height % 8, 1);
        Shader.Dispatch(3, ParticleList.Length / 16 + ParticleList.Length % 16, 1, 1);
    }

    void FixedUpdate()
    {
        Shader.SetFloat("Time", Time.fixedTime);
        //Shader.Dispatch(1, Texture.width / 8 + Texture.width % 8, Texture.height / 8 + Texture.height % 8, 1);
        Shader.Dispatch(0, ParticleList.Length / 16 + ParticleList.Length % 16, 1, 1);
        Shader.Dispatch(4, DiffuseTexture.width / 8 + DiffuseTexture.width % 8, DiffuseTexture.height / 8 + DiffuseTexture.height % 8, 1);
        Graphics.Blit(DiffuseTexture, Texture);
    }

    public Material Transparency;

    private void LateUpdate()
    {
        Graphics.Blit(Texture, DisplayTexture);
    }

    private void OnApplicationQuit()
    {
        ParticleBuffer.Dispose();
        Texture.Release();
    }
}
