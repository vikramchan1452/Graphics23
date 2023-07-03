// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
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

      DrawLineTest ();
   }
   readonly GrayBMP mBmp;
   readonly int mDX, mDY;

   void DrawLineTest () {
      mBmp.Begin ();
      for (int i = 0; i < 360; i += 10) {
         double a = i * Math.PI / 180;
         int x0 = mDX / 2, y0 = mDY / 2;
         int x1 = (int)(x0 + Math.Cos (a) * 290);
         int y1 = (int)(y0 + Math.Sin (a) * 290);
         mBmp.DrawLine (x0, y0, x1, y1, 255);
      }
      mBmp.End ();
   }
}
