# Fitcoin for Unity
Fitcoin is a service rewards users for taking care of themselves with in-game bonuses. This API allows games to interface with a user's Fitcoin account and make "purchases" on their behalf.

## Requirements
This package requrires **Newtonsoft.Json for Unity**, which you can find at https://github.com/jilleJr/Newtonsoft.Json-for-Unity.

## Getting Started
### Initial Setup
The Fitcoin for Unity package operates via the `FitcoinService` Component. Attach it to a Game Object, either in the Unity Editor, or via `AddComponent<FitcoinService>()`.

In order to perform server function calls, you will need to specify your game or service access token, like so:

```cs
// Set myService to an instance of FitcoinService
FitcoinService myService = GetComponent<FitcoinService>();

// Set the access token for your service
myService.AccessToken = "606a279cd773c3f571e183b4";
```

You're now ready to start interfacing with Fitcoin!

### The Fitcoin Flow
Generally speaking, setting up Fitcoin integration involves the following steps:

1. Create a new link request, using `CreateLinkRequest()`.
2. Generate and display a QR code for the link request, using `GetQRCodeForLinkRequest()`.
    - The user will scan the displayed QR code using a smartphone and either approve or deny the link request.
3. Check the status of the link request periodically until it has been either approved or denied, using `MonitorLinkRequestStatus()`.
    - Delete the link request using `DeleteLinkRequest()` once it has been responded to.
    - If the link request was approved, store the ID of the user who approved it.
    - If the link request was rejected, exit out.
4. Get the current user's information (including username and balance) with `GetUserInfo()`.
    - Objects can subscribe to the `onUserInfoUpdated` event in order to receive new user information when it arrives.
5. Make purchases for the user, using `MakePurchase()`.

**Note:** Your user should only need to approve a link request for your game/service once! Once they have approved it, your game can set the `UserID` property of its `FitcoinService` instance, and skip to step 4. Keep in mind that users may unlink games/services from their Fitcoin account at any time, so you must be ready to handle suddenly not having access to a user's account.

## Demo Project
See https://github.com/Meorge/FitcoinUnityExample for an example project that uses the Fitcoin API.