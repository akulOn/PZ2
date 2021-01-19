using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class CustomMessenger : Messenger
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public string Energy { get; set; }
        public CustomMessenger(int id, string name, string energy, GeneratorType type)
        {
            Id = id;
            Name = name;
            Energy = energy;

            if (type == GeneratorType.Solarni_Panel)
                ImageSource = "/Images/solarni_panel.jpg";
            else if (type == GeneratorType.Vetrogenerator)
                ImageSource = "/Images/vetrogenerator.jpg";

            if (energy == "Nevažeća dolazna vrednost!")
                ImageSource = "/Images/error.jpg";

            if (energy == "Privremena energetska vrednost.")
                ImageSource = "/Images/temporary.jpg";
        }
    }
}
