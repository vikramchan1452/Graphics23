// PolyFill.cs - Polygon filler
// ---------------------------------------------------------------------------------------
namespace GrayBMP;

class PolyFill {
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 != y1) {
         if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
         mLines.Add (new (new (x0, y0), new (x1, y1)));
      }
   }

   public void Fill (GrayBMP bmp, int color) {
      bmp.Begin ();
      List<int> X = new ();
      for (int y = 0; y < bmp.Height; y++) {
         X.Clear ();
         double yf = y + 0.5;
         foreach (var line in mLines) {
            if (line.A.Y > yf || line.B.Y < yf) continue;
            double lie = (double)(y - line.A.Y) / (line.B.Y - line.A.Y);
            double x = line.A.X * (1 - lie) + line.B.X * lie;
            X.Add ((int)(x + 0.5));
         }
         X.Sort ();
         for (int i = 0; i < X.Count - 1; i += 2)
            bmp.DrawHorizontalLine (X[i], X[i + 1], y, color);
      }
      bmp.Dirty (0, 0, bmp.Width - 1, bmp.Height - 1);
      bmp.End ();
   }

   readonly record struct NPoint (int X, int Y);
   readonly record struct NLine (NPoint A, NPoint B);

   List<NLine> mLines = new ();
}

class PolyFillFast {
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 == y1) return;   // Ignore horizontal lines
      if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);     // Ensure y0 < y1
      mEvents.Add (new Event (x0, y0, x1, mEdge.Count));
      var dx = (float)(x1 - x0) / (y1 - y0);
      mEdge.Add (new Edge { X = x0, DX = dx, Life = y1 - y0 });
   }
   List<Event> mEvents = new ();
   List<Edge> mEdge = new ();

   public void Fill (GrayBMP bmp, int color) {
      bmp.Begin ();
      mEvents.Sort (); 
      List<Edge> mActive = new ();
      List<int> X = new ();
      int nEvent = 0;   // Next event to consider

      for (int y = 0; y < bmp.Height; y++) {
         // Remove the active edges that are end-of-life
         for (int i = mActive.Count - 1; i >= 0; i--)
            if (mActive[i].Life == 0) mActive.RemoveAt (i);

         // Add the new edges that are entering at this Y
         while (nEvent < mEvents.Count && mEvents[nEvent].Y == y) {
#if SORTED
            var edge = mEdge[mEvents[nEvent++].Index];
            int n = mActive.BinarySearch (edge);
            if (n < 0) mActive.Insert (~n, edge);
            else throw new NotImplementedException ();
#else
            mActive.Add (mEdge[mEvents[nEvent++].Index]);
#endif
         }

         // Draw the active spans, and adjust the life
         X.Clear ();
         for (int i = 0; i < mActive.Count; i++) {
            Edge e = mActive[i];
            X.Add ((int)(e.X + 0.5));
            e.X += e.DX; e.Life--;
            mActive[i] = e; 
         }
#if !SORTED
         X.Sort ();
#endif
         for (int i = 0; i < X.Count; i += 2)
            bmp.DrawHorizontalLine (X[i], X[i + 1], y, color);
      }
      bmp.Dirty (0, 0, bmp.Width - 1, bmp.Height - 1);
      bmp.End ();
   }

   struct Edge : IComparable<Edge> {
      public float X;      // Current X value of this edge (increments on each scan)
      public float DX;     // X-step per line (InverseSlope)
      public int Life;     // How many more scan-lines is this edge 'alive' for?

      public int CompareTo (Edge other) {
         int n = X.CompareTo (other.X); if (n != 0) return n;
         n = DX.CompareTo (other.DX); if (n != 0) return n;
         return 0; 
      }
   }

   readonly record struct Event (int X, int Y, int X2, int Index) : IComparable<Event> {
      public int CompareTo (Event other) {
         int n = Y.CompareTo (other.Y); if (n != 0) return n;
         n = X.CompareTo (other.X); if (n != 0) return n;
         n = X2.CompareTo (other.X2); if (n != 0) return n;
         return 0; 
      }
   }
}
