using UnityEngine;

public class FirstLaunchUI : MonoBehaviour {
    public MenuUI menuScript;
	public void SetLanguage(int x)
    {
        int k = 0;
        if (PlayerPrefs.HasKey(GameConstants.PP_BASE_SETTINGS_PROPERTY)) k = PlayerPrefs.GetInt(GameConstants.PP_BASE_SETTINGS_PROPERTY);
        if (x == 1 & ((k & 1) == 0)) k += 1;
        if ((k & 2) == 0) k += 2;
        PlayerPrefs.SetInt(GameConstants.PP_BASE_SETTINGS_PROPERTY, k);
        PlayerPrefs.Save();
        menuScript.Options_ChangeLanguage(x);
        menuScript.Start();
        Destroy(gameObject);
    }
}
