using GalaSoft.MvvmLight.Messaging;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        private static BindingList<string> currentEntitiesList = new BindingList<string>();

        private static List<string> allIncommingData = new List<string>();

        private string selectedEntity;
        private string showEntity;
        private Canvas myCanvas;

        public MyICommand<Canvas> LoadCanvasCommand { get; set; }
        public MyICommand<ComboBox> LoadedComboBoxCommand { get; set; }
        public MyICommand<Canvas> ShowEntityCommand { get; set; }
        public MyICommand<string> SelectedItemChangedComboBoxCommand { get; set; }

        public MeasurementGraphViewModel()
        {
            LoadCanvasCommand = new MyICommand<Canvas>(OnLoadCanvas);
            LoadedComboBoxCommand = new MyICommand<ComboBox>(OnLoadedComboBox);
            ShowEntityCommand = new MyICommand<Canvas>(OnShowEntity, CanShowEntity);
            SelectedItemChangedComboBoxCommand = new MyICommand<string>(OnSelectedItemChangedComboBox);

            Messenger.Default.Register<CustomMessenger>
            (
            this,
            (action) => UpdateAllIncimmingDataList(action)
            );
        }
        public string SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                selectedEntity = value;
                OnPropertyChanged("SelectedEntity");
            }
        }
        public string ShowEntity
        {
            get { return showEntity; }
            set
            {
                showEntity = value;
                OnPropertyChanged("ShowEntity");
            }
        }

        private void UpdateAllIncimmingDataList(CustomMessenger messenger)
        {
            UpdateComboBox(messenger.Name);

            using (StreamReader readtext = new StreamReader("log.txt"))
            {
                string readText = readtext.ReadLine();

                if (!allIncommingData.Contains(readText))
                {
                    allIncommingData.Add(readText);
                }
            }
            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int left = 0;
                int top = 240;

                Queue<int> tempList = new Queue<int>();
                List<Canvas> canvasList = new List<Canvas>()
            {
                new Canvas(),
                new Canvas(),
                new Canvas(),
                new Canvas(),
                new Canvas()
            };

                foreach(Canvas canvas in canvasList)
                {
                    canvas.Children.Add(new Ellipse() { Height = 25, Width = 25 });
                    canvas.Children.Add(new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment= VerticalAlignment.Center, TextAlignment = TextAlignment.Center, FontSize = 16 });
                }

                ShowEntity = SelectedEntity;

                if (string.IsNullOrWhiteSpace(ShowEntity))
                    return;

                if (myCanvas.Children.Count != 8)
                {
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                }

                foreach (var item in allIncommingData)
                {
                    string entityName = "E" + item.Split(':')[2].Split('E')[1];
                    if (entityName == SelectedEntity)
                    {
                        if (tempList.Count == 5)
                            tempList.Dequeue();

                        tempList.Enqueue(int.Parse(item.Split(':')[3]));
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    int value;
                    try
                    {
                        value = tempList.Dequeue();
                    }
                    catch
                    {
                        value = -1;
                    }

                    if (value == -1)
                    {
                        //Canvas.SetTop(canvasList[i], top - 20 * value);
                        //Canvas.SetLeft(canvasList[i], left);

                        left = left + 80;
                    }
                    else if (value > 5)
                    {
                        ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Red);
                        ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                        ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                        Canvas.SetTop(canvasList[i], top - 20 * value);
                        Canvas.SetLeft(canvasList[i], left);

                        left = left + 80;
                    }
                    else if (value < 1)
                    {
                        ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Red);
                        ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                        ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                        Canvas.SetTop(canvasList[i], top - 20 * value);
                        Canvas.SetLeft(canvasList[i], left);

                        left = left + 80;
                    }
                    else
                    {
                        ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Black);
                        ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                        ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                        Canvas.SetTop(canvasList[i], top - 20 * value);
                        Canvas.SetLeft(canvasList[i], left);

                        left = left + 80;
                    }
                    myCanvas.Children.Add(canvasList[i]);
                }
            });
        }
        private void UpdateComboBox(string name)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (!currentEntitiesList.Contains(name))
                {
                    currentEntitiesList.Add(name);
                }
            });
        }

        private void OnLoadedComboBox(ComboBox comboBox)
        {
            comboBox.ItemsSource = currentEntitiesList;
        }
        private void OnLoadCanvas(Canvas canvas)
        {
            myCanvas = canvas;
        }

        private bool CanShowEntity(Canvas canvas)
        {
            //if (string.IsNullOrWhiteSpace(SelectedEntity))
            //    return false;
            return true;
        }
        private void OnShowEntity(Canvas canvas)
        {
            int left = 0;
            int top = 240;

            Queue<int> tempList = new Queue<int>();
            List<Canvas> canvasList = new List<Canvas>()
            {
                new Canvas(),
                new Canvas(),
                new Canvas(),
                new Canvas(),
                new Canvas()
            };

            foreach (Canvas canvas1 in canvasList)
            {
                canvas1.Children.Add(new Ellipse() { Height = 25, Width = 25 });
                canvas1.Children.Add(new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center, FontSize = 16 });
            }

            ShowEntity = SelectedEntity;

            if (string.IsNullOrWhiteSpace(ShowEntity))
                return;

            if (myCanvas.Children.Count != 8)
            {
                myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
                myCanvas.Children.RemoveAt(myCanvas.Children.Count - 1);
            }

            foreach (var item in allIncommingData)
            {
                string entityName = "E" + item.Split(':')[2].Split('E')[1];
                if (entityName == SelectedEntity)
                {
                    if (tempList.Count == 5)
                        tempList.Dequeue();

                    tempList.Enqueue(int.Parse(item.Split(':')[3]));
                }
            }

            for (int i = 0; i < 5; i++)
            {
                int value;
                try
                {
                    value = tempList.Dequeue();
                }
                catch
                {
                    value = -1;
                }

                if (value == -1)
                {
                    //Canvas.SetTop(canvasList[i], top - 20 * value);
                    //Canvas.SetLeft(canvasList[i], left);

                    left = left + 80;
                }
                else if (value > 5)
                {
                    ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Red);
                    ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                    ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                    Canvas.SetTop(canvasList[i], top - 20 * value);
                    Canvas.SetLeft(canvasList[i], left);

                    left = left + 80;
                }
                else if (value < 1)
                {
                    ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Red);
                    ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                    ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                    Canvas.SetTop(canvasList[i], top - 20 * value);
                    Canvas.SetLeft(canvasList[i], left);

                    left = left + 80;
                }
                else
                {
                    ((Ellipse)canvasList[i].Children[0]).Stroke = new SolidColorBrush(Colors.Black);
                    ((Ellipse)canvasList[i].Children[0]).StrokeThickness = 2;
                    ((TextBlock)canvasList[i].Children[1]).Text = "  " + value.ToString();

                    Canvas.SetTop(canvasList[i], top - 20 * value);
                    Canvas.SetLeft(canvasList[i], left);

                    left = left + 80;
                }
                myCanvas.Children.Add(canvasList[i]);
            }
        }

        private void OnSelectedItemChangedComboBox(string item)
        {
            SelectedEntity = item;
        }
    }
}
