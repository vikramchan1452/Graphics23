// Geometry.cs - Contains some basic Geometry structs (Complex numbers, Points, Vectors)
// ---------------------------------------------------------------------------------------
using static System.Math;
namespace GrayBMP;

/// <summary>A number in the complex plane of the form (X + iY)</summary>
readonly record struct Complex (double X, double Y) {
   public double Norm => Math.Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public static readonly Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);
}

/// <summary>A point in 2D space, with double-precision coordinates (X, Y)</summary>
readonly record struct Point2 (double X, double Y) {
   public (int X, int Y) Round () => ((int)(X + 0.5), (int)(Y + 0.5));

   public double AngleTo (Point2 b) => Math.Atan2 (b.Y - Y, b.X - X);
   public Point2 RadialMove (double r, double th) => new (X + r * Cos (th), Y + r * Sin (th));

   public static Vector2 operator - (Point2 a, Point2 b) => new (a.X - b.X, a.Y - b.Y);
   public static Point2 operator + (Point2 p, Vector2 v) => new (p.X + v.X, p.Y + v.Y);
}

readonly record struct Vector2 (double X, double Y) {
   /// <summary>Length of the vector</summary>
   public double Length => Sqrt (X * X + Y * Y);
}

/// <summary>Represents a bounding box in 2 dimensions</summary>
readonly struct Bound2 {
   public Bound2 (IEnumerable<Point2> pts) {
      X0 = Y0 = double.MaxValue; X1 = Y1 = double.MinValue;
      foreach (var (x, y) in pts) {
         X0 = Min (X0, x); Y0 = Min (Y0, y);
         X1 = Max (X1, x); Y1 = Max (Y1, y);
      }
   }

   public override string ToString ()
      => $"{Round (X0, 3)},{Round (Y0, 3)} to {Round (X1, 3)},{Round (Y1, 3)}";

   public Bound2 (IEnumerable<Bound2> bounds) {
      X0 = Y0 = double.MaxValue; X1 = Y1 = double.MinValue;
      foreach (var b in bounds) {
         X0 = Min (X0, b.X0); Y0 = Min (Y0, b.Y0);
         X1 = Max (X1, b.X1); Y1 = Max (Y1, b.Y1);
      }
   }

   public double Width => X1 - X0;
   public double Height => Y1 - Y0;
   public Point2 Midpoint => new ((X0 + X1) / 2, (Y0 + Y1) / 2);

   public bool IsEmpty => X0 >= X1;
   public readonly double X0, Y0, X1, Y1;
}

/// <summary>A Polygon is a set of points making a closed shape</summary>
class Polygon {
   public Polygon (IEnumerable<Point2> pts) => mPts = pts.ToArray ();

   public IReadOnlyList<Point2> Pts => mPts;
   readonly Point2[] mPts;

   public Bound2 Bound {
      get {
         if (mBound.IsEmpty) mBound = new Bound2 (mPts);
         return mBound;
      }
   }
   Bound2 mBound;

   public IEnumerable<(Point2 A, Point2 B)> Lines {
      get {
         for (int i = 0, n = mPts.Length; i < n; i++)
            yield return (mPts[i], mPts[(i + 1) % n]);
      }
   }
}

/// <summary>A drawing is a collection of polygons</summary>
class Drawing {
   public void Add (Polygon poly) {
      mPolys.Add (poly); 
      mBound = new (); 
   }
   public IReadOnlyList<Polygon> Polys => mPolys;

   public Bound2 Bound {
      get {
         if (mBound.IsEmpty) mBound = new (Polys.Select (a => a.Bound));
         return mBound;
      }
   }
   Bound2 mBound;

   List<Polygon> mPolys = new ();
}
