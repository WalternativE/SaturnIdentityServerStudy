module Login

module View =
    open Giraffe.GiraffeViewEngine
    open Shared

    let loginView =
        [
            div [ _class "container" ] [
                h1 [ _class "login" ] [ encodedText "Login" ]
            ]
        ]
        |> App.layout

module Controller =
    open Microsoft.AspNetCore.Http
    open Shared.Util
    open Saturn
    
    let indexAction (ctx : HttpContext) =
        View.loginView
        |> renderHtml ctx

    let loginController = controller {
        index indexAction }