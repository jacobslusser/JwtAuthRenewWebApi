# Validating a JWT

Web API already has some pretty nice features built-in for handling authenticated requests.
Ideally what we want to do is make sure that our JWT validation taps into that so we can benefit from what's already there.

*If you haven't already read and understand the guide on [Issuing a JWT](Issuing-JWT.md) you should do that before going any further.
As already stated there, make sure you get your `using` statements correct so we can leverage the `System.IdentityModel.Tokens.Jwt` library for validating JWTs.*

## Creating an IAuthenticationFilter

The method by which we can tap into the Web API authentication pipeline is to create a class which implements the `IAuthenticationFilter` interface.
More information about this interface can be found in the references at the end of this document, but the key is to implement the `AuthenticateAsync` method which has the following signature:

```cs
public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken);
```

Our job will be to look for a JWT in the request, and if found and valid, set the current user / thread principal.
All of Web APIs authorization checks, including the `[Authorize]` attribute depend on that.
If the user is not authenticated, we leave the current principal as is and Web API will do the work for us of rejecting the request with a `401 Unauthorized` error.

*Microsoft would have you believe that you also need a full implementation of the `IAuthenticationFilter.ChallengeAsync` method as well, but that is not true.
The HTTP spec allows for a special "challenge" response to be sent in response to an unauthenticated request that the browser will then use to prompt the user for their credentials.
Since we are serving data of our Web API, not UI, there is no need for this prompting of the users for credentials.
We can ignore the `ChallengeAsync` method and let any client code (JavaScript) handle the error code (`401 Unauthorized`) appropriately.*

## AuthenticateAsync

Our implementation of the `IAuthenticationFilter` interface is found in the `BearerAuthenticationFilter` class and this is what our `AuthenticateAsync` method looks like:

```cs
public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
{
    var request = context.Request;
    var authorization = request.Headers.Authorization;

    if (authorization == null || authorization.Scheme != "Bearer")
    {
        // Not a Bearer authorized request.
        // Just let it pass through without a Principal
        return;
    }

    if(string.IsNullOrEmpty(authorization.Parameter))
    {
        context.ErrorResult = new UnauthorizedResult(null, request);
        return;
    }

    // Validate the JWT
    var token = authorization.Parameter;
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = SecurityConfiguration.TokenIssuer,

        ValidateAudience = true,
        ValidAudience = SecurityConfiguration.TokenAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SecurityConfiguration.SecurityKey,

        RequireExpirationTime = true,
        ValidateLifetime = true
    };

    try
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.InboundClaimTypeMap["name"] = ClaimTypes.Name;

        SecurityToken securityToken;
        var principal = context.Principal = tokenHandler.ValidateToken(token, validationParams, out securityToken);
    }
    catch(Exception ex)
    {
        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
    }
}
```

As is the custom, we look for the JWT token in the `Authorization` HTTP header using the `"Bearer"` scheme.
If the header is not present, we assume this to be an unauthenticated request, exit the method, and let the Web API pipeline process as usual.

If a token is found, we run it through the `JwtSecurityTokenHandler` -- the very same utility class we used to create the token.
Not surprisingly, we pass it almost the same arguments, this time to validate the token rather than create it.
Namely, we want to tell it to validate the issuer, audience, and signing key using the values we keep in our `Web.config` via the `SecurityConfiguration` class.
We also ask it to validate the expiration time. This is important and something you do not want to miss; otherwise, a token would never be considered expired and could be used forever.
The token is valid when the payload hash matches the attached signature using the signing key, the issuer and audience match our whitelist, *and* the token's expiration date/time has not passed.

If the token is valid, the helper class will automatically create a `ClaimsPrincipal` that we can use to set the current user.
That is the key to getting the `[Authorize]` attribute to work. The `AuthorizeAttribute` looks for a current user / thread principal.
If found the request is considered authorized; if not, it is rejected automatically with a `401 Unauthorized` error.

## Advanced: Customizing the ClaimsPrincipal

For the `JwtSecurityTokenHandler` to set the current `User.Identity.Name` in the principal / identity, the name claim needs to use the key `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`.
The standards for claim names is a little loose and Microsoft has chosen to pursue URL-like claim names, similar to the way DOCTYPE or XML namespaces are done.
You may recall that we put the user's full name in the `"name"` claim when we issued it, but the `JwtSecurityTokenHandler` won't find it there because it is expecting to find it at the claim `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`.
To rectify that, we simply tell the helper class to map `"name"` to `ClaimTypes.Name` (a constant for `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`).

Problem solved. Now the `Name` property of the controller `User.Identity.Name` will return the user's full name as expected and we don't have to follow Microsoft's weird convention for claim names.

*This step is only necessary because I'm picky about my JWT claim names. We could have just as easily used the `ClaimTypes.Name` constant when we created the JWT and avoided this mapping step.*

## Registering the Filter

We want the `BearerAuthenticationFilter` class we created to run on every request, so the last step to getting this setup is to register the filter at startup in the `WebApiConfig`:

```cs
public static void Register(HttpConfiguration config)
{
    // Web API routes
    config.MapHttpAttributeRoutes();

    // Configure the authentication filter to run on every request marked with the AuthorizeAttribute
    config.Filters.Add(new BearerAuthenticationFilter());
}
```

## Using the Claims Data

Once the `ClaimsPrincipal` is set, we can make use of it in any controller that needs that lookup data.
For example, below we pull the current user ID from the `ClaimsPrincipal`.
We can then use that to enforce permissions and make sure the calling user can only request his / her own record, and not another user's.

```cs
[Authorize]
[Route("{userId:long}")]
public async Task<IHttpActionResult> GetUser(long userId)
{
    // Example of using the JWT claims to ensure a user can only access their own user information
    if (userId.ToString() != User.Identity.GetUserId())
        return Unauthorized();

    var user = users.First(u => u.UserId == userId);
    return Ok(user);
}
```

Hopefully you can see how this idea can be expanded. Any of the claims data we previously stored can be pulled back out again and used in the request.
It has been validated and attached the the current thread / user principal for our convenience.

In most / all cases this renders the use of "session state" obsolete.

## Seeing it in Action

To see how authorization works, we can obtain a token from the `api/v1/users/authenticate` API and then use it in another call such as `api/v1/ping/authenticated`.
If we call `ping/authenticated` without a token we'll get a `401 Unauthorized` error because the route is tagged with the `[Authorize]` attribute.
If on the other hand, we call `users/authenticate` first, the token will get stored in a `token` variable and our call to `ping/authenticated` will succeed.

The following jQuery code demonstrates those two calls and is an excerpt from the `Example.html` file included with the project:

```js
var token;

$('#ping').submit(function (event) {
    event.preventDefault();
    var settings = {
        url: 'api/v1/ping/authenticated',
        headers: {
            // Include the JWT in the Authorization header
            Authorization: (token ? 'Bearer ' + token : null)
        }
    };
    $.get(settings)
        .then(function () {
            alert('Success');
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            alert('Unauthenticated or unexpected error.');
        });
});

$('#authenticate').submit(function (event) {
    event.preventDefault();
    var credentials = $(this).serialize();
    $.post('api/v1/users/authenticate', credentials)
        .then(function (user) {
            // The JWT is returned in the token property
            token = user.token;
            alert('Success');
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            alert('Invalid email address and / or password, or unexpected error.');
        });
});
```
## References
* [JSON Web Token Introduction](https://jwt.io/introduction/)
* [Authentication Filters in ASP.NET Web API 2](https://www.asp.net/web-api/overview/security/authentication-filters)