# Issuing a JWT

A JWT is just JSON and so it is entirely possible for us to create a model class representing a JWT and serialize it to JSON ourselves.
However, a JWT is signed and timestamped and it's easy to get it wrong.
As with most security related code, it is best to leave this stuff to the professionals and vetted libraries.
In this case that would be Microsoft's `System.IdentityModel.Tokens.Jwt` library.

## Getting the Namespaces Right

When Microsoft updated the `System.IdentityModel.Tokens.Jwt` library from version 4 to 5 they monkeyed with the namespaces.
In addition, this library brings in some new classes related to claims that overlap what is already present in Web API.
The result is that it is easy to get the namespaces wrong and cause mysterious compilation problems.

Long story short, what you want in your JWT related classes are the following using statements:

```cs
using Microsoft.IdentityModel.Tokens; // not System.IdentityModel.Tokens
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
```

*Version 5+ of `System.IdentityModel.Tokens.Jwt` is only compatible with .NET 4.5 or higher.
If you are running an older version of the framework you should upgrade (preferably) or you can try version 4 of the nuget package.
I haven't use that version, but it does essentially the same thing.*

## Security Configuration

A signed JWT needs at a minimum a secret signing key.

The Microsoft library also asks that we mark the JWT with who issued it and the intended audience.
This allows you to limit the scope of who can consume the token.
For our purposes, we are the issuer and audience, because we are the sole users of the token, but you could change these values if your API is intended for a third-party.

Finally, we also need a timeout value for the token.

We store all this information in the `Web.config`:

```xml
<!-- DO NOT USE THE SAMPLE SIGNING KEY BELOW IN YOUR OWN PROJECT. GENERATE YOUR OWN. -->
<add key="SigningKey" value="4b990cd882af4519878c8e0a94419b0f90b23cd097c8226192ce22d9a619733a" />
<add key="TokenIssuer" value="http://my.website.com" />
<add key="TokenAudience" value="http://my.website.com" />
<add key="TokenLifetimeInMinutes" value="30" />
```

_You'll want to make sure the `SigningKey` is a randomly generated, cryptographically secure value that you keep secret at all times.
**Do not use the default value in the sample project**. Create your own and guard it like you would a connection string to your database._

To make using these values easier they are pulled from the `Web.config` into the `SecurityConfiguration` class.

## CreateToken

The code for creating a JWT token lives in our `UsersController` and looks roughly like this:

```cs
// Create the JWT
var claimsIdentity = new ClaimsIdentity(new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, userId),
    new Claim("name", fullName)
    // And any other bit of (session) data you want....
});

var now = DateTime.UtcNow;
var tokenHandler = new JwtSecurityTokenHandler();
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = claimsIdentity,
    Issuer = SecurityConfiguration.TokenIssuer,
    Audience = SecurityConfiguration.TokenAudience,
    SigningCredentials = SecurityConfiguration.SigningCredentials,
    IssuedAt = now,
    Expires = now.AddMinutes(lifetimeInMinutes)
};

var token = tokenHandler.CreateToken(tokenDescriptor);
```

Pretty simple right? We first create the claims we want to be part of this token.
This can be any custom data you want.
Typically it includes at a minimum the user identifier and by convention this claim is named `"sub"` (think subject) which we do using one of the built-in constants, but it is entire up to you.
If you are the only consumer of the JWT you can do it however you want.

To demonstrate the arbitrary nature of claims you can see how we also add the user's name in a claim called `"name"`.

You can think of this a bit like session state.
These values are going to be included with the JWT every time it comes back to the server so small lookup values are ideal.
*You would not want to abuse this and store large amounts of data in the token because it gets sent back to the server on every authenticated request.*


The hard part is done for us using the `JwtSecurityTokenHandler`.
We just need to give it all the claims we want, the issuer, audience, signing key, and timeout spoken of earlier and it generates a JWT for us.
We then return that token string in our Web API response for the client to store and include with any future authenticated requests.

## Seeing it in Action

To see this in action we can call our `UsersController` using Postman and one of the test credentials:

```
POST http://localhost:30908/api/v1/users/authenticate

{
  "EmailAddress": "liz.lemon@example.com",
  "Password": "Password1"
}
```

and you should get a response like:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMDAxIiwibmFtZSI6IkxpeiBMZW1vbiIsIm5iZiI6MTQ4MDIwMDQ1MywiZXhwIjoxNDgwMjAyMjUzLCJpYXQiOjE0ODAyMDA0NTMsImlzcyI6Imh0dHA6Ly9teS53ZWJzaXRlLmNvbSIsImF1ZCI6Imh0dHA6Ly9teS53ZWJzaXRlLmNvbSJ9.JPMnq7f_GTUwzVB8qaVH6ejFA2XWwdty3uXnh8bcgjg",
  "lifetimeInMinutes": 30,
  "fullName": "Liz Lemon"
}
```

That string labelled `"token"` is our glorious JWT, serialized, signed, and Base64 URL encoded.

## Dissecting the Token

A JWT, as we have just created it, is not secret.
Our claims data and the timestamp are all just JSON data that is then Base64 URL encoded.
To see what that JSON data looks like we can use any number of tools which will decode it.

Here is the decoded data using an online tool referenced at the end of this document:

```
{
 alg: "HS256",
 typ: "JWT"
}.
{
 sub: "3001",
 name: "Liz Lemon",
 nbf: 1480200453,
 exp: 1480202253,
 iat: 1480200453,
 iss: "http://my.website.com",
 aud: "http://my.website.com"
}.
[signature]
```

Wait! What? How is this secure if a simple online tool can decode it?
Because the type of JWT we created is *signed*, not *encrypted*.
The difference is important.

An encrypted value is protected from peering eyes and tampering, while a signed value is projected just from tampering.
So while it is true that a user can decode the JWT, they can't tamper with it and that is the key to making sure that it can be used for authentication.
You can be sure that the JWT you created is *exactly* the same one that comes back to you because it is signed with your secret `SigningKey` that nobody else knows.

*Encrypted JWTs are also supported and only slightly more difficult to create than what we just did above.
But I have yet to find a reason to use them.
When used properly a JWT should only contain lookup values, IDs, etc... and never sensitive data.
Usually a signed JWT is all that is necessary.*

Finally, it should go without saying that you should only ever exchange a JWT over a **secure SSL/HTTPS connection** -- same as you would a cookie.

## References

* [Introduction to JSON Web Tokens](https://jwt.io/introduction/)
* [JSON Web Token Handler](https://msdn.microsoft.com/en-us/library/dn205065(v=vs.110).aspx)
* [System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/)
* [Stack Overflow: Error trying to generate token using .NET JWT library](http://stackoverflow.com/a/38364979)
* [Postman](https://www.getpostman.com/)
* [JWT Decoder](http://calebb.net/)
* [Ultra High Security Password Generator](https://www.grc.com/passwords.htm)
