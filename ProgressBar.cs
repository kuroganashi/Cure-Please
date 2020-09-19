namespace CurePlease
{
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Windows.Forms;

  public class NewProgressBar : ProgressBar
  {
    public NewProgressBar() => SetStyle(ControlStyles.UserPaint, true);

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      if (ClientRectangle.Width > 0 && ClientRectangle.Height > 0)
      {

      }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      const int inset = 2; // A single inset value to control teh sizing of the inner rect.

      using (Image offscreenImage = new Bitmap(Width, Height))
      {
        using (var offscreen = Graphics.FromImage(offscreenImage))
        {
          var rect = new Rectangle(0, 0, Width, Height);

          if (ProgressBarRenderer.IsSupported)
          {
            ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);
          }

          rect.Inflate(new Size(-inset, -inset)); // Deflate inner rect.
          rect.Width = (int)(rect.Width * ((double)Value / Maximum));

          if (rect.Width != 0)
          {
            var brush = new LinearGradientBrush(rect, ForeColor, ForeColor, LinearGradientMode.Vertical);
            offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);
            e.Graphics.DrawImage(offscreenImage, 0, 0);
            offscreenImage.Dispose();

          }
          else
          {
            rect.Width = 215;

            var brush = new LinearGradientBrush(rect, SystemColors.ScrollBar, SystemColors.ScrollBar, LinearGradientMode.Vertical);
            offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);
            e.Graphics.DrawImage(offscreenImage, 0, 0);
            offscreenImage.Dispose();

          }
        }
      }
    }
  }
}

// Source: http://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
