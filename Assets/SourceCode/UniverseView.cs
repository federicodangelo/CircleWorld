using UnityEngine;
using Universe;

public class UniverseView : MonoBehaviour
{
    private ThingsContainer thingsContainer;
    private float time;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private bool firstTime = true;

    public void Start()
    {
        thingsContainer = new ThingsContainer();
        thingsContainer.Create(0);
    }

    public void Update()
    {
        time += Time.deltaTime;

        thingsContainer.UpdatePositions(time);

        //if (firstTime)
            UpdateMesh();
    }

    private void UpdateMesh()
    {
        Thing[] things = thingsContainer.things;
        ThingPosition[] thingsPositions = thingsContainer.thingsPositions;
        ushort[] thingsToRender = thingsContainer.thingsToRender;
        ushort thingsToRenderAmount = thingsContainer.thingsToRenderAmount;

        int vertexCount = thingsToRenderAmount * 4;
        int triangleCount = thingsToRenderAmount * 6;

        if (vertices == null || vertices.Length != vertexCount)
            vertices = new Vector3[vertexCount];

        if (uvs == null || uvs.Length != vertexCount)
            uvs = new Vector2[vertexCount];

        if (triangles == null || triangles.Length != triangleCount)
            triangles = new int[triangleCount];

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        int vertexOffset = 0;

        if (firstTime)
        {
            float tx = 1.0f / 4.0f;
            float ty = 1.0f / 4.0f;
            float tt = 1.0f / 256.0f;

            int triangleOffset = 0;

            for (ushort i = 0; i < thingsToRenderAmount; i++)
            {
                Thing thing = things[thingsToRender[i]];

                int textureId = (int) (((uint) thing.seed) % 16);

                float uvx = (textureId % 4) / 4.0f;
                float uvy = (textureId / 4) / 4.0f;

                uvs[vertexOffset + 0] = new Vector2(uvx, 1.0f - uvy);
                uvs[vertexOffset + 1] = new Vector2(uvx + tx - tt, 1.0f - uvy);
                uvs[vertexOffset + 2] = new Vector2(uvx + tx - tt, 1.0f - (uvy + ty) + tt);
                uvs[vertexOffset + 3] = new Vector2(uvx, 1.0f - (uvy + ty) + tt);

                triangles[triangleOffset + 0] = vertexOffset + 0;
                triangles[triangleOffset + 1] = vertexOffset + 1;
                triangles[triangleOffset + 2] = vertexOffset + 2;

                triangles[triangleOffset + 3] = vertexOffset + 2;
                triangles[triangleOffset + 4] = vertexOffset + 3;
                triangles[triangleOffset + 5] = vertexOffset + 0;

                triangleOffset += 6;
                vertexOffset += 4;
            }
        }

        vertexOffset = 0;

        for (int i = 0; i < thingsToRenderAmount; i++)
        {
            ThingPosition position = thingsPositions[thingsToRender[i]];

            vertices[vertexOffset + 0].x = position.x - position.radius;
            vertices[vertexOffset + 0].y = position.y - position.radius;

            vertices[vertexOffset + 1].x = position.x - position.radius;
            vertices[vertexOffset + 1].y = position.y + position.radius;

            vertices[vertexOffset + 2].x = position.x + position.radius;
            vertices[vertexOffset + 2].y = position.y + position.radius;

            vertices[vertexOffset + 3].x = position.x + position.radius;
            vertices[vertexOffset + 3].y = position.y - position.radius;

            vertexOffset += 4;
        }

        mesh.vertices = vertices;

        if (firstTime)
        {
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(ushort.MaxValue * 2, ushort.MaxValue * 2, 0.0f));

            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        firstTime = false;
    }

    /*
    public void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }

    public void OnDrawGizmos()
    {
        if (thingsContainer != null)
        {
            int amount = thingsContainer.thingsAmount;

            Thing[] things = thingsContainer.things;
            ThingPosition[] positions = thingsContainer.thingsPositions;

            for (int i = 0; i < amount; i++)
            {
                Color color;

                switch ((ThingType) things[i].type)
                {
                    case ThingType.Galaxy:
                        //color = Color.blue;
                        continue;
                        //break;

                    case ThingType.SolarSystem:
                        //color = Color.cyan;
                        continue;
                        //break;

                    case ThingType.Sun:
                        color = Color.yellow;
                        break;

                    case ThingType.Moon:
                        color = Color.gray;
                        break;

                    case ThingType.Planet:
                        color = Color.green;
                        break;

                    default:
                        color = Color.gray;
                        break;
                }

                Gizmos.color = color;

                //if (things[i].safeRadius > 0)
                //    Gizmos.DrawWireSphere(new Vector3(positions[i].x, positions[i].y, 0.0f), things[i].safeRadius); 

                if (things[i].radius > 0)
                    Gizmos.DrawSphere(new Vector3(positions[i].x, positions[i].y, 0.0f), things[i].radius); 
            }
        }
    }
    */
}


