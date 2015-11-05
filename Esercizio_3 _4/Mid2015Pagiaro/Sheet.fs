module Sheet

open LWCs
open System.Drawing
open Tratti
open Line
open System.Windows
open ap
type Sheet() =
    inherit LWC()
    
    let m_SheetToView = new Drawing2D.Matrix()
    let m_ViewToSheet = new Drawing2D.Matrix()
    let tratti = ResizeArray<Tratti>()
    let mutable (totalPath:Region) = null
    let mutable (selectedTratto:int) = -1
    let mutable regionInvalidate = true

    member this.SheetToView with get() = m_SheetToView 
    member this.ViewToSheet with get() = m_ViewToSheet
    member this.Tratti with get() = tratti
    member this.SelectedTratto with get() = selectedTratto and set(v) = if selectedTratto <> -1 then tratti.[selectedTratto].Selected <- false; selectedTratto <- v
    
    (*Ritorna una regione contenente tutti gli elementi disegnati*)
    member this.DrawingRegion() =
            if totalPath <> null then totalPath.Dispose()
            totalPath <- new Region(new Drawing2D.GraphicsPath())
            for x in tratti do
                if x.Path <> null && x.Path.PathPoints.Length = 2 then
                    //Tutta sta roba qui serve per creare un rettangolo e dare spessora alla linee prima 
                    //di inserirla dentro la region, altrimenti le palline la attraversano come niente
                    let pt = new Drawing2D.GraphicsPath()
                    let pts = x.Path.PathPoints
                    let pw2 = x.PenSize / 2.f + 1.f
                    let coeff_retta = (pts.[1].Y - pts.[0].Y)/(pts.[1].X - pts.[0].X)
                    let angle = Vector.AngleBetween(Vector(1., 0.), Vector(1.,float coeff_retta))
                    let rt = RectangleF(0.f, 0.f, (ap.PointDist pts.[0] pts.[1]), pw2)
                    let region = new Region(rt)
                    use m = new Drawing2D.Matrix()
                    let px, py = if pts.[0].X < pts.[1].X then (pts.[0].X, pts.[0].Y) else (pts.[1].X, pts.[1].Y)
                    m.Translate(px, py)
                    m.Rotate(single angle)
                    m.Translate(0.f, -pw2/2.f)
                    region.Transform(m) 
                    totalPath.Union(region)
                 else
                    totalPath.Union(x.Path)
            totalPath.Transform(this.SheetToView)
            totalPath

    member this.PenWidth =
        if (selectedTratto <> -1 ) then 
            tratti.[selectedTratto].PenSize 
        else 
            1.f
    member this.SetPenWidth v =
        if selectedTratto <> -1 then
            tratti.[selectedTratto].PenSize <- v


    override this.OnPaint e =
        let savedGraph = e.Graphics.Save()
        e.Graphics.Transform <- m_SheetToView
        //let rect = RectangleF(0.f, 0.f, this.Size.Width, this.Size.Height)
        //e.Graphics.FillRectangle(Brushes.White,rect)
        tratti |> Seq.iter (fun t -> t.OnPaint e)
        e.Graphics.Restore(savedGraph)

    member this.SelectItem clickedPoint =
        let mutable notFound = true
        if selectedTratto = -1 then
            for i in {0 .. 1 .. (tratti.Count - 1) } do
                if notFound && tratti.[i].HitTest clickedPoint then
                    if selectedTratto > -1 then
                        tratti.[selectedTratto].Selected <- false
                    tratti.[i].Selected <- true 
                    selectedTratto <- i
                    this.Invalidate()
                    notFound <- false

    override this.OnMouseDown e =
        if selectedTratto <> -1 then
            tratti.[selectedTratto].OnMouseDown e
            this.Invalidate()
                
    override this.OnMouseMove e =
        if selectedTratto <> -1 then
            tratti.[selectedTratto].OnMouseMove e
            this.Invalidate()
