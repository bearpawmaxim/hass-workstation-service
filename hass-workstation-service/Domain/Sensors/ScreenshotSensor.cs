using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class ScreenshotSensor : AbstractSensor
	{
		private readonly Rectangle _bounds;
		private decimal _scaleFactor;
		
		public decimal ScaleFactor => _scaleFactor;
		private bool Resize => _scaleFactor != 1.0m && _scaleFactor != 0.0m;

		public ScreenshotSensor(MqttPublisher publisher, string name, int? updateInterval = null, Guid id = default,
				decimal scaleFactor = 1.0m) :
			base(publisher, name ?? "Screenshot", updateInterval ?? 60, id) {
			Domain = "camera";
			var width = GetSystemMetrics(0);
			var height = GetSystemMetrics(1);
			_scaleFactor = scaleFactor;
			_bounds = new Rectangle(0, 0, width, height);
		}

		public override string Domain { get; }

		private static Bitmap ResizeImage(Image image, int width, int height) {
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);
			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			using var graphics = Graphics.FromImage(destImage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			using var wrapMode = new ImageAttributes();
			wrapMode.SetWrapMode(WrapMode.TileFlipXY);
			graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
			return destImage;
		}

		public override DiscoveryConfigModel GetAutoDiscoveryConfig() {
			var topic =
				$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state";
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new CameraDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Image_encoding = "b64",
				Encoding = "utf-8",
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				Topic = topic,
				State_topic = topic,
				Icon = "mdi:screen",
				Availability_topic = $"homeassistant/sensor/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			using (var bitmap = new Bitmap(_bounds.Width, _bounds.Height)) {
				using (var g = Graphics.FromImage(bitmap)) {
					g.CopyFromScreen(Point.Empty, Point.Empty, _bounds.Size);
				}

				using (var ms = new MemoryStream()) {
					var resized = Resize
						? ResizeImage(bitmap,
							(int)(_bounds.Width * _scaleFactor),
							(int)(_bounds.Height  * _scaleFactor))
						: bitmap;
					resized.Save(ms, ImageFormat.Jpeg);
					return Convert.ToBase64String(ms.ToArray());
				}
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetSystemMetrics(int nIndex);
	}

}