using MyMods.Properties;
using System;
using System.Drawing;
using System.IO;

namespace VectorModderPack
{
    public class RectangleMod : IPaintable
    {
        public Bitmap Icon => Resources.Rectangle;

        public string ToolTitle => nameof(Resources.Rectangle);

        public void Draw(Graphics graphics, Pen pen, Point start, Point end)
        {
            int width = end.X - start.X;
            int height = end.Y - start.Y;

            graphics.DrawRectangle(Pens.Black, start.X, start.Y, width, height);
        }

        public void Fill(Graphics graphics, Brush brush, Point start, Point end)
        {
            throw new NotImplementedException();
        }
    }
}
