﻿using System.Drawing;
using CloudLayout.Interfaces;
using ResultOfTask;

namespace CloudLayout;

public class CircularCloudLayout
{
    private readonly Point center;
    private readonly int radius;
    private readonly IEnumerable<PointF> spiralPoints;
    private readonly List<RectangleF> placedRectangles;

    public CircularCloudLayout(ISpiralDrawer drawer, IInputOptions options)
    {
        center = options.CenterPoint;
        if (center.X < 1)
            throw new ArgumentException("Center point X is zero or less");
        if (center.Y < 1)
            throw new ArgumentException("Center point Y is zero or less");
        radius = center.X < center.Y ? center.X : center.Y;
        placedRectangles = new();
        spiralPoints = drawer.GetSpiralPoints(center);
    }

    public Result<RectangleF> PutNextRectangle(SizeF size)
    {
        var rectangle = new RectangleF();
        if (!ValidateSize(size))
            throw new ArgumentException("Word size dimension is zero or less");
        if (placedRectangles.Count == 0)
            return TryPlaceRectangleInCenter(out rectangle, size) 
                ? rectangle.AsResult() 
                : Result.Fail<RectangleF>("Can't be placed in layout");

        foreach (var point in spiralPoints)
        {
            if (PointLiesInRectangles(point))
                continue;
            rectangle = TryPlaceRectangle(point, size);
            if (rectangle.IsEmpty)
                continue;
            OffsetRectangle(ref rectangle);
            break;
        }

        if (rectangle.IsEmpty)
            return Result.Fail<RectangleF>("Can't be placed in layout");
        placedRectangles.Add(rectangle);
        return rectangle.AsResult();
    }

    private void OffsetRectangle(ref RectangleF rectangle)
    {
        var canOffsetX = true;
        var canOffsetY = true;
        while (canOffsetY || canOffsetX)
        {
            canOffsetX = rectangle.GetCenter().X > center.X
                ? TryOffSet(ref rectangle, -1, 0, (rectangle) => rectangle.GetCenter().X < center.X)
                : TryOffSet(ref rectangle, 1, 0, (rectangle) => rectangle.GetCenter().X > center.X);
            canOffsetY = rectangle.GetCenter().Y > center.Y
                ? TryOffSet(ref rectangle, 0, -1, (rectangle) => rectangle.GetCenter().Y < center.Y)
                : TryOffSet(ref rectangle, 0, 1, (rectangle) => rectangle.GetCenter().Y > center.Y);
        }
    }

    private bool TryOffSet(ref RectangleF rectangle, int x, int y, Func<RectangleF, bool> closeToCenter)
    {
        var buffer = rectangle;
        buffer.Offset(x, y);
        if (closeToCenter(buffer))
            return false;
        if (RectangleIntersects(buffer))
            return false;
        rectangle = buffer;
        return true;
    }

    private RectangleF AdjustRectanglePosition(PointF point, SizeF size)
    {
        var x = point.X > center.X ? point.X : point.X - size.Width;
        var y = point.Y > center.Y ? point.Y : point.Y - size.Height;
        return new RectangleF(new PointF(x, y), size);
    }

    private RectangleF TryPlaceRectangle(PointF pointer, SizeF size)
    {
        var rectangle = AdjustRectanglePosition(pointer, size);
        if (!RectangleIntersects(rectangle) && !RectangleOutOfCircleRange(rectangle))
            return rectangle;
        return new RectangleF();
    }

    private bool TryPlaceRectangleInCenter(out RectangleF rectangle, SizeF size)
    {
        rectangle = new RectangleF(
            new PointF(center.X - size.Width / 2, center.Y - size.Height / 2),
            size);
        if (RectangleOutOfCircleRange(rectangle))
            return false;
        placedRectangles.Add(rectangle);
        return true;
    }

    private bool PointLiesInRectangles(PointF p) => placedRectangles.Any(x => x.Contains(p));

    private bool RectangleIntersects(RectangleF rectangle) =>
        placedRectangles.Any(x => x.IntersectsWith(rectangle));

    private bool RectangleOutOfCircleRange(RectangleF rectangle)
    {
        var x1 = rectangle.Left - center.X;
        var y1 = rectangle.Top - center.Y;
        var x2 = rectangle.Right - center.X;
        var y2 = rectangle.Bottom - center.Y;
        return Math.Sqrt(x1 * x1 + y1 * y1) > radius
               || Math.Sqrt(x2 * x2 + y1 * y1) > radius
               || Math.Sqrt(x1 * x1 + y2 * y2) > radius
               || Math.Sqrt(x2 * x2 + y2 * y2) > radius;
    }

    private static bool ValidateSize(SizeF size) => size.Height > 0 && size.Width > 0;
}