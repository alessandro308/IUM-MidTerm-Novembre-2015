module Curva

open LWCs
open System.Drawing
open System.Windows.Forms
open System

type Curva() =
    inherit LWC()

    let pts = [| PointF(); PointF(20.f, 20.f); PointF(50.f, 50.f); PointF(50.f, 100.f) |]
    
    let mutable tension = 1.f
    let handleSize = 5.f
    let mutable selected = None
    let mutable offsetDrag = PointF()
    
    let handleHitTest (p:PointF) (h:PointF) =
        let x = p.X - h.X
        let y = p.Y - h.Y
        x * x + y * y < handleSize * handleSize

    let transformP (m:Drawing2D.Matrix) (p:Point) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]

    override this.HitTest e =
        let mutable result = false
        pts |> Seq.iter (fun p -> if float (p.X-e.X)**2. + float (p.Y-e.Y)**2. < float handleSize**2. then result <- true)
        result

    member this.Tension
        with get () = tension
        and set (v) = tension <- v; this.Invalidate()

    override this.OnPaint e =
        let g = e.Graphics
        let drawHandle (p:PointF) =
          let w = 5.f
          g.DrawEllipse(Pens.Black, p.X - w, p.Y - w, 2.f * w, 2.f * w)
        let ctx = g.Save()
        g.DrawBezier(Pens.Black, pts.[0], pts.[1], pts.[2], pts.[3])
        g.DrawLine(Pens.Red, pts.[0], pts.[1])
        g.DrawLine(Pens.Red, pts.[2], pts.[3])
        g.DrawCurve(Pens.Blue, pts, tension)
        // let (|>) x f = f x
        pts |> Array.iter drawHandle
        g.Restore(ctx)
        base.OnPaint(e)

    override this.OnMouseDown e =
        let l = PointF(single e.Location.X, single e.Location.Y)
        let ht = handleHitTest (PointF(single e.Location.X,single e.Location.Y))
        selected <- pts |> Array.tryFindIndex ht
        match selected with
        | Some(idx) ->
          let p = pts.[idx]
          offsetDrag <- PointF(p.X - l.X, p.Y - l.Y)
        | None -> ()
        this.Invalidate()

     override this.OnMouseUp e =
        selected <- None

     override this.OnMouseMove e =
        let l = PointF(single e.Location.X, single e.Location.Y)
        match selected with
        | Some idx -> 
          pts.[idx] <- PointF(l.X + offsetDrag.X, l.Y + offsetDrag.Y)
          this.Invalidate()
        | None -> ()
        this.Invalidate()