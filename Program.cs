﻿using System.Windows;
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
      Image image = new Image () {
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
               Complex c = new Complex (x1 + x * step, y1 - y * step);
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
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         }
         finally {
            mBmp.Unlock ();
         }
      }
   }

   void OnMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
      if (FirstClick) {
         StartPoint = e.GetPosition (this);
         FirstClick = false;
      }
      else {
         EndPoint = e.GetPosition (this);
         Line (StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
         FirstClick = true;
      }
   }
   public bool FirstClick = true;
   public Point StartPoint;
   public Point EndPoint;

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

   void Line (double x1, double y1, double x2, double y2) {
      var Length = Point.Subtract (StartPoint, EndPoint).Length;
      var dX = (x2 - x1) / Length;
      var dY = (y2 - y1) / Length;
      try {  
         mBmp.Lock (); mBase = mBmp.BackBuffer;
         for (int i = 0; i < Length; i++) {
            x1 += dX;
            y1 += dY;
            int x = (int)x1, y = (int)y1;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         }
      } finally {
         mBmp.Unlock();
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}