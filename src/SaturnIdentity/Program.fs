open System
open Saturn
open Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open IdentityConfig
open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http

let layout (content : XmlNode list ) =
    html [] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Saturn Identity"]
            link [_rel "stylesheet"; _href "https://cdnjs.com/libraries/bulma"]
        ]
        body [] content
    ]

let indexAction (ctx : HttpContext) = task {
        return layout [ h1 [] [ encodedText "Hello, world!" ] ]
    }

let homeController = controller {
    index indexAction }

let mainRouter = router {
    forward "" homeController }

let configureServices (services : IServiceCollection) =
    services.AddIdentityServer()
        .AddDeveloperSigningCredential()
        .AddInMemoryIdentityResources(identityResources)
        .AddInMemoryClients(clients)
        .AddTestUsers(testUsers)
        .AddInMemoryPersistedGrants() |> ignore
    services

let configureApp (app : IApplicationBuilder) =
    app.UseIdentityServer() |> ignore
    app

let app = application {
    use_router mainRouter

    url "https://localhost:8085/"

    service_config configureServices
    app_config configureApp

    force_ssl
    memory_cache
    use_gzip }

[<EntryPoint>]
let main _ =
    run app
    0
