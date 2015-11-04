(*Il seguente codice è parte del progetto InterpolazioneMatrice, che contiene tutti i riferimenti necessari per la corretta esecuzione*)
open System.Drawing
open System
open System.Windows.Forms

let decomposeMatrix (md:Drawing2D.Matrix) =
    let el = md.Elements
    let transl_x = el.[4]
    let transl_y = el.[5]
    let zoom_x = Math.Sqrt(float(el.[0]**2.f + el.[1]**2.f))
    let zoom_y = Math.Sqrt(float(el.[2]**2.f + el.[3]**2.f))
    let rotAngle = Math.Atan2(float el.[2], float el.[3])
    [|single transl_x; single transl_y; single zoom_x - 1.f; single zoom_y - 1.f; single(-rotAngle*180./Math.PI)|]

let compose (els:single[]) =
    let m = new Drawing2D.Matrix()
    m.Translate(els.[0], els.[1])
    m.Rotate(els.[4])
    m.Scale(els.[2], els.[3])
    m

let showFrame (m:Drawing2D.Matrix) (f:Form) (frameNumber:int) path (prev:Drawing2D.Matrix)=
    let els = decomposeMatrix m
    let prevEls = decomposeMatrix prev
    els.[2] <- els.[2] - prevEls.[2] 
    els.[3] <- els.[3] - prevEls.[3]
    for i in {0 .. 1 .. 4} do
        els.[i] <- els.[i] / single frameNumber
    for i in {0 .. 1 .. frameNumber-1} do
        let temp:single[] = [|0.f; 0.f; 1.f; 1.f; 0.f|]
        for j in {0 .. 1 .. 4} do
            temp.[j] <- els.[j] * single i
        temp.[2] <- temp.[2] + prevEls.[2] + 1.f
        temp.[3] <- temp.[3] + prevEls.[3] + 1.f
        f.Paint.Add(fun e ->
            let newM = compose(temp)
            newM.Multiply(prev, Drawing2D.MatrixOrder.Append)
            e.Graphics.Transform <- newM
            e.Graphics.DrawPath(Pens.Black, path)
        )

let x = new Drawing2D.Matrix()
let y = new Drawing2D.Matrix()
x.Translate(300.f, 150.f)
x.Rotate(10.f)
x.Scale(1.5f, 1.7f)
y.Translate(50.f, 250.f)
y.Rotate(-45.f)

let path = new Drawing2D.GraphicsPath()
let rt = new RectangleF(10.f, 10.f, 120.f, 70.f)
path.AddRectangle(rt)
let el = decomposeMatrix x
let m = compose el
let f = new Form(Text="Matrix Interpolation", TopMost=true)

f.Show()
f.Paint.Add(fun e ->
    e.Graphics.SmoothingMode <- Drawing2D.SmoothingMode.AntiAlias
    use temp = x.Clone()
    temp.Multiply(y, Drawing2D.MatrixOrder.Append)
    e.Graphics.Transform <- temp
    e.Graphics.DrawPath(Pens.Red, path)
)
showFrame x f 10 path y

Application.Run(f)