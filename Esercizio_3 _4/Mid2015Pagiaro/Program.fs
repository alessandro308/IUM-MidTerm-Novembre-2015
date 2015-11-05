//Altre informazioni su F# all'indirizzo http://fsharp.org
// Per ulteriori informazioni, vedere il progetto 'Esercitazione su F#'.
open System.Drawing
open System.Windows.Forms
open Editor

[<EntryPoint>]
let main argv = 
    let mutable firstTime = true
    let f = new Form(Text="Pagiaro Alessandro - Game and Editor", Size=Size(450, 450), MinimumSize=Size(450, 450))
    let e = new Editor(Dock = DockStyle.Fill)

    f.Controls.Add(e)
    //e.Focus()
    f.Show()

    Application.Run(f)
    0 // restituisci un intero come codice di uscita
