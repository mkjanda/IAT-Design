using System;
using System.Drawing;
using System.Collections.Generic;
using IATClient.Images;

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
                    ResumeLayout(false);
                _PreviewPanel = value;
                if (value == null)
                {
                    SuspendLayout();
                    return;
                }
                ResumeLayout(true);
            }
        }

        public DIComposite() : base() {
        }

        public DIComposite(Uri uri) : base(uri)
        {
        }

        protected override void OnImageChanged(ImageChangedEvent evt, IImageMedia img, object arg)
        {
            base.OnImageChanged(evt, img, arg);
            if ((evt == Images.ImageChangedEvent.Updated) || (evt == ImageChangedEvent.Initialized))
                if (PreviewPanel != null)
                    if (!PreviewPanel.IsDisposed)
                        PreviewPanel.SetImage(img);
//            Validate();
        }

        public abstract void ReleaseDI(Uri uri);
        public abstract bool ComponentsExist { get; }
        public abstract bool ComponentsValid { get; }
        public abstract bool ComponentsStale { get; }
    }
}
