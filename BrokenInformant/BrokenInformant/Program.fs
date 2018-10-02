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

open System
open System.IO
open Github
open Todo

/// Traverses the specified folder and returns a seq with all files under that folder.
let rec allFilesUnder folder =
    seq {
        yield! Directory.GetFiles(folder)
        for subDirectory in Directory.GetDirectories(folder) do
            yield! allFilesUnder subDirectory
    }

let listCommand () = 
    let files = allFilesUnder (System.Environment.CurrentDirectory)
    let todos = todosInFiles files
    for todo in todos do
        //TODO: Strip unnecessary part of file path when printing.
        printfn "%s" (todo.ToString())
    ()

let reportCommand (credentials : GithubPersonalToken)(repo : string) =
    //TODO(#4): Implement report command to perform reporting of issues to Github.
    let files = allFilesUnder (System.Environment.CurrentDirectory)
    let todos = todosInFiles files

    let test = Todo.Unreported "" "Test Issue from BrokenInformant"
    let newTodo = API.postIssue credentials repo test

    ()

let usage () = 
    //TODO(#6): Implement usage help command for invalid or wrong passed command line arguments.
    failwith "Usage not implemented"

[<EntryPoint>]
let main argv =
    let token = Github.readGithubCredentials()
    let credentials = match token with 
                      | Some x -> x
                      | None -> 
                        printf "No Github credentials was loaded. Exiting...\n"
                        exit 1

    if argv.Length > 1 then
        match argv with
        | [|"list"|] ->
            listCommand()
        | [|"report"; repo|] ->
            reportCommand credentials repo
        | _ -> usage()
    else
        usage()

    0
