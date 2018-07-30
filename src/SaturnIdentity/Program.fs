open Saturn
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open IdentityConfig

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser" }

let browserRouter = router {
    pipe_through browser
    forward "" Home.Controller.homeController
    forward "/account/login" Login.Controller.loginController }

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
    use_router browserRouter

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
