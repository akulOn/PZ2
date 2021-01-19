using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class DER : ValidationBase
    {
        private int id;
        private string name;
        private GeneratorType type;
        private string energyValue;

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public GeneratorType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public string EnergyValue
        {
            get { return energyValue; }
            set
            {
                if (energyValue != value)
                {
                    energyValue = value;
                    OnPropertyChanged("EnergyValue");
                }
            }
        }
        protected override void ValidateSelf()
        {
            if (Id < -1)
            {
                this.ValidationErrors["Id"] = "Mora biti veće od -1.";
            }
            if (Id >= 12)
            {
                this.ValidationErrors["Id"] = "Mora biti manje od 12.";
            }

            if (int.Parse(EnergyValue) < 0)
            {
                this.ValidationErrors["EnergyValue"] = "Mora biti veće od 0.";
            }
            if (int.Parse(EnergyValue) > 5)
            {
                this.ValidationErrors["EnergyValue"] = "Mora biti manje od 5.";
            }
        }
    }
}
