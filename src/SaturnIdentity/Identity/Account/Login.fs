module Login

type ExternalProvider =
    { DisplayName : string
      AuthenticationScheme : string }

type LoginViewModel =
    { AllowRememberLogin : bool
      EnableLocalLogin : bool
      ReturnUrl : string
      Username : string
      ExternalProviders : ExternalProvider list }

type LoginInputModel =
    { Username : string
      Password : string
      ReturnUrl : string }

module View =
    open Giraffe.GiraffeViewEngine
    open Shared

    // TODO: wire up all the model parts
    let loginView (model : LoginViewModel ) =
        [
            div [ _class "section" ] [
                div [ _class "container" ] [
                    h1 [ _class "login" ] [ encodedText "Login" ]
                    br []
                    form [ _method "post" ] [
                        input [ _type "hidden"; _name "ReturnUrl" ;_value model.ReturnUrl ]
                        div [ _class "field" ] [
                            label [ _class "label" ] [ encodedText "Username" ]
                            div [ _class "control" ] [
                                input [ _class "input"; _type "text"; _name "Username" ; _placeholder "Username"; _value model.Username ]
                            ]
                        ]
                        div [ _class "field" ] [
                            label [ _class "label" ] [ encodedText "Password" ]
                            div [ _class "control" ] [
                                input [ _class "input"; _type "password"; _name "Password" ; _placeholder "Password" ]
                            ]
                        ]
                        div [ _class "field is-grouped" ] [
                            div [ _class "control" ] [
                                input [ _type "submit"; _class "button is-link"; _name "Button"; _value "Submit" ]
                            ]
                            div [ _class "control" ] [
                                input [ _type "submit"; _class "button is-link"; _name "Button"; _value "Cancel" ]
                            ] 
                        ]
                    ]
                ]
            ]
        ]
        |> App.layout

module Controller =
    open Microsoft.AspNetCore.Http
    open Shared.Util
    open Saturn
    open Giraffe
    open IdentityServer4.Services
    open Microsoft.AspNetCore.Authentication
    open IdentityServer4.Stores

    let buildLoginViewModel
        (interaction : IIdentityServerInteractionService)
        (schemeProvider : IAuthenticationSchemeProvider)
        (clientStore : IClientStore)
        (returnUrl : string) = task {
            let! context = interaction.GetAuthorizationContextAsync returnUrl
            let context = if isNotNull context then Some context else None

            let! schemes = schemeProvider.GetAllSchemesAsync()

            let providers =
                schemes
                |> Seq.filter (fun s -> isNotNull s.DisplayName)
                |> Seq.map (fun s -> { DisplayName = s.DisplayName; AuthenticationScheme = s.Name })
                |> Seq.toList

            let! client =
                match context with
                | None -> task { return None }
                | Some ctx ->
                    if isNotNull ctx.ClientId then
                        task {
                            let! c = (clientStore.FindEnabledClientByIdAsync ctx.ClientId)
                            return Some c
                        }
                    else
                        task { return None }

            let (allowLocal, filteredProviders) =
                match client with
                | None -> (true, providers)
                | Some c ->
                    let p =
                        if isNotNull c.IdentityProviderRestrictions && not (Seq.isEmpty c.IdentityProviderRestrictions) then
                            providers
                            |> List.filter (fun p -> c.IdentityProviderRestrictions.Contains p.AuthenticationScheme)
                        else
                            providers
                    (c.EnableLocalLogin, p)

            let username =
                match context with
                | None -> ""
                | Some ctx -> ctx.LoginHint

            return
                { AllowRememberLogin = true
                  EnableLocalLogin = allowLocal
                  ReturnUrl = returnUrl
                  Username = username
                  ExternalProviders = filteredProviders }
        }

    let buildLoginViewModelFromInputModel
        (interaction : IIdentityServerInteractionService)
        (schemeProvider : IAuthenticationSchemeProvider)
        (clientStore : IClientStore)
        (inputModel : LoginInputModel) = task {
            let! model = buildLoginViewModel interaction schemeProvider clientStore inputModel.ReturnUrl
            return { model with Username = inputModel.Username }
    }

    let indexAction (nxt : HttpFunc) (ctx : HttpContext) = task {
        let returnUrl =
            match ctx.TryGetQueryStringValue "returnUrl" with
            | None -> ""
            | Some q -> q

        let interaction = ctx.GetService<IIdentityServerInteractionService>()
        let schemeProvider = ctx.GetService<IAuthenticationSchemeProvider>()
        let clientStore = ctx.GetService<IClientStore>()

        let! loginViewModel =
            buildLoginViewModel interaction schemeProvider clientStore returnUrl

        let! v =
            View.loginView loginViewModel
            |> renderHtml ctx
        return v
    }

    let formPostBackAction (nxt : HttpFunc) (ctx : HttpContext) = task {
        let! loginInputModel = ctx.BindFormAsync<LoginInputModel>()

        let returnUrl =
            match ctx.TryGetQueryStringValue "returnUrl" with
            | None -> ""
            | Some q -> q

        let interaction = ctx.GetService<IIdentityServerInteractionService>()
        let schemeProvider = ctx.GetService<IAuthenticationSchemeProvider>()
        let clientStore = ctx.GetService<IClientStore>()

        let! context = interaction.GetAuthorizationContextAsync returnUrl

        match isNotNull context with
        | false ->
            let! loginViewModel =
                buildLoginViewModelFromInputModel interaction schemeProvider clientStore loginInputModel
            return!
                View.loginView loginViewModel
                |> renderHtml ctx
        | true -> return! Controller.redirect ctx "/"
    }

    let loginController = router {
        get "" indexAction
        post "" formPostBackAction
    }