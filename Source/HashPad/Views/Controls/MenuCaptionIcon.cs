using System;
using System.Windows;
using System.Windows.Media;

namespace HashPad.Views.Controls;

public class MenuCaptionIcon : CaptionIcon
{
	protected override void Draw(DrawingContext drawingContext, double factor)
	{
		var line = new Pen(Stroke, Math.Round(StrokeThickness * factor, MidpointRounding.AwayFromZero) / factor);
		var lineRadius = line.Thickness / 2D;

		var topCenterY = lineRadius; // Y coordinate of upper line's center
		var topStartPoint = new Point(0, topCenterY - lineRadius);
		var topEndPoint = new Point(Width, topCenterY - lineRadius);

		var middleCenterY = Height / 2D; // Y coordinate of middle line's center
		var middleStartPoint = new Point(0, middleCenterY - lineRadius);
		var middleEndPoint = new Point(Width, middleCenterY - lineRadius);

		var bottomCenterY = Height - lineRadius; // Y coordinate of lower line's center
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