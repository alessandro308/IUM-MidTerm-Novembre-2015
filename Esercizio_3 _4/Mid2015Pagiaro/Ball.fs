module Ball

open LWCs
open ap
open System.Drawing.Drawing2D
open System.Drawing
open System.Windows
open System

type Ball() as this =
    inherit LWC()
    let mutable velocity = new Vector()
    let border = ResizeArray<GraphicsPath>()
    //let mutable leftTop = PointF()
    let mutable rect = new RectangleF(this.Location, SizeF(40.f, 40.f))
    let mutable showVelVector = true
    let mutable previousTime = DateTime()
    let random = Random()
    let mutable b = new SolidBrush(Color.FromArgb(random.Next(20, 250), random.Next(20,250),random.Next(20,250)))

    
    let MouseToBall (pt:Drawing.Point) =
        PointF(single pt.X - this.Location.X - (this.Size.Width / 2.f), single pt.Y - this.Location.Y - (this.Size.Height / 2.f))

    let ruotaVettore (v:Vector) (a:float) =
        Vector(v.X*Math.Cos(a)-v.Y*Math.Sin(a),
                v.X*Math.Sin(a)+v.Y*Math.Cos(a))
    
    let checkBorder () =
        if this.Location.X + this.Size.Width > single this.Parent.Size.Width then
            this.Location <- PointF(single this.Parent.Size.Width - this.Size.Width, this.Location.Y)
            velocity.X <- velocity.X*(-1.)
        if this.Location.X < 0.f then
            this.Location <- PointF(0.f, this.Location.Y)
            velocity.X <- velocity.X*(-1.)
        if this.Location.Y + this.Size.Height > single this.Parent.Size.Height then
            this.Location <- PointF(this.Location.X, single this.Parent.Size.Height - this.Size.Height)
            velocity.Y <- velocity.Y*(-1.)
        if this.Location.Y < 0.f then
            this.Location <- PointF(this.Location.X, 0.f)
            velocity.Y <- velocity.Y*(-1.)

    do
        for i in {0 .. 1 .. 19} do
            let gp = new GraphicsPath()
            gp.AddArc(rect, single i * 18.f, 18.f)
            border.Add(gp)

        this.Size <- SizeF(40.f, 40.f)

    //member this.Location with get() = leftTop and set(v) = leftTop <- v; 
    member this.Borders with get() = border
                                
    member this.UpdateBorder() =
        for x in border do
            x.Dispose()
        let mutable rect = new RectangleF(this.Location, SizeF(40.f, 40.f))
        border.Clear()
        for i in {0 .. 1 .. 19} do
            let gp = new GraphicsPath()
            gp.AddArc(rect, single i * 18.f, 18.f)
            border.Add(gp)

    override this.OnPaint e =
        let rectangle = RectangleF(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height)
        //e.Graphics.FillRectangle(Brushes.Black, 0, 0, 10, 10)
        e.Graphics.FillEllipse(b, rectangle)
        e.Graphics.DrawEllipse(Pens.Black, rectangle)
        //printfn "%f" velocity.Length
        if showVelVector then
            use p = new Pen(Color.Black, 2.f)
            p.EndCap <- Drawing2D.LineCap.ArrowAnchor
            e.Graphics.DrawLine(p, this.Location.X + 20.f, this.Location.Y + 20.f, this.Location.X + 20.f + single velocity.X, this.Location.Y + 20.f + single velocity.Y)
            let velString = velocity.Length.ToString()
            let sp = "Speed: "
            e.Graphics.DrawString((if velString.Length > 4 then String.Concat([|sp;velString.Substring(0,4)|]) else String.Concat([|sp;velString|])), 
                this.Parent.Font, Brushes.Black, 
                PointF(this.Location.X - 20.f + single velocity.X, this.Location.Y - 20.f + single velocity.Y) )
    
    override this.OnMouseDown e =
//        let radius = 20.f
        this.Location <- ap.PointF (Drawing.Point(e.Location.X - 20, e.Location.Y - 20))
        this.UpdateBorder()
//        this.Size <- SizeF(2.f*radius, 2.f*radius)
//        velocity <- Vector()
        this.Invalidate()
    
    override this.OnMouseMove e =
        let pt = MouseToBall e.Location
        //let pt1 = PointF(pt.X - this.Size.Width / 2.f, pt.Y - this.Size.Height /2.f)
        velocity <- Vector(float pt.X, float pt.Y)
        this.Invalidate()
    
    override this.OnMouseUp e =
        showVelVector <- false
        if velocity.Length = 0. then
            velocity<-Vector(30., 50.)
        previousTime <- DateTime.Now
        this.Invalidate()

    member this.UpdatePosition =
        if not(showVelVector) then //Se non la sto ancora creando
            let now = DateTime.Now
            let interval = now - previousTime
            let newLocation = PointF(this.Location.X + float32 interval.Milliseconds/1000.f * single velocity.X, 
                                    this.Location.Y +  single interval.Milliseconds/1000.f*single velocity.Y)
            this.Location <- newLocation
            checkBorder()  
            this.UpdateBorder()
            previousTime <- now

    member this.UpdateVelocity (sector:int) shiftSum =
        let mutable shift = 0.f
        match velocity.Length with
        | t when t < 100. -> shift <- 1.f
        | t when t < 150. -> shift <- 2.5f
        | t when t < 200. -> shift <- 3.5f
        | _ -> shift <- 4.f
        shift <- shift - shiftSum
        match sector with
        //EST
        | t when t = 0 || t = 19 -> if velocity.X > 0. then velocity <- Vector(Math.Abs(velocity.X)*(-1.), velocity.Y)
        //SUD-EST
        | t when t <= 3 -> velocity <- Vector(Math.Abs(velocity.X)*(-1.), Math.Abs(velocity.Y)*(-1.))
                           this.Location <- PointF(this.Location.X - shift, this.Location.Y - shift)
        //SUD
        | t when t <=5 -> if velocity.Y > 0. then velocity <- Vector(velocity.X, Math.Abs(velocity.Y)*(-1.))
        //SUD-OVEST
        | t when t <=8 -> velocity <- Vector(Math.Abs(velocity.X), Math.Abs(velocity.Y)*(-1.))
                          this.Location <- PointF(this.Location.X + shift, this.Location.Y - shift)
        //OVEST
        | t when t <=10 -> if velocity.X > 0. then velocity <- Vector(Math.Abs(velocity.X), velocity.Y)
        //NORD-OVEST
        | t when t <=13 -> velocity <- Vector(Math.Abs(velocity.X), Math.Abs(velocity.Y))
                           this.Location <- PointF(this.Location.X + shift, this.Location.Y + shift)
        //NORD
        | t when t <= 15 -> if velocity.Y > 0. then velocity <- Vector(velocity.X, Math.Abs(velocity.Y))
        //NORD-EST
        | t when t <= 19 -> velocity <- Vector(Math.Abs(velocity.X)*(-1.), Math.Abs(velocity.Y))
                            this.Location <- PointF(this.Location.X - shift, this.Location.Y + shift)
        | _ -> printfn "SETTORE SCONOSCIUTO"

        velocity <- ruotaVettore velocity (float(sector)*18.)

    member this.BouncOn (x:Ball) =
        let u = ap.toRad 18.
        let distFromCenters = ap.PointDist this.Location x.Location
        let overBallSize = distFromCenters - this.Size.Width // Width = 2*radius
        if distFromCenters < this.Size.Width then
            let contactPoint = ap.MiddlePoint (this.Location, x.Location)
            let contactVector = PointF(contactPoint.X - this.Location.X, contactPoint.Y - this.Location.Y)
            match contactVector with 
            //SUD-EST
            | pt when pt.X >= 0.f && pt.Y >= 0.f ->
                if pt.Y < 20.f * (single (Math.Sin u)) then
                    this.UpdateVelocity 0 overBallSize 
                else 
                    if pt.Y > 20.f * (single (Math.Sin (Math.PI/2. - u))) then
                        this.UpdateVelocity 4 overBallSize 
                    else
                        this.UpdateVelocity 3 overBallSize 
            //NORD-EST
            | pt when pt.X >= 0.f && pt.Y <= 0.f ->
                if pt.X > 20.f * (single (Math.Cos u)) then
                    this.UpdateVelocity 19 overBallSize 
                else 
                    if pt.X < 20.f * single (Math.Cos(Math.PI/2. - u)) then
                        this.UpdateVelocity 15 overBallSize 
                    else
                        this.UpdateVelocity 17 overBallSize 
            //NORD-OVEST
            | pt when pt.X < 0.f && pt.Y < 0.f ->
                if -pt.X  < 20.f * (single (Math.Cos u)) then
                    this.UpdateVelocity 14 overBallSize 
                else
                    if -pt.X > 20.f * (single (Math.Cos u) ) then
                        this.UpdateVelocity 10 overBallSize 
                    else
                        this.UpdateVelocity 12 overBallSize 
            //SUD-OVEST
            | pt -> 
                if -pt.X  < 20.f * (single (Math.Cos u)) then
                    this.UpdateVelocity 5 overBallSize 
                else
                    if -pt.X > 20.f * (single (Math.Cos u) ) then
                        this.UpdateVelocity 9 overBallSize 
                    else
                        this.UpdateVelocity 7 overBallSize 