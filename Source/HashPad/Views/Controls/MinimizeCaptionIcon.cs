using System;
using System.Windows;
using System.Windows.Media;

namespace HashPad.Views.Controls;

public class MinimizeCaptionIcon : CaptionIcon
{
	protected override void Draw(DrawingContext drawingContext, double factor)
	{
		var line = new Pen(Stroke, Math.Round(StrokeThickness * factor, MidpointRounding.AwayFromZero) / factor);
		var lineRadius = line.Thickness / 2D;

		var middleCenterY = Height / 2D; // Y coordinate of middle line's center
		var middleStartPoint = new Point(0, middleCenterY - lineRadius);
		var middleEndPoint = new Point(Width, middleCenterY - lineRadius);

		// Create a guidelines set.
		var guidelines = new GuidelineSet();
		guidelines.GuidelinesY.Add(middleStartPoint.Y);

		drawingContext.PushGuidelineSet(guidelines);

		// Draw line.
		drawingContext.DrawLine(line, middleStartPoint, middleEndPoint);

		drawingContext.Pop();
	}
}