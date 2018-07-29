module IdentityConfig

open IdentityServer4.Models
open IdentityServer4.Test

let testUsers =
    [ TestUser(SubjectId = "b1c303e7-c946-4fd7-a711-615568c96926",
               Username = "gregor",
               Password = "password") ]
    |> ResizeArray<TestUser>

let identityResources : IdentityResource seq =
    seq [ IdentityResources.OpenId(); IdentityResources.Profile() ]

let clients : Client seq =
    let client =
        Client(ClientId = "client",
               AllowedGrantTypes = GrantTypes.ClientCredentials,
               ClientSecrets = (seq [ Secret("secret".Sha256()) ] |> ResizeArray<Secret>),
               AllowedScopes = (seq [ "openid"; "profile" ] |> ResizeArray<string> ) )

    seq [ client ]