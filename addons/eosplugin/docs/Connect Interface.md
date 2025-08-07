Epic Developer [Resources](https://dev.epicgames.com/docs) Connect Interface

# Connect Interface

The Connect Interface handles connections between users' accounts under different identity providers.

# Connect Interface

Integrate the Connect Interface into your game (or other software application) to use the features of the Epic Online Services (EOS) Connect service. You can use the Connect Interface to let players sign in to your game through a supported identity provider. For more information about supported identity providers and configuration, see the Identity Provider [Management](https://dev.epicgames.com/docs/en-US/dev-portal/identity-provider-management) documentation.

The Connect Interface:

- Provides access to the EOS Game Services.
- Generates a common unique user identifier within the cross-platform services.
- Links external identity providers' accounts to our services.

A player can associate one or more external user accounts with a unique player ID called a Product User ID (PUID). This Product User ID is a unique ID for each Product under the same Organization.

**Note:** This service is also available as a web API. See the [Connect](https://dev.epicgames.com/docs/en-US/web-api-ref/connect-web-api) Web APIs documentation for more information.

## Differences between Auth Interface and Connect Interface

You can also use the Auth Interface to let players sign in to their Epic Games account directly from your game. See the Auth Interface [documentation](https://dev.epicgames.com/docs/en-US/epic-account-services/auth/auth-interface) for more information on using the Auth Interface. The differences between the Auth Interface and Connect Interface are listed in the table below:

<span id="page-0-0"></span>

|                          | Auth Interface                                                                                                                                | Connect Interface                                                                                                                                                                   |
|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Player ID                | The player's Epic Games account ID. The player can<br>sign in with an email and password combination, or a<br>linked external account1.       | A Product User ID (PUID) for a player, that's linked to one<br>or more supported identity providers2. Note that the PUID<br>is specific to a single product.                        |
| Access                   | The player can sign in to Epic Account Services across<br>any platform (PC (Windows, Mac, and Linux), console,<br>and mobile) and storefront. | The player can authenticate their account in your game to<br>access EOS Game Services across any platform (PC<br>(Windows, Mac, and Linux), console, and mobile) and<br>storefront. |
| Supported<br>Services    | The player has access to the Friends3, Presence4, and<br>Ecom5 interfaces, as well as the EOS Social Overlay6.                                | The player has access to Game Services across<br>Multiplayer, Progression, Moderation, and Operations.                                                                              |
| Access Token<br>Lifetime | The game automatically refreshes the token as long as<br>it calls EOS_Platform_Tick7.                                                         | The game must periodically refresh the player's token<br>based on expiration notification events8.                                                                                  |

## Sign In Flow for Organizations With Multiple Products

A player's saved data (stats, achievements, leaderboard rankings, and stored data such as game save data) on one platform won't necessarily be accessible on a different platform. This might affect your product if it meets all of the following criteria:

In the Developer Portal, you have multiple products in the same **organization** that use Epic Online Services (EOS).

- One or more of your products allows players to sign in with a number of different accounts.
- One or more of your products support crossplay across multiple platforms.

#### **What do I need to do?**

Before the player signs in through the Connect Interface, we recommend that you display a message to the player that is similar to this: "Have you played another one of our games on an existing account before?"

If the player selects "Yes", respond with a message similar to this: "If you want to keep your saved data (stats, achievements, leaderboard rankings, and stored data such as game save data) from your existing account, please sign in with that same account now."

If the player selects "No" for the first question, follow the standard sign in flow to create a new account (see the [Authenticating](#page-2-0) Users section on this page).

#### **What if my players can't sign in to the same account across different platforms?**

In some cases, players might not be able to sign in to the same account across different platforms. For example, the player might play your game on their PSN account on PlayStation and then they might play the same game using their Nintendo account on Nintendo Switch. In this case, the player can't sign into their PSN account on Nintendo Switch.

To solve this, the player must link both accounts to a common **identity provider** , such as an Epic Games account. The player can then sign in to the game with that linked common identity provider across all platforms. The player's saved data (stats, achievements, leaderboard rankings, and stored data such as game save data) connected to that common identity provider persists across all platforms that the player uses the common identity provider on.

**Note:** The player might be able to sign in with the platform account instead of the linked common identity provider, because the platform account is linked to the common identity provider. See the EOS SDK documentation for the platform for details: EOS SDK for [Platforms.](https://dev.epicgames.com/docs/en-US/epic-online-services/platforms)

Before your player signs in through the Connect Interface, we recommend that you display a message to the player that is similar to this: "If you want to play our game across different platforms and keep your saved data (stats, achievements, leaderboard rankings, and stored data such as game save data), you must link your accounts to an Epic Games account to access your data across platforms. Then return to this login screen and sign in with your linked Epic Games account. This ensures that you can access your stored data across platforms."

**Note:** The common identity provider can be an Epic Games account (as used in the example message) or another common identity provider (including your own proprietary account system). Either way, to access their saved data across accounts, your players must link each account to a common identity provider.

If you do not follow this recommended sign in flow, separate accounts might have different saved data. This means that players cannot access their saved data (stats, achievements, leaderboard rankings, and stored data such as game save data) across different platforms.

## Managing users

Users login using a set of external credentials. If the user already exists for this **product** , an **access token** is granted that lasts for a set period of time (currently one hour). The game is notified prior to the token expiration and is required to refresh the existing token, given a valid external credential at that time.

Epic Online Services interfaces cannot control the access permissions of the external provider. As such, **Connect Interface**

### **DOES:**

- Verify the user is still valid with the external account provider
- Extend the time the user is able to access our services

#### **DOES NOT:**

- Implicitly refresh tokens
- Grant lengthy access

The external **account** provider is expected to have authoritative access to the application.

![](_page_2_Figure_2.jpeg)

Access to services available via this user identifier requires external account identification via the EOS\_ProductUserId data structure. All interfaces clearly indicate this usage and type safety should be assured. It is separate from EOS\_EpicAccountId that is associated with the authentication interface provided for Epic Account Services.

Epic Games accounts are not required for using the Connect Interface that can be used with any of the supported external account types.

### **Authenticating Users**

**Note:** If you have multiple products in your organization, follow the guidance in the Sign In Flow for Organizations With Multiple PUIDS section on this page before you authenticate users.

To authenticate with EOS Connect, follow these steps:

- 1. Call EOS\_Connect\_Login with the EOS\_Connect\_LoginOptions containing external credentials from a supported platform. You can sign in with any supported identity provider (for example, Steam, Xbox, or Nintendo). See the Identity Provider [Management](https://dev.epicgames.com/docs/en-US/dev-portal/identity-provider-management) documentation for a complete list of supported identity providers. For instance, to authenticate with an Epic Games account, retrieve an ID token for the local Epic user from EOS\_Auth\_CopyIdToken and set the EOS\_EExternaCredentiaType to EOS\_ECT\_EPIC\_ID\_TOKEN .
- 2. Pass the EOS\_HConnect handle, your EOS\_Connect\_LoginOptions structure, and your callback information to EOS\_Connect\_Login .

<span id="page-2-0"></span>After authenticating, these additional actions may occur:

- Provided that the EOS\_HPatform handle is ticking, the callback you provide executes when the operation finishes.
- After a successful result, the application may access additional interfaces that require authentication.
- If the user does not exist, the login API returns a EOS\_InvaidUser result along with a EOS\_ContinuanceToken . The token provides details about the login attempt and is required during the next steps in the login flow.

If your game supports cross-platform progression, when the player logs in the first time ask them if they have already previously played the game on another platform. If so, the player can authenticate using that platform's external account to connect with their existing game progression, and then link that current platform account to their existing EOS user (see Linking an [Account\)](#page-3-0). Otherwise, you can ask if they want a fresh start by creating a new EOS user (see [Creating](#page-3-1) a User).

#### **User authentication refresh notification**

A callback function provided to EOS\_Connect\_AddNotifyAuthExpiration will run whenever an existing access token is about to expire. This should give the application enough time to acquire another external access token and provide it to the SDK via the EOS\_Connect\_Login function each time.

Use the EOS\_Connect\_RemoveNotifyAuthExpiration function to stop listening for these notifications.

#### **User authentication status change notification**

To assure that your application knows whenever a user's authentication status has changed, the Connect Interface provides a callback for such notifications.

The callback function you provide to [EOS\\_Connect\\_AddNotifyLoginStatusChanged](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-connect-add-notify-login-status-changed) runs whenever a local user's authentication status changes. This only occurs if the expiration notification is ignored, the external provider credentials are unable to refresh the access token, or the access was revoked explicitly by the backend service for some administrative reason. This callback fires only when other calls in the EOS ecosystem detect that this auth token is invalid; it does not automatically fire otherwise.

You must handle all the error messages in specific calls that may fail because of authentication. Recovery is often possible by calling EOS\_Connect\_Login then retrying the original call on success.

Use the [EOS\\_Connect\\_RemoveNotifyLoginStatusChanged](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-connect-remove-notify-login-status-changed) function to stop listening for notifications.

#### **Checking current user authentication status**

To check the player's current status on demand, use the EOS\_Connect\_GetLoginStatus function. This function determines authentication status based on the most recent communication with the online service, so the result is returned instantly and does not use a callback function.

## Creating a new user

Creating a user will generate a new Product User ID for the user given the external credentials used during login. It is almost always necessary to prompt the user and see if there is an alternate way to login before creating a new user. This will reduce confusion and user game progression merging issues in cross-platform situations.

If the application intends to create a new product user ID for the user, then simply call EOS\_Connect\_CreateUser with the EOS\_Connect\_CreateUserOptions containing the EOS\_ContinuanceToken from the previous call to EOS\_Connect\_Login .

Pass the EOS\_HConnect handle, your EOS\_Connect\_CreateUserOptions structure, and your callback information to the function. Provided that the EOS\_HPatform handle is ticking, the callback you provided will execute when the operation finishes.

## Linking an external account

<span id="page-3-1"></span>If during the application login flow, it is determined that the user is able to login with a secondary set of external credentials, store the EOS\_ContinuanceToken from the first login call, reattempt to login via EOS\_Connect\_Login , and once successfully logged in, immediately call EOS\_Connect\_LinkAccount . This will associate the two external accounts to the one product user ID. It is possible to link more than two accounts as the capabilities of our services grow.

If the intent of the application is to link a new external account with an existing product user ID, then call EOS\_Connect\_LinkAccount with the EOS\_Connect\_LinkAccountOptions containing the EOS\_ContinuanceToken from the previous call to EOS\_Connect\_Login and the product user ID for the user logged in with a successful call to EOS\_Connect\_Login .

<span id="page-3-0"></span>Pass the EOS\_HConnect handle, your EOS\_Connect\_LinkAccountOptions structure, and your callback information to the function. Provided that the EOS\_HPatform handle is ticking, the callback you provided will execute when the operation finishes.

You can search EOS Connect users by their external account IDs or using the product user ID through the Accounts dashboard in the Developer Portal. As the keychain of a player's linked external accounts is managed at the Organization level, the Accounts dashboard can be found next to the Organization management menu items. Through the Accounts dashboard, you will be able to find players and your customer support will be able to help to remove account linkings by player requests. You can also use this dashboard to remove your EOS Connect user or any linked accounts during SDK integration testing. Along with this, we will provide web REST APIs for developers to integrate EOS user management into their existing internal dashboards.

## Unlinking an external account

During the first time user login flow, it is possible for the user to end up in a state where they inadvertently created a new product user ID for the logged in account (that is typically the local platform account), instead of reusing existing game progression that they may have from playing on another platform or using another external account credentials. The user may not be aware of the crossplay capabilities of the game.

In such a scenario, the user skips any early login dialogs and continues with their default platform account credentials (the logged in account) to create a new EOS\_ProductUserId through the flow with EOS\_Connect\_Login and EOS\_Connect\_CreateUser APIs. As a consequence, their logged in account that is used with EOS\_Connect\_Login at game start is now tied with an unintended product user ID. To recover from this state, the user needs to be able to unlink the logged in account from the newly created keychain, so that they may enter the first time user login flow again.

Once they have unlinked the logged in account, they may login using another set of external account credentials linked with another existing keychain. On success, they then proceed to link the default logged in account with that desired existing keychain using the flow described in the Linking an Externa Account section above.

In another scenario, the user may simply want to disassociate the logged in account from its current keychain in order to separate game progressions between platforms or otherwise link it to another keychain.

To unlink an external account from a keychain that owns the product user ID currently logged in, call EOS\_Connect\_UninkAccount with the EOS\_Connect\_UninkAccountOptions , specifying LocaUserId as the EOS\_ProductUserId that was returned by a previous call to EOS\_Connect\_Login . If the external account is not associated with any other product user IDs, the external account will be removed from the keychain. This means that the next attempt to login with EOS\_Connect\_Login using credentials associated with the same external account will return an EOS\_InvaidUser result with an EOS\_ContinuanceToken . If the external account is associated with other product user IDs, the operation removes the external account from the currently logged in product user ID but does not remove the external account from the keychain. This means that the external account is no longer returned in query APIs for the product, but the external account can still be used with the remaining product user IDs.

Similarly to the account linking creation operations, history of unlinking operations can be audited through the Accounts dashboard in the Developer Portal.

### **External account unlinking restrictions**

In order to protect against account theft, unlinking accounts from a keychain is only possible for those accounts that the local user is currently logged in with. This prevents a malicious actor from gaining access to one of the linked accounts and using it to remove all other accounts linked with the keychain. It also prevents a malicious actor from replacing the unlinked account with their own corresponding account on the same platform, as the unlinking operation will ensure that any existing authentication session cannot be used to re-link and overwrite the entry without authenticating with one of the other linked accounts in the keychain.

These restrictions are in place to limit the potential attack surface related to account theft scenarios.

## Using Device ID

This feature is relevant for Mobile personal devices and PC Desktop only. Device ID cannot be used with the Anti-Cheat Interfaces, which require an external account.

In cases where an application believes that users may not yet be invested enough to create a new account or link an existing account, an application may create a new persistent pseudo-account using the EOS Connect Device ID feature. This feature allows an application to create a persistent access credential for a local user without using any external login credentials. This allows the backend services to remember the current user on the local device across game sessions. The Device ID feature is used especially on mobile personal devices to allow automatically logging in a user without prompting for account credentials, as well as allowing to start playing the game without immediately requiring a user login to persist the game progression data.

However, as the Device ID is not tied to a real user account and can only be used to uniquely identify the local device, it is highly recommended to ask the user to link an external authentication method to their Device ID account once a user has progressed through some early progression milestone to prevent losing the credential and thus their account.

Without eventually linking the Device ID account with a real user identity, all progress and game data will be permanently lost should something happen to the device itself. The EOS SDK stores the Device ID credential locally in the keychain of the currently logged in user of the local device. If the local user profile is reset or otherwise lost, it is not possible to recover a lost Device ID after that.

To create a new Device ID, an application must call the EOS\_Connect\_CreateDeviceId function with a valid EOS\_Connect\_CreateDeviceIdOptions structure containing the user's DeviceModel. This call will eventually trigger a callback to the function bound to the

EOS\_Connect\_OnCreateDeviceIdCaback . If a new Device ID was successfully created and stored in the keychain for the logged in user of the local device, the ResutCode parameter in the callback parameters will be set to EOS\_EResut::EOS\_Success . If an existing Device ID was already present, EOS\_EResut::EOS\_DupicateNotAowed is returned. Otherwise, the ResutCode parameter identifies the reason for the operation failure.

#### To login the local user with their unique Device ID, call the EOS\_Connect\_Login API with the

EOS\_EExternaCredentiaType::EOS\_ECT\_DEVICEID\_ACCESS\_TOKEN external credential type. As the SDK manages the stored Device ID credential automatically, the Token parameter of the input EOS\_Connect\_Credentias struct must be null. However, as the Device ID is not tied to a real user identity, the EOS\_Connect\_UserLoginInfo input struct needs to be provided with a valid DispayName for the user.

To link the local Device ID with a real external user account, first have the game login the local user normally using the Device ID credential type. Then, ask the user to login with a real identity and after EOS\_Connect\_Login returns back with an EOS\_ContinuanceToken , link the external account using the EOS\_Connect\_Link API to associate the local device with the user's real account. This will allow logging in the user automatically each time the game is started, as the Device ID can still be used as intended to automatically login the user without prompting for external user account credentials.

## **Deletion of Device ID credentials**

You may also delete the Device ID for the current user profile of the local device by calling the EOS\_Connect\_DeeteDeviceId API. The deletion operation is a permanent and nonrecoverable operation. However, it is always possible to create a new Device ID and link it with an existing external user account to restore the automated login functionality.

On Android and iOS devices, uninstalling the application will automatically delete any local Device ID credentials created by the application.

On Desktop platforms (Linux, macOS, Windows), Device ID credentials are not automatically deleted. Applications may re-use existing Device ID credentials for the local OS user when the application is re-installed, or call the **EOS\_Connect\_DeleteDeviceId** API on the first run to ensure a fresh start for the user.

### **Linking Device ID based game progression with an existing linked accounts keychain**

There is an edge-case scenario in which the common path to link a Device ID pseudo-account with a real external user account is not possible. In this scenario, the real user account that is logged in, already belongs to an existing keychain under the same EOS Organization. In such case, when the game first automatically logs in the user using the local Device ID login type and then the user logs in with real external user account credentials, the EOS\_Connect\_Login API will return an existing EOS\_ProductUserId instead of an EOS\_ContinuanceToken . To handle this specific scenario, the EOS\_Connect\_TransferDeviceIdAccount API is used.

The Device ID pseudo-account cannot be linked into the existing keychain, as the EOS\_Connect\_Link API requires the use of an EOS\_ContinuanceToken . Additionally, as the player is now faced with two separate EOS\_ProductUserIds , the player needs to be given a choice by the game on whether to discard one of the EOS\_ProductUserIds (i.e. game profiles) as obsolete and continue to play with the other one. If the player chooses to discard one of the profiles, the game can link the local Device ID account into the existing keychain of linked accounts that is persistent on the backend.

To handle the scenario of two EOS\_ProductUserIds , the game needs to recognize when it has logged in the local user using the Device ID login type and then the user, in the same game session, logs in using an external account and receives another EOS\_ProductUserId session for it. When this happens, the game should attempt to automatically check whether one of the EOS\_ProductUserIds does not have any meaningful game progression on the backend side. In this case, it should automatically discard it and link the local Device ID pseudo-account with the existing keychain of the real external user account.

In case the game is unable to determine this trivial case that can be automated on behalf of the user, it should prompt the user to make a choice on how to proceed. In this dialog, the player should be able to review the game progression as a comparison format for each EOS\_ProductUserId and select which one to keep. It should be made very clear to the user that the discarded game progression will be lost forever and cannot be recovered afterwards. The game may also offer the user the option to not discard either profile, and to choose which one to continue playing with for the current game session.

If the user chooses to discard one of their game profiles and switch to another permanently, the game should call the

EOS\_Connect\_TransferDeviceIdAccount API to transfer the local Device ID pseudo-account into the keychain that is linked with the real external user accounts. In the API, input struct EOS\_Connect\_TransferDeviceIdAccountOptions and set the ProductUserIdToPreserve parameter to point to the correct EOS\_ProductUserId value to preserve in the Device ID transfer operation.

## Retrieving Product User ID mappings

Other interfaces will expect the EOS\_ProductUserId for remote or otherwise external users; say a list of friends or players on the same server. It is possible to convert external user account identifiers to the EOS\_ProductUserId via the mapping API.

EOS\_Connect\_QueryExternaAccountMappings is an asynchronous call that will convert external account IDs to the SDK representation. Simply set the EOS\_EExternaAccountType and provide a contiguous list of external account IDs in string form.

Pass the EOS\_HConnect handle, your EOS\_Connect\_QueryExternaAccountMappingsOptions structure, and your callback information to the function. Provided that the EOS\_HPatform handle is ticking, the callback you provided will execute when the operation finishes.

Once the callback has successfully returned, it is possible to retrieve the mappings via EOS\_Connect\_GetExternaAccountMapping . This function takes the EOS\_EExternaAccountType as before and returns a single EOS\_ProductUserId for each call that provides an external account ID in string form.

A common use case is to query other users who are connected through the same account system as the player (e.g. a player on Steam that queries another player on Steam for matchmaking). Queries using external account IDs of another account system are never available. For example, someone playing on Xbox can't access another player's Steam ID.

## Retrieving external account mappings

Just as one may retrieve the product user ID for an external account, you can also retrieve the external accounts for a product user ID.

EOS\_Connect\_QueryProductUserIdMappings is an asynchronous call that will convert EOS\_ProductUserIds to their external counterpart on a given platform, along with additional account data including the display name and the last login time. Simply provide a contiguous list of product user IDs.

Pass the EOS\_HConnect handle, your EOS\_Connect\_QueryProductUserIdMappingsOptions structure, and your callback information to the function. Provided that the EOS\_HPatform handle is ticking, the callback you provided will execute when the operation finishes.

Once the callback has successfully returned, it is possible to retrieve the mappings via

EOS\_Connect\_GetProductUserIdMapping . This function takes the EOS\_EExternaAccountType , an input buffer, and buffer length to fill with the external account ID. If the buffer is too small, the proper size of the buffer will be returned.

Use EOS\_Connect\_GetProductUserExternaAccountCount to retrieve the number of linked external accounts for a product user.

Use EOS\_Connect\_CopyProductUserExternaAccountByIndex to fetch information about an external account linked to a product user using the index.

Use EOS\_Connect\_CopyProductUserExternaAccountByAccountType to fetch information about an external account of a specific type linked to a product user.

Use EOS\_Connect\_CopyProductUserExternaAccountByAccountId to fetch information about an external account linked to a product user using the account ID.

Use EOS\_Connect\_CopyProductUserInfo to fetch information about a product user, using the external account that they most recently logged in with as the reference.

If the product user ID does not map to the given external platform, then the data will not be present in the results returned for the above. This means that the user has never connected that external account type to their product user ID.

If you are using Epic Account Services in your game, only the display name is returned for platform accounts.

## User verification using an ID Token

ID Tokens are part of the OpenID [Connect](https://openid.net/specs/openid-connect-core-1_0.html#IDToken) protocol and can be used to verify a user's identity on server-side. An ID Token is a JSON Web Token (JWT) that contains information about the authenticated user, such as the product user ID. This allows backend services and game servers to securely verify user identifiers it receives from clients.

ID Tokens can not be used to execute actions on behalf of a user. They are only intended for the use of user identity verification.

The ID token returned by your OpenId provider must have the kid parameter value set in its header.

#### **Retrieving an ID Token for user**

Game clients can obtain ID Tokens for local users by calling the EOS\_Connect\_CopyIdToken SDK API after the user has been authenticated with EOS Connect, passing in a EOS\_Connect\_CopyIdTokenOptions structure containing the EOS\_ProductUserId of the user.

The outputted EOS\_Connect\_IdToken structure contains the EOS\_ProductUserId of the user, and a JWT representing the ID Token data. Note, you must call EOS\_Connect\_IdToken\_Reease to release the ID Token structure when you are done with it.

Once retrieved, the game client can then provide the ID Token to another party. A new ID Token is provided after each successful EOS\_Connect\_Login call.

The ID Token is valid for the lifetime of the local user's EOS Connect authentication session. When the game client refreshes the authentication session, it should expect any earlier used ID Token to expire soon after, and a new ID Token to be used if needed.

#### **Validating ID Tokens on game server using SDK**

The JSON Web Key Set (JWKS) endpoint for EOS Connect ID Tokens is: <https://api.epicgames.dev/auth/v1/oauth/jwks>.

Game servers can validate ID Tokens by calling the EOS\_Connect\_VerifyIdToken SDK API, and passing in a EOS\_Connect\_VerifyIdTokenOptions containing the EOS\_Connect\_IdToken . Note, game servers should use EOS\_ProductUserId\_FromString to populate the EOS\_ProductUserId part of the EOS\_Connect\_IdToken structure before calling verify, because the server's user handles will be different to the user handles on the client.

#### **Validating ID Tokens on backend without SDK**

Backend services can verify the validity of ID Tokens and extract the token claims using any of the publicly available standard JWT libraries. See <https://jwt.io/> for a list of libraries for this purpose. The used library should allow automatic caching of the retrieved JWKS information for the best performance and to reduce networking overhead.

The EOS SDK and other support libraries take care of the needed steps for securely validating the ID Token before its containing claims can be securely trusted. The steps performed are as following:

- 1. Verify that the token signature algorithm ("alg") is present and is not set to "none".
- 2. Verify the JWT signature against the expected public certificate using the JWKS endpoint hosted by Epic Online Services.
- 3. Verify that the token issuer ("iss") is present and starts with the base URL of [https://api.epicgames.dev.](https://api.epicgames.dev/)
- 4. Verify that the token issue time ("iat") is in the past.
- 5. Verify that the token expiration time ("exp") is in the future.
- 6. Verify that the Client ID ("aud") matches the Client ID that you are using to initialize the EOS SDK with game clients or otherwise to authenticate users with EOS Connect.

After successfully verifying the ID Token you can trust the Product User ID ("sub") value.

### **ID Token structure**

The ID Token contains the following JSON structures:

#### **Header**

| Key | Type   | Description                                                               |
|-----|--------|---------------------------------------------------------------------------|
| alg | string | Signature algorithm.                                                      |
| kid | string | This is required. Identifier for the key that was used to sign the token. |

#### **Payload**

| Key | Type    | Description                                                  |
|-----|---------|--------------------------------------------------------------|
| aud | string  | Client ID used to authenticate the user with EOS Connect.    |
| exp | integer | Expiration time of the token, seconds since the epoch.       |
| iat | integer | Issue time of the token, seconds since the epoc.             |
| iss | string  | Token issuer. Always starts with https://api.epicgames.dev . |

| Key   | Type        | Description                                                                       |
|-------|-------------|-----------------------------------------------------------------------------------|
| pfdid | string      | EOS Deployment ID.                                                                |
| pfpid | string      | EOS Product ID.                                                                   |
| pfsid | string      | EOS Sandbox ID.                                                                   |
| sub   | string      | Product User ID of the authenticated user.                                        |
| act   | json object | Identifies the external account that was used to authenticate the connected user. |

## External account information ( act )

| Key   | Type   | Description                                                                                                                                                                                                                                       |
|-------|--------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| eat   | string | Identifies the external account type. Possible values include: amazon , apple , discord , epicgames , gog ,<br>google , itchio , nintendo_id , nintendo_nsa_id , oculus , openid , psn , steam , xbl                                              |
| eaid  | string | External account ID.                                                                                                                                                                                                                              |
| pltfm | string | Platform that the user is connected from. Possible values include: other , playstation , steam , switch ,<br>xbox                                                                                                                                 |
| dty   | string | Device Type. Identifies the device that the user is connected from. Can be used to securely verify that the user is<br>connected through a real Console device. Possible values include: PSVITA , PS3 , PS4 , PS5 , Switch ,<br>Xbox360 , XboxOne |

## Footnotes

- 1. See the Auth [Interface](https://dev.epicgames.com/docs/en-US/epic-account-services/auth/auth-interface#external-account-authentication) documentation for more information on linked external accounts. [↩](#page-0-0)
- 2. See the Identity Provider [Management](https://dev.epicgames.com/docs/en-US/dev-portal/identity-provider-management) documentation for more information on identity providers. [↩](#page-0-0)
- 3. See the Friends [Interface](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-friends-interface) documentation for more information on using the Friends Interface. [↩](#page-0-0)
- 4. See the [Presence](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-presence-interface) Interface documentation for more information on using the Presence Interface. [↩](#page-0-0)
- 5. See the Ecom [Interface](https://dev.epicgames.com/docs/en-US/epic-games-store/services/ecom/ecom-overview) documentation for more information on using the Ecom Interface. [↩](#page-0-0)
- 6. See the Social Overlay [Overview](https://dev.epicgames.com/docs/en-US/epic-account-services/social-overlay-overview) documentation for more information on the EOS social overlay. [↩](#page-0-0)
- 7. See the Api [Reference](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-platform-tick) documentation for more information on calling EOS\_Patform\_Tick . [↩](#page-0-0)
- <span id="page-8-0"></span>8. See the Connect [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-connect-interface#user-authentication-refresh-notification) documentation for more information on expiration notification events. [↩](#page-0-0)
