using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        public static BindingList<DER> DERs { get; set; } // bolji od ObservableCollection
        public static List<DER> savedDERs = new List<DER>();

        private static BindingList<string> comboBoxRadioButtonItems = new BindingList<string>();
        private static BindingList<string> comboBoxComboItems = new BindingList<string>();


        public MyICommand AddDERCommand { get; set; }
        public MyICommand DeleteDERCommand { get; set; }
        public MyICommand FilterComboBoxDERCommand { get; set; }
        public MyICommand<StackPanel> FilterRadioButtonDERCommand { get; set; }
        public MyICommand<DER> SelectedItemChangedCommand { get; set; }
        public MyICommand<StackPanel> LoadStackPanelCommand { get; set; }
        public MyICommand<string> SelectedItemChangedComboBox1Command { get; set; }
        public MyICommand<string> SelectedItemChangedComboBox2Command { get; set; }
        public MyICommand<StackPanel> FilterComboDERCommand { get; set; }

        private DER currentDER;
        private DER removeDER;
        private DER filterDER;

        public NetworkEntitiesViewModel()
        {
            currentDER = new DER() { Id = 0, EnergyValue = "1", Name = "temp" };
            removeDER = new DER();
            filterDER = new DER() { Id = 0, EnergyValue = "1", Name = "temp" };

            DERs = new BindingList<DER>();

            fillList();

            AddDERCommand = new MyICommand(OnAdd, CanAdd);
            DeleteDERCommand = new MyICommand(OnDelete, CanDelete);
            FilterComboBoxDERCommand = new MyICommand(OnFilterComboBox, CanFilter);
            FilterRadioButtonDERCommand = new MyICommand<StackPanel>(OnFilterRadioButtonDER, CanFilterrRadioButton);

            SelectedItemChangedCommand = new MyICommand<DER>(OnSelectedItemChanged);
            LoadStackPanelCommand = new MyICommand<StackPanel>(OnLoadStackPanel);
            SelectedItemChangedComboBox1Command = new MyICommand<string>(OnSelectedItemChangedComboBox1);
            SelectedItemChangedComboBox2Command = new MyICommand<string>(OnSelectedItemChangedComboBox2);
            FilterComboDERCommand = new MyICommand<StackPanel>(OnFilterComboDER, CanFilterComboDER);
        }

        public DER CurrentDER
        {
            get { return currentDER; }
            set
            {
                currentDER = value;
                OnPropertyChanged("CurrentDER");
            }
        }
        public DER RemoveDER
        {
            get { return removeDER; }
            set
            {
                removeDER = value;
                OnPropertyChanged("RemoveDER");
            }
        }
        public DER FilterDER
        {
            get { return filterDER; }
            set
            {
                removeDER = value;
                OnPropertyChanged("FilterDER");
            }
        }

        public void fillList()
        {
            if (savedDERs.Count != 0)
            {
                foreach (var item in savedDERs.ToList())
                {
                    if (DERs.FirstOrDefault(x => x.Id == item.Id) == null)
                    {
                        DERs.Add(item);
                    }
                }
            }
            //DERs = savedDERs;
        }

        private bool CanAdd()
        {
            CurrentDER.Validate();

            return CurrentDER.IsValid;
            //return DERs.Count(x => x.Id == currentDER.Id) == 0;
        }
        private void OnAdd()
        {
            for (int i = 0; i < 12; i++)
            {
                CurrentDER.Id = i;
                if (DERs.FirstOrDefault(x => x.Id == currentDER.Id) == null)
                {
                    DERs.Add(new DER() { Id = CurrentDER.Id, Name = "Temporary name.", Type = CurrentDER.Type, EnergyValue = "Privremena energetska vrednost." });
                    savedDERs.Add(new DER() { Id = CurrentDER.Id, Name = "Temporary name.", Type = CurrentDER.Type, EnergyValue = "Privremena energetska vrednost." });
                    return;
                }
            }
        }

        private bool CanDelete()
        {
            return RemoveDER != null;
        }
        private void OnDelete()
        {
            DERs.Remove(DERs.FirstOrDefault(x => x.Id == RemoveDER.Id));

            foreach (var item in savedDERs.ToList())
            {
                if (DERs.FirstOrDefault(x => x.Id == item.Id) == null)
                {
                    savedDERs.Remove(item);
                }
            }
        }

        private bool CanFilter()
        {
            FilterDER.Validate();

            return FilterDER.IsValid;
        }
        private void OnFilterComboBox()
        {
            fillList();

            IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Type != FilterDER.Type)).ToList();

            foreach (DER user in usersToRemove)
            {
                DERs.Remove(user);
            }
        }

        private bool CanFilterrRadioButton(StackPanel stackPanel)
        {
            FilterDER.Validate();

            return FilterDER.IsValid;
        }
        private void OnFilterRadioButtonDER(StackPanel stackPanel)
        {
            fillList();

            RadioButton greater = (RadioButton)stackPanel.Children[3];
            RadioButton less = (RadioButton)stackPanel.Children[4];

            if (greater.IsChecked == true)
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id < filterDER.Id)).ToList();

                foreach (var item in usersToRemove)
                {
                    DERs.Remove(item);
                }
                if (comboBoxRadioButtonItems.FirstOrDefault(x => x == "Veće od " + filterDER.Id) == null)
                    comboBoxRadioButtonItems.Add("Veće od " + filterDER.Id);
            }

            if (less.IsChecked == true)
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id > filterDER.Id)).ToList();

                foreach (var item in usersToRemove)
                {
                    DERs.Remove(item);
                }
                if (comboBoxRadioButtonItems.FirstOrDefault(x => x == "Manje od " + filterDER.Id) == null)
                    comboBoxRadioButtonItems.Add("Manje od " + filterDER.Id);
            }
        }

        private void OnSelectedItemChanged(DER removeDER)
        {
            RemoveDER = removeDER;
        }

        private void OnLoadStackPanel(StackPanel stackPanel)
        {
            ComboBox comboBox1 = (ComboBox)stackPanel.Children[8];
            ComboBox comboBox2 = (ComboBox)stackPanel.Children[11];

            comboBox1.ItemsSource = comboBoxRadioButtonItems;
            comboBox2.ItemsSource = comboBoxComboItems;
        }

        private void OnSelectedItemChangedComboBox1(string selection)
        {
            fillList();

            char filterType = selection[0];
            int filterValue = int.Parse(selection.Split(' ')[2]);

            if (filterType == 'V')
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id < filterValue)).ToList();

                foreach (var item in usersToRemove)
                {
                    DERs.Remove(item);
                }
            }

            if (filterType == 'M')
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id > filterValue)).ToList();

                foreach (var item in usersToRemove)
                {
                    DERs.Remove(item);
                }
            }
        }
        private void OnSelectedItemChangedComboBox2(string selection)
        {
            fillList();


            string filterType = selection.Split(' ')[0];
            char filterValue = selection[selection.Length - 8];
            int filterId = int.Parse(selection.Split(' ')[5]);

            if (filterType == "Solarni_Panel")
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Type != GeneratorType.Solarni_Panel)).ToList();

                foreach (DER user in usersToRemove)
                {
                    DERs.Remove(user);
                }
            }
            else if (filterType == "Vetrogenerator")
            {
                IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Type != GeneratorType.Vetrogenerator)).ToList();

                foreach (DER user in usersToRemove)
                {
                    DERs.Remove(user);
                }
            }
            if (filterId >= 10)
            {
                filterValue = selection[selection.Length - 9];

                if (filterValue == 'ć')
                {
                    IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id <= filterId)).ToList();

                    foreach (var item in usersToRemove)
                    {
                        DERs.Remove(item);
                    }
                }
                else if (filterValue == 'j')
                {
                    IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id >= filterId)).ToList();

                    foreach (var item in usersToRemove)
                    {
                        DERs.Remove(item);
                    }
                }
            }
            else
            {
                if (filterValue == 'ć')
                {
                    IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id <= filterId)).ToList();

                    foreach (var item in usersToRemove)
                    {
                        DERs.Remove(item);
                    }
                }
                else if (filterValue == 'j')
                {
                    IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Id >= filterId)).ToList();

                    foreach (var item in usersToRemove)
                    {
                        DERs.Remove(item);
                    }
                }
            }
        }

        private bool CanFilterComboDER(StackPanel stackPanel)
        {
            FilterDER.Validate();

            return FilterDER.IsValid;
        }
        private void OnFilterComboDER(StackPanel stackPanel)
        {
            OnFilterRadioButtonDER(stackPanel);

            IReadOnlyList<DER> usersToRemove = DERs.Where(x => (x.Type != FilterDER.Type)).ToList();

            foreach (DER user in usersToRemove)
            {
                DERs.Remove(user);
            }

            string temp = FilterDER.Type.ToString() + " sa Id-em ";
            RadioButton greater = (RadioButton)stackPanel.Children[3];
            RadioButton less = (RadioButton)stackPanel.Children[4];

            if (greater.IsChecked == true)
                temp = temp + "većim od " + filterDER.Id;
            if (less.IsChecked == true)
                temp = temp + "manjim od " + filterDER.Id;

            if (comboBoxComboItems.FirstOrDefault(x => x == temp) == null)
                comboBoxComboItems.Add(temp);
        }
    }
}
