using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;
      MouseMove += OnMouseMove;
      MouseLeftButtonDown += OnMouseLeftButtonDown;
      //DrawMandelbrot (-0.5, 0, 1);
      
   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      }
      finally {
         mBmp.Unlock ();
      }
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 256; i++) {
         if (z.NormSq > 4) return (byte)(i * 1);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            //SetPixel (x, y, 255);
            //mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         }
         finally {
            mBmp.Unlock ();
         }
      }
   }

   void OnMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
      if (mFirstClick) {
         mStartPoint = e.GetPosition (this);
         mFirstClick = false;
      }
      else {
         mEndPoint = e.GetPosition (this);
         //DrawFLine (mStartPoint.X, mStartPoint.Y, mEndPoint.X, mEndPoint.Y);
         DrawIntLine ((int)mStartPoint.X, (int)mStartPoint.Y, (int)mEndPoint.X, (int)mEndPoint.Y);
         //DrawInt2Line ((int)mStartPoint.X, (int)mStartPoint.Y, (int)mEndPoint.X, (int)mEndPoint.Y);
         mFirstClick = true;
      }
   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      }
      finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   // Drawing line using FLOAT arithmetic:
   void DrawFloatLine (double x1, double y1, double x2, double y2) {
      var Length = Point.Subtract (mStartPoint, mEndPoint).Length;
      var (dX, dY) = ((x2 - x1) / Length, (y2 - y1) / Length);
      try {
         mBmp.Lock (); mBase = mBmp.BackBuffer;
         for (int i = 0; i < Length; i++) {
            x1 += dX; y1 += dY;
            var (x, y) = ((int)x1, (int)y1);
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         }
      } finally { mBmp.Unlock (); }
   }

   // Drawing line using INTEGER arithmetic (Bresenham method):
   void DrawIntLine (int x1, int y1, int x2, int y2) {
      if (x2 < x1) (x1, y1, x2, y2) = (x2, y2, x1, y1); // We always plot the graph for increasing x.
      var (delX, delY, yInc) = (x2 - x1, y2 - y1, 1);
      if (delY < 0) (yInc, delY) = (-1, -delY);
      try {
         mBmp.Lock (); mBase = mBmp.BackBuffer;
         var (P, x, y, cond) = (2 * delY - delX, x1, y1, true);
         if (delX > delY) { // for (slope < 1)
            while (x <= x2) {
               SetPixel (x, y, 255);
               mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
               if (P >= 0) { P -= 2 * delX; y += yInc; }
               P += 2 * delY; x++;
            }
         }
         else { //for (slope >= 1)
            while (cond) {
               SetPixel (x, y, 255);
               mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
               if (P >= 0) { P -= 2 * delY; x++; }
               P += 2 * delX;
               if (yInc == 1) { cond = y <= y2; y++; }
               else { cond = y >= y2; y--; }
            }
         }
         //var (P, x, y, cond) = (2 * delY - delX, x1, y1, true);
         //while (cond) {
         //   SetPixel (x, y, 255);
         //   mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         //   if (delX > delY) { // for (slope < 1)
         //      if (P >= 0) { P -= 2 * delX; y += yInc; }
         //      P += 2 * delY; x++;
         //      cond = x <= x2;
         //   } 
         //   else { // for (slope >= 1)
         //      if (P >= 0) { P -= 2 * delY; x++; }
         //      P += 2 * delX;
         //      if (yInc == 1) { cond = y <= y2; y++; }
         //      else { cond = y >= y2; y--; }
         //   }
         //}
      } finally { mBmp.Unlock (); }
   }

   // Drawing line using INTEGER arithmetic (Optimized Bresenham method):
   void DrawInt2Line (int x0, int y0, int x1, int y1) {
      var (dx, dy) = (Math.Abs (x1 - x0), -Math.Abs (y1 - y0));
      var (sx, sy) = (x0 < x1 ? 1 : -1, y0 < y1 ? 1 : -1);
      var error = dx + dy;
      try {
         mBmp.Lock (); mBase = mBmp.BackBuffer;
         while (true) {
            SetPixel (x0, y0, 255);
            mBmp.AddDirtyRect (new Int32Rect (x0, y0, 1, 1));
            if (x0 == x1 && y0 == y1) break;
            var e2 = 2 * error;
            if (e2 >= dy) {
               if (x0 == x1) break;
               error += dy;
               x0 += sx;
            }
            else if (e2 >= dx) {
               if (y0 == y1) break;
               error += dx;
               y0 += sy;
            }
         }
      } finally { mBmp.Unlock (); }
   }

   readonly WriteableBitmap mBmp;
   readonly int mStride;
   nint mBase;
   public bool mFirstClick = true;
   public Point mStartPoint, mEndPoint;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new ();
      app.Run ();
   }
}