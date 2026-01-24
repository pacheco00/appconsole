using Microsoft.Identity.Client;

/*Declarando cliente_id y el tenant_id*/
string _clientId = "<client_id>";
string _tenantId = "<tenant_id>";

/*Definimos los alcances  y creamos el cliente de auth*/
string[] _scopes = { "User.Read" };

/*Cliente de MSAL de microsoft*/
var app = PublicClientApplicationBuilder
          .Create(_clientId)
          .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
          .WithDefaultRedirectUri()
          .Build();

/*Código que solicita el token de acceso*/

AuthenticationResult result;

try
{
    /*Intentar obtener el token de acceso desde cache*/
    var accounts= await app.GetAccountsAsync();
    result = await app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
        .ExecuteAsync();

}
catch (MsalUiRequiredException ex) 
{ 
    Console.WriteLine($"Cache token no esta {ex.Message}");
    result = await app.AcquireTokenInteractive(_scopes).ExecuteAsync();

}


/*Imprimir en consola el token de acceso*/
Console.WriteLine($"Access Token: \n {result.AccessToken}");








