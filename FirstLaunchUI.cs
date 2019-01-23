using UnityEngine;

public class FirstLaunchUI : MonoBehaviour {
    public MenuUI menuScript;
	public void SetLanguage(int x)
    {
        int k = 0;
        if (PlayerPrefs.HasKey(GameConstants.BASE_SETTINGS_PLAYERPREF)) k = PlayerPrefs.GetInt(GameConstants.BASE_SETTINGS_PLAYERPREF);
        if (x == 1 & ((k & 1) == 0)) k += 1;
        if ((k & 2) == 0) k += 2;
        PlayerPrefs.SetInt(GameConstants.BASE_SETTINGS_PLAYERPREF, k);
        PlayerPrefs.Save();
        menuScript.Options_ChangeLanguage(x);
        menuScript.Start();
        Destroy(gameObject);
    }
}
