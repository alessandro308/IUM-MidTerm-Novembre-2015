module Bezier

open System.Drawing
open System.Windows.Forms
open System
open ap
open Tratti
type Bezier() =
    inherit Tratti()

    //let random = Random()
    let mutable startPoint = PointF()
    let mutable controlPoint1 = startPoint
    let mutable endPoint = PointF()
    let mutable controlPoint2 = endPoint
    let mutable endPoint = PointF()
    let mutable selectedPoint = -1
    //let mutable (path:Drawing2D.GraphicsPath) = null
    //let mutable p = new Pen(Color.FromArgb(random.Next(0, 255), random.Next(0,255),random.Next(0,255)), 1.f)

    let drawCircle (center:PointF) (radius:single) (g:Graphics) =
        let rectangle = RectangleF(center.X-radius, center.Y-radius, 2.f*radius, 2.f*radius)
        g.DrawEllipse(Pens.Black, rectangle)

    member this.StartPoint with get() = startPoint and set(v) = startPoint <- v; controlPoint1 <- PointF(v.X + 50.f, v.Y + 50.f)
    member this.EndPoint with get() = endPoint and set(v) = endPoint <- v; controlPoint2 <- PointF(v.X +  50.f, v.Y + 50.f)
    member this.ControlPoint1 with get() = controlPoint1 and set(v) = controlPoint1 <- v
    member this.ControlPoint2 with get() = controlPoint2 and set(v) = controlPoint2 <- v
    //member this.Pen with get() = p and set(v) = p <- v

    override this.OnPaint (e:PaintEventArgs) =
        if this.Path <> null then this.Path.Dispose()
        this.Path <- new Drawing2D.GraphicsPath()
        this.Path.AddBezier(startPoint, controlPoint1, controlPoint2, endPoint)
        e.Graphics.DrawPath(this.Pen, this.Path)
        if this.Selected then
            drawCircle startPoint 5.f e.Graphics
            drawCircle endPoint 5.f e.Graphics
            drawCircle controlPoint1 5.f e.Graphics
            drawCircle controlPoint2 5.f e.Graphics
            use p = new Pen(Color.Black)
            p.DashPattern <- [|4.f; 2.f; 1.f; 3.f|];
            e.Graphics.DrawLine(p, startPoint, controlPoint1)
            e.Graphics.DrawLine(p, endPoint, controlPoint2)

    override this.OnMouseDown e =
        let pt = ap.PointF e.Location
        (*Scusi la serie di IF ma non trovavo una maniera più elegante*)
        if (startPoint.X - pt.X)**2.f + (startPoint.Y - pt.Y)**2.f < 25.f then
            selectedPoint <- 0
        else if (endPoint.X - pt.X)**2.f + (endPoint.Y - pt.Y)**2.f < 25.f then
            selectedPoint <- 3
            else if (controlPoint1.X - pt.X)**2.f + (controlPoint1.Y - pt.Y)**2.f < 25.f then
                selectedPoint <- 1
                else if (controlPoint2.X - pt.X)**2.f + (controlPoint2.Y - pt.Y)**2.f < 25.f then
                    selectedPoint <- 2
                    else 
                        selectedPoint <- -1

    override this.OnMouseMove e =
        if this.Selected then
            match selectedPoint with
            | 0 -> let tmp = startPoint
                   startPoint <- PointF(single e.Location.X, single e.Location.Y)
                   controlPoint1 <- PointF(controlPoint1.X + startPoint.X - tmp.X, controlPoint1.Y + startPoint.Y - tmp.Y)
            | 1 -> controlPoint1 <- PointF(single e.Location.X, single e.Location.Y)
            | 2 -> controlPoint2 <- PointF(single e.Location.X, single e.Location.Y)
            | 3 -> let tmp = endPoint
                   endPoint <- PointF(single e.Location.X, single e.Location.Y)
                   controlPoint2 <- PointF(controlPoint2.X + endPoint.X - tmp.X, controlPoint2.Y + endPoint.Y - tmp.Y)
            | _ -> ()
        else
            this.EndPoint <- PointF(single e.Location.X, single e.Location.Y)

    override this.OnMouseUp e =
        if this.Selected then
            selectedPoint <- -1

    override this.HitTest pt =
        let mutable result = false
        (*GUIDA PER HIT TEST: http://geekswithblogs.net/Perspectives/archive/2007/06/28/windows-forms-hit-testing.aspx*)
        if (this.Path <> null) then
            use p = new Pen(Color.Black, 5.f)
            if this.Path.IsOutlineVisible(pt, p) then
                result <- true
        result