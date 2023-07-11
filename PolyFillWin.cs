// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
//#define SMALLBMP
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Windows.Threading;

namespace GrayBMP;

class PolyFillWin : Window {
   public PolyFillWin () {
      Width = 900; Height = 600;
      Left = 200; Top = 50; WindowStyle = WindowStyle.None;

      mBmp = new GrayBMP (Width * mScale, Height * mScale);
      Console.WriteLine ("{0} pixels", mBmp.Width * mBmp.Height);
#if SMALLBMP
      mSmallBmp = new GrayBMP (Width, Height);
#endif
      Image image = new () {
         Stretch = Stretch.Fill,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
#if SMALLBMP
         Source = mSmallBmp.Bitmap
#else
         Source = mBmp.Bitmap
#endif
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.HighQuality);
      RenderOptions.SetEdgeMode (image, EdgeMode.Unspecified);
      Content = image;

      mDwg ??= LoadDrawing ();
      // Start a timer to repaint a new frame periodically
      DispatcherTimer timer = new () {
         Interval = TimeSpan.FromMilliseconds (500), IsEnabled = true,
      };
      timer.Tick += NextFrame;
   }
   readonly GrayBMP mBmp;
#if SMALLBMP
   readonly GrayBMP mSmallBmp;
#endif
   int mScale = 1; 

   void NextFrame (object sender, EventArgs e) {
      mBmp.Begin ();
      using (new BlockTimer ("Leaf"))
         for (int i = 0; i < 1; i++)
            FillLeaf ();
      mBmp.End ();
   }

   void DrawAndFillLeaf () {
      mBmp.Begin ();
      mBmp.Clear (192);
      PolyFill pf = new ();
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         pf.AddLine (x0, y0, x1, y1);
      }
      pf.Fill (mBmp, 255);
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         mBmp.DrawLine (x0, y0, x1, y1, 0);
      }
      mBmp.End ();
   }

   void DrawLeaf () {
      mBmp.Begin ();
      mBmp.Clear (0);
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         mBmp.DrawLine (x0, y0, x1, y1, 255);
      }
      mBmp.End ();
   }

   void FillLeaf () {
      mBmp.Begin ();
      mBmp.Clear (192);
      PolyFill pf = new ();
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         pf.AddLine (x0, y0, x1, y1);
      }
      pf.Fill (mBmp, 255);

      foreach (var line in mDwg.Lines) {
         Vector2 dir = (line.B - line.A).Normalized ();
         Vector2 perp = new (-dir.Y, dir.X);
         for (int i = -mScale / 2; i <= mScale / 2; i++) {
            Point2 pa = line.A + perp * i, pb = line.B + perp * i;
            var ((x0, y0), (x1, y1)) = (pa.Round (), pb.Round ());
            mBmp.DrawLine (x0, y0, x1, y1, 0);
         }
      }
      mBmp.End ();
#if SMALLBMP
      mSmallBmp.ShrinkCopyFrom (mBmp, mScale);
#endif
   }

   Drawing LoadDrawing () {
      Drawing dwg = new ();
      using (var stm = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GrayBMP.Data.leaf-fill.txt"))
      using (var sr = new StreamReader (stm)) {
         for (; ; ) {
            string line = sr.ReadLine (); if (line == null) break;
            double[] w = line.Split ().Select (double.Parse).Select (a => a * mScale).ToArray ();
            Point2 a = new (w[0], w[1]), b = new (w[2], w[3]);
            dwg.AddLine (new Line (a, b));
         }
      }
      return dwg;
   }
   Drawing mDwg;
}