using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HashPad.Views.Controls
{
	public abstract class CaptionIcon : FrameworkElement
	{
		#region Property

		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}
		public static DependencyProperty StrokeThicknessProperty = Path.StrokeThicknessProperty.AddOwner(typeof(CaptionIcon));

		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}
		public static DependencyProperty StrokeProperty = Path.StrokeProperty.AddOwner(typeof(CaptionIcon));

		#endregion

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			SetDrawingFactor();
		}

		protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
		{
			base.OnDpiChanged(oldDpi, newDpi);

			SetDrawingFactor(newDpi);
		}

		private double _drawingFactor = 1D;

		private void SetDrawingFactor(DpiScale dpi = default)
		{
			if (dpi.Equals(default(DpiScale)))
				dpi = VisualTreeHelper.GetDpi(this);

			_drawingFactor = dpi.DpiScaleX;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			Draw(drawingContext, _drawingFactor);

			base.OnRender(drawingContext);
		}

		protected abstract void Draw(DrawingContext drawingContext, double factor);
	}
}