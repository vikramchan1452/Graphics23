// Geometry.cs - Contains some basic Geometry structs (Complex numbers, Points, Vectors)
// ---------------------------------------------------------------------------------------
namespace GrayBMP;

/// <summary>A number in the complex plane of the form (X + iY)</summary>
readonly struct Complex {
   public Complex (double x, double y) => (X, Y) = (x, y);
   public override string ToString () => $"{X} + i{Y}";

   public double Norm => Math.Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public static readonly Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);

   public readonly double X, Y;
}

/// <summary>A point in 2D space, with double-precision coordinates (X, Y)</summary>
readonly struct Point2 {
   public Point2 (double x, double y) => (X, Y) = (x, y);
   public (int X, int Y) Round () => ((int)(X + 0.5), (int)(Y + 0.5));
   public override string ToString () => $"({Math.Round (X, 6)}, {Math.Round (Y, 6)})";

   public static Vector2 operator - (Point2 a, Point2 b)
      => new (a.X - b.X, a.Y - b.Y);
   public static Point2 operator + (Point2 a, Vector2 b)
      => new (a.X + b.X, a.Y + b.Y);

   public readonly double X, Y;
}

/// <summary>A vector in 2D space</summary>
readonly struct Vector2 {
   public Vector2 (double x, double y) => (X, Y) = (x, y);
   public double Length => Math.Sqrt (X * X + Y * Y);

   public Vector2 Normalized () {
      double len = Length;
      return new Vector2 (X / len, Y / len);
   }

   public static Vector2 operator - (Vector2 a)
      => new (-a.X, -a.Y);
   public static Vector2 operator + (Vector2 a, Vector2 b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Vector2 operator * (Vector2 a, double f)
      => new (a.X * f, a.Y * f);

   public readonly double X, Y;
}

/// <summary>A Line in 2 dimensions (A -> B)</summary>
readonly struct Line {
   public Line (Point2 a, Point2 b) => (A, B) = (a, b);
   public override string ToString () => $"{A} -> {B}";

   public readonly Point2 A, B;
}

/// <summary>A drawing is a collection of lines</summary>
class Drawing {
   public void AddLine (Line line) => mLines.Add (line);

   public IReadOnlyList<Line> Lines => mLines;
   List<Line> mLines = new ();
}

/// <summary>A rigid-body transformation matrix in 2D</summary>
class Matrix2 {
   public Matrix2 (double m11, double m12, double m21, double m22, double dx, double dy)
      => (M11, M12, M21, M22, DX, DY) = (m11, m12, m21, m22, dx, dy);

   public static readonly Matrix2 Identity = new (1, 0, 0, 1, 0, 0);

   public static Matrix2 Rotation (double angle) {
      var (c, s) = (Math.Cos (angle), Math.Sin (angle));
      return new Matrix2 (c, s, -s, c, 0, 0);
   }

   public static Matrix2 Scaling (double s)
      => new (s, 0, 0, s, 0, 0);

   public static Matrix2 Translation (double dx, double dy) 
      => new (1, 0, 0, 1, dx, dy);

   public static Point2 operator * (Point2 p, Matrix2 m) 
      => new (p.X * m.M11 + p.Y * m.M21 + m.DX, p.X * m.M12 + p.Y * m.M22 + m.DY);

   public static Vector2 operator * (Vector2 p, Matrix2 m)
      => new (p.X * m.M11 + p.Y * m.M21, p.X * m.M12 + p.Y * m.M22);

   public static Matrix2 operator * (Matrix2 a, Matrix2 b)
      => new (a.M11 * b.M11 + a.M12 * b.M21, a.M11 * b.M12 + a.M12 * b.M22,
              a.M21 * b.M11 + a.M22 * b.M21, a.M21 * b.M12 + a.M22 * b.M22,
              a.DX * b.M11 + a.DY * b.M21 + b.DX, a.DX * b.M12 + a.DY * b.M22 + b.DY);

   public readonly double M11, M12, M21, M22, DX, DY;
}
