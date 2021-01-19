using GalaSoft.MvvmLight.Messaging;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        public static BindingList<DisplayDER> DisplayDERs { get; set; }
        private DisplayDER selectedDisplayDER;

        public MyICommand<DisplayDER> SelectedItemChangedCommand { get; set; }
        public MyICommand<ListView> MouseLeftButtonUpCommand { get; set; }
        public MyICommand<ListView> LoadListViewCommand { get; set; }

        public MyICommand<Canvas> DropCommand { get; set; }
        public MyICommand<Canvas> DragOverCommand { get; set; }
        public MyICommand<Canvas> DragLeaveCommand { get; set; }
        public MyICommand<Canvas> MouseLeftButtonDownCanvasCommand { get; set; }
        public MyICommand<Canvas> MouseLeftButtonUpCanvasCommand { get; set; }
        public MyICommand<Canvas> MouseRightButtonDownCanvasCommand { get; set; }
        public MyICommand<Canvas> FreeCanvasCommand { get; set; }
        public MyICommand<Canvas> ArrangeElementsCommand { get; set; }
        public MyICommand<Canvas> MainCanvasLoadedCommand { get; set; }

        private bool dragging = false;
        private bool drawing = false;
        private string tempName;

        private ListView listView;
        private static Canvas mainCanvas = new Canvas();
        private List<int> tempList;
        private static Line line = new Line() { StrokeThickness = 4, Stroke = new SolidColorBrush(Colors.Black) };

        public NetworkDisplayViewModel()
        {
            SelectedDisplayDER = null;
            DisplayDERs = new BindingList<DisplayDER>();
            tempList = new List<int>();

            fillList();

            LoadListViewCommand = new MyICommand<ListView>(OnLoadListView);                   // ucitaj listView
            SelectedItemChangedCommand = new MyICommand<DisplayDER>(OnSelectedItemChanged);   // zapocni drag sa SelectedItem
            MouseLeftButtonUpCommand = new MyICommand<ListView>(OnMouseLeftButtonUp);         // kad se podigne levi klik, zavrsi drag

            DropCommand = new MyICommand<Canvas>(OnDrop);
            DragOverCommand = new MyICommand<Canvas>(OnDragOver);
            DragLeaveCommand = new MyICommand<Canvas>(OnDragLeave);

            MouseLeftButtonDownCanvasCommand = new MyICommand<Canvas>(OnMouseLeftButtonDownCanvas);
            MouseLeftButtonUpCanvasCommand = new MyICommand<Canvas>(OnMouseLeftButtonUpCanvas);
            MouseRightButtonDownCanvasCommand = new MyICommand<Canvas>(OnMouseRightButtonDownCanvas);

            FreeCanvasCommand = new MyICommand<Canvas>(OnFree);
            ArrangeElementsCommand = new MyICommand<Canvas>(OnArrangeElements);

            MainCanvasLoadedCommand = new MyICommand<Canvas>(OnMainCanvasLoaded);

            Messenger.Default.Register<CustomMessenger>
            (
            this,
            (action) => UpdateCanvas(action)
            );
        }

        public DisplayDER SelectedDisplayDER
        {
            get { return selectedDisplayDER; }
            set
            {
                selectedDisplayDER = value;
                OnPropertyChanged("SelectedDisplayDER");
            }
        }
        public Canvas MainCanvas
        {
            get { return mainCanvas; }
            set
            {
                mainCanvas = value;
                OnPropertyChanged("MainCanvas");
            }
        }

        private void fillList()
        {
            foreach (var item in NetworkEntitiesViewModel.DERs.ToList())
            {
                DisplayDER displayDER = new DisplayDER(item);

                DisplayDERs.Add(displayDER);

                checkCanvas(displayDER);
            }
            UpdateCanvasOnViewChange();
        }
        private void checkCanvas(DisplayDER displayDER)
        {
            for (int i = 1; i < MainCanvas.Children.Count; i++)   // prodi kroz svaki canvas
            {
                Canvas canvas = (Canvas)MainCanvas.Children[i];
                if (canvas.Resources["taken"] != null) // proveri da li je canvas zauzet, ako jeste pronadi mu id i promeni mu vrednosti
                {
                    if (!string.IsNullOrWhiteSpace(((TextBlock)(canvas).Children[1]).Text))
                        if (int.Parse(((TextBlock)(canvas).Children[1]).Text) == displayDER.Id)
                        {
                            if (!tempList.Contains(int.Parse(((TextBlock)(canvas).Children[1]).Text)))
                                tempList.Add(int.Parse(((TextBlock)(canvas).Children[1]).Text));
                        }
                }
            }
        }

        private void OnLoadListView(ListView listView)
        {
            this.listView = listView;
        }
        private void OnSelectedItemChanged(DisplayDER displayDER)
        {
            if (!dragging)
            {
                dragging = true;
                SelectedDisplayDER = new DisplayDER(displayDER);
                DragDrop.DoDragDrop(listView, SelectedDisplayDER, DragDropEffects.Move);
            }
        }
        private void OnMouseLeftButtonUp(ListView listView)
        {
            selectedDisplayDER = null;
            listView.SelectedItem = null;
            dragging = false;
        }

        private void OnDrop(Canvas canvas)
        {
            if (SelectedDisplayDER != null)
            {
                if (canvas.Resources["taken"] == null)
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,," + SelectedDisplayDER.ImageSource, UriKind.RelativeOrAbsolute);
                    logo.EndInit();

                    canvas.Background = new ImageBrush(logo);
                    canvas.Resources.Add("taken", true);
                    ((TextBlock)(canvas).Children[0]).Text = SelectedDisplayDER.Name;
                    ((TextBlock)(canvas).Children[0]).Foreground = Brushes.White;
                    ((TextBlock)(canvas).Children[1]).Text = SelectedDisplayDER.Id.ToString();
                    ((TextBlock)(canvas).Children[1]).Foreground = Brushes.Transparent;

                    tempName = SelectedDisplayDER.Name;

                    DisplayDERs.Remove(DisplayDERs.FirstOrDefault(x => x.Id == SelectedDisplayDER.Id));
                    SelectedDisplayDER = null;
                }
                ((TextBlock)(canvas).Children[0]).Text = tempName;
                ((TextBlock)(canvas).Children[0]).Foreground = Brushes.White;
                dragging = false;
            }
        }
        private void OnDragOver(Canvas canvas)
        {
            if (canvas.Resources["taken"] != null)
            {
                if (((TextBlock)(canvas).Children[0]).Text != "Polje je zauzeto!")
                {
                    tempName = ((TextBlock)(canvas).Children[0]).Text;
                    ((TextBlock)(canvas).Children[0]).Foreground = Brushes.White;
                }

                ((TextBlock)(canvas).Children[0]).Text = "Polje je zauzeto!";
                ((TextBlock)(canvas).Children[0]).Foreground = Brushes.Red;
            }
        }
        private void OnDragLeave(Canvas canvas)
        {
            if (((TextBlock)(canvas).Children[0]).Text == "Polje je zauzeto!")
            {
                ((TextBlock)(canvas).Children[0]).Text = tempName;
                ((TextBlock)(canvas).Children[0]).Foreground = Brushes.White;
            }
        }

        private void OnMouseLeftButtonDownCanvas(Canvas canvas)
        {
            if (!dragging)
            {
                if (canvas.Resources["taken"] != null)
                {
                    dragging = true;
                    SelectedDisplayDER = new DisplayDER(new DisplayDER(NetworkEntitiesViewModel.DERs.FirstOrDefault(x => x.Id == int.Parse(((TextBlock)(canvas).Children[1]).Text))));
                    if (DragDrop.DoDragDrop(listView, SelectedDisplayDER, DragDropEffects.Move) != DragDropEffects.None)
                    {
                        canvas.Background = Brushes.Azure;
                        canvas.Resources.Remove("taken");

                        ((TextBlock)(canvas).Children[0]).Text = "Slobodno mesto";
                        ((TextBlock)(canvas).Children[0]).Foreground = Brushes.Black;
                        ((TextBlock)(canvas).Children[1]).Text = "";
                        ((TextBlock)(canvas).Children[1]).Foreground = Brushes.Transparent;
                    }
                }
                dragging = false;
            }
        }
        private void OnMouseLeftButtonUpCanvas(Canvas canvas)
        {
            SelectedDisplayDER = null;
            dragging = false;
        }
        private void OnMouseRightButtonDownCanvas(Canvas canvas)
        {
            Canvas parentCanvas = (Canvas)VisualTreeHelper.GetParent(canvas);

            if (canvas.Resources["taken"] != null)
            {
                if (!drawing)
                {
                    line.X1 = Canvas.GetLeft(canvas) + canvas.ActualWidth / 2;
                    line.Y1 = Canvas.GetTop(canvas) + canvas.ActualHeight / 2;

                    parentCanvas.Children.Add(line);

                    drawing = true;
                }
                else
                {
                    line.X2 = Canvas.GetLeft(canvas) + canvas.ActualWidth / 2;
                    line.Y2 = Canvas.GetTop(canvas) + canvas.ActualHeight / 2;

                    line = new Line() { StrokeThickness = 4, Stroke = new SolidColorBrush(Colors.Black) };

                    drawing = false;
                }
            }
        }

        private void OnFree(Canvas canvas)
        {
            if (canvas.Resources["taken"] != null)
            {
                DisplayDERs.Add(new DisplayDER(NetworkEntitiesViewModel.DERs.FirstOrDefault(x => x.Id == int.Parse(((TextBlock)(canvas).Children[1]).Text))));

                canvas.Background = Brushes.Azure;
                canvas.Resources.Remove("taken");

                ((TextBlock)(canvas).Children[0]).Text = "Slobodno mesto";
                ((TextBlock)(canvas).Children[0]).Foreground = Brushes.Black;
                ((TextBlock)(canvas).Children[1]).Text = "";
                ((TextBlock)(canvas).Children[1]).Foreground = Brushes.Transparent;
            }
        }

        private void OnArrangeElements(Canvas canvas)
        {
            foreach (var item in DisplayDERs.ToList())
            {
                for (int i = 1; i < 13; i++)
                {
                    Canvas canvas1 = (Canvas)(MainCanvas).Children[i];
                    if (canvas1.Resources["taken"] == null)
                    {
                        BitmapImage logo = new BitmapImage();
                        logo.BeginInit();
                        logo.UriSource = new Uri("pack://application:,,," + item.ImageSource, UriKind.RelativeOrAbsolute);
                        logo.EndInit();

                        canvas1.Background = new ImageBrush(logo);
                        canvas1.Resources.Add("taken", true);
                        ((TextBlock)(canvas1).Children[0]).Text = item.Name;
                        ((TextBlock)(canvas1).Children[0]).Foreground = Brushes.White;
                        ((TextBlock)(canvas1).Children[1]).Text = item.Id.ToString();
                        ((TextBlock)(canvas1).Children[1]).Foreground = Brushes.Transparent;

                        DisplayDERs.Remove(DisplayDERs.FirstOrDefault(x => x.Id == item.Id));

                        break;
                    }
                }
            }
        }

        private void OnMainCanvasLoaded(Canvas canvas)
        {
            if (MainCanvas.Children.Count == 0)
            {
                MainCanvas = canvas;
            }
            else
            {
                for (int i = 1; i < 13; i++)
                {
                    Canvas showCanvas = (Canvas)canvas.Children[i];
                    Canvas savedCanvas = (Canvas)MainCanvas.Children[i];

                    showCanvas.Background = savedCanvas.Background;
                    showCanvas.Resources = savedCanvas.Resources;
                    ((TextBlock)(showCanvas).Children[0]).Text = ((TextBlock)(savedCanvas).Children[0]).Text;
                    ((TextBlock)(showCanvas).Children[0]).Foreground = ((TextBlock)(savedCanvas).Children[0]).Foreground;
                    ((TextBlock)(showCanvas).Children[1]).Text = ((TextBlock)(savedCanvas).Children[1]).Text;
                    ((TextBlock)(showCanvas).Children[1]).Foreground = ((TextBlock)(savedCanvas).Children[1]).Foreground;

                    if (!string.IsNullOrEmpty(((TextBlock)(showCanvas).Children[1]).Text))
                        DisplayDERs.Remove(DisplayDERs.FirstOrDefault(x => x.Id == int.Parse(((TextBlock)showCanvas.Children[1]).Text)));
                }
                MainCanvas = canvas;
            }
        }

        private void UpdateCanvas(CustomMessenger messenger)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 1; i < MainCanvas.Children.Count; i++)   // prodi kroz svaki canvas
                {
                    Canvas canvas = (Canvas)MainCanvas.Children[i];
                    if (canvas.Resources["taken"] != null) // proveri da li je canvas zauzet, ako jeste pronadi mu id i promeni mu vrednosti
                    {
                        if (!string.IsNullOrWhiteSpace(((TextBlock)(canvas).Children[1]).Text))
                            if (int.Parse(((TextBlock)(canvas).Children[1]).Text) == messenger.Id)
                            {
                                BitmapImage logo = new BitmapImage();
                                logo.BeginInit();
                                logo.UriSource = new Uri("pack://application:,,," + messenger.ImageSource, UriKind.RelativeOrAbsolute);
                                logo.EndInit();

                                ((TextBlock)(canvas).Children[0]).Text = messenger.Name;
                                ((TextBlock)(canvas).Children[0]).Foreground = Brushes.White;

                                canvas.Background = new ImageBrush(logo);

                                //MainCanvas.Children.RemoveAt(i);
                                //MainCanvas.Children[i] = canvas;
                            }
                    }
                }
            });
        }
        private void UpdateCanvasOnViewChange()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 1; i < MainCanvas.Children.Count; i++)   // prodi kroz svaki canvas
                {
                    Canvas canvas = (Canvas)MainCanvas.Children[i];

                    if (!string.IsNullOrWhiteSpace(((TextBlock)(canvas).Children[1]).Text))
                        if (!tempList.Contains(int.Parse(((TextBlock)(canvas).Children[1]).Text)))
                        {
                            canvas.Background = Brushes.Azure;
                            canvas.Resources.Remove("taken");

                            ((TextBlock)(canvas).Children[0]).Text = "Slobodno mesto";
                            ((TextBlock)(canvas).Children[0]).Foreground = Brushes.Black;
                            ((TextBlock)(canvas).Children[1]).Text = "";
                            ((TextBlock)(canvas).Children[1]).Foreground = Brushes.Transparent;
                        }
                }
            });
        }
    }
}
