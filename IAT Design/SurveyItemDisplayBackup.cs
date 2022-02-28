using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    abstract class SurveyItemDisplayBackup : UserControl
    {
        private bool _IsActive;
        protected Font DisplayFont;

        public abstract bool Selected { get; set; }

        public bool Active
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if (value != _IsActive)
                {
                    OnActivate(value);
                }
                _IsActive = value;
            }
        }

        public SurveyItemDisplayBackup()
        {
            _IsActive = false;
            this.MouseEnter += new EventHandler(SurveyItemDisplay_MouseEnter);
            this.Paint += new PaintEventHandler(SurveyItemDisplay_Paint);
        }

        protected abstract void SurveyItemDisplay_Paint(object sender, PaintEventArgs e);

        void SurveyItemDisplay_MouseEnter(object sender, EventArgs e)
        {
            Active = true;
        }

        protected virtual void OnActivate(bool BecomingActive)
        {
            if ((BecomingActive == true) && (Parent != null))
                ((SurveyDisplay)Parent).ClearActiveQuestion();
        }

        public abstract void RecalcSize();
    }
}
