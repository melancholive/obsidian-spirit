using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{
    public GameObject gameOverScreen;
    public float playerScore;
    public Text scoreText;
    public float pointsPerUnit = 1f;
    public Camera playerCamera;
    
    private Vector3 lastCameraPosition;
    private float totalForwardDistance = 0f;

    void Start()
    {
        Time.timeScale = 1f;
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (playerCamera == null)
            playerCamera = Camera.main;
            
        lastCameraPosition = playerCamera.transform.position;
    }

    void Update()
    {
        Vector3 currentPosition = playerCamera.transform.position;
        Vector3 movement = currentPosition - lastCameraPosition;
        
        if (movement.y > 0)
        {
            totalForwardDistance += movement.y;
            playerScore = totalForwardDistance * pointsPerUnit;
        }
        
        scoreText.text = Mathf.FloorToInt(playerScore).ToString();
        lastCameraPosition = currentPosition;
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0f;
    }
}
