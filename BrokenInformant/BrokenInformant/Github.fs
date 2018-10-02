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

module Github
    open System
    open Todo

    type GithubPersonalToken = string

    //TODO: Change from using environment variables to configurations file
    let readGithubCredentials () : GithubPersonalToken option =
        let credentials = System.Environment.GetEnvironmentVariable("BROKENINFORMANT_CREDENTIALS")

        if (credentials = null) then
            printf "No credentials could be found.\nDo you want to add your Github credentials? (y/n) "
            let response = Console.ReadLine()
            match response.ToLower().[0] with
            | 'y' -> 
                printf "Personal Token: "
                let token = Console.ReadLine()
                System.Environment.SetEnvironmentVariable("BROKENINFORMANT_CREDENTIALS", token)
                Some(System.Environment.GetEnvironmentVariable("BROKENINFORMANT_CREDENTIALS"))
            | _ -> None
        else 
            Some credentials
    
    module API =
        open System.Text
        open System.Net.Http
        open Newtonsoft.Json
        open Newtonsoft.Json.Linq

        /// Takes a github personal token, a target repo and a todo to report and submits
        /// the given todo as an issue to the specified github repo.
        /// Returns the given TODO but with it's ID field populated with the newly assigned ID.
        let postIssue (credentials : GithubPersonalToken) (repo : string) (todo : Todo) : Todo =
            async {
                let body = new JObject(new JProperty("title", todo.suffix),
                                        new JProperty("body", ""))
                let bodyString = body.ToString()

                let client = new HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", "Anything");
                client.DefaultRequestHeaders.Add("Authorization", (sprintf "token %s" credentials))
                let httpContent = new StringContent(bodyString, Encoding.UTF8, "application/json")
                let! response = client.PostAsync("https://api.github.com/repos/"+repo+"/issues", httpContent) |> Async.AwaitTask
                response.EnsureSuccessStatusCode() |> ignore

                let! jsonResponse = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let todoId = JObject.Parse(jsonResponse).["number"].Value<int>()

                return {todo with id = Some (sprintf "#%d" todoId)}
            } |> Async.RunSynchronously