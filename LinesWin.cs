// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GrayBMP;

class LinesWin : Window {
   public LinesWin () {
      Width = 800; Height = 600;
      Left = 200; Top = 50;
      WindowStyle = WindowStyle.None;
      mBmp = new GrayBMP (Width, Height);

      Image image = new () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
      Content = image;
      mDX = mBmp.Width; mDY = mBmp.Height;

      // Start a timer to repaint a new frame every 33 milliseconds
      DispatcherTimer timer = new () {
         Interval = TimeSpan.FromMilliseconds (100), IsEnabled = true,
      };
      timer.Tick += NextFrame2;
   }
   readonly GrayBMP mBmp;
   readonly int mDX, mDY;

   void NextFrame (object sender, EventArgs e) {
      mBmp.Begin ();
      mBmp.Clear (0);
      for (int i = mStart; i < mStart + 360; i += 10) {
         double a = i * Math.PI / 180;
         int x0 = mDX / 2, y0 = mDY / 2;
         int x1 = (int)(x0 + Math.Cos (a) * 290 + 0.5);
         int y1 = (int)(y0 + Math.Sin (a) * 290 + 0.5);
         mBmp.DrawLine (x0, y0, x1, y1, 255);
      }
      mBmp.End ();
      mStart++;
   }
   int mStart;

   void NextFrame1 (object sender, EventArgs e) {
      using (var bt = new BlockTimer ("Clear")) {
         mBmp.Begin ();
         int gray = R.Next (256);
         for (int i = 0; i < 1000; i++)
            mBmp.Clear (gray);
         mBmp.End ();
      }
   }
   Random R = new ();

   void NextFrame2 (object sender, EventArgs e) {
      using (var bt = new BlockTimer ("Lines")) {
         mBmp.Begin ();
         mBmp.Clear (0);
         for (int i = 0; i < 100000; i++) {
            int x0 = R.Next (mDX), y0 = R.Next (mDY),
                x1 = R.Next (mDX), y1 = R.Next (mDY),
                color = R.Next (256);
            mBmp.DrawLine (x0, y0, x1, y1, color);
         }
         mBmp.End ();
      }
   }
}

class BlockTimer : IDisposable {
   public BlockTimer (string message) {
      mStart = DateTime.Now;
      mMessage = message;
   }
   readonly DateTime mStart;
   readonly string mMessage;

   public void Dispose () {
      int elapsed = (int)((DateTime.Now - mStart).TotalMilliseconds + 0.5);
      Console.WriteLine ($"{mMessage}: {elapsed}ms");
   }
}