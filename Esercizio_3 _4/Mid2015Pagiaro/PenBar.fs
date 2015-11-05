module PenBar

open LWCs
open System.Drawing
open System
open System.Windows.Forms
open ap
type PenBar() as this =
    inherit LWC()
    let mutable text = "1"
    let mutable selected = false
    let mutable alterna = 0
    let timer = new Timer(Interval=300)
    
    do
        timer.Tick.Add(fun _ ->
            alterna <- (alterna + 1) % 2
            this.Invalidate()
        )

    override this.OnPaint e =
        let sv = e.Graphics.Save()
        let rt = new RectangleF(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height)
        e.Graphics.FillRectangle(Brushes.White, rt)
        e.Graphics.TranslateTransform(this.Location.X, this.Location.Y)
        e.Graphics.DrawRectangle(Pens.Black, 0.f, 0.f, this.Size.Width, this.Size.Height)
        e.Graphics.DrawString(text, this.Parent.Font, Brushes.Black, 2.f, 3.f)
        if selected then
            if alterna = 0 then
                let ms = e.Graphics.MeasureString(text, this.Parent.Font)
                e.Graphics.DrawLine(Pens.Black, ms.Width + 2.f, 3.f, ms.Width + 2.f,this.Size.Height-3.f )
        e.Graphics.Restore(sv)

    member this.Selected with get() = selected

    member this.SetSelected =
        selected <- true
        timer.Start()
        
    member this.SetUnselected =
        selected <- false
        timer.Stop()

    member this.OnKeyDown (e:KeyEventArgs) =
        match e.KeyCode with
        | Keys.Back -> text <- if text.Length > 1 then text.Remove(text.Length - 1 , 1) else ""; 
        | Keys.D0 -> text <- text.Insert(text.Length , "0")
        | Keys.NumPad0 -> text <- text.Insert(text.Length , "0")
        | Keys.D1 -> text <- text.Insert(text.Length , "1")
        | Keys.NumPad1 -> text <- text.Insert(text.Length , "1")
        | Keys.D2 -> text <- text.Insert(text.Length , "2")
        | Keys.NumPad2 -> text <- text.Insert(text.Length , "2")
        | Keys.D3 -> text <- text.Insert(text.Length , "3")
        | Keys.NumPad3 -> text <- text.Insert(text.Length , "3")
        | Keys.D4 -> text <- text.Insert(text.Length , "4")
        | Keys.NumPad4 -> text <- text.Insert(text.Length , "4")
        | Keys.D5 -> text <- text.Insert(text.Length , "5")
        | Keys.NumPad5 -> text <- text.Insert(text.Length , "5")
        | Keys.D6 -> text <- text.Insert(text.Length , "6")
        | Keys.NumPad6 -> text <- text.Insert(text.Length , "6")
        | Keys.D7 -> text <- text.Insert(text.Length , "7")
        | Keys.NumPad7 -> text <- text.Insert(text.Length , "7")
        | Keys.D8 -> text <- text.Insert(text.Length , "8")
        | Keys.NumPad8 -> text <- text.Insert(text.Length , "8")
        | Keys.D9 -> text <- text.Insert(text.Length , "9")
        | Keys.NumPad9 -> text <- text.Insert(text.Length , "9")
        | _ -> ()
        this.Invalidate()

    override this.HitTest (pt:PointF) =
        let rt = new RectangleF(this.Location, this.Size)
        rt.Contains(pt)

    member this.Value =
            if text <> "" then
                System.Single.Parse(text)
            else   
                1.f

    member this.SetValue v =
            text <- sprintf "%d" (int v)