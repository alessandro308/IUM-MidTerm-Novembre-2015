module ToolButtons

open LWCs
open System.Drawing
open System.Windows.Forms
open ap
open PenBar
type ToolButton() =
    inherit LWC()
    let mutable selected = false
    let mutable text = ""
    let mutable tool:Tool = Tool.Pan
    do
        base.Size <- SizeF(50.f, 50.f)

    member this.Text with get() = text and set(v) = text <- v
    member this.Tool with get() = tool and set(v) = tool <- v
    member this.Selected with set(v) = selected <- v
   
    override this.OnPaint e =
        let rect = RectangleF(this.Location.X, this.Location.Y, base.Size.Width, base.Size.Height)
        let rectInt = RectangleF(this.Location.X+4.f, this.Location.Y+4.f, base.Size.Width-8.f, base.Size.Height-8.f)
        if selected then
            e.Graphics.FillEllipse(Brushes.Red, rect) 
        e.Graphics.FillEllipse(Brushes.Black, rectInt)
        let ms = e.Graphics.MeasureString(text, base.Parent.Font)
        let start_text = PointF(this.Location.X + (this.Size.Width - ms.Width)/2.f, this.Location.Y + (this.Size.Height - ms.Height)/2.f)
        e.Graphics.DrawString(text, base.Parent.Font, Brushes.White, start_text)
        

    override this.HitTest (p:PointF) =
        let rect = RectangleF(this.Location.X, this.Location.Y, base.Size.Width, base.Size.Height)
        let click = rect.Contains(p)
        if click then
            selected <- true
            this.Invalidate()
        click
        

type ToolButtons() =
    inherit LWC()
    
    let tools = ResizeArray<ToolButton>()
    let mutable selectedTool:Tool = Tool.Pan 
    let mutable existsTrattoSelected = false
    let mutable writeMode = false
    //let mutable gameMode = false
    let textPen = new PenBar(Location=PointF(0.f, 40.f), Size=SizeF(40.f, 20.f))

    let confirmEdit = ToolButton(Text="DONE", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 0.f))
    let endGameBt = ToolButton(Text="END", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 0.f))
    do
        tools.Add(
            new ToolButton(Text="EDIT", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 0.f), Tool=Tool.Edit))
        tools.Add(
            new ToolButton(Text="PAN", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 40.f), Tool=Tool.Pan))
        tools.Add(
            new ToolButton(Text="Z+", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 80.f), Tool=Tool.ZoomUp))
        tools.Add(
            new ToolButton(Text="Z-", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 120.f), Tool=Tool.ZoomDown))
        tools.Add(
            new ToolButton(Text="LINE", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 160.f), Tool=Tool.Line))
        tools.Add(
            new ToolButton(Text="RECT", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 200.f), Tool=Tool.Rect))
        tools.Add(
            new ToolButton(Text="ELLIP", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 240.f), Tool=Tool.Ellipse))
        tools.Add(
            new ToolButton(Text="BEZ", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 280.f), Tool=Tool.Bez))
        tools.Add(
            new ToolButton(Text="GAME", Size=SizeF(40.f, 40.f), Location=PointF(0.f, 320.f), Tool=Tool.Game))
        let str = (String.concat "" ["Clean";System.Environment.NewLine; "   all"])
        tools.Add(
            new ToolButton(Text=str, Size=SizeF(40.f, 40.f), Location=PointF(0.f, 360.f), Tool=Tool.CleanAll))
    
    member this.WriteMode with get() = writeMode and set(v) = writeMode <- v; if writeMode then textPen.SetSelected else textPen.SetUnselected
    
    member this.SetParent v =
        base.Parent <- v
        tools |> Seq.iter(fun t -> t.Parent <- this.Parent)
        textPen.Parent <- this.Parent
        endGameBt.Parent <- this.Parent
        confirmEdit.Parent<-this.Parent

    member this.ExistsTrattoSelected 
        with get() = existsTrattoSelected 
        and set(v) = existsTrattoSelected <- v

    member this.SelectedTool with get() = selectedTool and set(v) = selectedTool <- v
    member this.TextPen with get() = textPen 
    //member this.GameMode with get() = gameMode and set(v) = gameMode <- v

    override this.OnPaint e =
        if existsTrattoSelected then
            confirmEdit.OnPaint e
            textPen.OnPaint e
        else 
            if selectedTool = Tool.Game then
                endGameBt.OnPaint e
            else
                tools |> Seq.iter(fun t -> t.OnPaint e)

    override this.OnMouseDown e =
        if existsTrattoSelected then
            if confirmEdit.HitTest (ap.PointF e.Location) then
                textPen.SetUnselected
                selectedTool <- Tool.ConfirmEdit
            else 
                if textPen.HitTest (ap.PointF e.Location) then
                    textPen.SetSelected
                    writeMode <- true
                else
                    textPen.SetUnselected
                    writeMode <- false
        else
            tools |> Seq.iter(fun t ->
                if t.HitTest (PointF(single e.Location.X, single e.Location.Y)) then
                    selectedTool <- t.Tool
                else
                    t.Selected <- false
            )

    member this.OnKeyDown e =
        //printfn "ARRIVA TASTO A TOOLBUTTONS" 
        if textPen.Selected then
            textPen.OnKeyDown e
            this.Invalidate()

    override this.HitTest pt =
        let mutable result = false
        if selectedTool = Tool.Game then
            let rt = RectangleF(0.f, 0.f, 40.f, 40.f)
            result <- rt.Contains(pt)
        else
            if existsTrattoSelected then
                let rt = RectangleF(0.f, 0.f, 40.f, 60.f)
                result <- rt.Contains(pt)
            else
                let rt = RectangleF(0.f, 0.f, this.Size.Width, this.Size.Height)
                result <- rt.Contains(pt)
        result