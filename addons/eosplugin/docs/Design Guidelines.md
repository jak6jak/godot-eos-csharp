Epic Developer [Resources](https://dev.epicgames.com/docs) Design Guidelines

# Design Guidelines

Integrating with EOS Epic Account Services Authentication.

Epic Account Services let players get started with your video game quickly and easily on any platform so they can enjoy more meaningful cross-platform experiences with their friends. In this document, we offer guidelines, tips, and considerations for creating a great sign-in experience with Epic Account Services.

# User Experience Guidelines

### Ask for "Sign in with Epic Games" in context

When asking a user to sign in with their Epic Games account, provide the user with context on what that means and what the experience will offer once signed in.

As part of the sign-in process, the user may encounter a consent flow to grant Epic Games the right to share the user's account data with your application. When you configure your application on the Developer Portal, you can select different scopes of Epic Games account data for your application to request access to. Only request access to scopes of data that your application currently needs—don't attempt to future-proof. When the feature set in your application changes, you can at any time update the selected data scopes on the Developer Portal. It is important for people to understand why your application needs that account information and how that will provide them with a better experience.

Generally it is a good practice to position the sign-in process early in your application's start-up flow. If the user has played your video game before on another platform, they may already have progression data saved. Early sign-in flows avoid scenarios where multiple versions of saved data conflict and result in challenges around account merging.

#### Balance Your Different Authentication Options

By using external account systems, you can make it easy for users to sign in to your application. Buttons to sign in with the different authentication options you provide shouldn't be hidden behind multiple steps. Epic Games sign-in option buttons and other third-party sign-in options should be displayed equally prominent. For example, buttons should be approximately the same size and have similar visual weight. The user should understand all sign-in options are equally valid.

After people have signed in with Epic Games, avoid immediately prompting your users to set up a new username and password combination for your application. It is okay to provide this as a flow later in the experience, but consider keeping it optional.

## Importing Account Data to Other Social Graphs

It is common and often expected by users to receive friend recommendations based on the relationships they may have in other social graphs. Before importing any Epic Account data into your or another third-party social graph, you must ask the user for consent and explain what will happen if the user consents. Users must give consent by taking specific, intentional action to import their Epic account data and friends into another ecosystem. Imports can be done in batches, but always need to happen at the time of consent and this consent cannot be durable beyond that one-time event.

In all cases, it must be optional for the user to import their social connections from their Epic Games account to your or any other third-party social graph.

## Withdrawal of Consent and Deletion

The sharing of account data from Epic Games to your application is based on explicit consent by the user. Users can withdraw this consent at any time at which point Epic Games will stop your access to the account data.

If you have imported any Epic Account data into your or another third-party social graph, you must provide a transparent and easily accessible mechanism for your users to withdraw their consent to that use of their Epic Account data.

You are required to promptly and securely delete all of that user's Epic Games account data when notified by Epic Games, or requested by the user

For more information on these and other obligations, refer to the Epic Account Services [Addendum.](https://dev.epicgames.com/en-US/services/terms/agreements)

# Epic Games Brand Guidelines

Epic Games provides two different "Sign in with Epic Games" buttons you can use to let people set up an account and sign in. If necessary, you can create a custom button to offer Sign in with Epic Games. Using the standard Epic Games Sign-In button is strongly recommended as it enables your users to identify the Epic Games option more quickly. For a full list of branding resources and policies, review the Brand [Guidelines](https://dev.epicgames.com/docs/en-US/epic-games-store/get-started/brand-guidelines-overview) Overview and [Content](https://dev.epicgames.com/docs/en-US/epic-games-store/requirements-guidelines/content-ratings/content-guidelines) [Guidelines](https://dev.epicgames.com/docs/en-US/epic-games-store/requirements-guidelines/content-ratings/content-guidelines).

## Default Sign-in Button

The standard "Sign in with Epic Games" button comes with two options: a dark button for light backgrounds and a white button for dark backgrounds.

The size of both buttons is 210 x 50 pixels.

![](_page_1_Picture_7.jpeg)

# Custom Sign-in Button

Video games bring their own visual identity. We want to provide you with the freedom to design sign-in flows that fit the experience desired for each individual application. Below are a combination of strict rules that must be adhered to as well as general guidelines designed to result in the best user experience.

#### **Epic Games Logo**

The Epic Games logo is available in two styles: black and white.

#### [Download](https://epicgames.box.com/s/82lsf9a2lfqyiwl6fra3y9cac2rm5cbd) Epic Games logo

To be in compliance with the design guidelines for Epic Account Services, you may not modify the Epic Games logo colors. You can scale the logo as needed for different devices and screen sizes, but you must preserve the logo aspect ratio so that the Epic Games logo is not stretched. Do not distort the dimensions of the Epic Games logo.

Respect the minimum padding as specified below and don't size the logo below the minimum size. For a logo size of 21 x 24 px, the padding to the left and right of the logo should be 11px. The top padding should be 10px, and there should be 9px between the logo and the bottom. When scaling up, these ratios should be preserved.

| BIGGER BUTTON, NO ROUNDED CORNERS | SMALLER BUTTON, FULL ROUNDED CORNERS |
|-----------------------------------|--------------------------------------|
| EPIC Sign in with Epic Games      | EPIC Sign in with Epic Games         |

The box encapsulating the logo and the copy can be any shape.

![](_page_2_Picture_2.jpeg)

#### **Background**

The button background can be any color of your choosing. Like on the default button, lean to using the white logo on dark-colored button backgrounds and the black logo on light-colored button backgrounds.

![](_page_2_Picture_6.jpeg)

#### **Copy and Font**

Always include text in your sign in button. The recommended English version is "Sign in with Epic Games", "Log in with Epic Games", or "Continue with Epic Games". It's not recommended to have a sign in button that is solely the logo or solely some copy. Always wrap the copy and logo in a button box.

Choose the font and font style that looks best in the context of your video game, but optimize for easy legibility. Minimum recommended font size is 12pt, while the default font size is 14pt.

| ARIAL REGULAR 12 PT, WHITE ON BLACK | ARIAL REGULAR 12 PT, BLACK ON WHITE |
|-------------------------------------|-------------------------------------|
|-------------------------------------|-------------------------------------|

|  | <img alt="Epic Games Logo" src="epic_logo.png"/> Sign in with Epic Games |
|--|--------------------------------------------------------------------------|
|  | <img alt="Epic Games Logo" src="epic_logo.png"/> Sign in with Epic Games |
