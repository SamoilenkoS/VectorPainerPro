using MyMods.Properties;
using System;
using System.Drawing;
using System.IO;

namespace VectorModderPack
{
    public class LineMod : IPaintable
    {
        public Bitmap Icon => Resources.Pencil;

        public string ToolTitle => "Line";

        public void Draw(Graphics graphics, Pen pen, Point start, Point end)
        {
            graphics.DrawLine(pen, start, end);
        }
    }
}
