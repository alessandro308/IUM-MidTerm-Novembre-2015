open System.Windows.Forms
open System.Drawing
open LWCs
open Curva

let f = new Form(Text="Curve editor (con LWC Mondo)", TopMost=true)
f.Show()

type IumButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<MouseEventArgs>()
  let upevt = new Event<MouseEventArgs>()
  let moveevt = new Event<MouseEventArgs>()
  
  do this.Size <- SizeF(32.f, 32.f)

  let mutable text = ""

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()

  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())

  override this.OnMouseMove e = moveevt.Trigger(e)

  override this.OnMouseDown e = downevt.Trigger(e)

  override this.OnPaint e =
    let g = e.Graphics
    g.FillEllipse(Brushes.Red, new Rectangle(0,0, int(this.Size.Width),int(this.Size.Height)))
    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.White, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))

type NavBut =
| Up = 0
| Right = 1
| Left = 2
| Down = 3

type Editor() as this =
  inherit LWContainer()

  let buttons = [| 
    new IumButton(Text="U",Location=PointF(32.f, 0.f), Vista = true);
    new IumButton(Text="R",Location=PointF(64.f, 32.f));
    new IumButton(Text="L",Location=PointF(0.f, 32.f));
    new IumButton(Text="D",Location=PointF(32.f, 64.f));
  |]

  //Ora ho un problemino, quanto spazio occupa la curva e quindi quanto deve essere grande questo LWC?
  let curva = new Curva(Location=PointF(20.f, 20.f), Coordinate=Coordinate.Mondo, Parent=this)

  let button (k:NavBut) =
    buttons.[int(k)]

  let translateW (tx, ty) =
    this.W2V.Translate(tx, ty)
    this.V2W.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)

  let rotateW a =
    this.W2V.Rotate a
    this.V2W.Rotate(-a, Drawing2D.MatrixOrder.Append)

  let rotateAtW p a =
    this.W2V.RotateAt(a, p)
    this.V2W.RotateAt(-a, p, Drawing2D.MatrixOrder.Append)

  let scaleW (sx, sy) =
    this.W2V.Scale(sx, sy)
    this.V2W.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)

  let transformP (m:Drawing2D.Matrix) (p:Point) =
    let a = [| PointF(single p.X, single p.Y) |]
    m.TransformPoints(a)
    a.[0]


  do 
    this.LWControls.Add(curva)
    buttons |> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))

  let scrollBy dir =
    match dir with
    | NavBut.Up -> (0.f, -10.f)
    | NavBut.Down -> (0.f, 10.f)
    | NavBut.Left -> (-10.f, 0.f)
    | NavBut.Right -> (10.f, 0.f)

  let translate (x, y) =
    let t = [| PointF(0.f, 0.f); PointF(x, y) |]
    this.V2W.TransformPoints(t)
    translateW(t.[1].X - t.[0].X, t.[1].Y - t.[0].Y)
  
  let handleCommand (k:Keys) =

    match k with
    | Keys.W ->
      scrollBy NavBut.Up |> translate
      this.Invalidate()
    | Keys.A ->
      scrollBy NavBut.Left |> translate
      this.Invalidate()
    | Keys.S ->
      scrollBy NavBut.Down |> translate
      this.Invalidate()
    | Keys.D ->
      scrollBy NavBut.Right |> translate
      this.Invalidate()
    | Keys.Q ->
      let p = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      rotateAtW p 10.f
      this.Invalidate()
    | Keys.E ->
      let p = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      rotateAtW p -10.f
      this.Invalidate()
    | Keys.Z ->
      let p = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      scaleW(1.1f, 1.1f)
      let p1 = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      translateW(p1.X - p.X, p1.Y - p.Y)
      this.Invalidate()
    | Keys.X ->
      let p = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      scaleW(1.f/1.1f, 1.f / 1.1f)
      let p1 = transformP this.V2W (Point(this.Width / 2, this.Height / 2))
      translateW(p1.X - p.X, p1.Y - p.Y)
      this.Invalidate()
    | _ -> ()

//  do 
//    buttons.[int(NavBut.Up)].Click.Add(fun _ -> handleCommand Keys.W)
//    buttons.[int(NavBut.Down)].Click.Add(fun _ -> handleCommand Keys.S)
//    buttons.[int(NavBut.Left)].Click.Add(fun _ -> handleCommand Keys.A)
//    buttons.[int(NavBut.Right)].Click.Add(fun _ -> handleCommand Keys.D)
  

  let scrollTimer = new Timer(Interval=100)
  let mutable scrollDir = NavBut.Up

  do scrollTimer.Tick.Add(fun _ ->
    scrollBy scrollDir |> translate
    this.Invalidate()
  )

  do 
    buttons.[int(NavBut.Up)].MouseDown.Add(fun _ -> scrollDir <- NavBut.Up)
    buttons.[int(NavBut.Down)].MouseDown.Add(fun _ -> scrollDir <- NavBut.Down)
    buttons.[int(NavBut.Left)].MouseDown.Add(fun _ -> scrollDir <- NavBut.Left)
    buttons.[int(NavBut.Right)].MouseDown.Add(fun _ -> scrollDir <- NavBut.Right)
    for v in [ NavBut.Up; NavBut.Left; NavBut.Right; NavBut.Down] do
      let idx = int(v)
      buttons.[idx].MouseDown.Add(fun _ -> scrollTimer.Start()) //sono due call per MouseDown!
      buttons.[idx].MouseUp.Add(fun _ -> scrollTimer.Stop(); printfn "Stoooop")

  override this.OnKeyDown e =
    handleCommand e.KeyCode

let e = new Editor(Dock=DockStyle.Fill)
f.Controls.Add(e)
e.Focus()

// <NoInMidTerm>
//let b = new Button(Text="OK")
//e.Controls.Add(b)
// </NoInMidTerm>

//e.Tension <- 1.f

e.MouseDown.Add(fun _ -> printfn "Ahi!")

Application.Run(f)