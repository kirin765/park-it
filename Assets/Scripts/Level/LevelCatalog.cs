using UnityEngine;

namespace Parking
{
    public static class LevelCatalog
    {
        public static readonly Color32 PlayerBody = Pal.C("#e2554c");

        static readonly Color32[] Parked =
        {
            Pal.C("#4f8fd6"), Pal.C("#e8bf4a"), Pal.C("#5cc0a8"), Pal.C("#9b6fd0"),
            Pal.C("#e08a45"), Pal.C("#6aa84f"), Pal.C("#d8d8d8"), Pal.C("#d878a0"),
            Pal.C("#7c8794"), Pal.C("#c9d24b")
        };

        public static int Count => 6;

        public static Color32 ColorFor(int i) => Parked[Mathf.Abs(i) % Parked.Length];

        public static LevelDef Get(int index)
        {
            switch (index)
            {
                case 0: return FirstBay();
                case 1: return TJunction();
                case 2: return ParallelStreet();
                case 3: return AngledSqueeze();
                case 4: return TightGarage();
                default: return ReverseMaster();
            }
        }

        static void Perimeter(LevelDef d, float x, float y, float w, float h, float t = 0.4f)
        {
            d.Walls.Add(new WallDef(0f, -h / 2f + y - t / 2f, w + t * 2f, t));
            d.Walls.Add(new WallDef(0f, h / 2f + y + t / 2f, w + t * 2f, t));
            d.Walls.Add(new WallDef(-w / 2f + x - t / 2f, y, t, h));
            d.Walls.Add(new WallDef(w / 2f + x + t / 2f, y, t, h));
        }

        // ---------------------------------------------------------------- 1
        static LevelDef FirstBay()
        {
            var d = new LevelDef
            {
                Name = "FIRST BAY",
                Short = "BAY ONE",
                Tag = "PERPENDICULAR",
                Par = 24f,
                Instruction = "PULL INTO THE GLOWING BAY"
            };
            d.Asphalt.Add(new RectDef(0f, -0.2f, 10.6f, 17.2f));
            Perimeter(d, 0f, -0.2f, 10.6f, 17.2f);

            float[] ys = { -5.2f, -2.6f, 0f, 2.6f, 5.2f };
            for (int i = 0; i < ys.Length; i++)
            {
                d.Bays.Add(new BayDef(3.78f, ys[i], 90f, 2.6f, 3.0f, i + 1));
                d.Bays.Add(new BayDef(-3.78f, ys[i], -90f, 2.6f, 3.0f, i + 6));
            }

            d.Cars.Add(new CarDef(3.78f, -5.2f, 90f, ColorFor(0)));
            d.Cars.Add(new CarDef(3.78f, 2.6f, 90f, ColorFor(1)));
            d.Cars.Add(new CarDef(-3.78f, -2.6f, -90f, ColorFor(4)));
            d.Cars.Add(new CarDef(-3.78f, 5.2f, -90f, ColorFor(3)));

            d.SlotCenter = new Vector2(3.78f, 0f);
            d.SlotAng = 90f;
            d.SlotSize = new Vector2(1.85f, 2.72f);
            d.Start = new Vector2(0f, -5.85f);
            d.Props.Add(new PropDef(-5.0f, 7.6f, PropKind.Tree));
            d.Props.Add(new PropDef(5.0f, -7.9f, PropKind.Tree));
            return d;
        }

        // ---------------------------------------------------------------- 2
        static LevelDef TJunction()
        {
            var d = new LevelDef
            {
                Name = "T-JUNCTION",
                Short = "T-SHAPE",
                Tag = "T PARKING",
                Par = 32f,
                Instruction = "UP, LEFT, THEN INTO THE BAY"
            };
            d.Asphalt.Add(new RectDef(0f, -4.25f, 4.4f, 9.1f));
            d.Asphalt.Add(new RectDef(0f, 2.4f, 10.8f, 4.2f));

            d.Planters.Add(new RectDef(-3.8f, -4.25f, 3.2f, 9.1f));
            d.Planters.Add(new RectDef(3.8f, -4.25f, 3.2f, 9.1f));

            d.Walls.Add(new WallDef(0f, -8.96f, 11.2f, 0.4f));
            d.Walls.Add(new WallDef(0f, 7.6f, 11.2f, 0.4f));
            d.Walls.Add(new WallDef(-5.46f, -0.4f, 0.32f, 17.4f));
            d.Walls.Add(new WallDef(5.46f, -0.4f, 0.32f, 17.4f));

            float[] xs = { -3.95f, -1.32f, 1.32f, 3.95f };
            for (int i = 0; i < xs.Length; i++)
                d.Bays.Add(new BayDef(xs[i], 5.95f, 0f, 2.6f, 2.9f, i + 1));

            d.Cars.Add(new CarDef(-1.32f, 5.95f, 0f, ColorFor(2)));
            d.Cars.Add(new CarDef(1.32f, 5.95f, 0f, ColorFor(6)));
            d.Cars.Add(new CarDef(3.95f, 5.95f, 0f, ColorFor(0)));

            d.SlotCenter = new Vector2(-3.95f, 5.95f);
            d.SlotAng = 0f;
            d.SlotSize = new Vector2(1.82f, 2.7f);
            d.Start = new Vector2(0f, -5.85f);
            d.Props.Add(new PropDef(-3.9f, -5.7f, PropKind.Tree));
            d.Props.Add(new PropDef(-3.9f, -1.6f, PropKind.Tree, 0f, 0.9f));
            d.Props.Add(new PropDef(3.9f, -6.2f, PropKind.Tree, 0f, 1.1f));
            d.Props.Add(new PropDef(3.9f, -2.6f, PropKind.Bush));
            d.Props.Add(new PropDef(-3.9f, -7.8f, PropKind.Bush));
            return d;
        }

        // ---------------------------------------------------------------- 3
        static LevelDef ParallelStreet()
        {
            var d = new LevelDef
            {
                Name = "PARALLEL STREET",
                Short = "PARALLEL",
                Tag = "SIDEWAYS",
                Par = 36f,
                Hint = "PULL PAST, THEN BACK IN",
                Instruction = "REVERSE INTO THE GAP"
            };
            d.Asphalt.Add(new RectDef(0.25f, -0.2f, 6.5f, 17.2f));
            d.Buildings.Add(new RectDef(-4.3f, -0.2f, 2.6f, 17.2f));
            d.Buildings.Add(new RectDef(4.55f, -0.2f, 2.1f, 17.2f));

            d.Walls.Add(new WallDef(0.25f, -8.96f, 6.9f, 0.4f));
            d.Walls.Add(new WallDef(0.25f, 8.56f, 6.9f, 0.4f));
            d.Walls.Add(new WallDef(-3.0f, -0.2f, 0.22f, 17.4f));
            d.Walls.Add(new WallDef(3.5f, -0.2f, 0.22f, 17.4f));

            d.Cars.Add(new CarDef(2.52f, 1.9f, 0f, ColorFor(6)));
            d.Cars.Add(new CarDef(2.52f, -4.35f, 0f, ColorFor(0)));
            d.Cars.Add(new CarDef(-2.28f, 6.4f, 180f, ColorFor(2)));
            d.Cars.Add(new CarDef(-2.28f, -6.6f, 180f, ColorFor(1)));

            d.SlotCenter = new Vector2(2.48f, -1.225f);
            d.SlotAng = 0f;
            d.SlotSize = new Vector2(1.78f, 3.45f);
            d.Start = new Vector2(0.35f, -5.85f);

            d.Props.Add(new PropDef(4.4f, 4.8f, PropKind.Hydrant));
            d.Props.Add(new PropDef(4.5f, -3.4f, PropKind.Bin));
            d.Props.Add(new PropDef(-4.2f, 3.4f, PropKind.Tree, 0f, 0.9f));
            d.Props.Add(new PropDef(-4.2f, -4.2f, PropKind.Tree, 0f, 1.0f));
            d.Props.Add(new PropDef(4.6f, 7.4f, PropKind.Tree, 0f, 0.85f));
            return d;
        }

        // ---------------------------------------------------------------- 4
        static LevelDef AngledSqueeze()
        {
            var d = new LevelDef
            {
                Name = "ANGLED SQUEEZE",
                Short = "ANGLED",
                Tag = "45 DEGREES",
                Par = 34f,
                Instruction = "TUCK INTO THE ANGLED BAY"
            };
            d.Asphalt.Add(new RectDef(0f, -0.2f, 10.6f, 17.2f));
            Perimeter(d, 0f, -0.2f, 10.6f, 17.2f);

            float[] ys = { -4.9f, -2.3f, 0.3f, 2.9f, 5.5f };
            for (int i = 0; i < ys.Length; i++)
            {
                d.Bays.Add(new BayDef(3.35f, ys[i], 45f, 2.55f, 2.9f, i + 1));
                d.Bays.Add(new BayDef(-3.35f, ys[i], -45f, 2.55f, 2.9f, i + 6));
            }

            d.Cars.Add(new CarDef(3.35f, -4.9f, 45f, ColorFor(0)));
            d.Cars.Add(new CarDef(3.35f, -2.3f, 45f, ColorFor(2)));
            d.Cars.Add(new CarDef(3.35f, 2.9f, 45f, ColorFor(6)));
            d.Cars.Add(new CarDef(3.35f, 5.5f, 45f, ColorFor(4)));
            d.Cars.Add(new CarDef(-3.35f, -4.9f, -45f, ColorFor(1)));
            d.Cars.Add(new CarDef(-3.35f, 0.3f, -45f, ColorFor(3)));
            d.Cars.Add(new CarDef(-3.35f, 5.5f, -45f, ColorFor(7)));

            d.SlotCenter = new Vector2(3.35f, 0.3f);
            d.SlotAng = 45f;
            d.SlotSize = new Vector2(1.85f, 2.7f);
            d.Start = new Vector2(0f, -5.85f);
            d.GroundWord = "45";
            d.GroundWordPos = new Vector2(0f, -6.2f);
            d.GroundWordScale = 2.2f;
            return d;
        }

        // ---------------------------------------------------------------- 5
        static LevelDef TightGarage()
        {
            var d = new LevelDef
            {
                Name = "TIGHT GARAGE",
                Short = "GARAGE",
                Tag = "INDOOR",
                Par = 44f,
                Hint = "MIND THE PILLARS",
                Instruction = "WEAVE UP TO THE TOP BAY"
            };
            d.Ground = GroundStyle.Concrete;
            Perimeter(d, 0f, -0.2f, 10.4f, 17.2f);

            float[] ys = { -4.9f, -2.0f, 0.9f, 3.8f };
            for (int i = 0; i < ys.Length; i++)
            {
                d.Bays.Add(new BayDef(3.8f, ys[i], 90f, 2.85f, 2.9f, i + 1));
                d.Bays.Add(new BayDef(-3.8f, ys[i], -90f, 2.85f, 2.9f, i + 6));
            }

            d.Cars.Add(new CarDef(3.8f, -4.9f, 90f, ColorFor(0)));
            d.Cars.Add(new CarDef(3.8f, -2.0f, 90f, ColorFor(6)));
            d.Cars.Add(new CarDef(3.8f, 0.9f, 90f, ColorFor(1)));
            d.Cars.Add(new CarDef(-3.8f, -4.9f, -90f, ColorFor(2)));
            d.Cars.Add(new CarDef(-3.8f, 0.9f, -90f, ColorFor(3)));
            d.Cars.Add(new CarDef(-3.8f, 3.8f, -90f, ColorFor(8)));

            d.Props.Add(new PropDef(0f, -3.9f, PropKind.Pillar));
            d.Props.Add(new PropDef(0f, -0.9f, PropKind.Pillar));
            d.Props.Add(new PropDef(0f, 2.6f, PropKind.Pillar));
            d.Props.Add(new PropDef(-2.4f, 6.6f, PropKind.Sign, 0f, 1f));
            d.Props.Add(new PropDef(4.9f, -7.4f, PropKind.Cone));

            d.SlotCenter = new Vector2(3.8f, 3.8f);
            d.SlotAng = 90f;
            d.SlotSize = new Vector2(1.8f, 2.7f);
            d.Start = new Vector2(0f, -5.85f);
            return d;
        }

        // ---------------------------------------------------------------- 6
        static LevelDef ReverseMaster()
        {
            var d = new LevelDef
            {
                Name = "REVERSE MASTER",
                Short = "REVERSE",
                Tag = "TIGHT FIT",
                Par = 40f,
                Hint = "NOSE OUT, TAIL IN",
                Instruction = "BACK INTO THE TIGHT BAY"
            };
            d.Asphalt.Add(new RectDef(0f, -0.2f, 9.0f, 17.2f));
            Perimeter(d, 0f, -0.2f, 9.0f, 17.2f);

            float[] ys = { -5.2f, -2.3f, 0.6f, 3.5f };
            for (int i = 0; i < ys.Length; i++)
            {
                d.Bays.Add(new BayDef(3.05f, ys[i], 90f, 2.9f, 2.85f, i + 1));
                d.Bays.Add(new BayDef(-3.05f, ys[i], -90f, 2.9f, 2.85f, i + 6));
            }

            d.Cars.Add(new CarDef(3.05f, -5.2f, 90f, ColorFor(1)));
            d.Cars.Add(new CarDef(3.05f, -2.3f, 90f, ColorFor(4)));
            d.Cars.Add(new CarDef(3.05f, 0.6f, 90f, ColorFor(0)));
            d.Cars.Add(new CarDef(3.05f, 3.5f, 90f, ColorFor(5)));
            d.Cars.Add(new CarDef(-3.05f, -5.2f, -90f, ColorFor(9)));
            d.Cars.Add(new CarDef(-3.05f, -2.3f, -90f, ColorFor(7)));
            d.Cars.Add(new CarDef(-3.05f, 3.5f, -90f, ColorFor(2)));

            d.SlotCenter = new Vector2(-3.05f, 0.6f);
            d.SlotAng = -90f;
            d.SlotSize = new Vector2(1.8f, 2.66f);
            d.Start = new Vector2(0f, -5.85f);
            d.Props.Add(new PropDef(1.2f, -6.4f, PropKind.Cone));
            d.Props.Add(new PropDef(-1.2f, -6.7f, PropKind.Cone));
            d.Props.Add(new PropDef(4.1f, -7.8f, PropKind.Bin));
            return d;
        }
    }
}
