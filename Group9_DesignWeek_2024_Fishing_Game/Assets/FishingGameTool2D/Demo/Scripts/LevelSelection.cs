using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishingGameTool2D.Example
{
    public class LevelSelection : MonoBehaviour
    {
        public void LoadSideView()
        {
            SceneManager.LoadScene(1);
        }

        public void LoadTopdownView()
        {
            SceneManager.LoadScene(2);
        }
    }
}
