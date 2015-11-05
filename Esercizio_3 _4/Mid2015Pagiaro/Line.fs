module Line

open System.Drawing
open System.Windows.Forms
open System
open System.Windows
open ap
open Tratti
type Line() as this =
    inherit Tratti()
    
    let random = Random()
    let mutable startPoint = PointF()
    let mutable endPoint = PointF()
    let mutable selectedPoint = -1
    let mutable leftTop = PointF()
    let maniglie = [|PointF(); PointF(); PointF(); PointF()|]

    let drawCircle (center:PointF) (radius:single) (g:Graphics) =
        let rectangle = RectangleF(center.X-radius, center.Y-radius, 2.f*radius, 2.f*radius)
        g.DrawEllipse(Pens.Black, rectangle)
    
    let drawRotate (center:PointF) (size:single) (g:Graphics) =
        use p = new Pen(Color.Black)
        p.EndCap <- Drawing2D.LineCap.ArrowAnchor
        let rectangle = Rectangle(int (center.X-size/2.f), int(center.Y-size/2.f),int size,int size)
        g.DrawArc(p, rectangle, 0.f, 320.f)
       

    let drawTranslate (center:PointF) (size:single) (g:Graphics) =
        let p = new Pen(Color.Black)
        p.EndCap <- Drawing2D.LineCap.ArrowAnchor
        g.DrawLine(p, center, PointF(center.X, center.Y + size/2.f))
        g.DrawLine(p, center, PointF(center.X, center.Y - size/2.f))
        g.DrawLine(p, center, PointF(center.X - size/2.f, center.Y))
        g.DrawLine(p, center, PointF(center.X + size/2.f, center.Y))

    let updateManigliePosition (leftTop:PointF) =
        maniglie.[0] <- startPoint
        maniglie.[2] <- PointF(endPoint.X,startPoint.Y)
        maniglie.[1] <- PointF(startPoint.X, endPoint.Y)
        maniglie.[3] <- endPoint

    let testManiglie (pt:PointF) =
        let mutable index = -1
        for i in {0 .. 1 .. 3} do
            let center = maniglie.[i]
            let size = 10.f
            let rectangle = RectangleF(center.X-size/2.f, center.Y-size/2.f, size, size)
            if rectangle.Contains(pt) then
                index <- i
        index

    member this.StartPoint with get() = startPoint and set(v) = startPoint <- v
    member this.EndPoint with get() = endPoint and set(v) = endPoint <- v

    override this.OnPaint (e:PaintEventArgs) =
        if this.Path <> null then this.Path.Dispose()
        this.Path <- new Drawing2D.GraphicsPath()
        this.Path.AddLine(startPoint, endPoint)
        this.Path.Transform(this.ShapeToSheet)
        e.Graphics.DrawPath(this.Pen, this.Path)
        
        let save = e.Graphics.Save()
        e.Graphics.MultiplyTransform(this.ShapeToSheet)
        if this.Selected then
            use p = new Pen(Color.Black)
            p.DashPattern <- [|4.f; 2.f; 1.f; 3.f|];
            e.Graphics.DrawRectangle(p, leftTop.X, leftTop.Y, this.Size.Width, this.Size.Height)
            drawCircle startPoint 5.f e.Graphics 
            drawCircle endPoint 5.f e.Graphics
            //drawRotate (PointF(startPoint.X, endPoint.Y)) 10.f e.Graphics
            drawTranslate (PointF(endPoint.X,startPoint.Y)) 10.f e.Graphics
        e.Graphics.Restore(save)

    override this.OnMouseDown e =
        let pt = ap.PointF e.Location
        let pt = ap.transformP this.SheetToShape e.Location
        leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
        this.Size <- SizeF(Math.Max(startPoint.X, endPoint.X)-leftTop.X, Math.Max(startPoint.Y, endPoint.Y)-leftTop.Y)
        updateManigliePosition leftTop
        selectedPoint <- testManiglie pt

    override this.OnMouseMove e =
        let pt = ap.transformP this.SheetToShape e.Location
        if this.Selected then
            match selectedPoint with
            | 0 -> 
                startPoint <- pt
                leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
                this.Size <- SizeF(Math.Max(startPoint.X, endPoint.X)-leftTop.X, Math.Max(startPoint.Y, endPoint.Y)-leftTop.Y)
                updateManigliePosition leftTop
            | 2 -> 
                let manigliaPt = maniglie.[2]
                this.ShapeToSheet.Translate(-manigliaPt.X + pt.X, -manigliaPt.Y + pt.Y)
                this.SheetToShape.Translate(manigliaPt.X - pt.X, manigliaPt.Y - pt.Y, Drawing2D.MatrixOrder.Append)
            | 3 -> 
                endPoint <- pt
                leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
                this.Size <- SizeF(Math.Max(startPoint.X, endPoint.X)-leftTop.X, Math.Max(startPoint.Y, endPoint.Y)-leftTop.Y)
                updateManigliePosition leftTop
            | _ -> ()
        else
            this.EndPoint <- pt
            leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
            this.Size <- SizeF(Math.Max(startPoint.X, endPoint.X)-leftTop.X, Math.Max(startPoint.Y, endPoint.Y)-leftTop.Y)
            updateManigliePosition leftTop
    
    override this.OnMouseUp e =
        if this.Selected then
            selectedPoint <- -1

    override this.HitTest clickP =
        let mutable result = false
        if (this.Path <> null) then
            use p = new Pen(Color.Black, 5.f)
            if this.Path.IsOutlineVisible(clickP, p) then
                result <- true
        result