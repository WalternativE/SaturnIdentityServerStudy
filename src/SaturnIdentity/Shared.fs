namespace Shared

module Util =
    open Giraffe
    open Microsoft.AspNetCore.Http
    open Giraffe.GiraffeViewEngine

    let renderHtml (ctx : HttpContext) (template : XmlNode) =
        template
        |> renderHtmlDocument
        |> ctx.WriteHtmlStringAsync


module App =
    open Giraffe.GiraffeViewEngine

    let layout (content : XmlNode list ) =
        html [ _class "has-navbar-fixed-top" ] [
            head [] [
                meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
                title [] [ encodedText "Saturn Identity" ]
                link [ _rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.1/css/bulma.min.css" ]
                script [ _defer; _src "https://use.fontawesome.com/releases/v5.1.0/js/all.js" ] []
            ]
            body [] [
                yield nav [ _class "navbar is-fixed-top has-shadow" ] [
                    div [_class "navbar-brand"] [
                        a [_class "navbar-item"; _href "/"] [ encodedText "IdentityServer4" ]
                        div [_class "navbar-burger burger"; attr "data-target" "navMenu"] [
                            span [] []
                            span [] []
                            span [] []
                        ]
                    ]
                    div [ _class "navbar-menu"; _id "navbarMenu" ] [
                        div [ _class "navbar-start" ] [
                            div [ _class "navbar-item"] [
                                a [ _href "/account/login"] [ encodedText "Login" ]
                            ]
                        ]
                    ]
                ]
                yield! content
            ]
        ]