module LWCs

open System.Windows.Forms
open System.Drawing

type Coordinate =
    | Vista = 1
    | Mondo = 0

// Lightweight controls: astrazione programmativa che imita i controlli grafici
type LWC() =
  let mutable parent : Control = null
  let mutable location = PointF()
  let mutable size = SizeF()
  let mutable coordinateVista = true

  member this.Coordinate 
    with get() = if coordinateVista = false then
                    Coordinate.Mondo 
                 else 
                    Coordinate.Vista
    and set(v) = if v = Coordinate.Vista then
                    coordinateVista <- true
                 else
                    coordinateVista <- false
  member this.Vista with get() = coordinateVista and set(v) = coordinateVista <- v
  
  abstract OnMouseDown : MouseEventArgs -> unit
  default this.OnMouseDown _ = ()

  abstract OnMouseMove : MouseEventArgs -> unit
  default this.OnMouseMove _ = ()

  abstract OnMouseUp : MouseEventArgs -> unit
  default this.OnMouseUp _ = ()

  abstract OnPaint : PaintEventArgs -> unit
  default this.OnPaint _ = ()

  abstract HitTest : PointF -> bool
  default this.HitTest p =
    (RectangleF(PointF(), size)).Contains(p)

  member this.Invalidate() =
    if parent <> null then parent.Invalidate()

  member this.Location
    with get() = location
    and set(v) = location <- v; this.Invalidate()

  member this.Size
    with get() = size
    and set(v) = size <- v; this.Invalidate()

  member this.Parent
    with get() = parent
    and set(v) = parent <- v

type LWContainer() =
  (*
    Questo Container si preoccuperà di passare l'ambiente grafico adattato alle esigenze del LWC (vista o mondo)
  *)
  inherit UserControl()

  let controls = ResizeArray<LWC>()
    
  let mutable w2v = new Drawing2D.Matrix()
  let mutable v2w = new Drawing2D.Matrix()

  let cloneMouseEvent (c:LWC) (e:MouseEventArgs) =
    if c.Coordinate = Coordinate.Vista then
        new MouseEventArgs(e.Button, e.Clicks, e.X - int(c.Location.X), e.Y - int(c.Location.Y), e.Delta)
    else
        let tmp = [|e.Location|]
        v2w.TransformPoints(tmp)
        new MouseEventArgs(e.Button, e.Clicks, tmp.[0].X, tmp.[0].Y, e.Delta)


  let correlate (e:MouseEventArgs) (f:LWC->MouseEventArgs->unit) =
    let mutable found = false
    for i in { (controls.Count - 1) .. -1 .. 0 } do
      if not found then
        let c = controls.[i]
        if c.Coordinate = Coordinate.Vista then
            if c.HitTest(PointF(single(e.X) - c.Location.X, single(e.Y) - c.Location.Y)) then
              found <- true
              f c (cloneMouseEvent c e)
        else
//            printfn "Mondo -> Vista: %f %f" elt.[4] elt.[5]
//            printfn "Mouse (Vista): %d %d" e.Location.X e.Location.Y
            let tmp = [|e.Location|]
            v2w.TransformPoints(tmp)
            if c.HitTest(PointF(single(tmp.[0].X), single(tmp.[0].Y))) then
              found <- true
              f c (cloneMouseEvent c e)

  let mutable captured : LWC option = None

  member this.LWControls = controls
  member this.W2V with get() = w2v
  member this.V2W with get() = v2w

  override this.OnMouseDown e =
    correlate e (fun c ev -> captured <- Some(c); c.OnMouseDown(ev))
    base.OnMouseDown e

  override this.OnMouseUp e =
    correlate e (fun c ev -> c.OnMouseUp(ev))
    match captured with
    | Some c -> c.OnMouseUp(cloneMouseEvent c e); captured <- None
    | None  -> ()
    base.OnMouseUp e

  override this.OnMouseMove e =
    correlate e (fun c ev -> c.OnMouseMove(ev))
    match captured with
    | Some c -> c.OnMouseMove(cloneMouseEvent c e)
    | None  -> ()
    base.OnMouseMove e

  override this.OnPaint e =
    controls |> Seq.iter (fun c ->
      let s = e.Graphics.Save()
      if c.Coordinate = Coordinate.Vista then
          e.Graphics.TranslateTransform(c.Location.X, c.Location.Y)
           //Il clip bound Ë disabilitato in Coord.Mondo perchË la curva non mi permette di gestire facilmente il ClipBound
          e.Graphics.Clip <- new Region(RectangleF(0.f, 0.f, c.Size.Width, c.Size.Height))
      else //Coordinate = Coordinate.Mondo
          e.Graphics.Transform <- w2v
      let r = e.Graphics.ClipBounds
      let evt = new PaintEventArgs(e.Graphics, new Rectangle(int(r.Left), int(r.Top), int(r.Width), int(r.Height)))
      c.OnPaint evt
      e.Graphics.Restore(s)
    )
    base.OnPaint(e)

type NavBut =
| Up = 0
| Right = 1
| Left = 2
| Down = 3
