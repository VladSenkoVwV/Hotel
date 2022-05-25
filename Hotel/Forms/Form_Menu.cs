using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotel.HotelDb;
using Hotel.MyClasses;

namespace Hotel.Forms
{
    public partial class Form_Menu : Form
    {
        public Form_Menu()
        {
            InitializeComponent();

            MyForms.Form_Menu = this;

            using (HotelContext hotel = new HotelContext())
            {
                DateTime today = DateTime.Today;  

                Action<ClientCard> checkPaid = card =>
                {
                    if (card.Client == null)
                    {
                        return;
                    }

                    decimal mustPaid = MyMethods.ClientCardAmountOfPayment(card) +
                        MyMethods.ClientCardServicesCost(card);
                    Client client = card.Client;
                    List<ArchivalRecord> archivalRecords = hotel.ArchivalRecords
                        .Where(record =>
                        record.ClientDocSeries == client.DocSeries &&
                        record.ClientDocNumber == client.DocNumber).ToList();

                    if (card.DepartureDate < today)
                    {
                        if (card.Paid > mustPaid)
                        {
                            card.Paid = mustPaid;
                            archivalRecords
                                .First(record => record.ArrivalDate == card.ArrivalDate).ClientPaid = mustPaid;
                        }

                        if (card.Paid == mustPaid)
                        {
                            hotel.ClientsCards.Remove(card);

                            hotel.SaveChanges();

                            return;
                        }

                        client.Status = true;
                        hotel.ClientsCards.RemoveRange(client.ClientCards);
                        hotel.ArchivalRecords.RemoveRange(archivalRecords
                            .Where(record => record.ArrivalDate >= card.ArrivalDate));

                        hotel.SaveChanges();
                    }
                };

                hotel.ClientsCards.OrderBy(card => card.ArrivalDate).ToList().ForEach(checkPaid);

                hotel.SaveChanges();
            }
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            MyForms.Form_Menu.Close();
        }

        private void button_Administration_Click(object sender, EventArgs e)
        {
            MyForms.Form_Admin = new Form_Admin();
            MyForms.Form_Admin.Show();
            MyForms.Form_Menu.Hide();
        }

        private void button_Clients_Click(object sender, EventArgs e)
        {
            MyForms.Form_ClientsEntry = new Form_ClientsEntry();
            MyForms.Form_ClientsEntry.Show();
            MyForms.Form_Menu.Hide();
        }
    }
}
