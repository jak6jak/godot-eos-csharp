|                          | Overview                                                 | Account Services                                          | Game Services                                             | Get                                                     | j | jak6jak | Dev Portal |
|--------------------------|----------------------------------------------------------|-----------------------------------------------------------|-----------------------------------------------------------|---------------------------------------------------------|---|---------|------------|
| Epic Developer Resources | <span style="font-size: 1em;">➔</span> EOS Game Services | <span style="font-size: 1em;">➔</span> Lobbies & Sessions | <span style="font-size: 1em;">➔</span> Sessions Interface | <span style="font-size: 1em;">➔</span> Modify a Session |   |         |            |

# Modify a Session

How to modify session properties and create custom attributes.

To modify an existing session, first call EOS\_Sessions\_UpdateSessionModification with a pointer to a default EOS\_HSessionModification object and an EOS\_Sessions\_UpdateSessionModificationOptions structure initialized as follows:

| ApiVersion  | EOS_SESSIONS_UPDATESESSIONMODIFICATION_API_LATEST |
|-------------|---------------------------------------------------|
| SessionName | The name of the session you want to modify        |

If this call succeeds, it will return EOS\_Success , the modification will be applied to the local session, and the

EOS\_HSessionModification object you provided will now be a valid handle. If you are the session owner, you can use that handle to apply the local changes you've made to the back-end service's version of the session, by calling EOS\_Sessions\_UpdateSession . This function works for both new sessions (those that have not yet been created on the server)

and pre-existing ones. The following functions modify different aspects of the session:

| Function                                         | Effect                                                                                                                                                                                                                  |
|--------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EOS_SessionModification_SetHostAddress           | This changes the string containing the data required<br>to reach the server. The host address does not have<br>to be an IP address; a socket ID, URL, or other<br>attribution can work.                                 |
| EOS_SessionModification_SetBucketId              | The Bucket ID is the main search criteria, containing<br>game-specific information that is required in all<br>searches. For example, a format like<br>"GameMode:Region:MapName" could be used to<br>form the Bucket ID. |
| EOS_SessionModification_SetMaxPlayers            | Use this to set the maximum number of players<br>allowed in the session.                                                                                                                                                |
| EOS_SessionModification_SetJoinInProgressAllowed | You can permit or forbid players to join games that<br>have already begun (see the section on start and<br>end play for more details) with this function.                                                               |

| Function                                   | Effect                                                                                                                                                                                                                                                                                                                                                                                                                  |
|--------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EOS_SessionModification_SetPermissionLevel | This function can change the session's privacy settings to any of the following:<br>EOS_OSPF_PublicAdvertised : The session will be visible to all players and will show up in searches. EOS_OSPF_JoinViaPresence : Only players with access to the creating user's presence information, which contains the session ID, can find this session. EOS_OSPF_InviteOnly : The session is available only to invited players. |
| EOS_SessionModification_AddAttribute       | Adds a custom attribute (type EOS_SessionDetails_AttributeData ) to the session. See the Custom Attributes section for more information.                                                                                                                                                                                                                                                                                |
| EOS_SessionModification_RemoveAttribute    | Removes a custom attribute from the session. See the Custom Attributes section for more information.                                                                                                                                                                                                                                                                                                                    |

After modifying the session locally, you can update the session on the back-end service if you are the session owner. To do this, call EOS\_Sessions\_UpdateSession with an EOS\_Sessions\_UpdateSessionOptions containing the following information:

| Property                  | Value                                                                                              |
|---------------------------|----------------------------------------------------------------------------------------------------|
| ApiVersion                | EOS_SESSIONS_UPDATESESSION_API_LATEST                                                              |
| SessionModificationHandle | The handle ( EOS_HSessionModification ) for the session you want the server<br>to create or update |

This operation will call your callback function, of type EOS\_Sessions\_OnUpdateSessionCaback , with an EOS\_Sessions\_UpdateSessionCabackInfo data structure upon completion.

EOS supports modifying sessions during play. Even a session that has ended but has not been destroyed can receive updates and start again. A common use case for this behavior is a game that runs a match on one map or level, then cycles through a list to find the next one, and starts a fresh match there, with different players joining and leaving between (or even during) matches.

# Custom Attributes

<span id="page-1-0"></span>Sessions can contain user-defined data, called **attributes**. Each attribute has a name, which acts as a string key, a value, an enumerated variable identifying the value's type, and a visibility setting. The following variable types are currently supported:

| EOS_ESessionAttributeType | Value Type                                |  |
|---------------------------|-------------------------------------------|--|
| EOS_SAT_BOOLEAN           | EOS_Bool                                  |  |
| EOS_SAT_INT64             | int64_t                                   |  |
| EOS_SAT_DOUBLE            | double                                    |  |
| EOS_SAT_STRING            | const char* (null-terminated UTF8 string) |  |

#### The following visibility types are available:

| EOS_ESessionAttributeAdvertisementType | Visibility                 |
|----------------------------------------|----------------------------|
| EOS_SAAT_DontAdvertise                 | Not visible to other users |
| EOS_SAAT_Advertise                     | Visible to other users     |

## **Access an Attribute**

You can find out how many attributes a session has by calling EOS\_SessionDetais\_GetSessionAttributeCount with a valid EOS\_HSessionDetais handle and an EOS\_SessionDetais\_CopySessionAttributeByIndexOptions containing the following information:

| Property   | Value                                                  |
|------------|--------------------------------------------------------|
| ApiVersion | EOS_SESSIONDETAILS_GETSESSIONATTRIBUTECOUNT_API_LATEST |

To get a copy of an attribute, call EOS\_SessionDetais\_CopySessionAttributeByIndex with a valid EOS\_HSessionDetais handle and an EOS\_SessionDetais\_CopySessionAttributeByIndexOptions initialized as follows:

| Property   | Value                                                     |
|------------|-----------------------------------------------------------|
| ApiVersion | EOS_SESSIONDETAILS_COPYSESSIONATTRIBUTEBYINDEX_API_LATEST |
| AttrIndex  | The index of the attribute to copy                        |

On success, this will return EOS\_Success and your output parameter will contain a copy of the EOS\_SessionDetais\_Attribute structure corresponding to the attribute index you requested. This structure contains the Attribute's value and type in an EOS\_Sessions\_AttributeData data structure, and its visibility in an EOS\_ESessionAttributeAdvertisementType enumerated value. When you no longer need this data, release it with EOS\_SessionDetais\_Attribute\_Reease .

### **Add an Attribute**

You can set up an attribute that you would like to add or modify by filling an EOS\_Sessions\_AttributeData data structure with the following information:

| Property   | Value                                                                      |
|------------|----------------------------------------------------------------------------|
| ApiVersion | EOS_SESSIONS_SESSIONATTRIBUTEDATA_API_LATEST                               |
| Key        | The name of the attribute                                                  |
| Value      | The attribute's value, or, in the case of strings, a pointer to the string |
| ValueType  | An EOS_ESessionAttributeType that describes Value                          |

Once you have this data ready, call EOS\_SessionModification\_AddAttribute to add the attribute. You must provide your EOS\_HSessionModification handle and an EOS\_SessionModification\_AddAttributeOptions , intialized as follows:

| Property          | Value                                                                                                            |
|-------------------|------------------------------------------------------------------------------------------------------------------|
| ApiVersion        | EOS_SESSIONMODIFICATION_ADDATTRIBUTE_API_LATEST                                                                  |
| SessionAttribute  | A const pointer to an EOS_Sessions_AttributeData containing the modification you<br>want to make                 |
| AdvertisementType | An EOS_ESessionAttributeAdvertisementType indicating whether or not this<br>attribute should be publicly visible |

You can store up to EOS\_SESSIONMODIFICATION\_MAX\_SESSION\_ATTRIBUTES (currently 64) in a session, and each attribute's name can be up to EOS\_SESSIONMODIFICATION\_MAX\_SESSION\_ATTRIBUTE\_LENGTH (currently 32) characters long.

This function only sets up the attribute that you want to add or update. It does not actually add or update the attribute, or interact with the session in any way. You will still need to call EOS\_Sessions\_UpdateSession as described at the top of the documentation: Modify a [Session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session).

#### **Remove an Attribute**

To remove an attribute, call EOS\_SessionModification\_RemoveAttribute with the EOS\_HSessionModification handle for your session, and an EOS\_SessionModification\_RemoveAttributeOptions structure containing the following information:

| Property   | Value                                              |
|------------|----------------------------------------------------|
| ApiVersion | EOS_SESSIONMODIFICATION_REMOVEATTRIBUTE_API_LATEST |

| Property | Value                                              |
|----------|----------------------------------------------------|
| Key      | The name (key) of the attribute you want to remove |

This function only establishes that you want to remove a certain attribute. It does not actually remove the attribute, or interact with the session in any way. You will still need to call EOS\_Sessions\_UpdateSession as described at the top of the documentation: Modify a [Session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session).

## Permission Levels

When you create a session you can specify a **permission level** for the session using EOS\_SessionModification\_SetPermissionLeve . Sessions have 3 levels of security:

| Security Level            | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EOS_OSPF_PublicAdvertised | Any client can get the session in search results without needing to know the<br>session id and can read session information as long as the session is not<br>started or session allows join in progress. Any client/player that has access to<br>the unique session identifier can view session information even if the session is<br>not joinable. Players can also find the session using a registered player's ID, as<br>long as the registered player is in a public session. |
| EOS_OSPF_JoinViaPresence  | Any client/player that has access to the unique session identifier can view<br>session information (typically this information is shared via presence data but<br>can be shared in other ways as well).                                                                                                                                                                                                                                                                           |
| EOS_OSPF_InviteOnly       | Only players which have been explicitly invited to the session by an existing<br>member of the session can view session information.                                                                                                                                                                                                                                                                                                                                              |
