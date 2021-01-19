using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class DisplayDER : DER
    {
        private string imageSource;

        public DisplayDER()
        {

        }
        public DisplayDER(DER der)
        {
            Id = der.Id;
            Name = der.Name;
            EnergyValue = der.EnergyValue;
            Type = der.Type;

            if (Type == GeneratorType.Solarni_Panel)
                ImageSource = "/Images/solarni_panel.jpg";
            else if (Type == GeneratorType.Vetrogenerator)
                ImageSource = "/Images/vetrogenerator.jpg";

            if (EnergyValue == "Nevažeća dolazna vrednost!")
                ImageSource = "/Images/error.jpg";

            if (EnergyValue == "Privremena energetska vrednost.")
                ImageSource = "/Images/temporary.jpg";
        }
        public string ImageSource
        {
            get { return imageSource; }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged("ImageSource");
                }
            }
        }
        public void ChanageImage()
        {
            if (Type == GeneratorType.Solarni_Panel)
                ImageSource = "/Images/solarni_panel.jpg";
            else if (Type == GeneratorType.Vetrogenerator)
                ImageSource = "/Images/vetrogenerator.jpg";

            if (EnergyValue == "Nevažeća dolazna vrednost!")
                ImageSource = "/Images/error.jpg";

            if (EnergyValue == "Privremena energetska vrednost.")
                ImageSource = "/Images/temporary.jpg";
        }
    }
}
