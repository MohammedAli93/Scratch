using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

public class UserAccountManager : MonoBehaviour {

    public static UserAccountManager Instance;

    public static UnityEvent OnSignInSuccess = new UnityEvent ();
    public static UnityEvent<string> OnSignInFailed = new UnityEvent<string> ();
    public static UnityEvent<string> OnCreateAccountFailed = new UnityEvent<string> ();
    public static UnityEvent<string, string> OnUserDataRetrieved = new UnityEvent<string, string> ();
    public static UnityEvent<string, List<PlayerLeaderboardEntry>> OnLeaderboardRetrieved = new UnityEvent<string, List<PlayerLeaderboardEntry>> ();
    public static UnityEvent<string, StatisticValue> OnStatisticRetrieved = new UnityEvent<string, StatisticValue> ();

    public static string playfabID;
    public static UserAccountInfo userAccountInfo;

    void Awake () {
        Instance = this;
    }

    /*
        ACCOUNT
    */

    public void CreateAccount (string username, string emailAddress, string password) {
        Debug.Log ($"Creating account for {username}, {emailAddress}, {password}");
        PlayFabClientAPI.RegisterPlayFabUser (
            new RegisterPlayFabUserRequest () {
                Email = emailAddress,
                Password = password,
                Username = username,
            },
            response => {
                Debug.Log ($"Successful Account Creation: {username}, {emailAddress}");
                SignIn (username, password);
            },
            error => {
                Debug.Log ($"Unsuccessful Account Creation: {username}, {emailAddress} \n {error.ErrorMessage}");
                OnCreateAccountFailed.Invoke (error.ErrorMessage);
            }
        );
    }

    public void SignIn (string username, string password) {
        PlayFabClientAPI.LoginWithPlayFab (new LoginWithPlayFabRequest () {
                Username = username,
                    Password = password
            },
            response => {
                Debug.Log ($"Successful Account Login: {username}");
                playfabID = response.PlayFabId;
                CheckDisplayName (username, () => OnSignInSuccess.Invoke ());
            },
            error => {
                Debug.Log ($"Unsuccessful Account Login: {username} \n {error.ErrorMessage}");
                OnSignInFailed.Invoke (error.ErrorMessage);
            }
        );
    }

    public void SignInWithDevice () {
        if (GetDeviceId (out string android_id, out string ios_id, out string custom_id)) {
            if (!string.IsNullOrEmpty (android_id)) {
                Debug.Log ("Using Android Device ID: " + android_id);

                PlayFabClientAPI.LoginWithAndroidDeviceID (new LoginWithAndroidDeviceIDRequest () {
                    AndroidDeviceId = android_id,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount = true
                }, result => {
                    Debug.Log ($"Successful Login with Android Device ID");
                    playfabID = result.PlayFabId;
                    CheckDisplayName ("User", () => OnSignInSuccess.Invoke ());
                }, error => {
                    Debug.Log ($"<color=red>Unsuccessful Login with Android Device ID</color>");
                    OnSignInFailed.Invoke (error.ErrorMessage);
                });
            } else if (!string.IsNullOrEmpty (ios_id)) {
                Debug.Log ("Using IOS Device ID: " + ios_id);

                PlayFabClientAPI.LoginWithIOSDeviceID (new LoginWithIOSDeviceIDRequest () {
                    DeviceId = ios_id,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount = true
                }, result => {
                    Debug.Log ($"Successful Login with IOS Device ID");
                    playfabID = result.PlayFabId;
                    CheckDisplayName ("User", () => OnSignInSuccess.Invoke ());
                }, error => {
                    Debug.Log ($"<color=red>Unsuccessful Login with IOS Device ID</color>");
                    OnSignInFailed.Invoke (error.ErrorMessage);
                });
            }
        } else {
            Debug.Log ("Using custom device ID: " + custom_id);

            PlayFabClientAPI.LoginWithCustomID (new LoginWithCustomIDRequest () {
                CustomId = custom_id,
                    TitleId = PlayFabSettings.TitleId,
                    CreateAccount = true
            }, result => {
                Debug.Log ($"Successful Login with custom device ID");
                playfabID = result.PlayFabId;
                CheckDisplayName ("User", () => OnSignInSuccess.Invoke ());
            }, error => {
                Debug.Log ($"<color=red>Unsuccessful Login with custom device ID</color>");
                OnSignInFailed.Invoke (error.ErrorMessage);
            });
        }
    }

    bool GetDeviceId (out string android_id, out string ios_id, out string custom_id) {
        android_id = string.Empty;
        ios_id = string.Empty;
        custom_id = string.Empty;

        if (CheckForSupportedMobilePlatform ()) {
#if UNITY_ANDROID
            //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
            AndroidJavaClass clsUnity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject> ("currentActivity");
            AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject> ("getContentResolver");
            AndroidJavaClass clsSecure = new AndroidJavaClass ("android.provider.Settings$Secure");
            android_id = clsSecure.CallStatic<string> ("getString", objResolver, "android_id");
#endif

#if UNITY_IPHONE
            ios_id = UnityEngine.iOS.Device.vendorIdentifier;
#endif
            return true;
        } else {
            custom_id = SystemInfo.deviceUniqueIdentifier;
            return false;
        }
    }

    bool CheckForSupportedMobilePlatform () {
        return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }

    /*
        DISPLAYNAME
    */

    void CheckDisplayName (string username, UnityAction completeAction) {
        PlayFabClientAPI.GetAccountInfo (new GetAccountInfoRequest () {
            PlayFabId = playfabID
        }, result => {
            userAccountInfo = result.AccountInfo;

            if (result.AccountInfo.TitleInfo.DisplayName == null || result.AccountInfo.TitleInfo.DisplayName.Length == 0) {
                PlayFabClientAPI.UpdateUserTitleDisplayName (new UpdateUserTitleDisplayNameRequest () {
                    DisplayName = username
                }, result => {
                    Debug.Log ($"Display name set to username");
                    completeAction.Invoke ();
                }, error => {
                    Debug.Log ($"Display name could not be set to username | {error.ErrorMessage}");
                });
            } else {
                completeAction.Invoke ();
            }
        }, error => {
            Debug.Log ($"Could not retrieve AccountInfo | {error.ErrorMessage}");
        });
    }

    public void SetDisplayName (string displayName) {
        PlayFabClientAPI.UpdateUserTitleDisplayName (new UpdateUserTitleDisplayNameRequest () {
            DisplayName = displayName
        }, result => {
            Debug.Log ($"Display name set to username");
        }, error => {
            Debug.Log ($"Display name could not be set to username | {error.ErrorMessage}");
        });
    }

    /*
        USERDATA
    */

    public void GetUserData (string key) {
        PlayFabClientAPI.GetUserData (new GetUserDataRequest () {
            PlayFabId = playfabID,
                Keys = new List<string> () {
                    key
                }
        }, result => {
            Debug.Log ($"User data retrieved successfully");
            if (result.Data.ContainsKey (key)) OnUserDataRetrieved.Invoke (key, result.Data[key].Value);
            else OnUserDataRetrieved.Invoke (key, null);
        }, error => {
            Debug.Log ($"Could not retrieve user data | {error.ErrorMessage}");
        });
    }

    public void SetUserData (string key, string userData) {
        PlayFabClientAPI.UpdateUserData (new UpdateUserDataRequest () {
            Data = new Dictionary<string, string> () { { key, userData }
            }
        }, result => {
            Debug.Log ($"{key} successfully updated");
        }, error => {
            Debug.Log ($"{key} update unsuccessful | {error.ErrorMessage}");
        });
    }

    /*
        STATISTICS & LEADERBOARDS
    */

    public void GetStatistic (string statistic) {
        PlayFabClientAPI.GetPlayerStatistics (new GetPlayerStatisticsRequest () {
            StatisticNames = new List<string> () {
                statistic
            }
        }, result => {
            if (result.Statistics.Count > 0) {
                Debug.Log ($"Successfully got {statistic} | {result.Statistics[0]}");
                if (result.Statistics != null) OnStatisticRetrieved.Invoke (statistic, result.Statistics[0]);
            } else {
                Debug.Log ($"No existing statistic [{statistic}] for user");
            }
        }, error => {
            Debug.Log ($"Could not retrieve {statistic} | {error.ErrorMessage}");
        });
    }

    public void SetStatistic (string statistic, int value) {
        PlayFabClientAPI.UpdatePlayerStatistics (new UpdatePlayerStatisticsRequest () {
            Statistics = new List<StatisticUpdate> () {
                new StatisticUpdate () {
                    StatisticName = statistic,
                        Value = value
                }
            }
        }, result => {
            Debug.Log ($"{statistic} successfully updated");
            GetLeaderboard (statistic);
        }, error => {
            Debug.Log ($"{statistic} update unsuccessful | {error.ErrorMessage}");
        });
    }

    public void GetLeaderboard (string statistic) {
        PlayFabClientAPI.GetLeaderboard (new GetLeaderboardRequest () {
            StatisticName = statistic
        }, result => {
            Debug.Log ($"Successfully got {statistic} leaderboard | 0.{result.Leaderboard[0].DisplayName} {result.Leaderboard[0].StatValue}");
            OnLeaderboardRetrieved.Invoke (statistic, result.Leaderboard);
        }, error => {
            Debug.Log ($"Could not retrieve {statistic} leaderboard | {error.ErrorMessage}");
        });
    }

    public void GetLeaderboardDelayed (string statistic) {
        StartCoroutine (CheckLeaderboardDelay (statistic));
    }

    IEnumerator CheckLeaderboardDelay (string statistic) {
        yield return new WaitForSeconds (3);
        GetLeaderboard (statistic);
    }

}