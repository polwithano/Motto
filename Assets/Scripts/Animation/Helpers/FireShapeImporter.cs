using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Animation.Helpers
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FireShapeImporter : MonoBehaviour
    {
        [Header("Source")]
        public TextAsset svgFile;
        public float scale = 100f; 

        [Header("Shader control (mirror des properties)")]
        [Range(0f, 3f)] public float height    = 1f;
        [Range(0f, 50f)] public float speed     = 1.2f;
        [Range(0f, 100f)] public float intensity = 0.5f;

        struct CubicBezier { public Vector2 p0, p1, p2, p3; }

        MaterialPropertyBlock _mpb;
        MeshRenderer          _mr;

        void Start()
        {
            _mr  = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();

            transform.localPosition = new Vector3(-scale / 2, -scale / 2, 1);
            
            var curves = ParseSVG(svgFile.text);
            if (curves.Count == 0) { Debug.LogError("SVG : aucune courbe trouvée"); return; }

            GetComponent<MeshFilter>().mesh = BuildMesh(curves);
            UpdateShader();
        }

        void Update() => UpdateShader();   // permet de tweaker en live depuis l'Inspector

        void UpdateShader()
        {
            _mr.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Height",    height);
            _mpb.SetFloat("_Speed",     speed);
            _mpb.SetFloat("_Intensity", intensity);
            _mr.SetPropertyBlock(_mpb);
        }

        // ── API publique : appeler depuis le GameManager ─────────────────
        public void SetHeight(float normalizedScore)     => height    = normalizedScore;
        public void SetIntensity(float normalizedScore)  => intensity = normalizedScore;

        // ════════════════════════════════════════════════════════════════
        // SVG PARSING
        // ════════════════════════════════════════════════════════════════
        List<CubicBezier> ParseSVG(string svg)
        {
            var result = new List<CubicBezier>();
            var pathMatches = Regex.Matches(svg, @"<path[^>]*\sd=""([^""]+)""", RegexOptions.IgnoreCase);
            foreach (Match pm in pathMatches)
                result.AddRange(ParsePathD(pm.Groups[1].Value));
            return result;
        }

        List<CubicBezier> ParsePathD(string d)
        {
            var curves  = new List<CubicBezier>();
            var tokens  = Regex.Split(d.Trim(), @"(?=[MmCcLlZzSs])");
            Vector2 cur = Vector2.zero, start = Vector2.zero;

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                char     cmd  = token[0];
                var      nums = ExtractFloats(token.Substring(1));
                int      idx  = 0;

                switch (cmd)
                {
                    case 'M': cur = start = V(nums, 0); break;
                    case 'm': cur = start = cur + V(nums, 0); break;

                    case 'C':
                        while (idx + 5 < nums.Count + 1)
                        {
                            curves.Add(new CubicBezier {
                                p0 = cur,             p1 = V(nums, idx),
                                p2 = V(nums, idx + 2), p3 = V(nums, idx + 4) });
                            cur = V(nums, idx + 4); idx += 6;
                        }
                        break;

                    case 'c':
                        while (idx + 5 < nums.Count + 1)
                        {
                            curves.Add(new CubicBezier {
                                p0 = cur,                  p1 = cur + V(nums, idx),
                                p2 = cur + V(nums, idx+2), p3 = cur + V(nums, idx+4) });
                            cur = cur + V(nums, idx + 4); idx += 6;
                        }
                        break;

                    case 'L':
                        while (idx + 1 < nums.Count + 1)
                        { var e = V(nums,idx); curves.Add(Line(cur,e)); cur=e; idx+=2; }
                        break;

                    case 'Z': case 'z':
                        if (cur != start) curves.Add(Line(cur, start));
                        cur = start; break;
                }
            }
            return curves;
        }

        CubicBezier Line(Vector2 a, Vector2 b) => new CubicBezier {
            p0=a, p1=Vector2.Lerp(a,b,.333f), p2=Vector2.Lerp(a,b,.666f), p3=b };

        Vector2 V(List<float> n, int i) => new Vector2(n[i], -n[i+1]); // flip Y

        List<float> ExtractFloats(string s)
        {
            var r = new List<float>();
            foreach (Match m in Regex.Matches(s, @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?"))
                if (float.TryParse(m.Value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float f)) r.Add(f);
            return r;
        }

        // ════════════════════════════════════════════════════════════════
        // MESH GENERATION
        // ════════════════════════════════════════════════════════════════
        Mesh BuildMesh(List<CubicBezier> curves)
        {
            // 1. Contour
            var contour = new List<Vector2>();
            foreach (var c in curves)
                for (int s = 0; s < 20; s++)
                    contour.Add(SampleCubic(c, s / 20f));

            // 2. Normalise dans [0,1] — uv.y = hauteur normalisée (important pour le shader)
            float minX=float.MaxValue, maxX=float.MinValue, minY=float.MaxValue, maxY=float.MinValue;
            foreach (var p in contour) {
                if(p.x<minX)minX=p.x; if(p.x>maxX)maxX=p.x;
                if(p.y<minY)minY=p.y; if(p.y>maxY)maxY=p.y;
            }
            float sx = maxX-minX, sy = maxY-minY, sc = Mathf.Max(sx,sy);
            for (int i = 0; i < contour.Count; i++)
                contour[i] = new Vector2((contour[i].x-minX)/sc, (contour[i].y-minY)/sc);

            // 3. Verts + UVs (uv.y = hauteur normalisée → utilisée par le shader pour gradient + animation)
            var verts = new List<Vector3>();
            var uvs   = new List<Vector2>();
            foreach (var p in contour)
            {
                verts.Add(p * scale); 
                uvs.Add(new Vector2(p.x, p.y));
            }

            var mesh = new Mesh();
            mesh.vertices  = verts.ToArray();
            mesh.uv        = uvs.ToArray();
            mesh.triangles = EarClip(contour).ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        Vector2 SampleCubic(CubicBezier c, float t)
        {
            float u = 1f-t;
            return u*u*u*c.p0 + 3*u*u*t*c.p1 + 3*u*t*t*c.p2 + t*t*t*c.p3;
        }

        List<int> EarClip(List<Vector2> poly)
        {
            var tris = new List<int>();
            var idx  = new List<int>(); for(int i=0;i<poly.Count;i++) idx.Add(i);
            int safety = poly.Count * poly.Count, iter = 0;
            while (idx.Count > 3 && iter++ < safety)
            {
                for (int i = 0; i < idx.Count; i++)
                {
                    int a=idx[(i-1+idx.Count)%idx.Count], b=idx[i], c=idx[(i+1)%idx.Count];
                    if (!IsEar(poly,a,b,c,idx)) continue;
                    tris.Add(a); tris.Add(b); tris.Add(c);
                    idx.RemoveAt(i); break;
                }
            }
            if (idx.Count==3) tris.AddRange(new[]{idx[0],idx[1],idx[2]});
            return tris;
        }

        bool IsEar(List<Vector2> p, int a, int b, int c, List<int> idx)
        {
            if (Cross(p[b]-p[a], p[c]-p[a]) <= 0f) return false;
            foreach (int i in idx) {
                if (i==a||i==b||i==c) continue;
                if (InTri(p[i],p[a],p[b],p[c])) return false;
            }
            return true;
        }
        float Cross(Vector2 a, Vector2 b) => a.x*b.y - a.y*b.x;
        bool  InTri(Vector2 p, Vector2 A, Vector2 B, Vector2 C)
        {
            float d1=Cross(p-A,B-A), d2=Cross(p-B,C-B), d3=Cross(p-C,A-C);
            return !((d1<0||d2<0||d3<0) && (d1>0||d2>0||d3>0));
        }
    }
}