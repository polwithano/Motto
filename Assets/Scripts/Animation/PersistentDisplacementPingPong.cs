using UnityEngine;

public class PersistentDisplacementPingPong : MonoBehaviour
{
    [Header("Materials")]
    public Material updateMaterial;   // BalatroDisplacementUpdateURP
    public Material renderMaterial;   // BalatroRipplePersistentURP

    [Header("RenderTexture")]
    public int resolution = 512;
    public RenderTextureFormat format = RenderTextureFormat.RGHalf;

    RenderTexture rtA;
    RenderTexture rtB;
    bool ping;

    void Start()
    {
        rtA = CreateRT();
        rtB = CreateRT();

        ClearRT(rtA, new Color(0.5f, 0.5f, 0, 1));
        ClearRT(rtB, new Color(0.5f, 0.5f, 0, 1));

        renderMaterial.SetTexture("_DispTex", rtA);
        updateMaterial.SetTexture("_DispTex", rtA);
    }

    static void ClearRT(RenderTexture rt, Color c)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, c);
        RenderTexture.active = prev;
    }

    void LateUpdate()
    {
        var src = ping ? rtA : rtB;
        var dst = ping ? rtB : rtA;

        updateMaterial.SetTexture("_DispTex", src);
        Graphics.Blit(src, dst, updateMaterial);

        ping = !ping;

        // Feed final material
        renderMaterial.SetTexture("_DispTex", ping ? rtA : rtB);
    }

    RenderTexture CreateRT()
    {
        var rt = new RenderTexture(resolution, resolution, 0, format);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;
        rt.enableRandomWrite = false;
        rt.Create();
        return rt;
    }

    void OnDestroy()
    {
        if (rtA) rtA.Release();
        if (rtB) rtB.Release();
    }
}