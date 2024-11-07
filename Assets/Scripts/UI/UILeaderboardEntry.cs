using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardEntry : MonoBehaviour {

    [SerializeField] Text leaderboardNameText;
    [SerializeField] Text leaderboardScoreText;

    public void SetLeaderboardEntry (PlayFab.ClientModels.PlayerLeaderboardEntry entry) {
        leaderboardNameText.text = $"{entry.Position}. {entry.DisplayName}";
        if (entry.PlayFabId == UserAccountManager.playfabID) {
            leaderboardScoreText.text = UserProfile.Instance.highscore.ToString ();
        } else {
            leaderboardScoreText.text = entry.StatValue.ToString ();
        }
    }

}