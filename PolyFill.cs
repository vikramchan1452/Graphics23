// PolyFill.cs - Polygon filler
// ---------------------------------------------------------------------------------------
//#define SORTEDAEL
namespace GrayBMP;

class PolyFill {
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 == y1) return;
      if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      mLines.Add (new NLine (new NPoint (x0, y0), new NPoint (x1, y1)));
   }

   public void Fill (GrayBMP bmp, int color) {
      bmp.Begin ();
      List<int> ints = new ();
      for (int y = 0; y < bmp.Height; y++) {
         double yf = y + 0.5;
         ints.Clear ();
         foreach (var line in mLines) {
            if (line.A.Y > yf || line.B.Y < yf) continue;
            double t = (double)(y - line.A.Y) / (line.B.Y - line.A.Y);
            double x = line.A.X * (1 - t) + line.B.X * t;
            ints.Add ((int)(x + 0.5));
         }
         ints.Sort ();
         for (int i = 0; i < ints.Count; i += 2)
            bmp.DrawHorizontalLine (ints[i], ints[i + 1], y, color);
      }
      bmp.Dirty ();
      bmp.End ();
   }

   readonly record struct NPoint (int X, int Y);
   readonly record struct NLine (NPoint A, NPoint B);
   List<NLine> mLines = new ();
}

class PolyFillFast {
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 == y1) return;
      if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      mYMin = Math.Min (mYMin, y0);
      mYMax = Math.Max (mYMax, y1);
      mEvents.Add (new Event (y0, mEdges.Count));
      double dx = (double)(x1 - x0) / (y1 - y0);
      mEdges.Add (new Edge (x0, dx, y1 - y0));
   }
   int mYMin = int.MaxValue, mYMax = int.MinValue;

   public void Fill (GrayBMP bmp, int color) {
      bmp.Begin ();

      mEvents.Sort ();
      List<Edge> active = new ();
      List<int> ints = new ();
      int nEvent = 0;   // Next event to process
      for (int y = mYMin; y <= mYMax; y++) {
         // Remove dead edges from the AEL
         active.RemoveAll (a => a.Life == 0);

         // Figure out all the edges that need to come in here
         while (nEvent < mEvents.Count && mEvents[nEvent].Y == y) {
#if SORTEDAEL
            var e = mEdges[mEvents[nEvent++].Index];
            int n = active.BinarySearch (e);
            if (n < 0) active.Insert (~n, e);
            else throw new NotImplementedException ();
#else
            active.Add (mEdges[mEvents[nEvent++].Index]);
#endif
         }

         ints.Clear ();
         for (int i = 0; i < active.Count; i++) {
            var e = active[i];
            ints.Add ((int)(e.X + 0.5));
            e.X += e.DX; e.Life--;
            active[i] = e;
         }
#if !SORTEDAEL
         ints.Sort ();
#endif
         for (int i = 0; i < ints.Count; i += 2)
            bmp.DrawHorizontalLine (ints[i], ints[i + 1], y, color);
      }

      bmp.Dirty ();
      bmp.End ();
   }

   public void Reset () {
      mEvents.Clear (); mEdges.Clear ();
      mYMin = int.MaxValue; mYMax = int.MinValue;
   }

   readonly record struct Event (int Y, int Index) : IComparable<Event> {
      public int CompareTo (Event other) => Y.CompareTo (other.Y);
   }
   List<Event> mEvents = new ();
   record struct Edge (double X, double DX, int Life) : IComparable<Edge> {
      public int CompareTo (Edge other) {
         int n = X.CompareTo (other.X); if (n != 0) return n;
         return DX.CompareTo (other.DX);
      }
   }
   List<Edge> mEdges = new ();
}
