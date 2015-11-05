module Tratti

open System.Drawing
open System.Windows.Forms
open System
open ap

type Tratti() =
    
    let mutable size = SizeF()
    let mutable selected = false
    let random = Random()
    let mutable (path:Drawing2D.GraphicsPath) = null
    //Evito colori troppo chiari e troppo scuri
    let mutable p = new Pen(Color.FromArgb(random.Next(20, 250), random.Next(20,250),random.Next(20,250)), 1.f)
    let m_SheetToShape = new Drawing2D.Matrix()
    let m_ShapeToSheet = new Drawing2D.Matrix()

    member this.Size with get() = size and set(v) = size <- v
    member this.PenSize with set(v) = p.Width <- v and get() = p.Width
    member this.Path
        with get() = path
        and set(v) = path <- v
    member this.SheetToShape with get() = m_SheetToShape
    member this.ShapeToSheet with get() = m_ShapeToSheet
    

    abstract OnMouseDown: MouseEventArgs -> unit
    default this.OnMouseDown e =
       ()

    abstract OnMouseMove: MouseEventArgs -> unit
    default this.OnMouseMove e =
       ()
    abstract OnMouseUp: MouseEventArgs -> unit
    default this.OnMouseUp e =
       ()

    abstract OnPaint: PaintEventArgs -> unit
    default this.OnPaint e =
       ()

    abstract HitTest: PointF -> bool
    default this.HitTest pt =
        false

    member this.Selected with get() = selected and set(v) = selected <- v
    member this.Pen with get() = p and set(v) = p <- v
