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

## AuthenticateAsync

Our implementation of the `IAuthenticationFilter` interface is found in the `BearerAuthenticationFilter` class.

Our `AuthenticateAsync` method looks something like this:

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

If a token is found, we run it though the `JwtSecurityTokenHandler` -- the very same utility class we used to create the token.
Not suprisingly, we pass it almost the same arguments, this time to validate the token rather than create it.
Namely, we want to tell it to validate the issuer, audience, and signing key using the values we keep in our `Web.config` via the `SecurityConfiguration` class.
We also ask it to validate the expiration time. This is important and something you do no want to miss.

If the token is valid, the helper class will automatically create the `ClaimsPrincipal` we set for the current user / thread.
That is the key to getting the `[Authorize]` attribute to work. The `AuthorizeAttribute` looks for a current user / thread principal.
If found the request is considered authorized; if not, it is rejected automatically with a `401 Unauthorized` error for routes using the `[Authorize]` attribute.

## Advanced: Customizing the ClaimsPrincipal

For the `JwtSecurityTokenHandler` to set the current `User.Identity.Name` in the principal / identity, the name claim needs to use the key `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`.
The standards for claim names is a little loose and Microsoft has chosen to pursue URL-like claim names, similar to the way DOCTYPE or XML namespaces are done.
You may recall that we put the user's full name in the `"name"` claim when we issued it, but the `JwtSecurityTokenHandler` won't find it there because it is expecting to find it at the claim `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`.
To rectify that, we simply tell the helper class to map `"name"` to `ClaimTypes.Name` (a constant for `"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"`).

Problem solved. Now the `Name` property of the controller `User.Identity.Name` will return the user's full name as expected and we don't have to follow Microsoft's weird convention for claim names.

*This step is only necessary because I'm picky about my JWT claim names. We could have just as easily used the `ClaimTypes.Name` constant when we created the JWT and avoided this mapping step.*

## Using the IAuthenticationFilter

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
We can then use that to make sure the user is only requesting the user record for him / herself, and not another user.

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

To see how authorization works, we can obtain a token from the `api/v1/authenticate` API and then subsequently use it in a few other calls.

The following jQuery code demonstrates a few of the calls in the sample project that either require or don't require authentication.

```js
$.get('http://localhost:30908/api/v1/ping')
  .done(function () {
    // This call will always work because it doesn't have the [Authorize] attribute
  });

$.get('http://localhost:30908/api/v1/ping/authenticated')
  .fail(function () {
    // This call will only work when the request is authenticated because it uses the [Authorize] attribute
  });

var credentials = {
  emailAddress: 'liz.lemon@example.com',
  password: 'Password1'
};

var promise = $.post('http://localhost:30908/api/v1/users/authenticate', credentials);
promise.done(function (data) {
  var jwt = data.token; // Could be stored in localStorage

  $.get({
    url: 'http://localhost:30908/api/v1/ping/authenticated',
    headers: {
      Authorization: 'Bearer ' + jwt
    }
  })
    .done(function () {
      // This call will now succeed because we are passing the JWT in the Authorization header
    });

  $.get({
    url: 'http://localhost:30908/api/v1/users/3001',
    headers: {
      Authorization: 'Bearer ' + jwt
    }
  })
    .done(function () {
      // This call will succeed when we are asking for our own user record, but will fail if we try to access another user's record
    });
});
```