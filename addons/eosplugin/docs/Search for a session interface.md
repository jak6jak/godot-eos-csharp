| Overview                 | Account Services | Game Services     | Get | j                  | jak6jak | Dev Portal         |   |                      |
|--------------------------|------------------|-------------------|-----|--------------------|---------|--------------------|---|----------------------|
| Epic Developer Resources | >                | EOS Game Services | >   | Lobbies & Sessions | >       | Sessions Interface | > | Search For a Session |

# Search For a Session

How to search for sessions.

## Search For a Session

To find a remote session, you need to configure a search, execute that search, then examine the results.

## Configure a Search

To begin a search, first call EOS\_Sessions\_CreateSessionSearch to create a search handle. Pass in an EOS\_Sessions\_CreateSessionSearchOptions structure, initialized as follows:

| Property         | Value                                       |
|------------------|---------------------------------------------|
| ApiVersion       | EOS_SESSIONS_CREATESESSIONSEARCH_API_LATEST |
| MaxSearchResults | Maximum number of search results to return  |

This function runs locally and will fill out the default search handle (of type EOS\_HSessionSearch ) on success. The next step is to configure the handle to perform a specific search with the criteria you require. EOS provides three methods to search for sessions:

- **Session ID:** Finds a single session with a known ID
- **User ID:** Finds all sessions involving a known user presently limited to local users
- **Attribute Data:** Finds all sessions that match some user-defined filtering criteria

### **Configure for Session ID**

If you want a specific session and know its server-side ID, call EOS\_SessionSearch\_SetSessionId with your search handle and an EOS\_SessionSearch\_SetSessionIdOptions initialized as follows:

| Property   | Value                                     |
|------------|-------------------------------------------|
| ApiVersion | EOS_SESSIONSEARCH_SETSESSIONID_API_LATEST |
| SessionId  | The ID of the session you want to find    |

#### 5/23/25, 8:50 PM Search For a Session | Epic Online Services Developer

This is the only step required to configure a session ID search. Unlike the other search methods, this will never return more than one result.

Only sessions marked as EOS\_OSPF\_PubicAdvertised or EOS\_OSPF\_JoinViaPresence can be found this way.

Because sessions that you configure in this way are publicly visible, applications outside of the EOS ecosystem could discover them. These applications could use session data to retrieve the IP address of the server or P2P host, and potentially attempt to cause disruption.

#### **Configure for User ID**

To find all sessions that involve a known user, call EOS\_SessionSearch\_SetTargetUserId with your search handle and an EOS\_SessionSearch\_SetTargetUserIdOptions containing the following information:

| Property     | Value                                        |
|--------------|----------------------------------------------|
| ApiVersion   | EOS_SESSIONSEARCH_SETTARGETUSERID_API_LATEST |
| TargetUserId | The user ID to find within sessions          |

This is the only step required to configure a user ID search. Must be a local, logged-in user.

The application can only find locally authenticated users or remote users that are registered in public sessions.

Registered users of publicly visible sessions (sessions configured with EOS\_OSPF\_PubicAdvertised or EOS\_OSPF\_JoinViaPresence ) are themselves publicly visible, including applications outside of the EOS ecosystem. These applications could use session data to discover the IP address of the server or P2P host, and potentially attempt to cause disruption.

### **Configure for Attribute Data**

The most robust way to find sessions is to search based on a set of search parameters, which act as filters. Some parameters could be exposed to the user, such as enabling the user to select a certain game type or map, while others might be hidden, such as using the player's estimated skill level to find matches with appropriate opponents. This search method can take multiple parameters, and will only find sessions that pass through all of them. To set up a search parameter, call

EOS\_SessionSearch\_SetParameter with your search handle and an EOS\_SessionSearch\_SetParameterOptions containing the following information:

| Property   | Value                                                                    |
|------------|--------------------------------------------------------------------------|
| ApiVersion | EOS_SESSIONSEARCH_SETPARAMETER_API_LATEST                                |
| Parameter  | A key and a value to compare to an attribute associated with the session |

| ComparisonOp | The type of comparison to make |
|--------------|--------------------------------|
|--------------|--------------------------------|

This function can be called multiple times to set up multiple filters, all of which must be satisfied for a session to show up in the search results. The following table lists the types of comparisons you can use, what value types they work on, and what condition must be met in order to pass:

| ComparisonOp              | Acceptable<br>Value Types | Success Condition                                                                                                                  |
|---------------------------|---------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| EOS_CO_EQUAL              | All                       | Attribute is equal to the search value                                                                                             |
| EOS_CO_NOTEQUAL           | All                       | Attribute is not equal to the search value                                                                                         |
| EOS_CO_GREATERTHAN        | Numerical<br>Types        | Attribute is greater than the search value                                                                                         |
| EOS_CO_GREATERTHANOREQUAL | Numerical<br>Types        | Attribute is greater than or equal to the search value                                                                             |
| EOS_CO_LESSTHAN           | Numerical<br>Types        | Attribute is less than the search value                                                                                            |
| EOS_CO_LESSTHANOREQUAL    | Numerical<br>Types        | Attribute is less than or equal to the search value                                                                                |
| EOS_CO_DISTANCE           | Numerical<br>Types        | Not a filter; attributes are sorted based on how close<br>they are to to the search value, or Abs(AttributeValue<br>- SearchValue) |
| EOS_CO_ANYOF              | Strings                   | Attribute matches any member of a semicolon-delimited<br>list (for example, "This;OrThis;MaybeThis")                               |
| EOS_CO_NOTANYOF           | Strings                   | Attribute does not match any member of a semicolon-<br>delimited list (for example "NotThis;OrThisEither")                         |

### **Reserved Attribute Constants**

The eos\_sessions\_types.h header file, included in the EOS SDK, defines attribute constants that you can use in your search configuration. The available reserved constants are listed in the table below:

Constant Description

EOS*SESSIONS*SEARCH*BUCKET*ID

EOS\_SESSIONS\_SEARCH\_BUCKET\_ID Search for a matching bucket ID. Use this as a key to query the attribute set

| Constant                                  | Description                                                                                 |
|-------------------------------------------|---------------------------------------------------------------------------------------------|
|                                           | EOS_SessionModification_SetBucketId . Set<br>AttrData.Value to the string of the bucket ID. |
| EOS_SESSIONS_SEARCH_EMPTY_SERVERS_ONLY    | Search for only empty servers. Set the<br>AttrData.Value to true .                          |
| EOS_SESSIONS_SEARCH_NONEMPTY_SERVERS_ONLY | Search for only full servers. Set AttrData.Value to<br>true .                               |
| EOS_SESSIONS_SEARCH_MINSLOTSAVAILABLE     | Search for a specified number of slots available. Set<br>AttrData.Value to an integer.      |

The example configuration below uses the EOS\_SESSIONS\_SEARCH\_BUCKET\_ID constant to search for a matching bucket ID:

EOS\_Sessions\_AttributeData AttrData; AttrData.ApiVersion = EOS\_SESSIONS\_SESSIONATTRIBUTEDATA\_API\_LATEST; ParamOptions.Parameter = &AttrData; AttrData.Key = EOS\_SESSIONS\_SEARCH\_BUCKET\_ID; AttrData.Vaue.AsUtf8 = YOUR\_STRING\_FOR\_THE\_BUCKET; AttrData.VaueType = EOS\_AT\_STRING; EOS\_SessionSearch\_SetParameter(SearchHande, &ParamOptions);

## Execute a Search

To execute the search, call EOS\_SessionSearch\_Find with your search handle and an [EOS\\_SessionSearch\\_FindOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-session-search-find-options) structure initialized as follows:

| Property    | Value                                                   |
|-------------|---------------------------------------------------------|
| ApiVersion  | EOS_SESSIONSEARCH_FIND_API_LATEST                       |
| LocalUserId | The Product User ID of the local user who is searching. |

This is an asynchronous operation. When it finishes, your callback function, of type EOS\_SessionSearch\_OnFindCaback , will receive an EOS\_SessionSearch\_FindCabackInfo structure notifying you of the success or failure of the search. Following successful completion, you can get copies of the search results from the EOS cache.

EOS supports running multiple EOS\_SessionSearch\_Find operations in parallel.

**Tip**: You can search by the attribute EOS\_SESSIONS\_SEARCH\_MINSLOTSAVAILABLE to find sessions that are not full (still have slots available). Set the value type to int64\_t , and use the EOS\_CO\_GREATERTHANOREQUAL comparison operator to set the value to at least 1 . This excludes full sessions from the results.

## Examine Search Results

EOS\_SessionSearch\_GetSearchResutCount for a given EOS\_HSessionSearch handle and EOS\_SessionSearch\_CopySearchResutByIndex will return the session details one by one through individual EOS\_HSessionDetais handles.

After completing a successful search, use EOS\_SessionSearch\_GetSearchResutCount with your search handle to get the number of results that the search returned. You can then call EOS\_SessionSearch\_CopySearchResutByIndex to get a copy of the EOS\_HSessionDetais handle associated with the active [session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#active-sessions) at that index. This handle provides access to [session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/get-information-about-a-session#session-details) [details](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/get-information-about-a-session#session-details) data, which you can use to display information about the session to the local user or determine whether or not to [join](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#join-a-session) the session using your own game logic. You must release the EOS\_HSessionDetais handle with EOS\_SessionDetais\_Reease when you no longer need it.
