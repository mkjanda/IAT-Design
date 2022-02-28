using IATClient.Images;
using System;

namespace IATClient
{
    abstract public class DIComposite : DIGenerated
    {
        private IImageDisplay _PreviewPanel = null;
        public override bool IsComposite { get { return true; } }
        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if (value == _PreviewPanel)
                    return;
                _PreviewPanel = value;
                if (value == null)
                {
                    SuspendLayout();
                    return;
                }
                if (!value.IsHandleCreated)
                    value.HandleCreated += (sender, args) => value.SetImage(IImage);
                else
                    PreviewPanel.SetImage(IImage);
                ResumeLayout(true);
            }
        }

        public DIComposite() : base()
        {
            SuspendLayout();
        }

        public DIComposite(Uri uri) : base(uri)
        {
            SuspendLayout();
        }

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if ((evt == Images.ImageEvent.Updated) || (evt == ImageEvent.Initialized))
                if (PreviewPanel != null)
                    if (!PreviewPanel.IsDisposed)
                        PreviewPanel.SetImage(IImage);
        }


        public abstract bool ComponentsExist { get; }
        public abstract bool ComponentsStale { get; }
    }
}
