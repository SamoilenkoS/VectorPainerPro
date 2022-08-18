using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VectorModderPack;

namespace VectorPainerPro
{
    public partial class Form1 : Form
    {
        private bool _isClicked;
        private Point _start;
        private Action<Graphics, Pen, Point, Point> DrawSomething;
        Bitmap _temp;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _temp = (Bitmap)pictureBox1.Image.Clone();
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            if(openDllDialog.ShowDialog() == DialogResult.OK)
            {
                var assembly = Assembly.LoadFrom(openDllDialog.FileName);
                var types = assembly
                    .GetTypes()
                    .Where(x =>
                        x.GetInterface(typeof(IPaintable).FullName) != null);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DrawSomething = DrawLine;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _isClicked = true;
            _start = e.Location;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _isClicked = false;
            _temp = (Bitmap)pictureBox1.Image.Clone();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isClicked)
            {
                using (var bitmap = new Bitmap(_temp, pictureBox1.Width, pictureBox1.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        DrawSomething?.Invoke(graphics, Pens.Black, _start, e.Location);
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = (Bitmap)bitmap.Clone();
                    }
                }
            }
        }

        private void DrawLine(Graphics graphics, Pen pen, Point start, Point end)
        {
            graphics.DrawLine(pen, start, end);
        }

        private void DrawRectangle(Graphics graphics, Pen pen, Point start, Point end)
        {
            int width = end.X - start.X;
            int height = end.Y - start.Y;

            graphics.DrawRectangle(Pens.Black, start.X, start.Y, width, height);
        }

        private void toolStripRectangle_Click(object sender, EventArgs e)
        {
            DrawSomething = DrawRectangle;
        }
    }
}
