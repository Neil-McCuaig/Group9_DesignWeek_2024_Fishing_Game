using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishingGameTool2D.Example
{
    public class ChangeLevel : MonoBehaviour
    {
        public void Change()
        {
            SceneManager.LoadScene(0);
        }
    }
}
