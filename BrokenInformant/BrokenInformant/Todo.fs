// MIT License
// 
// Copyright (c) 2018 Oskar Mendel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
    } 
    with
        override x.ToString() = match x.id with
                                | Some id -> 
                                    sprintf "%s:%d: %sTODO(%s): %s" x.fileName x.line x.prefix id x.suffix
                                | None -> 
                                    sprintf "%s:%d: %sTODO: %s" x.fileName x.line x.prefix x.suffix
        member x.Output() = match x.id with
                            | Some id -> 
                                sprintf "%sTODO(%s): %s" x.prefix id x.suffix
                            | None ->
                                sprintf "%sTODO: %s" x.prefix x.suffix
        static member Unreported prefix suffix = {prefix = prefix; id = None; suffix = suffix; fileName = ""; line = 0}
        static member Reported prefix id suffix = { Todo.Unreported prefix suffix with id = Some id } 

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [for g in m.Groups -> g.Value])
        else None

    let getCommitMessage todo = 
        match todo.id with
        | Some x -> sprintf "TODO(%s)" x
        | None -> "TODO"

    /// Reads a TODO's file and then outputs all its content but the line of the TODO and 
    /// instead of the original line yields the contents of the TODO.
    let updateTodoInFile todo = 
        let inputFileContents = File.ReadAllLines(todo.fileName)
        let outputFileContents = [| for i in 0 .. (inputFileContents.Length - 1) do 
                                    if i = (todo.line - 1) then
                                        yield todo.Output()
                                    else 
                                        yield inputFileContents.[i]
                                 |]
        File.WriteAllLines(todo.fileName, outputFileContents)
        ()

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

    /// Walks every line within specified file and returns seq of TODOs
    let todosInFile file =
        File.ReadAllLines(file)
        |> Seq.mapi lineToTodo
        |> Seq.choose id
        |> Seq.map (fun x -> {x with fileName = file})

    // Walks every file specified, collects all its TODOs and returns a seq with all TODOs
    // within all the given files.
    let todosInFiles files =
        seq {
            for file in files do
                yield! todosInFile file
        }
