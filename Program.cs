// Program.cs - Entry point into the GrayBMP application   
// ---------------------------------------------------------------------------------------
using System.Windows;
namespace GrayBMP;

class Program {
   [STAThread]
   static void Main () {
      // Create a MandelWin that shows an animated Mandelbrot set,
      // and create an Application object to do message-pumping and keep
      // the window alive
      new MandelWin ().Show ();
      new Application ().Run ();
   }
}
