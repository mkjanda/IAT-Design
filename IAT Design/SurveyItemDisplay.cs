using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public interface ISurveyItemDisplay
    {
        bool Selected { get; set; }
        bool IsUnique { get; set; }
        bool Active { get; set; }
        CSurveyItem SurveyItem { get; set; }
        Point Location { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        Size Size { get; set; }
        Task<int> RecalcSize(bool recalcChildren);
    }


    abstract partial class SurveyItemDisplay : UserControl, ISurveyItemDisplay
    {
        private bool _IsActive;
        public abstract bool Selected { get; set; }
        public abstract bool IsUnique { get; set; }
        public abstract CSurveyItem SurveyItem { get; set; }
        protected CSurvey Survey { get; set; }

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

        public SurveyItemDisplay()
        {
            _IsActive = false;
            this.MouseEnter += new EventHandler(SurveyItemDisplay_MouseEnter);
            this.Paint += new PaintEventHandler(SurveyItemDisplay_Paint);
            InitializeComponent();
        }

        protected abstract void SurveyItemDisplay_Paint(object sender, PaintEventArgs e);

        void SurveyItemDisplay_MouseEnter(object sender, EventArgs e)
        {
            Active = true;
            Invalidate();
        }

        protected virtual void OnActivate(bool BecomingActive)
        {
            if ((BecomingActive == true) && (Parent != null))
                ((SurveyDisplay)Parent).ClearActiveQuestion();
        }

        public abstract Task<int> RecalcSize(bool recalcChildren);

    }
}
