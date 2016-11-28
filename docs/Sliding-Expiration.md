# Reissusing a JWT with a New (Sliding) Expiration

*This document supposes that you have already read and understand the guides on [Issuing a JWT](Issuing-JWT.md) and [Validating a JWT](Validating-JWT.md).*

I was once on a project where the developers were using JWT for authentication but it had an absolute expiration.
I asked them to make it a sliding expiration and their response was that it would be a huge development task because they would have to potentially return a new JWT in any / every response and would therefore need to add it is a property in the response model for every call.
At this point I face palmed and started working on this sample project to illustrate how much easier it is than that when you understand the HTTP protocol and the Web API pipeline.

The JWT is a piece of authenticating information -- metadata about the request / response, not part of the request / response content.
Thus it belongs in an HTTP header that describes the request / response (metadata), not part of the request / response content itself.
Think of it like a cookie.
A cookie is sent by the browser to the server in the `Cookie` header.
The server sends an updated cookie to the browser in the `Set-Cookie` header.
To achieve "sliding expiration" with JWT we are going to do essentially the same thing.

In our case the JWT is sent to the server in the `Authorization` header.
Following the pattern used by cookies, we can return an updated JWT in the response in the `Set-Authorization` header.
*The `Set-Authorization` header is not a standard header. I came up with it following the naming convention for cookies.
You can call your response header anything you want because the HTTP specification allows any number of arbitrary headers.*

So do we need to modify every Web API method to inspect the request and place a new JWT in the response headers? No.
Web API has a pipeline for processing requests.
We only need to tap into that pipeline so that we can run every request / response through a custom handler, a `DelegatingHandler` to be exact.

## SlidingExpirationHandler

The class I created to do this work is the `SlidingExpirationHandler` and it derives from the `DelegatingHandler` class.
The `DelegatingHandler` has a single method, `SendAsync` that we can override to massage the request or response.

Below are the significant parts of that method:

```cs
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    var response = await base.SendAsync(request, cancellationToken);

    // Preflight check 1: did the request come with a token?
    var authorization = request.Headers.Authorization;
    if (authorization == null || authorization.Scheme != "Bearer" || string.IsNullOrEmpty(authorization.Parameter))
    {
        // No token on the request
        return response;
    }

    // Preflight check 2: did that token pass authentication?
    var claimsPrincipal = request.GetRequestContext().Principal as ClaimsPrincipal;
    if (claimsPrincipal == null)
    {
        // Not an authorized request
        return response;
    }

    // Extract the claims and put them into a new JWT
    var fullName = claimsPrincipal.Identity.Name;
    var userId = claimsPrincipal.Identity.GetUserId();
    var lifetimeInMinutes = int.Parse(WebConfigurationManager.AppSettings["TokenLifetimeInMinutes"]);

    var token = UsersController.CreateToken(userId, fullName, lifetimeInMinutes);
    response.Headers.Add("Set-Authorization", token);

    return response;
}
```

What we first do is process the request in it's entirety so we can intercept the response.
We then do a few preflight checks to make sure this was an authenticated request to begin with.
If it is, we create a new JWT using the data from the one passed in, although this time with a new expiration, and send it back in the `Set-Authorization` header.
Couldn't be easier.

If we wanted to get fancy we could minimize how often we respond with an updated JWT by calculating the time left before expiration and only issue a new one if it is about to expire.

Remember, we only get a valid `ClaimsPrincipal` if the request was authenticated with a valid JWT to begin with, so there is no danger of us returning an updated JWT to a caller who never had one in the first place.

## Registering the Handler

The only thing that remains is to make sure this handler gets run on every request / response.
We do that by registering it at application startup like so:

```cs
public static void Register(HttpConfiguration config)
{
    // Web API routes
    config.MapHttpAttributeRoutes();

    // Configure the sliding expiration handler to run on every request
    config.MessageHandlers.Add(new SlidingExpirationHandler());
}
```

## Seeing it in Action

Obviously our client code needs to understand that an updated JWT may be returned in the `Set-Authentication` header of any of our Web API responses.
Fortunately most AJAX libraries, including jQuery, have mechanisms to run the same bit of code on every call similar to the way we can run a handler for every request / response in the Web API pipeline.
If we use these hooks to look for an updated JWT and update our copy in memory, we can automatically get the latest JWT from any response and use it in any future requests.

Here is just one way this can be done using jQuery:

```js
var token;

$(document).ajaxComplete(function (event, jqXHR, ajaxOptions) {
  if (jqXHR.status >= 200 && jqXHR.status < 400) {
    var newToken = jqXHR.getResponseHeader('Set-Authorization');
    if (newToken) {
      token = newToken;
    }
  }
});
```

In practice, we would probably store the `token` in `localStorage` or `sessionStorage` and not a simple in-memory variable, but you get the idea.

## References
* [HTTP Message Handlers in ASP.NET Web API](https://www.asp.net/web-api/overview/advanced/http-message-handlers)
* [jQuery ajaxComplete event](http://api.jquery.com/ajaxcomplete/)
* [Stack Overflow: jQuery and AJAX response header](http://stackoverflow.com/a/4236041)
* [HTTP cookies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies)