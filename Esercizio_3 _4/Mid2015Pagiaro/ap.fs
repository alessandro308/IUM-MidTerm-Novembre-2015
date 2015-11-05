module ap

open System.Drawing
open System
open System.Windows
type ap() =
    static member PointF (p:Drawing.Point) =
        PointF(single p.X, single p.Y)

    static member Point (p:PointF) =
        Drawing.Point(int p.X, int p.Y)

    static member transformP (m:Drawing2D.Matrix) (p:Drawing.Point) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]

    static member transformPF (m:Drawing2D.Matrix) (p:PointF) =
        let a = [| p |]
        m.TransformPoints(a)
        a.[0]

    static member PointDist (pt:PointF) (pt1:PointF) =
        single (Math.Sqrt(float((pt.X - pt1.X)*(pt.X - pt1.X)) + float((pt.Y-pt1.Y)*(pt.Y-pt1.Y))))
    
    static member RotateVector (v:Vector) (a:float) =
        Vector(v.X*Math.Cos a - v.Y * Math.Sin a,
                v.X*Math.Cos a + v.Y * Math.Cos a)
    static member MiddlePoint ((pt1:PointF), (pt2:PointF)) =
        PointF((pt1.X + pt2.X)/2.f, (pt1.Y + pt2.Y)/2.f)
    static member toRad a =
        a*Math.PI / 180.

    static member decomposeMatrix (md:Drawing2D.Matrix) =
        let el = md.Elements
        let transl_x = el.[4]
        let transl_y = el.[5]
        let zoom_x = float(Math.Sign(el.[0])) * Math.Sqrt(float(el.[0]**2.f + el.[1]**2.f))
        let zoom_y = float(Math.Sign(el.[3])) * Math.Sqrt(float(el.[2]**2.f + el.[3]**2.f))
        let rotAngle = Math.Atan2(float -el.[1], float el.[0])
        [|single transl_x; single transl_y; single zoom_x; single zoom_y; single rotAngle|]

//    static member Hypot a b =
//        Math.Sqrt(a**2 + b**2)
//
//    static member distAlong(x, y, xAlong, yAlong) =
//        (x * xAlong + y * yAlong) / ap.Hypot(xAlong yAlong)
