using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ExportTerrain : EditorWindow
{
    enum SaveFormat { Triangles, Quads }
    enum Resolution { Full, Half, Quarter, Eighth, Sixteenth }

    static SaveFormat format = SaveFormat.Triangles;
    static Resolution resolution = Resolution.Full;

    [MenuItem("Terrain/Export To Obj...")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ExportTerrain)).Show();
    }

    void OnGUI()
    {
        format = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", format);
        resolution = (Resolution)EditorGUILayout.EnumPopup("Resolution", resolution);

        if (GUILayout.Button("Export"))
        {
            Export();
        }
    }

    void Export()
    {
        TerrainData terrain = Terrain.activeTerrain?.terrainData;
        if (!terrain)
        {
            EditorUtility.DisplayDialog("Error", "No active terrain found in the scene.", "OK");
            return;
        }

        string fileName = EditorUtility.SaveFilePanel("Export to OBJ", "", "Terrain", "obj");
        if (string.IsNullOrEmpty(fileName)) return;

        int w = terrain.heightmapResolution;
        int h = terrain.heightmapResolution;
        Vector3 meshScale = terrain.size;
        int tRes = (int)Mathf.Pow(2, (int)resolution);
        
        meshScale.x /= (w - 1);
        meshScale.z /= (h - 1);

        float[,] tData = terrain.GetHeights(0, 0, w, h);
        Vector3[] vertices = new Vector3[(w / tRes) * (h / tRes)];
        Vector2[] uvs = new Vector2[vertices.Length];

        int yStep = 0;
        for (int y = 0; y < h; y += tRes)
        {
            int xStep = 0;
            for (int x = 0; x < w; x += tRes)
            {
                int index = yStep * (w / tRes) + xStep;
                if (index < vertices.Length)
                {
                    vertices[index] = Vector3.Scale(meshScale, new Vector3(x, tData[y, x], y));
                    uvs[index] = new Vector2((float)x / w, (float)y / h);
                }
                xStep++;
            }
            yStep++;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("# Unity Terrain OBJ File\n");
        
        foreach (Vector3 v in vertices)
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        
        foreach (Vector2 uv in uvs)
            sb.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));

        int xSize = w / tRes;
        int ySize = h / tRes;

        for (int y = 0; y < ySize - 1; y++)
        {
            for (int x = 0; x < xSize - 1; x++)
            {
                int l1 = y * xSize + x + 1;
                int l2 = y * xSize + (x + 1) + 1;
                int l3 = (y + 1) * xSize + x + 1;
                int l4 = (y + 1) * xSize + (x + 1) + 1;

                if (format == SaveFormat.Triangles)
                {
                    sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", l1, l2, l3));
                    sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", l2, l4, l3));
                }
                else
                {
                    sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2} {3}/{3}\n", l1, l2, l4, l3));
                }
            }
        }

        File.WriteAllText(fileName, sb.ToString());
        EditorUtility.DisplayDialog("Success", "Terrain exported successfully!", "OK");
        Close();
    }
}
