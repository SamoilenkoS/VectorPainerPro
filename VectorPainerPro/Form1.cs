using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VectorModderPack;

namespace VectorPainerPro
{
    //public class Figure
    //{
    //    public string ToolTitle { get; set; }
    //    public List<Point> Points { get; set; }
    //    public int LineWidth { get; set; }
    //    public int Color { get; set; }
    //}

    //public class ImageInfo
    //{
    //    public List<Figure> Figures { get; set; }
    //    public Dictionary<string, string> ToolsFilepathes { get; set; }//key = tool title
    //    //value => path to library
    //}

    public partial class Form1 : Form
    {
        private bool _isClicked;
        private string _currentTool;
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

                foreach (var type in types)
                {
                    GenerateButton(type);
                }
            }
        }

        private void GenerateButton(Type type)
        {
            if (type.GetInterface(typeof(IPaintable).FullName) == null)
            {
                throw new ArgumentException();
            }

            var obj = Activator.CreateInstance(type);
            var toolTitle = GetPropertyFromType<string>(type, nameof(IPaintable.ToolTitle), obj);
            var icon = GetPropertyFromType<Bitmap>(type, nameof(IPaintable.Icon), obj);

            var onClickMethod = type.GetMethod(nameof(IPaintable.Draw), BindingFlags.Public | BindingFlags.Instance);
            var action = (Action<Graphics, Pen, Point, Point>)Delegate
                .CreateDelegate(typeof(Action<Graphics, Pen, Point, Point>), obj, onClickMethod);

            var onClick = new EventHandler((x, y) =>
            {
                _currentTool = toolTitle;
                DrawSomething = action;
            });

            ToolStripButton toolStripButton = new ToolStripButton(toolTitle, icon, onClick, toolTitle);

            toolStripTools.Items.Add(toolStripButton);
        }

        private T GetPropertyFromType<T>(Type type, string propertyTitle, object instance)
        {
            var property = type
              .GetProperty(
                  propertyTitle,
                  BindingFlags.Public | BindingFlags.Instance);

            var propertyValue = property.GetValue(instance);

            return (T)propertyValue;
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
                        //foreach var figure in figures
                        //ToolChoose click
                        DrawSomething?.Invoke(graphics, Pens.Black, _start, e.Location);
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = (Bitmap)bitmap.Clone();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(Test);
            thread.Start();
        }

        private void Test()
        {
            for (int i = 0; i < 200; i++)
            {
                using (var bitmap = new Bitmap(_temp, pictureBox1.Width, pictureBox1.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.DrawLine(Pens.Black, 0, 0, i, i);
                        Thread.Sleep(10);
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = (Bitmap)bitmap.Clone();
                    }
                }
            }
        }
    }
}
