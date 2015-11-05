module Rect

open System.Drawing
open System.Windows
open System.Windows.Forms
open System
open Tratti
open ap
type Rect() as this =
    inherit Tratti()

   // let random = Random()
    let mutable startPoint = PointF()
    let mutable endPoint = PointF()
    let mutable leftTop = PointF()
    //let mutable (path : Drawing2D.GraphicsPath) = null
    //let mutable p = new Pen(Color.FromArgb(random.Next(0, 255), random.Next(0,255),random.Next(0,255)), 1.f)

    let maniglie = [|PointF(); PointF(); PointF(); PointF()|]
    let mutable selectedPoint = -1

    let drawSquare (center:PointF) (size:single) (g:Graphics) =
        let rectangle = Rectangle(int (center.X-size/2.f), int(center.Y-size/2.f),int size,int size)
        g.DrawRectangle(Pens.Black, rectangle)
    
    let drawResize (center:PointF) (size:single) (g:Graphics) =
        use p = new Pen(Color.Black)
        p.EndCap <- Drawing2D.LineCap.ArrowAnchor
        g.DrawLine(p, center.X, center.Y, center.X-size/2.f, center.Y-size/2.f)
        g.DrawLine(p, center.X, center.Y, center.X+size/2.f, center.Y+size/2.f)

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

    let testManiglie (pt:PointF) =
        let mutable index = -1
        for i in {0 .. 1 .. 3} do
            let center = maniglie.[i]
            let size = 10.f
            let rectangle = RectangleF(center.X-size/2.f, center.Y-size/2.f, size, size)
            if rectangle.Contains(pt) then
                index <- i
        index

    let updateManigliePosition (leftTop:PointF) =
        maniglie.[0] <- PointF(leftTop.X, leftTop.Y)
        maniglie.[1] <- PointF(leftTop.X + this.Size.Width, leftTop.Y)
        maniglie.[2] <- PointF(leftTop.X, leftTop.Y + this.Size.Height)
        maniglie.[3] <- PointF(leftTop.X + this.Size.Width, leftTop.Y + this.Size.Height)
        //leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
                

    member this.StartPoint with get() = startPoint and set(v) = startPoint <- v
    member this.EndPoint with get() = endPoint and set(v) = endPoint <- v
   // member this.Pen with get() = p and set(v) = p <- v

    override this.OnMouseDown e =
        //let pt = ap.PointF e.Location
        let pt = ap.transformP this.SheetToShape e.Location
        selectedPoint <- testManiglie pt

    override this.OnMouseMove e =

        let pt = ap.transformP this.SheetToShape e.Location

        if this.Selected then
            match selectedPoint with
            | 0 ->
                startPoint <- pt
                let width = Math.Abs(this.Size.Width - (pt.X - leftTop.X))
                let height = Math.Abs(this.Size.Height - (pt.Y - leftTop.Y))
                this.Size <- SizeF(width, height)
                endPoint <- PointF(startPoint.X + this.Size.Width, startPoint.Y + this.Size.Height)
                leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
                updateManigliePosition leftTop
            | 1 -> //ROTATE 
                let cx = leftTop.X + this.Size.Width/2.f
                let cy = leftTop.Y + this.Size.Height/2.f
                let v1 = new Vector(float (this.Size.Width/2.f), float (this.Size.Height/2.f))
                let v2 = new Vector(float(pt.X - cx), -float(pt.Y - cy) )
                let a = Vector.AngleBetween(v1, v2)
                this.ShapeToSheet.RotateAt(single -a, PointF(cx, cy))
                this.SheetToShape.RotateAt(single a, PointF(cx, cy), Drawing2D.MatrixOrder.Append)
                //updateManigliePosition leftTop this.Size.Width this.Size.Height
            | 3 -> // TRANSLATE
                let manigliaPt = PointF(leftTop.X + this.Size.Width, leftTop.Y + this.Size.Height)
                this.ShapeToSheet.Translate(-manigliaPt.X + pt.X, -manigliaPt.Y + pt.Y)
                this.SheetToShape.Translate(manigliaPt.X - pt.X, manigliaPt.Y - pt.Y, Drawing2D.MatrixOrder.Append)
                //updateManigliePosition leftTop this.Size.Width this.Size.Height
            | _ -> ()
        else //AVVIO
            endPoint <- PointF(single e.Location.X, single e.Location.Y)
            
            this.Size <- SizeF(Math.Abs(startPoint.X - endPoint.X),
                                Math.Abs(startPoint.Y - endPoint.Y))
            leftTop <- PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
            maniglie.[0] <- PointF(leftTop.X, leftTop.Y) //RESIZE
            maniglie.[1] <- PointF(leftTop.X + this.Size.Width, leftTop.Y) // ROTATE
            maniglie.[2] <- PointF(leftTop.X, leftTop.Y + this.Size.Height) 
            maniglie.[3] <- PointF(leftTop.X + this.Size.Width, leftTop.Y + this.Size.Height)// TTRANSLATE

    override this.OnMouseUp e =
        if this.Selected then
            selectedPoint <- -1

    override this.OnPaint (e:PaintEventArgs) =
        //Figura principale
        if this.Path <> null then this.Path.Dispose()
        this.Path <- new Drawing2D.GraphicsPath()
        let rect = new RectangleF(leftTop.X, leftTop.Y, this.Size.Width, this.Size.Height)
        this.Path.AddRectangle(rect)
        this.Path.Transform(this.ShapeToSheet)
        e.Graphics.DrawPath(this.Pen, this.Path)

        //Maniglie 
        let save = e.Graphics.Save()
        e.Graphics.MultiplyTransform(this.ShapeToSheet)
        if this.Selected then
                drawResize maniglie.[0] 10.f e.Graphics
                drawRotate maniglie.[1] 10.f e.Graphics
                drawTranslate maniglie.[3] 20.f e.Graphics
        e.Graphics.Restore(save)

    override this.HitTest e =
        let clickP = ap.transformPF this.SheetToShape e
        let mutable result = false
        if (this.Path <> null) then
            use p = new Pen(Color.Black, 5.f)
            if this.Path.IsOutlineVisible(clickP, p) then
                result <- true
        result
