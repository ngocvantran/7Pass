using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public partial class AttachmentDetails
    {
        private double _initialScale;

        public AttachmentDetails()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var db = Cache.Database;
            if (db == null)
            {
                this.BackToDBs();
                return;
            }

            var queries = NavigationContext.QueryString;
            var entry = db.GetEntry(queries["id"]);
            var attachment = entry.Attachments[
                Convert.ToInt32(queries["att"])];

            border.Background = new ImageBrush
            {
                ImageSource = GetImage(
                    attachment.Value),
            };
        }

        private static ImageSource GetImage(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (bytes.Length == 0)
                return null;

            var knownHeaders = new[]
            {
                new byte[] {0x42, 0x4D}, // BMP
                new byte[] {0xFF, 0xD8}, // JPEG
                new byte[] {0x47, 0x49, 0x46}, // GIF
                new byte[] {0x89, 0x50, 0x4E, 0x47}, // PNG
            };

            var isKnownType = knownHeaders
                .Any(x => bytes
                    .Take(x.Length)
                    .SequenceEqual(x));

            if (!isKnownType)
                return null;

            try
            {
                using (var buffer = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.SetSource(buffer);

                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        private void OnDragDelta(object sender,
            DragDeltaGestureEventArgs e)
        {
            transform.TranslateX += e.HorizontalChange;
            transform.TranslateY += e.VerticalChange;
        }

        private void OnPinchDelta(object sender,
            PinchGestureEventArgs e)
        {
            transform.ScaleX = transform.ScaleY =
                _initialScale*e.DistanceRatio;
        }

        private void OnPinchStarted(object sender,
            PinchStartedGestureEventArgs e)
        {
            _initialScale = transform.ScaleX;
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            transform.ScaleX = transform.ScaleY = 1;
            transform.TranslateX = transform.TranslateY = 0;
        }
    }
}