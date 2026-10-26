using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoxBlast.Visuals3D
{
    /// <summary>
    /// Procedural 3D Model & Mesh Generator for Box Blast 3D.
    /// Generates console-quality rounded jewel cubes, smooth spheres, faceted gems,
    /// floating board pedestal, and materials matching the Box-blast-2D design palette.
    /// </summary>
    public static class ModelGenerator3D
    {
        public static readonly Color[] PaletteColors = new Color[]
        {
            new Color(1.00f, 0.16f, 0.30f), // Ruby Red
            new Color(0.18f, 0.48f, 1.00f), // Sapphire Blue
            new Color(0.12f, 0.84f, 0.38f), // Emerald Green
            new Color(1.00f, 0.84f, 0.00f), // Lemon Yellow
            new Color(1.00f, 0.58f, 0.06f), // Golden Topaz
            new Color(0.00f, 0.90f, 1.00f), // Electric Cyan
            new Color(0.68f, 0.20f, 1.00f), // Amethyst Purple
            new Color(1.00f, 0.20f, 0.65f)  // Candy Pink
        };

        public static readonly string[] ColorNames = new string[]
        {
            "RubyRed", "SapphireBlue", "EmeraldGreen", "LemonYellow",
            "GoldenTopaz", "ElectricCyan", "AmethystPurple", "CandyPink"
        };

        /// <summary>
        /// Generates a mathematically perfect rounded cube mesh with chamfered edges and smooth corners.
        /// </summary>
        public static Mesh CreateRoundedCubeMesh(float size = 1f, float bevelRadius = 0.2f, int subdivisions = 6)
        {
            Mesh mesh = new Mesh { name = "Mesh_Cube_Rounded_Beveled" };
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            float half = size * 0.5f;
            float inner = half - bevelRadius;

            Vector3[] faceNormals = new Vector3[]
            {
                Vector3.forward, Vector3.back,
                Vector3.up, Vector3.down,
                Vector3.right, Vector3.left
            };

            Vector3[] faceRight = new Vector3[]
            {
                Vector3.right, Vector3.left,
                Vector3.right, Vector3.right,
                Vector3.back, Vector3.forward
            };

            Vector3[] faceUp = new Vector3[]
            {
                Vector3.up, Vector3.up,
                Vector3.back, Vector3.forward,
                Vector3.up, Vector3.up
            };

            for (int f = 0; f < 6; f++)
            {
                int baseIndex = vertices.Count;
                Vector3 fNorm = faceNormals[f];
                Vector3 fRight = faceRight[f];
                Vector3 fUp = faceUp[f];

                for (int y = 0; y <= subdivisions; y++)
                {
                    float ty = (float)y / subdivisions * 2f - 1f; // -1 to 1
                    for (int x = 0; x <= subdivisions; x++)
                    {
                        float tx = (float)x / subdivisions * 2f - 1f; // -1 to 1

                        Vector3 flatPos = fNorm * half + fRight * (tx * half) + fUp * (ty * half);

                        // Clamp to inner core
                        float cx = Mathf.Clamp(flatPos.x, -inner, inner);
                        float cy = Mathf.Clamp(flatPos.y, -inner, inner);
                        float cz = Mathf.Clamp(flatPos.z, -inner, inner);
                        Vector3 corePos = new Vector3(cx, cy, cz);

                        Vector3 diff = flatPos - corePos;
                        float dist = diff.magnitude;

                        Vector3 finalPos;
                        Vector3 normal;

                        if (dist > 0.0001f)
                        {
                            normal = diff / dist;
                            finalPos = corePos + normal * bevelRadius;
                        }
                        else
                        {
                            normal = fNorm;
                            finalPos = flatPos;
                        }

                        vertices.Add(finalPos);
                        normals.Add(normal);
                        uvs.Add(new Vector2((float)x / subdivisions, (float)y / subdivisions));
                    }
                }

                int stride = subdivisions + 1;
                for (int y = 0; y < subdivisions; y++)
                {
                    for (int x = 0; x < subdivisions; x++)
                    {
                        int i0 = baseIndex + y * stride + x;
                        int i1 = i0 + 1;
                        int i2 = baseIndex + (y + 1) * stride + x;
                        int i3 = i2 + 1;

                        triangles.Add(i0);
                        triangles.Add(i2);
                        triangles.Add(i1);

                        triangles.Add(i1);
                        triangles.Add(i2);
                        triangles.Add(i3);
                    }
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        /// Generates a high-quality UV crystal sphere mesh.
        /// </summary>
        public static Mesh CreateCrystalSphereMesh(float radius = 0.5f, int latSegments = 24, int lonSegments = 32)
        {
            Mesh mesh = new Mesh { name = "Mesh_Sphere_Crystal_Smooth" };
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float theta = v * Mathf.PI;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float phi = u * Mathf.PI * 2f;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 norm = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);
                    vertices.Add(norm * radius);
                    normals.Add(norm);
                    uvs.Add(new Vector2(u, 1f - v));
                }
            }

            int stride = lonSegments + 1;
            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = lat * stride + lon;
                    int next = current + stride;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        /// Generates a brilliant-cut 3D diamond/gem mesh with faceted faces.
        /// </summary>
        public static Mesh CreateDiamondGemMesh(float radius = 0.4f, float tableHeight = 0.25f, float culetDepth = -0.35f)
        {
            Mesh mesh = new Mesh { name = "Mesh_Gem_Diamond_Faceted" };
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            int numFacets = 8;
            Vector3 tableCenter = new Vector3(0, tableHeight, 0);
            Vector3 culet = new Vector3(0, culetDepth, 0);

            // Table ring
            Vector3[] tableRing = new Vector3[numFacets];
            for (int i = 0; i < numFacets; i++)
            {
                float angle = (i * 360f / numFacets) * Mathf.Deg2Rad;
                tableRing[i] = new Vector3(Mathf.Cos(angle) * radius * 0.6f, tableHeight, Mathf.Sin(angle) * radius * 0.6f);
            }

            // Girdle ring
            Vector3[] girdleRing = new Vector3[numFacets];
            for (int i = 0; i < numFacets; i++)
            {
                float angle = ((i + 0.5f) * 360f / numFacets) * Mathf.Deg2Rad;
                girdleRing[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            // Table faces (fan)
            for (int i = 0; i < numFacets; i++)
            {
                int next = (i + 1) % numFacets;
                AddTriangle(vertices, normals, triangles, tableCenter, tableRing[i], tableRing[next]);
            }

            // Crown facets (between table and girdle)
            for (int i = 0; i < numFacets; i++)
            {
                int next = (i + 1) % numFacets;
                Vector3 g = girdleRing[i];
                AddTriangle(vertices, normals, triangles, tableRing[i], g, tableRing[next]);

                int prevG = (i - 1 + numFacets) % numFacets;
                AddTriangle(vertices, normals, triangles, tableRing[i], girdleRing[prevG], g);
            }

            // Pavilion facets (from girdle to culet)
            for (int i = 0; i < numFacets; i++)
            {
                int next = (i + 1) % numFacets;
                AddTriangle(vertices, normals, triangles, culet, girdleRing[next], girdleRing[i]);
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates an 8x8 floating pedestal platform with chamfered borders and recessed grid tray.
        /// </summary>
        public static Mesh CreateBoardPedestalMesh(float width = 6.4f, float height = 0.5f, float border = 0.4f, float recessDepth = 0.15f)
        {
            Mesh mesh = new Mesh { name = "Mesh_Board_Pedestal_8x8" };
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            float hw = width * 0.5f;
            float innerW = hw - border;

            // Top border surface
            Vector3 bTopLeft = new Vector3(-hw, 0, hw);
            Vector3 bTopRight = new Vector3(hw, 0, hw);
            Vector3 bBottomRight = new Vector3(hw, 0, -hw);
            Vector3 bBottomLeft = new Vector3(-hw, 0, -hw);

            Vector3 iTopLeft = new Vector3(-innerW, 0, innerW);
            Vector3 iTopRight = new Vector3(innerW, 0, innerW);
            Vector3 iBottomRight = new Vector3(innerW, 0, -innerW);
            Vector3 iBottomLeft = new Vector3(-innerW, 0, -innerW);

            // Recessed floor
            Vector3 rTopLeft = new Vector3(-innerW, -recessDepth, innerW);
            Vector3 rTopRight = new Vector3(innerW, -recessDepth, innerW);
            Vector3 rBottomRight = new Vector3(innerW, -recessDepth, -innerW);
            Vector3 rBottomLeft = new Vector3(-innerW, -recessDepth, -innerW);

            // Base bottom
            Vector3 baseTopLeft = new Vector3(-hw * 0.95f, -height, hw * 0.95f);
            Vector3 baseTopRight = new Vector3(hw * 0.95f, -height, hw * 0.95f);
            Vector3 baseBottomRight = new Vector3(hw * 0.95f, -height, -hw * 0.95f);
            Vector3 baseBottomLeft = new Vector3(-hw * 0.95f, -height, -hw * 0.95f);

            // Top borders
            AddQuad(vertices, normals, triangles, bTopLeft, bTopRight, iTopRight, iTopLeft);
            AddQuad(vertices, normals, triangles, bTopRight, bBottomRight, iBottomRight, iTopRight);
            AddQuad(vertices, normals, triangles, bBottomRight, bBottomLeft, iBottomLeft, iBottomRight);
            AddQuad(vertices, normals, triangles, bBottomLeft, bTopLeft, iTopLeft, iBottomLeft);

            // Recess walls
            AddQuad(vertices, normals, triangles, iTopLeft, iTopRight, rTopRight, rTopLeft);
            AddQuad(vertices, normals, triangles, iTopRight, iBottomRight, rBottomRight, rTopRight);
            AddQuad(vertices, normals, triangles, iBottomRight, iBottomLeft, rBottomLeft, rBottomRight);
            AddQuad(vertices, normals, triangles, iBottomLeft, iTopLeft, rTopLeft, rBottomLeft);

            // Recess floor
            AddQuad(vertices, normals, triangles, rTopLeft, rTopRight, rBottomRight, rBottomLeft);

            // Outer bevelled skirt walls
            AddQuad(vertices, normals, triangles, bTopRight, bTopLeft, baseTopLeft, baseTopRight);
            AddQuad(vertices, normals, triangles, bBottomRight, bTopRight, baseTopRight, baseBottomRight);
            AddQuad(vertices, normals, triangles, bBottomLeft, bBottomRight, baseBottomRight, baseBottomLeft);
            AddQuad(vertices, normals, triangles, bTopLeft, bBottomLeft, baseBottomLeft, baseTopLeft);

            // Bottom base
            AddQuad(vertices, normals, triangles, baseTopLeft, baseBottomLeft, baseBottomRight, baseTopRight);

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddTriangle(List<Vector3> verts, List<Vector3> norms, List<int> tris, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            int idx = verts.Count;
            verts.Add(v0); verts.Add(v1); verts.Add(v2);
            norms.Add(normal); norms.Add(normal); norms.Add(normal);
            tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
        }

        private static void AddQuad(List<Vector3> verts, List<Vector3> norms, List<int> tris, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 n0 = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            Vector3 n1 = Vector3.Cross(v2 - v0, v3 - v0).normalized;
            Vector3 normal = ((n0 + n1) * 0.5f).normalized;

            int idx = verts.Count;
            verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);
            norms.Add(normal); norms.Add(normal); norms.Add(normal); norms.Add(normal);

            tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
            tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 3);
        }

        /// <summary>
        /// Exports a runtime Mesh into a standard Wavefront .OBJ text string.
        /// </summary>
        public static string MeshToObjString(Mesh mesh, string materialName = "DefaultMaterial")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Box Blast 3D Procedural Model Export");
            sb.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"mtllib Materials.mtl");
            sb.AppendLine($"o {mesh.name}");
            sb.AppendLine();

            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "v {0:F6} {1:F6} {2:F6}", verts[i].x, verts[i].y, verts[i].z));
            }

            Vector3[] norms = mesh.normals;
            for (int i = 0; i < norms.Length; i++)
            {
                sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "vn {0:F6} {1:F6} {2:F6}", norms[i].x, norms[i].y, norms[i].z));
            }

            Vector2[] uvs = mesh.uv;
            if (uvs != null && uvs.Length == verts.Length)
            {
                for (int i = 0; i < uvs.Length; i++)
                {
                    sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "vt {0:F6} {1:F6}", uvs[i].x, uvs[i].y));
                }
            }

            sb.AppendLine();
            sb.AppendLine($"usemtl {materialName}");
            sb.AppendLine("s 1");

            int[] tris = mesh.triangles;
            bool hasUVs = uvs != null && uvs.Length == verts.Length;

            for (int i = 0; i < tris.Length; i += 3)
            {
                int i0 = tris[i] + 1;
                int i1 = tris[i + 1] + 1;
                int i2 = tris[i + 2] + 1;

                if (hasUVs)
                {
                    sb.AppendLine($"f {i0}/{i0}/{i0} {i1}/{i1}/{i1} {i2}/{i2}/{i2}");
                }
                else
                {
                    sb.AppendLine($"f {i0}//{i0} {i1}//{i1} {i2}//{i2}");
                }
            }

            return sb.ToString();
        }
    }
}
