using GalaSoft.MvvmLight.Messaging;
using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkService
{
    public class MainWindowViewModel : BindableBase
    {
        private int count = 12; // Inicijalna vrednost broja objekata u sistemu
                                // ######### ZAMENITI stvarnim brojem elemenata
                                //           zavisno od broja entiteta u listi

        private DER incommingDER;

        public MyICommand<string> NavCommand { get; private set; }
        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();
        private BindableBase currentViewModel;

        public MainWindowViewModel()
        {
            createListener(); //Povezivanje sa serverskom aplikacijom

            NavCommand = new MyICommand<string>(OnNav);
            CurrentViewModel = networkEntitiesViewModel;
        }
        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "networkEntities":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "networkDisplay":
                    CurrentViewModel = networkDisplayViewModel;
                    break;
                case "measurementGraph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;
            }
        }
        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(NetworkEntitiesViewModel.savedDERs.Count().ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                            incommingDER = ProcessIncomming(incomming);

                            using (StreamWriter writetext = new StreamWriter("log.txt"))
                            {
                                writetext.WriteLine(DateTime.Now + "/" + incomming);
                            }
                            Messenger.Default.Send<CustomMessenger>(new CustomMessenger(incommingDER.Id, incommingDER.Name, incommingDER.EnergyValue, incommingDER.Type));

                            if (NetworkEntitiesViewModel.DERs.ToList().FirstOrDefault(x => x.Id == incommingDER.Id) != null)
                            {
                                foreach(var item in NetworkEntitiesViewModel.DERs.ToList())
                                {
                                    if (item.Id == incommingDER.Id)
                                    {
                                        item.EnergyValue = incommingDER.EnergyValue;
                                        item.Name = incommingDER.Name;
                                    }
                                }
                                foreach (var item in NetworkEntitiesViewModel.savedDERs.ToList())
                                {
                                    if (item.Id == incommingDER.Id)
                                    {
                                        item.EnergyValue = incommingDER.EnergyValue;
                                        item.Name = incommingDER.Name;
                                    }
                                }
                            }

                            if (NetworkDisplayViewModel.DisplayDERs.ToList().FirstOrDefault(x => x.Id == incommingDER.Id) != null)
                            {
                                foreach (var item in NetworkDisplayViewModel.DisplayDERs.ToList())
                                {
                                    if (item.Id == incommingDER.Id)
                                    {
                                        item.EnergyValue = incommingDER.EnergyValue;
                                        item.Name = incommingDER.Name;
                                        item.ChanageImage();
                                    }
                                }
                            }

                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
        private DER ProcessIncomming(string incomming)
        {
            DER temp = new DER();

            temp.Id = int.Parse(incomming.Split(':')[0].Split('_')[1]);
            temp.Name = incomming.Split(':')[0];

            if (int.Parse(incomming.Split(':')[1]) >= 1 && int.Parse(incomming.Split(':')[1]) <= 5) // Validation
                temp.EnergyValue = incomming.Split(':')[1] + " MW";
            else
                temp.EnergyValue = "Nevažeća dolazna vrednost!";

            return temp;
        }
    }
}
