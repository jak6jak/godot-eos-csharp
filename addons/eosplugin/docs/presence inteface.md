Epic Developer [Resources](https://dev.epicgames.com/docs) Presence Interface

# Presence Interface

Interface that enables users to inform their friends of their current activities.

With the **Presence Interface**, an application can advertise its local player's status, known as **presence** , and query the presence of other players online. An application may also advertise transient data to others, in order to share more detailed information about the state of the local player. Users can only receive presence information about other users with whom they are [friends](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-friends-interface).

To use the Presence Interface, your product must have **Epic Account Services** (EAS) active, and must obtain user [consent](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-data-privacy-visibility) to access **Online Presence** data. You can activate EAS on the [Developer](https://dev.epicgames.com/docs/en-US/dev-portal) Portal, or learn more in Epic's [documentation.](https://dev.epicgames.com/docs/en-US/epic-account-services) Without EAS and user consent, you will still be able to initialize the EOS SDK and the Presence Interface, but all Presence Interface function calls to the back-end service will fail.

## Managing Presence Information

To use the **Presence Interface**, you must acquire a handle of type EOS\_HPresence from the **Platform [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-platform-interface)** function, EOS\_Patform\_GetPresenceInterface . With this handle, you can download and cache presence information about other users on your friends list, or update your own presence.

### Retrieving and Caching Presence Information

To retrieve presence information about a user, call EOS\_Presence\_QueryPresence with an EOS\_Presence\_QueryPresenceOptions structure, an optional CientData parameter, and a callback function. Initialize the EOS\_Presence\_QueryPresenceOptions with the following field values:

| Property     | Value                                                                                                                                                                 |
|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion   | EOS_FRIENDS_QUERYPRESENCE_API_LATEST                                                                                                                                  |
| LocalUserId  | The EOS_EpicAccountId of the logged-in user making the request.                                                                                                       |
| TargetUserId | The EOS_EpicAccountId of the user whose presence data you want to         retrieve. This value must either match<br>LocalUserId, or be a friend         of that user. |

The callback function will receive the CientData parameter you provide here, and will run following completion of the operation regardless of success or failure. The only exception to this is if the call fails locally due to the EOS\_Presence\_QueryPresenceOptions structure lacking any of the required information. In the event of failure due to lack of permission to view the target user's presence, the callback function's ResutCode field will be EOS\_NotFound . Any other value, aside from EOS\_Success , indicates an input or state failure, such as invalid parameters, LocaUserId not matching a local, logged-in user, or the local user being offline.

## Updating Presence Information

There are two ways to update presence information in your local cache. The first is to call EOS\_Presence\_QueryPresence periodically, or shortly before accessing the cache. The second is to receive notification from the Presence Interface when a change has occurred. To enable this feature, call EOS\_Presence\_AddNotifyOnPresenceChanged . This function requires an EOS\_Presence\_AddNotifyOnPresenceChangedOptions struct, a userdefined CientData parameter, and a callback function of type EOS\_Presence\_OnPresenceChangedCaback . The callback function will receive the CientData parameter and the EOS\_EpicAccountId of the user whose presence changed. Initialize the EOS\_Presence\_OnPresenceChangedCaback structure as follows:

| Property   | Value                                             |
|------------|---------------------------------------------------|
| ApiVersion | EOS_FRIENDS_ADDNOTIFYONPRESENCECHANGED_API_LATEST |

If successful, EOS\_Presence\_AddNotifyOnPresenceChanged will return a valid EOS\_NotifcationId . In the case of an error, it will return EOS\_INVALID\_NOTIFICATIONID .

To deactivate this feature, call EOS\_Presence\_RemoveNotifyOnPresenceChanged , passing the presence handle and notification ID as parameters. This function stops notifications to a handle previously registered with EOS\_Presence\_AddNotifyOnPresenceChanged .

There can be multiple callbacks registered at a time. In this case all the callbacks will be called once the event occurs.

## Examining Presence Information

Once EOS\_Presence\_QueryPresence has populated the cache with a user's presence information, you can begin examining it. To establish whether or not the cache contains a given user's presence information, call EOS\_Presence\_HasPresence with your EOS\_HPresence hande , and an EOS\_Presence\_HasPresenceOptions structure initialized as follows:

| Property     | Value                                                                            |
|--------------|----------------------------------------------------------------------------------|
| ApiVersion   | EOS_FRIENDS_HASPRESENCE_API_LATEST                                               |
| LocalUserId  | The EOS_EpicAccountId of the logged-in user making the request.                  |
| TargetUserId | The EOS_EpicAccountId of the user whose cached presence data you want to locate. |

EOS\_Presence\_HasPresence will return EOS\_TRUE if it succeeds and finds data, or EOS\_FALSE if it receives bad input or if the cache does not contain data for the target user.

EOS\_Presence\_CopyPresence provides copies of presence information from the cache. The Presence Interface will return data as a new [EOS\\_Presence\\_Info](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-presence-info) object. This function requires your Presence Interface handle, an EOS\_Presence\_CopyPresenceOptions structure, and an output parameter to hold your new EOS\_Presence\_Info object. Initialize the EOS\_Presence\_CopyPresenceOptions structure as follows:

| Property     | Value                                                                          |
|--------------|--------------------------------------------------------------------------------|
| ApiVersion   | EOS_FRIENDS_COPYPRESENCE_API_LATEST                                            |
| LocalUserId  | The EOS_EpicAccountId of the logged-in user making the request.                |
| TargetUserId | The EOS_EpicAccountId of the user whose cached presence data you want to copy. |

If the Presence Interface successfully pulls a copy of the information from the cache, EOS\_Presence\_CopyPresence will return EOS\_Success and fill out the EOS\_Presence\_Info pointer you provided with a copy of the target user's data. The caller owns this pointer, so the EOS SDK will never modify its contents or free the memory associated with it. Call EOS\_Presence\_Info\_Reease with the pointer when you no longer need this data to free it. Failure to do so will result in a memory leak.

EOS\_Presence\_GetJoinInfo provides an easy way to get the previously set Join Info string from a remote user's presence data. For this function to succeed, the user's presence data must already be in the presence cache. This function requires your Presence Interface handle, an EOS\_Presence\_GetJoinInfoOptions structure, OutBuffer set to a pointer to a char buffer, and InOutBufferLength set to the maximum length of the char buffer. Initialize the EOS\_Presence\_GetJoinInfoOptions structure as follows:

| Parameter  | Description                                     |
|------------|-------------------------------------------------|
| ApiVersion | Set this to EOS_PRESENCE_GETJOININFO_API_LATEST |

| Parameter    | Description                                                                                                                                              |
|--------------|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| LocalUserId  | The EOS_EpicAccountId of the logged-in user making the request                                                                                           |
| TargetUserId | The EOS_EpicAccountId of the user whose Join Info you want to retrieve. This value must either be a logged-in Local<br>User, or a Friend of LocalUserId. |

The length of the OutBuffer char buffer is recommended to be EOS\_PRESENCEMODIFICATION\_JOININFO\_MAX\_LENGTH , or it may be too short to store some values and will fail the request.

**Note:** Updates to presence information, even when using EOS\_Presence\_AddNotifyOnPresenceChanged , will not be reflected in existing EOS\_Presence\_Info objects. This is because these objects are copies of cache data, not pointers to the cache, and because the Presence Interface does not own them and will not modify them after the initial copy.

# Modifying Local Presence

To modify a local user's presence, you must create an EOS\_HPresenceModification handle, and set changes by calling one or more of the following functions:

- EOS\_PresenceModification\_SetStatus
- EOS\_PresenceModification\_SetRawRichText
- EOS\_PresenceModification\_SetData
- EOS\_PresenceModification\_DeeteData
- EOS\_PresenceModification\_SetJoinInfo

**Note:** Changes are reflected after a call to EOS\_Presence\_SetPresence succeeds.

### Creating a PresenceModification Handle

To modify a local user's presence, first create a **PresenceModification** handle by calling EOS\_Presence\_CreatePresenceModification with a valid EOS\_HPresence handle, an initialized EOS\_Presence\_CreatePresenceModificationOptions struct, and a pointer to an invalid EOS\_HPresenceModification handle. Initialize the EOS\_Presence\_CreatePresenceModificationOptions struct as follows:

| Property    | Value                                              |
|-------------|----------------------------------------------------|
| ApiVersion  | EOS_PRESENCE_CREATEPRESENCEMODIFICATION_API_LATEST |
| LocalUserId | Valid local user in a logged-in state.             |

If successful, the EOS\_Presence\_CreatePresenceModification function returns EOS\_EResut::EOS\_Success , and the OutPresenceModificationHande will be initialized for use with functions in the EOS\_PresenceModification sandbox. The resulting handle must also be released when it is no longer needed by calling the EOS\_PresenceModification\_Reease method.

### Making Changes to a PresenceModification

With a valid EOS\_HPresenceModification, you can build the update for a user's presence by calling functions within the EOS\_PresenceModification function sandbox.

#### **Modifying Presence Status**

To set a new status, invoke EOS\_PresenceModification\_SetStatus with a valid EOS\_HPresenceModification handle, and initialize an EOS\_PresenceModification\_SetStatusOptions struct as follows:

If successful, EOS\_PresenceModification\_SetStatus returns EOS\_EResut::EOS\_Success . Otherwise, it returns an error code describing an issue with the request. Changes will not be reflected in a user's presence until a call to EOS\_Presence\_SetPresence completes with EOS\_EResut::EOS\_Success .

## Modifying A Rich Text string

To set a new rich text string, invoke EOS\_PresenceModification\_SetRawRichText with a valid EOS\_HPresenceModification handle, and initialize an EOS\_PresenceModification\_SetRawRichText struct as follows:

| Property   | Value                                                                                       |
|------------|---------------------------------------------------------------------------------------------|
| ApiVersion | EOS_PRESENCE_SETRAWRICHTEXT_API_LATEST                                                      |
| RichText   | Non-null string that is smaller than byte size of EOS_PRESENCE_RICH_TEXT_MAX_VALUE_LENGTH . |

If successful, EOS\_PresenceModification\_SetRawRichText returns EOS\_EResut::EOS\_Success ; otherwise, it returns an error code describing an issue with the request. Changes will not be reflected in a user's presence until a call to EOS\_Presence\_SetPresence completes with EOS\_EResut::EOS\_Success .

## Adding or Replacing Presence Data

To add or replace existing presence data to a user's presence, invoke EOS\_PresenceModification\_SetData with a valid EOS\_HPresenceModification handle, and initialize an EOS\_PresenceModification\_SetDataOptions struct as follows:

| Property     | Value                                                                                                                                                                 |
|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion   | EOS_PRESENCE_SETDATA_API_LATEST                                                                                                                                       |
| RecordsCount | How many EOS_Presence_DataRecord are present in Records , and Records must be a pointer to an array of<br>EOS_Presence_DataRecord that is at least RecordsCount long. |

#### Initialize EOS\_Presence\_DataRecord with the following:

| Property   | Value                                                                                      |
|------------|--------------------------------------------------------------------------------------------|
| ApiVersion | EOS_PRESENCE_DATARECORD_API_LATEST                                                         |
| Key        | Non-null string that is smaller than the byte size of EOS_PRESENCE_DATA_MAX_KEY_LENGTH .   |
| Value      | Non-null string that is smaller than the byte size of EOS_PRESENCE_DATA_MAX_VALUE_LENGTH . |

**Note:** In the case of conflicting values, such as having multiple DataRecords with the same key, the last conflicting value in the Records array will be used.

If successful, EOS\_PresenceModification\_SetData returns EOS\_EResut::EOS\_Success . Otherwise, it returns an error code describing an issue with the request. Changes will not be reflected in a user's presence until a call to EOS\_Presence\_SetPresence completes with EOS\_EResut::EOS\_Success .

## Deleting Presence Data

Similar to EOS\_PresenceModification\_SetData , EOS\_PresenceModification\_DeeteData removes presence data that matches the key of previously set data. To delete presence data, invoke EOS\_PresenceModification\_DeeteData with a valid EOS\_HPresenceModification handle, and initialize an EOS\_PresenceModification\_DeeteDataOptions struct as follows:

| Property     | Value                                                                                                                                                           |
|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion   | EOS_PRESENCE_DELETEDATA_API_LATEST                                                                                                                              |
| RecordsCount | How many EOS_PresenceModification_DataRecordId are present in Records , and Records must be a pointer<br>to an array of EOS_PresenceModification_DataRecordId . |

Initialize EOS\_PresenceModification\_DataRecordId with the following:

| Property   | Value                                                                                    |
|------------|------------------------------------------------------------------------------------------|
| ApiVersion | EOS_PRESENCE_DELETEDATA_API_LATEST                                                       |
| Key        | Non-null string that is smaller than the byte size of EOS_PRESENCE_DATA_MAX_KEY_LENGTH . |

If a key is marked for delete but does not exist, this part of the pending presence modification process is silently ignored. Additionally, if multiple keys are set to delete the same key, the extra keys will be silently ignored.

If successful, EOS\_PresenceModification\_DeeteData returns EOS\_EResut::EOS\_Success ; otherwise, it returns an error code describing an issue with the request. Changes will not be reflected in a user's presence until a call to EOS\_Presence\_SetPresence completes with an EOS\_EResut::EOS\_Success .

## Setting the Presence JoinInfo Data

The helper function EOS\_PresenceModification\_SetJoinInfo sets the EOS\_JoinInfo Presence data key with a specified Join Info string. This data may also be retrieved through usage of the EOS\_Presence\_GetJoinInfo function. This data will be sent to games when invoking the Join Game feature of the EOS Social Overlay, and should contain whatever information is necessary for a game to find and join a user's match or party, depending on what makes sense for the title.

To invoke the EOS\_PresenceModification\_SetJoinInfo function successfully, it must be called with a valid EOS\_HPresenceModification handle, and a valid EOS\_PresenceModifcation\_SetJoinInfoOptions struct initialized as follows:

| Parameter  | Description                                                                                                                           |
|------------|---------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion | Set to EOS_PRESENCEMODIFICATION_SETJOININFO_API_LATEST                                                                                |
| JoinInfo   | A null-terminated string of up to EOS_PRESENCEMODIFICATION_JOININFO_MAX_LENGTH bytes in length, not including<br>the null-terminator. |

## Applying PresenceModification Changes

Once all changes to an EOS\_HPresenceHande have been made, they must be applied to a user by calling EOS\_Presence\_SetPresence with a valid EOS\_HPresence handle, an optional CientData field, and an EOS\_Presence\_SetPresenceCompeteCaback callback function. The Callback function will contain the value of the CientData parameter. You will also need to provide an EOS\_Presence\_SetPresenceOptions struct, which you should initialize as follows:

| Property                   | Value                                                          |
|----------------------------|----------------------------------------------------------------|
| ApiVersion                 | EOS_PRESENCE_SETPRESENCE_API_LATEST                            |
| LocalUserId                | Local user who created the presence modification.              |
| PresenceModificationHandle | The EOS_HPresenceModification handle that has pending changes. |

It is safe to release the EOS\_HPresenceModification handle immediately after the call to EOS\_Presence\_SetPresence , however, if there is an error, these changes could be lost. We recommend maintaining this handle until the callback function returns a successful result code or at a time when you want to abandon the changes. Additionally, it is invalid for a single user to have more than EOS\_PRESENCE\_DATA\_MAX\_KEYS of unique presence data, and attempting to set more than this many unique presence data keys will fail.

When complete, the callback function will be invoked with an EOS\_Presence\_SetPresenceCompeteInfo struct that contains the following:

| Property   | Description                                                                |
|------------|----------------------------------------------------------------------------|
| ResultCode | This is the call's result code.                                            |
| ClientData | This is the client data from the ClientData parameter.                     |
| AccountId  | This is the account identity value of the local user who invoked the call. |

If multiple calls to EOS\_Presence\_SetPresence happen during a single frame, they may be combined automatically. The callback function for all calls will still be invoked separately but they may share a ResutCode . If there are conflicting modifications, such as a status being set twice across one (or multiple) modifications, the last-set field will overwrite previous changes.

# Subscribing to Social Overlay Notifications

It is also possible to subscribe to notifications related to the **Social Overlay**, such as JoinGameAccepted event, via a EOS\_Presence\_AddNotifyJoinGameAccepted / EOS\_Presence\_RemoveNotifyJoinGameAccepted pair. For more information please refer to the Social [Overlay](https://dev.epicgames.com/docs/en-US/epic-account-services/social-overlay-overview) page.
