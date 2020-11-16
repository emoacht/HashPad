using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HashPad.Views.Controls
{
	public class MenuCaptionIcon : CaptionIcon
	{
		protected override void Draw(DrawingContext drawingContext, double factor)
		{
			var line = new Pen(Stroke, Math.Round(StrokeThickness * factor, MidpointRounding.AwayFromZero) / factor);
			var lineRadius = line.Thickness / 2D;

			var topCenterY = StrokeThickness / 2D; // Y coordinate of top line's center
			var topStartPoint = new Point(0, topCenterY - lineRadius);
			var topEndPoint = new Point(Width, topCenterY - lineRadius);

			var middleCenterY = Height / 2D; // Y coordinate of middle line's center
			var middleStartPoint = new Point(0, middleCenterY - lineRadius);
			var middleEndPoint = new Point(Width, middleCenterY - lineRadius);

			var bottomCenterY = Height - (StrokeThickness / 2D); // Y coordinate of bottom line's center
			var bottomStartPoint = new Point(0, bottomCenterY - lineRadius);
			var bottomEndPoint = new Point(Width, bottomCenterY - lineRadius);

			// Create a guidelines set.
			var guidelines = new GuidelineSet();
			guidelines.GuidelinesY.Add(topStartPoint.Y);
			guidelines.GuidelinesY.Add(middleStartPoint.Y);
			guidelines.GuidelinesY.Add(bottomStartPoint.Y);

			drawingContext.PushGuidelineSet(guidelines);

			// Draw lines.
			drawingContext.DrawLine(line, topStartPoint, topEndPoint);
			drawingContext.DrawLine(line, middleStartPoint, middleEndPoint);
			drawingContext.DrawLine(line, bottomStartPoint, bottomEndPoint);

			drawingContext.Pop();
		}
	}
}