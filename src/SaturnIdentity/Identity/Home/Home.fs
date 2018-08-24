module Home

module View =
    open Shared
    open Giraffe.GiraffeViewEngine

    let indexView =
        [
            section [ _class "hero is-primary" ] [
                div [ _class "hero-body" ] [
                    div [ _class "container" ] [
                        div [ _class "columns is-vcentered" ] [
                            div [ _class "column" ] [
                                p [ _class "title" ] [ encodedText "Welcome to IdentityServer4!" ]
                            ]
                        ]
                    ]
                ]
            ]
            section [ _class "section" ] [
                div [ _class "container" ] [
                    p [] [
                        encodedText "Identity server publishes a "
                        a [_href "/.well-known/openid-configuration"] [ encodedText "discovery document " ]
                        encodedText "where you can find metadata and links to all the endpoints, key material, etc."
                    ]
                    br []
                    p [] [
                        encodedText "Click here "
                        a [_href "/grants"] [ encodedText "here " ]
                        encodedText "to manage your stored grants."
                    ]
                ]
            ]
        ]
        |> App.layout

module Controller =
    open Microsoft.AspNetCore.Http
    open Saturn
    open Shared.Util

    let indexAction (ctx : HttpContext) =
        View.indexView
        |> renderHtml ctx

    let homeController = controller {
        index indexAction }