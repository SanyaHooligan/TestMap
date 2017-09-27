using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TestMapApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Tick += DrawABitMore;
            FindEllipses();
        }
        IEnumerable<Ellipse> ellipseCollection;
        DispatcherTimer timer;
        Polyline l;
        const int FPS = 100;
        const int INF = int.MaxValue;
        Counter counter = new Counter(FPS, 0);
        int countCoordinates = 1;
        int GlobalRouteCounter = 1;
        Double[,] matrix;
        Double[,] weightMatrix;
        int[,] pathMatrix;
        int length;
        Point TempStartPoint;
        Point TempEndPoint;

        private Ellipse FindTag(int firstTag)
        {
            Ellipse outEllipse = null;
            foreach (var item in ellipseCollection)
            {
                if (item.Tag.ToString().Split(',')[0] == firstTag.ToString())
                {
                    outEllipse = item;
                }
            }
            if (outEllipse == null)
            {
                throw new NullReferenceException("Не найдено элемента с заданным Tag");
            }
            return outEllipse;
        }

        private int GetTagFirstElementFromEllipse(String ellipseName)
        {
            Ellipse ellipse;
            try
            {
                ellipse = this.FindName(ellipseName) as Ellipse;
            }
            catch (Exception ex)
            {
                ellipse = null;
                MessageBox.Show(ex.Message);
                throw new NullReferenceException("не надено точки");
            }
            String[] ellipseTag = ellipse.Tag.ToString().Split(',');
            int TagInt = 0;
            bool TagFlag = int.TryParse(ellipseTag[0], out TagInt);
            if (!TagFlag) throw new NullReferenceException("не удалось преобразовать элемент в int");
            return TagInt;
        }

        private void FindEllipses()
        {
            ellipseCollection = MainCanvas.Children.OfType<Ellipse>();
            matrix = new Double[ellipseCollection.Count(), ellipseCollection.Count()];
            length = matrix.GetLength(0);
            pathMatrix = new int[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (i == j) matrix[i, j] = 0;
                    else matrix[i, j] = INF;
                    pathMatrix[i, j] = j;
                }
            }
            foreach (var item in ellipseCollection)
            {
                List<String> tags = item.Tag.ToString().Split(',').ToList();
                int first = 0;
                bool flag = int.TryParse(tags[0], out first);
                if (!flag) throw new NullReferenceException("не удалось преобразовать первый элемент в int");
                if (first == 6)
                {

                }
                tags.RemoveAt(0);
                foreach (var tag in tags)
                {
                    int TagInt = 0;
                    bool TagFlag = int.TryParse(tag, out TagInt);
                    if (!TagFlag) throw new NullReferenceException("не удалось преобразовать элемент в int");
                    matrix[first - 1, TagInt - 1] = GetLegthBetweenEllipses(FindTag(first), FindTag(TagInt));
                }
            }
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    Debug.Write(matrix[i, j] + "        ");
                }
                Debug.WriteLine("");
            }
            weightMatrix = matrix;
            //создание матрицы растояний между всеми элементами и матрицы предков
            for (int k = 0; k < length; k++)
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        double min = weightMatrix[i, k] + weightMatrix[k, j];
                        if (weightMatrix[i, j] > min)
                        {
                            weightMatrix[i, j] = min;
                            pathMatrix[i, j] = pathMatrix[i, k];
                        }
                    }
                }
            }
        }

        private Double GetLegthBetweenPoints(Point start, Point end)
        {
            Double X = Math.Abs(end.X - start.X);
            Double Y = Math.Abs(end.Y - start.Y);
            Double legth = Math.Sqrt(X * X + Y * Y);
            return legth;
        }

        private Double GetLegthBetweenEllipses(Ellipse startEllipse, Ellipse endEllipse)
        {
            Point startPoint = new Point(Double.Parse(startEllipse.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(startEllipse.GetValue(Canvas.TopProperty).ToString()));
            Point endPoint = new Point(Double.Parse(endEllipse.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(endEllipse.GetValue(Canvas.TopProperty).ToString()));
            return GetLegthBetweenPoints(startPoint, endPoint);
        }

        //метод получения отдельного восстановленного пути
        private List<int> GetClosestRoute(int startIndex, int endIndex)
        {
            List<int> pathList = new List<int>();
            int k = startIndex - 1;
            while (k != endIndex - 1)
            {
                pathList.Add(k);
                k = pathMatrix[k, endIndex - 1];
            }
            pathList.Add(k);
            return pathList;
        }

        public void DrawRoute(String StartElement, String EndElement)
        {
            Ellipse startElement;
            Ellipse endElement;
            try
            {
                startElement = (Ellipse)this.FindName(StartElement);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("не найдено начальной точки");
            }

            try
            {
                endElement = (Ellipse)this.FindName(EndElement);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("не найдено конечной точки");
            }

            Point startPoint = new Point(Double.Parse(startElement.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(startElement.GetValue(Canvas.TopProperty).ToString()));
            Point endPoint = new Point(Double.Parse(endElement.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(endElement.GetValue(Canvas.TopProperty).ToString()));
            //Double length = GetLegthBetweenPoints(startPoint, endPoint);
            List<int> route = GetClosestRoute(GetTagFirstElementFromEllipse(StartElement), GetTagFirstElementFromEllipse(EndElement));
            if (l == null)
            {
                l = new Polyline();
                MainCanvas.Children.Add(l);
            }
            l.Stroke = new SolidColorBrush(Colors.Purple);
            l.StrokeThickness = 10.0;
            l.StrokeLineJoin = PenLineJoin.Round;
            l.Points = new PointCollection();
            l.Name = "NewLine";
            Canvas.SetZIndex(l, 1);
            for (int i = 0; i < route.Count - 1; i++)
            {
                DrawOneLine(route[i], route[i + 1]);
            }
            //foreach(var item in route)
            //{
            //    Point itemPoint = new Point(Double.Parse(FindTag(item+1).GetValue(Canvas.LeftProperty).ToString())+FindTag(item+1).ActualWidth/2, Double.Parse(FindTag(item+1).GetValue(Canvas.TopProperty).ToString())+FindTag(item+1).ActualHeight/2);
            //    l.Points.Add(itemPoint);
            //}
            //try
            //{
            //    MainCanvas.Children.Remove((Polyline)MainCanvas.FindName("NewLine"));
            //}
            //catch(NullReferenceException)
            //{

            //}
        }

        private void DrawOneLine(int StartElement, int EndElement)
        {
            Ellipse startEllipse = FindTag(StartElement+1);
            Ellipse endEllipse = FindTag(EndElement+1);
            TempStartPoint = new Point(Double.Parse(startEllipse.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(startEllipse.GetValue(Canvas.TopProperty).ToString()));
            TempEndPoint = new Point(Double.Parse(endEllipse.GetValue(Canvas.LeftProperty).ToString()), Double.Parse(endEllipse.GetValue(Canvas.TopProperty).ToString()));
            l.Points.Add(new Point(TempStartPoint.X + startEllipse.ActualWidth / 2, TempStartPoint.Y + startEllipse.ActualHeight / 2));
            l.Points.Add(new Point(TempStartPoint.X + (TempEndPoint.X - TempStartPoint.X) / FPS + endEllipse.ActualWidth / 2, TempStartPoint.Y + (TempEndPoint.Y - TempStartPoint.Y) / FPS + endEllipse.ActualHeight / 2));
            //MainCanvas.Children.Add(l);
            timer.Start();
            counter.LimitReached += StopDrawing;
        }

        private void DrawABitMore(object sender, EventArgs e)
        {
            double x2 = TempStartPoint.X + counter.Total * (TempEndPoint.X - TempStartPoint.X) / FPS + Second.ActualWidth / 2;
            double y2 = TempStartPoint.Y + counter.Total * (TempEndPoint.Y - TempStartPoint.Y) / FPS + Second.ActualHeight / 2;
            try
            {
                l.Points.RemoveAt(l.Points.Count - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("ошибка в индексировании координат");
            }
            l.Points.Add(new Point(x2, y2));
            counter.Add(1);
        }

        private void ChangeAngle()
        {
            GlobalRouteCounter++;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String[] input = SearchField.Text.Split(' ');
            DrawRoute(input[0], input[1]);
        }

        private void StopDrawing(object sender, EventArgs e)
        {
            timer.Stop();
            //ChangeAngle();
        }
    }
}
