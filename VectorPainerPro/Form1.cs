using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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

        CancellationTokenSource cancelationTokenSource;
        bool wasStopped;
        FigureAnimation figureAnimation;
        ToolParams currentParams;
        int currentAnimationIndex;
        int currentRepeatIndex;

        private void button1_Click(object sender, EventArgs e)
        {
            if (!wasStopped)
            {
                using (StreamReader sr = new StreamReader(
                    @"C:\Users\Sviatoslav_Samoilenk\AppData\Local\Temp\tmp84B.tmp"))
                {
                    figureAnimation = JsonSerializer.Deserialize<FigureAnimation>(sr.ReadToEnd());
                }

                currentParams = (ToolParams)figureAnimation.ToolParams.Clone();
                currentAnimationIndex = 0;
                currentRepeatIndex = 0;
            }

            cancelationTokenSource = new CancellationTokenSource();
            Thread thread = new Thread(() => Test(cancelationTokenSource.Token));
            thread.Start();
        }

        private void Test(CancellationToken cancellationToken)
        {
            do
            {
                using (var pen = new Pen(
                    Color.FromArgb(figureAnimation.ToolParams.Color), figureAnimation.ToolParams.LineWidth))
                {
                    for (; currentAnimationIndex < figureAnimation.AnimationActions.Count; ++currentAnimationIndex)
                    {
                        for (; currentRepeatIndex < figureAnimation.AnimationActions[currentAnimationIndex].Count; ++currentRepeatIndex)
                        {
                            using (var bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height))
                            {
                                using (var graphics = Graphics.FromImage(bitmap))
                                {
                                    graphics.DrawRectangle(
                                       pen,
                                       currentParams.StartX,
                                       currentParams.StartY,
                                       currentParams.EndX,
                                       currentParams.EndY);

                                    currentParams.Update(figureAnimation.AnimationActions[currentAnimationIndex]);
                                    Thread.Sleep(10);
                                    pictureBox1.Image?.Dispose();
                                    pictureBox1.Image = (Bitmap)bitmap.Clone();

                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        wasStopped = true;
                                        return;
                                    }
                                }
                            }
                        }

                        currentRepeatIndex = 0;
                    }

                    currentAnimationIndex = 0;
                    currentParams = (ToolParams)figureAnimation.ToolParams.Clone();
                }
            } while (true);


            //for (int i = 0; i < 200; i++)
            //{
            //    using (var bitmap = new Bitmap(_temp, pictureBox1.Width, pictureBox1.Height))
            //    {
            //        using (var graphics = Graphics.FromImage(bitmap))
            //        {
            //            graphics.DrawLine(Pens.Black, 0, 0, i, i);
            //            Thread.Sleep(10);
            //            pictureBox1.Image?.Dispose();
            //            pictureBox1.Image = (Bitmap)bitmap.Clone();
            //        }
            //    }
            //}
        }

        private void buttonCreateTestAnimation_Click(object sender, EventArgs e)
        {
            var random = new Random();
            var size = 100;
            var widthCenter = pictureBox1.Width / 2;
            var heightCenter = pictureBox1.Height / 2;
            var animationActions = new List<AnimationAction>();
            for (int i = 0; i < 5; i++)
            {
                animationActions.Add(new AnimationAction
                {
                    Count = (short)random.Next(10, 100),
                    DeltaStartX = (short)random.Next(-2, 2),
                    DeltaStartY = (short)random.Next(-2, 2)
                });
            }

            var animation = new FigureAnimation
            {
                ToolTitle = nameof(Rectangle),
                ToolParams = new ToolParams
                {
                    Color = Color.FromArgb(
                        random.Next(0, 255),
                        random.Next(0, 255),
                        random.Next(0, 255)).ToArgb(),
                    LineWidth = 2,
                    StartX = widthCenter - size,
                    EndX = heightCenter - size,
                    StartY = size,
                    EndY = size
                },
                AnimationActions = animationActions
            };

            var filepath = Path.GetTempFileName();
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.Write(JsonSerializer.Serialize(animation));
            }
            MessageBox.Show(filepath);
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            cancelationTokenSource.Cancel();
        }
    }
}
