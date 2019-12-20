using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOutCode : MonoBehaviour
{
    /*
     * flood fill
     * scan line
     * lambert lighting
     */
    private static readonly int resX = 300;
    private static readonly int resY = 200;
    private float myAngle;
    private Texture2D screen;
    private readonly Vector3[] cube = new Vector3[8];
    private int[] triangles;
    Color newColor = Color.magenta;

    // Start is called before the first frame update
    void Start()
    {
        //Cube
        cube[0] = new Vector3(1, 1, 1);
        cube[1] = new Vector3(-1, 1, 1);
        cube[2] = new Vector3(-1, -1, 1);
        cube[3] = new Vector3(1, -1, 1);
        cube[4] = new Vector3(1, 1, -1);
        cube[5] = new Vector3(-1, 1, -1);
        cube[6] = new Vector3(-1, -1, -1);
        cube[7] = new Vector3(1, -1, -1);

        //Triangles to draw faces
        triangles = new int[]
        {
            0, 1, 2,        //Front face
            0, 2, 3,
            4, 0, 3,        //Right face
            4, 3, 7,
            1, 5, 6,        //Left face
            1, 6, 2,
            5, 4, 7,        //Back face
            5, 7, 6,
            4, 5, 1,        //Top face
            4, 1, 0,
            3, 2, 6,        //Bottom face
            3, 6, 7
        };
    }

    Matrix4x4 RotationMatrix(float angle, Vector3 axis)
    {
        return Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis.normalized), Vector3.one);
    }

    Matrix4x4 ViewingMatrix(Vector3 PosCamera, Vector3 target, Vector3 up)
    {
        Quaternion rot = Quaternion.LookRotation(target - PosCamera, up.normalized);
        return Matrix4x4.TRS(-PosCamera, rot, Vector3.one);
    }

    bool LineClip(ref Vector2 v, ref Vector2 u)
    {
        OutCode inViewPort = new OutCode();
        OutCode v_outcode = new OutCode(v);
        OutCode u_outcode = new OutCode(u);

        if ((v_outcode == inViewPort) && (u_outcode == inViewPort))
            return true;

        if ((v_outcode * u_outcode) != inViewPort)
            return false;


        if (v_outcode == inViewPort) return LineClip(ref u, ref v);

        if (v_outcode.Up)
        {
            v = Intercept(u, v, 0);
            OutCode v2_Outcode = new OutCode(v);

            if (v2_Outcode == inViewPort) return LineClip(ref u, ref v);
        }
        if (v_outcode.Down)
        {
            v = Intercept(u, v, 1);
            OutCode v2_Outcode = new OutCode(v);

            if (v2_Outcode == inViewPort) return LineClip(ref u, ref v);
        }
        if (v_outcode.Left)
        {
            v = Intercept(u, v, 2);
            OutCode v2_Outcode = new OutCode(v);

            if (v2_Outcode == inViewPort) return LineClip(ref u, ref v);
        }
        if (v_outcode.Right)
        {
            v = Intercept(u, v, 3);
            OutCode v2_Outcode = new OutCode(v);

            if (v2_Outcode == inViewPort) return LineClip(ref u, ref v);
        }
        return false;
    }


    Vector2 Intercept(Vector2 u, Vector2 v, int edge)
    {
        float m = (v.y - u.y) / (v.x - u.x);


        if (edge == 0)
            return new Vector2(u.x + (1 / m) * (1 - u.y), 1);

        if (edge == 1)
            return new Vector2(u.x + (1 / m) * (-1 - u.y), -1);

        if (edge == 2)
            return new Vector2(-1, u.y + m * (-1 - u.x));

        return new Vector2(1, u.y + m * (1 - u.x));
    }


    List<Vector2> Bresenham(Vector2 start, Vector2 finish)
    {
        float dx = finish.x - start.x;
        if (dx < 0)
            return Bresenham(finish, start);

        float dy = finish.y - start.y;
        if (dy < 0) //Negative slope
            return NegativeY(Bresenham(NegativeY(start), NegativeY(finish)));

        if (dy > dx) //Slope > 1
            return SwapXY(Bresenham(SwapXY(start), SwapXY(finish)));

        //Bresenham algorithm
        List<Vector2> pixels = new List<Vector2>();
        Vector2 line = new Vector2((int)start.x, start.y);
        float slope = ((finish.y - start.y) / (finish.x - start.x));
        float y = start.y;
        float x = start.x;
        while (line.x <= finish.x)
        {
            pixels.Add(line);
            line.x = (int)x++;
            line.y = (int)Math.Round(y += slope);
        }
        return pixels;
    }

    Vector2 NegativeY(Vector2 vector)
    {
        return new Vector2(vector.x, -vector.y);
    }

    List<Vector2> NegativeY(List<Vector2> list)
    {
        List<Vector2> newList = new List<Vector2>();
        foreach (Vector2 vector in list)
            newList.Add(NegativeY(vector));
        return newList;
    }

    List<Vector2> SwapXY(List<Vector2> list)
    {
        List<Vector2> newList = new List<Vector2>();
        foreach (Vector2 vector in list)
            newList.Add(SwapXY(vector));
        return newList;
    }

    Vector2 SwapXY(Vector2 vector)
    {
        return new Vector2(vector.y, vector.x);
    }

    private Vector3[] MatrixTransform(
        Vector3[] meshVertices,
        Matrix4x4 transformMatrix)
    {
        Vector3[] output = new Vector3[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
            output[i] = transformMatrix *
                new Vector4(
                meshVertices[i].x,
                meshVertices[i].y,
                meshVertices[i].z,
                    1);

        return output;
    }

    // Takes the vertices after projection and draws the cube
    private void DrawCube(Vector3[] verts)
    {
        Destroy(screen);
        screen = new Texture2D(resX, resY);
        GetComponent<Renderer>().material.mainTexture = screen;

        Vector2[] verts2d = DivideByZ(verts);

        //Cube lines
        DrawLine(verts2d[0], verts2d[1]);
        DrawLine(verts2d[1], verts2d[2]);
        DrawLine(verts2d[2], verts2d[3]);
        DrawLine(verts2d[0], verts2d[3]);
        DrawLine(verts2d[0], verts2d[4]);
        DrawLine(verts2d[1], verts2d[5]);
        DrawLine(verts2d[2], verts2d[6]);
        DrawLine(verts2d[3], verts2d[7]);
        DrawLine(verts2d[4], verts2d[5]);
        DrawLine(verts2d[5], verts2d[6]);
        DrawLine(verts2d[6], verts2d[7]);
        DrawLine(verts2d[7], verts2d[4]);
        screen.Apply();
    }

    void FloodFill(Vector2 vector)
    {
        if (screen.GetPixel((int)vector.x, (int)vector.y) != newColor)
        {
            screen.SetPixel((int)vector.x, (int)vector.y, newColor);
            FloodFill(new Vector2(vector.x + 1, vector.y));
            FloodFill(new Vector2(vector.x, vector.y + 1));
            FloodFill(new Vector2(vector.x - 1, vector.y));
            FloodFill(new Vector2(vector.x, vector.y - 1));
        }
    }

    // Draws the line segment from two unclipped 2d points
    private void DrawLine(Vector2 start, Vector2 finish)
    {
        Vector2 u = start;
        Vector2 v = finish;

        if (LineClip(ref u, ref v))
            Rasterise(u, v);
    }

    private void Rasterise(Vector2 a, Vector2 b)
    {
        List<Vector2> pixels = Bresenham(ConvertToPixelSpace(a), ConvertToPixelSpace(b));
        foreach (Vector2 pixel in pixels)
            screen.SetPixel((int)pixel.x, (int)pixel.y, newColor);
    }

    private Vector2 ConvertToPixelSpace(Vector2 a)
    {
        return new Vector2((int)((a.x + 1) * (resX - 1) / 2.0f), (int)((1 - a.y) * (resY - 1) / 2.0f));
    }

    private Vector2[] DivideByZ(Vector3[] verts)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (Vector3 v in verts)
            output.Add(new Vector2(-v.x / v.z, -v.y / v.z));

        return output.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        myAngle = myAngle + 1;
        Matrix4x4 persp = Matrix4x4.Perspective(45, (float)1.6, 1, 1000);

        Matrix4x4 viewing = ViewingMatrix(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        Matrix4x4 world = RotationMatrix(myAngle, new Vector3(0, 1.5f, 0));

        Matrix4x4 overall = persp * viewing * world;

        DrawCube(MatrixTransform(cube, overall));
    }
}