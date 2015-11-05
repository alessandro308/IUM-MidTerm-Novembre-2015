module EditTools

open System.Drawing

type Pan () =
    let mutable startPoint = Point()

    member this.MouseDown (p:Point) =
        startPoint <- p

    member this.MouseMove p:Point =
        let temp = startPoint
        startPoint <- p
        Point(temp.X-p.X, temp.Y-p.Y)

    member this.MouseUp (p:Point) =
        let temp = startPoint
        startPoint <- p
        Point(temp.X-p.X, temp.Y-p.Y)
