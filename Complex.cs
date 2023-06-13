namespace A25;

public struct Complex {
   public Complex (double x, double y) => (X, Y) = (x, y);
   public override string ToString () => $"{X} + i{Y}";

   public double Norm => Math.Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public readonly static Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);

   public readonly double X, Y;
}
