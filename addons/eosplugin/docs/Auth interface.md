Epic Developer [Resources](https://dev.epicgames.com/docs) Auth Interface

# Auth Interface

Interface to handle verification of user accounts, including login and logout functionality.

# Auth Interface

You can use the Auth Interface to let players sign in to their Epic Games account directly from your game. Use the interface to authenticate players, get access tokens, and handle other account-related interactions with Epic Online Services. When a player signs in to their Epic Games account, you can give them access to other Epic Account Services such as Friends, Presence, User Info and Ecom Interfaces.

If you have a userless client setup (i.e. your Client Policy [doesn't](https://dev.epicgames.com/docs/en-US/dev-portal/client-credentials#policy-types) require a user), you can initialize the client using just the cient\_credentias . You don't need any user information.

To use the Auth Interface, your game (product) must have Epic Account Services (EAS) active, and must obtain user [consent](https://dev.epicgames.com/docs/en-US/epic-account-services/consent-management) to access Basic Profile data. You can activate EAS on the [Developer](https://dev.epicgames.com/docs/en-US/dev-portal) Portal, or learn more in Epic's [documentation](https://dev.epicgames.com/docs/en-US/epic-account-services). Without EAS and user consent, you can still initialize the EOS SDK and the Auth Interface, but all Auth Interface function calls to the back-end service fail.

**Note:** To use Epic Online Services (EOS) SDK, your local network, router, and firewall must allow access to specific host addresses. For a complete list of these host addresses, see the Firewall [Considerations](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/firewall-considerations) documentation.

**Note:** This service is also available as a web API. See the [Auth](https://dev.epicgames.com/docs/en-US/web-api-ref/authentication) Web APIs documentation for more information.

# Differences between Auth Interface and Connect Interface

You can also use the Connect Interface to let players sign in to your game through a supported identity provider. See the Connect [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-connect-interface) [documentation](https://dev.epicgames.com/docs/en-US/game-services/eos-connect-interface) for more information on using the Connect Interface in your game. The differences between the Auth Interface and Connect Interface are listed in the table below:

<span id="page-0-0"></span>

|                          | Auth Interface                                                                                                                                | Connect Interface                                                                                                                                                                   |
|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Player ID                | The player's Epic Games account ID. The player can<br>sign in with an email and password combination, or a<br>linked external account1.       | A Product User ID (PUID) for a player, that's linked to one<br>or more supported identity providers2. Note that the PUID<br>is specific to a single product.                        |
| Access                   | The player can sign in to Epic Account Services across<br>any platform (PC (Windows, Mac, and Linux), console,<br>and mobile) and storefront. | The player can authenticate their account in your game to<br>access EOS Game Services across any platform (PC<br>(Windows, Mac, and Linux), console, and mobile) and<br>storefront. |
| Supported<br>Services    | The player has access to the Friends3, Presence4, and<br>Ecom5 interfaces, as well as the EOS Social Overlay6.                                | The player has access to Game Services across<br>Multiplayer, Progression, Moderation, and Operations.                                                                              |
| Access Token<br>Lifetime | The game automatically refreshes the token as long as<br>it calls EOS_Platform_Tick7.                                                         | The game must periodically refresh the player's token<br>based on expiration notification events8.                                                                                  |

# Authentication Functions

To access authentication functions, you must first acquire an EOS\_HAuth handle. Auth Interface functions require this handle to access user information. To acquire an EOS\_HAuth handle, call the Platform Interface's function [EOS\\_Patform\\_GetAuthInterface](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-platform-get-auth-interface) . See the Platform [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-platform-interface) documentation for more information on how to use the Platform Interface.

# **Logging In**

To begin interacting with EAS's online features, players must first log in with a valid Epic Account. To set this up so that players can do this, call the [EOS\\_Auth\\_Login](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-login) function with an [EOS\\_Auth\\_LoginOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-auth-login-options) structure containing a local player's **account** credentials. Whether the login attempt succeeds or fails, your callback function, of type [EOS\\_Auth\\_OnLoginCaback](https://dev.epicgames.com/docs/en-US/api-ref/callbacks/eos-auth-on-login-callback) , runs upon completion.

The brand review process verifies your game's brand with Epic Games. Once verified, players outside of your **organization** can use your game's integration of Epic Account Services. Prior to brand review, players receive an error if they try to log into your game with an external account. See the documentation on the Brand Review [Process](https://dev.epicgames.com/docs/en-US/epic-account-services/brand-review) for more details.

The EOS\_Auth\_LoginOptions must be initialized with its ApiVersion variable set to EOS\_AUTH\_LOGIN\_API\_LATEST , and its Credentias variable (of type [EOS\\_Auth\\_Credentias](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-auth-credentials) ) containing the following information:

| Property                     | Value                                                                                                                                                                              |
|------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion                   | EOS_AUTH_CREDENTIALS_API_LATEST                                                                                                                                                    |
| Id                           | The identity of the user logging in. Unlike most other functions, this should be a user-readable identity,<br>like an email address or display name.                               |
| Token                        | The user's login credentials or authentication token.                                                                                                                              |
| Type                         | The type of credential that this login attempt is using. EOS_ELoginCredentialType lists the available<br>kinds of credentials.                                                     |
| SystemAuthCredentialsOptions | This field is for system specific options, if any are needed.                                                                                                                      |
| ExternalType                 | If Type is set to EOS_LCT_ExternalAuth , this field indicates which external authentication method<br>to use. See EOS_EExternalCredentialType for a list of all available methods. |

Pass the Auth Interface handle, your EOS\_Auth\_LoginOptions structure, and your callback information to the function. Provided that the EOS\_HPatform handle is ticking, the callback you provided runs when the operation finishes.

# **Preferred Login Types for Epic Account**

The preferred login types by platform are as follows:

| Platform             | Login Type                                           | Summary                                                                                                                                                                                            |
|----------------------|------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Epic Games Launcher  | EOS_LCT_ExchangeCode                                 | Exchange code received from the launcher and used to<br>automatically login the user.                                                                                                              |
| Nintendo Switch      | EOS_LCT_AccountPortal with<br>EOS_LCT_PersistentAuth | Users are prompted to login using their Epic account<br>credentials, after which a long-lived refresh token is stored<br>locally to enable automatic login across consecutive<br>application runs. |
| PlayStation and Xbox | EOS_LCT_ExternalAuth                                 | Platform access token used to automatically login the<br>platform user to their associated Epic account.                                                                                           |

#### 5/26/25, 7:04 AM Auth Interface | Epic Online Services Developer

| Platform                                                                          | Login Type                                           | Summary                                                                                                                                                                                            |
|-----------------------------------------------------------------------------------|------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Steam Client                                                                      | EOS_LCT_ExternalAuth                                 | Steam Session Ticket used to automatically login the Steam<br>user to their associated Epic account.                                                                                               |
| Other store platforms and<br>standalone distributions on<br>PC and Mobile Devices | EOS_LCT_AccountPortal with<br>EOS_LCT_PersistentAuth | Users are prompted to login using their Epic account<br>credentials, after which a long-lived refresh token is stored<br>locally to enable automatic login across consecutive<br>application runs. |

# Epic Games Launcher

<span id="page-2-0"></span>When an application associated with the Epic Games Launcher starts, the launcher provides a command line with the following parameters:

-AUTH\_LOGIN=unused -AUTH\_PASSWORD=<password> -AUTH\_TYPE=exchangecode -epicapp=<appid> -epicenv=Prod -EpicPorta epicusername=<username> -epicuserid=<userid> -epicocae=en-US -epicsandboxid=<sandboxid> -epicdepoymentid= <depoymentid>

**Note:** If you have not specified a deployment ID for the artifact for the application, then the Epic Games Launcher does not send the epicdepoymentID parameter. For information on how to set a deployment ID on an artifact, see Manage Artifacts: Select [Deployment](https://dev.epicgames.com/docs/en-US/epic-games-store/store-presence/manage-artifacts#step-5-select-deployment-id-for-epic-online-services-only) ID.

The important fields of this command line are as follows:

| Property      | Value                                                                                                                  |
|---------------|------------------------------------------------------------------------------------------------------------------------|
| AUTH_LOGIN    | This field might be the user ID, but it is presently unused.                                                           |
| AUTH_PASSWORD | This field is the Exchange Code itself, which should be provided as the Token during login.                            |
| AUTH_TYPE     | The type reads "exchangecode", indicating that EOS_Auth_LoginCredentials should use the type<br>EOS_LCT_ExchangeCode . |

The application must parse this information and pass it into EOS\_Auth\_Login through the EOS\_Auth\_Credentias structure. EOS\_Auth\_Credentias has three variables: Id , Token , and Type . You can leave Id blank, as this login method does not require an ID. For Token , provide the Exchange Code from the AUTH\_PASSWORD command line parameter. Finally, Type should be EOS\_LCT\_ExchangeCode .

## Nintendo Switch

Your game stores a long-lived Epic refresh token on the local device for automatic login across game sessions. See the section on [persistent](#page-3-0) logins for more information.

#### PlayStation, Steam, and Xbox

Your game retrieves an access token from the platform for the local user account. Using the EOS\_LCT\_ExternaAuth login type, the platform user is logged into their Epic account. See the section on External Account [Authentication](#page-4-0) for the detailed login flow, and the console specific documentation for the platform code integration.

You can only access console documentation if you have the appropriate permissions. See the Get Started Steps: EOS SDK [Download](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/services-quick-start#eos-sdk-download-types) Types documentation for more information on how to access the EOS SDKs for consoles and their associated documentation.

## PC and Mobile Devices

PCs and mobile devices usually use persistent logins, enabled by long-lived refresh tokens granted by the authentication backend, and specific to the device and user account. On these platforms, the SDK automatically stores and retrieves these tokens as needed, and updates them following each login. See the section on [persistent](#page-3-0) logins for more information.

## Auth Scopes

As of EOS SDK version 1.5, EOS\_Auth\_LoginOptions contains a new field named ScopeFags , of type [EOS\\_EAuthScopeFags](https://dev.epicgames.com/docs/en-US/api-ref/enums/eos-e-auth-scope-flags) . Scopes are a set of permissions that are required for your application to function properly. For example, if your application needs to see the user's friends list, then you must request the EOS\_AS\_FriendsList scope, and the user is then asked to give consent for it during the login flow. If the user does not consent to one of the requested scopes, then the login fails. When requesting consent, your request must exactly match the scopes configured for the **product** in the Developer Portal.

User scope consent is prompted on the first login only for all login types except EOS\_LCT\_AccountPorta . For EOS\_LCT\_AccountPorta , scope consent is prompted on every login.

Multiple users can be logged in at the same time on a single local device and using the same shared EOS\_HPatform instance

## **Persisting user login to Epic Account outside Epic Games Launcher**

On PC and mobile platforms, to support persistent user login outside of the Epic Games Launcher, use the EOS\_LCT\_AccountPorta login type.

<span id="page-3-0"></span>The SDK automatically receives a refresh token from the authentication backend after a successful login to the user's Epic Account. It stores the refresh token in the local keychain of the locally logged-in user on the device. For the local keychain, the SDK uses the secure credentials store provided by the device's operating system.

When automatically logging in the local user, the game should first call EOS\_Auth\_Login with the EOS\_LCT\_PersistentAuth login type. The Id and Token input fields should be set to NULL since the SDK manages the long-lived access credentials. The SDK then checks for a refresh token in the keychain of the local user, and it automatically uses a token, if it finds one, to log the user into their Epic Account. Following a successful login on those platforms, the SDK automatically updates the refresh token in the local keychain.

If EOS\_Auth\_Login fails for any reason, proceed with the default login method for the platform. If EOS\_Auth\_Login finds a refresh token but fails to log in because the server rejected the token — meaning the call fails for a reason other than not having a token, connection or service issues, or the operation canceling or waiting to retry — the application should delete the token, since it is obsolete and will continue to cause failures in all future sessions. Call [EOS\\_Auth\\_DeetePersistentAuth](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-delete-persistent-auth) to explicitly remove any stored credentials in the local keychain for the user. The application should then proceed to the platform's default login flow.

In the case that a logged-in user wants to disable automatic login, call [EOS\\_Auth\\_Logout](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-logout) to log out, and EOS\_Auth\_DeetePersistentAuth to revoke the user's long-lived logon session on the authentication backend. This also deletes the long-lived refresh token from the keychain of the local user.

**Note:** When you change the target EOS SDK deployment, you must delete stored credentials using [EOS\\_Auth\\_DeetePersistentAuth](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-delete-persistent-auth) API. If you do not delete stored credentials in this circumstance, some EOS SDK API calls return an HTTP 403 (forbidden) error.

# **Logging Out**

To log out, call the EOS\_Auth\_Logout function with an [EOS\\_Auth\\_LogoutOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-auth-logout-options) data structure. When the operation completes, your callback function, of type [EOS\\_Auth\\_OnLogoutCaback](https://dev.epicgames.com/docs/en-US/api-ref/callbacks/eos-auth-on-logout-callback) , runs. Initialize your EOS\_Auth\_LogoutOptions structure as follows:

| Property    | Value                      |
|-------------|----------------------------|
| ApiVersion  | EOS_AUTH_LOGOUT_API_LATEST |
| LocalUserId | The EOS_EpicAccountId      |

Pass the Auth Interface handle, your EOS\_Auth\_LogoutOptions structure, and your callback function to EOS\_Auth\_Logout . Provided that the EOS\_HPatform handle is ticking, the callback you provided runs when the operation finishes.

If the EOS\_LCT\_PersistentAuth login type has been used, be sure to also call the function EOS\_Auth\_DeetePersistentAuth to revoke the longlived logon session on the authentication backend. This also permanently forgets the local user login on the local device.

# **Status Change Notification**

The EOS SDK periodically verifies local users' authentication status during the application's lifetime. This helps to make sure that the user hasn't signed in elsewhere or otherwise lost access for reasons external to the application itself. To assure that your application knows whenever a user's authentication status has changed, the Auth Interface invokes a callback of type [EOS\\_Auth\\_OnLoginStatusChangedCaback](https://dev.epicgames.com/docs/en-US/api-ref/callbacks/eos-auth-on-login-status-changed-callback) upon any such change for any local player. You can attach your own callback function to this process with the [EOS\\_Auth\\_AddNotifyLoginStatusChanged](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-add-notify-login-status-changed) function.

The EOS\_Auth\_OnLoginStatusChangedCaback callback function you provide runs whenever a local user's authentication status changes. This includes explicitly logging in and out with the Auth Interface, meaning that you receive both the callback for the log in (or log out) event as well as this callback.

Connectivity loss during an application's lifetime does not indicate that a user is logged out. The EOS backend explicitly notifies the Auth Interface when a logout event takes place, and this is the only case in which it is safe to assume that the user is officially considered offline. User connectivity problems such as service outages, or local hardware failure can cause various API features to fail. If the game can continue without these interactions, the recommended course of action is to continue playing with the assumption that connectivity might eventually resume without logging the user out.

# **Checking Current Authentication Status**

To check the player's current status on demand, use the [EOS\\_Auth\\_GetLoginStatus](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-get-login-status) function. This function determines authentication status based on the most recent communication with the online service, so the result is returned instantly, and does not use a callback function.

#### **External Account Authentication**

To log in with EOS\_Auth\_Login using an external account, set Type in EOS\_Auth\_Credentias to EOS\_LCT\_ExternaAuth , set ExternaType to an external credential type (See [EOS\\_EExternaCredentiaType](https://dev.epicgames.com/docs/en-US/api-ref/enums/eos-e-external-credential-type) for a list of all available methods), and set Token to the external authentication token. For example, if you want to log in with Steam, you would use EOS\_ECT\_STEAM\_SESSION\_TICKET as the ExternaType , and the Token would be the Steam Session Ticket.

<span id="page-4-0"></span>EOS\_Auth\_Login returns the error EOS\_InvaidUser when the external auth login fails due to an external account not being linked. An EOS\_ContinuanceToken is set in the EOS\_Auth\_LoginCabackInfo data. EOS\_Auth\_LinkAccount should be called with the EOS\_ContinuanceToken and LinkAccountFags set to EOS\_LA\_NoFags (for most cases where consent is required via the Account Portal or PIN Grant) to continue the external account login and link the external account. Afterwards, the external account is linked to the user's Epic Account.

The Identity Providers on the Developer Portal need to be configured for the Product to allow providers to be linked using external account authentication. See the [Configure](https://dev.epicgames.com/docs/en-US/dev-portal/identity-provider-management#configure-identity-providers-for-your-product) Identity Providers for Your Product documentation for more information.

![](_page_5_Figure_2.jpeg)

# **Integrating a Game Launcher with Epic Games Store**

If your game provides a launcher to include additional launch options, promotions or other news, then your launcher must manage the login flow. Exchange codes generated by the Epic Games Launcher expire after a short period of time, so care must be taken to prevent the exchange code from expiring. Use the following pattern when the Epic Games Launcher is not directly launching the game application:

- 1. The Epic Games Launcher starts the third-party launcher, passing the exchange code on the command line as described above in the section [Epic](#page-2-0) Games [Launcher.](#page-2-0)
- 2. The third-party launcher uses the Exchange Code to login the player by using the EOS\_Auth\_Login API. Initialize EOS\_Auth\_LoginOptions by setting the Type and Token fields of the EOS\_Auth\_Credentias struct to EOS\_LCT\_ExchangeCode and the exchange code from the command line respectively.
- 3. When the player chooses to launch the game, use the EOS\_Auth\_CopyUserAuthToken API to get a copy of the token details. Copy the RefreshToken from the EOS\_Auth\_Token and call the EOS\_Auth\_Token\_Reease API to free the memory allocated by the SDK.
- 4. Pass the refresh token to the game application by setting an environment variable that the game can read on startup. Do not log the player out in the third-party launcher when it exits as this will invalidate the refresh token.
- 5. When the game process starts up, the game can log the player in using the EOS\_Auth\_Login API. Initialize EOS\_Auth\_LoginOptions by setting the Type and Token fields of the EOS\_Auth\_Credentias struct to EOS\_LCT\_RefreshToken and the refresh token from the environment variable respectively.

# User Verification Using an ID Token

ID Tokens are part of the OpenID [Connect](https://openid.net/specs/openid-connect-core-1_0.html#IDToken) protocol and can be used to verify a user's identity on server-side. An ID Token is a JSON Web Token (JWT) that contains information about the authenticated user, such as their account id. This allows backend services and game servers to securely verify user identifiers it receives from clients.

ID Tokens can not be used to execute actions on behalf of a user. They are only intended for the use of user identity verification.

## **Retrieving an ID Token For User**

Game clients can obtain ID Tokens for local users by calling the EOS\_Auth\_CopyIdToken SDK API after the user has been logged in, passing in a EOS\_Auth\_CopyIdTokenOptions structure containing the EOS\_EpicAccountId of the user.

The outputted [EOS\\_Auth\\_IdToken](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-auth-id-token) structure contains the EOS\_EpicAccountId of the user, and a JWT representing the ID Token data. Note, you must call EOS\_Auth\_IdToken\_Reease to release the ID Token structure when you are done with it.

Once retrieved, the game client can then provide the ID Token to another party. An ID Token is always readily available for a logged in local user.

## **Validating ID Tokens on Game Server Using SDK**

The JSON Web Key Set (JWKS) endpoint for EOS Auth ID Tokens is: [https://api.epicgames.dev/epic/oauth/v2/.well-known/jwks.json.](https://api.epicgames.dev/epic/oauth/v2/.well-known/jwks.json)

Game servers can validate ID Tokens by calling the [EOS\\_Auth\\_VerifyIdToken](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-auth-verify-id-token) SDK API, and passing in a [EOS\\_Auth\\_VerifyIdTokenOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-auth-verify-id-token-options) containing the EOS\_Auth\_IdToken . Note, game servers should use [EOS\\_EpicAccountId\\_FromString](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-epic-account-id-from-string) to populate the EOS\_EpicAccountId part of the EOS\_Auth\_IdToken structure before calling verify, because the server's user handles are different to the user handles on the client.

#### **Validating ID Tokens on Backend Without SDK**

Backend services can verify the validity of ID Tokens and extract the token claims using any of the publicly available standard JWT libraries. See <https://jwt.io/> for a list of libraries for this purpose. The used library should allow automatic caching of the retrieved JWKS information for the best performance and to reduce networking overhead.

The EOS SDK and other support libraries take care of the needed steps for securely validating the ID Token before its containing claims can be securely trusted. The steps performed are as following:

- 1. Verify that the token signature algorithm ("alg") is present and is not set to "none".
- 2. Verify the JWT signature against the expected public certificate using the JWKS endpoint hosted by Epic Online Services.
- 3. Verify that the token issuer ("iss") is present and starts with the base URL of [https://api.epicgames.dev.](https://api.epicgames.dev/)
- 4. Verify that the token issue time ("iat") is in the past.
- 5. Verify that the token expiration time ("exp") is in the future.
- 6. Verify that the Client ID ("aud") matches the Client ID that you are using to initialize the EOS SDK with game clients or otherwise to authenticate users with Epic Account Services.

After successfully verifying the ID Token you can trust the Epic Account ID ("sub") value.

## **ID Token Structure**

The ID Token contains the following JSON structures:

#### **Header**

#### **Payload**

| Key   | Type    | Description                                                                                                                                                                                    |
|-------|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| appid | string  | EAS Application ID.                                                                                                                                                                            |
| aud   | string  | Client ID used to authenticate the user with Epic Account Services.                                                                                                                            |
| cty   | string  | Country code that the Epic Account has been registered with, in ISO 3166 2-letter format. This optional claim will<br>be present if the application has requested the country scope.           |
| dn    | string  | Epic Account display name.                                                                                                                                                                     |
| exp   | integer | Expiration time of the token, seconds since the epoch.                                                                                                                                         |
| iat   | integer | Issue time of the token, seconds since the epoch.                                                                                                                                              |
| iss   | string  | Token issuer. Always starts with https://api.epicgames.dev .                                                                                                                                   |
| pfdid | string  | EOS Deployment ID.                                                                                                                                                                             |
| pfpid | string  | EOS Product ID.                                                                                                                                                                                |
| pfsid | string  | EOS Sandbox ID.                                                                                                                                                                                |
| sub   | string  | Epic Account ID of the authenticated user.                                                                                                                                                     |
| eat   | string  | External acccount type. This optional claim will be present if the user has logged in to their Epic Account using<br>external account credentials, e.g. through local platform authentication. |
| eadn  | string  | External account display name. This claim might not be always present.                                                                                                                         |
| pltfm | string  | Platform that the user is connected from. Included if the eat claim is present. Possible values include:<br>• other<br>• playstation<br>• steam<br>• switch<br>• xbox                          |

# Footnotes

- 1. See the Auth [Interface](https://dev.epicgames.com/docs/en-US/epic-account-services/auth/auth-interface#external-account-authentication) documentation for more information on linked external accounts. [↩](#page-0-0)
- 2. See the Identity Provider [Management](https://dev.epicgames.com/docs/en-US/dev-portal/identity-provider-management) documentation for more information on identity providers. [↩](#page-0-0)
- 3. See the Friends [Interface](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-friends-interface) documentation for more information on using the Friends Interface. [↩](#page-0-0)
- 4. See the [Presence](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-presence-interface) Interface documentation for more information on using the Presence Interface. [↩](#page-0-0)
- 5. See the Ecom [Interface](https://dev.epicgames.com/docs/en-US/epic-games-store/services/ecom/ecom-overview) documentation for more information on using the Ecom Interface. [↩](#page-0-0)
- 6. See the Social Overlay [Overview](https://dev.epicgames.com/docs/en-US/epic-account-services/social-overlay-overview) documentation for more information on the EOS social overlay. [↩](#page-0-0)
- 7. See the Api [Reference](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-platform-tick) documentation for more information on calling EOS\_Patform\_Tick . [↩](#page-0-0)
- <span id="page-7-0"></span>8. See the Connect [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-connect-interface#user-authentication-refresh-notification) documentation for more information on expiration notification events. [↩](#page-0-0)
