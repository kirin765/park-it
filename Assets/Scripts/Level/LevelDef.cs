using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    public enum GroundStyle { Asphalt, Concrete }
    public enum WallStyle { Curb, Barrier, Metal }
    public enum PropKind { Cone, Tree, Bush, Pillar, Hydrant, Bin, Sign, Light }

    public class RectDef
    {
        public float x, y, w, h, ang;
        public RectDef(float x, float y, float w, float h, float ang = 0f)
        { this.x = x; this.y = y; this.w = w; this.h = h; this.ang = ang; }
        public Vector2 Pos => new Vector2(x, y);
        public Vector2 Size => new Vector2(w, h);
    }

    public class WallDef : RectDef
    {
        public WallStyle style;
        public WallDef(float x, float y, float w, float h, WallStyle style = WallStyle.Curb) : base(x, y, w, h)
        { this.style = style; }
    }

    public class BayDef
    {
        public float x, y, ang;
        public float w = 2.5f, d = 3f;
        public int number;
        public BayDef(float x, float y, float ang, float w = 2.5f, float d = 3f, int number = 0)
        { this.x = x; this.y = y; this.ang = ang; this.w = w; this.d = d; this.number = number; }
    }

    public class CarDef
    {
        public float x, y, ang;
        public Color32 body;
        public CarDef(float x, float y, float ang, Color32 body)
        { this.x = x; this.y = y; this.ang = ang; this.body = body; }
    }

    public class PropDef
    {
        public float x, y, ang, scale;
        public PropKind kind;
        public PropDef(float x, float y, PropKind kind, float ang = 0f, float scale = 1f)
        { this.x = x; this.y = y; this.kind = kind; this.ang = ang; this.scale = scale; }
    }

    public class LevelDef
    {
        public string Name = "";
        public string Short = "";
        public string Tag = "";
        public string Hint = "";
        public string Instruction = "PARK IN THE GLOWING BAY";
        public float Par = 30f;
        public GroundStyle Ground = GroundStyle.Asphalt;

        public readonly List<RectDef> Asphalt = new List<RectDef>();
        public readonly List<RectDef> Planters = new List<RectDef>();
        public readonly List<RectDef> Buildings = new List<RectDef>();
        public readonly List<WallDef> Walls = new List<WallDef>();
        public readonly List<BayDef> Bays = new List<BayDef>();
        public readonly List<CarDef> Cars = new List<CarDef>();
        public readonly List<PropDef> Props = new List<PropDef>();

        public Vector2 Start = new Vector2(0f, -7.2f);
        public float StartAng = 0f;

        public Vector2 SlotCenter = Vector2.zero;
        public float SlotAng = 0f;
        public Vector2 SlotSize = new Vector2(2.5f, 2.9f);

        // Decoration: big painted letter in the aisle
        public string GroundWord = "";
        public Vector2 GroundWordPos = Vector2.zero;
        public float GroundWordAng = 0f;
        public float GroundWordScale = 3f;

        public static Vector2 Heading(float angDeg)
        {
            float r = angDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(r), Mathf.Cos(r));
        }

        public static float AngleDiff(float a, float b)
        {
            float d = Mathf.Repeat(a - b, 180f);
            if (d > 90f) d = 180f - d;
            return d;
        }
    }
}
