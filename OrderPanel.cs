using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class OrderPanel : UserControl
    {
        private PurchasePanel.Pricing _Pricing;

        public OrderPanel()
        {
            InitializeComponent();
            AdministrationsDrop.SelectedIndexChanged += new EventHandler(CalcTotal);
            DiskSpaceDrop.SelectedIndexChanged += new EventHandler(CalcTotal);
            IATsDrop.SelectedIndexChanged += new EventHandler(CalcTotal);
        }

        public void SetPricing(PurchasePanel.Pricing pricing)
        {
            _Pricing = pricing;
            this.Invoke((Action<ComboBox, String>)AddToDrop, AdministrationsDrop, "0");
            foreach (PurchasePanel.ResourcePrice rp in pricing.AdministrationPrices)
                this.Invoke((Action<ComboBox, String>)AddToDrop, AdministrationsDrop, rp.Quantity.ToString());
            this.Invoke((Action<ComboBox, String>)AddToDrop, DiskSpaceDrop, "0MB");
            foreach (PurchasePanel.ResourcePrice rp in pricing.DiskSpacePrices)
                this.Invoke((Action<ComboBox, String>)AddToDrop, DiskSpaceDrop, rp.Quantity.ToString() + "MB");
            this.Invoke((Action<ComboBox, String>)AddToDrop, IATsDrop, "0");
            foreach (PurchasePanel.ResourcePrice rp in pricing.IATPrices)
                this.Invoke((Action<ComboBox, String>)AddToDrop, IATsDrop, rp.Quantity.ToString());
        }

        private void AddToDrop(ComboBox drop, String val)
        {
            drop.Items.Add(val);
        }

        private void CalcTotal(object sender, EventArgs e)
        {
            int adminPrice = (AdministrationsDrop.SelectedIndex < 1) ? 0 : _Pricing.AdministrationPrices[AdministrationsDrop.SelectedIndex - 1].Price;
            int diskPrice = (DiskSpaceDrop.SelectedIndex < 1) ? 0 : _Pricing.DiskSpacePrices[DiskSpaceDrop.SelectedIndex - 1].Price;
            int iatPrice = (IATsDrop.SelectedIndex < 1) ? 0 : _Pricing.IATPrices[IATsDrop.SelectedIndex - 1].Price;
            TotalBox.Text = String.Format("{0:C}", adminPrice + diskPrice + iatPrice);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            ((IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName]).FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void PurchaseButton_Click(object sender, EventArgs e)
        {
            PurchaseOrder pOrder = new PurchaseOrder();
            if (AdministrationsDrop.SelectedIndex > 0)
                pOrder.NumAdministrations = _Pricing.AdministrationPrices[AdministrationsDrop.SelectedIndex - 1].Quantity;
            if (DiskSpaceDrop.SelectedIndex > 0)
                pOrder.DiskSpace = _Pricing.DiskSpacePrices[DiskSpaceDrop.SelectedIndex - 1].Quantity;
            if (IATsDrop.SelectedIndex > 0)
                pOrder.NumTests = _Pricing.DiskSpacePrices[IATsDrop.SelectedIndex - 1].Quantity;
            int adminPrice = (AdministrationsDrop.SelectedIndex < 1) ? 0 : _Pricing.AdministrationPrices[AdministrationsDrop.SelectedIndex - 1].Price;
            int diskPrice = (DiskSpaceDrop.SelectedIndex < 1) ? 0 : _Pricing.DiskSpacePrices[DiskSpaceDrop.SelectedIndex - 1].Price;
            int iatPrice = (IATsDrop.SelectedIndex < 1) ? 0 : _Pricing.IATPrices[IATsDrop.SelectedIndex - 1].Price;
            pOrder.Total = adminPrice + diskPrice + iatPrice; 
            ((PurchasePanel)Parent).SendPurchase(pOrder);
        }


    }
}
