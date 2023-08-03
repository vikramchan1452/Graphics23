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

/// <summary>A Vector2 in 2D space</summary>
readonly record struct Vector2 (double X, double Y) {
   /// <summary>Length of the vector</summary>
   public double Length => Sqrt (X * X + Y * Y);

   public double Dot (Vector2 b) => X * b.X + Y * b.Y;

   public double ZCross (Vector2 b) => X * b.Y - b.X * Y;

   public static Vector2 operator + (Vector2 a, Vector2 b) => new (a.X + b.X, a.Y + b.Y);
   public static Vector2 operator * (Vector2 a, double f) => new (a.X * f, a.Y * f);
   public static Vector2 operator - (Vector2 a) => new (-a.X, -a.Y);
}

class Matrix2 {
   public Matrix2 (double m11, double m12, double m21, double m22, double dx, double dy)
      => (M11, M12, M21, M22, DX, DY) = (m11, m12, m21, m22, dx, dy);

   public static Matrix2 Translation (Vector2 v)
      => new (1, 0, 0, 1, v.X, v.Y);
   public static Matrix2 Scaling (double f)
      => new (f, 0, 0, f, 0, 0);
   public static Matrix2 Rotation (double theta) {
      var (s, c) = (Sin (theta), Cos (theta));
      return new (c, s, -s, c, 0, 0);
   }

   public static Point2 operator * (Point2 p, Matrix2 m)
      => new (p.X * m.M11 + p.Y * m.M21 + m.DX, p.X * m.M12 + p.Y * m.M22 + m.DY);

   public static Matrix2 operator * (Matrix2 a, Matrix2 b)
      => new (a.M11 * b.M11 + a.M12 * b.M21, a.M11 * b.M12 + a.M12 * b.M22,
              a.M21 * b.M11 + a.M22 * b.M21, a.M21 * b.M12 + a.M22 * b.M22,
              a.DX * b.M11 + a.DY * b.M21 + b.DX, a.DX * b.M12 + a.DY * b.M22 + b.DY);

   public readonly double M11, M12, M21, M22, DX, DY;
}

/// <summary>Represents a bounding box in 2 dimensions</summary>
readonly struct Bound2 {
   /// <summary>Compute the bound of a set of points</summary>
   public Bound2 (IEnumerable<Point2> pts) {
      X0 = Y0 = double.MaxValue; X1 = Y1 = double.MinValue;
      foreach (var (x, y) in pts) {
         X0 = Min (X0, x); Y0 = Min (Y0, y);
         X1 = Max (X1, x); Y1 = Max (Y1, y);
      }
   }

   public override string ToString ()
      => $"{Round (X0, 3)},{Round (Y0, 3)} to {Round (X1, 3)},{Round (Y1, 3)}";

   /// <summary>Compute the overall bound of a set of bounds (union)</summary>
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

   /// <summary>The bound of the polygon</summary>
   public Bound2 Bound {
      get {
         if (mBound.IsEmpty) mBound = new Bound2 (mPts);
         return mBound;
      }
   }
   Bound2 mBound;

   public static Polygon operator * (Polygon p, Matrix2 m)
      => new Polygon (p.Pts.Select (a => a * m));

   /// <summary>Enumerate all the 'lines' in this Polygon</summary>
   public IEnumerable<(Point2 A, Point2 B)> EnumLines (Matrix2 xfm) {
      Point2 p0 = mPts[^1] * xfm;
      for (int i = 0, n = mPts.Length; i < n; i++) {
         Point2 p1 = mPts[i] * xfm;
         yield return (p0, p1);
         p0 = p1;
      }
   }

}

/// <summary>A drawing is a collection of polygons</summary>
class Drawing {
   public void Add (Polygon poly) {
      mPolys.Add (poly);
      if (mConvexEnvelope is not null) 
         GetEnvelope(mConvexEnvelope.Concat(poly.Pts));
      mBound = new ();
   }

   public IReadOnlyList<Polygon> Polys => mPolys;
   List<Polygon> mPolys = new ();

   public static Drawing operator * (Drawing d, Matrix2 m) {
      Drawing d2 = new Drawing ();
      foreach (var p in d.Polys) d2.Add (p * m);
      return d2;
   }

   public Bound2 Bound {
      get {
         if (mBound.IsEmpty) mBound = new (ConvexHull);
         return mBound;
      }
   }
   Bound2 mBound;

   public Bound2 GetBound (Matrix2 xfm)
      => new (ConvexHull.Select (a => a * xfm));

   /// <summary>Enumerate all the lines in this drawing</summary>
   public IEnumerable<(Point2 A, Point2 B)> EnumLines (Matrix2 xfm) {
      //return mPolys.SelectMany (a => a.EnumLines (xfm));
      var Hull = ConvexHull;
      var p0 = Hull[^1] * xfm;
      foreach (var p in Hull) {
         var p1 = p * xfm;
         yield return (p0, p1);
         p0 = p1;
      }
   }

   public IReadOnlyList<Point2> ConvexHull {
      get {
         if (mConvexEnvelope is null) GetEnvelope (Polys.SelectMany(a => a.Pts));
         return mConvexEnvelope;
      }
   }
   List<Point2> mConvexEnvelope = null;

   void GetEnvelope(IEnumerable<Point2> pts) {
      var bottomPt = pts.MinBy (p => p.Y);
      var spts = pts.OrderBy (a => a.AngleTo (bottomPt)).ToList ();
      var HullPts = new Stack<Point2> ();
      HullPts.Push (bottomPt);
      HullPts.Push (spts[0]);
      //HullPts.Push (spts[1]);
      var (x1, y1) = (bottomPt.X, bottomPt.Y);
      for (int i = 1; i < pts.Count(); i++) {
         var (x2, y2) = (HullPts.Peek().X, HullPts.Peek().Y);
         var (x3, y3) = (spts[i].X, spts[i].Y);
         if (((x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1)) > 0) HullPts.Push (new Point2 (x3, y3));
         else HullPts.Pop ();
      }
      mConvexEnvelope= HullPts.ToList ();
   }
}
