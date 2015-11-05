module Editor

open LWCs
open ToolButtons
open System.Drawing
open System.Windows.Forms
open ap
open EditTools
open Sheet
open Tratti
open Line
open Bezier
open Ellipse
open Rect
open Ball
open PenBar

type Editor() as this =
    inherit LWContainer()

    let mutable selectedTool = Tool.Edit
    let toolButton = new ToolButtons(Size=SizeF(40.f, 400.f), Location=PointF(0.f, 0.f))
    //let penBar = new PenBar(Size=SizeF(150.f, 20.f))
    let sheet = new Sheet(Size=SizeF(400.f, 400.f), Location=PointF(0.f, 0.f), Parent=this)
    let pan = Pan()
    let mutable countTratti = 0
    let mutable mouseDown = false
    let mutable clickOnTool = false
    let mutable oggettoSelezionato = false
    let balls = new ResizeArray<Ball>()
    let mutable ballCount = 0
    let updateBallTimer = new Timer(Interval=10)

    do
        this.DoubleBuffered <- true
        toolButton.SetParent(this)
        this.LWControls.Add(sheet)
        this.LWControls.Add(toolButton)
        //this.LWControls.Add(penBar)
        this.BackColor <- Color.White
        updateBallTimer.Tick.Add(fun t ->
            use drawingRegion = sheet.DrawingRegion()
            balls |> Seq.iter (fun x -> 
                x.UpdatePosition //Procede e se sbatte ai bordi cambia verso
                // Faccio qui il test con il disegno
                for settore in {0 .. 1 .. 19} do
                    use temp = drawingRegion.Clone()
                    temp.Intersect(x.Borders.[settore])
                    use g = this.CreateGraphics()
                    if not (temp.IsEmpty(g)) then
                        //printfn "COLPITO DISEGNO"
                        x.UpdateVelocity settore 0.f
                for altraPalla in {0 .. 1 .. (balls.Count-1)} do
                    let bt = balls.[altraPalla]
                    if x <> bt then
                        x.BouncOn bt 
                            
            )
            this.Invalidate()
        )
        //penBar.Location <- PointF(single this.ClientSize.Width - penBar.Size.Width, single this.ClientSize.Height - penBar.Size.Height)

//    override this.OnResize e =
//        penBar.Location <- PointF(single this.ClientSize.Width - penBar.Size.Width, single this.ClientSize.Height - penBar.Size.Height)
    

    override this.OnMouseDown e =
        mouseDown <- true
        if toolButton.HitTest (ap.PointF e.Location) then
            toolButton.OnMouseDown e
            selectedTool <- toolButton.SelectedTool
            if selectedTool <> Tool.Game then
                balls.Clear()
                ballCount <- 0
                updateBallTimer.Stop()
            else
                updateBallTimer.Start()
            if (oggettoSelezionato && selectedTool = Tool.ConfirmEdit) then
                oggettoSelezionato <- false
                selectedTool <- Tool.Edit
                sheet.SelectedTratto <- -1
                toolButton.SelectedTool <- Tool.Edit
                toolButton.ExistsTrattoSelected <- false
                this.Invalidate()
            if selectedTool = Tool.CleanAll then
                sheet.Tratti.Clear()
                countTratti <- 0
            clickOnTool <- true
        else
            //Non sto cliccando sui bottoni, quindi sto agendo sul foglio da disegno
            match selectedTool with
            | Tool.Game ->
                    let newBall = new Ball(Location=PointF(single e.Location.X, single e.Location.Y))
                    newBall.Parent <- this
                    newBall.OnMouseDown e
                    ballCount <- ballCount + 1
                    balls.Add(newBall)
            | Tool.Pan -> 
                //let pt = apg.Point(ap.transformP sheet.ViewToSheet e.Location)
                    pan.MouseDown e.Location
            | Tool.ZoomUp -> 
                    let mutable p = ap.transformP sheet.ViewToSheet e.Location
                    sheet.SheetToView.Scale(1.1f, 1.1f)
                    sheet.ViewToSheet.Scale(1.f/1.1f, 1.f/1.1f, Drawing2D.MatrixOrder.Append)
                    let p1 = ap.transformP sheet.ViewToSheet e.Location
                    sheet.SheetToView.Translate(p1.X - p.X, p1.Y - p.Y)
                    sheet.ViewToSheet.Translate(-(p1.X - p.X), -(p1.Y - p.Y), Drawing2D.MatrixOrder.Append)
                    this.Invalidate()
            | Tool.ZoomDown ->
                    let mutable p = ap.transformP sheet.ViewToSheet e.Location
                    sheet.SheetToView.Scale(1.f/1.1f, 1.f/1.1f)
                    sheet.ViewToSheet.Scale(1.1f, 1.1f, Drawing2D.MatrixOrder.Append)
                    let p1 = ap.transformP sheet.ViewToSheet e.Location
                    sheet.SheetToView.Translate(p1.X - p.X, p1.Y - p.Y)
                    sheet.ViewToSheet.Translate(-(p1.X - p.X), -(p1.Y - p.Y), Drawing2D.MatrixOrder.Append)
                    this.Invalidate()
            | Tool.Line ->
                    let ln = new Line()
                    let transPoint =  ap.transformP sheet.ViewToSheet e.Location
                    ln.StartPoint <- transPoint
                    ln.EndPoint <- transPoint
                    countTratti <- countTratti + 1
                    sheet.Tratti.Add( ln )
                    sheet.Invalidate()
            | Tool.Edit ->
                    let pt = ap.transformP sheet.ViewToSheet e.Location
                    sheet.SelectItem  pt
                    if sheet.SelectedTratto <> -1 then
                        sheet.OnMouseDown (MouseEventArgs(e.Button, e.Clicks, int pt.X,int pt.Y, e.Delta))
                        oggettoSelezionato <- true
                        toolButton.ExistsTrattoSelected <- true
                        toolButton.SelectedTool <- Tool.Edit
                        toolButton.TextPen.SetValue (int sheet.PenWidth)
            | Tool.Rect ->
                    let rt = new Rect()
                    let transPoint =  ap.transformP sheet.ViewToSheet e.Location
                    rt.StartPoint <- transPoint
                    rt.EndPoint <- transPoint
                    countTratti <- countTratti + 1
                    sheet.Tratti.Add( rt )
                    sheet.Invalidate()
            | Tool.Bez ->
                    let bz = new Bezier()
                    let transPoint = ap.transformP sheet.ViewToSheet e.Location
                    bz.StartPoint <- transPoint
                    bz.EndPoint <- transPoint
                    countTratti <- countTratti + 1
                    sheet.Tratti.Add(bz)
                    sheet.Invalidate()
            | Tool.Ellipse ->
                    let el = new Ellipse()
                    let transPoint =  ap.transformP sheet.ViewToSheet e.Location
                    el.StartPoint <- transPoint
                    el.EndPoint <- transPoint
                    countTratti <- countTratti + 1
                    sheet.Tratti.Add(el)
                    sheet.Invalidate()
            
            | _ -> ()

    override this.OnMouseMove e =
        if (clickOnTool=false) then
            if mouseDown then
                match selectedTool with
                | Tool.Game ->
                    let bl = balls.[ballCount - 1]
                    bl.OnMouseMove e
                | Tool.Pan -> 
                    //let pt1 = ap.Point(ap.transformP sheet.ViewToSheet e.Location)
                    let offset = ap.PointF(pan.MouseMove(e.Location))
                    let pt = offset
                    //let pt = ap.transformPF sheet.ViewToSheet pt1
                    sheet.SheetToView.Translate(-pt.X, -pt.Y, Drawing2D.MatrixOrder.Append)
                    sheet.ViewToSheet.Translate(pt.X, pt.Y(*, Drawing2D.MatrixOrder.Append*))
                    this.Invalidate()
                | Tool.Line ->
                    let ln = sheet.Tratti.[countTratti-1]
                    let translPoint = ap.transformP sheet.ViewToSheet e.Location
                    ln.OnMouseMove (new MouseEventArgs(e.Button,e.Clicks, int translPoint.X, int translPoint.Y, e.Delta))
                    sheet.Invalidate()
                | Tool.Bez ->
                    let bz = sheet.Tratti.[countTratti-1]
                    let translPoint = ap.transformP sheet.ViewToSheet e.Location
                    bz.OnMouseMove (new MouseEventArgs(e.Button,e.Clicks, int translPoint.X, int translPoint.Y, e.Delta))
                    sheet.Invalidate()
                | Tool.Rect ->
                    let rt = sheet.Tratti.[countTratti-1]
                    let translPoint = ap.transformP sheet.ViewToSheet e.Location
                    rt.OnMouseMove (new MouseEventArgs(e.Button,e.Clicks, int translPoint.X, int translPoint.Y, e.Delta))
                    sheet.Invalidate()
                | Tool.Ellipse ->
                    let ln = sheet.Tratti.[countTratti-1]
                    let translPoint = ap.transformP sheet.ViewToSheet e.Location
                    ln.OnMouseMove (new MouseEventArgs(e.Button,e.Clicks, int translPoint.X, int translPoint.Y, e.Delta))
                    sheet.Invalidate()
                | Tool.Edit ->
                    //printfn "EDITOR MOUSE MOVE EDIT TOOL"
                    if sheet.SelectedTratto <> -1 then
                        let pt = ap.transformP sheet.ViewToSheet e.Location
                        sheet.OnMouseMove (MouseEventArgs(e.Button, e.Clicks, int pt.X, int pt.Y, e.Delta))
                | _ -> ()

    override this.OnMouseUp e =
        mouseDown <- false
        if clickOnTool then
            clickOnTool <- false
        else
            match selectedTool with
            | Tool.Game ->
                let bl = balls.[ballCount - 1]
                bl.OnMouseUp e
            | Tool.Pan ->
                //let pt1 = ap.transformP sheet.ViewToSheet e.Location
                let offset = ap.PointF(pan.MouseUp(e.Location))
                let pt = offset
                sheet.SheetToView.Translate(-pt.X, -pt.Y, Drawing2D.MatrixOrder.Append)
                sheet.ViewToSheet.Translate(pt.X, pt.Y)
                this.Invalidate()
            | _ -> ()

    override this.OnKeyDown e =
        //printfn "ARRIVA KEYDOWN A EDITOR"
        if oggettoSelezionato then
            toolButton.OnKeyDown e
        sheet.SetPenWidth toolButton.TextPen.Value

    override this.OnPaint e =  
        e.Graphics.SmoothingMode <- Drawing2D.SmoothingMode.AntiAlias
        //e.Graphics.FillRegion(Brushes.Red, sheet.DrawingRegion())

        this.LWControls |> Seq.iter (fun c ->
          let s = e.Graphics.Save()
          e.Graphics.TranslateTransform(c.Location.X, c.Location.Y)
          //e.Graphics.Clip <- new Region(RectangleF(0.f, 0.f, c.Size.Width, c.Size.Height))
          //let r = e.Graphics.ClipBounds
          //let evt = new PaintEventArgs(e.Graphics, new Rectangle(int(r.Left), int(r.Top), int(r.Width), int(r.Height)))
          c.OnPaint e
          e.Graphics.Restore(s)
        )
        if selectedTool = Tool.Game then
            e.Graphics.FillRegion(Brushes.Beige, sheet.DrawingRegion())
        balls |> Seq.iter(fun b -> b.OnPaint e)
        base.OnPaint(e)