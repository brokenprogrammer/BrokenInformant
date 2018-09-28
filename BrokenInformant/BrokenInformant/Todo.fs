module Todo
    open System
    open System.IO
    open System.Text.RegularExpressions

    type Todo = {
        prefix : string
        id : string option
        suffix : string
        fileName : string
        line : int
    } with
        static member Unreported prefix suffix = {prefix = prefix; id = None; suffix = suffix; fileName = ""; line = 0}
        static member Reported prefix id suffix = {prefix = prefix; id = Some id; suffix = suffix; fileName = ""; line = 0} 

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [for g in m.Groups -> g.Value])
        else None

    
    /// Pattern matches specified line against the format of a Unreported TODO
    /// returns a unreported TODO if successfull match; None otherwise.
    let lineToUnreportedTodo line =
        let regex = """^(.*)TODO: (.*)$"""
        match line with
        | Regex regex [prefix; suffix] -> Some(Todo.Unreported prefix suffix)
        | _ -> None

    /// Pattern matches specified line against the format of a Reported TODO
    /// returns a reported TODO if successfull match; None otherwise.
    let lineToReportedTodo line =
        let regex = """^(.*)TODO\((.*)\): (.*)$"""
        match line with
        | Regex regex [prefix; id; suffix] -> Some(Todo.Reported prefix id suffix)
        | _ -> None

    /// Determines if the specified line is either a reported, unreported TODO or neither
    /// returns either a TODO or None.
    let lineToTodo lineNumber line = 
        let unreported = lineToUnreportedTodo line
        
        match unreported with
        | Some x -> Some( { x with line = lineNumber+1 } )
        | None -> 
            let reported = lineToReportedTodo line
            match reported with
            | Some x -> Some( { x with line = lineNumber+1 } )
            | None -> None


    /// Walks every line within specified file and returns seq of Todos
    let todosInFile file =
        File.ReadLines(file)
        |> Seq.mapi lineToTodo
        |> Seq.choose id
        |> Seq.map (fun x -> {x with fileName = file})