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

        //let setHeaders token req =
            //req
            //|> Request.setHeader(Authorization (sprintf "token %s" token))
            //|> Request.setHeader(ContentType <| ContentType.create ("application", "json"))

        let postIssue (credentials : GithubPersonalToken) (repo : string) (todo : Todo) =
            async {
                let body = new JObject(new JProperty("title", todo.suffix),
                                        new JProperty("body", ""))
                let bodyString = body.ToString()

                let client = new HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", "Anything");
                client.DefaultRequestHeaders.Add("Authorization", (sprintf "token %s" credentials));
                let httpContent = new StringContent(bodyString, Encoding.UTF8, "application/json");
                let! reponse = client.PostAsync("https://api.github.com/repos/"+repo+"/issues", httpContent)

                //TODO: Retrieve returned issue and add id to the todo

            } |> Async.RunSynchronously